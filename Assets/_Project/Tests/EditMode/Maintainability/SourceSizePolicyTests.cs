using System;
using System.Linq;
using NUnit.Framework;

namespace SeaLion.Tests.EditMode.Maintainability
{
    public sealed class SourceSizePolicyTests
    {
        private const int PreferredLimit = 500;
        private const int ChangeLimit = 1000;

        [TestCase("Assets/_Project/Scripts/Core/Battle/BattleSession.cs", "authored")]
        [TestCase("specs/001-vertical-slice/plan.cs", "authored")]
        [TestCase("Packages/com.unity.test-framework/Editor/Test.cs", "generated")]
        [TestCase("Library/PackageCache/com.example/Generated.cs", "generated")]
        [TestCase("Assets/Plugins/Telemetry/Telemetry.cs", "vendor/generated")]
        [TestCase("Assets/ThirdParty/Example/Example.cs", "vendor/generated")]
        [TestCase("Assets/Vendor/Example/Example.cs", "vendor/generated")]
        [TestCase("Assets/_Project/Generated/Generated.cs", "vendor/generated")]
        [TestCase("Assets/_Project/Scripts/Generated/Generated.cs", "vendor/generated")]
        [TestCase("Assets/_Project/Scripts/Save.g.cs", "vendor/generated")]
        [TestCase("Assets/_Project/Scripts/Save.generated.cs", "vendor/generated")]
        public void CategorizationMatchesCheckerPolicy(string path, string expected)
        {
            Assert.That(Categorize(path), Is.EqualTo(expected));
        }

        [Test]
        public void GeneratedAndVendorFilesAreExcludedFromAuthoredChecks()
        {
            var paths = new[]
            {
                "Packages/com.unity.foo/Runtime/Generated.cs",
                "Assets/Plugins/Foo/Foo.cs",
                "Assets/ThirdParty/Foo/Foo.cs",
                "Assets/Vendor/Foo/Foo.cs",
                "Assets/_Project/Generated/Foo.cs",
                "Assets/_Project/Foo.generated.cs",
                "Assets/_Project/Foo.g.cs"
            };

            Assert.That(paths.Select(Categorize), Has.All.Not.EqualTo("authored"));
        }

        [Test]
        public void PhysicalLineCountingIncludesWhitespaceOnlyLines()
        {
            var source = "class Example\n\n    \t\n{\n}\n";

            Assert.That(CountPhysicalLines(source), Is.EqualTo(5));
        }

        [TestCase(500, "pass")]
        [TestCase(501, "warning")]
        [TestCase(999, "warning")]
        [TestCase(1000, "warning")]
        [TestCase(1001, "failure")]
        [TestCase(1499, "failure")]
        [TestCase(1500, "failure")]
        [TestCase(1501, "failure")]
        [TestCase(1750, "failure")]
        public void ThresholdResultsUsePhysicalLineCount(int sourceLines, string expected)
        {
            Assert.That(ResultFor(sourceLines), Is.EqualTo(expected));
        }

        private static string Categorize(string path)
        {
            if (path.StartsWith("Packages/", StringComparison.Ordinal) ||
                path.StartsWith("Library/", StringComparison.Ordinal) ||
                path.StartsWith("Temp/", StringComparison.Ordinal) ||
                path.StartsWith("Obj/", StringComparison.Ordinal) ||
                path.StartsWith("Build/", StringComparison.Ordinal) ||
                path.StartsWith("Builds/", StringComparison.Ordinal))
            {
                return "generated";
            }

            if (path.StartsWith("Assets/Plugins/", StringComparison.Ordinal) ||
                path.StartsWith("Assets/ThirdParty/", StringComparison.Ordinal) ||
                path.StartsWith("Assets/Vendor/", StringComparison.Ordinal) ||
                path.Contains("/Generated/", StringComparison.Ordinal) ||
                path.EndsWith(".g.cs", StringComparison.Ordinal) ||
                path.EndsWith(".generated.cs", StringComparison.Ordinal))
            {
                return "vendor/generated";
            }

            return "authored";
        }

        private static int CountPhysicalLines(string source)
        {
            if (source.Length == 0) return 0;
            var normalized = source.Replace("\r\n", "\n").Replace('\r', '\n');
            var count = normalized.Count(character => character == '\n');
            return normalized.EndsWith("\n", StringComparison.Ordinal) ? count : count + 1;
        }

        private static string ResultFor(int sourceLines)
        {
            if (sourceLines > ChangeLimit)
            {
                return "failure";
            }

            return sourceLines > PreferredLimit ? "warning" : "pass";
        }
    }
}
