# Core Layer

Godot autoload managers that bridge `src/Sim/` (pure C#) and `src/Views/` (Godot nodes).

Rules:
- All classes here are `partial Node` subclasses — they live in the Godot scene tree.
- Delegate ALL game logic to `src/Sim/` types. No business logic here.
- Emit signals via EventBus. Do not call other Manager methods directly.
- New autoloads require explicit approval — add them in `project.godot` only after discussion.

Current autoloads (registered in project.godot in this order):
1. EventBus — signal hub for cross-system communication
2. ResourceDatabase — loads and caches .tres Resource assets at startup
3. WorldSim — drives TickEngine.Advance each frame, emits GameTicked
4. GameManager — stub; will own match state (players, victory conditions)
5. InputRouter — translates InputEvents to WorldSim intents (pause, speed)
