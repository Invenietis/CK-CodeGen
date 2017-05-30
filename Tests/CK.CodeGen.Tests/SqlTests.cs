using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Reflection;
using System.Linq;
using FluentAssertions;

namespace CK.CodeGen.Tests
{
    public abstract class SimpleBase
    {
        public abstract SqlCommand Do(ref int? i);
    }

    [TestFixture]
    public class SqlTests
    {
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
                typeof(object).GetTypeInfo().Assembly,
                typeof(System.Diagnostics.Debug).GetTypeInfo().Assembly,
                typeof(SqlCommand).GetTypeInfo().Assembly,
                typeof(SimpleBase).GetTypeInfo().Assembly
            };
            Assembly a = TestHelper.CreateAssembly(source, references);
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
