using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Reflection;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CK.CodeGen.Tests
{
    public abstract class SimpleBase
    {
        public abstract SqlCommand Do(ref int? i);
    }

    [TestFixture]
    public class SqlTests
    {

#if !NET461
        class FixDBNull : ICodeGeneratorModule
        {
            class DBNullToCKFixRewriter : CSharpSyntaxRewriter
            {
                public override SyntaxNode VisitIdentifierName( IdentifierNameSyntax node )
                {
                    if( node.Identifier.Text == "DBNull")
                    {
                        return node.WithIdentifier( SyntaxFactory.Identifier( "CKFixDBNull" ) );
                    }
                    return node;
                }

                public static SyntaxTree Run( SyntaxTree t )
                {
                    return t.WithRootAndOptions( new DBNullToCKFixRewriter().Visit( t.GetRoot() ), t.Options );
                }
            }

            public IEnumerable<Assembly> RequiredAssemblies => new[] { typeof( System.Reflection.TypeInfo ).GetTypeInfo().Assembly };

            public void AppendSource( StringBuilder b ) 
            {
                b.AppendLine( @"namespace System 
                { 
                    using System.Reflection; 
                    public static class CKFixDBNull
                    { 
                        public static object Value = typeof( object ).GetTypeInfo().Assembly.GetType( ""System.DBNull"" ).GetField(""Value"").GetValue(null);
                    }
                }" );
            }

            public SyntaxTree PostProcess( SyntaxTree t ) => DBNullToCKFixRewriter.Run( t );
        }
#endif

        [Test]
        public void SqlTest()
        {
            NamespaceBuilder b = new NamespaceBuilder("CK._g");
            b.Usings.Build().Add("System").Add("System.Collections.Generic").Add("System.Data.SqlClient");

            var c = b.DefineClass("GGGG").Build()
                        .SetBase( typeof( SimpleBase ) )
                        .DefineOverrideMethod( typeof( SimpleBase ).GetMethod( "Do" ), body =>
                        {
                            body.Append( @"
                            if( i.HasValue )
                            {
                                i *= i;
                            }
                            var c = new SqlCommand(""p""+i.ToString());
                            var p = c.Parameters.AddWithValue(""@i"", (object)i ?? DBNull.Value);
                            return c;
                            " );
                        } );
            string source = b.CreateSource();
            Assembly[] references = new[]
            {
                //typeof(object).GetTypeInfo().Assembly,
                //typeof(System.Diagnostics.Debug).GetTypeInfo().Assembly,
                //typeof(SqlCommand).GetTypeInfo().Assembly,
                typeof(SimpleBase).GetTypeInfo().Assembly
            };
#if NET461
            Assembly a = TestHelper.CreateAssembly(source, references );
#else
            Assembly a = TestHelper.CreateAssembly(source, references, new FixDBNull() );
#endif
            Type t = a.GetTypes().Single(n => n.Name == "GGGG");
            SimpleBase gotIt = (SimpleBase)Activator.CreateInstance(t);
            int? k = 67;
            SqlCommand cmd = gotIt.Do(ref k);
            k.Should().Be(67 * 67);
            cmd.CommandText.Should().Be("p" + k);
            cmd.Parameters.Cast<SqlParameter>().Single().Value.Should().Be(k);
        }
    }
}
