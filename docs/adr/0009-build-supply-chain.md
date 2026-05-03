# ADR 0009 — Build & Supply Chain Hardening

**Status:** Accepted
**Date:** 2026-05-02
**Context phase:** S1

## Context

The Phase-0 csproj declared every dependency with a floating major version
(`2.*`, `5.*`, `13.*`, `17.*`). NuGet's default behaviour resolves these on
each `dotnet restore`, meaning a transitive bump on any maintainer's machine
or CI runner could silently change the bytes that ship. There was no
`packages.lock.json`, no Dependabot, no CodeQL, no vulnerable-package audit,
no signed-commit requirement, and no SBOM.

For an offline single-player game this is not catastrophic, but the build
pipeline is still the most attractive target a remote attacker has against
this repo: a malicious package or a compromised GitHub Action would land
code in the artifact. Phase 1 just shipped the first vertical slice; locking
the supply chain before Phase 2 widens the package set is the cheapest
moment to do this.

## Decision

We adopt the following supply-chain controls in this commit:

1. **NuGet lock file.** `RestorePackagesWithLockFile=true` in
   `SettlersX.csproj`. The first CI restore will generate
   `packages.lock.json`; that file is committed and reviewed like any other
   source file. Once committed, CI switches to `RestoreLockedMode=true` so
   any drift fails the build.

2. **Pin Newtonsoft.Json.** It is the highest-risk package in our set
   (deserialisation surface) and has had RCEs in older `TypeNameHandling`
   modes. We pin to `13.0.3`. Other Chickensoft / gdUnit packages keep
   their current floats; the lock file pins them once committed.

3. **Deterministic builds.** `Deterministic=true` and a conditional
   `ContinuousIntegrationBuild=true` are added to `SettlersX.csproj`. This
   strips machine-specific paths from PDBs and produces byte-stable
   assemblies given the same inputs.

4. **CodeQL on a schedule.** A new `.github/workflows/codeql.yml` runs the
   `security-and-quality` query pack on every push, PR, and weekly cron.

5. **Dependabot.** `.github/dependabot.yml` watches NuGet and the
   `github-actions` ecosystem on a weekly cadence with grouped patch and
   minor PRs.

6. **Vulnerable package audit.** `dotnet list package --vulnerable
   --include-transitive` runs in `tests.yaml` after restore, before build.
   It is non-fatal today (we may inherit a transitive vuln we cannot fix
   immediately), but the output is in CI logs and a reviewer is expected to
   read it. A future commit will gate merges on it once we know the
   baseline.

7. **Branch protection documented.** `docs/security/branch-rules.md` records
   what the GitHub repo Settings UI must enforce: required CodeQL + Tests +
   Spellcheck status checks, signed commits on `main`, no force-pushes,
   linear history.

8. **Architecture test extended.** A new test in
   `tests/Architecture/ProjectHardeningTests.cs` confirms the csproj
   contains the supply-chain switches above. This means a regression that
   accidentally drops the lock file flag will fail CI immediately.

## Consequences

- First CI run fails until `packages.lock.json` is regenerated and
  committed. This is documented in `threat-model.md` open follow-ups.
- Pinning Newtonsoft.Json to `13.0.3` means we no longer get patch updates
  automatically; Dependabot will open a PR when a newer 13.x ships.
- Action SHA pinning is **not** part of this ADR; it is a follow-up. Tag
  pinning still allows a malicious tag move on a compromised Action.
- SBOM generation is **not** in this ADR; we add it when we ship the first
  external release artifact.

## Alternatives Considered

- **Central Package Management (Directory.Packages.props).** Cleaner for
  multi-project solutions, but we are single-project. Reconsider if a
  separate test project ever splits out.
- **Pin every package exactly in csproj.** Equivalent to a partial lock
  file, but loses transitive pinning. The lock file is strictly stronger.
- **Disable NuGet feed entirely / vendor packages.** Maximum security,
  unworkable for a fast-moving dependency set.

## References

- `SettlersX.csproj`
- `.github/dependabot.yml`
- `.github/workflows/codeql.yml`
- `.github/workflows/tests.yaml`
- `docs/security/threat-model.md`
- `docs/security/branch-rules.md`
- `tests/Architecture/ProjectHardeningTests.cs`
