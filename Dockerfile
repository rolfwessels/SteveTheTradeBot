FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine

# Base Development Packages
RUN apk update \
  && apk upgrade \
  && apk add ca-certificates wget && update-ca-certificates \
  && apk add --no-cache --update \
  git \
  curl \
  wget \
  bash \
  make \
  rsync \
  nano

WORKDIR /SteveTheTradeBot

COPY src/SteveTheTradeBot.Api/*.csproj ./src/SteveTheTradeBot.Api/
COPY src/SteveTheTradeBot.Dal/*.csproj ./src/SteveTheTradeBot.Dal/
COPY src/SteveTheTradeBot.Sdk/*.csproj ./src/SteveTheTradeBot.Sdk/
COPY src/SteveTheTradeBot.Core/*.csproj ./src/SteveTheTradeBot.Core/
COPY src/SteveTheTradeBot.Shared/*.csproj ./src/SteveTheTradeBot.Shared/
COPY src/SteveTheTradeBot.Dal/*.csproj ./src/SteveTheTradeBot.Dal/
COPY src/SteveTheTradeBot.Utilities/*.csproj ./src/SteveTheTradeBot.Utilities/
COPY src/SteveTheTradeBot.Dal.MongoDb/*.csproj ./src/SteveTheTradeBot.Dal.MongoDb/

WORKDIR /SteveTheTradeBot/src/SteveTheTradeBot.Api
RUN dotnet restore

# Working Folder
WORKDIR /SteveTheTradeBot
ENV TERM xterm-256color
RUN printf 'export PS1="\[\e[0;34;0;33m\][DCKR]\[\e[0m\] \\t \[\e[40;38;5;28m\][\w]\[\e[0m\] \$ "' >> ~/.bashrc
