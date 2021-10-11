# Translation API Service

Public text translation service which communicates through **RabbitMQ** to send messages to **Domain detection** service and **MT systems**.

```

 -------------------------------        Fetch available translation systems for request validation
|                               |            --------------------------------
|                               |           |                                |
|    Translation API Service    |   → → →   |   Translation system service   |
|           [Public]            |           |                                |
|                               |            --------------------------------
 -------------------------------

          ↑  ↓
          ↑  ↓
          ↑  ↓  Requests info about detected domains
          ↑  ↓        --------------------------------
          ↑  ↓ → → → |                                |
          ↑  ↓ ← ← ← |        Domain detector         |
          ↑  ↓       |                                |
          ↑  ↓        --------------------------------
          ↑  ↓
          ↑  ↓
          ↑  ↓  Requests translation from MT systems
          ↑  ↓        --------------------------------
          ↑  ↓ → → → |                                |
          ↑ ← ← ← ←  |          MT system             |
                     |                                |
                      --------------------------------

```
# Test

Install prerequisites

```Shell
# install kubectl
choco install kubernetes-cli
# install helm
choco install kubernetes-helm
```

Install and deploy RabbitMQ

```Shell
helm repo add bitnami https://charts.bitnami.com/bitnami
helm repo update

# RabbitMQ
helm install rabbitmq --set auth.username=root,auth.password=root,auth.erlangCookie=secretcookie bitnami/rabbitmq
```

forward ports:

```Shell
# RabbitMQ
kubectl port-forward --namespace default svc/rabbitmq 15672:15672 5672:5672
```

Using docker compose
```
docker-compose up --build
```

Open Swagger
```
http://localhost:5003/swagger/index.html
```
