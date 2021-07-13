# SteveTheTradeBot

![SteveTheTradeBot Logo](https://github.com/rolfwessels/SteveTheTradeBot/raw/master/logo/stevethetradebot_logo.png)
[![Dockerhub Status](https://img.shields.io/badge/dockerhub-ok-blue.svg)](https://hub.docker.com/r/rolfwessels/stevethetradebot/)

Steve is a crypto trading bot. Well he is trying to be!

## Todo
 
- [x] Read historical data for back testing
- [ ] Create dummy broker
- [ ] Create simple agorythm
- [ ] Add a way do some back testing
- [ ] Integrate a broker to get real time data
- [ ] Integrate a broker (Valr for now)
- [ ] Integrate with grafana
- [ ] Deploy to AWS or Digital Ocean
- [ ] Add UI
- [ ] Move to using only OpenId

## Getting started with dev

Open the docker environment to do all development and deployment

```bash
# bring up dev environment
make build up
# to test the app
make test
# to run the app
make start
```

## Available make commands

### Commands outside the container

- `make up` : brings up the container & attach to the default container
- `make down` : stops the container
- `make build` : builds the container

### Commands to run inside the container

- `make start` : Run the app
- `make test` : To test the app

## Create certificates

see <https://benjii.me/2017/06/creating-self-signed-certificate-identity-server-azure/>

```cmd
cd src/SteveTheTradeBot.Api/Certificates
openssl req -x509 -newkey rsa:4096 -sha256 -nodes -keyout development.key -out development.crt -subj "/CN=localhost" -days 3650
openssl pkcs12 -export -out development.pfx -inkey development.key -in development.crt -certfile development.crt
rm development.crt
rm development.key

openssl req -x509 -newkey rsa:4096 -sha256 -nodes -keyout production.key -out production.crt -subj "/CN=localhost" -days 3650
openssl pkcs12 -export -out production.pfx -inkey production.key -in production.crt -certfile production.crt
rm production.crt
rm production.key
```

## Deploy docker files

```cmd
dotnet tool install --global dotnet-ef
cd src\SteveTheTradeBot.Core
dotnet build
dotnet ef migrations add InitialCreate --startup-project ..\SteveTheTradeBot.Api\SteveTheTradeBot.Api.csproj
dotnet ef database update --startup-project ..\SteveTheTradeBot.Api\SteveTheTradeBot.Api.csproj
```

## Deploy docker files

```cmd
cd src
docker-compose build;
docker-compose up;
```

Debugging

```cmd
cd src
docker-compose up -d;
docker-compose exec api bash
```


## Helpful Links

-  Timescaledb <https://docs.timescale.com/timescaledb/latest/how-to-guides/install-timescaledb/self-hosted/docker/installation-docker/#docker-hub> 
