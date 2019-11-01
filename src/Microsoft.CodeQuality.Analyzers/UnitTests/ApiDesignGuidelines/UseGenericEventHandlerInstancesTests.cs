﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeQuality.CSharp.Analyzers.ApiDesignGuidelines;
using Microsoft.CodeQuality.VisualBasic.Analyzers.ApiDesignGuidelines;
using Microsoft.CodeAnalysis.Diagnostics;
using Test.Utilities;
using Xunit;
using VerifyCS = Test.Utilities.CSharpCodeFixVerifier<
    Microsoft.CodeQuality.CSharp.Analyzers.ApiDesignGuidelines.CSharpUseGenericEventHandlerInstancesAnalyzer,
    Microsoft.CodeQuality.CSharp.Analyzers.ApiDesignGuidelines.CSharpUseGenericEventHandlerInstancesFixer>;
using VerifyVB = Test.Utilities.VisualBasicCodeFixVerifier<
    Microsoft.CodeQuality.VisualBasic.Analyzers.ApiDesignGuidelines.BasicUseGenericEventHandlerInstancesAnalyzer,
    Microsoft.CodeQuality.VisualBasic.Analyzers.ApiDesignGuidelines.BasicUseGenericEventHandlerInstancesFixer>;

namespace Microsoft.CodeQuality.Analyzers.ApiDesignGuidelines.UnitTests
{
    public class UseGenericEventHandlerTests : DiagnosticAnalyzerTestBase
    {
        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicUseGenericEventHandlerInstancesAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpUseGenericEventHandlerInstancesAnalyzer();
        }

        [Fact]
        public void TestAlreadyUsingGenericEventHandlerCSharp()
        {
            VerifyCSharp(@"
public class C
{
    public event System.EventHandler<System.EventArgs> E1;
    public event System.EventHandler E2;
}
");
        }

        [Fact]
        public void TestAlreadyUsingGenericEventHandlerBasic()
        {
            VerifyBasic(@"
Public Class C
    Public Event E1 As System.EventHandler(Of System.EventArgs)
    Public Event E2 As System.EventHandler
End Class
");
        }

        [Fact]
        public void TestUsingStructAsEventArgsForOptimizationCSharp()
        {
            VerifyCSharp(@"
public struct SpecialCaseStructEventArgs
{
}

public struct BadArgs
{
}

public class C
{
    public event System.EventHandler<SpecialCaseStructEventArgs> E1;
    public event System.EventHandler<BadArgs> E2;
}
",
            // Test0.cs(13,47): warning CA1003: Change the event 'E2' to replace the type 'System.EventHandler<BadArgs>' with a generic EventHandler, for e.g. EventHandler<T>, where T is a valid EventArgs
            GetCSharpResultAt(13, 47, UseGenericEventHandlerInstancesAnalyzer.RuleForEvents, "E2", "System.EventHandler<BadArgs>"));
        }

        [Fact]
        public void TestUsingStructAsEventArgsForOptimizationBasic()
        {
            VerifyBasic(@"
Public Structure SpecialCaseStructEventArgs
End Structure

Public Structure BadArgs
End Structure

Public Class C
    Public Event E1 As System.EventHandler(Of SpecialCaseStructEventArgs)
    Public Event E2 As System.EventHandler(Of BadArgs)
End Class
",
            // Test0.vb(10,18): warning CA1003: Change the event 'E2' to replace the type 'System.EventHandler(Of BadArgs)' with a generic EventHandler, for e.g. EventHandler<T>, where T is a valid EventArgs
            GetBasicResultAt(10, 18, UseGenericEventHandlerInstancesAnalyzer.RuleForEvents, "E2", "System.EventHandler(Of BadArgs)"));
        }

        [Fact]
        public void TestGeneratedEventHandlersBasic()
        {
            VerifyBasic(@"
Public Class C
    Public Event E1()
    Public Event E2(args As System.EventArgs)
    Public Event E3(sender As Object)
    Public Event E4(sender As Object, args As Integer)
End Class
",
            // Test0.vb(3,18): warning CA1003: Change the event 'E1' to use a generic EventHandler by defining the event type explicitly, for e.g. Event MyEvent As EventHandler(Of MyEventArgs).
            GetBasicResultAt(3, 18, UseGenericEventHandlerInstancesAnalyzer.RuleForEvents2, "E1"),
            // Test0.vb(4,18): warning CA1003: Change the event 'E2' to use a generic EventHandler by defining the event type explicitly, for e.g. Event MyEvent As EventHandler(Of MyEventArgs).
            GetBasicResultAt(4, 18, UseGenericEventHandlerInstancesAnalyzer.RuleForEvents2, "E2"),
            // Test0.vb(5,18): warning CA1003: Change the event 'E3' to use a generic EventHandler by defining the event type explicitly, for e.g. Event MyEvent As EventHandler(Of MyEventArgs).
            GetBasicResultAt(5, 18, UseGenericEventHandlerInstancesAnalyzer.RuleForEvents2, "E3"),
            // Test0.vb(6,18): warning CA1003: Change the event 'E4' to use a generic EventHandler by defining the event type explicitly, for e.g. Event MyEvent As EventHandler(Of MyEventArgs).
            GetBasicResultAt(6, 18, UseGenericEventHandlerInstancesAnalyzer.RuleForEvents2, "E4"));
        }

        [Fact]
        public void TestNonPublicEventAndNonPublicDelegate()
        {
            VerifyCSharp(@"
internal delegate void BadEventHandler(object senderId, System.EventArgs e);

public class EventsClass
{
    internal event BadEventHandler E;
}
");
        }

        [Fact]
        public void TestNonPublicEventButPublicDelegate()
        {
            VerifyCSharp(@"
public delegate void BadEventHandler(object senderId, System.EventArgs e);

public class EventsClass
{
    internal event BadEventHandler E;
}
",
            // Test0.cs(2,22): warning CA1003: Remove 'BadEventHandler' and replace its usage with a generic EventHandler, for e.g. EventHandler<T>, where T is a valid EventArgs
            GetCSharpResultAt(2, 22, UseGenericEventHandlerInstancesAnalyzer.RuleForDelegates, "BadEventHandler"));
        }

        [Fact]
        public void TestNonPublicEventAndPublicInvalidDelegate()
        {
            VerifyCSharp(@"
public delegate void BadEventHandler(object senderId);

public class EventsClass
{
    internal event BadEventHandler E;
}
");
        }

        [Fact]
        public void TestIgnoreEventsThatAreInterfaceImplementations()
        {
            VerifyCSharp(@"
using System;

public delegate void BadEventHandler(object senderId, EventArgs e);

public interface ITest
{
    event BadEventHandler TestEvent;
}

public class EventsClassImplicit : ITest
{
    public event BadEventHandler TestEvent;
}

public class EventsClassExplicit : ITest
{
    event BadEventHandler ITest.TestEvent
    {
        add
        {
        }
        remove
        {
        }
    }
}
",
            // Test0.cs(4,22): warning CA1003: Remove 'BadEventHandler' and replace its usage with a generic EventHandler, for e.g. EventHandler<T>, where T is a valid EventArgs
            GetCSharpResultAt(4, 22, UseGenericEventHandlerInstancesAnalyzer.RuleForDelegates, "BadEventHandler"));
        }

        [Fact]
        public void TestOverrideEvent()
        {
            VerifyCSharp(@"
public delegate void BadHandler();

public class C
{
    public virtual event BadHandler E;
}

public class D : C
{
    public override event BadHandler E;
}
",
            // Test0.cs(6,37): warning CA1003: Change the event 'E' to replace the type 'BadHandler' with a generic EventHandler, for e.g. EventHandler<T>, where T is a valid EventArgs
            GetCSharpResultAt(6, 37, UseGenericEventHandlerInstancesAnalyzer.RuleForEvents, "E", "BadHandler"));
        }

        [Fact]
        public void TestComSourceInterfaceEvent()
        {
            VerifyCSharp(@"
public delegate void BadHandler();

[System.Runtime.InteropServices.ComSourceInterfaces(""C"")]
public class C
{
    public event BadHandler E;
}
");
        }

        [Fact]
        public void TestViolatingEventsCSharp()
        {
            VerifyCSharp(@"
using System;

public delegate void BadHandler1();
public delegate int BadHandler2();
public delegate void BadHandler3(object sender);
public delegate void BadHandler4(EventArgs args);
public delegate void BadHandler5(object sender, EventArgs args);

public class C
{
    public event BadHandler1 E1;
    public event BadHandler2 E2;
    public event BadHandler3 E3;
    public event BadHandler4 E4;
    public event BadHandler5 E5;
    public event EventHandler<int> E6;
}
",
            // Test0.cs(8,22): warning CA1003: Remove 'BadHandler5' and replace its usage with a generic EventHandler, for e.g. EventHandler<T>, where T is a valid EventArgs
            GetCSharpResultAt(8, 22, UseGenericEventHandlerInstancesAnalyzer.RuleForDelegates, "BadHandler5"),
            // Test0.cs(12,30): warning CA1003: Change the event 'E1' to replace the type 'BadHandler1' with a generic EventHandler, for e.g. EventHandler<T>, where T is a valid EventArgs
            GetCSharpResultAt(12, 30, UseGenericEventHandlerInstancesAnalyzer.RuleForEvents, "E1", "BadHandler1"),
            // Test0.cs(13,30): warning CA1003: Change the event 'E2' to replace the type 'BadHandler2' with a generic EventHandler, for e.g. EventHandler<T>, where T is a valid EventArgs
            GetCSharpResultAt(13, 30, UseGenericEventHandlerInstancesAnalyzer.RuleForEvents, "E2", "BadHandler2"),
            // Test0.cs(14,30): warning CA1003: Change the event 'E3' to replace the type 'BadHandler3' with a generic EventHandler, for e.g. EventHandler<T>, where T is a valid EventArgs
            GetCSharpResultAt(14, 30, UseGenericEventHandlerInstancesAnalyzer.RuleForEvents, "E3", "BadHandler3"),
            // Test0.cs(15,30): warning CA1003: Change the event 'E4' to replace the type 'BadHandler4' with a generic EventHandler, for e.g. EventHandler<T>, where T is a valid EventArgs
            GetCSharpResultAt(15, 30, UseGenericEventHandlerInstancesAnalyzer.RuleForEvents, "E4", "BadHandler4"),
            // Test0.cs(17,36): warning CA1003: Change the event 'E6' to replace the type 'System.EventHandler<int>' with a generic EventHandler, for e.g. EventHandler<T>, where T is a valid EventArgs
            GetCSharpResultAt(17, 36, UseGenericEventHandlerInstancesAnalyzer.RuleForEvents, "E6", "System.EventHandler<int>"));
        }

        [Fact]
        public void TestViolatingEventsBasic()
        {
            VerifyBasic(@"
Public Delegate Sub BadHandler(sender As Object, args As System.EventArgs)

Public Class C
    Public Event E1 As BadHandler
    Public Event E2(sender As Object, e As System.EventArgs)
    Public Event E3(sender As Object, e As MyEventArgs)
End Class

Public Structure MyEventArgs
End Structure
",
            // Test0.vb(2,21): warning CA1003: Remove 'BadHandler' and replace its usage with a generic EventHandler, for e.g. EventHandler<T>, where T is a valid EventArgs
            GetBasicResultAt(2, 21, UseGenericEventHandlerInstancesAnalyzer.RuleForDelegates, "BadHandler"),
            // Test0.vb(6,18): warning CA1003: Change the event 'E2' to use a generic EventHandler by defining the event type explicitly, for e.g. Event MyEvent As EventHandler(Of MyEventArgs).
            GetBasicResultAt(6, 18, UseGenericEventHandlerInstancesAnalyzer.RuleForEvents2, "E2"),
            // Test0.vb(7,18): warning CA1003: Change the event 'E3' to use a generic EventHandler by defining the event type explicitly, for e.g. Event MyEvent As EventHandler(Of MyEventArgs).
            GetBasicResultAt(7, 18, UseGenericEventHandlerInstancesAnalyzer.RuleForEvents2, "E3"));
        }
    }
}
