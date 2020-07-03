using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;

namespace CK.CodeGen
{
    /// <summary>
    /// Defines a project definition files (.csproj) and its <see cref="Code"/>.
    /// </summary>
    public interface ICodeProject
    {
        /// <summary>
        /// Gets the code workspace of this project.
        /// </summary>
        ICodeWorkspace Code { get; }

        /// <summary>
        /// Gets the project name: that is the final assembly name (or names if more than one <see cref="TargetFrameworks"/> are defined).
        /// </summary>
        string ProjectName { get; }

        /// <summary>
        /// Gets the set of target frameworks.
        /// Defaults to empty: there must be at least one specified otherwise the generated project will be invalid.
        /// </summary>
        HashSet<string> TargetFrameworks { get; }

        /// <summary>
        /// Gets or sets the file format of the output file. This parameter can have one of the following values:
        /// "Library" (creates a code library, this is the default value), "Exe" (creates a console application),
        /// "Module" (creates a module) and  "Winexe" (creates a Windows-based program).
        /// This defaults to null: no &lt;OutputType&gt; element exists.
        /// </summary>
        string? OutputType { get; set; }

        /// <summary>
        /// Gets or sets the Sdk project attribute. Defaults to "Microsoft.NET.Sdk".
        /// </summary>
        string Sdk { get; set; }

        /// <summary>
        /// Gets or sets the language version.
        /// Defaults to null.
        /// </summary>
        string? LangVersion { get; set; }

        /// <summary>
        /// Additional explicit package references to consider.
        /// This list can contain duplicates. <see cref="UnifiedPackageReferences"/>.
        /// </summary>
        IList<PackageReference> PackageReferences { get; }

        /// <summary>
        /// Computes the unified packages from explicit <see cref="PackageReferences"/> and the ones obtained from
        /// the <see cref="ICodeWorkspace.AssemblyReferences"/> mapped by <see cref="PackageReference.FromAssembly(Assembly)"/>.
        /// Duplicates are mapped to the highest version.
        /// </summary>
        IReadOnlyCollection<PackageReference> UnifiedPackageReferences { get; }

        /// <summary>
        /// Creates the xml csproj (MSBuild) root element.
        /// </summary>
        /// <returns>The root project element.</returns>
        XElement CreateRootElement();

    }
}
