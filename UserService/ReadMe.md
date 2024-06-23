

## Настройка Docker desktop

Запустить контейнер с PostgreSQL

`docker run --name otus-pg -p 5432:5432 -e POSTGRES_PASSWORD=12345678 -d postgres`

В проекте обновляем настройки полключения к БД `appsettings.json`

  `"ConnectionStrings": {
    "DefaultConnection": "Host=172.18.0.2;Port=5432;Database=users;Username=postgres;Password=12345678"
  }`
  
Из студии запускаем приложение UserServiceAPI

Для соединения между контейнерами создаем сетку

`docker network create otus-network`

Добавляем в нее контейнер с БД

`docker network connect otus-network otus-pg`

Добавляем в нее контейнер с приложением UserServiceAPI

`network connect otus-network UserServiceAPI`

Смотрим что получилось

`docker network inspect otus-network`