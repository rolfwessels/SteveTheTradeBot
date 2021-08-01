# SteveTheTradeBot

[![BuildStatus](https://github.com/rolfwessels/SteveTheTradeBot/actions/workflows/github-action.yml/badge.svg)](https://github.com/rolfwessels/SteveTheTradeBot/actions)
[![Dockerhub Status](https://img.shields.io/badge/dockerhub-ok-blue.svg)](https://hub.docker.com/r/rolfwessels/steve-the-trade-bot/)

Steve is a crypto trading bot. Well he is trying to be!

## Todo

- [x] Read historical data for back testing.
- [x] Create dummy broker.
- [x] Create simple bot.
- [x] Add a way do some back testing.
- [x] Store metrics when importing data [RSI, Super trend,EMA100 , EMA200 ].
- [x] Have a BTC bot that actually makes money!
- [x] Store back test results
- [x] Run paper trades
- [x] Add stop loss and take profit
- [x] Add to docker
- [x] Integrate with grafana
- [x] Logging & monitoring
- [x] Slack
- [x] Deploy to staging
- [ ] Fix slack - better error logging
- [ ] Integrate a broker (Valr for now)
- [ ] More back test stats
- [ ] Integrate a broker to get real time data
- [ ] Prometheus & Slack counters
- [ ] Add UI
- [ ] Deploy to AWS or Digital Ocean
- [ ] Move to using only OpenId
- [ ] Move away from postgress

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

## Update the database

```cmd
dotnet tool install --global dotnet-ef
cd src\SteveTheTradeBot.Core
dotnet ef migrations  --startup-project ..\SteveTheTradeBot.Api\SteveTheTradeBot.Api.csproj add AddStrategy
dotnet ef database  --startup-project ..\SteveTheTradeBot.Api\SteveTheTradeBot.Api.csproj update
dotnet ef migrations  --startup-project ..\SteveTheTradeBot.Api\SteveTheTradeBot.Api.csproj list
#dotnet ef database update  --startup-project ..\SteveTheTradeBot.Api\SteveTheTradeBot.Api.csproj 20210719165547_AddMetricMapping
#dotnet ef migrations remove --startup-project ..\SteveTheTradeBot.Api\SteveTheTradeBot.Api.csproj

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

## Setup the database with user in postgress

```cmd

CREATE USER sttb_dev WITH ENCRYPTED PASSWORD 'xxxxxx';
ALTER ROLE sttb_dev WITH CREATEDB
CREATE DATABASE steve_the_trade_bot_dev OWNER sttb_dev;

```

Debugging

```cmd
cd src
docker-compose up -d;
docker-compose exec api bash
```

## Helpful Links

- Timescaledb <https://docs.timescale.com/timescaledb/latest/how-to-guides/install-timescaledb/self-hosted/docker/installation-docker/#docker-hub>
