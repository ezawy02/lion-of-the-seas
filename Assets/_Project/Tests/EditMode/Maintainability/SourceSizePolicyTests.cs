using System;
using System.Linq;
using NUnit.Framework;

namespace SeaLion.Tests.EditMode.Maintainability
{
    public sealed class SourceSizePolicyTests
    {
        private const int WarningLimit = 1000;
        private const int FailureLimit = 1500;

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
        public void NonBlankLineCountingIgnoresWhitespaceOnlyLines()
        {
            var source = "class Example\n\n    \t\n{\n}\n";

            Assert.That(CountNonBlankLines(source), Is.EqualTo(3));
        }

        [TestCase(999, "pass")]
        [TestCase(1000, "warning")]
        [TestCase(1499, "warning")]
        [TestCase(1500, "failure")]
        [TestCase(1750, "failure")]
        public void ThresholdResultsUseNonBlankLineCount(int nonBlankLines, string expected)
        {
            Assert.That(ResultFor(nonBlankLines), Is.EqualTo(expected));
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

        private static int CountNonBlankLines(string source)
        {
            return source.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None)
                .Count(line => line.Trim().Length > 0);
        }

        private static string ResultFor(int nonBlankLines)
        {
            if (nonBlankLines >= FailureLimit)
            {
                return "failure";
            }

            return nonBlankLines >= WarningLimit ? "warning" : "pass";
        }
    }
}
