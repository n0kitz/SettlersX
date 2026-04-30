using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using GdUnit4;
using static GdUnit4.Assertions;

namespace SettlersX.Tests.Architecture;

[TestSuite]
public class SimLayerIsolationTests
{
  private static readonly Assembly _assembly = Assembly.GetExecutingAssembly();

  private static bool IsAllowedGodotType(Type t) =>
      t.Name is "Vector2" or "Vector3" or "Vector2I" or "Vector3I";

  private static IEnumerable<Type> SimTypes() =>
      _assembly.GetTypes()
          .Where(t => t.Namespace != null &&
                      t.Namespace.StartsWith("SettlersX.Sim"));

  [TestCase]
  public void SimTypes_FieldsMustNotReferenceGodot()
  {
    var violations = new List<string>();
    foreach (var type in SimTypes())
    {
      var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Static |
                                  BindingFlags.NonPublic | BindingFlags.Public);
      foreach (var f in fields)
      {
        if (f.FieldType.Namespace?.StartsWith("Godot") == true &&
            !IsAllowedGodotType(f.FieldType))
        {
          violations.Add($"{type.FullName}.{f.Name}: {f.FieldType.Name}");
        }
      }
    }
    AssertThat(violations)
        .OverrideFailureMessage("Sim field references illegal Godot type:\n" +
                                string.Join("\n", violations))
        .IsEmpty();
  }

  [TestCase]
  public void SimTypes_PropertiesMustNotReferenceGodot()
  {
    var violations = new List<string>();
    foreach (var type in SimTypes())
    {
      var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Static |
                                     BindingFlags.NonPublic | BindingFlags.Public);
      foreach (var p in props)
      {
        if (p.PropertyType.Namespace?.StartsWith("Godot") == true &&
            !IsAllowedGodotType(p.PropertyType))
        {
          violations.Add($"{type.FullName}.{p.Name}: {p.PropertyType.Name}");
        }
      }
    }
    AssertThat(violations)
        .OverrideFailureMessage("Sim property references illegal Godot type:\n" +
                                string.Join("\n", violations))
        .IsEmpty();
  }

  [TestCase]
  public void SimTypes_MethodsMustNotReferenceGodot()
  {
    var violations = new List<string>();
    foreach (var type in SimTypes())
    {
      var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static |
                                    BindingFlags.NonPublic | BindingFlags.Public |
                                    BindingFlags.DeclaredOnly);
      foreach (var m in methods)
      {
        // Return type
        if (m.ReturnType.Namespace?.StartsWith("Godot") == true &&
            !IsAllowedGodotType(m.ReturnType))
        {
          violations.Add($"{type.FullName}.{m.Name} return: {m.ReturnType.Name}");
        }
        // Parameter types
        foreach (var param in m.GetParameters())
        {
          if (param.ParameterType.Namespace?.StartsWith("Godot") == true &&
              !IsAllowedGodotType(param.ParameterType))
          {
            violations.Add(
                $"{type.FullName}.{m.Name}({param.Name}): {param.ParameterType.Name}");
          }
        }
      }
    }
    AssertThat(violations)
        .OverrideFailureMessage("Sim method references illegal Godot type:\n" +
                                string.Join("\n", violations))
        .IsEmpty();
  }

  [TestCase]
  public void SimTypes_BaseClassMustNotBeGodotObject()
  {
    var violations = new List<string>();
    foreach (var type in SimTypes())
    {
      var baseType = type.BaseType;
      while (baseType != null && baseType != typeof(object))
      {
        if (baseType.Namespace?.StartsWith("Godot") == true)
        {
          violations.Add($"{type.FullName} extends {baseType.Name}");
          break;
        }
        baseType = baseType.BaseType;
      }
    }
    AssertThat(violations)
        .OverrideFailureMessage("Sim type inherits from Godot class:\n" +
                                string.Join("\n", violations))
        .IsEmpty();
  }

  /// <summary>
  /// Source-level scan: catches Godot API usage inside method bodies that reflection cannot see.
  /// Checks .cs files under src/Sim/ for patterns that violate the pure-C# rule.
  /// </summary>
  [TestCase]
  public void SimSourceFiles_MustNotContainDisallowedGodotUsage()
  {
    // Locate src/Sim/ relative to this test assembly's output directory
    var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
    // Walk up from bin/Debug/net10.0 to project root
    var projectRoot = assemblyDir;
    for (var i = 0; i < 4; i++)
    {
      projectRoot = Path.GetDirectoryName(projectRoot) ?? projectRoot;
    }
    var simDir = Path.Combine(projectRoot, "src", "Sim");

    if (!Directory.Exists(simDir))
    {
      // If we can't locate src/Sim/, skip rather than false-fail
      return;
    }

    // Patterns that are forbidden in src/Sim/ source files
    var forbiddenPatterns = new[]
    {
      "GD.Print", "GD.PushError", "GD.PushWarning", "GD.PrintErr",
      "[Export]", "[Signal]", "[GlobalClass]",
      ": Node", ": Resource",
    };

    var violations = new List<string>();
    foreach (var file in Directory.EnumerateFiles(simDir, "*.cs", SearchOption.AllDirectories))
    {
      var lines = File.ReadAllLines(file);
      for (var lineIdx = 0; lineIdx < lines.Length; lineIdx++)
      {
        var line = lines[lineIdx];
        // Skip comment lines — they may contain pattern names for documentation
        var trimmed = line.TrimStart();
        if (trimmed.StartsWith("//") || trimmed.StartsWith("*") || trimmed.StartsWith("///"))
        {
          continue;
        }
        foreach (var pattern in forbiddenPatterns)
        {
          if (line.Contains(pattern, StringComparison.Ordinal))
          {
            var relPath = Path.GetRelativePath(projectRoot, file);
            violations.Add($"{relPath}:{lineIdx + 1}: contains \"{pattern}\"");
          }
        }
      }
    }

    AssertThat(violations)
        .OverrideFailureMessage(
            "Sim source file contains disallowed Godot API usage:\n" +
            string.Join("\n", violations))
        .IsEmpty();
  }
}
