FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build-env
WORKDIR /app
COPY ./SteveTheTradeBot.Cmd/SteveTheTradeBot.Cmd.csproj ./SteveTheTradeBot.Cmd/SteveTheTradeBot.Cmd.csproj
COPY ./SteveTheTradeBot.Api/SteveTheTradeBot.Api.csproj ./SteveTheTradeBot.Api/SteveTheTradeBot.Api.csproj
COPY ./SteveTheTradeBot.Core/SteveTheTradeBot.Core.csproj ./SteveTheTradeBot.Core/SteveTheTradeBot.Core.csproj
WORKDIR /app/SteveTheTradeBot.Cmd
RUN dotnet restore
WORKDIR /app
COPY ./ ./
WORKDIR /app/SteveTheTradeBot.Cmd
RUN dotnet publish -c Release -o /app/output


# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine

WORKDIR /app
COPY --from=build-env /app/output .
RUN apk add curl
RUN chmod a+x /app/sttb
HEALTHCHECK CMD curl --fail http://localhost:5000/api/ping/hc || exit 1
EXPOSE 5000/tcp
ENTRYPOINT ["/app/sttb","service", "-v"]