// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Globalization;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.EquatableAnalyzer,
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.EquatableFixer>;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class EquatableAnalyzerTests
    {
        [Fact]
        public async Task NoDiagnosticForStructWithNoEqualsOverrideAndNoIEquatableImplementation()
        {
            var code = @"
struct S
{
}
";
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Fact]
        public async Task NoDiagnosticForClassWithNoEqualsOverrideAndNoIEquatableImplementation()
        {
            var code = @"
class C
{
}
";
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Fact]
        public async Task DiagnosticForStructWithEqualsOverrideButNoIEquatableImplementation()
        {
            var code = @"
struct S
{
    public override bool Equals(object other)
    {
        return true;
    }
}
";
            string expectedMessage = string.Format(CultureInfo.CurrentCulture, MicrosoftCodeQualityAnalyzersResources.ImplementIEquatableWhenOverridingObjectEqualsMessage, "S");
            await VerifyCS.VerifyAnalyzerAsync(code,
                GetCSharpResultAt(2, 8, EquatableAnalyzer.ImplementIEquatableDescriptor, expectedMessage));
        }

        [Fact]
        public async Task NoDiagnosticForClassWithEqualsOverrideAndNoIEquatableImplementation()
        {
            var code = @"
class C
{
    public override bool Equals(object other)
    {
        return true;
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Fact]
        public async Task DiagnosticForStructWithIEquatableImplementationButNoEqualsOverride()
        {
            var code = @"
using System;

struct S : IEquatable<S>
{
    public bool Equals(S other)
    {
        return true;
    }
}
";
            string expectedMessage = string.Format(CultureInfo.CurrentCulture, MicrosoftCodeQualityAnalyzersResources.OverrideObjectEqualsMessage, "S");
            await VerifyCS.VerifyAnalyzerAsync(code,
                GetCSharpResultAt(4, 8, EquatableAnalyzer.OverridesObjectEqualsDescriptor, expectedMessage));
        }

        [Fact]
        public async Task DiagnosticForClassWithIEquatableImplementationButNoEqualsOverride()
        {
            var code = @"
using System;

class C : IEquatable<C>
{
    public bool Equals(C other)
    {
        return true;
    }
}
";
            string expectedMessage = string.Format(CultureInfo.CurrentCulture, MicrosoftCodeQualityAnalyzersResources.OverrideObjectEqualsMessage, "C");
            await VerifyCS.VerifyAnalyzerAsync(code,
                GetCSharpResultAt(4, 7, EquatableAnalyzer.OverridesObjectEqualsDescriptor, expectedMessage));
        }

        [Fact]
        public async Task NoDiagnosticForClassWithIEquatableImplementationWithNoParameterListAndNoEqualsOverride()
        {
            var code = @"
using System;

class C : IEquatable<C>
{
    public bool Equals
    {
        return true;
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(code, CompilerDiagnostics.None);
        }

        [Fact]
        public async Task NoDiagnosticForClassWithIEquatableImplementationWithMalformedParameterListAndNoEqualsOverride()
        {
            var code = @"
using System;

class C : IEquatable<C>
{
    public bool Equals(
    {
        return true;
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(code, CompilerDiagnostics.None);
        }

        [Fact]
        public async Task NoDiagnosticForClassWithIEquatableImplementationWithMalformedParameterListAndNoEqualsOverride2()
        {
            var code = @"
using System;

class C : IEquatable<C>
{
    public bool Equals)
    {
        return true;
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(code, CompilerDiagnostics.None);
        }

        [Fact]
        public async Task NoDiagnosticForClassWithIEquatableImplementationWithNoParametersAndNoEqualsOverride()
        {
            var code = @"
using System;

class C : IEquatable<C>
{
    public bool Equals()
    {
        return true;
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(code, CompilerDiagnostics.None);
        }

        [Fact]
        public async Task NoDiagnosticForClassWithIEquatableImplementationWithMalformedParameterDeclarationAndNoEqualsOverride()
        {
            var code = @"
using System;

class C : IEquatable<C>
{
    public bool Equals(x)
    {
        return true;
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(code, CompilerDiagnostics.None);
        }

        [Fact]
        public async Task NoDiagnosticForClassWithIEquatableImplementationWithWrongReturnTypeAndNoEqualsOverride()
        {
            var code = @"
using System;

class C : IEquatable<C>
{
    public int Equals(C x)
    {
        return 1;
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(code, CompilerDiagnostics.None);
        }

        [Fact]
        public async Task DiagnosticForClassWithIEquatableImplementationWithNoBodyAndNoEqualsOverride()
        {
            var code = @"
using System;

class C : IEquatable<C>
{
    public bool Equals(C other)
}
";
            string expectedMessage = string.Format(CultureInfo.CurrentCulture, MicrosoftCodeQualityAnalyzersResources.OverrideObjectEqualsMessage, "C");
            await VerifyCS.VerifyAnalyzerAsync(code, CompilerDiagnostics.None,
                GetCSharpResultAt(4, 7, EquatableAnalyzer.OverridesObjectEqualsDescriptor, expectedMessage));
        }

        [Fact]
        public async Task NoDiagnosticForClassWithIEquatableImplementationWithNoReturnTypeAndNoEqualsOverride()
        {
            var code = @"
using System;

class C : IEquatable<C>
{
    public Equals(C other)
    {
        return true;
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(code, CompilerDiagnostics.None);
        }

        [Fact]
        public async Task NoDiagnosticForClassWithEqualsOverrideWithWrongSignatureAndNoIEquatableImplementation()
        {
            var code = @"
using System;

class C
{
    public override bool Equals(object other, int n)
    {
        return true;
    }
}
";
            await VerifyCS.VerifyAnalyzerAsync(code, CompilerDiagnostics.None);
        }

        [Fact]
        public async Task DiagnosticForClassWithExplicitIEquatableImplementationAndNoEqualsOverride()
        {
            var code = @"
using System;

class C : IEquatable<C>
{
    bool IEquatable<C>.Equals(C other)
    {
        return true;
    }
}
";
            string expectedMessage = string.Format(CultureInfo.CurrentCulture, MicrosoftCodeQualityAnalyzersResources.OverrideObjectEqualsMessage, "C");
            await VerifyCS.VerifyAnalyzerAsync(code,
                GetCSharpResultAt(4, 7, EquatableAnalyzer.OverridesObjectEqualsDescriptor, expectedMessage));
        }

        [Fact]
        public async Task DiagnosticForDerivedStructWithEqualsOverrideAndNoIEquatableImplementation()
        {
            var code = @"
using System;

struct B
{
    public override bool Equals(object other)
    {
        return false;
    }
}

struct C : B
{
    public override bool Equals(object other)
    {
        return true;
    }
}
";
            string expectedMessage1 = string.Format(CultureInfo.CurrentCulture, MicrosoftCodeQualityAnalyzersResources.ImplementIEquatableWhenOverridingObjectEqualsMessage, "B");
            string expectedMessage2 = string.Format(CultureInfo.CurrentCulture, MicrosoftCodeQualityAnalyzersResources.ImplementIEquatableWhenOverridingObjectEqualsMessage, "C");
            await VerifyCS.VerifyAnalyzerAsync(code, CompilerDiagnostics.None,
                GetCSharpResultAt(4, 8, EquatableAnalyzer.ImplementIEquatableDescriptor, expectedMessage1),
                GetCSharpResultAt(12, 8, EquatableAnalyzer.ImplementIEquatableDescriptor, expectedMessage2));
        }

        [Fact, WorkItem(1914, "https://github.com/dotnet/roslyn-analyzers/issues/1914")]
        public async Task NoDiagnosticForParentClassWithIEquatableImplementation()
        {
            var code = @"
using System;

public interface IValueObject<T> : IEquatable<T> { }

public struct S : IValueObject<S>
{
    private readonly int value;

    public override bool Equals(object obj) => obj is S other && Equals(other);

    public bool Equals(S other) => value == other.value;

    public override int GetHashCode() => value;
}";
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Fact, WorkItem(2027, "https://github.com/dotnet/roslyn-analyzers/issues/2027")]
        public async Task NoDiagnosticForDerivedTypesWithBaseTypeWithIEquatableImplementation_01()
        {
            var code = @"
using System;

public class A<T> : IEquatable<T>
    where T : A<T>
{
    public virtual bool Equals(T other) => false;

    public override bool Equals(object obj) => Equals(obj as T);
}

public class B : A<B>
{
}";
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Fact, WorkItem(2027, "https://github.com/dotnet/roslyn-analyzers/issues/2027")]
        public async Task NoDiagnosticForDerivedTypesWithBaseTypeWithIEquatableImplementation_02()
        {
            var code = @"
using System;

public class A<T> : IEquatable<T>
    where T: class
{
    public virtual bool Equals(T other) => false;

    public override bool Equals(object obj) => Equals(obj as T);
}

public class B : A<B>
{
}

public class C<T> : A<T>
    where T : class
{
}

public class D : C<D>
{
}";
            await VerifyCS.VerifyAnalyzerAsync(code);
        }

        [Fact, WorkItem(2324, "https://github.com/dotnet/roslyn-analyzers/issues/2324")]
        public async Task CA1066_CSharp_RefStruct_NoDiagnostic()
        {
            await new VerifyCS.Test
            {
                TestCode = @"
public ref struct S
{
    public override bool Equals(object other)
    {
        return false;
    }
}
",
                LanguageVersion = LanguageVersion.CSharp8
            }.RunAsync();
        }

        private static DiagnosticResult GetCSharpResultAt(int line, int column, DiagnosticDescriptor rule, string message)
            => new DiagnosticResult(rule)
                .WithLocation(line, column)
                .WithMessage(message);
    }
}
