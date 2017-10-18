using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using FluentAssertions;
using System.Threading.Tasks;
using CK.Text;
using CK.CodeGen.Abstractions.Tests;

namespace CK.CodeGen.Tests
{
    [TestFixture]
    public class ToSourceStringTests
    {
        [Test]
        public void writing_and_reading_simple_values_of_all_known_types()
        {
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

            var w = new SimpleCodeWriter();
            w.RawAppend( "using System; using NUnit.Framework; using CK.Text; using System.Collections.Generic; using System.Linq;" )
                .AppendLine()
                .RawAppend( "public class Tester {" )
                .AppendLine()
                .RawAppend( "public string Run() {" )
                .AppendLine()
                .RawAppend( @"
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
                .AppendLine()
                .RawAppend( $"var rewrite = " )
                .Append( array ).RawAppend( ";" )
                .AppendLine()
                .RawAppend( @"
                   var diff = array
                               .Select( ( o, idx ) => new { O = o, T = rewrite[idx], I = idx } )
                               .Where( x => (x.O == null && x.T != null) || (x.O != null && !x.O.Equals( x.T )) )
                               .Select( x => $""{x.I} - {x.O} != {x.T}"" )
                               .Concatenate();
                   return diff;" )
                .AppendLine()
                .RawAppend( "}}" )
                .AppendLine();

            Assembly[] references = new[]
            {
                typeof(object).Assembly,
                typeof(CK.Text.StringMatcher).Assembly,
                typeof(System.Diagnostics.Debug).Assembly,
                typeof(System.Linq.Enumerable).Assembly,
                typeof(TestFixtureAttribute).Assembly
            };

            var code = w.ToString();
            Assembly a = TestHelper.CreateAssembly( code, references );
            object tester = Activator.CreateInstance( a.ExportedTypes.Single( t => t.Name == "Tester" ) );
            string diff = (string)tester.GetType().GetMethod( "Run" ).Invoke( tester, Array.Empty<object>() );
            diff.Should().BeEmpty();
        }

    }
}
