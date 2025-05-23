using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace System.Reflection;


/// <summary>
/// Waiting for .Net7...
/// A class that represents nullability info
/// </summary>
public sealed class TEMPNullabilityInfo
{
    internal TEMPNullabilityInfo( Type type,
                                  NullabilityState readState,
                                  NullabilityState writeState,
                                  TEMPNullabilityInfo? elementType,
                                  TEMPNullabilityInfo[] typeArguments )
    {
        Type = type;
        ReadState = readState;
        WriteState = writeState;
        ElementType = elementType;
        GenericTypeArguments = typeArguments;
    }

    /// <summary>
    /// The <see cref="System.Type" /> of the member or generic parameter
    /// to which this NullabilityInfo belongs
    /// </summary>
    public Type Type { get; }
    /// <summary>
    /// The nullability read state of the member
    /// </summary>
    public NullabilityState ReadState { get; internal set; }
    /// <summary>
    /// The nullability write state of the member
    /// </summary>
    public NullabilityState WriteState { get; internal set; }
    /// <summary>
    /// If the member type is an array, gives the <see cref="NullabilityInfo" /> of the elements of the array, null otherwise
    /// </summary>
    public TEMPNullabilityInfo? ElementType { get; }
    /// <summary>
    /// If the member type is a generic type, gives the array of <see cref="NullabilityInfo" /> for each type parameter
    /// </summary>
    public TEMPNullabilityInfo[] GenericTypeArguments { get; }
}

/// <summary>
/// Waiting for .Net7...
/// Provides APIs for populating nullability information/context from reflection members:
/// <see cref="ParameterInfo"/>, <see cref="FieldInfo"/>, <see cref="PropertyInfo"/> and <see cref="EventInfo"/>.
/// </summary>
public sealed class TEMPNullabilityInfoContext
{
    const string CompilerServicesNameSpace = "System.Runtime.CompilerServices";
    readonly Dictionary<Module, NotAnnotatedStatus> _publicOnlyModules = new();
    readonly Dictionary<MemberInfo, NullabilityState> _context = new();

    internal static bool IsSupported { get; } =
        AppContext.TryGetSwitch( "System.Reflection.NullabilityInfoContext.IsSupported", out bool isSupported ) ? isSupported : true;

    [Flags]
    enum NotAnnotatedStatus
    {
        None = 0x0,    // no restriction, all members annotated
        Private = 0x1, // members not annotated
        Internal = 0x2 // internal members not annotated
    }

    NullabilityState? GetNullableContext( MemberInfo? memberInfo )
    {
        while( memberInfo != null )
        {
            if( _context.TryGetValue( memberInfo, out NullabilityState state ) )
            {
                return state;
            }

            foreach( CustomAttributeData attribute in memberInfo.GetCustomAttributesData() )
            {
                if( attribute.AttributeType.Name == "NullableContextAttribute" &&
                    attribute.AttributeType.Namespace == CompilerServicesNameSpace &&
                    attribute.ConstructorArguments.Count == 1 )
                {
                    state = TranslateByte( attribute.ConstructorArguments[0].Value );
                    _context.Add( memberInfo, state );
                    return state;
                }
            }

            memberInfo = memberInfo.DeclaringType;
        }

        return null;
    }

    /// <summary>
    /// Populates <see cref="TEMPNullabilityInfo" /> for the given <see cref="ParameterInfo" />.
    /// If the nullablePublicOnly feature is set for an assembly, like it does in .NET SDK, the and/or internal member's
    /// nullability attributes are omitted, in this case the API will return NullabilityState.Unknown state.
    /// </summary>
    /// <param name="parameterInfo">The parameter which nullability info gets populated</param>
    /// <exception cref="ArgumentNullException">If the parameterInfo parameter is null</exception>
    /// <returns><see cref="TEMPNullabilityInfo" /></returns>
    public TEMPNullabilityInfo Create( ParameterInfo parameterInfo )
    {
        ArgumentNullException.ThrowIfNull( parameterInfo );

        EnsureIsSupported();

        IList<CustomAttributeData> attributes = parameterInfo.GetCustomAttributesData();
        NullableAttributeStateParser parser = parameterInfo.Member is MethodBase method && IsPrivateOrInternalMethodAndAnnotationDisabled( method )
            ? NullableAttributeStateParser.Unknown
            : CreateParser( attributes );
        TEMPNullabilityInfo nullability = GetNullabilityInfo( parameterInfo.Member, parameterInfo.ParameterType, parser );

        if( nullability.ReadState != NullabilityState.Unknown )
        {
            CheckParameterMetadataType( parameterInfo, nullability );
        }

        CheckNullabilityAttributes( nullability, attributes );
        return nullability;
    }

    void CheckParameterMetadataType( ParameterInfo parameter, TEMPNullabilityInfo nullability )
    {
        if( parameter.Member is MethodInfo method )
        {
            MethodInfo metaMethod = GetMethodMetadataDefinition( method );
            ParameterInfo? metaParameter = null;
            if( string.IsNullOrEmpty( parameter.Name ) )
            {
                metaParameter = metaMethod.ReturnParameter;
            }
            else
            {
                ParameterInfo[] parameters = metaMethod.GetParameters();
                for( int i = 0; i < parameters.Length; i++ )
                {
                    if( parameter.Position == i &&
                        parameter.Name == parameters[i].Name )
                    {
                        metaParameter = parameters[i];
                        break;
                    }
                }
            }

            if( metaParameter != null )
            {
                CheckGenericParameters( nullability, metaMethod, metaParameter.ParameterType, parameter.Member.ReflectedType );
            }
        }
    }

    static MethodInfo GetMethodMetadataDefinition( MethodInfo method )
    {
        if( method.IsGenericMethod && !method.IsGenericMethodDefinition )
        {
            method = method.GetGenericMethodDefinition();
        }

        return (MethodInfo)GetMemberMetadataDefinition( method );
    }

    static void CheckNullabilityAttributes( TEMPNullabilityInfo nullability, IList<CustomAttributeData> attributes )
    {
        var codeAnalysisReadState = NullabilityState.Unknown;
        var codeAnalysisWriteState = NullabilityState.Unknown;

        foreach( CustomAttributeData attribute in attributes )
        {
            if( attribute.AttributeType.Namespace == "System.Diagnostics.CodeAnalysis" )
            {
                if( attribute.AttributeType.Name == "NotNullAttribute" )
                {
                    codeAnalysisReadState = NullabilityState.NotNull;
                }
                else if( (attribute.AttributeType.Name == "MaybeNullAttribute" ||
                        attribute.AttributeType.Name == "MaybeNullWhenAttribute") &&
                        codeAnalysisReadState == NullabilityState.Unknown &&
                        !IsValueTypeOrValueTypeByRef( nullability.Type ) )
                {
                    codeAnalysisReadState = NullabilityState.Nullable;
                }
                else if( attribute.AttributeType.Name == "DisallowNullAttribute" )
                {
                    codeAnalysisWriteState = NullabilityState.NotNull;
                }
                else if( attribute.AttributeType.Name == "AllowNullAttribute" &&
                    codeAnalysisWriteState == NullabilityState.Unknown &&
                    !IsValueTypeOrValueTypeByRef( nullability.Type ) )
                {
                    codeAnalysisWriteState = NullabilityState.Nullable;
                }
            }
        }

        if( codeAnalysisReadState != NullabilityState.Unknown )
        {
            nullability.ReadState = codeAnalysisReadState;
        }
        if( codeAnalysisWriteState != NullabilityState.Unknown )
        {
            nullability.WriteState = codeAnalysisWriteState;
        }
    }

    /// <summary>
    /// Populates <see cref="TEMPNullabilityInfo" /> for the given <see cref="PropertyInfo" />.
    /// If the nullablePublicOnly feature is set for an assembly, like it does in .NET SDK, the and/or internal member's
    /// nullability attributes are omitted, in this case the API will return NullabilityState.Unknown state.
    /// </summary>
    /// <param name="propertyInfo">The parameter which nullability info gets populated</param>
    /// <exception cref="ArgumentNullException">If the propertyInfo parameter is null</exception>
    /// <returns><see cref="TEMPNullabilityInfo" /></returns>
    public TEMPNullabilityInfo Create( PropertyInfo propertyInfo )
    {
        ArgumentNullException.ThrowIfNull( propertyInfo );

        EnsureIsSupported();

        MethodInfo? getter = propertyInfo.GetGetMethod( true );
        MethodInfo? setter = propertyInfo.GetSetMethod( true );
        bool annotationsDisabled = (getter == null || IsPrivateOrInternalMethodAndAnnotationDisabled( getter ))
            && (setter == null || IsPrivateOrInternalMethodAndAnnotationDisabled( setter ));
        NullableAttributeStateParser parser = annotationsDisabled ? NullableAttributeStateParser.Unknown : CreateParser( propertyInfo.GetCustomAttributesData() );
        TEMPNullabilityInfo nullability = GetNullabilityInfo( propertyInfo, propertyInfo.PropertyType, parser );

        if( getter != null )
        {
            CheckNullabilityAttributes( nullability, getter.ReturnParameter.GetCustomAttributesData() );
        }
        else
        {
            nullability.ReadState = NullabilityState.Unknown;
        }

        if( setter != null )
        {
            CheckNullabilityAttributes( nullability, setter.GetParameters()[^1].GetCustomAttributesData() );
        }
        else
        {
            nullability.WriteState = NullabilityState.Unknown;
        }

        return nullability;
    }

    bool IsPrivateOrInternalMethodAndAnnotationDisabled( MethodBase method )
    {
        if( (method.IsPrivate || method.IsFamilyAndAssembly || method.IsAssembly) &&
           IsPublicOnly( method.IsPrivate, method.IsFamilyAndAssembly, method.IsAssembly, method.Module ) )
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Populates <see cref="TEMPNullabilityInfo" /> for the given <see cref="EventInfo" />.
    /// If the nullablePublicOnly feature is set for an assembly, like it does in .NET SDK, the and/or internal member's
    /// nullability attributes are omitted, in this case the API will return NullabilityState.Unknown state.
    /// </summary>
    /// <param name="eventInfo">The parameter which nullability info gets populated</param>
    /// <exception cref="ArgumentNullException">If the eventInfo parameter is null</exception>
    /// <returns><see cref="TEMPNullabilityInfo" /></returns>
    public TEMPNullabilityInfo Create( EventInfo eventInfo )
    {
        ArgumentNullException.ThrowIfNull( eventInfo );

        EnsureIsSupported();

        return GetNullabilityInfo( eventInfo, eventInfo.EventHandlerType!, CreateParser( eventInfo.GetCustomAttributesData() ) );
    }

    /// <summary>
    /// Populates <see cref="TEMPNullabilityInfo" /> for the given <see cref="FieldInfo" />
    /// If the nullablePublicOnly feature is set for an assembly, like it does in .NET SDK, the and/or internal member's
    /// nullability attributes are omitted, in this case the API will return NullabilityState.Unknown state.
    /// </summary>
    /// <param name="fieldInfo">The parameter which nullability info gets populated</param>
    /// <exception cref="ArgumentNullException">If the fieldInfo parameter is null</exception>
    /// <returns><see cref="TEMPNullabilityInfo" /></returns>
    public TEMPNullabilityInfo Create( FieldInfo fieldInfo )
    {
        ArgumentNullException.ThrowIfNull( fieldInfo );

        EnsureIsSupported();

        IList<CustomAttributeData> attributes = fieldInfo.GetCustomAttributesData();
        NullableAttributeStateParser parser = IsPrivateOrInternalFieldAndAnnotationDisabled( fieldInfo ) ? NullableAttributeStateParser.Unknown : CreateParser( attributes );
        TEMPNullabilityInfo nullability = GetNullabilityInfo( fieldInfo, fieldInfo.FieldType, parser );
        CheckNullabilityAttributes( nullability, attributes );
        return nullability;
    }

    static void EnsureIsSupported()
    {
        if( !IsSupported )
        {
            throw new InvalidOperationException( "NullabilityInfoContext is not supported in the current application because 'System.Reflection.NullabilityInfoContext.IsSupported' is set to false. Set the MSBuild Property 'NullabilityInfoContextSupport' to true in order to enable it." );
        }
    }

    bool IsPrivateOrInternalFieldAndAnnotationDisabled( FieldInfo fieldInfo )
    {
        if( (fieldInfo.IsPrivate || fieldInfo.IsFamilyAndAssembly || fieldInfo.IsAssembly) &&
            IsPublicOnly( fieldInfo.IsPrivate, fieldInfo.IsFamilyAndAssembly, fieldInfo.IsAssembly, fieldInfo.Module ) )
        {
            return true;
        }

        return false;
    }

    bool IsPublicOnly( bool isPrivate, bool isFamilyAndAssembly, bool isAssembly, Module module )
    {
        if( !_publicOnlyModules.TryGetValue( module, out NotAnnotatedStatus value ) )
        {
            value = PopulateAnnotationInfo( module.GetCustomAttributesData() );
            _publicOnlyModules.Add( module, value );
        }

        if( value == NotAnnotatedStatus.None )
        {
            return false;
        }

        if( (isPrivate || isFamilyAndAssembly) && value.HasFlag( NotAnnotatedStatus.Private ) ||
             isAssembly && value.HasFlag( NotAnnotatedStatus.Internal ) )
        {
            return true;
        }

        return false;
    }

    static NotAnnotatedStatus PopulateAnnotationInfo( IList<CustomAttributeData> customAttributes )
    {
        foreach( CustomAttributeData attribute in customAttributes )
        {
            if( attribute.AttributeType.Name == "NullablePublicOnlyAttribute" &&
                attribute.AttributeType.Namespace == CompilerServicesNameSpace &&
                attribute.ConstructorArguments.Count == 1 )
            {
                if( attribute.ConstructorArguments[0].Value is bool boolValue && boolValue )
                {
                    return NotAnnotatedStatus.Internal | NotAnnotatedStatus.Private;
                }
                else
                {
                    return NotAnnotatedStatus.Private;
                }
            }
        }

        return NotAnnotatedStatus.None;
    }

    TEMPNullabilityInfo GetNullabilityInfo( MemberInfo memberInfo, Type type, NullableAttributeStateParser parser )
    {
        int index = 0;
        TEMPNullabilityInfo nullability = GetNullabilityInfo( memberInfo, type, parser, ref index );

        if( nullability.ReadState != NullabilityState.Unknown )
        {
            TryLoadGenericMetaTypeNullability( memberInfo, nullability );
        }

        return nullability;
    }

    TEMPNullabilityInfo GetNullabilityInfo( MemberInfo memberInfo, Type type, NullableAttributeStateParser parser, ref int index )
    {
        NullabilityState state = NullabilityState.Unknown;
        TEMPNullabilityInfo? elementState = null;
        TEMPNullabilityInfo[] genericArgumentsState = Array.Empty<TEMPNullabilityInfo>();
        Type underlyingType = type;

        if( underlyingType.IsByRef || underlyingType.IsPointer )
        {
            underlyingType = underlyingType.GetElementType()!;
        }

        if( underlyingType.IsValueType )
        {
            if( Nullable.GetUnderlyingType( underlyingType ) is { } nullableUnderlyingType )
            {
                underlyingType = nullableUnderlyingType;
                state = NullabilityState.Nullable;
            }
            else
            {
                state = NullabilityState.NotNull;
            }

            if( underlyingType.IsGenericType )
            {
                ++index;
            }
        }
        else
        {
            if( !parser.ParseNullableState( index++, ref state )
                && GetNullableContext( memberInfo ) is { } contextState )
            {
                state = contextState;
            }

            if( underlyingType.IsArray )
            {
                elementState = GetNullabilityInfo( memberInfo, underlyingType.GetElementType()!, parser, ref index );
            }
        }

        if( underlyingType.IsGenericType )
        {
            Type[] genericArguments = underlyingType.GetGenericArguments();
            genericArgumentsState = new TEMPNullabilityInfo[genericArguments.Length];

            for( int i = 0; i < genericArguments.Length; i++ )
            {
                genericArgumentsState[i] = GetNullabilityInfo( memberInfo, genericArguments[i], parser, ref index );
            }
        }

        return new TEMPNullabilityInfo( type, state, state, elementState, genericArgumentsState );
    }

    static NullableAttributeStateParser CreateParser( IList<CustomAttributeData> customAttributes )
    {
        foreach( CustomAttributeData attribute in customAttributes )
        {
            if( attribute.AttributeType.Name == "NullableAttribute" &&
                attribute.AttributeType.Namespace == CompilerServicesNameSpace &&
                attribute.ConstructorArguments.Count == 1 )
            {
                return new NullableAttributeStateParser( attribute.ConstructorArguments[0].Value );
            }
        }

        return new NullableAttributeStateParser( null );
    }

    void TryLoadGenericMetaTypeNullability( MemberInfo memberInfo, TEMPNullabilityInfo nullability )
    {
        MemberInfo? metaMember = GetMemberMetadataDefinition( memberInfo );
        Type? metaType = null;
        if( metaMember is FieldInfo field )
        {
            metaType = field.FieldType;
        }
        else if( metaMember is PropertyInfo property )
        {
            metaType = GetPropertyMetaType( property );
        }

        if( metaType != null )
        {
            CheckGenericParameters( nullability, metaMember!, metaType, memberInfo.ReflectedType );
        }
    }

    static MemberInfo GetMemberMetadataDefinition( MemberInfo member )
    {
        Type? type = member.DeclaringType;
        if( (type != null) && type.IsGenericType && !type.IsGenericTypeDefinition )
        {
            return type.GetGenericTypeDefinition().GetMemberWithSameMetadataDefinitionAs( member );
        }

        return member;
    }

    static Type GetPropertyMetaType( PropertyInfo property )
    {
        if( property.GetGetMethod( true ) is MethodInfo method )
        {
            return method.ReturnType;
        }

        return property.GetSetMethod( true )!.GetParameters()[0].ParameterType;
    }

    void CheckGenericParameters( TEMPNullabilityInfo nullability, MemberInfo metaMember, Type metaType, Type? reflectedType )
    {
        if( metaType.IsGenericParameter )
        {
            if( nullability.ReadState == NullabilityState.NotNull )
            {
                TryUpdateGenericParameterNullability( nullability, metaType, reflectedType );
            }
        }
        else if( metaType.ContainsGenericParameters )
        {
            if( nullability.GenericTypeArguments.Length > 0 )
            {
                Type[] genericArguments = metaType.GetGenericArguments();

                for( int i = 0; i < genericArguments.Length; i++ )
                {
                    CheckGenericParameters( nullability.GenericTypeArguments[i], metaMember, genericArguments[i], reflectedType );
                }
            }
            else if( nullability.ElementType is { } elementNullability && metaType.IsArray )
            {
                CheckGenericParameters( elementNullability, metaMember, metaType.GetElementType()!, reflectedType );
            }
            // We could also follow this branch for metaType.IsPointer, but since pointers must be unmanaged this
            // will be a no-op regardless
            else if( metaType.IsByRef )
            {
                CheckGenericParameters( nullability, metaMember, metaType.GetElementType()!, reflectedType );
            }
        }
    }

    bool TryUpdateGenericParameterNullability( TEMPNullabilityInfo nullability, Type genericParameter, Type? reflectedType )
    {
        Debug.Assert( genericParameter.IsGenericParameter );

        if( reflectedType is not null
            && !genericParameter.IsGenericMethodParameter
            && TryUpdateGenericTypeParameterNullabilityFromReflectedType( nullability, genericParameter, reflectedType, reflectedType ) )
        {
            return true;
        }

        if( IsValueTypeOrValueTypeByRef( nullability.Type ) )
        {
            return true;
        }

        var state = NullabilityState.Unknown;
        if( CreateParser( genericParameter.GetCustomAttributesData() ).ParseNullableState( 0, ref state ) )
        {
            nullability.ReadState = state;
            nullability.WriteState = state;
            return true;
        }

        if( GetNullableContext( genericParameter ) is { } contextState )
        {
            nullability.ReadState = contextState;
            nullability.WriteState = contextState;
            return true;
        }

        return false;
    }

    bool TryUpdateGenericTypeParameterNullabilityFromReflectedType( TEMPNullabilityInfo nullability, Type genericParameter, Type context, Type reflectedType )
    {
        Debug.Assert( genericParameter.IsGenericParameter && !genericParameter.IsGenericMethodParameter );

        Type contextTypeDefinition = context.IsGenericType && !context.IsGenericTypeDefinition ? context.GetGenericTypeDefinition() : context;
        if( genericParameter.DeclaringType == contextTypeDefinition )
        {
            return false;
        }

        Type? baseType = contextTypeDefinition.BaseType;
        if( baseType is null )
        {
            return false;
        }

        if( !baseType.IsGenericType
            || (baseType.IsGenericTypeDefinition ? baseType : baseType.GetGenericTypeDefinition()) != genericParameter.DeclaringType )
        {
            return TryUpdateGenericTypeParameterNullabilityFromReflectedType( nullability, genericParameter, baseType, reflectedType );
        }

        Type[] genericArguments = baseType.GetGenericArguments();
        Type genericArgument = genericArguments[genericParameter.GenericParameterPosition];
        if( genericArgument.IsGenericParameter )
        {
            return TryUpdateGenericParameterNullability( nullability, genericArgument, reflectedType );
        }

        NullableAttributeStateParser parser = CreateParser( contextTypeDefinition.GetCustomAttributesData() );
        int nullabilityStateIndex = 1; // start at 1 since index 0 is the type itself
        for( int i = 0; i < genericParameter.GenericParameterPosition; i++ )
        {
            nullabilityStateIndex += CountNullabilityStates( genericArguments[i] );
        }
        return TryPopulateNullabilityInfo( nullability, parser, ref nullabilityStateIndex );

        static int CountNullabilityStates( Type type )
        {
            Type underlyingType = Nullable.GetUnderlyingType( type ) ?? type;
            if( underlyingType.IsGenericType )
            {
                int count = 1;
                foreach( Type genericArgument in underlyingType.GetGenericArguments() )
                {
                    count += CountNullabilityStates( genericArgument );
                }
                return count;
            }

            if( underlyingType.HasElementType )
            {
                return (underlyingType.IsArray ? 1 : 0) + CountNullabilityStates( underlyingType.GetElementType()! );
            }

            return type.IsValueType ? 0 : 1;
        }
    }

    bool TryPopulateNullabilityInfo( TEMPNullabilityInfo nullability, NullableAttributeStateParser parser, ref int index )
    {
        bool isValueType = IsValueTypeOrValueTypeByRef( nullability.Type );
        if( !isValueType )
        {
            var state = NullabilityState.Unknown;
            if( !parser.ParseNullableState( index, ref state ) )
            {
                return false;
            }

            nullability.ReadState = state;
            nullability.WriteState = state;
        }

        if( !isValueType || (Nullable.GetUnderlyingType( nullability.Type ) ?? nullability.Type).IsGenericType )
        {
            index++;
        }

        if( nullability.GenericTypeArguments.Length > 0 )
        {
            foreach( TEMPNullabilityInfo genericTypeArgumentNullability in nullability.GenericTypeArguments )
            {
                TryPopulateNullabilityInfo( genericTypeArgumentNullability, parser, ref index );
            }
        }
        else if( nullability.ElementType is { } elementTypeNullability )
        {
            TryPopulateNullabilityInfo( elementTypeNullability, parser, ref index );
        }

        return true;
    }

    static NullabilityState TranslateByte( object? value )
    {
        return value is byte b ? TranslateByte( b ) : NullabilityState.Unknown;
    }

    static NullabilityState TranslateByte( byte b ) =>
        b switch
        {
            1 => NullabilityState.NotNull,
            2 => NullabilityState.Nullable,
            _ => NullabilityState.Unknown
        };

    static bool IsValueTypeOrValueTypeByRef( Type type ) =>
        type.IsValueType || ((type.IsByRef || type.IsPointer) && type.GetElementType()!.IsValueType);

    readonly struct NullableAttributeStateParser
    {
        static readonly object UnknownByte = (byte)0;

        readonly object? _nullableAttributeArgument;

        public NullableAttributeStateParser( object? nullableAttributeArgument )
        {
            this._nullableAttributeArgument = nullableAttributeArgument;
        }

        public static NullableAttributeStateParser Unknown => new( UnknownByte );

        public bool ParseNullableState( int index, ref NullabilityState state )
        {
            switch( this._nullableAttributeArgument )
            {
                case byte b:
                    state = TranslateByte( b );
                    return true;
                case ReadOnlyCollection<CustomAttributeTypedArgument> args
                    when index < args.Count && args[index].Value is byte elementB:
                    state = TranslateByte( elementB );
                    return true;
                default:
                    return false;
            }
        }
    }
}
