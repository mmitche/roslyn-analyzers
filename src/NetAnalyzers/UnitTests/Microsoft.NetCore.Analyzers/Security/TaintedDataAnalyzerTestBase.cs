﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Test.Utilities;
using Xunit;

namespace Microsoft.NetCore.Analyzers.Security.UnitTests
{
    [Trait(Traits.DataflowAnalysis, Traits.Dataflow.TaintedDataAnalysis)]
    public abstract class TaintedDataAnalyzerTestBase<TCSharpAnalyzer, TVisualBasicAnalyzer>
        where TCSharpAnalyzer : DiagnosticAnalyzer, new()
        where TVisualBasicAnalyzer : DiagnosticAnalyzer, new()
    {
        protected abstract DiagnosticDescriptor Rule { get; }

        protected virtual IEnumerable<string> AdditionalCSharpSources { get; }

        protected virtual IEnumerable<string> AdditionalVisualBasicSources { get; }

        protected DiagnosticResult GetCSharpResultAt(int sinkLine, int sinkColumn, int sourceLine, int sourceColumn, string sink, string sinkContainingMethod, string source, string sourceContainingMethod)
        {
            return new DiagnosticResult(Rule).WithArguments(sink, sinkContainingMethod, source, sourceContainingMethod)
                .WithLocation(sinkLine, sinkColumn)
                .WithLocation(sourceLine, sourceColumn);
        }

        protected async Task VerifyCSharpWithDependenciesAsync(string source, params DiagnosticResult[] expected)
        {
            var test = new CSharpSecurityCodeFixVerifier<TCSharpAnalyzer, EmptyCodeFixProvider>.Test();
            test.ReferenceAssemblies = AdditionalMetadataReferences.DefaultForTaintedDataAnalysis;
            test.TestState.AdditionalReferences.Add(AdditionalMetadataReferences.TestReferenceAssembly);

            test.TestState.Sources.Add(source);
            if (AdditionalCSharpSources is object)
            {
                foreach (var additionalSource in AdditionalCSharpSources)
                {
                    test.TestState.Sources.Add(additionalSource);
                }
            }

            test.TestState.ExpectedDiagnostics.AddRange(expected);
            await test.RunAsync();
        }

        protected async Task VerifyCSharpWithDependenciesAsync(string source, FileAndSource additionalFile, params DiagnosticResult[] expected)
        {
            var test = new CSharpSecurityCodeFixVerifier<TCSharpAnalyzer, EmptyCodeFixProvider>.Test();
            test.ReferenceAssemblies = AdditionalMetadataReferences.DefaultForTaintedDataAnalysis;
            test.TestState.AdditionalReferences.Add(AdditionalMetadataReferences.TestReferenceAssembly);

            test.TestState.Sources.Add(source);
            if (AdditionalCSharpSources is object)
            {
                foreach (var additionalSource in AdditionalCSharpSources)
                {
                    test.TestState.Sources.Add(additionalSource);
                }
            }

            test.TestState.AdditionalFiles.Add((additionalFile.FilePath, additionalFile.Source));

            test.TestState.ExpectedDiagnostics.AddRange(expected);
            await test.RunAsync();
        }

        protected DiagnosticResult GetBasicResultAt(int sinkLine, int sinkColumn, int sourceLine, int sourceColumn, string sink, string sinkContainingMethod, string source, string sourceContainingMethod)
        {
            return new DiagnosticResult(Rule).WithArguments(sink, sinkContainingMethod, source, sourceContainingMethod)
                .WithLocation(sinkLine, sinkColumn)
                .WithLocation(sourceLine, sourceColumn);
        }

        protected async Task VerifyVisualBasicWithDependenciesAsync(string source, params DiagnosticResult[] expected)
        {
            var test = new VisualBasicSecurityCodeFixVerifier<TVisualBasicAnalyzer, EmptyCodeFixProvider>.Test();
            test.ReferenceAssemblies = AdditionalMetadataReferences.DefaultForTaintedDataAnalysis;
            test.TestState.AdditionalReferences.Add(AdditionalMetadataReferences.TestReferenceAssembly);

            test.TestState.Sources.Add(source);
            if (AdditionalVisualBasicSources is object)
            {
                foreach (var additionalSource in AdditionalVisualBasicSources)
                {
                    test.TestState.Sources.Add(additionalSource);
                }
            }

            test.TestState.ExpectedDiagnostics.AddRange(expected);
            await test.RunAsync();
        }
    }
}
