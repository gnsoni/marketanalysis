# Implementation Plan — marketanalysis

## Findings from codebase exploration

| Fact | Detail |
|------|--------|
| .NET SDK installed | **10.0.202** — upgrade target is `net10.0` |
| All four projects today | `net8.0` |
| Wrong project reference | `MarketPlusStockSPA\MarketAnalysis.csproj` → `..\MarketPlus.Analysis\MarketPlusAnalysis.csproj` (path does not exist; correct path is `..\MarketAnalysis\MarketPlusAnalysis.csproj`) |
| Entities used by SPA? | **No** — the SPA has no `ProjectReference` to Entities. Entities is pulled in transitively only because `MarketPlusAnalysis.csproj` has `<ProjectReference Include="..\Entities\Entities.csproj" />`. The SPA's C# files never `using Entities` or reference any Entities type directly. |
| `System.Text.Json` in YFinance | Version `5.0.2` explicitly referenced — ships inbox on .NET 8+; must be removed |
| `Newtonsoft.Json` in MarketAnalysis | Listed in `MarketPlusAnalysis.csproj` but **no Newtonsoft/JsonConvert usage found** in any `.cs` file under `MarketAnalysis\` — safe to remove |
| `Microsoft.Extensions.Identity.Stores` | Current version `8.0.26` in Entities; latest available `10.0.14` — must be updated |
| Solution file | **Does not exist** — `marketanalysis.sln` must be created |
| `.gitignore` | **Does not exist** at repo root — must be created |
| `.github/` folder | **Does not exist** — must be created with workflow YAML |
| `Ohlc` class location | Defined in `MarketAnalysis\OHLC.cs` (namespace `MarketPlusAnalysis`) — the SPA controller resolves it via the `MarketPlusAnalysis` project reference, not from YFinance |
| `LICENSE` | Valid MIT licence; needs Third-Party sections for Yahoo Finance data and software dependencies |

---

## TASK 1 — Cross-check and fix the implementation

- [ ] 1. **Fix the broken ProjectReference path in `MarketPlusStockSPA\MarketAnalysis.csproj`.**
  Change `..\MarketPlus.Analysis\MarketPlusAnalysis.csproj` to `..\MarketAnalysis\MarketPlusAnalysis.csproj`.
  File: `MarketPlusStockSPA\MarketAnalysis.csproj`
  Verify: `dotnet build d:\gitProjects\marketanalysis\MarketPlusStockSPA\MarketAnalysis.csproj` — must compile without a "project not found" error.

- [ ] 2. **Remove the redundant `System.Text.Json` PackageReference from `MarketPlusYFinance.csproj`.**
  Delete the line `<PackageReference Include="System.Text.Json" Version="5.0.2" />` — the API ships in-box on .NET 8+ and must not be pegged to a downlevel version.
  File: `MarketPlusYFinance\MarketPlusYFinance.csproj`
  Verify: `dotnet build d:\gitProjects\marketanalysis\MarketPlusYFinance\MarketPlusYFinance.csproj` — builds cleanly.

- [ ] 3. **Remove the unused `Newtonsoft.Json` PackageReference from `MarketPlusAnalysis.csproj`.**
  No `.cs` file under `MarketAnalysis\` uses `Newtonsoft`, `JsonConvert`, or any `Newtonsoft.*` namespace — the dependency is vestigial. Delete `<PackageReference Include="Newtonsoft.Json" Version="13.0.1" />`.
  File: `MarketAnalysis\MarketPlusAnalysis.csproj`
  Verify: `dotnet build d:\gitProjects\marketanalysis\MarketAnalysis\MarketPlusAnalysis.csproj` — builds cleanly with no unresolved references.

- [ ] 4. **Document Entities transitive dependency (no code change needed).**
  Entities is **not** a direct dependency of the SPA. It is pulled in transitively because `MarketPlusAnalysis.csproj` references it for its `AppUser`/Identity types. The SPA itself has zero direct Entities imports. No change is required — this is a note for the solution file step below (Entities must still be included in the solution so `dotnet build` can resolve the full graph).

- [ ] 5. **Create `marketanalysis.sln` at the repo root** containing all four projects.
  Run these commands in order:
  ```
  cd d:\gitProjects\marketanalysis
  dotnet new sln -n marketanalysis
  dotnet sln marketanalysis.sln add Entities\Entities.csproj
  dotnet sln marketanalysis.sln add MarketAnalysis\MarketPlusAnalysis.csproj
  dotnet sln marketanalysis.sln add MarketPlusYFinance\MarketPlusYFinance.csproj
  dotnet sln marketanalysis.sln add MarketPlusStockSPA\MarketAnalysis.csproj
  ```
  File created: `marketanalysis.sln`
  Verify: `dotnet build d:\gitProjects\marketanalysis\marketanalysis.sln` — all four projects build, 0 errors.

---

## TASK 2 — Upgrade all projects to .NET 10

- [ ] 6. **Change `TargetFramework` from `net8.0` to `net10.0` in all four `.csproj` files.**
  Also update `Microsoft.Extensions.Identity.Stores` from `8.0.26` → `10.0.14` in `Entities\Entities.csproj`.
  Files (all four must be edited):
  - `Entities\Entities.csproj`
  - `MarketAnalysis\MarketPlusAnalysis.csproj`
  - `MarketPlusYFinance\MarketPlusYFinance.csproj`
  - `MarketPlusStockSPA\MarketAnalysis.csproj`
  Verify: `dotnet build d:\gitProjects\marketanalysis\marketanalysis.sln` — all projects target `net10.0`, 0 errors.
  Confirm with: `dotnet --version` (expected `10.0.202`).

---

## TASK 3 — Dockerfile

- [ ] 7. **Create `Dockerfile` at the repo root** using a multi-stage build.
  Stage 1 — `sdk:10.0-alpine` (or `sdk:10.0` if alpine unavailable): restore, publish the SPA in Release configuration to `/app/publish`.
  Stage 2 — `aspnet:10.0-alpine` (or `aspnet:10.0`): copy published output, set `ASPNETCORE_URLS=http://+:8080`, `EXPOSE 8080`, `ENTRYPOINT ["dotnet", "MarketAnalysis.dll"]`.
  The SPA project sets `<AssemblyName>MarketAnalysis</AssemblyName>` so the output dll is `MarketAnalysis.dll`.
  Use the solution file path in the build stage so all dependencies are restored in one pass:
  ```dockerfile
  FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
  WORKDIR /src
  COPY . .
  RUN dotnet publish MarketPlusStockSPA/MarketAnalysis.csproj \
      -c Release -o /app/publish --no-self-contained

  FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
  WORKDIR /app
  COPY --from=build /app/publish .
  ENV ASPNETCORE_URLS=http://+:8080
  EXPOSE 8080
  ENTRYPOINT ["dotnet", "MarketAnalysis.dll"]
  ```
  File: `Dockerfile` (repo root)
  Verify: `docker build -t marketanalysis:local .` from `d:\gitProjects\marketanalysis` — image builds successfully (requires Docker Desktop).

---

## TASK 4 — GitHub Actions workflow

- [ ] 8. **Create `.github/workflows/docker-build-push.yml`.**
  Directory `.github\workflows\` must be created.
  The workflow must:
  - Trigger on `push` to `main` or `master`, on version tags (`v*.*.*`), and on `pull_request` to `main`/`master`.
  - On PRs: build only (no push).
  - On push/tag: build and push to both GHCR and Docker Hub.
  - Use `docker/setup-buildx-action@v3`, `docker/login-action@v3`, `docker/metadata-action@v5`, `docker/build-push-action@v6`.
  - Add a comment block at the top explaining that `DOCKERHUB_USERNAME` and `DOCKERHUB_TOKEN` must be set as repository secrets (Settings → Secrets and variables → Actions).
  - GHCR login uses `${{ github.actor }}` / `${{ secrets.GITHUB_TOKEN }}` (automatic).

  Full file content:
  ```yaml
  # ──────────────────────────────────────────────────────────────────────────────
  # Required repository secrets (Settings → Secrets and variables → Actions):
  #   DOCKERHUB_USERNAME  – your Docker Hub username
  #   DOCKERHUB_TOKEN     – a Docker Hub Personal Access Token (read/write)
  # GITHUB_TOKEN is provided automatically by GitHub Actions.
  # ──────────────────────────────────────────────────────────────────────────────
  name: Docker Build & Push

  on:
    push:
      branches: [main, master]
      tags: ["v*.*.*"]
    pull_request:
      branches: [main, master]

  env:
    REGISTRY_GHCR: ghcr.io
    IMAGE_NAME: ${{ github.repository }}   # e.g. username/marketanalysis

  jobs:
    build-and-push:
      runs-on: ubuntu-latest
      permissions:
        contents: read
        packages: write

      steps:
        - name: Checkout
          uses: actions/checkout@v4

        - name: Set up Docker Buildx
          uses: docker/setup-buildx-action@v3

        - name: Log in to GitHub Container Registry
          if: github.event_name != 'pull_request'
          uses: docker/login-action@v3
          with:
            registry: ${{ env.REGISTRY_GHCR }}
            username: ${{ github.actor }}
            password: ${{ secrets.GITHUB_TOKEN }}

        - name: Log in to Docker Hub
          if: github.event_name != 'pull_request'
          uses: docker/login-action@v3
          with:
            username: ${{ secrets.DOCKERHUB_USERNAME }}
            password: ${{ secrets.DOCKERHUB_TOKEN }}

        - name: Extract image metadata
          id: meta
          uses: docker/metadata-action@v5
          with:
            images: |
              ${{ env.REGISTRY_GHCR }}/${{ env.IMAGE_NAME }}
              ${{ secrets.DOCKERHUB_USERNAME }}/marketanalysis
            tags: |
              type=ref,event=branch
              type=ref,event=pr
              type=semver,pattern={{version}}
              type=semver,pattern={{major}}.{{minor}}
              type=sha

        - name: Build and push
          uses: docker/build-push-action@v6
          with:
            context: .
            push: ${{ github.event_name != 'pull_request' }}
            tags: ${{ steps.meta.outputs.tags }}
            labels: ${{ steps.meta.outputs.labels }}
            cache-from: type=gha
            cache-to: type=gha,mode=max
  ```
  File: `.github\workflows\docker-build-push.yml`
  Verify: Push to GitHub and confirm Actions tab shows the workflow; it runs the build step on a PR without pushing.

---

## TASK 5 — Update LICENSE

- [ ] 9. **Replace `LICENSE` with expanded MIT text + third-party notices.**
  Keep the existing copyright holder (`Copyright (c) 2026 Gopal S`) and MIT body verbatim. Append two new sections:

  **THIRD-PARTY DATA NOTICE — Yahoo Finance**
  State that the application retrieves market data from Yahoo Finance via its public API. This data is subject to Yahoo's terms of service (https://legal.yahoo.com/us/en/yahoo/terms/otos/index.html). The application is not affiliated with or endorsed by Yahoo Inc. Use of Yahoo Finance data must comply with their terms; commercial use may require separate licensing. The project provides no warranty regarding data accuracy or availability.

  **THIRD-PARTY SOFTWARE DEPENDENCIES**
  List each dependency, its licence, and a source URL:
  | Dependency | Licence | Source |
  |---|---|---|
  | ASP.NET Core (Microsoft) | MIT | https://github.com/dotnet/aspnetcore |
  | Apache ECharts (Apache Software Foundation) | Apache 2.0 | https://github.com/apache/echarts |
  | Argon Dashboard (Creative Tim) | MIT | https://www.creative-tim.com/product/argon-dashboard |
  | Bootstrap | MIT | https://github.com/twbs/bootstrap |
  | Font Awesome Free | Icons: CC BY 4.0 / Fonts: SIL OFL 1.1 / Code: MIT | https://github.com/FortAwesome/Font-Awesome |
  | Newtonsoft.Json (James Newton-King) | MIT | https://github.com/JamesNK/Newtonsoft.Json |

  File: `LICENSE` (repo root — overwrite existing)
  Verify: `Get-Content d:\gitProjects\marketanalysis\LICENSE` — file contains the MIT text, the Yahoo Finance notice, and the dependencies table.

---

## TASK 6 — .gitignore

- [ ] 10. **Create a standard .NET `.gitignore` at the repo root.**
  Run:
  ```
  dotnet new gitignore -o d:\gitProjects\marketanalysis
  ```
  This generates `.gitignore` covering `bin/`, `obj/`, `.vs/`, user files, etc.
  File: `.gitignore` (repo root)
  Verify: `Test-Path d:\gitProjects\marketanalysis\.gitignore` returns `True`. Inspect that `obj/` and `bin/` are covered.

---

## TASK 7 — Final build verification

- [ ] 11. **Run the full solution build and confirm 0 errors.**
  ```
  dotnet build d:\gitProjects\marketanalysis\marketanalysis.sln -c Release
  ```
  Expected output: `Build succeeded. 0 Error(s)`.
  Report the resolved .NET version: `dotnet --version` → `10.0.202`.

---

## Dependency order summary

```
Step 1 (fix csproj path)
  ↓
Step 2 (remove System.Text.Json)
Step 3 (remove Newtonsoft.Json)
  ↓
Step 5 (create solution — needs corrected csproj)
  ↓
Step 6 (upgrade to net10.0 — needs solution)
  ↓
Step 7 (Dockerfile — uses net10.0 images)
Step 8 (GitHub Actions — references Dockerfile)
  ↓
Step 9 (LICENSE — independent, any order)
Step 10 (.gitignore — independent, any order)
  ↓
Step 11 (final build — all prior steps must be complete)
```
