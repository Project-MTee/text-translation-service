# Translation API Service

Public text translation service which communicates through **RabbitMQ** to send messages to **Domain detection** service and **MT systems**.

```

 -------------------------------
|                               |
|                               |
|    Translation API Service    |
|           [Public]            |
|                               |
 -------------------------------

          ↑  ↓
          ↑  ↓
          ↑  ↓  Requests info about detected domains
          ↑  ↓        -------------------------------
          ↑  ↓ → → → |                               |
          ↑  ↓ ← ← ← |        Domain detector        |
          ↑  ↓       |                               |
          ↑  ↓        -------------------------------
          ↑  ↓
          ↑  ↓
          ↑  ↓  Requests translation from MT systems
          ↑  ↓        -------------------------------
          ↑  ↓ → → → |                               |
          ↑ ← ← ← ←  |          MT system            |
                     |                               |
                      -------------------------------

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
helm install test --set auth.username=root,auth.password=root,auth.erlangCookie=secretcookie bitnami/rabbitmq
# helm delete test
```

forward ports:

```Shell
kubectl port-forward --namespace default svc/test-rabbitmq 15672:15672 5672:5672
```
