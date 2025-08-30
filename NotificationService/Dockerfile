FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY CarsAuction.sln ./
COPY NotificationService/NotificationService.csproj NotificationService/
COPY Contracts/Contracts.csproj Contracts/

RUN dotnet restore "NotificationService/NotificationService.csproj"

COPY . .

WORKDIR /src/NotificationService
RUN dotnet publish "NotificationService.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 80
COPY --from=build /app/publish .
ENTRYPOINT [ "dotnet", "NotificationService.dll" ]