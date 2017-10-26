using CK.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace CK.CodeGen
{
    class AttributeDefinition
    {
        public class OneAttribute
        {
            internal OneAttribute( TypeName name, IReadOnlyList<string> values )
            {
                Name = name;
                Values = values ?? Array.Empty<string>();
            }

            public TypeName Name { get; }

            public IReadOnlyList<string> Values { get; }

            public StringBuilder Write( StringBuilder b )
            {
                Name.Write( b );
                if( Values.Count > 0 )
                {
                    b.Append( '(' ).AppendStrings( Values, ", " ).Append( ')' );
                }
                return b;
            }
        }

        public AttributeDefinition( string target, IReadOnlyList<OneAttribute> attributes )
        {
            Target = target ?? String.Empty;
            Attributes = attributes ?? Array.Empty<OneAttribute>();
        }

        public string Target { get; }

        public IReadOnlyList<OneAttribute> Attributes { get; }

        public StringBuilder Write( StringBuilder b )
        {
            b.Append( '[' );
            if( Target.Length > 0 ) b.Append( Target ).Append( ": " );
            bool already = false;
            foreach( var one in Attributes )
            {
                if( already ) b.Append( ", " );
                else already = true;
                one.Write( b );
            }
            return b;
        }

    }
}
