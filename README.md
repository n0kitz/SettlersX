# SettlersX

Single-player 3D economic strategy game inspired by *The Settlers 7: Paths to a Kingdom*.
Build medieval production chains across sector-based territory. Compete via military, trade,
and prestige victory paths.

**Engine:** Godot 4.4.1 (binary) + Godot.NET.Sdk 4.6.2 | **Language:** C# 14 / .NET 10

## Build & Run

```sh
# Build
dotnet build SettlersX.sln -c Debug

# Format check
dotnet format --verify-no-changes

# Tests (fast — no Godot runtime needed)
dotnet test --settings .runsettings --filter "Category!=Integration"

# All tests
dotnet test --settings .runsettings

# Smoke run (requires GODOT env var)
godot --headless --path . --quit-after 5
```

## Architecture

Strict sim/view split — see [CLAUDE.md](CLAUDE.md) for the full rules.

```
src/Sim/    Pure C# game logic — deterministic, no Godot types
src/Views/  Godot Node subclasses — thin, no game state
src/Data/   Resource subclasses (BuildingDef, RecipeDef, UnitDef)
src/Core/   Autoload bridges (EventBus, WorldSim, GameManager, …)
data/       .tres files + balance.json
```

## Docs

- [PLAN.md](PLAN.md) — Phase roadmap and exit criteria
- [docs/adr/README.md](docs/adr/README.md) — Architecture Decision Records
- [docs/architecture.md](docs/architecture.md) — Layer guide and signal flow

## Status

Phase 0 complete (scaffold + autoloads + TickEngine).
Phase 1 (hex grid, roads, carrier) in planning — see PLAN.md.
