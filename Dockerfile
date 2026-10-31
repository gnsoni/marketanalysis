# ── Build stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the full source tree so every project is available in the build context,
# including nested library projects referenced by the web app.
COPY . .

RUN dotnet restore marketanalysis.sln

RUN dotnet publish marketanalysisspa/MarketAnalysis.csproj \
    -c Release -o /app/publish --no-restore

# ── Runtime stage ────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

LABEL org.opencontainers.image.title="MarketAnalysis" \
      org.opencontainers.image.description="Stock trend dashboard with MACD, RSI, EMA and more technical indicators" \
      org.opencontainers.image.licenses="MIT" \
      org.opencontainers.image.source="https://github.com/gnsoni/marketanalysis"

COPY --from=build /app/publish .
COPY --from=build /src/LICENSE /app/LICENSE

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "MarketAnalysis.dll"]
