# Data Layer

Resource subclasses and loaders. Read-only at runtime.
[GlobalClass] public partial class XyzDef : Resource
Filename must match class name exactly (case-sensitive).
No logic in Resource classes — [Export] properties only.
ResourceDatabase autoload pre-loads all .tres from data/ on startup.
Balance values go in data/balance.json — never in .cs or .tres directly.
