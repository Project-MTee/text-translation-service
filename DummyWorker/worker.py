import aio_pika
import logging
import asyncio
import os
import json

from aio_pika import ExchangeType
from aio_pika.message import Message
class DummyWorker():
    def __init__(self):
        self.__logger = logging.getLogger('DummyWorker')

        self.__exchange = os.environ.get("RABBITMQ_EXCHANGE", "translation")
        self.__queue = os.environ.get("RABBITMQ_QUEUE", "translation.en.et.general.plain")
        self.__routing_key = self.__queue

        self.__username = os.environ.get("RABBITMQ_USER", "root")
        self.__password = os.environ.get("RABBITMQ_PASS", "root")
        self.__host = os.environ.get("RABBITMQ_HOST", "localhost")
        self.__port = int(os.environ.get("RABBITMQ_PORT", "5672"))

        self.__logger.info(f"queue: {self.__queue}")
        self.__logger.info(f"rabbitMQ host: {self.__host}")

    def listen(self):
        self.__logger.info("listen init")

        loop = asyncio.new_event_loop()
        loop.run_until_complete(self.__main_loop(loop))
        loop.close()

    async def __main_loop(self, loop):
        self.__logger.info("init")
        connection = await aio_pika.connect_robust(
            host=self.__host,
            port=self.__port,
            login=self.__username,
            password=self.__password,
            loop=loop,
            client_properties={
                "client_properties": {
                    "connection_name": "Dummy worker"
                }
            }
        )

        async with connection:
            channel = await connection.channel()
            await channel.set_qos(prefetch_count=1)

            exchange = await channel.declare_exchange(self.__exchange, ExchangeType.DIRECT, durable=False)

            queue = await channel.declare_queue(self.__queue, auto_delete=False, durable=False)
            await queue.bind(exchange, routing_key=self.__routing_key)

            self.__logger.info("RabbitMQ ready for messages")

            async with queue.iterator() as queue_iter:
                async for message in queue_iter:
                    async with message.process():
                        message_body = json.loads(message.body)
                        # self.__logger.info(f"Work item received: {json.dumps(message_body, sort_keys=True, indent=4)}")
                        # self.__logger.info(f"{message.correlation_id}")

                        translations = message_body["text"]
                        for i in range(len(translations)):
                            translations[i] = "    :D     " + translations[i] if translations[i] else translations[i]

                        response = json.dumps({
                            "status":"Im ok",
                            "status_code":200,
                            "translation": translations
                        }).encode()

                        await channel.default_exchange.publish(
                            Message(
                                body=response,
                                content_type="application/json",
                                correlation_id=message.correlation_id,
                                headers={
                                    'RequestId': message.headers["RequestId"],
                                    'MT-MessageType': message.headers["ReturnMessageType"],
                                }
                            ),
                            routing_key=message.reply_to
                        )

        self.__logger.info("RabbitMQ stoped")

logging.basicConfig(level=logging.INFO,
                    format="%(asctime)s %(levelname)-8s [%(name)s:%(funcName)s:%(lineno)d] %(message)s")

worker = DummyWorker()
worker.listen()