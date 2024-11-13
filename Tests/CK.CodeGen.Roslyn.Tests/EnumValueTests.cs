using NUnit.Framework;
using System;
using System.Linq;
using System.Reflection;
using FluentAssertions;

namespace CK.CodeGen.Roslyn.Tests;

enum ESByte : sbyte
{
    Min = SByte.MinValue,
    Max = SByte.MaxValue
}

enum EByte : byte
{
    Min = Byte.MinValue,
    Max = Byte.MaxValue
}

enum EShort : short
{
    Min = Int16.MinValue,
    Max = Int16.MaxValue
}

enum EUShort : ushort
{
    Min = UInt16.MinValue,
    Max = UInt16.MaxValue
}

enum ELong : long
{
    Min = Int64.MinValue,
    Max = Int64.MaxValue
}

enum EULong : ulong
{
    Min = UInt64.MinValue,
    Max = UInt64.MaxValue
}

[TestFixture]
public class EnumValueTests
{

    [Test]
    public void extreme_enum_values()
    {
        var workspace = CodeWorkspace.Create();
        var global = workspace.Global;
        global.EnsureUsing( "System" )
              .EnsureUsing( "CK.CodeGen.Roslyn.Tests" );

        global.Append( @"
            namespace CK.CodeGen.Roslyn.Tests
            {
                enum ESByte : sbyte
                {
                    Min = SByte.MinValue,
                    Max = SByte.MaxValue
                }

                enum EByte : byte
                {
                    Min = Byte.MinValue,
                    Max = Byte.MaxValue
                }

                enum EShort : short
                {
                    Min = Int16.MinValue,
                    Max = Int16.MaxValue
                }

                enum EUShort : ushort
                {
                    Min = UInt16.MinValue,
                    Max = UInt16.MaxValue
                }

                enum ELong : long
                {
                    Min = Int64.MinValue,
                    Max = Int64.MaxValue
                }

                enum EULong : ulong
                {
                    Min = UInt64.MinValue,
                    Max = UInt64.MaxValue
                }
            }
            " );

        var t = global.CreateType( "public class Tester" );

        var f = t.CreateFunction( "public bool CheckTypes()" );
        f.Append( "ELong eLMax = " ).Append( ELong.Max ).Append( ';' ).NewLine()
         .Append( "EULong eULMax = " ).Append( EULong.Max ).Append( ';' ).NewLine()
         .Append( "EShort eSMax = " ).Append( EShort.Max ).Append( ';' ).NewLine()
         .Append( "EUShort eUSMax = " ).Append( EUShort.Max ).Append( ';' ).NewLine()
         .Append( "EByte eBMax = " ).Append( EByte.Max ).Append( ';' ).NewLine()
         .Append( "ESByte eSBMax = " ).Append( ESByte.Max ).Append( ';' ).NewLine();

        f.Append( "ELong eLMin = " ).Append( ELong.Min ).Append( ';' ).NewLine()
         .Append( "EULong eULMin = " ).Append( EULong.Min ).Append( ';' ).NewLine()
         .Append( "EShort eSMin = " ).Append( EShort.Min ).Append( ';' ).NewLine()
         .Append( "EUShort eUSMin = " ).Append( EUShort.Min ).Append( ';' ).NewLine()
         .Append( "EByte eBMin = " ).Append( EByte.Min ).Append( ';' ).NewLine()
         .Append( "ESByte eSBMin = " ).Append( ESByte.Min ).Append( ';' ).NewLine();

        f.Append( "return " )
         .Append( "eLMax == ELong.Max" ).NewLine()
         .Append( "&& eULMax == EULong.Max" ).NewLine()
         .Append( "&& eSMax == EShort.Max" ).NewLine()
         .Append( "&& eUSMax == EUShort.Max" ).NewLine()
         .Append( "&& eBMax == EByte.Max" ).NewLine()
         .Append( "&& eSBMax == ESByte.Max" ).NewLine()

         .Append( "&& eLMin == ELong.Min" ).NewLine()
         .Append( "&& eULMin == EULong.Min" ).NewLine()
         .Append( "&& eSMin == EShort.Min" ).NewLine()
         .Append( "&& eUSMin == EUShort.Min" ).NewLine()
         .Append( "&& eBMin == EByte.Min" ).NewLine()
         .Append( "&& eSBMin == ESByte.Min" ).NewLine()
         .Append( ";" ).NewLine();

        string source = workspace.GetGlobalSource();
        Assembly a = LocalTestHelper.CreateAssembly( source, workspace.AssemblyReferences );
        a.Should().NotBeNull();

        object tester = Activator.CreateInstance( a.ExportedTypes.Single( t => t.Name == "Tester" ) );
        bool success = (bool)tester.GetType().GetMethod( "CheckTypes" ).Invoke( tester, Array.Empty<object>() );
        success.Should().BeTrue( source );
    }

    [Test]
    public void non_generic_Append_works_the_same()
    {
        var g1 = CodeWorkspace.Create().Global;
        var t1 = g1.Append( ESByte.Min ).Append( ';' ).Append( ESByte.Max ).Append( ';' )
                   .Append( EShort.Min ).Append( ';' ).Append( EShort.Max ).Append( ';' )
                   .Append( EULong.Min ).Append( ';' ).Append( EULong.Max ).Append( ';' ).ToString();

        var g2 = CodeWorkspace.Create().Global;
        var t2 = g2.Append( (object)ESByte.Min ).Append( ';' ).Append( (object)ESByte.Max ).Append( ';' )
                   .Append( (object)EShort.Min ).Append( ';' ).Append( (object)EShort.Max ).Append( ';' )
                   .Append( (object)EULong.Min ).Append( ';' ).Append( (object)EULong.Max ).Append( ';' ).ToString();

        t2.Should().Be( t1 );
    }
}
