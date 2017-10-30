using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Reflection;
using System.Linq;
using FluentAssertions;
using CK.CodeGen.Abstractions;

namespace CK.CodeGen.Roslyn.Tests
{
    public abstract class BaseToBeOverridden
    {
        protected BaseToBeOverridden( int val )
        {
            ValFromCtor = val;
        }

        public int ValFromCtor { get; }

        public abstract int Simple1();

        public abstract void VoidMethod();

        protected abstract string Simple2( string x, Guid g );

        internal protected abstract BaseToBeOverridden Simple3( out string x, ref Guid g, int p );

    }

    public abstract class ContainsGenericMethods<T>
    {
        public abstract TResult Simple1<TResult>( T arg );

        public virtual bool Simple2<T1, T2>( T1 arg1, T2 arg2 )
        {
            return false;
        }
    }

    [TestFixture]
    public class CompileOverrideTests
    {
        [Test]
        public void BaseTest()
        {
            ICodeWorkspace workspace = CodeWorkspace.Create();
            INamespaceScope global = workspace.Global;
            INamespaceScope b = global.FindOrCreateNamespace( "CK._g" );

            Type t = typeof( BaseToBeOverridden );

            workspace.EnsureAssemblyReference( t );

            b.EnsureUsing( "System" )
             .EnsureUsing( "System.Collections.Generic" )
             .EnsureUsing( t.Namespace );

            var c = b.CreateType( h => h.Append( "class Specialized : " ).AppendCSharpName( t ) );
            c.AppendPassThroughConstructors( t )
             .AppendOverrideSignature( t.GetMethod( "Simple1" ) ).Append( "=> 3712;" ).NewLine()
             .AppendOverrideSignature( t.GetMethod( "VoidMethod" ) ).Append( "{}" ).NewLine()
             .AppendOverrideSignature( t.GetMethod( "Simple2", BindingFlags.Instance | BindingFlags.NonPublic ) )
                .Append( "=> x + '-' + g.ToString();" ).NewLine()
             .AppendOverrideSignature( t.GetMethod( "Simple3", BindingFlags.Instance | BindingFlags.NonPublic ) )
                .Append( "{ g = Guid.NewGuid();" ).NewLine()
                .Append( @"x = ""Hello World!"" + Simple2( ""YES"", g );" ).NewLine()
                .Append( "return this; }" ).NewLine();

            Assembly a = TestHelper.CreateAssembly( workspace.GetGlobalSource(), workspace.AssemblyReferences );

            Type tC = a.GetTypes().Single( n => n.Name == "Specialized" );
            BaseToBeOverridden gotIt = (BaseToBeOverridden)Activator.CreateInstance( tC, new object[] { 3712 * 3712 } );
            gotIt.ValFromCtor.Should().Be( 3712 * 3712 );
            gotIt.Simple1().Should().Be( 3712 );
            string s;
            Guid g = Guid.Empty;
            gotIt.Simple3( out s, ref g, 9 ).Should().BeSameAs( gotIt );
            s.Should().Be( "Hello World!YES-" + g.ToString() );
            g.Should().NotBeEmpty();
        }

        [Test]
        public void BuildGenericMethods()
        {
            ICodeWorkspace workspace = CodeWorkspace.Create();
            INamespaceScope global = workspace.Global;
            INamespaceScope b = global.FindOrCreateNamespace( "CK._g" );

            Type t = typeof( ContainsGenericMethods<> );

            workspace.EnsureAssemblyReference( t );

            b.EnsureUsing( t.Namespace );
            var c = b.CreateType( header => header.Append( "class Specialized<T> : " ).AppendCSharpName( t ).NewLine() );
            c.AppendOverrideSignature( t.GetMethod( "Simple1" ) )
                .Append( "{ if (arg.Equals(default(T))) throw new System.ArgumentException();" ).NewLine()
                .Append( "return default(TResult); }" )
              .AppendOverrideSignature( t.GetMethod( "Simple2" ) )
                .Append( "=> arg2 is T1;" );

            Assembly a = TestHelper.CreateAssembly( workspace.GetGlobalSource(), workspace.AssemblyReferences );

            Type tC = a.GetTypes().Single( n => n.Name == "Specialized`1" ).MakeGenericType( typeof( int ) );
            ContainsGenericMethods<int> gotIt = (ContainsGenericMethods<int>)Activator.CreateInstance( tC );
            gotIt.Simple1<bool>( 25 ).Should().BeFalse();
            gotIt.Simple2( new object(), "test" ).Should().BeTrue();
        }
    }
}
