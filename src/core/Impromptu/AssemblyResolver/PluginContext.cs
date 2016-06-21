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
    /// <summary>
    /// Helper class to perform actions related to Assembly reference resolution
    /// </summary>
    public static class PluginContext<T> where T : class
    {
        private static readonly string SharedTypeAssemblyName = typeof(T).Assembly.FullName;

        /// <summary>
        /// Event Handler which resolves the assembly full name to an assembly, using the calling assembly path as a 
        /// search folder. Search is only performed if the caller path is the same as ResolveInFolder path
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="args">Resolve Parameters</param>
        /// <returns></returns>
        public static Assembly ResolveByFullAssemblyName(object sender, ResolveEventArgs args)
        {
            // pass the resolution to default context if this is a shared type
            if (args.RequestingAssembly == null || args.Name == SharedTypeAssemblyName)
            {
                var newArgs = new ResolveEventArgs(args.Name);
                return DefaultContext.Resolve(AppDomain.CurrentDomain, newArgs);
            }

            var searchInPath = Common.NormalizePath(Path.GetDirectoryName(args.RequestingAssembly.Location));

            // Check if this assembly has already been loaded
            Assembly loadedAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => !a.IsDynamic && !string.IsNullOrWhiteSpace(a.Location) && Common.NormalizePath(Path.GetDirectoryName(a.Location)) == searchInPath && a.FullName == args.Name);

            return loadedAssembly ?? Common.ResolveByFullAssemblyNameInternal(searchInPath, args.Name);
        }



        /// <summary>
        /// Discovers all assemblies, located in the specified folder and contain types derived from <see cref="T"/>.
        /// </summary>
        /// <returns></returns>
        public static Assembly[] DiscoverHotAssemblies(string folder)
        {
            var baseType = typeof(T);

            var files = Directory.GetFiles(folder, "*.*");
            return files.Where(
                p =>
                {
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

                        return instanceInNewDomain.DoesAssemblyContainInheritedTypes(p,
                            baseType);
                    }
                    finally
                    {
                        if (newDomain != null)
                            AppDomain.Unload(newDomain);
                    }
                })
                .Select(Assembly.LoadFile)
                .ToArray();
        }
    }


}
