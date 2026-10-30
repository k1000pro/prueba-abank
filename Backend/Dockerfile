FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# primero solo los csproj para aprovechar la caché del restore
COPY src/Abank.Usuarios.Domain/*.csproj src/Abank.Usuarios.Domain/
COPY src/Abank.Usuarios.Application/*.csproj src/Abank.Usuarios.Application/
COPY src/Abank.Usuarios.Infrastructure/*.csproj src/Abank.Usuarios.Infrastructure/
COPY src/Abank.Usuarios.Api/*.csproj src/Abank.Usuarios.Api/
RUN dotnet restore src/Abank.Usuarios.Api/Abank.Usuarios.Api.csproj

COPY src/ src/
RUN dotnet publish src/Abank.Usuarios.Api/Abank.Usuarios.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

USER app
ENTRYPOINT ["dotnet", "Abank.Usuarios.Api.dll"]
