# Deployment

> Cross-references: [Docs Index](INDEX.md) · [Architecture](architecture.md)

Backend → Render (free tier, Docker). Frontend → Vercel (native GitHub integration). CI gate → [`.github/workflows/api-ci.yml`](../.github/workflows/api-ci.yml).

## Why this split

Vercel has no .NET runtime, so the API can't live there (see [CLAUDE.md](../CLAUDE.md) "Future Considerations"). Render's free tier has no native .NET runtime either, so the API ships as a Docker image ([Dockerfile](../src/GalleryManager.Api/Dockerfile)). The Angular build is static output — Vercel handles that natively, no container needed.

`api-ci.yml`'s `deploy` job only triggers Render (via deploy-hook `curl`) after `build-and-test` passes — a broken build never reaches Render. Vercel deploys itself on every push to `main` via its own GitHub App integration, independent of this workflow.

## Checklist

### Repo
- [x] `git init`, initial commit, `.gitignore` verified (no `bin/`, `obj/`, `node_modules/`, secrets)
- [x] `gh repo create gallery-manager --public --source=. --push`
- [x] Default branch renamed `master` → `main` (matches `api-ci.yml` branch trigger)

### Backend code readiness
- [x] `Program.cs` binds to Render's `$PORT` when set (local dev unaffected)
- [x] `Program.cs` has `UseForwardedHeaders` before `UseHttpsRedirection` (avoids redirect loop behind Render's proxy)
- [x] [`src/GalleryManager.Api/Dockerfile`](../src/GalleryManager.Api/Dockerfile) — multi-stage `sdk:8.0` → `aspnet:8.0`
- [x] [`.dockerignore`](../.dockerignore) at repo root
- [x] `/health` endpoint exists (used as Render health check)
- [x] CORS reads `Frontend:VercelUrl` config key (already wired pre-deployment)

### CI pipeline
- [x] `api-ci.yml` `deploy` job replaced placeholder with `curl -fsS -X POST "${{ secrets.RENDER_DEPLOY_HOOK_URL }}"`
- [x] Verified `build-and-test` job goes green on push to `main`
- [x] `RENDER_DEPLOY_HOOK_URL` secret set on GitHub repo; `deploy` job verified green (rerun of run 28881113751)

### Render (backend) — manual dashboard steps
- [x] New Web Service created, connected to `dariemcarlosdev/gallery-manager` — `gallery-manager-api`
- [x] Runtime: Docker, Dockerfile path `src/GalleryManager.Api/Dockerfile`, build context = repo root
- [ ] Env var `ConnectionStrings__GalleryDb` set (Neon connection string) — confirm in Render dashboard
- [ ] Env var `ASPNETCORE_ENVIRONMENT=Production` set — confirm in Render dashboard
- [ ] Health check path set to `/health` — confirm in Render dashboard
- [x] Deploy-hook URL copied → added as GitHub secret `RENDER_DEPLOY_HOOK_URL`
- [x] Service URL copied — `https://gallery-manager-api.onrender.com`

### Vercel (frontend) — manual dashboard steps
- [x] Project imported from same GitHub repo
- [x] Root Directory set to `src/gallery-manager-web`
- [x] Framework preset: Angular
- [x] Build command: `npm run build -- --configuration production`
- [x] Output directory: `dist/gallery-manager-web/browser`
- [x] Vercel URL copied — `https://gallery-manager-henna.vercel.app`

### Cross-wire (final step, after both URLs known)
- [x] `src/gallery-manager-web/src/environments/environment.ts` `apiUrl` → `https://gallery-manager-api.onrender.com/api`, commit + push
- [ ] Render env var `Frontend__VercelUrl` → `https://gallery-manager-henna.vercel.app`, redeploy (user to set in Render dashboard)

## Verification

1. `dotnet build GalleryManager.sln` — clean.
2. Push to `main` → `gh run list` shows `build-and-test` green, `deploy` green (once Render secret set).
3. `GET https://<render-service>.onrender.com/health` → `{"status":"ok"}`.
4. Open Vercel URL → `/artworks` loads, calls Render API, no CORS error in browser console.

## Last synced
2026-07-07 — repo pushed, CI build gate verified green, Render/Vercel dashboard steps pending.
