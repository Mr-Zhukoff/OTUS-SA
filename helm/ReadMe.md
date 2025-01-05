Для всех ДЗ необходимо установить Helm и Ingress-Nginx

### Устанавливаем Helm
`https://github.com/helm/helm/releases`

### Прописываем arch.homework в hosts
### Устанавливаем Ingress-Nginx Controller

`kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.11.2/deploy/static/provider/cloud/deploy.yaml`


## ДЗ 3 Helm

### Устанавливаем приложение
`helm install app UserService`

###
Тестируем через Newman

`newman run Helm_homework.postman_collection.json`

## ДЗ 4 Prometheus и Grafana

### Создаеми неймспейс для мониторинга
`kubectl create namespace monitoring`

### Создаем роль
`kubectl apply -f cluster-role.yaml`

### Создаем конфиги Prometheus
`kubectl apply -f prometheus-configmap.yaml`

### Деплоим Prometheus
`kubectl apply -f prometheus-deployment.yaml`
`kubectl apply -f prometheus-service.yaml`
`kubectl apply -f prometheus-ingress.yaml`

### Создаем конфиги Grafana
`kubectl apply -f prometheus-deployment.yaml`

### Устанавливаем Grafana
`kubectl apply -f grafana-deployment.yaml`
`kubectl apply -f grafana-service.yaml`

### Добавляем чарты Prometheus в репозиторий
`helm repo add prometheus-community https://prometheus-community.github.io/helm-charts
helm repo update`
`helm repo update`

### Устанавливаем Prometheus
`helm install prometheus prometheus-community/prometheus`

### Пробрасываем порты
`kubectl expose service prometheus-server --type=NodePort --target-port=9090 --name=prometheus-server-ext`

### Проверяем доступ

### Добавляем чарты Grafana в репозиторий 
`helm repo add grafana https://grafana.github.io/helm-charts`
`helm repo update`

### Устанавливаем Grafana
`helm install grafana grafana/grafana`

### Пробрасываем порты
`kubectl expose service grafana --type=NodePort --target-port=3000 --name=grafana-ext`

## ДЗ 5 BFF

### Переходим в папку домашнего задания 
\OTUS-SA\helm\Homework_BFF

### Устанавливаем приложение
`helm install usersvc userservice`

### Тестируем через Newman

`newman run BFF_Homework.postman_collection.json`