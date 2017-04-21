using System.Collections.Generic;
using System.Text;

namespace CK.CodeGen
{
    static class BuildHelpers
    {
        internal static readonly string[] OneSpace = new[] { " " };

        internal static void BuildAttributes(List<string> attributes, StringBuilder sb)
        {
            foreach (string attribute in attributes)
            {
                if(attribute == "out" || attribute == "ref" ) sb.AppendWithWhitespace(attribute);
                else
                {
                    if (!attribute.StartsWith("[")) sb.Append("[");
                    sb.Append(attribute);
                    if (!attribute.EndsWith("]")) sb.Append("]");
                }
            }
        }

        internal static void BuildFrontModifiers(List<string> modifiers, StringBuilder sb)
        {
            foreach (string modifier in modifiers) sb.AppendWithWhitespace(modifier);
        }

        internal static void BuildParameters(List<Parameter> parameters, StringBuilder sb)
        {
            sb.Append("(");
            bool isFirst = true;
            foreach (Parameter parameter in parameters)
            {
                if (isFirst) isFirst = false;
                else sb.Append(", ");
                parameter.Build(sb);
            }
            sb.Append(")");
        }

        internal static void BuildMethodBody(string body, StringBuilder sb)
        {
            body = body.ToString().Trim();
            bool isLambda = body.StartsWith("=>");
            if (isLambda)
            {
                sb.Append(body);
                if (!body.EndsWith(";")) sb.Append(";");
            }
            else
            {
                sb.Append("{").Append(body).Append("}");
            }
        }
    }
}
