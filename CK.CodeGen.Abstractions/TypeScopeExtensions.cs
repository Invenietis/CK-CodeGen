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
        /// Appends all (or filtered set) constructors from a type (that should be the base type)
        /// to this type wich simply relay the call to the base class.
        /// </summary>
        /// <param name="this">This type scope.</param>
        /// <param name="baseType">The base type.</param>
        /// <param name="accessBuilder">
        /// Optional filter (returning null skips the constructor) and
        /// access protection builder. The default acces protection is "public ".
        /// </param>
        /// <returns>This type scope to enable fluent syntax.</returns>
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
                @this.Append( Helper.RemoveGenericParameters( @this.Name ) )
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
            Helper.CheckIsOverridable( method );
            Helper.DoAppendSignature( @this, AccessProtectionOption.ThrowOnPureInternal, "override ", method );
            return @this;
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
            Helper.CheckIsOverridable( method );
            Helper.DoAppendSignature( @this, AccessProtectionOption.ThrowOnPureInternal, "override sealed ", method );
            return @this;
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
            Helper.DoAppendSignature( @this, access, null, method );
            return @this;
        }

        /// <summary>
        /// Creates an overridden method.
        /// The <paramref name="method"/> must be virtual (not static nor sealed) and not purely internal.
        /// </summary>
        /// <param name="this">This type scope.</param>
        /// <param name="method">The method description.</param>
        /// <returns>The newly created function scope.</returns>
        public static IFunctionScope CreateOverride( this ITypeScope @this, MethodInfo method )
        {
            Helper.CheckIsOverridable( method );
            return @this.CreateFunction( h => h.DoAppendSignature( AccessProtectionOption.ThrowOnPureInternal, "override ", method ) );
        }

        /// <summary>
        /// Creates a sealed overridden method.
        /// The <paramref name="method"/> must be virtual (not static nor sealed) and not purely internal.
        /// </summary>
        /// <param name="this">This type scope.</param>
        /// <param name="method">The method description.</param>
        /// <returns>The newly created function scope.</returns>
        public static IFunctionScope CreateSealedOverride( this ITypeScope @this, MethodInfo method )
        {
            Helper.CheckIsOverridable( method );
            return @this.CreateFunction( h => h.DoAppendSignature( AccessProtectionOption.ThrowOnPureInternal, "sealed override ", method ) );
        }

    }
}
