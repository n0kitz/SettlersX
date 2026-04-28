using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GdUnit4;
using static GdUnit4.Assertions;

[TestSuite]
public class SimLayerIsolationTests
{
  [TestCase]
  public void SimAssembly_MustNotReferenceGodot()
  {
    // Load all types from the Sim namespace
    var assembly = Assembly.GetExecutingAssembly();
    var simTypes = assembly.GetTypes()
        .Where(t => t.Namespace != null &&
                    t.Namespace.StartsWith("SettlersX.Sim"))
        .ToList();

    var illegalUsings = new List<string>();

    foreach (var type in simTypes)
    {
      // Check if type references any Godot types beyond math structs
      var refs = type.GetFields(BindingFlags.Instance |
                                BindingFlags.Static |
                                BindingFlags.NonPublic |
                                BindingFlags.Public)
          .Where(f => f.FieldType.Namespace?.StartsWith("Godot") == true)
          .Where(f => f.FieldType.Name is not
              ("Vector2" or "Vector2I" or "Vector3" or "Vector3I" or "Color"))
          .Select(f => $"{type.Name}.{f.Name}: {f.FieldType.Name}");

      illegalUsings.AddRange(refs);
    }

    AssertThat(illegalUsings)
        .OverrideFailureMessage(
            "Sim layer references Godot types beyond math structs:\n" +
            string.Join("\n", illegalUsings))
        .IsEmpty();
  }
}
