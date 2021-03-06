﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.UnitTests;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Desktop.Analyzers.UnitTests
{
    public partial class DoNotUseInsecureDTDProcessingAnalyzerTests : DiagnosticAnalyzerTestBase
    {
        private const string CA3075RuleId = DoNotUseInsecureDTDProcessingAnalyzer<SyntaxKind>.RuleId;

        private readonly string _CA3075LoadXmlMessage = DesktopAnalyzersResources.DoNotUseDtdProcessingOverloadsMessage;

        protected override DiagnosticAnalyzer GetBasicDiagnosticAnalyzer()
        {
            return new BasicDoNotUseInsecureDTDProcessingAnalyzer();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CSharpDoNotUseInsecureDTDProcessingAnalyzer();
        }

        private DiagnosticResult GetCA3075LoadXmlCSharpResultAt(int line, int column, string name)
        {
            return GetCSharpResultAt(line, column, CA3075RuleId, string.Format(_CA3075LoadXmlMessage, name, "LoadXml"));
        }

        private DiagnosticResult GetCA3075LoadXmlBasicResultAt(int line, int column, string name)
        {
            return GetBasicResultAt(line, column, CA3075RuleId, string.Format(_CA3075LoadXmlMessage, name, "LoadXml"));
        }

        [Fact]
        public void UseXmlDocumentLoadXmlShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    public class DoNotUseLoadXml
    {
        public void TestMethod(string xml)
        {
            XmlDocument doc = new XmlDocument(){ XmlResolver = null };
            doc.LoadXml(xml);
        }
    }
}",
                GetCA3075LoadXmlCSharpResultAt(11, 13, "TestMethod")
            );

            VerifyBasic(@"
Imports System
Imports System.Xml

Module TestClass
    Sub TestMethod(xml as String)
        Dim doc As XmlDocument = New XmlDocument() With { _
            .XmlResolver = Nothing _
        }
        Call doc.LoadXml(xml)
    End Sub
End Module",
                GetCA3075LoadXmlBasicResultAt(10, 14, "TestMethod")
            );
        }

        [Fact]
        public void UseXmlDocumentLoadXmlInGetShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

class TestClass
{
    public XmlDocument Test
    {
        get {
            var xml = """";
            XmlDocument doc = new XmlDocument() { XmlResolver = null };
            doc.LoadXml(xml);
            return doc;
        }
    }
}",
                GetCA3075LoadXmlCSharpResultAt(11, 13, "get_Test")
            );

            VerifyBasic(@"
Imports System.Xml

Class TestClass
    Public ReadOnly Property Test() As XmlDocument
        Get
            Dim xml = """"
            Dim doc As New XmlDocument() With { _
                .XmlResolver = Nothing _
            }
            Call doc.LoadXml(xml)
            Return doc
        End Get
    End Property
End Class",
                GetCA3075LoadXmlBasicResultAt(11, 18, "get_Test")
            );
        }

        [Fact]
        public void UseXmlDocumentLoadXmlInSetShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

class TestClass
{
    XmlDocument privateDoc;
    public XmlDocument GetDoc
    {
        set
        {
            if (value == null)
            {
                var xml = """";
                XmlDocument doc = new XmlDocument() { XmlResolver = null };
                doc.LoadXml(xml);
                privateDoc = doc;
            }
            else
                privateDoc = value;
        }
    }
}",
                GetCA3075LoadXmlCSharpResultAt(15, 17, "set_GetDoc")
            );

            VerifyBasic(@"
Imports System.Xml

Class TestClass
    Private privateDoc As XmlDocument
    Public WriteOnly Property GetDoc() As XmlDocument
        Set
            If value Is Nothing Then
                Dim xml = """"
                Dim doc As New XmlDocument() With { _
                    .XmlResolver = Nothing _
                }
                doc.LoadXml(xml)
                privateDoc = doc
            Else
                privateDoc = value
            End If
        End Set
    End Property
End Class",
                GetCA3075LoadXmlBasicResultAt(13, 17, "set_GetDoc")
            );
        }

        [Fact]
        public void UseXmlDocumentLoadXmlInTryBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

class TestClass
{
    private void TestMethod()
    {
        try
        {
            var xml = """";
            XmlDocument doc = new XmlDocument() { XmlResolver = null };
            doc.LoadXml(xml);
        }
        catch (Exception) { throw; }
        finally { }
    }
}",
                GetCA3075LoadXmlCSharpResultAt(13, 13, "TestMethod")
            );

            VerifyBasic(@"
Imports System.Xml

Class TestClass
    Private Sub TestMethod()
        Try
            Dim xml = """"
            Dim doc As New XmlDocument() With { _
                .XmlResolver = Nothing _
            }
            doc.LoadXml(xml)
        Catch generatedExceptionName As Exception
            Throw
        Finally
        End Try
    End Sub
End Class",
                GetCA3075LoadXmlBasicResultAt(11, 13, "TestMethod")
            );
        }

        [Fact]
        public void UseXmlDocumentLoadXmlInCatchBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

class TestClass
{
    private void TestMethod()
    {
        try { }
        catch (Exception)
        {
            var xml = """";
            XmlDocument doc = new XmlDocument() { XmlResolver = null };
            doc.LoadXml(xml);
        }
        finally { }
    }
}",
                GetCA3075LoadXmlCSharpResultAt(14, 13, "TestMethod")
            );

            VerifyBasic(@"
Imports System.Xml

Class TestClass
    Private Sub TestMethod()
        Try
        Catch generatedExceptionName As Exception
            Dim xml = """"
            Dim doc As New XmlDocument() With { _
                .XmlResolver = Nothing _
            }
            doc.LoadXml(xml)
        Finally
        End Try
    End Sub
End Class",
                GetCA3075LoadXmlBasicResultAt(12, 13, "TestMethod")
            );
        }

        [Fact]
        public void UseXmlDocumentLoadXmlInFinallyBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

class TestClass
{
    private void TestMethod()
    {
        try { }
        catch (Exception) { throw; }
        finally
        {
            var xml = """";
            XmlDocument doc = new XmlDocument() { XmlResolver = null };
            doc.LoadXml(xml);
        }
    }
}",
                GetCA3075LoadXmlCSharpResultAt(15, 13, "TestMethod")
            );

            VerifyBasic(@"
Imports System.Xml

Class TestClass
    Private Sub TestMethod()
        Try
        Catch generatedExceptionName As Exception
            Throw
        Finally
            Dim xml = """"
            Dim doc As New XmlDocument() With { _
                .XmlResolver = Nothing _
            }
            doc.LoadXml(xml)
        End Try
    End Sub
End Class",
                GetCA3075LoadXmlBasicResultAt(14, 13, "TestMethod")
            );
        }

        [Fact]
        public void UseXmlDocumentLoadXmlInAsyncAwaitShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Threading.Tasks;
using System.Xml;

class TestClass
{
    private async Task TestMethod()
    {
        await Task.Run(() => {
            var xml = """";
            XmlDocument doc = new XmlDocument() { XmlResolver = null };
            doc.LoadXml(xml);
        });
    }

    private async void TestMethod2()
    {
        await TestMethod();
    }
}",
                GetCA3075LoadXmlCSharpResultAt(12, 13, "Run")
            );

            VerifyBasic(@"
Imports System.Threading.Tasks
Imports System.Xml

Class TestClass
    Private Function TestMethod() As Task
        Await Task.Run(Function() 
        Dim xml = """"
        Dim doc As New XmlDocument() With { _
            .XmlResolver = Nothing _
        }
        doc.LoadXml(xml)

End Function)
    End Function

    Private Sub TestMethod2()
        Await TestMethod()
    End Sub
End Class",
                GetCA3075LoadXmlBasicResultAt(12, 9, "Run")
            );
        }

        [Fact]
        public void UseXmlDataDocumentLoadXmlShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

namespace TestNamespace
{
    public class DoNotUseLoadXml
    {
        public void TestMethod1(string xml)
        {
            XmlDataDocument doc = new XmlDataDocument(){ XmlResolver = null };
            doc.LoadXml(xml);
        }
    }
}",
                GetCA3075LoadXmlCSharpResultAt(11, 13, "TestMethod1")
            );

            VerifyBasic(@"
Imports System.Xml

Namespace TestNamespace
    Public Class DoNotUseLoadXml
        Public Sub TestMethod1(xml As String)
            Dim doc As New XmlDataDocument() With { _
                .XmlResolver = Nothing _
            }
            doc.LoadXml(xml)
        End Sub
    End Class
End Namespace",
                GetCA3075LoadXmlBasicResultAt(10, 13, "TestMethod1")
            );
        }

        [Fact]
        public void UseXmlDataDocumentLoadXmlInSetShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Xml;

class TestClass
{
    XmlDataDocument privateDoc;
    public XmlDataDocument GetDoc
    {
        set
        {
            if (value == null)
            {
                var xml = """";
                XmlDataDocument doc = new XmlDataDocument() { XmlResolver = null };
                doc.LoadXml(xml);
                privateDoc = doc;
            }
            else
                privateDoc = value;
        }
    }
}",
                GetCA3075LoadXmlCSharpResultAt(15, 17, "set_GetDoc")
            );

            VerifyBasic(@"
Imports System.Xml

Class TestClass
    Private privateDoc As XmlDataDocument
    Public WriteOnly Property GetDoc() As XmlDataDocument
        Set
            If value Is Nothing Then
                Dim xml = """"
                Dim doc As New XmlDataDocument() With { _
                    .XmlResolver = Nothing _
                }
                doc.LoadXml(xml)
                privateDoc = doc
            Else
                privateDoc = value
            End If
        End Set
    End Property
End Class",
                GetCA3075LoadXmlBasicResultAt(13, 17, "set_GetDoc")
            );
        }

        [Fact]
        public void UseXmlDataDocumentLoadXmlInTryBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

class TestClass
{
    private void TestMethod()
    {
        try
        {
            var xml = """";
            XmlDataDocument doc = new XmlDataDocument() { XmlResolver = null };
            doc.LoadXml(xml);
        }
        catch (Exception) { throw; }
        finally { }
    }
}",
                GetCA3075LoadXmlCSharpResultAt(13, 13, "TestMethod")
            );

            VerifyBasic(@"
Imports System.Xml

Class TestClass
    Private Sub TestMethod()
        Try
            Dim xml = """"
            Dim doc As New XmlDataDocument() With { _
                .XmlResolver = Nothing _
            }
            doc.LoadXml(xml)
        Catch generatedExceptionName As Exception
            Throw
        Finally
        End Try
    End Sub
End Class",
                GetCA3075LoadXmlBasicResultAt(11, 13, "TestMethod")
            );
        }

        [Fact]
        public void UseXmlDataDocumentLoadXmlInCatchBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

class TestClass
{
    private void TestMethod()
    {
        try { }
        catch (Exception)
        {
            var xml = """";
            XmlDataDocument doc = new XmlDataDocument() { XmlResolver = null };
            doc.LoadXml(xml);
        }
        finally { }
    }
}",
                GetCA3075LoadXmlCSharpResultAt(14, 13, "TestMethod")
            );

            VerifyBasic(@"
Imports System.Xml

Class TestClass
    Private Sub TestMethod()
        Try
        Catch generatedExceptionName As Exception
            Dim xml = """"
            Dim doc As New XmlDataDocument() With { _
                .XmlResolver = Nothing _
            }
            doc.LoadXml(xml)
        Finally
        End Try
    End Sub
End Class",
                GetCA3075LoadXmlBasicResultAt(12, 13, "TestMethod")
            );
        }

        [Fact]
        public void UseXmlDataDocumentLoadXmlInFinallyBlockShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System;
using System.Xml;

class TestClass
{
    private void TestMethod()
    {
        try { }
        catch (Exception) { throw; }
        finally
        {
            var xml = """";
            XmlDataDocument doc = new XmlDataDocument() { XmlResolver = null };
            doc.LoadXml(xml);
        }
    }
}",
                GetCA3075LoadXmlCSharpResultAt(15, 13, "TestMethod")
            );

            VerifyBasic(@"
Imports System.Xml

Class TestClass
    Private Sub TestMethod()
        Try
        Catch generatedExceptionName As Exception
            Throw
        Finally
            Dim xml = """"
            Dim doc As New XmlDataDocument() With { _
                .XmlResolver = Nothing _
            }
            doc.LoadXml(xml)
        End Try
    End Sub
End Class",
                GetCA3075LoadXmlBasicResultAt(14, 13, "TestMethod")
            );
        }

        [Fact]
        public void UseXmlDataDocumentLoadXmlInAsyncAwaitShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Threading.Tasks;
using System.Xml;

class TestClass
{
    private async Task TestMethod()
    {
        await Task.Run(() => {
            var xml = """";
            XmlDataDocument doc = new XmlDataDocument() { XmlResolver = null };
            doc.LoadXml(xml);
        });
    }

    private async void TestMethod2()
    {
        await TestMethod();
    }
}",
                GetCA3075LoadXmlCSharpResultAt(12, 13, "Run")
            );

            VerifyBasic(@"
Imports System.Threading.Tasks
Imports System.Xml

Class TestClass
    Private Function TestMethod() As Task
        Await Task.Run(Function() 
        Dim xml = """"
        Dim doc As New XmlDataDocument() With { _
            .XmlResolver = Nothing _
        }
        doc.LoadXml(xml)

End Function)
    End Function

    Private Sub TestMethod2()
        Await TestMethod()
    End Sub
End Class",
                GetCA3075LoadXmlBasicResultAt(12, 9, "Run")
            );
        }

        [Fact]
        public void UseXmlDataDocumentLoadXmlInDelegateShouldGenerateDiagnostic()
        {
            VerifyCSharp(@"
using System.Threading.Tasks;
using System.Xml;

class TestClass
{
    private async Task TestMethod()
    {
        await Task.Run(() => {
            var xml = """";
            XmlDataDocument doc = new XmlDataDocument() { XmlResolver = null };
            doc.LoadXml(xml);
        });
    }

    private async void TestMethod2()
    {
        await TestMethod();
    }
}",
                GetCA3075LoadXmlCSharpResultAt(12, 13, "Run")
            );

            VerifyBasic(@"
Imports System.Threading.Tasks
Imports System.Xml

Class TestClass
    Private Function TestMethod() As Task
        Await Task.Run(Function() 
        Dim xml = """"
        Dim doc As New XmlDataDocument() With { _
            .XmlResolver = Nothing _
        }
        doc.LoadXml(xml)

End Function)
    End Function

    Private Sub TestMethod2()
        Await TestMethod()
    End Sub
End Class",
                GetCA3075LoadXmlBasicResultAt(12, 9, "Run")
            );
        }
    }
}

