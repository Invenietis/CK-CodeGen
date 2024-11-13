using NUnit.Framework;
using System;
using System.Text;
using FluentAssertions;
using System.Runtime.CompilerServices;

namespace CK.CodeGen.Tests;

[TestFixture]
public class CodeWriterTests
{
    [Test]
    public void appending_an_unknown_typed_object_is_an_error()
    {
        var w = new StringCodeWriter();
        w.Invoking( x => x.Append( this ) ).Should().Throw<ArgumentException>();
    }

    [Test]
    public void AppendVariable_use_At_sign_for_reserved_keywords()
    {
        var writer = new StringCodeWriter( new StringBuilder() );
        foreach( var n in ReservedKeyword.ReservedKeywords )
        {
            writer.AppendVariable( n ).Append( "|" );
        }
        var c = writer.ToString();
        foreach( var n in ReservedKeyword.ReservedKeywords )
        {
            c.Should().Contain( "@" + n + "|" );
        }
    }

    [Test]
    public void And_combiner_works()
    {
        var w1 = new StringCodeWriter( new StringBuilder() );
        var w2 = new StringCodeWriter( new StringBuilder() );

        w1.And( w2 ).AppendSourceString( "Hello!" );

        w1.ToString().Should().Be( "@\"Hello!\"" );
        w2.ToString().Should().Be( "@\"Hello!\"" );
    }

    static string ThisFile( [CallerFilePath] string? f = null ) => f;

    [Test]
    public void region_extension_work()
    {
        var w = new StringCodeWriter( new StringBuilder() );
        w.Append( "Hello" );
        using( w.Region( "R1" ) )
        {
            w.Append( "World" );
            using( w.Region() )
            {
                w.Append( "World Again" );
            }
        }

        w.ToString().ReplaceLineEndings().Should().Be( $"""
                Hello
                #region R1 - Generated by 'region_extension_work' in {ThisFile()}, line: 53.
                World
                #region Generated by 'region_extension_work' in {ThisFile()}, line: 56.
                World Again
                #endregion 'region_extension_work'.

                #endregion R1 - 'region_extension_work'.

                """.ReplaceLineEndings() );

    }

}
