FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY CarsAuction.sln ./
COPY IdentityService/IdentityService.csproj IdentityService/

RUN dotnet restore "IdentityService/IdentityService.csproj"

COPY . .

WORKDIR /src/IdentityService
RUN dotnet publish "IdentityService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 80
COPY --from=build /app/publish .
ENTRYPOINT [ "dotnet", "IdentityService.dll" ]