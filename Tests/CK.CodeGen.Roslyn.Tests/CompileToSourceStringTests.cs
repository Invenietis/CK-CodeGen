using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using FluentAssertions;
using System.Threading.Tasks;
using CK.Text;
using CK.CodeGen.Abstractions;

namespace CK.CodeGen.Roslyn.Tests
{
    [TestFixture]
    public class CompileToSourceStringTests
    {

        [Test]
        public void skipCompilation_tests()
        {
            var workspace = CodeWorkspace.Create();
            var global = workspace.Global;
            global.EnsureUsing( "System" );
            global.CreateType( "public class Tester" )
                     .Append( "public bool OK => true;" ).NewLine();

            Assembly a = TestHelper.CreateAssembly( workspace.GetGlobalSource(), workspace.AssemblyReferences );
            a.Should().NotBeNull();

            var g = new CodeGenerator( CodeWorkspace.Factory );
            var r = g.Generate( workspace, null, true );
            r.Success.Should().BeTrue();
            r.Sources.Should().HaveCount( 1 );
        }

        [Test]
        public void writing_and_reading_simple_values_of_all_known_types()
        {
            var workspace = CodeWorkspace.Create();
            var global = workspace.Global;

            #region Injected code
            DateTime tD = new DateTime(2017,5,27,13,47,23,DateTimeKind.Local);
            DateTimeOffset tO = new DateTimeOffset( tD.Ticks, TimeSpan.FromMinutes(42) );
            TimeSpan tT = TimeSpan.FromMilliseconds( 987897689 );
            Guid g = new Guid( "4CCF5143-11C2-485F-93CE-76B4B8E1AFA0" );

            object[] array = new object[]
            {
                null,
                true,
                false,
                "Hello World!",
                (sbyte)1,
                (short)2,
                3,
                4398798739879743,
                (byte)5,
                (ushort)6,
                (uint)7,
                (ulong)8,
                'a',
                '\uD800', // A high surrogate
                '\uDC00', // A low surrogate
                '\0',
                '\u3712',
                '\t',
                4.89e-2,
                4.8965e-2f,
                new Decimal( Math.E ),
                g,
                tD,
                tO,
                tT,
                System.Type.Missing,
                typeof(Dictionary<string,int>),
            };
            #endregion

            global.EnsureUsing( "System" )
                  .EnsureUsing( "NUnit.Framework" )
                  .EnsureUsing( "CK.Text" )
                  .EnsureUsing( "System.Collections.Generic" )
                  .EnsureUsing( "System.Linq" );

            global.CreateType( "public class Tester" )
                .Append( "public string Run() {" ).NewLine()
                .Append( @"
            DateTime tD = new DateTime(2017,5,27,13,47,23,DateTimeKind.Local);
            DateTimeOffset tO = new DateTimeOffset( tD.Ticks, TimeSpan.FromMinutes(42) );
            TimeSpan tT = TimeSpan.FromMilliseconds( 987897689 );
            Guid g = new Guid( ""4CCF5143-11C2-485F-93CE-76B4B8E1AFA0"" );

            object[] array = new object[]
            {
                null,
                true,
                false,
                ""Hello World!"",
                (sbyte)1,
                (short)2,
                3,
                4398798739879743,
                (byte)5,
                (ushort)6,
                (uint)7,
                (ulong)8,
                'a',
                '\uD800', // A high surrogate
                '\uDC00', // A low surrogate
                '\0',
                '\u3712',
                '\t',
                4.89e-2,
                4.8965e-2f,
                new decimal( Math.E ),
                g,
                tD,
                tO,
                tT,
                System.Type.Missing,
                typeof(Dictionary<string,int>),
            }; " )
                .NewLine()
                .Append( "var rewrite = " ).AppendArray( array ).Append( ";" ).NewLine()
                .Append( @"
                   var diff = array
                               .Select( ( o, idx ) => new { O = o, T = rewrite[idx], I = idx } )
                               .Where( x => (x.O == null && x.T != null) || (x.O != null && !x.O.Equals( x.T )) )
                               .Select( x => $""{x.I} - {x.O} != {x.T}"" )
                               .Concatenate();
                   return diff;" ).NewLine()
                .Append( "}" )
                .NewLine();

            workspace.EnsureAssemblyReference(
                typeof(object),
                typeof(StringMatcher),
                typeof(System.Diagnostics.Debug),
                typeof(System.Linq.Enumerable),
                typeof(TestFixtureAttribute)
            );

            var source = workspace.GetGlobalSource();
            var references = workspace.AssemblyReferences;
            Assembly a = TestHelper.CreateAssembly( source, references );
            object tester = Activator.CreateInstance( a.ExportedTypes.Single( t => t.Name == "Tester" ) );
            string diff = (string)tester.GetType().GetMethod( "Run" ).Invoke( tester, Array.Empty<object>() );
            diff.Should().BeEmpty();
        }

    }
}
