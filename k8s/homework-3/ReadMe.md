

# ДЗ 3 Helm

### Устанавливаем Ingress-Nginx Controller

`kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.11.2/deploy/static/provider/cloud/deploy.yaml`


### Устанавливаем приложение
```
kubectl apply -f userservice-configmap.yaml
kubectl apply -f userservice-deployment.yaml
kubectl apply -f userservice-service.yaml
kubectl apply -f userservice-ingress.yaml
```


### Устанавливаем БД
```
kubectl apply -f postgres-secrets.yaml
kubectl apply -f postgres-pv.yaml
kubectl apply -f postgres-pvclaim.yaml
kubectl apply -f postgres-deployment.yaml
kubectl apply -f postgres-service.yaml
```



