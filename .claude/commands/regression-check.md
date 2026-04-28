---
name: regression-check
description: Run full test suite + build. Required before every Phase 2+ commit.
allowed-tools: Bash(dotnet build:*), Bash(dotnet test:*), Bash(dotnet format:*)
model: claude-sonnet-4-6
---
Run in this exact order and report results:

1. dotnet format --verify-no-changes
   (fail = formatting errors exist — fix before continuing)

2. dotnet build SettlersX.sln -c Debug
   (fail = compile errors or warnings — stop, report all)

3. dotnet test --settings .runsettings --filter "Category!=Integration"
   (fast sim tests — must all pass)

4. dotnet test --settings .runsettings --filter "Category=Architecture"
   (architecture isolation — must pass)

5. dotnet test --settings .runsettings
   (all tests including integration if Godot is available)

Report format:
  Format:       [PASS/FAIL]
  Build:        [PASS/FAIL — N warnings]
  Sim tests:    [PASS/FAIL — N/N passed]
  Arch test:    [PASS/FAIL]
  All tests:    [PASS/FAIL — N/N passed]
  OVERALL:      [GREEN / RED]

If RED: list every failing test with the failure message.
Do not commit if RED.
