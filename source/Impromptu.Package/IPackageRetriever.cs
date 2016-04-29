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

using NuGet;

namespace Impromptu.Package
{
    /// <summary>
    /// Base interface for <see cref="Impromptu.Package"/> package retrieval implementations
    /// </summary>
    public interface IPackageRetriever
    {
        /// <summary>
        /// Retrieves package by PackageId and Version and unpacks it into a subfolder of the Destination Path 
        /// </summary>
        /// <param name="destinationBasePath">Path to the root HotAssembly Package directory</param>
        /// <param name="packageId">Package Id</param>
        /// <param name="version">Package Version</param>
        /// <returns>Path to the package directory, or null if the package was not found</returns>
        string Retrieve(string destinationBasePath, string packageId, SemanticVersion version);
        /// <summary>
        /// Retrieves the recent version of the package by PackageId and unpacks it into a subfolder of the Destination Path 
        /// </summary>
        /// <param name="destinationBasePath">Path to the root HotAssembly Package directory</param>
        /// <param name="packageId">Package Id</param>
        /// <returns>Path to the package directory, or null if the package was not found</returns>
        string Retrieve(string destinationBasePath, string packageId);
    }
}
