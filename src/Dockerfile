
# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "./VGManager.Library.Api/VGManager.Library.Api.csproj" --disable-parallel
RUN dotnet publish "./VGManager.Library.Api/VGManager.Library.Api.csproj" -c release -o /app --no-restore

# Serve Stage
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app ./

EXPOSE 5002

ENTRYPOINT ["dotnet", "VGManager.Library.Api.dll"]
