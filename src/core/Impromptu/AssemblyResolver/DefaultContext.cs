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
    /// This class provides all methods to resolve assemblies in the default load context
    /// </summary>
    public static class DefaultContext
    {
        private static readonly object ResolverLock = new object();

        private static bool _resolverWiredUp;

        /// <summary>
        /// This method wires up a Default Load Context resolver
        /// </summary>
        public static void WireUpResolver()
        {
            lock (ResolverLock)
            {
                if (_resolverWiredUp)
                    return;

                AppDomain.CurrentDomain.AssemblyResolve += Resolve;
                _resolverWiredUp = true;
            }
        }

        /// <summary>
        /// Event Handler that handles the AssemblyResolve event raised by the assemblies that are in the BaseDirectory of an application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static Assembly Resolve(object sender, ResolveEventArgs args)
        {
            // only resolve when the request comes from the current AppDomain itself, 
            // or from an assembly that is in the BaseDirectory of the current AppDomain
            // todo: implement the same steps as described here: https://msdn.microsoft.com/en-us/library/yx7xezcf%28v=vs.110%29.aspx?f=255&MSPPError=-2147217396

            // just in case if we end up with a null name
            if (args.Name == null)
                return null;

            // get either a location of a requesting assembly, or a sender AppDomain baseDirectory
            var requestorPath = string.IsNullOrWhiteSpace(args.RequestingAssembly?.Location)
                ? (sender as AppDomain)?.BaseDirectory
                : Path.GetDirectoryName(args.RequestingAssembly?.Location);

            // failed to get the requestor path (unlikely)
            if (string.IsNullOrWhiteSpace(requestorPath))
                return null;

            var domainBaseDirectory =
                Common.NormalizePath(AppDomain.CurrentDomain.BaseDirectory);

            // Now when we have a path, let's compare it to the current AppDomain's base directory
            if (Common.NormalizePath(requestorPath) != domainBaseDirectory)
                return null;

            // Check if this assembly has already been loaded
            var loadedAssembly =
                AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(
                        a => !string.IsNullOrWhiteSpace(a.Location) &&
                                Common.NormalizePath(Path.GetDirectoryName(a.Location)) ==
                                domainBaseDirectory && a.FullName == args.Name);
            if (loadedAssembly != null)
                return loadedAssembly;

            return Common.ResolveByFullAssemblyNameInternal(domainBaseDirectory, args.Name);
        }
    }
}
