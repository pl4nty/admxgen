# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Copy the project files
COPY admxgen/*.csproj .

# Restore dependencies
RUN dotnet restore .

COPY admxgen .

# Build the project
RUN dotnet build . -c Release -o /app/build

# Publish the project
RUN dotnet publish . -c Release -o /app/publish

FROM joseluisq/static-web-server:2.33.0
COPY --from=build /app/publish/wwwroot ./public
