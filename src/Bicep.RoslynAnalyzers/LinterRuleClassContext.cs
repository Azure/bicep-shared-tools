// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bicep.RoslynAnalyzers
{
    public class LinterRuleClassContext : IEquatable<LinterRuleClassContext>
    {
        public INamedTypeSymbol RuleClassSymbol { get; private set; }

        public LinterRuleClassContext(INamedTypeSymbol namedTypeSymbol)
        {
            this.RuleClassSymbol = namedTypeSymbol;
        }

        public bool Equals(LinterRuleClassContext other) => this.RuleClassSymbol.Equals(other.RuleClassSymbol, SymbolEqualityComparer.Default);

        public override bool Equals(object obj)
        {
            if(obj is LinterRuleClassContext other)
            {
                return this.Equals(other);
            }

            return false;
        }

        public override int GetHashCode() => 1634863193 + SymbolEqualityComparer.Default.GetHashCode(RuleClassSymbol);
    }
}
