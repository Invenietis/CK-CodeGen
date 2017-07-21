using System;
using System.Collections.Generic;
using System.Text;

namespace CK.CodeGen
{
    static class BuildHelpers
    {
        internal static readonly string[] OneSpace = new[] { " " };

        internal static string CheckValidFullName(string n)
        {
            if (string.IsNullOrWhiteSpace(n) || n[n.Length - 1] == '.') throw new ArgumentException($"Invalid name '{n}'. Must not be empty nor ends with a dot.");
            return n;
        }

        internal static void BuildAttributes(StringBuilder b, List<string> attributes)
        {
            foreach (string attribute in attributes)
            {
                if(attribute == "out" || attribute == "ref" ) b.AppendWithWhitespace(attribute);
                else
                {
                    if (!attribute.StartsWith("[")) b.Append("[");
                    b.Append(attribute);
                    if (!attribute.EndsWith("]")) b.Append("]");
                    b.AppendLine();
                }
            }
        }

        internal static void BuildFrontModifiers(StringBuilder b, IEnumerable<string> modifiers)
        {
            foreach (string modifier in modifiers) b.AppendWithWhitespace(modifier);
        }

        internal static void BuildMethodBody(StringBuilder b, string body)
        {
            body = body.ToString().Trim();
            bool isLambda = body.StartsWith("=>");
            if (isLambda)
            {
                b.Append(body);
                if (!body.EndsWith(";")) b.AppendLine(";");
            }
            else
            {
                b.Append("{").Append(body).AppendLine("}");
            }
        }
    }
}
