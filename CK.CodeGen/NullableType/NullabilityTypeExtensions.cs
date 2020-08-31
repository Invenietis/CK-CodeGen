using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CK.CodeGen
{
    /// <summary>
    /// Extends Type with helper methods.
    /// </summary>
    public static class NullabilityTypeExtensions
    {
        /// <summary>
        /// Gets the <see cref="NullabilityTypeKind"/> for a type.
        /// A reference type cannot be, by itself, a <see cref="NullabilityTypeKind.NonNullableReferenceType"/>: this
        /// method always return <see cref="NullabilityTypeKind.NullableReferenceType"/> or <see cref="NullabilityTypeKind.NullableGenericReferenceType"/> for
        /// classes and interfaces.
        /// </summary>
        /// <param name="this">This type.</param>
        /// <remarks>
        /// <para>
        /// <c>typeof(List&lt;string?&gt;)</c> is valid and type but <c>typeof(List&lt;string?&gt;?)</c>
        /// cannot compile and this makes sense: the "outer", "root" nullability depends on the usage of the type: non nullable reference types can be obtained
        /// via a <see cref="ParameterInfo"/> or a <see cref="PropertyInfo"/> that "references" their type.
        /// <para>
        /// </para>
        /// However, <c>typeof(List&lt;string?&gt;)</c> could have been a <see cref="NullabilityTypeKind.NRTFullNullable"/>, but it is not, it is actually
        /// oblivious to nullable: both <c>typeof(List&lt;string?&gt;)</c> and <c>typeof(List&lt;string&gt;)</c> are marked with with a single 0 byte.
        /// </para>
        /// </remarks>
        /// <returns>The nullability kind.</returns>
        public static NullabilityTypeKind GetNullabilityKind( this Type @this )
        {
            if( @this.IsInterface )
            {
                return @this.IsGenericType ? NullabilityTypeKind.NullableGenericReferenceType : NullabilityTypeKind.NullableReferenceType;
            }
            if( @this.IsClass )
            {
                if( @this.IsGenericType ) return NullabilityTypeKind.NullableGenericReferenceType;
                return NullabilityTypeKind.NullableReferenceType;
            }
            if( @this.IsValueType )
            {
                Type inner;
                if( @this.IsGenericType && (inner = Nullable.GetUnderlyingType( @this )) != null )
                {
                    if( !inner.IsGenericType ) return NullabilityTypeKind.NullableValueType;
                    return inner.IsValueTuple() ? NullabilityTypeKind.NullableTupleType : NullabilityTypeKind.NullableGenericValueType;
                }
                if( !@this.IsGenericType ) return NullabilityTypeKind.NonNullableValueType;
                return @this.IsValueTuple() ? NullabilityTypeKind.NonNullableTupleType : NullabilityTypeKind.NonNullableGenericValueType;
            }
            throw new Exception( $"What's this type that is not an interface, a class or a value type?: {@this.AssemblyQualifiedName}" );
        }

        /// <summary>
        /// Gets the <see cref="NullabilityTypeInfo"/> for a parameter.
        /// <param name="this">This parameter.</param>
        /// </summary>
        /// <returns>The nullability info for the parameter.</returns>
        [DebuggerStepThrough]
        public static NullabilityTypeInfo GetNullabilityInfo( this ParameterInfo @this )
        {
            return GetNullabilityInfo( @this.ParameterType, @this.Member, @this.GetCustomAttributesData(), () => $" parameter '{@this.Name}' of {@this.Member.DeclaringType}.{@this.Member.Name}." );
        }

        /// <summary>
        /// Creates a <see cref="NullableTypeTree"/> for a parameter's type.
        /// The type mus not be a nested type (its <see cref="Type.DeclaringType"/> must be null) otherwise
        /// an <see cref="ArgumentException"/> is thrown.
        /// </summary>
        /// <param name="this">This parameter.</param>
        /// <returns>The nullable tree for the parameter' type.</returns>
        [DebuggerStepThrough]
        public static NullableTypeTree GetNullableTypeInfo( this ParameterInfo @this )
        {
            var info = GetNullabilityInfo( @this );
            return GetNullableTypeTree( @this.ParameterType, info );
        }

        /// <summary>
        /// Gets the <see cref="NullabilityTypeInfo"/> for a property.
        /// </summary>
        /// <param name="this">This property.</param>
        /// <returns>The nullability info for the parameter.</returns>
        [DebuggerStepThrough]
        public static NullabilityTypeInfo GetNullabilityInfo( this PropertyInfo @this )
        {
            return GetNullabilityInfo( @this.PropertyType, @this.DeclaringType, @this.GetCustomAttributesData(), () => $" property '{@this.Name}' of {@this.DeclaringType}." );
        }

        /// <summary>
        /// Creates a <see cref="NullableTypeTree"/> for a property's type.
        /// The type mus not be a nested type (its <see cref="Type.DeclaringType"/> must be null) otherwise
        /// an <see cref="ArgumentException"/> is thrown.
        /// </summary>
        /// <param name="this">This property.</param>
        /// <returns>The nullable tree for the property's type.</returns>
        [DebuggerStepThrough]
        public static NullableTypeTree GetNullableTypeInfo( this PropertyInfo @this )
        {
            var info = GetNullabilityInfo( @this );
            return GetNullableTypeTree( @this.PropertyType, info );
        }

        /// <summary>
        /// Gets the <see cref="NullabilityTypeInfo"/> for a field.
        /// </summary>
        /// <param name="this">This field.</param>
        /// <returns>The nullability info for the field.</returns>
        [DebuggerStepThrough]
        public static NullabilityTypeInfo GetNullabilityInfo( this FieldInfo @this )
        {
            return GetNullabilityInfo( @this.FieldType, @this.DeclaringType, @this.GetCustomAttributesData(), () => $" field '{@this.Name}' of {@this.DeclaringType}." );
        }

        /// <summary>
        /// Creates a <see cref="NullableTypeTree"/> for a field's type.
        /// The type mus not be a nested type (its <see cref="Type.DeclaringType"/> must be null) otherwise
        /// an <see cref="ArgumentException"/> is thrown.
        /// </summary>
        /// <param name="this">This field.</param>
        /// <returns>The nullable tree for the fields's type.</returns>
        [DebuggerStepThrough]
        public static NullableTypeTree GetNullableTypeInfo( this FieldInfo @this )
        {
            var info = GetNullabilityInfo( @this );
            return GetNullableTypeTree( @this.FieldType, info );
        }

        /// <summary>
        /// Creates a <see cref="NullableTypeTree"/> for this type based on a root <see cref="NullabilityTypeInfo"/> that must
        /// have been computed for this type otherwise behavior is undefined. 
        /// </summary>
        /// <param name="this">This type.</param>
        /// <param name="info">The nullability info.</param>
        /// <returns>The detailed, recursive, <see cref="NullableTypeTree"/>.</returns>
        [DebuggerStepThrough]
        public static NullableTypeTree GetNullableTypeTree( this Type @this, NullabilityTypeInfo info )
        {
            return GetNullableTypeTree( @this, info.GenerateAnnotations().GetEnumerator(), info.Kind );
        }

        static NullableTypeTree GetNullableTypeTree( Type t, IEnumerator<byte> annotations, NullabilityTypeKind known )
        {
            if( t.DeclaringType != null && t.DeclaringType.IsGenericType ) throw new ArgumentException( $"Type '{t.Name}' is nested in a generic type ({t.DeclaringType.ToCSharpName()}). Only nested types in non generic types are supported.", nameof( t ) );
            NullableTypeTree[] sub = Array.Empty<NullableTypeTree>();
            bool isInside;
            if( isInside = (known == NullabilityTypeKind.Unknown) )
            {
                known = t.GetNullabilityKind();
            }
            if( known.IsNullableValueType() )
            {
                // Lift the Nullable<T>.
                t = Nullable.GetUnderlyingType( t )!;
            }
            if( isInside && !known.IsNonGenericValueType() )
            {
                // Consume our annotation.
                if( !annotations.MoveNext() )
                {
                    throw new InvalidOperationException( $"Byte annotations too short." );
                }
                var thisOne = annotations.Current;
                // Annotations only apply to reference types. Only 1 (not null!) is of interest.
                if( thisOne == 1 && known.IsReferenceType() )
                {
                    Debug.Assert( known.IsNullable() );
                    known &= ~NullabilityTypeKind.IsNullable;
                }
            }
            if( t.HasElementType )
            {
                Debug.Assert( known.IsReferenceType() );
                sub = new[] { GetNullableTypeTree( t.GetElementType()!, annotations, default ) };
            }
            else if( t.IsGenericType )
            {
                Debug.Assert( (known & NullabilityTypeKind.IsGenericType) != 0, "This has been already computed." );
                var genArgs = t.GetGenericArguments();
                sub = new NullableTypeTree[genArgs.Length];
                int idx = 0;
                foreach( var g in genArgs )
                {
                    sub[idx] = GetNullableTypeTree( g, annotations, default );
                    ++idx;
                }
            }
            return new NullableTypeTree( t, known, sub );
        }

        static NullabilityTypeInfo GetNullabilityInfo( Type t, MemberInfo? parent, IEnumerable<CustomAttributeData> attributes, Func<string> locationForError )
        {
            byte[]? profile = null;
            var n = GetNullabilityKind( t );
            if( !n.IsNonGenericValueType() )
            {
                var a = attributes.FirstOrDefault( a => a.AttributeType.Name == "NullableAttribute" && a.AttributeType.Namespace == "System.Runtime.CompilerServices" );
                if( a == null )
                {
                    while( parent != null )
                    {
                        a = parent.GetCustomAttributesData().FirstOrDefault( a => a.AttributeType.Name == "NullableContextAttribute" && a.AttributeType.Namespace == "System.Runtime.CompilerServices" );
                        if( a != null )
                        {
                            n = HandleByte( locationForError, n, (byte)a.ConstructorArguments[0].Value! );
                            break;
                        }
                        parent = parent.DeclaringType;
                    }
                }
                else
                {
                    object? data = a.ConstructorArguments[0].Value;
                    // A single value means "apply to everything in the type", e.g. 1 for Dictionary<string, string>, 2 for Dictionary<string?, string?>?
                    if( data is byte b )
                    {
                        n = HandleByte( locationForError, n, b );
                    }
                    else if( data is System.Collections.ObjectModel.ReadOnlyCollection<CustomAttributeTypedArgument> arguments )
                    {
                        var firstByte = (byte)arguments[0].Value;
                        // Complex nullability marker.
                        n |= NullabilityTypeKind.NRTFullNullable | NullabilityTypeKind.NRTFullNonNullable;
                        // Quick check.
                        if( firstByte != 0 && (n & NullabilityTypeKind.IsValueType) != 0 )
                        {
                            throw new Exception( $"First byte annotation is {firstByte} but the type is a ValueType." );
                        }
                        // Apply the first byte for this information.
                        if( firstByte == 1 ) n &= ~NullabilityTypeKind.IsNullable;

                        profile = new byte[arguments.Count - 1];
                        for( int i = 0; i < profile.Length; ++i )
                        {
                            profile[i] = (byte)arguments[i + 1].Value;
                        }
                        if( profile.Length == 0 )
                        {
                            throw new Exception( $"Mono byte annotation array found." );
                        }
                    }
                    else
                    {
                        throw new Exception( $"Invalid data type '{data?.GetType()}' in NullableAttribute for {locationForError()}." );
                    }
                }
            }
            return new NullabilityTypeInfo( n, profile );

            static NullabilityTypeKind HandleByte( Func<string> locationForError, NullabilityTypeKind n, byte b )
            {
                if( b == 1 )
                {
                    n &= ~NullabilityTypeKind.IsNullable;
                    n |= NullabilityTypeKind.NRTFullNonNullable;
                }
                else if( b == 2 )
                {
                    n |= NullabilityTypeKind.NRTFullNullable;
                }
                else
                {
                    return n;
                }
                if( (n & NullabilityTypeKind.IsValueType) != 0 )
                {
                    throw new Exception( $"Single byte annotation is {b} but the type is a ValueType." );
                }
                return n;
            }
        }

    }
}
