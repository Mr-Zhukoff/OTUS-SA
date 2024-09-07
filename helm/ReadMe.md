# ДЗ 3 Helm

### Устанавливаем Helm
`https://github.com/helm/helm/releases`

### Прописываем arch.homework в hosts
### Устанавливаем Ingress-Nginx Controller

`kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.11.2/deploy/static/provider/cloud/deploy.yaml`


### Устанавливаем приложение
`helm install app UserService`



