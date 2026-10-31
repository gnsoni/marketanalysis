# MarketAnalysis

MarketAnalysis is an open-source stock charting and technical analysis dashboard built with .NET 10. It provides a lightweight, browser-based interface for researching stock symbols, viewing trend data, and exploring popular technical indicators such as MACD, RSI, EMA, Bollinger Bands, ADX, Aroon, and SAR.

This project is a smaller, focused subset of the earlier MarketPlusStockPro effort, which was retired in 2025. The goal is to preserve the market-analysis workflow in a simpler, easier-to-run codebase for local experimentation, learning, and community use.

## Features

- Search and filter symbols across major exchanges
- Technical analysis dashboard with multiple chart views
- Trend and momentum indicators
- ASP.NET Core web app with an interactive sidebar UI
- Multi-arch container support for Linux AMD64 and ARM64

## Project structure

- `marketanalysisspa/` – ASP.NET Core web app
- `marketanalysis/` – analysis library
- `marketanalysisyfinance/` – market data integration library
- `.github/workflows/container-build.yml` – GitHub Actions workflow for container publishing
- `Dockerfile` – container build for the app
- `docker-compose.yml` – example deployment file for local or GHCR-based runs

## Requirements

- .NET 10 SDK
- Docker or Podman (optional, for container builds)
- GitHub Container Registry access if publishing to `ghcr.io`
- Docker Hub access if publishing to `docker.io`

## Run locally

### Option 1: Run with .NET

From the repository root:

```bash
dotnet restore marketanalysis.sln
dotnet run --project marketanalysisspa/MarketAnalysis.csproj
```

The app defaults to the ASP.NET Core development URL, usually:

- http://localhost:5000
- https://localhost:5001

### Option 2: Run with Docker

Build locally:

```bash
docker build -t marketanalysis:local .
```

Run locally:

```bash
docker run --rm -p 8080:8080 marketanalysis:local
```

Then open:

```text
http://localhost:8080
```

## Docker Compose

A sample `docker-compose.yml` is included in the repository root.

### Run from GHCR

```yaml
version: "3.9"

services:
  marketanalysis:
    image: ghcr.io/gnsoni/marketanalysis:latest
    container_name: marketanalysis
    restart: unless-stopped
    environment:
      ASPNETCORE_URLS: http://+:8080
      TZ: UTC
    ports:
      - "8080:8080"
    volumes:
      - ./LICENSE:/app/LICENSE:ro
```

Start it with:

```bash
docker compose up -d
```

To follow logs:

```bash
docker compose logs -f marketanalysis
```

### Run from a local Docker build

If you want to build the image from source instead of pulling GHCR:

```yaml
version: "3.9"

services:
  marketanalysis:
    build:
      context: .
      dockerfile: Dockerfile
    container_name: marketanalysis
    restart: unless-stopped
    environment:
      ASPNETCORE_URLS: http://+:8080
      TZ: UTC
    ports:
      - "8080:8080"
    volumes:
      - ./LICENSE:/app/LICENSE:ro
```

Start it with:

```bash
docker compose up --build -d
```

## Deploy from GitHub Container Registry (ghcr.io)

After the GitHub Action runs, the image is published to GHCR using the repo owner and image name. Example:

```bash
docker pull ghcr.io/<your-user>/marketanalysis:latest
```

Run it:

```bash
docker run --rm -p 8080:8080 ghcr.io/<your-user>/marketanalysis:latest
```

If you want to pin a specific tag or commit:

```bash
docker pull ghcr.io/<your-user>/marketanalysis:<tag>
docker run --rm -p 8080:8080 ghcr.io/<your-user>/marketanalysis:<tag>
```

## Deploy from Docker Hub (docker.io)

Tag the image for Docker Hub:

```bash
docker tag marketanalysis:local <your-dockerhub-user>/marketanalysis:latest
docker push <your-dockerhub-user>/marketanalysis:latest
```

Then run from Docker Hub:

```bash
docker run --rm -p 8080:8080 <your-dockerhub-user>/marketanalysis:latest
```

## Change the port

The container listens on port 8080 by default. You can change this by overriding the environment variable and the published port mapping.

Example: run on port 9000 instead of 8080:

```bash
docker run --rm -p 9000:8080 -e ASPNETCORE_URLS=http://+:8080 marketanalysis:local
```

Then open:

```text
http://localhost:9000
```

If you are running the .NET app directly and want to override the ASP.NET Core port, use:

```bash
dotnet run --project marketanalysisspa/MarketAnalysis.csproj --urls http://localhost:9000
```

## View logs

When running in Docker:

```bash
docker logs <container-name-or-id>
```

For a running container in foreground mode:

```bash
docker run --rm -p 8080:8080 marketanalysis:local
```

You can also tail logs in a running container:

```bash
docker logs -f <container-name-or-id>
```

With Podman, the commands are the same pattern:

```bash
podman run --rm -p 8080:8080 marketanalysis:local
podman logs -f <container-name-or-id>
```

## License

This project is licensed under the MIT License. The license file is included in the repository root as `LICENSE` and is also copied into the container image so the About page can serve it as part of the application.

## Notes

- The app is designed for technical analysis and charting workflows.
- The container workflow builds for Linux AMD64 and ARM64.
- The project is a lighter subset of the retired MarketPlusStockPro effort.
