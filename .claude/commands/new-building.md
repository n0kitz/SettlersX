---
name: new-building
description: Scaffold building def, scene, view, and tests
argument-hint: <BuildingName>
allowed-tools: Read, Edit, Write, Bash(dotnet build:*), Glob, Grep
model: claude-sonnet-4-6
---
Create building `$ARGUMENTS`. Steps:

1. src/Data/Buildings/$ARGUMENTS Def.cs — [GlobalClass] Resource, [Export] only
2. data/buildings/$ARGUMENTS.tres — matching .tres with default values
3. scenes/buildings/$ARGUMENTS.tscn — placeholder Node3D scene
4. src/Views/Buildings/$ARGUMENTS View.cs — thin partial Node3D
5. tests/Sim/$ARGUMENTS DefTests.cs — validation test

Rules: @src/Data/CLAUDE.md
Existing: !ls data/buildings/
Balance values → data/balance.json only. Never in .cs or .tres.
