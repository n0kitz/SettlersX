---
name: new-system
description: Scaffold a new game system with manager, entity, tests
argument-hint: <SystemName>
allowed-tools: Read, Edit, Write, Bash(dotnet build:*), Bash(dotnet test:*), Glob, Grep
model: claude-sonnet-4-6
---
Create a new game system called `$ARGUMENTS`. Steps:

1. src/Sim/$ARGUMENTS/$ARGUMENTS Manager.cs — pure C# class, no Godot types
2. src/Sim/$ARGUMENTS/$ARGUMENTS Entity.cs — record or plain class
3. src/Views/$ARGUMENTS/$ARGUMENTS View.cs — partial Node3D, thin
4. tests/Sim/$ARGUMENTS/$ARGUMENTS ManagerTests.cs — baseline failing test
5. Update src/Sim/CLAUDE.md systems list

Context loaded automatically:
- Existing systems: !ls src/Sim/
- Autoloads: !grep -A 30 "[autoload]" project.godot
- Rules: @src/Sim/CLAUDE.md

After scaffolding: run dotnet build and confirm zero warnings.
