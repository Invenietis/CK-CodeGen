using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Reflection;
using System.Linq;
using FluentAssertions;
using CK.CodeGen.Abstractions;

namespace CK.CodeGen.Tests
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
    public class OverrideTests
    {
        [Test]
        public void BaseTest()
        {
            INamespaceScope global = CodeScope.CreateGlobalNamespace();
            INamespaceScope b = global.FindOrCreateNamespace( "CK._g" );
            Type t = typeof( BaseToBeOverridden );

            b.EnsureUsing( "System" ).EnsureUsing( "System.Collections.Generic" ).EnsureUsing( t.Namespace );
            var c = b.CreateType( h => h.DefineKind("class")
                                        .DefineName( "Specialized" )
                                        .SetBase( t ) );
            c.DefinePassThroughConstructors( t )
                .DefineOverrideMethod( t.GetMethod( "Simple1" ), body =>
                {
                    body.Append( "return 3712" );
                } )
                .DefineOverrideMethod( t.GetMethod( "VoidMethod" ), body =>
                {
                } )
                .DefineOverrideMethod( t.GetMethod( "Simple2", BindingFlags.Instance | BindingFlags.NonPublic ), body =>
                   {
                       body.Append( "return x + '-' + g.ToString();" );
                   } )
                .DefineOverrideMethod( t.GetMethod( "Simple3", BindingFlags.Instance | BindingFlags.NonPublic ), body =>
                   {
                       body.AppendLine( "g = Guid.NewGuid();" )
                           .AppendLine( @"x = ""Hello World!"" + Simple2( ""YES"", g );" )
                           .AppendLine( "return this;" );
                   } );
            // TODO
            //string source = b.CreateSource();
            //Assembly[] references = new[]
            //{
            //    typeof(BaseToBeOverridden).GetTypeInfo().Assembly
            //};
            //Assembly a = TestHelper.CreateAssembly(source, references);
            //Type tC = a.GetTypes().Single(n => n.Name == "Specialized");
            //BaseToBeOverridden gotIt = (BaseToBeOverridden)Activator.CreateInstance(tC, new object[] { 3712*3712 } );
            //gotIt.ValFromCtor.Should().Be( 3712 * 3712 );
            //gotIt.Simple1().Should().Be(3712);
            //string s;
            //Guid g = Guid.Empty;
            //gotIt.Simple3(out s, ref g, 9).Should().BeSameAs(gotIt);
            //s.Should().Be("Hello World!YES-"+g.ToString());
            //g.Should().NotBeEmpty();
        }

        [Test]
        public void BuildGenericMethods()
        {
            INamespaceScope global = CodeScope.CreateGlobalNamespace();
            INamespaceScope b = global.FindOrCreateNamespace( "CK._g" );
            Type t = typeof( ContainsGenericMethods<> );

            b.EnsureUsing( t.Namespace );
            var c = b.CreateType( header => header.DefineKind("class")
                                                  .DefineName( "Specialized<T>" )
                                                  .SetBase( t ) )
                        .DefineOverrideMethod( t.GetMethod( "Simple1" ), body =>
                           {
                               body.Append( "if (arg.Equals(default(T))) throw new System.ArgumentException();" )
                                   .Append( "return default(TResult);" );
                           } )
                        .DefineOverrideMethod( t.GetMethod( "Simple2" ), body =>
                           {
                               body.Append( "=> arg2 is T1" );
                           } );
            // TODO
            //string source = b.CreateSource();
            //Assembly[] references = new[]
            //{
            //    typeof(ContainsGenericMethods<>).GetTypeInfo().Assembly
            //};
            //Assembly a = TestHelper.CreateAssembly( source, references );
            //Type tC = a.GetTypes().Single( n => n.Name == "Specialized`1" ).MakeGenericType( typeof( int ) );
            //ContainsGenericMethods<int> gotIt = (ContainsGenericMethods<int>)Activator.CreateInstance( tC );
            //gotIt.Simple1<bool>( 25 ).Should().BeFalse();
            //gotIt.Simple2( new object(), "test" ).Should().BeTrue();
        }
    }
}
