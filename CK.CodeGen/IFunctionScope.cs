namespace CK.CodeGen;

/// <summary>
/// A function is defined in a <see cref="ITypeScope"/> or in another <see cref="IFunctionScope"/>.
/// A function can be a constructor.
/// </summary>
public interface IFunctionScope : IFunctionDefinerScope, ICodeWriter
{
    /// <summary>
    /// Gets the function name description.
    /// </summary>
    FunctionDefinition Definition { get; }

    /// <summary>
    /// Gets the closest type that contains this function or method.
    /// </summary>
    ITypeScope EnclosingType { get; }

    /// <summary>
    /// Gets whether this function is a local function.
    /// </summary>
    bool IsLocalFunction { get; }

    /// <summary>
    /// Gets whether this function is a constructor (its <see cref="FunctionDefinition.ReturnType"/> is null).
    /// </summary>
    bool IsConstructor { get; }

    /// <summary>
    /// Creates a segment of code inside this function.
    /// </summary>
    /// <param name="top">
    /// Optionally creates the new part at the start of the code instead of at the
    /// current writing position in the code.
    /// </param>
    /// <returns>The function part to use.</returns>
    IFunctionScopePart CreatePart( bool top = false );

    /// <summary>
    /// Returns the body of the function.
    /// </summary>
    /// <returns>The function body.</returns>
    string ToString();
}
