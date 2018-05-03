using CK.CodeGen.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace CK.CodeGen.ScopeModel
{
    class FunctionNameImpl : IFunctionName
    {
        readonly string _normalized;

        public FunctionNameImpl( string n, string gen, string par, string normalized )
        {
            NakedName = n;
            GenericPart = gen;
            ParametersPart = par;
            _normalized = normalized;
        }

        public string NakedName { get; }

        public string GenericPart { get; }

        public string ParametersPart { get; }

        public override string ToString() => _normalized;
    }
}
