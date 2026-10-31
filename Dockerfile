# ── Build stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files in the exact casing used by the repo so Linux builds work.
COPY marketanalysisspa/MarketAnalysis.csproj marketanalysisspa/
COPY marketanalysis/MarketPlusAnalysis.csproj marketanalysis/
COPY marketanalysisyfinance/MarketPlusYFinance.csproj marketanalysisyfinance/

RUN dotnet restore marketanalysisspa/MarketAnalysis.csproj

# Copy the rest of the source after restore so project references are available.
COPY . .

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

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "MarketAnalysis.dll"]
