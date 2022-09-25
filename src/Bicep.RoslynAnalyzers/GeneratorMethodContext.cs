// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;

namespace Bicep.RoslynAnalyzers
{
    public class GeneratorMethodContext : IEquatable<GeneratorMethodContext>
    {
        public string Namespace { get; private set; }

        public string ClassName { get; private set; }

        public string MethodName { get; private set; }

        public GeneratorMethodContext(string @namespace, string className, string methodName)
        {
            this.Namespace = @namespace;
            this.ClassName = className;
            this.MethodName = methodName;
        }

        public bool Equals(GeneratorMethodContext other) =>
            string.Equals(this.Namespace, other.Namespace, StringComparison.Ordinal) &&
            string.Equals(this.ClassName, other.ClassName, StringComparison.Ordinal) &&
            string.Equals(this.MethodName, other.MethodName, StringComparison.Ordinal);

        public override bool Equals(object obj)
        {
            if(obj is GeneratorMethodContext other)
            {
                return this.Equals(other);
            }

            return false;
        }

        public override int GetHashCode()
        {
            int hashCode = -998431153;
            hashCode = hashCode * -1521134295 + StringComparer.Ordinal.GetHashCode(Namespace);
            hashCode = hashCode * -1521134295 + StringComparer.Ordinal.GetHashCode(ClassName);
            hashCode = hashCode * -1521134295 + StringComparer.Ordinal.GetHashCode(MethodName);
            return hashCode;
        }
    }
}
