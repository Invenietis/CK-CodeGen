using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CK.CodeGen
{
    class ModifierHelper
    {
        public static void AddFrontMofifiersProtection( MethodInfo baseMethod, List<string> front )
        {
            if( baseMethod.IsPublic ) front.Add( "public" );
            else if( baseMethod.IsFamily ) front.Add( "protected" );
            else if( baseMethod.IsFamilyAndAssembly )
            {
                // This is invalid since we are in another assembly!
                front.Add( "private protected" );
            }
            else if( baseMethod.IsFamilyOrAssembly )
            {
                // Should be "internal protected" but since we are in another assembly,
                // "internal" changes the protection. 
                front.Add( "protected" );
            }
        }
    }
}
