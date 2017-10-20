using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace CK.CodeGen.Abstractions.Tests
{
    public enum Enum1
    {
        None = 0,
        Case1 = 1,
        Case2
    }

    public struct Struct1
    {
        private readonly int _x;
        private readonly int _y;

        public Struct1( int x, int y )
        {
            _x = x;
            _y = y;
        }

        public int X => _x;

        public int Y => _y;

        public override bool Equals( object obj )
        {
            if( !(obj is Struct1) ) return false;
            Struct1 other = (Struct1)obj;
            return _x == other._x
                && _y == other._y;
        }

        public override int GetHashCode()
        {
            return _x.GetHashCode() ^ _y.GetHashCode();
        }
    }

    public interface Interface1
    {
        Struct1 M1( int arg1, Enum1 arg2 );

        string M2( string arg );
    }

    public interface Interface2 : Interface1
    {
        int M3( string arg );
    }

    public interface Interface3 : Interface2, IEnumerable<string>
    {
        Guid M4();
    }

    public interface Interface4
    {
        DateTime M5();
    }

    public interface Interface5
    {
        int P1 { get; set; }
    }

    public abstract class Class1 : Interface4
    {
        int _f1;

        protected Class1( int arg )
        {
            _f1 = arg;
        }

        public abstract DateTime M5();

        public int P1
        {
            get { return _f1; }
            protected set { _f1 = value; }
        }
    }

    public sealed class Class2<T1, T2> : Class1, Interface3, Interface5
        where T1 : class
        where T2 : struct, Interface5
    {
        readonly string _f2;
        static T1 _f3 = null;
        T2 _f4 = new T2();

        public Class2( int arg1, string arg2 = null )
            : base( arg1 )
        {
            _f2 = arg2;
        }

        public IEnumerator<string> GetEnumerator()
        {
            for( int i = 0; i < P1; i++ ) yield return i.ToString();
        }

        public Struct1 M1( int arg1, Enum1 arg2 )
        {
            int x;
            if( arg2 == Enum1.None ) x = 0;
            else if( arg2 == Enum1.Case1 ) x = 1;
            else x = 2;
            return new Struct1( x, arg1 );
        }

        public string M2( string s = null )
        {
            if( s == null ) s = string.Empty;
            return string.Concat( s, s );
        }

        public int M3( string s ) => int.Parse( s );

        public Guid M4()
        {
            return Guid.NewGuid();
        }

        public override DateTime M5()
        {
            return DateTime.UtcNow;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        int Interface5.P1 { get => P1; set => P1 = value; }

        public int P2 { get; } = 25;

        public static int M6<T>( T arg ) where T : Interface5
        {
            return arg.P1;
        }

        public static T1 P3
        {
            get
            {
                return _f3;
            }
            set
            {
                if( value != null && value != _f3 ) _f3 = value;
            }
        }

        public T2 P4
        {
            get => _f4.P1 > 15 ? _f4 : new T2();
            internal set
            {
                Debug.Assert( value.P1 != 0 );
                _f4 = value;
            }
        }

        public int P5 { get; set; }
    }
}
