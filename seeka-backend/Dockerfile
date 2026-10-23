FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY seeka-backend/seeka-backend.csproj seeka-backend/
RUN dotnet restore seeka-backend/seeka-backend.csproj
COPY seeka-backend/ seeka-backend/
RUN dotnet publish seeka-backend/seeka-backend.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "seeka-backend.dll"]
