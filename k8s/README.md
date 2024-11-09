# Kubenetes

### ДЗ 2
1. Установить [Docker Desktop](https://www.docker.com/products/docker-desktop/) 
1. Включить Kubernetes ([Getting Started with Kubernetes on Docker Desktop](https://birthday.play-with-docker.com/kubernetes-docker-desktop/))
1. Установить Ингресс
 
	 `kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.10.1/deploy/static/provider/cloud/deploy.yaml`

1. Раскатать Deployment   

	`kubectl apply -f https://raw.githubusercontent.com/Mr-Zhukoff/OTUS-SA/main/k8s/homework-2/dp-testwebapp.yaml`
1. Раскатать Service   

	`kubectl apply -f https://raw.githubusercontent.com/Mr-Zhukoff/OTUS-SA/main/k8s/homework-2/svc-testwebapp.yaml`
1. Раскатать Ingress   

	`kubectl apply -f https://raw.githubusercontent.com/Mr-Zhukoff/OTUS-SA/main/k8s/homework-2/ing-testwebapp.yaml`
3.   




## Основные команды
 Получить список `kubectl get no/po/svc/ing/pv/cm`
 
 Получить подробное описание `kubectl describe no/po/svc/ing/pv/cm`

где:
* no - ноды
* po - поды
* svc - cервисы
* ing - Ингресс
* pv - persistent volumes
* cm - config maps


#### Kubernetes Dashboard
https://raw.githubusercontent.com/kubernetes/dashboard/v2.7.0/aio/deploy/recommended.yaml



### Все зачистить

```
kubectl delete ing
kubectl delete svc
kubectl delete deployment
```




