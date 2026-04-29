using System;
using System.Collections.Generic;
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
}
