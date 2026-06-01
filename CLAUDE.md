# CLAUDE.md — SettlersX

## Overview
Single-player 3D economic strategy game inspired by The Settlers 7: Paths to 
a Kingdom (Ubisoft Blue Byte, 2010). Players build medieval production chains 
across sector-based territory, competing via military, trade, and prestige 
victory paths. Windows-first. Single-player with bot opponents. Godot 4.4 + C#.

## Tech Stack
- Engine: Godot 4.4.1 (binary) + Godot.NET.Sdk 4.6.2 (NuGet), Mobile renderer
- Language: C# 14 (.NET 10), nullable enabled, warnings-as-errors
- State machines: Chickensoft.LogicBlocks (units and buildings)
- DI: Chickensoft.AutoInject
- Tests: gdUnit4Net 5.x via dotnet test
- Save: Newtonsoft.Json, versioned envelope, atomic write
- ECS: Deferred — add Friflo.Engine.ECS only in Phase 2+ if profiler justifies

## Build & Run Commands
- Build:       dotnet build SettlersX.sln -c Debug
- Format:      dotnet format
- Test (fast): dotnet test --settings .runsettings --filter "Category!=Integration"
- Test (all):  dotnet test --settings .runsettings
- Regression:  /regression-check
- Smoke:       godot --headless --path . --quit-after 5

## Architecture — READ BEFORE WRITING CODE
Strict sim/view split. Violating this is the most expensive mistake possible.

src/Sim/   — Pure C# game logic. No using Godot; except Vector2/3/2I/3I.
             Deterministic. Double arithmetic. No Node references.
             Owns: TickEngine, Economy, Production, Pathfinding, AI, Combat.

src/Views/ — Node subclasses only. Thin. Subscribe to Manager signals.
             No game state. No business logic. Send intents to Managers only.

src/Data/  — Resource subclasses (BuildingDef, RecipeDef, UnitDef).
             Read-only at runtime. Loaded by ResourceDatabase on startup.

data/      — .tres files and balance.json. Never hardcode numbers in .cs.

Managers   — Autoloads. Own the sim. Emit signals to views. 
             No cross-Manager method calls — use EventBus signals only.

Map model has TWO separate systems (both required from Phase 1):
  1. HexGrid — building footprints, pathfinding, AStar3D points
  2. SectorGraph — polygon regions for ownership, capture, VP scoring
  These are independent. Do not merge them.

Architecture test in tests/Architecture/ enforces Sim layer isolation.
Read @docs/architecture.md when it exists. Read @docs/adr/README.md 
before any structural change. Read src/<system>/CLAUDE.md for per-system rules.

## Game Rules That Affect Code
- Tick order: Input → AI intents → Pathfinding → Production → Transport →
  Consumption → Combat → Diplomacy → View snapshot
- Economy updates AFTER combat every tick
- Storehouse owns its carrier pool — no storehouse = no transport in sector
- Sectors need a Storehouse before production buildings function
- Save format is versioned JSON — schema bumps need a migration function
- Three victory paths: Military, Trade, Prestige — all three share one VP pool
- Carriers belong to storehouses, not to roads (Settlers 7 rule)

## Testing Requirements (mandatory per Opus review)
- Every new Sim class needs at least one unit test before merging
- Production chain logic needs property-based tests (input → expected output)
- Sim tests must NOT use [RequireGodotRuntime] — they run without Godot
- Integration tests (with Godot runtime) go in tests/Integration/ only
- Run /regression-check before every commit in Phase 2+

## Conventions
- C#: 2-space indent, PascalCase types/methods, _camelCase private fields
- One type per file. Filename must exactly match class name (GlobalClass rule)
- Every GodotObject subclass: public partial class Foo : Node
- Signal delegates: PastTenseVerb + EventHandler (e.g. BuildingConstructedEventHandler)
- Tests: <Class>Tests class, <Method>_<Condition>_<Expected> methods, AAA
- Commits: Conventional Commits. Scopes: sim, view, data, ui, ai, build, test, sec
- Claude Code auto-generated branches use the claude/ prefix (e.g. claude/<short-id>)
- Double for sim arithmetic — continuous quantities (positions, rates, multipliers, timers).
  Discrete counters (resource amounts, tick integers, hex coordinates, IDs) may use int.
  Fixed-point serialization only at save boundary.

## Progress
See PLAN.md for current phase and exit criteria.
Update PLAN.md at the end of every work session.

## Git Rules
- Branch: feat/<short> or fix/<short> from main
- Run: dotnet format && dotnet build && dotnet test before every commit
- Commit per milestone exit criterion, not per session
- NEVER touch project.godot autoloads without explicit approval
- NEVER push without asking

## Anti-Patterns — NEVER DO
- Game state in Node subclasses — use Resources or plain C# records
- using Godot; inside src/Sim/ except math structs
- Cross-Manager method calls — use EventBus signals
- await in _Ready() or _EnterTree() — use _ = InitAsync();
- CharacterBody3D for crowds — use MultiMesh + PhysicsServer3D
- NavigationAgent3D with avoidance on more than 10 agents
- Hardcoded balance numbers in .cs — edit data/balance.json
- New autoloads without explicit approval
- Godot.Collections for internal sim state (marshaling cost)
- Dispose() on GodotObject — use QueueFree()
- Godot 3 API: Instance(), connect(str,obj,str), Spatial, VisualServer
- Missing IsInstanceValid(this) after any await in a Node method
- Widening scope beyond the active PLAN.md milestone without asking

## Godot 4 C# Rules (never forget)
- Every GodotObject subclass must be partial
- No constructor parameters on Node or Resource subclasses
- GlobalClass filename must match class name case-sensitively
- Cache StringNames: static readonly StringName _name = "name";
- Every signal Connect in _Ready needs Disconnect in _ExitTree
- Scene tree is single-threaded — use CallDeferred from workers
- RenderingServer, PhysicsServer3D, NavigationServer3D are thread-safe
- Delta in _Process is double, not float

## Skills

@caveman/SKILL.md

@promptimprover/SKILL.md
