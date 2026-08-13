FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY UniversidadeAPI/UniversidadeAPI.csproj UniversidadeAPI/
RUN dotnet restore UniversidadeAPI/UniversidadeAPI.csproj

COPY UniversidadeAPI/ UniversidadeAPI/
RUN dotnet publish UniversidadeAPI/UniversidadeAPI.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "UniversidadeAPI.dll"]
