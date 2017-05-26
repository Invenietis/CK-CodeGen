using NUnit.Framework;
using System;
using System.IO;
using System.Reflection;
using FluentAssertions;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using CK.Core;
using System.Text;

namespace CK.CodeGen.Tests
{
    [TestFixture]
    public class NamespaceBuilderTests
    {
        [TestCase( null, "null" )]
        [TestCase( new byte[0], "Array.Empty<byte>()" )]
        [TestCase( new byte[] { 1 }, "new byte[] {1}" )]
        [TestCase( new byte[] { 8, 78, 211 }, "new byte[] {8,78,211}" )]
        public void bytes_array_to_source( byte[] b, string result )
        {
            b.ToSourceString( new StringBuilder() ).ToString().Should().Be( result );
        }

        [Test]
        public void SimpleTest()
        {
            NamespaceBuilder builder = new NamespaceBuilder("Simple.Namespace");
            {
                InterfaceBuilder simpleInterface = builder.DefineInterface("SimpleInterface");
                simpleInterface.DefineProperty("int", "P1");
            }

            {
                ClassBuilder simpleClass = builder.DefineClass("SimpleClass");
                simpleClass.FrontModifiers.Add("public");
                simpleClass.Interfaces.Add("SimpleInterface");

                FieldBuilder f = simpleClass.DefineField("int", "_x");
                f.FrontModifiers.Add("private");

                ConstructorBuilder c = simpleClass.DefineConstructor( "internal" );
                c.Parameters.Add(new ParameterBuilder { ParameterType = "int", Name = "x" });
                c.Body.Append("_x = x;");

                MethodBuilder toString = simpleClass.DefineMethod("ToString");
                toString.FrontModifiers.AddRange(new[] { "public", "override" });
                toString.ReturnType = "string";
                toString.Body.Append(@"=> string.Format(""X: {0}"", _x)");

                PropertyBuilder p = simpleClass.DefineProperty("int", "P1");
                p.FrontModifiers.Add("public");
                p.GetMethod.Body.AppendLine("return _x;");
                p.SetMethod.Body.AppendLine("_x = value;");
            }

            ClassBuilder factoryBuilder;
            {
                factoryBuilder = builder.DefineClass("SimpleFactory");
                factoryBuilder.FrontModifiers.AddRange(new[] { "public", "static" });
                MethodBuilder mb = factoryBuilder.DefineMethod("Create");
                mb.FrontModifiers.AddRange(new[] { "public", "static" });
                mb.ReturnType = "SimpleClass";
                mb.Parameters.Add(new ParameterBuilder { ParameterType = "int", Name = "n" });
                mb.Body.Append("=> new SimpleClass(n)");
            }

            Assembly[] references = new[]
            {
                typeof(object).GetTypeInfo().Assembly
            };
            Assembly assembly = TestHelper.CreateAssembly(builder.CreateSource(), references);

            {
                Type factory = assembly.GetType(factoryBuilder.FullName);
                dynamic instance = factory.GetMethod("Create").Invoke(null, new object[] { 25 });
                int currentP1 = instance.P1;
                currentP1.Should().Be(25);

                instance.P1 = 12;
                currentP1 = instance.P1;
                currentP1.Should().Be(12);

                string s = instance.ToString();
                s.Should().Be("X: 12");
            }
        }

        [Test]
        public void CompleteTest()
        {
            NamespaceBuilder sut = CreateNamespaceBuilder();
            string source = sut.CreateSource();
            Assembly[] references = new[]
            {
                typeof(object).GetTypeInfo().Assembly,
                typeof(System.Diagnostics.Debug).GetTypeInfo().Assembly
            };
            Assembly a = TestHelper.CreateAssembly(source, references);
            a.Invoking(x => x.GetType("Class2")).ShouldNotThrow();
        }

        NamespaceBuilder CreateNamespaceBuilder()
        {
            NamespaceBuilder nsBuilder = new NamespaceBuilder("CK.CodeGen.Tests");
            nsBuilder.Usings.AddRange(new[]
            {
                "System",
                "System.Collections",
                "System.Collections.Generic",
                "System.Diagnostics"
            });

            {
                EnumBuilder enum1 = nsBuilder.DefineEnum("Enum1");
                enum1.FrontModifiers.Add("public");
                enum1.Values.AddRange(new[]
                {
                    new EnumValue { Name = "None", Value = "0" },
                    new EnumValue { Name = "Case1", Value = "1" },
                    new EnumValue { Name = "Case2" }
                });
            }

            {
                StructBuilder struct1 = nsBuilder.DefineStruct("Struct1");
                struct1.FrontModifiers.Add("public");

                FieldBuilder xBuilder = struct1.DefineField("int", "_x");
                xBuilder.FrontModifiers.AddRange(new[] { "private", "readonly" });
                FieldBuilder yBuilder = struct1.DefineField("int", "_y");
                yBuilder.FrontModifiers.AddRange(new[] { "private", "readonly" });

                ConstructorBuilder ctorBuilder = struct1.DefineConstructor();
                ctorBuilder.Parameters.AddRange(new[]
                {
                    new ParameterBuilder { ParameterType = "int", Name = "x" },
                    new ParameterBuilder { ParameterType = "int", Name = "y" }
                });
                ctorBuilder.Body
                    .Append("_x = x;")
                    .Append("_y = y;");

                PropertyBuilder xPropBuilder = struct1.DefineProperty("int", "X");
                xPropBuilder.FrontModifiers.Add("public");
                xPropBuilder.GetMethod.Body.Append("=> _x");

                PropertyBuilder yPropBuilder = struct1.DefineProperty("int", "Y");
                yPropBuilder.FrontModifiers.Add("public");
                yPropBuilder.GetMethod.Body.Append("=> _y");

                MethodBuilder equalsBuilder = struct1.DefineMethod("Equals");
                equalsBuilder.FrontModifiers.AddRange(new[] { "public", "override" });
                equalsBuilder.ReturnType = "bool";
                equalsBuilder.Parameters.Add(new ParameterBuilder { ParameterType = "object", Name = "obj" });
                equalsBuilder.Body
                    .Append("if (!(obj is Struct1)) return false;")
                    .Append("Struct1 other = (Struct1)obj;")
                    .Append("return _x == other._x && _y == other._y;");

                MethodBuilder getHashCodeBuilder = struct1.DefineMethod("GetHashCode");
                getHashCodeBuilder.FrontModifiers.AddRange(new[] { "public", "override" });
                getHashCodeBuilder.ReturnType = "int";
                getHashCodeBuilder.Body.Append("return _x.GetHashCode() ^ _y.GetHashCode();");
            }

            {
                InterfaceBuilder interface1 = nsBuilder.DefineInterface("Interface1");
                interface1.FrontModifiers.Add("public");

                var m1Builer = interface1.DefineMethod("M1");
                m1Builer.ReturnType = "Struct1";
                m1Builer.Parameters.AddRange(new[]
                {
                    new ParameterBuilder { ParameterType = "int", Name = "arg1"},
                    new ParameterBuilder { ParameterType = "Enum1", Name = "arg2"}
                });

                var m2Builer = interface1.DefineMethod("M2");
                m2Builer.ReturnType = "string";
                m2Builer.Parameters.Add(new ParameterBuilder { ParameterType = "string", Name = "arg" });
            }

            {
                InterfaceBuilder interface2 = nsBuilder.DefineInterface("Interface2");
                interface2.FrontModifiers.Add("public");
                interface2.Interfaces.Add("Interface1");

                var m3Builder = interface2.DefineMethod("M3");
                m3Builder.ReturnType = "int";
                m3Builder.Parameters.Add(new ParameterBuilder { ParameterType = "string", Name = "arg" });
            }

            {
                InterfaceBuilder interface3 = nsBuilder.DefineInterface("Interface3");
                interface3.FrontModifiers.Add("public");
                interface3.Interfaces.AddRange(new[] { "Interface1", "IEnumerable<string>" });

                var m4Builder = interface3.DefineMethod("M4");
                m4Builder.ReturnType = "Guid";
            }

            {
                InterfaceBuilder interface4 = nsBuilder.DefineInterface("Interface4");
                interface4.FrontModifiers.Add("public");

                var m4Builder = interface4.DefineMethod("M5");
                m4Builder.ReturnType = "DateTime";
            }

            {
                InterfaceBuilder interface5 = nsBuilder.DefineInterface("Interface5");
                interface5.FrontModifiers.Add("public");

                interface5.DefineProperty("int", "P1");
            }

            {
                ClassBuilder class1 = nsBuilder.DefineClass("Class1");
                class1.FrontModifiers.AddRange(new[] { "public", "abstract" });
                class1.Interfaces.Add("Interface4");

                class1.DefineField("int", "_f1");

                ConstructorBuilder ctorBuilder = class1.DefineConstructor( "protected" );
                ctorBuilder.Parameters.Add(new ParameterBuilder { ParameterType = "int", Name = "arg" });
                ctorBuilder.Body.Append("_f1 = arg;");

                MethodBuilder m5Builder = class1.DefineMethod("M5");
                m5Builder.FrontModifiers.AddRange(new[] { "public", "abstract" });
                m5Builder.ReturnType = "DateTime";

                PropertyBuilder p1Builder = class1.DefineProperty("int", "P1");
                p1Builder.FrontModifiers.Add("public");
                p1Builder.GetMethod.Body.Append("return _f1;");
                p1Builder.SetMethod.FrontModifier = "protected";
                p1Builder.SetMethod.Body.Append("_f1 = value;");
            }

            {
                ClassBuilder class2 = nsBuilder.DefineClass("Class2<T1,T2>");
                class2.FrontModifiers.AddRange(new[] { "public", "sealed" });
                class2.BaseType = "Class1";
                class2.Interfaces.AddRange(new[] { "Interface3", "Interface5" });

                GenericConstraint gc1 = new GenericConstraint { GenericParameterName = "T1" };
                gc1.Constraints.Add("class");
                GenericConstraint gc2 = new GenericConstraint { GenericParameterName = "T2" };
                gc2.Constraints.AddRange(new[] { "struct", "Interface5" });
                class2.GenericConstraints.AddRange(new[] { gc1, gc2 });

                FieldBuilder f2Builder = class2.DefineField("string", "_f2");
                f2Builder.FrontModifiers.Add("readonly");

                FieldBuilder f3Builder = class2.DefineField("T1", "_f3");
                f3Builder.FrontModifiers.Add("static");
                f3Builder.InitialValue = "null";

                FieldBuilder f4Builder = class2.DefineField("T2", "_f4");
                f4Builder.InitialValue = "new T2()";

                ConstructorBuilder ctorBuilder = class2.DefineConstructor();
                ctorBuilder.Parameters.AddRange(new[]
                {
                    new ParameterBuilder { ParameterType = "int", Name = "arg1"},
                    new ParameterBuilder { ParameterType = "string", Name = "arg2", DefaultValue = "null" }
                });
                ctorBuilder.Initializer = "base(arg1)";
                ctorBuilder.Body.Append("_f2 = arg2;");

                MethodBuilder getEnumeratorBuilder = class2.DefineMethod("GetEnumerator");
                getEnumeratorBuilder.FrontModifiers.Add("public");
                getEnumeratorBuilder.ReturnType = "IEnumerator<string>";
                getEnumeratorBuilder.Body.Append("for (int i = 0; i < P1; i++) yield return i.ToString();");

                MethodBuilder m1Builder = class2.DefineMethod("M1");
                m1Builder.FrontModifiers.Add("public");
                m1Builder.ReturnType = "Struct1";
                m1Builder.Parameters.AddRange(new[]
                {
                    new ParameterBuilder { ParameterType = "int", Name = "arg1" },
                    new ParameterBuilder { ParameterType = "Enum1", Name = "arg2" }
                });
                m1Builder.Body
                    .Append("int x;")
                    .Append("if (arg2 == Enum1.None) x = 0;")
                    .Append("else if (arg2 == Enum1.Case1) x = 1;")
                    .Append("else x = 2;")
                    .Append("return new Struct1(x, arg1);");

                MethodBuilder m2Builder = class2.DefineMethod("M2");
                m2Builder.FrontModifiers.Add("public");
                m2Builder.ReturnType = "string";
                m2Builder.Parameters.Add(new ParameterBuilder { ParameterType = "string", Name = "s", DefaultValue = "null" });
                m2Builder.Body
                    .Append("if (s == null) s = string.Empty;")
                    .Append("return string.Concat(s, s);");

                MethodBuilder m3Builder = class2.DefineMethod("M3");
                m3Builder.FrontModifiers.Add("public");
                m3Builder.ReturnType = "int";
                m3Builder.Parameters.Add(new ParameterBuilder { ParameterType = "string", Name = "s" });
                m3Builder.Body.Append("=> int.Parse(s)");

                MethodBuilder m4Builder = class2.DefineMethod("M4");
                m4Builder.FrontModifiers.Add("public");
                m4Builder.ReturnType = "Guid";
                m4Builder.Body.Append("return Guid.NewGuid();");

                MethodBuilder m5Builder = class2.DefineMethod("M5");
                m5Builder.FrontModifiers.AddRange(new[] { "public", "override" });
                m5Builder.ReturnType = "DateTime";
                m5Builder.Body.Append("return DateTime.UtcNow;");

                getEnumeratorBuilder = class2.DefineMethod("IEnumerable.GetEnumerator");
                getEnumeratorBuilder.ReturnType = "IEnumerator";
                getEnumeratorBuilder.Body.Append("return GetEnumerator();");

                PropertyBuilder p1Property = class2.DefineProperty("int", "Interface5.P1");
                p1Property.GetMethod.Body.Append("=> P1");
                p1Property.SetMethod.Body.Append("=> P1 = value");

                AutoImplmentedPropertyBuilder p2Property = class2.DefineAutoImplementedProperty("int", "P2");
                p2Property.FrontModifiers.Add("public");
                p2Property.Setter.Exists = false;
                p2Property.InitialValue = "25";

                MethodBuilder m6Builder = class2.DefineMethod("M6<T>");
                m6Builder.FrontModifiers.AddRange(new[] { "public", "static" });
                m6Builder.ReturnType = "int";
                m6Builder.Parameters.Add(new ParameterBuilder { ParameterType = "T", Name = "arg" });
                GenericConstraint gc3 = new GenericConstraint { GenericParameterName = "T" };
                gc3.Constraints.Add("Interface5");
                m6Builder.GenericConstraints.Add(gc3);
                m6Builder.Body.Append("return arg.P1;");

                PropertyBuilder p3Property = class2.DefineProperty("T1", "P3");
                p3Property.FrontModifiers.AddRange(new[] { "public", "static" });
                p3Property.GetMethod.Body.Append("return _f3;");
                p3Property.SetMethod.Body.Append("if(value != null && value != _f3) _f3 = value;");

                PropertyBuilder p4Property = class2.DefineProperty("T2", "P4");
                p4Property.FrontModifiers.Add("public");
                p4Property.GetMethod.Body.Append("=> _f4.P1 > 15 ? _f4 : new T2()");
                p4Property.SetMethod.FrontModifier = "internal";
                p4Property.SetMethod.Body
                    .Append("Debug.Assert(value.P1 != 0);")
                    .Append("_f4 = value;");

                AutoImplmentedPropertyBuilder p5Property = class2.DefineAutoImplementedProperty("int", "P5");
                p5Property.FrontModifiers.Add("public");
            }

            return nsBuilder;
        }

        string RandomDllName() => string.Format("Test-{0}", Guid.NewGuid().ToString().Substring(0, 8));
    }
}
