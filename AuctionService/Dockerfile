FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY CarsAuction.sln ./
COPY AuctionService/AuctionService.csproj AuctionService/
COPY Contracts/Contracts.csproj Contracts/

RUN dotnet restore "AuctionService/AuctionService.csproj"

COPY . .

WORKDIR /src/AuctionService
RUN dotnet publish "AuctionService.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 80
COPY --from=build /app/publish .
ENTRYPOINT [ "dotnet", "AuctionService.dll" ]