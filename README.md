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

### Translation to MT system via RabbitMQ:

> exchange created automatically on demand


| Parameter           | Value                                                                             |
| ------------------- | --------------------------------------------------------------------------------- |
| exchange            | translation                                                                       |
| exchange type       | direct                                                                            |
| exchange options    |                                                                                   |
| routing key         | translation.`{SourceLanguage}`.`{TargetLanguage}`.`{Domain}`.`{InputType}`        |

### Domain detection via RabbitMQ:

> exchange created automatically on demand

| Parameter           | Value                                                                             |
| ------------------- | --------------------------------------------------------------------------------- |
| exchange            | domain-detection                                                                  |
| exchange type       | direct                                                                            |
| exchange options    |                                                                                   |
| routing key         | domain-detection.`{SourceLanguage}`                                               |


# Monitor

## Healthcheck probes

https://kubernetes.io/docs/tasks/configure-pod-container/configure-liveness-readiness-startup-probes/

Startup probe / Readiness probe:

`/health/ready`

Liveness probe:

`/health/live`


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
