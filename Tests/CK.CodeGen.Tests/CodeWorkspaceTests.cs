using System;
using CK.CodeGen;
using NUnit.Framework;
using FluentAssertions;
using static CK.Testing.MonitorTestHelper;
using CK.Text;
using System.IO;
using CK.Core;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CK.CodeGen.Tests
{
    [TestFixture]
    public class CodeWorkspaceTests
    {
        class Tracker
        {
            public int NamespaceCount { get; private set; }
            public int TypeCount { get; private set; }
            public int FunctionCount { get; private set; }
            public int AllCount => NamespaceCount + TypeCount + FunctionCount;

            public Tracker( ICodeWorkspace w )
            {
                w.NamespaceCreated += OnNamespaceCreated;
                w.TypeCreated += OnTypeCreated;
                w.FunctionCreated += OnFunctionCreated;
            }

            void OnFunctionCreated( IFunctionScope obj ) => FunctionCount++;
            void OnNamespaceCreated( INamespaceScope obj ) => NamespaceCount++;
            void OnTypeCreated( ITypeScope obj ) => TypeCount++;
        }


        [Test]
        public void workspace_events()
        {
            var ws = CodeWorkspace.Create();
            var tracker = new Tracker( ws );
            INamespaceScope g = ws.Global;
            tracker.AllCount.Should().Be( 0 );
            var t1 = g.CreateType( "public class C1" );
            var ns1 = g.FindOrCreateNamespace( "Yop.Yup.Yip" );
            tracker.NamespaceCount.Should().Be( 3 );
            tracker.TypeCount.Should().Be( 1 );
            var t2 = ns1.CreateType( t => t.Append( "private ref readonly struct XRQ { public readonly int X = 3; }" ) );
            tracker.TypeCount.Should().Be( 2 );
            var f1 = t2.CreateFunction( "static public void Func() => X;" );
            tracker.NamespaceCount.Should().Be( 3 );
            tracker.TypeCount.Should().Be( 2 );
            tracker.FunctionCount.Should().Be( 1 );
        }

        [Test]
        public void added_CodeGenerated_attributes_thanks_to_the_events()
        {
            INamespaceScope g = CodeWorkspace.Create().Global;
            g.Workspace.TypeCreated += Workspace_TypeCreated;
            var t1 = g.GeneratedByComment().CreateType( "public class C1" );
            var t2 = g.FindOrCreateNamespace( "Yop.Yup.Yip" ).CreateType( t => t.Append( "private ref readonly struct XRQ { public readonly int X = 3; }" ) );

            var text = g.ToString();
            // Here we add the a "Name" property (the real StObjGenAttribute has no parameters nor properties).
            text.Should().Contain( @"[type: StObjGen(Name = @""XRQ"")]readonly ref struct XRQ" );
            text.Should().Contain( @"[type: StObjGen(Name = @""C1"")]public class C1" );
        }

        void Workspace_TypeCreated( ITypeScope t )
        {
            t.Definition.Attributes.Ensure( CodeAttributeTarget.Type ).Attributes.Add( new AttributeDefinition( "StObjGen" , $"Name = {t.Definition.Name.Name.ToSourceString()}" ) );
        }
    }
}
