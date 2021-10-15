FROM python:3

WORKDIR /src

COPY worker.py worker.py

RUN pip install aio-pika

ENTRYPOINT ["python", "worker.py"]