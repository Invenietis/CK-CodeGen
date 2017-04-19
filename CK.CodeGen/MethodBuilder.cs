using System.Collections.Generic;
using System.Text;

namespace CK.CodeGen
{
    public class MethodBuilder
    {
        public List<string> Attributes { get; } = new List<string>();

        public List<string> FrontModifiers { get; } = new List<string>();

        public string Name { get; set; }

        public string ReturnType { get; set; }

        public List<Parameter> Parameters { get; } = new List<Parameter>();

        public List<GenericConstraint> GenericConstraints { get; set; } = new List<GenericConstraint>();

        public StringBuilder Body { get; } = new StringBuilder();


    }
}