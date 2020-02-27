using CSemVer;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace CK.CodeGen.Abstractions
{
    /// <summary>
    /// Simple immutable package reference that exposes a centralized mapping from <see cref="Assembly"/> to their packages.
    /// Offers association from <see cref="Assembly"/> to its package thanks to <see cref="SetAssemblyPackage(string, PackageReference)"/>
    /// and <see cref="FromAssembly(Assembly)"/> methods.
    /// </summary>
    public class PackageReference
    {
        static Dictionary<string, PackageReference> _mappings = new Dictionary<string, PackageReference>();

        /// <summary>
        /// Gets the package name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the package version.
        /// </summary>
        public SVersion Version { get; }

        /// <summary>
        /// Initializes a new <see cref="PackageReference"/>.
        /// </summary>
        /// <param name="name">The package name.</param>
        /// <param name="version">The package's version.</param>
        public PackageReference( string name, SVersion version )
        {
            Name = name;
            Version = version;
        }

        /// <summary>
        /// Overridden to return the PackageReference Xml element.
        /// </summary>
        /// <returns>A readable string.</returns>
        public override string ToString() => $"<PackageReference Include=\"{Name}\" Version=\"{Version}\" />";

        /// <summary>
        /// Associates a <see cref="PackageReference"/> to use for an <see cref="Assembly"/>.
        /// This method is thread safe.
        /// </summary>
        /// <param name="assemblyName">The assembly's full name or short name (ie. <see cref="AssemblyName.Name"/>).</param>
        /// <param name="package">The package that must be used.</param>
        public static void SetAssemblyPackage( string assemblyName, PackageReference package )
        {
            lock( _mappings )
            {
                _mappings[assemblyName] = package;
            }
        }

        /// <summary>
        /// Gets a package reference from an assembly. The package may have been explicitely registered via <see cref="SetAssemblyPackage"/>
        /// or built from the <see cref="AssemblyName.Name"/> and <see cref="InformationalVersion.Version"/> or <see cref="AssemblyName.Version"/>.
        /// This method is thread safe.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns>A package reference to use.</returns>
        public static PackageReference FromAssembly( Assembly assembly )
        {
            if( assembly == null ) throw new ArgumentNullException( nameof( assembly ) );
            var name = assembly.GetName();
            lock( _mappings )
            {
                PackageReference result;
                if( _mappings.TryGetValue( name.FullName, out result )
                    || _mappings.TryGetValue( name.Name, out result ) )
                {
                    return result;
                }
            }
            return new PackageReference( name.Name, GetPackageVersion( assembly, name ) );
        }

        static SVersion GetPackageVersion( Assembly a, AssemblyName n )
        {
            var info = InformationalVersion.ReadFromAssembly( a );
            if( info.IsValidSyntax && info.Version.IsValid ) return info.Version;
            var v = n.Version;
            return SVersion.Create( v.Major, v.Minor, v.Build );
        }

    }
}
