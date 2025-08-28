FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY CarsAuction.sln ./
COPY BiddingService/BiddingService.csproj BiddingService/
COPY Contracts/Contracts.csproj Contracts/

RUN dotnet restore "BiddingService/BiddingService.csproj"

COPY . .

WORKDIR /src/BiddingService
RUN dotnet publish "BiddingService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 80
COPY --from=build /app/publish .
ENTRYPOINT [ "dotnet", "BiddingService.dll" ]