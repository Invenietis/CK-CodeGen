using System;
using System.Collections.Generic;
using System.Text;
using CK.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Linq;

namespace CK.CodeGen
{
    static class StringMatcherExtensions
    {
        internal static bool TryMatchCSharpIdentifier( this StringMatcher @this, out string identifier, bool skipAtSign = false )
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

        static void EatCompileTimeStuff( this StringMatcher @this, out string stuff )
        {
            int depth = 0;
            StringBuilder b = new StringBuilder();
            while( !@this.IsEnd
                    && (depth != 0 || (@this.Head != ')' && @this.Head != ']' || @this.Head != '}' || @this.Head != ',') ) )
            {
                if( @this.Head == '(' || @this.Head == '[' || @this.Head == '{' )
                {
                    ++depth;
                    b.Append( @this.Head );
                    @this.UncheckedMove( 1 );
                }
                else if( @this.Head == ')' || @this.Head == ']' )
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
                else if( !Char.IsWhiteSpace( @this.Head ) )
                {
                    b.Append( @this.Head );
                    @this.UncheckedMove( 1 );
                }
            }
            stuff = b.ToString();
        }

        static bool TryMatchCSharpString( this StringMatcher @this, out string s )
        {
            if( @this.TryMatchText( "$@\"" ) )
            {
                return @this.EatVerbatimString( 3, out s );
            }
            if( @this.TryMatchText( "@\"" ) )
            {
                return @this.EatVerbatimString( 2, out s );
            }
            if( @this.Head == '"' )
            {
                return @this.EatString( out s );
            }
            s = null;
            return false;
        }

        static bool EatString( this StringMatcher @this, out string s )
        {
            int startIdx = @this.StartIndex;
            while( !@this.IsEnd )
            {
                if( @this.Head == '"' )
                {
                    @this.UncheckedMove( 1 );
                    s = @this.GetText( startIdx, @this.StartIndex - startIdx );
                    return true;
                }
                if( @this.Head == '\\' ) @this.UncheckedMove( 2 );
            }
            s = null;
            return false;
        }

        static bool EatVerbatimString( this StringMatcher @this, int start, out string s )
        {
            int startIdx = @this.StartIndex - start;
            while( !@this.IsEnd )
            {
                if( @this.Head == '"' )
                {
                    @this.UncheckedMove( 1 );
                    if( @this.IsEnd ) break;
                    if( @this.Head == '"' ) continue;
                    s = @this.GetText( startIdx, @this.StartIndex - startIdx );
                    return true;
                }
            }
            s = null;
            return false;
        }

        internal static bool MatchPotentialAttributes( this StringMatcher @this, out List<AttributeDefinition> attributes )
        {
            attributes = null;
            while( @this.TryMatchAttribute( out var a ) )
            {
                if( attributes == null ) attributes = new List<AttributeDefinition>();
                attributes.Add( a );
                @this.SkipWhiteSpacesAndJSComments();
            }
            return !@this.IsError;
        }

        internal static bool TryMatchAttribute( this StringMatcher @this, out AttributeDefinition attr )
        {
            attr = null;
            if( !@this.TryMatchChar( '[' ) ) return false;
            @this.SkipWhiteSpacesAndJSComments();
            if( !@this.TryMatchCSharpIdentifier( out string target ) ) return @this.AddError( "Attribute definition expected." );
            if( target == "assembly"
                || target == "module"
                || target == "field"
                || target == "event"
                || target == "method"
                || target == "param"
                || target == "property"
                || target == "return"
                || target == "type" )
            {
                if( !@this.MatchChar( ':' ) ) return false;
                target = null;
                @this.SkipWhiteSpacesAndJSComments();
            }
            List<AttributeDefinition.OneAttribute> attributes = new List<AttributeDefinition.OneAttribute>();
            do
            {
                if( !@this.MatchTypeName( out TypeName name, target ) ) return @this.AddError( "Attribute definition expected." );
                target = null;
                if( name.Name.EndsWith( "Attribute", StringComparison.Ordinal ) )
                {
                    name = new TypeName( name.Name.Remove( name.Name.Length - 9 ), name.GenArgs, name.ArrayDims );
                }
                List<string> values = new List<string>();
                @this.SkipWhiteSpacesAndJSComments();
                if( @this.TryMatchChar( '(' ) )
                {
                    @this.SkipWhiteSpacesAndJSComments();
                    while( !@this.TryMatchChar( ')' ) )
                    {
                        @this.EatCompileTimeStuff( out string stuff );
                        values.Add( stuff );
                        @this.SkipWhiteSpacesAndJSComments();
                        // Allow training comma. Don't care.
                        if( @this.TryMatchChar( ',' ) ) @this.SkipWhiteSpacesAndJSComments();
                    }
                }
                attributes.Add( new AttributeDefinition.OneAttribute( name, values ) );
            }
            while( @this.TryMatchChar( ',' ) );
            if( !@this.MatchChar( ']' ) ) return false;
            attr = new AttributeDefinition( target, attributes );
            return true;
        }

        #region TypeDefinition

        internal static string CollectModifiersUntilIdentifier( this StringMatcher @this, out Modifiers modifiers )
        {
            modifiers = Modifiers.None;
            string id;
            while( @this.TryMatchCSharpIdentifier( out id )
                   && ModifiersExtension.Combine( ref modifiers, id ) )
            {
                @this.SkipWhiteSpacesAndJSComments();
            }
            return id;
        }

        internal static bool MatchTypeKey( this StringMatcher @this, out string key )
        {
            key = null;
            if( !@this.MatchPotentialAttributes( out var attributes ) ) return false;
            string head = @this.CollectModifiersUntilIdentifier( out var modifiers );
            if( head == "class" || head == "struct" || head == "interface" || head == "enum" )
            {
                head = null;
            }
            @this.SkipWhiteSpacesAndJSComments();
            if( !@this.MatchTypeName( out var name, head ) ) return false;
            key = name.TypeKey;
            return true;
        }

        internal static bool MatchTypeDefinition( this StringMatcher @this, out TypeDefinition typeDef, bool isNestedType, out bool hasCodeOpener )
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
            List<TypeName> baseTypes = null;
            @this.SkipWhiteSpacesAndJSComments();
            if( @this.TryMatchChar( ':' ) )
            {
                @this.SkipWhiteSpacesAndJSComments();
                if( !@this.MatchBaseTypesOrConstraints( out baseTypes ) ) return false;
            }
            List<TypeParameterConstraint> wheres = null;
            @this.SkipWhiteSpacesAndJSComments();
            while( !(hasCodeOpener = @this.TryMatchChar( '{' )) && !@this.IsEnd )
            {
                if( !@this.MatchTypeParameterConstraint( out var c ) ) return false;
                if( wheres == null ) wheres = new List<TypeParameterConstraint>();
                else if( wheres.Any( x => x.ParameterName == c.ParameterName ) ) return @this.SetError( $"Duplicate where constraint: where {c.ParameterName}." );
                wheres.Add( c );
                @this.SkipWhiteSpacesAndJSComments();
            }
            if( wheres != null ) wheres.Sort();
            typeDef = new TypeDefinition( attributes, modifiers, kind, name, baseTypes, wheres );
            return true;
        }

        /// <summary>
        /// BaseTypeOrConstraint => TypeName | new()
        /// The "new()" is becomes the <see cref="TypeName.Name"/> of a pseudo type name.
        /// </summary>
        static bool MatchBaseTypeOrConstraint( this StringMatcher @this, out TypeName t )
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
                        t = new TypeName( "new()", null, null );
                        return true;
                    }
                }
                return @this.SetError( "Invalid new() constraint." );
            }
            @this.SkipWhiteSpacesAndJSComments();
            return @this.MatchTypeName( out t, baseName );
        }

        /// <summary>
        /// BaseTypesOrConstraints => comma separated MatchBaseTypeOrConstraint that we sort except
        /// the first one that is the base class (this applies to where constraints as well as base types
        /// list per se). Note that the "new()" pseudo base type is sorted after any other type names.
        /// </summary>
        static bool MatchBaseTypesOrConstraints( this StringMatcher @this, out List<TypeName> types )
        {
            types = new List<TypeName>();
            do
            {
                @this.SkipWhiteSpacesAndJSComments();
                if( !@this.MatchBaseTypeOrConstraint( out var t ) ) return @this.AddError( "Expected base type.", true );
                types.Add( t );
                @this.SkipWhiteSpacesAndJSComments();
            }
            while( @this.TryMatchChar( ',' ) );
            types.Sort( 1, types.Count - 1, Comparer<TypeName>.Default );
            return true;
        }

        /// <summary>
        /// TypeParameterConstraint => where : BaseTypesOrConstraints
        /// </summary>
        static bool MatchTypeParameterConstraint( this StringMatcher @this, out TypeParameterConstraint c )
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
        internal static bool MatchTypeName( this StringMatcher @this, out TypeName type, string knownName = null )
        {
            type = null;
            if( knownName != null
                || @this.TryMatchCSharpIdentifier( out knownName ) )
            {
                List<TypeName.GenParam> genArgs = null;
                List<int> arrayDim = null;
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
                            genArgs.Add( TypeName.GenParam.Empty );
                            @this.SkipWhiteSpacesAndJSComments();
                            break;
                        }
                        if( !MatchGenParam( @this, out var genArg ) ) return @this.AddError( "Expected generic type parameter." );
                        genArgs.Add( genArg );
                        @this.SkipWhiteSpacesAndJSComments();
                        if( @this.TryMatchChar( '>' ) )
                        {
                            @this.SkipWhiteSpacesAndJSComments();
                            break;
                        }
                        if( @this.TryMatchChar( ',' ) ) continue;
                    }
                }
                while( @this.TryMatchChar( '[' ) )
                {
                    if( arrayDim == null ) arrayDim = new List<int>();
                    int dim = 0;
                    while( @this.TryMatchChar( ',' ) )
                    {
                        ++dim;
                        @this.SkipWhiteSpacesAndJSComments();
                    }
                    if( !@this.TryMatchChar( ']' ) ) return @this.SetError( "Expected closing ']' array." );
                    arrayDim.Add( dim );
                    @this.SkipWhiteSpacesAndJSComments();
                }
                type = new TypeName( knownName, genArgs, arrayDim );
                return true;
            }
            return @this.SetError( "Type name." );
        }

        static bool MatchGenParam( StringMatcher @this, out TypeName.GenParam genArg )
        {
            genArg = TypeName.GenParam.Empty;
            if( @this.TryMatchCSharpIdentifier( out string nameOrVariance ) )
            {
                TypeName.VariantModifier v = TypeName.VariantModifier.None;
                if( nameOrVariance == "out" )
                {
                    v = TypeName.VariantModifier.Out;
                    nameOrVariance = null;
                    @this.SkipWhiteSpacesAndJSComments();
                }
                else if( nameOrVariance == "in" )
                {
                    v = TypeName.VariantModifier.In;
                    nameOrVariance = null;
                    @this.SkipWhiteSpacesAndJSComments();
                }
                if( !@this.MatchTypeName( out var gT, nameOrVariance ) ) return false;
                genArg = new TypeName.GenParam( v, gT );
                return true;
            }
            return false;
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
