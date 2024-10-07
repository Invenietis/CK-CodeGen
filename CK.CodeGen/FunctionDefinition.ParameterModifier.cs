using System;

namespace CK.CodeGen;

public partial class FunctionDefinition
{
    /// <summary>
    /// Parameter's modifier.
    /// </summary>
    [Flags]
    public enum ParameterModifier
    {
        /// <summary>
        /// No modifiers.
        /// </summary>
        None,

        /// <summary>
        /// Out parameter.
        /// </summary>
        Out = 1 << 0,

        /// <summary>
        /// Ref parameter.
        /// </summary>
        Ref = 1 << 1,

        /// <summary>
        /// Multiple 'params' modifier. 
        /// </summary>
        Params = 1 << 2,

        /// <summary>
        /// This (extension method) modifier.
        /// </summary>
        This = 1 << 3,

        /// <summary>
        /// In modifier.
        /// </summary>
        In = 1 << 4,

        /// <summary>
        /// Scoped modifier.
        /// </summary>
        Scoped = 1 << 5
    }
}
