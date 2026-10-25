FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY seeka-backend/Seeka.Engines/Seeka.Engines.csproj seeka-backend/Seeka.Engines/
COPY seeka-backend/Seeka.Managers/Seeka.Managers.csproj seeka-backend/Seeka.Managers/
COPY seeka-backend/Seeka.ResourceAccess/Seeka.ResourceAccess.csproj seeka-backend/Seeka.ResourceAccess/
COPY seeka-backend/Seeka.Api/Seeka.Api.csproj seeka-backend/Seeka.Api/
RUN dotnet restore seeka-backend/Seeka.Api/Seeka.Api.csproj
COPY seeka-backend/ seeka-backend/
RUN dotnet publish seeka-backend/Seeka.Api/Seeka.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Seeka.Api.dll"]
