# ── Build stage ───────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY RaceDay.MVC/RaceDay.MVC.csproj RaceDay.MVC/
RUN dotnet restore RaceDay.MVC/RaceDay.MVC.csproj

COPY RaceDay.MVC/ RaceDay.MVC/
WORKDIR /src/RaceDay.MVC
RUN dotnet publish -c Release -o /app/publish --no-restore

# ── Runtime stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production
# Point to API running on the host (override at runtime with -e)
ENV ApiSettings__BaseUrl=http://host.docker.internal:5000/

EXPOSE 80
ENTRYPOINT ["dotnet", "RaceDay.MVC.dll"]
