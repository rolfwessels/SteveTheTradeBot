FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build-env
WORKDIR /app
COPY ./ ./
WORKDIR /app/SteveTheTradeBot.Api
RUN dotnet publish -c Release -o out


# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine

WORKDIR /app
COPY --from=build-env /app/SteveTheTradeBot.Api/out .
RUN apk add curl
HEALTHCHECK CMD curl --fail http://localhost:5000/api/ping/hc || exit 1
#  <file value="C:\temp\Logs\SteveTheTradeBot.Api.log" />
#     <lockingModel type="log4net.Appender.FileAppender+MinimalLock" />
EXPOSE 5000/tcp
ENTRYPOINT ["dotnet", "SteveTheTradeBot.Api.dll"]
