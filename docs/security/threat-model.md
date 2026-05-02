# SettlersX Threat Model (v0.1)

> Living document. Each security phase (S1–S5) appends a new section.
> Phase numbers below reference the roadmap in
> `/root/.claude/plans/plan-the-next-steps-synthetic-widget.md`.

---

## Scope

SettlersX is a single-player offline desktop game. The trust boundaries we
defend today are:

| Boundary                         | Crossed by                                    | Phase |
|----------------------------------|-----------------------------------------------|-------|
| Source repo ↔ build pipeline     | NuGet packages, GitHub Actions, contributors  | S1    |
| Disk ↔ sim                       | Save files, mod resources, balance.json       | S2/S3 |
| Player input ↔ sim               | Intents, keyboard/mouse                       | S4    |
| Process ↔ outside world          | Crash dumps, telemetry, network               | S5    |

Out of scope (today): online multiplayer, telemetry, account systems,
in-game purchases. None exist. If any are added, this document gates
their merge.

---

## Threat Actors

- **External attacker** — targets the build pipeline (typosquat, malicious
  Action, compromised registry mirror) to land code in a release artifact.
- **Local attacker with disk access** — modifies save files or installs
  hostile mods. Cannot be defended against fully (key material is on disk),
  but corruption / accidental damage is detectable.
- **Curious modder** — legitimate contributor of fan content; we want to
  enable mods without granting them the same trust as shipped data.
- **Buggy contributor** — anyone (Claude included) who could land a
  regression that lets sim state leak across the Sim/View boundary or
  introduces a non-deterministic call. Architecture tests catch most.

We do **not** model nation-state adversaries: this is an offline single
-player game.

---

## S1 — Build & Supply Chain (current section)

### Assets

- `SettlersX.csproj` and `global.json` — declare every dependency and the
  exact .NET SDK band.
- `.github/workflows/*.yaml` — execute privileged code on push/PR.
- `packages.lock.json` (added by S1) — pins NuGet versions across restores.
- The release artifact (eventually).

### STRIDE table

| Threat                                     | Mitigation                                                                       |
|--------------------------------------------|----------------------------------------------------------------------------------|
| **Spoof:** typosquatted NuGet package      | NuGet feeds are read-only mirrors of nuget.org; no third-party feeds in nuget.config |
| **Tamper:** silent transitive bump         | `RestorePackagesWithLockFile=true` + `packages.lock.json` committed; CI fails on drift |
| **Tamper:** malicious GH Action update     | Dependabot opens grouped PRs weekly; long-term goal is SHA-pinning (follow-up)   |
| **Repudiation:** unsigned commits to main  | Signed-commits requirement documented in `branch-rules.md` (manual repo setting) |
| **Information disclosure:** secrets in CI  | GitHub secret scanning enabled at repo level; deny-list in `.claude/settings.json` |
| **DoS:** vuln in dep with public exploit   | `dotnet list package --vulnerable` runs on every CI build; `codeql.yml` weekly   |
| **Elevation:** build-time code execution   | `Deterministic=true` + `ContinuousIntegrationBuild=true` reduce undefined behavior |

### Open follow-ups

- [ ] Pin every GH Action to a commit SHA, not a moving tag.
- [ ] Add SBOM generation step (`dotnet-CycloneDX`) on release.
- [ ] Investigate the workflow-level `working-directory: SettlersX` —
      either the repo is nested or this path is stale.
- [ ] First successful CI run must commit the generated `packages.lock.json`
      into the branch; thereafter switch CI flag to `RestoreLockedMode=true`.

---

## S2 — Save File Integrity (placeholder)

Lands with F2.5. Will cover: HMAC, strict Newtonsoft settings, schema
validation, fuzz harness, save-bomb defence.

## S3 — Resource & Mod Sandboxing (placeholder)

Lands with F2b. Will cover: `.tres` allow-list, path-traversal guard,
per-def validators, mod manifest format.

## S4 — Sim Determinism + Anti-Cheat Hooks (placeholder)

Lands with F3. Will cover: replay tests, forbidden-API list (`DateTime.Now`,
unseeded `Random`), intent validation hardening.

## S5 — Telemetry, Crash Reports & Privacy (placeholder)

Lands after F3. Will cover: offline-only enforcement, PII scrubbing,
`SECURITY.md`, `privacy.md`, incident-response process.
