FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source

# Copy solution file and all project files first for layer caching
COPY aidemo.slnx ./
COPY src/ReservationPlatform.Domain/ReservationPlatform.Domain.csproj src/ReservationPlatform.Domain/
COPY src/ReservationPlatform.Application/ReservationPlatform.Application.csproj src/ReservationPlatform.Application/
COPY src/ReservationPlatform.Infrastructure/ReservationPlatform.Infrastructure.csproj src/ReservationPlatform.Infrastructure/
COPY src/ReservationPlatform.API/ReservationPlatform.API.csproj src/ReservationPlatform.API/

RUN dotnet restore src/ReservationPlatform.API/ReservationPlatform.API.csproj

# Copy all source code and publish
COPY src/ src/
RUN dotnet publish src/ReservationPlatform.API/ReservationPlatform.API.csproj \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 80

# Create logs directory
RUN mkdir -p /app/logs

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ReservationPlatform.API.dll"]
