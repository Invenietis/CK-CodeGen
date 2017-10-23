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
using CK.CodeGen.Abstractions;

namespace CK.CodeGen.Tests
{
    public abstract class SimpleBase
    {
        public abstract SqlCommand Do( ref int? i );
    }

    [TestFixture]
    public class CompileSqlTests
    {
        [Test]
        public void SqlTest()
        {
            ICodeWorkspace workspace = CodeWorkspace.Create();
            INamespaceScope global = workspace.Global;
            INamespaceScope b = global.FindOrCreateNamespace( "CK._g" );

            workspace.EnsureAssemblyReference( typeof( SqlCommand ), typeof( SimpleBase ) );
            
            b.EnsureUsing( "System" )
             .EnsureUsing( "System.Collections.Generic" )
             .EnsureUsing( "System.Data.SqlClient" );

            var type = b.CreateType( w => w.Append( "public class GGGG : " ).AppendCSharpName( typeof( SimpleBase ) ) );
            type.CreateOverride( typeof( SimpleBase ).GetMethod( "Do" ) )
                .Append(
                @"if( i.HasValue )
                {
                    i *= i;
                }
                var c = new SqlCommand(""p""+i.ToString());
                var p = c.Parameters.AddWithValue(""@i"", (object)i ?? DBNull.Value);
                return c;" );

            var source = workspace.GetGlobalSource();
            var references = workspace.AssemblyReferences;

            Assembly a = TestHelper.CreateAssembly( source, references );
            Type t = a.GetTypes().Single( n => n.Name == "GGGG" );
            SimpleBase gotIt = (SimpleBase)Activator.CreateInstance( t );
            int? k = 67;
            SqlCommand cmd = gotIt.Do( ref k );
            k.Should().Be( 67 * 67 );
            cmd.CommandText.Should().Be( "p" + k );
            cmd.Parameters.Cast<SqlParameter>().Single().Value.Should().Be( k );
        }
    }
}
