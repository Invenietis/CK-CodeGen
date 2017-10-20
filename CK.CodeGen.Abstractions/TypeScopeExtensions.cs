using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CK.CodeGen.Abstractions
{
    /// <summary>
    /// Provides useful extension methods to <see cref="ITypeScope"/>.
    /// </summary>
    public static class TypeScopeExtensions
    {

        public static ITypeScope AppendPassThroughConstructors( this ITypeScope @this, Type baseType, Func<ConstructorInfo, string> accessBuilder = null )
        {
            foreach( var c in baseType.GetConstructors( BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )
                                      .Where( c => c.IsPublic || c.IsFamily || c.IsFamilyOrAssembly ) )
            {
                string access = "public ";
                if( accessBuilder != null && (access = accessBuilder( c )) == null ) continue;
                if( access.Length > 0 )
                {
                    @this.Append( access );
                    if( !Char.IsWhiteSpace( access, access.Length - 1 ) ) @this.Space();
                }
                var parameters = c.GetParameters();
                @this.Append( RemoveGenericParameters( @this.Name ) )
                     .AppendParameters( parameters );

                if( parameters.Length > 0 )
                {
                    @this.Append( " : base( " );
                    bool isFirst = true;
                    foreach( var p in parameters )
                    {
                        @this.Append( p.Name );
                        if( isFirst ) isFirst = false;
                        else @this.Append( ", " );
                    }
                    @this.Append( " )" );
                }
                @this.Append( "{}" ).NewLine();
            }
            return @this;
        }


        /// <summary>
        /// Appends the method signature with an "override " modifier, adapting the
        /// original <paramref name="method"/> access protection.
        /// The method must be virtual (not static nor sealed) and not purely internal.
        /// This does not open any body for the method nor adds a ; terminator.
        /// </summary>
        /// <param name="this">This type scope.</param>
        /// <param name="method">The method description (from the base class).</param>
        /// <returns>This type scope to enable fluent syntax.</returns>
        public static ITypeScope AppendOverrideSignature( this ITypeScope @this, MethodInfo method )
        {
            CheckIsOverridable( method );
            return DoAppendSignature( @this, AccessProtectionOption.ThrowOnPureInternal, "override ", method );
        }

        /// <summary>
        /// Appends the method signature with an "override sealed " modifier, adapting the
        /// original <paramref name="method"/> access protection.
        /// The method must be virtual (not static nor sealed) and not purely internal.
        /// This does not open any body for the method nor adds a ; terminator.
        /// </summary>
        /// <param name="this">This type scope.</param>
        /// <param name="method">The method description (from the base class).</param>
        /// <returns>This type scope to enable fluent syntax.</returns>
        public static ITypeScope AppendSealedOverrideSignature( this ITypeScope @this, MethodInfo method )
        {
            CheckIsOverridable( method );
            return DoAppendSignature( @this, AccessProtectionOption.ThrowOnPureInternal, "override sealed ", method );
        }

        /// <summary>
        /// Appends the method signature.
        /// This does not open any body for the method nor adds a ; terminator.
        /// </summary>
        /// <param name="this">This type scope.</param>
        /// <param name="method">The method description.</param>
        /// <param name="access">Access protection option.</param>
        /// <returns>This type scope to enable fluent syntax.</returns>
        public static ITypeScope AppendSignature( this ITypeScope @this, MethodInfo method, AccessProtectionOption access = AccessProtectionOption.All )
        {
            return DoAppendSignature( @this, access, null, method );
        }

        static ITypeScope DoAppendSignature(
            this ITypeScope @this,
            AccessProtectionOption protection,
            string frontModifier,
            MethodInfo method )
        {
            if( method == null ) throw new ArgumentNullException( nameof( method ) );
            string name = method.Name;
            if( method.ContainsGenericParameters )
            {
                name += '<';
                name += String.Join( ",", method.GetGenericArguments().Select( a => a.Name ) );
                name += '>';
            }
            if( protection != AccessProtectionOption.None ) @this.AppendAccessProtection( method, protection );
            @this.Append( frontModifier )
                 .AppendCSharpName( method.ReturnType )
                 .Space()
                 .Append( name )
                 .AppendParameters( method.GetParameters() );
            return @this;
        }

        static ITypeScope AppendAccessProtection( this ITypeScope w, MethodInfo method, AccessProtectionOption p )
        {
            Debug.Assert( p != AccessProtectionOption.None );
            if( p == AccessProtectionOption.ThrowOnPureInternal
                && (method.IsAssembly || method.IsFamilyAndAssembly) )
            {
                throw new ArgumentException( $"Method {method} must not be internal.", nameof( method ) );
            }
            if( method.IsPublic ) w.Append( "public " );
            else if( method.IsFamily ) w.Append( "protected " );
            else if( method.IsAssembly )
            {
                if( p == AccessProtectionOption.All )
                {
                    w.Append( "internal " );
                }
            }
            else if( method.IsFamilyAndAssembly )
            {
                if( p == AccessProtectionOption.All )
                {
                    w.Append( "private protected " ); 
                }
                else w.Append( "protected " );
            }
            else if( method.IsFamilyOrAssembly )
            {
                if( p == AccessProtectionOption.All )
                {
                    w.Append( "internal protected " );
                }
                else w.Append( "protected " );
            }
            return w;
        }

        static ITypeScope AppendParameters( this ITypeScope @this, IReadOnlyList<ParameterInfo> parameters )
        {
            if( parameters.Count == 0 ) return @this.Append( "()" );
            @this.Append( "( " );
            bool isFirstParameter = true;
            foreach( var p in parameters )
            {
                if( isFirstParameter ) isFirstParameter = false;
                else @this.Append( ", " );
                @this.AddParameter( p );
            }
            return @this.Append( " )" );
        }

        static ITypeScope AddParameter( this ITypeScope @this, ParameterInfo p )
        {
            if( p.IsOut ) @this.Append( "out " );
            else if( p.ParameterType.IsByRef ) @this.Append( "ref " );
            Type parameterType = p.ParameterType.IsByRef ? p.ParameterType.GetElementType() : p.ParameterType;
            return @this.AppendCSharpName( parameterType, true )
                        .Space()
                        .Append( p.Name );
        }

        static void CheckIsOverridable( MethodInfo method )
        {
            if( method == null ) throw new ArgumentNullException( nameof( method ) );
            if( !method.IsVirtual || method.IsStatic || method.IsFinal )
                throw new ArgumentException( $"Method {method} is not overridable.", nameof( method ) );
        }

        static string RemoveGenericParameters( string typeName )
        {
            int idx = typeName.IndexOf( '<' );
            return idx < 0 ? typeName : typeName.Substring( idx );
        }


    }
}
