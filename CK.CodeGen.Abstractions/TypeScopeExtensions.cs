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
        /// <summary>
        /// Appends the method signature with an "override " modifier, adapting the
        /// original <paramref name="method"/> acces protection.
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
        /// original <paramref name="method"/> acces protection.
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
        /// <param name="protection">Access protection option.</param>
        /// <returns>This type scope to enable fluent syntax.</returns>
        public static ITypeScope AppendSignature( this ITypeScope @this, MethodInfo method, AccessProtectionOption protection = AccessProtectionOption.All )
        {
            return DoAppendSignature( @this, protection, null, method );
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
            @this.RawAppend( frontModifier )
                 .AppendCSharpName( method.ReturnType )
                 .AppendWhiteSpace()
                 .RawAppend( name )
                 .AddParameters( method );
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
            if( method.IsPublic ) w.RawAppend( "public " );
            else if( method.IsFamily ) w.RawAppend( "protected " );
            else if( method.IsAssembly )
            {
                if( p == AccessProtectionOption.All )
                {
                    w.RawAppend( "internal " );
                }
            }
            else if( method.IsFamilyAndAssembly )
            {
                if( p == AccessProtectionOption.All )
                {
                    w.RawAppend( "private protected " ); 
                }
                else w.RawAppend( "protected " );
            }
            else if( method.IsFamilyOrAssembly )
            {
                if( p == AccessProtectionOption.All )
                {
                    w.RawAppend( "internal protected " );
                }
                else w.RawAppend( "protected " );
            }
            return w;
        }

        static ITypeScope AddParameters( this ITypeScope @this, MethodInfo baseMethod )
        {
            @this.RawAppend( "(" );
            bool isFirstParameter = true;
            foreach( var p in baseMethod.GetParameters() )
            {
                @this.AppendWhiteSpace().AddParameter( p );
                if( isFirstParameter ) isFirstParameter = false;
                else @this.RawAppend( "," );
            }
            return @this.RawAppend( " )" );
        }

        static ITypeScope AddParameter( this ITypeScope @this, ParameterInfo p )
        {
            if( p.IsOut ) @this.RawAppend( "out " );
            else if( p.ParameterType.IsByRef ) @this.RawAppend( "ref " );
            Type parameterType = p.ParameterType.IsByRef ? p.ParameterType.GetElementType() : p.ParameterType;
            return @this.AppendCSharpName( parameterType, true )
                        .AppendWhiteSpace()
                        .RawAppend( p.Name );
        }

        static void CheckIsOverridable( MethodInfo method )
        {
            if( method == null ) throw new ArgumentNullException( nameof( method ) );
            if( !method.IsVirtual || method.IsStatic || method.IsFinal )
                throw new ArgumentException( $"Method {method} is not overridable.", nameof( method ) );
        }

    }
}
