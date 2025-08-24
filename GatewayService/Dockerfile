FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY CarsAuction.sln ./
COPY GatewayService/GatewayService.csproj GatewayService/

RUN dotnet restore "GatewayService/GatewayService.csproj"

COPY . .

WORKDIR /src/GatewayService
RUN dotnet publish "GatewayService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 80
COPY --from=build /app/publish .
ENTRYPOINT [ "dotnet", "GatewayService.dll" ]