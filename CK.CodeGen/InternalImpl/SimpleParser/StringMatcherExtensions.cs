using System;
using System.Collections.Generic;
using System.Text;
using CK.Core;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using CK.CodeGen;
using CK.CodeGen.SimpleParser;
using System.Diagnostics;

namespace CK.CodeGen
{
    static class StringMatcherExtensions
    {
        /// <summary>
        /// Tries to match a //.... or /* ... */ comment.
        /// Proper termination of comment (by a new line or the closing */) is not required: 
        /// a ending /*... is considered valid.
        /// </summary>
        /// <param name="this">This <see cref="IStringMatcher"/>.</param>
        /// <returns>True on success, false if the <see cref="IStringMatcher.Head"/> is not on a /.</returns>
        internal static bool TryMatchComment( this StringMatcher @this )
        {
            if( !@this.TryMatchChar( '/' ) ) return false;
            if( @this.TryMatchChar( '/' ) )
            {
                while( !@this.IsEnd && @this.Head != '\n' ) @this.UncheckedMove( 1 );
                if( !@this.IsEnd ) @this.UncheckedMove( 1 );
                return true;
            }
            else if( @this.TryMatchChar( '*' ) )
            {
                while( !@this.IsEnd )
                {
                    if( @this.Head == '*' )
                    {
                        @this.UncheckedMove( 1 );
                        if( @this.IsEnd || @this.TryMatchChar( '/' ) ) return true;
                    }
                    @this.UncheckedMove( 1 );
                }
                return true;
            }
            @this.UncheckedMove( -1 );
            return false;
        }

        /// <summary>
        /// Skips any white spaces or JS comments (//... or /* ... */) and always returns true.
        /// </summary>
        /// <param name="this">This <see cref="IStringMatcher"/>.</param>
        /// <returns>Always true to ease composition.</returns>
        internal static bool SkipWhiteSpacesAndJSComments( this StringMatcher @this )
        {
            @this.MatchWhiteSpaces( 0 );
            while( @this.TryMatchComment() ) @this.MatchWhiteSpaces( 0 );
            return true;
        }

        internal static bool TryMatchCSharpIdentifier( this StringMatcher @this, [NotNullWhen( true )]out string? identifier, bool skipAtSign = false )
        {
            identifier = null;
            if( @this.IsEnd ) return false;
            int savedIdx = @this.StartIndex;
            bool at = @this.TryMatchChar( '@' );
            if( IsValidIdentifierStart( @this.Head ) )
            {
                while( @this.UncheckedMove( 1 ) && !@this.IsEnd && IsValidIdentifierChar( @this.Head ) ) ;
                if( at && skipAtSign ) ++savedIdx;
                identifier = @this.Text.Substring( savedIdx, @this.StartIndex - savedIdx );
                return true;
            }
            if( at ) @this.UncheckedMove( -1 );
            return false;
        }

        static bool EatRawCode( this StringMatcher @this, StringBuilder b, bool stopOnComma, bool removeWhiteSpaces = true )
        {
            int bPos = b.Length;
            int depth = 0;
            while( !@this.IsEnd
                    && (depth != 0 || (@this.Head != ')' && @this.Head != ']' && @this.Head != '}' && (!stopOnComma || @this.Head != ','))) )
            {
                if( @this.Head == '(' || @this.Head == '[' || @this.Head == '{' )
                {
                    ++depth;
                    b.Append( @this.Head );
                    @this.UncheckedMove( 1 );
                }
                else if( @this.Head == ')' || @this.Head == ']' || @this.Head == '}' )
                {
                    --depth;
                    b.Append( @this.Head );
                    @this.UncheckedMove( 1 );
                }
                else if( @this.TryMatchCSharpIdentifier( out var id, skipAtSign: false ) )
                {
                    b.Append( id );
                }
                else if( @this.TryMatchCSharpString( out var str ) )
                {
                    b.Append( str );
                }
                else
                {
                    if( !(removeWhiteSpaces && Char.IsWhiteSpace( @this.Head )) ) b.Append( @this.Head );
                    @this.UncheckedMove( 1 );
                }
            }
            return b.Length > bPos;
        }

        static bool TryMatchCSharpString( this StringMatcher @this, [NotNullWhen( true )]out string? s )
        {
            if( @this.TryMatchText( "$@\"" ) )
            {
                return @this.EatVerbatimString( 3, out s );
            }
            if( @this.TryMatchText( "@\"" ) )
            {
                return @this.EatVerbatimString( 2, out s );
            }
            if( @this.TryMatchChar( '"' ) )
            {
                return @this.EatString( out s, '"' );
            }
            if( @this.TryMatchChar( '\'' ) )
            {
                return @this.EatString( out s, '\'' );
            }
            s = null;
            return false;
        }

        static bool EatString( this StringMatcher @this, [NotNullWhen( true )]out string? s, char mark )
        {
            int startIdx = @this.StartIndex - 1;
            while( !@this.IsEnd )
            {
                if( @this.Head == mark )
                {
                    @this.UncheckedMove( 1 );
                    s = @this.GetText( startIdx, @this.StartIndex - startIdx );
                    return true;
                }
                @this.UncheckedMove( @this.Head == '\\' ? 2 : 1 );
            }
            s = null;
            return false;
        }

        static bool EatVerbatimString( this StringMatcher @this, int start, [NotNullWhen( true )]out string? s )
        {
            int startIdx = @this.StartIndex - start;
            while( !@this.IsEnd )
            {
                if( @this.Head == '"' )
                {
                    @this.UncheckedMove( 1 );
                    if( @this.IsEnd ) break;
                    if (@this.Head == '"')
                    {
                        @this.UncheckedMove( 1 );
                        continue;
                    }
                    s = @this.GetText( startIdx, @this.StartIndex - startIdx );
                    return true;
                }
                @this.UncheckedMove( 1 );
            }
            s = null;
            return false;
        }

        internal static bool MatchPotentialAttributes( this StringMatcher @this, out AttributeCollection? attributes )
        {
            attributes = null;
            while( @this.TryMatchAttribute( out var a ) )
            {
                if( attributes == null ) attributes = new AttributeCollection();
                attributes.Ensure( a );
                @this.SkipWhiteSpacesAndJSComments();
            }
            return !@this.IsError;
        }

        static CodeAttributeTarget MapAttributeTarget( string s )
        {
            return s switch
            {
                "assembly" => CodeAttributeTarget.Assembly,
                "module" => CodeAttributeTarget.Module,
                "field" => CodeAttributeTarget.Field,
                "event" => CodeAttributeTarget.Event,
                "method" => CodeAttributeTarget.Method,
                "param" => CodeAttributeTarget.Param,
                "property" => CodeAttributeTarget.Property,
                "return" => CodeAttributeTarget.Return,
                "type" => CodeAttributeTarget.Type,
                _ => CodeAttributeTarget.None,
            };
        }

        internal static bool TryMatchAttribute( this StringMatcher @this, [NotNullWhen( true )]out AttributeSetDefinition? attr )
        {
            attr = null;
            if( !@this.TryMatchChar( '[' ) ) return false;
            @this.SkipWhiteSpacesAndJSComments();
            if( !@this.TryMatchCSharpIdentifier( out string? targetOrName ) ) return @this.AddError( "Attribute definition expected." );
            var target = MapAttributeTarget( targetOrName );
            if( target != CodeAttributeTarget.None )
            {
                if( !@this.MatchChar( ':' ) ) return false;
                targetOrName = null;
            }
            List<AttributeDefinition> attributes = new List<AttributeDefinition>();
            do
            {
                @this.SkipWhiteSpacesAndJSComments();
                if( !@this.MatchTypeName( out TypeName? name, targetOrName ) ) return @this.AddError( "Attribute definition expected." );
                targetOrName = null;
                if( name.Name.EndsWith( "Attribute", StringComparison.Ordinal ) )
                {
                    name = new TypeName( name.Name.Remove( name.Name.Length - 9 ), name.GenericParameters );
                }
                var bAttrValue = new StringBuilder();
                List<string> values = new List<string>();
                @this.SkipWhiteSpacesAndJSComments();
                if( @this.TryMatchChar( '(' ) )
                {
                    @this.SkipWhiteSpacesAndJSComments();
                    while( !@this.TryMatchChar( ')' ) )
                    {
                        if( !@this.EatRawCode( bAttrValue, true, true ) ) return @this.SetError( "Values expected." );
                        values.Add( bAttrValue.ToString() );
                        bAttrValue.Clear();
                        // Allow training comma. Don't care.
                        if( @this.TryMatchChar( ',' ) ) @this.SkipWhiteSpacesAndJSComments();
                    }
                }
                attributes.Add( new AttributeDefinition( name, values ) );
                @this.SkipWhiteSpacesAndJSComments();
            }
            while ( @this.TryMatchChar( ',' ) );
            @this.SkipWhiteSpacesAndJSComments();
            if( !@this.MatchChar( ']' ) ) return false;
            attr = new AttributeSetDefinition( target, attributes );
            return true;
        }

        internal static bool MatchMethodDefinition( this StringMatcher @this, [NotNullWhen( true )] out FunctionDefinition? mDef, out bool hasCodeOpener )
        {
            mDef = null;
            hasCodeOpener = false;

            if( !@this.MatchPotentialAttributes( out var attributes ) ) return false;

            var startName = CollectModifiersUntilIdentifier( @this, out var modifiers );
            modifiers = modifiers.NormalizeMemberProtection();

            @this.SkipWhiteSpacesAndJSComments();
            if( !@this.MatchExtendedTypeName( out var returnType, startName ) ) return false;

            @this.SkipWhiteSpacesAndJSComments();
            bool isIndexer = false;
            TypeName? methodName;
            if( @this.TryMatchChar( '(' ) )
            {
                if( returnType.IsTuple ) return @this.SetError( $"Invalid syntax: unexpected tuple {returnType}." );
                methodName = returnType.TypeName;
                returnType = null;
            }
            else
            {
                if( !@this.MatchTypeName( out methodName ) ) return false;
                @this.SkipWhiteSpacesAndJSComments();
                if( !@this.MatchChar( '(' ) && !(isIndexer = @this.TryMatchChar( '[' ) ) ) return @this.SetError( "Expected '[' or '('." ); 
            }
            Debug.Assert( methodName != null );
            var buffer = new StringBuilder();
            @this.SkipWhiteSpacesAndJSComments();
            List<FunctionDefinition.Parameter> parameters = new List<FunctionDefinition.Parameter>();
            while( !@this.TryMatchChar( isIndexer ? ']' : ')' ) )
            {
                do
                {
                    @this.SkipWhiteSpacesAndJSComments();
                    if( !@this.MatchPotentialAttributes( out var pAttr ) ) return false;

                    FunctionDefinition.ParameterModifier mod = FunctionDefinition.ParameterModifier.None;
                    if( @this.TryMatchCSharpIdentifier( out var pTypeStart ) )
                    {
                        switch( pTypeStart )
                        {
                            case "this": mod = FunctionDefinition.ParameterModifier.This; pTypeStart = null; break;
                            case "params": mod = FunctionDefinition.ParameterModifier.Params; pTypeStart = null; break;
                            case "out": mod = FunctionDefinition.ParameterModifier.Out; pTypeStart = null; break;
                            case "ref": mod = FunctionDefinition.ParameterModifier.Ref; pTypeStart = null; break;
                            case "in": mod = FunctionDefinition.ParameterModifier.In; pTypeStart = null; break;
                        }
                    }
                    @this.SkipWhiteSpacesAndJSComments();
                    if( !@this.MatchExtendedTypeName( out var pType, pTypeStart ) ) return false;
                    @this.SkipWhiteSpacesAndJSComments();
                    if( !@this.TryMatchCSharpIdentifier( out var pName ) ) return false;
                    @this.SkipWhiteSpacesAndJSComments();
                    string? defVal = null;
                    if( @this.TryMatchChar( '=' ) )
                    {
                        if( !@this.EatRawCode( buffer, true, true ) ) return @this.SetError( "Unable to read default value." );
                        defVal = buffer.ToString();
                        buffer.Clear();
                    }
                    else @this.SkipWhiteSpacesAndJSComments();
                    parameters.Add( new FunctionDefinition.Parameter( pAttr, mod, pType, pName, defVal ) );
                }
                while( @this.TryMatchChar( ',' ) );
            }
            var thisOrBaseCall = FunctionDefinition.CallConstructor.None;
            string? thisOrBaseCallParameter = null;
            @this.SkipWhiteSpacesAndJSComments();
            if( returnType == null )
            {
                if( @this.TryMatchChar( ':' ) )
                {
                    @this.SkipWhiteSpacesAndJSComments();
                    if( @this.TryMatchText( "this", StringComparison.Ordinal ) ) thisOrBaseCall = FunctionDefinition.CallConstructor.This;
                    else if( @this.TryMatchText( "base", StringComparison.Ordinal ) ) thisOrBaseCall = FunctionDefinition.CallConstructor.Base;
                    if( thisOrBaseCall != FunctionDefinition.CallConstructor.None )
                    {
                        @this.SkipWhiteSpacesAndJSComments();
                        if( !@this.MatchChar( '(' ) ) return @this.SetError( "this(...) or base(...) : missing '('." );
                        @this.EatRawCode( buffer, false, false );
                        thisOrBaseCallParameter = buffer.ToString();
                        @this.SkipWhiteSpacesAndJSComments();
                        if( !@this.TryMatchChar( ')' ) ) return @this.SetError( "this(...) or base(...) : missing ')'." );
                        @this.SkipWhiteSpacesAndJSComments();
                    }
                    else return @this.SetError( "this(...) or base(...) expected." );
                }
            }
            buffer.Clear();
            List<TypeParameterConstraint>? wheres;
            if( !@this.MatchWhereConstraints( out hasCodeOpener, out wheres ) ) return false;
            mDef = new FunctionDefinition( attributes, modifiers, returnType, methodName, thisOrBaseCall, thisOrBaseCallParameter, isIndexer, parameters, wheres, buffer );
            return true;
        }

        #region TypeDefinition

        internal static string? CollectModifiersUntilIdentifier( this StringMatcher @this, out Modifiers modifiers )
        {
            modifiers = Modifiers.None;
            string? id;
            while( @this.TryMatchCSharpIdentifier( out id )
                   && ModifiersExtension.ParseAndCombine( ref modifiers, id ) )
            {
                @this.SkipWhiteSpacesAndJSComments();
            }
            return id;
        }

        internal static bool MatchTypeKey( this StringMatcher @this, [NotNullWhen( true )]out string? key )
        {
            key = null;
            if( !@this.MatchPotentialAttributes( out var attributes ) ) return false;
            string? head = @this.CollectModifiersUntilIdentifier( out var modifiers );
            if( head == "class" || head == "struct" || head == "interface" || head == "enum" )
            {
                head = null;
            }
            @this.SkipWhiteSpacesAndJSComments();
            if( !@this.MatchTypeName( out var name, head ) ) return false;
            key = name.Key;
            return true;
        }

        internal static bool MatchTypeDefinition( this StringMatcher @this, [NotNullWhen( true )]out TypeDefinition? typeDef, bool isNestedType, out bool hasCodeOpener )
        {
            typeDef = null;
            hasCodeOpener = false;

            if( !@this.MatchPotentialAttributes( out var attributes ) ) return false;

            TypeDefinition.TypeKind kind;
            switch( CollectModifiersUntilIdentifier( @this, out var modifiers ) )
            {
                case "class": kind = TypeDefinition.TypeKind.Class; break;
                case "struct": kind = TypeDefinition.TypeKind.Struct; break;
                case "interface": kind = TypeDefinition.TypeKind.Interface; break;
                case "enum": kind = TypeDefinition.TypeKind.Enum; break;
                default: return @this.SetError( "Expected: class, struct, interface or enum." );
            }
            modifiers = modifiers.NormalizeForType();
            if( isNestedType ) modifiers = modifiers.NormalizeMemberProtection();
            else modifiers = modifiers.NormalizeNamespaceProtection();

            @this.SkipWhiteSpacesAndJSComments();
            if( !@this.MatchTypeName( out var name ) ) return false;
            List<ExtendedTypeName>? baseTypes = null;
            @this.SkipWhiteSpacesAndJSComments();
            if( @this.TryMatchChar( ':' ) )
            {
                @this.SkipWhiteSpacesAndJSComments();
                if( !@this.MatchBaseTypesOrConstraints( out baseTypes ) ) return false;
            }
            @this.SkipWhiteSpacesAndJSComments();
            List<TypeParameterConstraint>? wheres;
            if( !@this.MatchWhereConstraints( out hasCodeOpener, out wheres ) ) return false;
            typeDef = new TypeDefinition( attributes, modifiers, kind, name, baseTypes, wheres );
            return true;
        }

        static bool MatchWhereConstraints( this StringMatcher @this, out bool hasCodeOpener, out List<TypeParameterConstraint>? wheres )
        {
            hasCodeOpener = false;
            wheres = null;
            while( !@this.IsEnd && !(hasCodeOpener = (@this.Head == '{' || @this.Head == '=')) )
            {
                @this.SkipWhiteSpacesAndJSComments();
                if( !@this.MatchTypeParameterConstraint( out var c ) ) return false;
                if( wheres == null ) wheres = new List<TypeParameterConstraint>();
                else if( wheres.Any( x => x.ParameterName == c.ParameterName ) ) return @this.SetError( $"Duplicate where constraint: where {c.ParameterName}." );
                wheres.Add( c );
                @this.SkipWhiteSpacesAndJSComments();
            }
            if( hasCodeOpener )
            {
                // If we stopped on '{', forwards the head.
                @this.TryMatchChar( '{' );
                hasCodeOpener = !@this.IsEnd;
            }
            return true;
        }

        /// <summary>
        /// BaseTypeOrConstraint => TypeName | new()
        /// The "new()" is becomes the <see cref="TypeName.Name"/> of a pseudo type name.
        /// </summary>
        static bool MatchBaseTypeOrConstraint( this StringMatcher @this, [NotNullWhen( true )]out ExtendedTypeName? t )
        {
            t = null;
            if( !@this.TryMatchCSharpIdentifier( out var baseName ) ) return @this.SetError( "Expected identifier." );
            if( baseName == "new" )
            {
                @this.SkipWhiteSpacesAndJSComments();
                if( @this.TryMatchChar( '(' ) )
                {
                    @this.SkipWhiteSpacesAndJSComments();
                    if( @this.TryMatchChar( ')' ) )
                    {
                        t = new ExtendedTypeName( new TypeName( "new()", null ) );
                        return true;
                    }
                }
                return @this.SetError( "Invalid new() constraint." );
            }
            @this.SkipWhiteSpacesAndJSComments();
            return @this.MatchExtendedTypeName( out t, baseName );
        }

        /// <summary>
        /// BaseTypesOrConstraints => comma separated MatchBaseTypeOrConstraint.
        /// </summary>
        static bool MatchBaseTypesOrConstraints( this StringMatcher @this, out List<ExtendedTypeName> types )
        {
            types = new List<ExtendedTypeName>();
            do
            {
                @this.SkipWhiteSpacesAndJSComments();
                if( !@this.MatchBaseTypeOrConstraint( out var t ) ) return @this.AddError( "Expected base type.", true );
                types.Add( t );
                @this.SkipWhiteSpacesAndJSComments();
            }
            while( @this.TryMatchChar( ',' ) );
            return true;
        }

        /// <summary>
        /// TypeParameterConstraint => where : BaseTypesOrConstraints
        /// </summary>
        static bool MatchTypeParameterConstraint( this StringMatcher @this, [NotNullWhen( true )]out TypeParameterConstraint? c )
        {
            c = null;
            if( !@this.TryMatchCSharpIdentifier( out var name ) || name != "where" ) @this.SetError( "Expected where constraint." );
            @this.SkipWhiteSpacesAndJSComments();
            if( !@this.TryMatchCSharpIdentifier( out name ) ) return false;
            @this.SkipWhiteSpacesAndJSComments();
            if( !@this.MatchChar( ':' ) ) return false;
            @this.SkipWhiteSpacesAndJSComments();
            if( !@this.MatchBaseTypesOrConstraints( out var baseTypes ) ) return false;
            c = new TypeParameterConstraint( name, baseTypes );
            return true;
        }

        #endregion

        #region TypeName

        /// <summary>
        /// Relaxed syntax here: we allow empty or single-field tuples (this is not valid)
        /// and an ending comma in the list.
        /// </summary>
        /// <param name="this">This matcher.</param>
        /// <param name="type">The tuple type name on success.</param>
        /// <returns>True on success, false on error.</returns>
        internal static bool TryMatchTupleTypeName( this StringMatcher @this, [NotNullWhen( true )] out TupleTypeName? type )
        {
            type = null;
            if( !@this.TryMatchChar( '(' ) ) return false;
            List<TupleTypeName.Field> fields = new List<TupleTypeName.Field>();
            while( !@this.TryMatchChar( ')' ) )
            {
                ExtendedTypeName? fType = null;
                string? fName = null;
                if( !@this.MatchExtendedTypeName( out fType ) ) return false;
                @this.SkipWhiteSpacesAndJSComments();
                if( @this.TryMatchCSharpIdentifier( out fName ) ) @this.SkipWhiteSpacesAndJSComments();
                fields.Add( new TupleTypeName.Field( fType, fName ) );
                if( @this.TryMatchChar( ',' ) ) @this.SkipWhiteSpacesAndJSComments();
            }
            @this.SkipWhiteSpacesAndJSComments();
            type = new TupleTypeName( fields );
            return true;
        }

        internal static bool MatchExtendedTypeName( this StringMatcher @this, [NotNullWhen( true )] out ExtendedTypeName? type, string? knownName = null )
        {
            type = null;
            TupleTypeName? tuple = null;
            TypeName? regularType = null;
            if( knownName != null || !@this.TryMatchTupleTypeName( out tuple ) )
            {
                if( @this.MatchTypeName( out regularType, knownName ) )
                {
                    if( regularType.GenericParameters.Count == 1 && (regularType.Name == "Nullable" || regularType.Name == "System.Nullable") )
                    {
                        type = regularType.GenericParameters[0].Type.WithNullable( true );
                    }
                }
            }
            if( type == null )
            {
                // Nullable<Nullable<...>> cannot exist.
                bool isNullable = @this.TryMatchChar( '?' );
                if( isNullable ) @this.SkipWhiteSpacesAndJSComments();
                if( tuple != null ) type = new ExtendedTypeName( tuple, isNullable );
                else if( regularType != null ) type = new ExtendedTypeName( regularType, isNullable );
                else return false;
            }
            List<int>? arrayDim = null;
            while( @this.TryMatchChar( '[' ) )
            {
                if( arrayDim == null ) arrayDim = new List<int>();
                int dim = 0;
                while( @this.TryMatchChar( ',' ) )
                {
                    ++dim;
                    @this.SkipWhiteSpacesAndJSComments();
                }
                if( !@this.TryMatchChar( ']' ) ) return @this.SetError( "Closing ']' array." );
                arrayDim.Add( dim );
                @this.SkipWhiteSpacesAndJSComments();
            }
            if( arrayDim != null )
            {
                bool isNullable = @this.TryMatchChar( '?' );
                if( isNullable ) @this.SkipWhiteSpacesAndJSComments();
                type = new ExtendedTypeName( type, arrayDim, isNullable ); 
            }
            return true;
        }

        internal static bool MatchTypeName( this StringMatcher @this, [NotNullWhen( true )]out TypeName? type, string? knownName = null )
        {
            type = null;
            if( knownName != null
                || @this.TryMatchCSharpIdentifier( out knownName ) )
            {
                List<TypeName.GenParam>? genArgs = null;
                @this.SkipWhiteSpacesAndJSComments();
                while( @this.TryMatchChar('.') )
                {
                    @this.SkipWhiteSpacesAndJSComments();
                    if( !@this.TryMatchCSharpIdentifier( out var sub ) ) return false;
                    knownName += '.' + sub;
                    @this.SkipWhiteSpacesAndJSComments();
                }
                if( @this.TryMatchChar( '<' ) )
                {
                    genArgs = new List<TypeName.GenParam>();
                    for( ; ; )
                    {
                        @this.SkipWhiteSpacesAndJSComments();
                        if( @this.TryMatchChar( ',' ) )
                        {
                            genArgs.Add(TypeName.GenParam.Empty);
                            continue;
                        }
                        if( @this.TryMatchChar( '>' ) )
                        {
                            // Handles open generic definition like "G<>" or "G<,>".
                            genArgs.Add(TypeName.GenParam.Empty);
                            @this.SkipWhiteSpacesAndJSComments();
                            break;
                        }
                        if( !MatchGenParam( @this, out var genArg ) ) return @this.AddError( "Expected generic type parameter." );
                        genArgs.Add( genArg.Value );
                        @this.SkipWhiteSpacesAndJSComments();
                        if( @this.TryMatchChar( '>' ) )
                        {
                            @this.SkipWhiteSpacesAndJSComments();
                            break;
                        }
                        if( @this.TryMatchChar( ',' ) ) continue;
                    }
                }
                type = new TypeName( knownName, genArgs );
                return true;
            }
            return @this.SetError( "Type name." );
        }

        static bool MatchGenParam( StringMatcher @this, [NotNullWhen( true )]out TypeName.GenParam? genArg )
        {
            genArg = null;
            var v = TypeName.GenParam.Variance.None;
            if( @this.TryMatchCSharpIdentifier( out string? nameOrVariance ) )
            {
                if( nameOrVariance == "out" )
                {
                    v = TypeName.GenParam.Variance.Out;
                    nameOrVariance = null;
                    @this.SkipWhiteSpacesAndJSComments();
                }
                else if( nameOrVariance == "in" )
                {
                    v = TypeName.GenParam.Variance.In;
                    nameOrVariance = null;
                    @this.SkipWhiteSpacesAndJSComments();
                }
            }
            if( !@this.MatchExtendedTypeName( out var gT, nameOrVariance ) ) return false;
            genArg = new TypeName.GenParam( v, gT );
            return true;
        }

        // This is adapted from: https://stackoverflow.com/questions/1829679/how-to-determine-if-a-string-is-a-valid-variable-name
        // This has been (heavily) simplified: forgetting about surrogate pairs and UnicodeCategory.Format stuff.
        static bool IsValidIdentifierStart( char c )
        {
            return c == '_'
                    || char.IsLetter( c )
                    || char.GetUnicodeCategory( c ) == UnicodeCategory.LetterNumber;
        }

        static bool IsValidIdentifierChar( char c )
        {
            if( c == '_' || (c >= '0' && c <= '9') || char.IsLetter( c ) ) return true;

            switch( char.GetUnicodeCategory( c ) )
            {
                case UnicodeCategory.LetterNumber:
                case UnicodeCategory.DecimalDigitNumber:
                case UnicodeCategory.ConnectorPunctuation:
                case UnicodeCategory.NonSpacingMark:
                case UnicodeCategory.SpacingCombiningMark:
                    return true;
                default:
                    return false;
            }
        }

    }
    #endregion
}
