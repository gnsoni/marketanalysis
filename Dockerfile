# ── Build stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and all project files first (layer-cache friendly)
COPY marketanalysis.slnx .
COPY Entities/Entities.csproj               Entities/
COPY MarketAnalysis/MarketPlusAnalysis.csproj MarketAnalysis/
COPY MarketPlusYFinance/MarketPlusYFinance.csproj MarketPlusYFinance/
COPY marketanalysisspa/MarketAnalysis.csproj marketanalysisspa/

RUN dotnet restore marketanalysisspa/MarketAnalysis.csproj

# Copy the rest of the source
COPY . .

RUN dotnet publish marketanalysisspa/MarketAnalysis.csproj \
    -c Release -o /app/publish --no-restore

# ── Runtime stage ────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

LABEL org.opencontainers.image.title="MarketAnalysis" \
      org.opencontainers.image.description="Stock trend dashboard with MACD, RSI, EMA and more technical indicators" \
      org.opencontainers.image.licenses="MIT" \
      org.opencontainers.image.source="https://github.com/YOUR_USERNAME/marketanalysis"

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "MarketAnalysis.dll"]
