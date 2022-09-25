using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using System.Reflection;

namespace Bicep.RoslynAnalyzers.UnitTests
{
    [TestClass]
    public class LinterRuleGeneratorTests
    {
        [TestMethod]
        public void GeneratorShouldNotFailInInertCompilation()
        {
            var compilation = CreateCompilation(@"
namespace MyCode
{
    public class Program
    {
        public static void Main(string[] args)
        {
        }
    }
}
");
            compilation.GetDiagnostics().Should().BeEmpty();
            var (updatedCompilation, generatorDiagnostics) = RunGeneratorTest(compilation);

            generatorDiagnostics.Should().BeEmpty();
            updatedCompilation.GetDiagnostics().Should().BeEmpty();
        }

        [TestMethod]
        public void GeneratorShouldGenerateEmptyList()
        {
            var compilation = CreateCompilation(@"
using Bicep.RoslynAnalyzers;
using System;
using System.Collections.Generic;

namespace MyCode
{
    public partial class Program
    {
        public static void Main(string[] args)
        {
        }

        [LinterRuleTypesGenerator]
        public partial IEnumerable<Type> GetMyTypes();
    }
}
");
            // we are referencing an attribute that hasn't been generated yet, so there will be errors
            compilation.GetDiagnostics().Should().NotBeEmpty();

            var (updatedCompilation, generatorDiagnostics) = RunGeneratorTest(compilation);

            generatorDiagnostics.Should().BeEmpty();
            updatedCompilation.GetDiagnostics().Should().BeEmpty();

            var generatedSyntaxTree = updatedCompilation.SyntaxTrees.First(syntaxTree => syntaxTree.FilePath.EndsWith("interRuleTypeGenerator.Program.GetMyTypes.g.cs"));

            var expectedSyntaxTree = CSharpSyntaxTree.ParseText(@"// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace MyCode
{
    public partial class Program
    {
        public partial IEnumerable<Type> GetMyTypes()
        {
            return new Type[]
            {
            };
        }
    }
}
");

            generatedSyntaxTree.IsEquivalentTo(expectedSyntaxTree).Should().BeTrue("because generated code is:\n" + generatedSyntaxTree.ToString());
        }

        [TestMethod]
        public void GeneratorShouldProduceValidList()
        {
            var compilation = CreateCompilation(@"
using Bicep.RoslynAnalyzers;
using Bicep.Core.Analyzers.Interfaces;
using System;
using System.Collections.Generic;

namespace MyCode
{
    public partial class Program
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("""","""")]
        public static void Main(string[] args)
        {
        }

        [LinterRuleTypesGenerator]
        public partial IEnumerable<Type> GetMyTypes();
    }

    public class NotDerivedFromInterface : IDisposable
    {
        public void Dispose()
        {
        }
    }

    public class DerivedFromInterfaceInProgram : IBicepAnalyzerRule
    {
    }

    public abstract class AbstractClassInProgram : IBicepAnalyzerRule
    {
    }
}
", @"
namespace Bicep.Core.Analyzers.Interfaces
{
    public interface IBicepAnalyzerRule
    {
    }
}
", @"
using Bicep.Core.Analyzers.Interfaces;

namespace MyOtherCode
{
    public abstract class BaseClass : IBicepAnalyzerRule
    {
    }

    public class DerivedFromBase : BaseClass
    {
    }

    public partial class DuplicatedDerivedFromBase : BaseClass
    {
    }

    public partial class DuplicatedDerivedFromBase : BaseClass
    {
    }
}
");
            // we are referencing an attribute that hasn't been generated yet, so there will be errors
            compilation.GetDiagnostics().Should().NotBeEmpty();

            var (updatedCompilation, generatorDiagnostics) = RunGeneratorTest(compilation);

            generatorDiagnostics.Should().BeEmpty();
            updatedCompilation.GetDiagnostics().Should().BeEmpty();

            var generatedSyntaxTree = updatedCompilation.SyntaxTrees.First(syntaxTree => syntaxTree.FilePath.EndsWith("interRuleTypeGenerator.Program.GetMyTypes.g.cs"));

            var expectedSyntaxTree = CSharpSyntaxTree.ParseText(@"// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace MyCode
{
    public partial class Program
    {
        public partial IEnumerable<Type> GetMyTypes()
        {
            return new Type[]
            {
                typeof(MyCode.DerivedFromInterfaceInProgram),
                typeof(MyOtherCode.DerivedFromBase),
                typeof(MyOtherCode.DuplicatedDerivedFromBase),
            };
        }
    }
}
");

            generatedSyntaxTree.IsEquivalentTo(expectedSyntaxTree).Should().BeTrue("because generated code is:\n" + generatedSyntaxTree.ToString());
        }

        private static (Compilation, ImmutableArray<Diagnostic>) RunGeneratorTest(Compilation compilation)
        {
            var generator = new LinterRuleTypeGenerator();
            var driver = CSharpGeneratorDriver.Create(generator);

            _ = (CSharpGeneratorDriver)driver.RunGeneratorsAndUpdateCompilation(compilation, out var updatedCompilation, out var generatorDiagnostics);

            return (updatedCompilation, generatorDiagnostics);
        }

        private static Compilation CreateCompilation(params string[] sources)
            => CSharpCompilation.Create("compilation",
                sources.Select(source=> CSharpSyntaxTree.ParseText(source)),
                new[] { MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location) },
                new CSharpCompilationOptions(OutputKind.ConsoleApplication));
    }
}