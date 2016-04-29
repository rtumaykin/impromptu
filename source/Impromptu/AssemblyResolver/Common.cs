//-----------------------------------------------------------------------
//Copyright 2015-2016 Roman Tumaykin
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
//-----------------------------------------------------------------------
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Impromptu.AssemblyResolver
{
    internal static class Common
    {
        /// <summary>
        /// Event Handler which resolves the assembly full name to an assembly, using the calling assembly path as a 
        /// search folder.
        /// </summary>
        /// <param name="basePath">Base Path where an assembly with a given Full Name is searched in</param>
        /// <param name="assemblyFullName">Assembly Full Name</param>
        /// <returns></returns>
        internal static Assembly ResolveByFullAssemblyNameInternal(string basePath, string assemblyFullName)
        {
            if (string.IsNullOrWhiteSpace(basePath))
                return null;

            AppDomain newDomain = null;

            try
            {
                var newDomainSetup = new AppDomainSetup()
                {
                    ApplicationBase = AppDomain.CurrentDomain.BaseDirectory
                };

                newDomain = AppDomain.CreateDomain(Guid.NewGuid().ToString("N"), null, newDomainSetup);
                var instanceInNewDomain = (ResolverAppDomainAgent)newDomain.CreateInstanceFromAndUnwrap(
                    typeof(ResolverAppDomainAgent).Assembly.Location,
                    typeof(ResolverAppDomainAgent).FullName,
                    true,
                    BindingFlags.Default,
                    null,
                    null,
                    null,
                    null);

                var files = Directory.GetFiles(basePath, "*.*");
                var assemblyPath =
                    files.FirstOrDefault(p => instanceInNewDomain.CompareAssemblyFullName(p, assemblyFullName));

                return !string.IsNullOrWhiteSpace(assemblyPath)
                    ? (basePath == AppDomain.CurrentDomain.BaseDirectory
                        ? Assembly.LoadFrom(assemblyPath)
                        : Assembly.LoadFile(assemblyPath))
                    : null;
            }
            finally
            {
                if (newDomain != null)
                    AppDomain.Unload(newDomain);
            }
        }

        internal static string NormalizePath(string path)
        {
            return Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
    }
    /// <summary>
    /// Agent type that is being injected into a new AppDomain to perform actual search tasks
    /// </summary>
    internal class ResolverAppDomainAgent : MarshalByRefObject
    {
        /// <summary>
        /// Searches in an assembly file (<see cref="assemblyPath"/>) for implementations of <see cref="baseType"/>.
        /// </summary>
        /// <param name="assemblyPath">Path to Assembly</param>
        /// <param name="baseType">Type to search for implementations of.</param>
        /// <returns></returns>
        internal bool DoesAssemblyContainInheritedTypes
            (string assemblyPath, Type baseType)
        {
            try
            {
                var assembly = Assembly.LoadFrom(assemblyPath);
                return
                    assembly.ExportedTypes.Any(
                        t =>
                            t.IsClass &&
                            baseType.IsAssignableFrom(t) &&
                            t.GetConstructors().Any());
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// Retrieves an assembly full name and compares to a provided <see cref="assemblyFullName"/>.
        /// </summary>
        /// <param name="assemblyPath">Assembly File Path</param>
        /// <param name="assemblyFullName">Full Name to compare to</param>
        /// <returns></returns>
        internal bool CompareAssemblyFullName(string assemblyPath, string assemblyFullName)
        {
            try
            {
                var assembly = Assembly.LoadFrom(assemblyPath);
                return assembly.FullName == assemblyFullName;
            }
            catch
            {
                return false;
            }

        }
    }
}
