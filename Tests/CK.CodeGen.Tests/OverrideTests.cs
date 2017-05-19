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
    public abstract class BaseToBeOverridden
    {
        public abstract int Simple1();

        protected abstract string Simple2(string x, Guid g);

        internal protected abstract BaseToBeOverridden Simple3(out string x, ref Guid g, int p);

    }

    public abstract class ContainsGenericMethods<T>
    {
        public abstract TResult Simple1<TResult>(T arg);

        public virtual bool Simple2<T1, T2>(T1 arg1, T2 arg2)
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
            NamespaceBuilder b = new NamespaceBuilder("CK._g");
            Type t = typeof(BaseToBeOverridden);

            b.Usings.Build().Add("System").Add("System.Collections.Generic").Add(t.Namespace);
            var c = b.DefineClass("Specialized")
                        .Build()
                        .SetBase( t )
                        .DefineOverrideMethod( t.GetMethod("Simple1"), body =>
                        {
                            body.Append( "=> 3712" );
                        })
                        .DefineOverrideMethod(t.GetMethod("Simple2", BindingFlags.Instance | BindingFlags.NonPublic), body =>
                        {
                            body.Append("return x + '-' + g.ToString();");
                        })
                        .DefineOverrideMethod(t.GetMethod("Simple3", BindingFlags.Instance | BindingFlags.NonPublic), body =>
                        {
                            body.AppendLine("g = Guid.NewGuid();")
                                .AppendLine(@"x = ""Hello World!"" + Simple2( ""YES"", g );")
                                .AppendLine("return this;");
                        });
            string source = b.CreateSource();
            Assembly[] references = new[]
            {
                typeof(object).GetTypeInfo().Assembly,
                typeof(BaseToBeOverridden).GetTypeInfo().Assembly
            };
            Assembly a = TestHelper.CreateAssembly(source, references);
            Type tC = a.GetTypes().Single(n => n.Name == "Specialized");
            BaseToBeOverridden gotIt = (BaseToBeOverridden)Activator.CreateInstance(tC);
            gotIt.Simple1().Should().Be(3712);
            string s;
            Guid g = Guid.Empty;
            gotIt.Simple3(out s, ref g, 9).Should().BeSameAs(gotIt);
            s.Should().Be("Hello World!YES-"+g.ToString());
            g.Should().NotBeEmpty();
        }

        [Test]
        public void BuildGenericMethods()
        {
            NamespaceBuilder b = new NamespaceBuilder("CK._g");
            Type t = typeof(ContainsGenericMethods<>);

            b.Usings.Build().Add(t.Namespace);
            var c = b.DefineClass("Specialized<T>")
                        .Build()
                        .SetBase(t, "T")
                        .DefineOverrideMethod(t.GetMethod("Simple1"), body =>
                        {
                            body.Append("if (arg.Equals(default(T))) throw new System.ArgumentException();")
                                .Append("return default(TResult);");
                        })
                        .DefineOverrideMethod(t.GetMethod("Simple2"), body =>
                        {
                            body.Append("=> arg2 is T1");
                        });
            string source = b.CreateSource();
            Assembly[] references = new[]
            {
                typeof(object).GetTypeInfo().Assembly,
                typeof(ContainsGenericMethods<>).GetTypeInfo().Assembly
            };
            Assembly a = TestHelper.CreateAssembly(source, references);
            Type tC = a.GetTypes().Single(n => n.Name == "Specialized`1").MakeGenericType(typeof(int));
            ContainsGenericMethods<int> gotIt = (ContainsGenericMethods<int>)Activator.CreateInstance(tC);
            gotIt.Simple1<bool>(25).Should().BeFalse();
            gotIt.Simple2(new object(), "test").Should().BeTrue();
        }
    }
}
