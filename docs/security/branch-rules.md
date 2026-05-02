# Branch Protection Rules

These rules are configured via the GitHub repo Settings UI and cannot be
expressed as code in this repo. This file documents the required state so a
new maintainer can audit it.

## `main`

- **Require pull request before merging:** yes.
- **Require approvals:** at least 1.
- **Require review from Code Owners:** yes (when `CODEOWNERS` is added).
- **Dismiss stale approvals on new commits:** yes.
- **Require status checks before merging:** yes. Required checks:
  - `🧪 Tests / 🧪 Build & Test`
  - `🛡 CodeQL / 🔍 Analyze C#`
  - `🔤 Spellcheck` (existing workflow)
- **Require branches to be up to date:** yes.
- **Require signed commits:** yes (GPG or SSH-signed). Rejects unsigned
  pushes.
- **Require linear history:** yes (no merge commits on `main`).
- **Restrict who can push:** maintainers only.
- **Allow force pushes:** **no**.
- **Allow deletions:** **no**.

## `claude/**`

Working branches authored by Claude Code. The same protections apply with
two relaxations:

- Force pushes from the original author are allowed (work-in-progress).
- Linear history is not required (rebases happen frequently during review).

Required checks remain the same; any green PR can land via squash merge to
`main`, never a fast-forward merge.

## `feat/**` and `sec/**`

Same as `claude/**`. Naming follows the roadmap's convention:
`feat/f1-hex-grid`, `sec/s1-supply-chain`.

## Repository-level security settings

- **Secret scanning:** enabled.
- **Push protection:** enabled.
- **Dependency graph:** enabled.
- **Dependabot alerts:** enabled.
- **Dependabot security updates:** enabled.
- **Code scanning (CodeQL):** enabled via the `codeql.yml` workflow on push,
  PR, and the weekly schedule.
- **Vulnerability reporting:** the `SECURITY.md` (added in S5) lists a
  contact and disclosure timeline.

## Auditing

A maintainer should check this list every quarter or after any major repo
re-org. If a setting drifts from this document, update both the setting
and this file in the same PR.
