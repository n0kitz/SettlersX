using System.IO;
using System.Reflection;
using GdUnit4;
using static GdUnit4.Assertions;

namespace SettlersX.Tests.Architecture;

/// <summary>
/// Regression guard for ADR 0009 (build & supply chain hardening). Confirms the
/// csproj keeps the supply-chain switches that S1 added: NuGet lock file,
/// deterministic builds, the CI-build flag, and a pinned Newtonsoft.Json version.
/// </summary>
[TestSuite]
public class ProjectHardeningTests
{
  private static string ReadCsproj()
  {
    var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
    var projectRoot = assemblyDir;
    for (var i = 0; i < 4; i++)
    {
      projectRoot = Path.GetDirectoryName(projectRoot) ?? projectRoot;
    }
    var csproj = Path.Combine(projectRoot, "SettlersX.csproj");
    if (!File.Exists(csproj))
    {
      // Tests run from various working dirs across CI/local; if we cannot
      // locate the file, surface that explicitly rather than passing silently.
      return string.Empty;
    }
    return File.ReadAllText(csproj);
  }

  [TestCase]
  public void Csproj_DeclaresLockFileFlag()
  {
    var content = ReadCsproj();
    AssertThat(content.Length > 0).IsTrue();
    AssertThat(content.Contains("RestorePackagesWithLockFile")).IsTrue();
  }

  [TestCase]
  public void Csproj_DeclaresDeterministicBuilds()
  {
    var content = ReadCsproj();
    AssertThat(content.Length > 0).IsTrue();
    AssertThat(content.Contains("<Deterministic>true</Deterministic>")).IsTrue();
  }

  [TestCase]
  public void Csproj_PinsNewtonsoftJson_ToExactVersion()
  {
    var content = ReadCsproj();
    AssertThat(content.Length > 0).IsTrue();
    // Newtonsoft.Json is the highest-risk package; require an exact pin.
    AssertThat(content.Contains("Newtonsoft.Json\" Version=\"13.0.3\"")).IsTrue();
  }

  [TestCase]
  public void Csproj_DeclaresContinuousIntegrationBuildFlag()
  {
    var content = ReadCsproj();
    AssertThat(content.Length > 0).IsTrue();
    AssertThat(content.Contains("ContinuousIntegrationBuild")).IsTrue();
  }
}
