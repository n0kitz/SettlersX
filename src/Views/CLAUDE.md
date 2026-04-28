# Views Layer

Thin Godot Node subclasses only. Contains NO game logic.
Subscribe to Manager signals in _Ready. Unsubscribe in _ExitTree.
Send player intents to Managers via method calls (never to other views).
No direct sim object references — receive data via signal parameters only.
Check IsInstanceValid(this) after every await.
MultiMesh for repeated geometry: settlers, trees, road segments, resources.
CallDeferred for any scene tree modification from callbacks.
