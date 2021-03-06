// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.IdentifiersShouldNotContainTypeNames,
    Microsoft.CodeAnalysis.Testing.EmptyCodeFixProvider>;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class IdentifiersShouldNotContainTypeNamesTests
    {
        [Fact]
        public async Task CSharp_CA1720_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class IntA
{
}
");
        }

        [Fact]
        public async Task CSharp_CA1720_SomeDiagnostic1()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class Int
{
}
",
    GetCA1720CSharpResultAt(line: 2, column: 14, identifierName: "Int"));
        }

        [Fact, WorkItem(1432, "https://github.com/dotnet/roslyn-analyzers/issues/1432")]
        public async Task CSharp_CA1720_Internal_NoDiagnostic()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
class Int
{
}

public class C
{
    private class Int
    {
    }
}

internal class C2
{
    public class Int
    {
    }
}
");
        }

        [Fact]
        public async Task CSharp_CA1720_SomeDiagnostic2()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public struct Int32
{
}
",
    GetCA1720CSharpResultAt(line: 2, column: 15, identifierName: "Int32"));
        }

        [Fact]
        public async Task CSharp_CA1720_SomeDiagnostic3()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public enum Int64
{
}
",
    GetCA1720CSharpResultAt(line: 2, column: 13, identifierName: "Int64"));
        }

        [Fact]
        public async Task CSharp_CA1720_SomeDiagnostic4()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class Derived
{
   public void Int()
   {
   }
}
",
    GetCA1720CSharpResultAt(line: 4, column: 16, identifierName: "Int"));
        }

        [Fact]
        public async Task CSharp_CA1720_SomeDiagnostic5()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class Bar
{
   public void BarMethod(int Int)
   {
   }
}
",
    GetCA1720CSharpResultAt(line: 4, column: 30, identifierName: "Int"));
        }

        [Fact]
        public async Task CSharp_CA1720_SomeDiagnostic6()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class DerivedBar
{
   public int Int;
}
",
    GetCA1720CSharpResultAt(line: 4, column: 15, identifierName: "Int"));
        }

        [Fact]
        public async Task CSharp_CA1720_NoDiagnosticOnEqualsOverride()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class Bar
{
   public override bool Equals(object obj)
   {
        throw new System.NotImplementedException();
   }
}
");
        }

        [Fact]
        public async Task CSharp_CA1720_DiagnosticOnAbstractBaseNotImplementation()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public abstract class Base
{
    public abstract void BaseMethod(object okay, object obj);
    public abstract int this[Guid guid] { get; }
}

public class Derived : Base
{
    public override void BaseMethod(object okay, object obj)
    {
    }

    public override int this[Guid guid]
    {
        get { return 0; }
    }
}",
    GetCA1720CSharpResultAt(line: 6, column: 57, identifierName: "obj"),
    GetCA1720CSharpResultAt(line: 7, column: 35, identifierName: "guid"));
        }

        [Fact]
        public async Task CSharp_CA1720_DiagnosticOnBaseNotImplementation()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public class Base
{
    public virtual void BaseMethod(object okay, object obj) 
    { 
    }

    public virtual int this[Guid guid]
    { 
        get { return 0; }
    }
}

public class Derived : Base
{
    public override void BaseMethod(object okay, object obj) 
    { 
    }

    public override int this[Guid guid]
    {
        get { return 1; }
    }
}",
    GetCA1720CSharpResultAt(line: 6, column: 56, identifierName: "obj"),
    GetCA1720CSharpResultAt(line: 10, column: 34, identifierName: "guid"));
        }

        [Fact]
        public async Task CSharp_CA1720_DiagnosticOnBaseNotNestedImplementation()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
public class Base
{
    public virtual void BaseMethod(object okay, object obj)
    {
    }
}

public class Derived : Base
{
}

public class Bar : Derived
{
    public override void BaseMethod(object okay, object obj)
    {
    }
}",
    GetCA1720CSharpResultAt(line: 4, column: 56, identifierName: "obj"));
        }

        [Fact]
        public async Task CSharp_CA1720_DiagnosticOnInterfaceNotImplicitImplementation()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public interface IDerived
{
    void DerivedMethod(object okay, object obj);
}

public class Derived : IDerived
{
    public void DerivedMethod(object okay, object obj) 
    {
    }
}",
    GetCA1720CSharpResultAt(line: 6, column: 44, identifierName: "obj"));
        }

        [Fact]
        public async Task CSharp_CA1720_DiagnosticOnInterfaceNotExplicitImplementation()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public interface IDerived
{
    void DerivedMethod(object okay, object obj);
}

public class Derived : IDerived
{
    void IDerived.DerivedMethod(object okay, object obj) 
    {
    }
}",
    GetCA1720CSharpResultAt(line: 6, column: 44, identifierName: "obj"));
        }

        [Fact]
        public async Task CSharp_CA1720_DiagnosticOnGenericInterfaceNotImplicitImplementation()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public interface IDerived<in T1, in T2>
{
    void DerivedMethod(int okay, T1 obj, T2 @int);
}

public class Derived : IDerived<int, string>
{
    public void DerivedMethod(int okay, int obj, string @int)
    {
    }
}",
    GetCA1720CSharpResultAt(line: 6, column: 37, identifierName: "obj"),
    GetCA1720CSharpResultAt(line: 6, column: 45, identifierName: "int"));
        }

        [Fact]
        public async Task CSharp_CA1720_DiagnosticOnGenericInterfaceNotExplicitImplementation()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public interface IDerived<in T1, in T2>
{
    void DerivedMethod(int okay, T1 obj, T2 @int);
}

public class Derived : IDerived<int, string>
{
    void IDerived<int, string>.DerivedMethod(int okay, int obj, string @int)
    {
    }
}",
    GetCA1720CSharpResultAt(line: 6, column: 37, identifierName: "obj"),
    GetCA1720CSharpResultAt(line: 6, column: 45, identifierName: "int"));
        }

        [Fact]
        public async Task CSharp_CA1720_DiagnosticOnInterfaceNotNestedImplicitImplementation()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public interface IDerived
{
    void DerivedMethod(object okay, object obj);
}

public interface IBar : IDerived
{
}

public class Derived : IBar
{
    public void DerivedMethod(object okay, object obj) 
    {
    }
}",
    GetCA1720CSharpResultAt(line: 6, column: 44, identifierName: "obj"));
        }

        [Fact]
        public async Task CSharp_CA1720_DiagnosticOnInterfaceNotNestedExplicitImplementation()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public interface IDerived
{
    void DerivedMethod(object okay, object obj);
}

public interface IBar : IDerived
{
}

public class Derived : IBar
{
    void IDerived.DerivedMethod(object okay, object obj) 
    {
    }
}",
    GetCA1720CSharpResultAt(line: 6, column: 44, identifierName: "obj"));
        }

        [Fact]
        public async Task CSharp_CA1720_DiagnosticOnGenericInterfaceNotNestedImplicitImplementation()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public interface IDerived<in T1, in T2>
{
    void DerivedMethod(int okay, T1 obj, T2 @int);
}

public interface IBar<in T1, in T2> : IDerived<T1, T2>
{
}

public class Derived : IBar<int, string>
{
    public void DerivedMethod(int okay, int obj, string @int)
    {
    }
}",
    GetCA1720CSharpResultAt(line: 6, column: 37, identifierName: "obj"),
    GetCA1720CSharpResultAt(line: 6, column: 45, identifierName: "int"));
        }

        [Fact]
        public async Task CSharp_CA1720_DiagnosticOnGenericInterfaceNotNestedExplicitImplementation()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;

public interface IDerived<in T1, in T2>
{
    void DerivedMethod(int okay, T1 obj, T2 @int);
}

public interface IBar<in T1, in T2> : IDerived<T1, T2>
{
}

public class Derived : IBar<int, string>
{
    void IDerived<int, string>.DerivedMethod(int okay, int obj, string @int)
    {
    }
}",
    GetCA1720CSharpResultAt(line: 6, column: 37, identifierName: "obj"),
    GetCA1720CSharpResultAt(line: 6, column: 45, identifierName: "int"));
        }

        [Fact]
        public async Task CSharp_CA1720_NoDiagnosticOnIEqualityComparerGetHashCodeImplicitImplementation()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Collections.Generic;

public sealed class SomeEqualityComparer : IEqualityComparer<string>, IEqualityComparer<int>
{
    public bool Equals(string x, string y) { throw new NotImplementedException(); }

    public bool Equals(int x, int y) { throw new NotImplementedException(); }

    public int GetHashCode(string obj)
    {
        throw new NotImplementedException();
    }

    public int GetHashCode(int obj)
    {
        throw new NotImplementedException();
    }
}
");
        }

        [Fact]
        public async Task CSharp_CA1720_NoDiagnosticOnIEqualityComparerGetHashCodeExplicitImplementation()
        {
            await VerifyCS.VerifyAnalyzerAsync(@"
using System;
using System.Collections.Generic;

public sealed class SomeEqualityComparer : IEqualityComparer<string>, IEqualityComparer<int>
{
    public bool Equals(string x, string y) { throw new NotImplementedException(); }

    public bool Equals(int x, int y) { throw new NotImplementedException(); }

    int IEqualityComparer<string>.GetHashCode(string obj)
    {
        throw new NotImplementedException();
    }

    int IEqualityComparer<int>.GetHashCode(int obj)
    {
        throw new NotImplementedException();
    }
}
");
        }

        #region Helpers

        private static DiagnosticResult GetCA1720CSharpResultAt(int line, int column, string identifierName)
            => VerifyCS.Diagnostic()
                .WithLocation(line, column)
                .WithArguments(identifierName);

        #endregion
    }
}