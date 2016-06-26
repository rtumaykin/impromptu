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
using System.Threading;
using NuGet;

namespace Impromptu.Package
{
    /// <summary>
    /// NugetPackageRetriever retrieves and stores locally a requested nuget package from a NuGet native repository
    /// </summary>
    [Serializable]
    public class NugetPackageRetriever : IPackageRetriever
    {
        private readonly string[] _repositories;

        /// <summary>
        /// Reads Impromptu/NuGetRepos section from the application 
        /// configuration file to initialize the repositories. If the section does not 
        /// exist or not configured, it initialized with default NuGet Url
        /// "https://packages.nuget.org/api/v2"
        /// </summary>
        public NugetPackageRetriever()
        {
            _repositories = new[] { "https://packages.nuget.org/api/v2" };
        }
        /// <summary>
        /// Initializes the class using a collection of paths to the NuGet repositories
        /// </summary>
        /// <param name="repositories"></param>
        public NugetPackageRetriever(string[] repositories)
        {
            _repositories = repositories;
        }

        /// <summary>
        /// Retrieves package from a NuGet native repository, using PackageId and Version and unpacks it into a subfolder of the Destination Path.
        /// </summary>
        /// <param name="destinationBasePath">Path to the root Impromptu Package directory</param>
        /// <param name="packageId">Package Id</param>
        /// <param name="version">Package Version</param>
        /// <returns>Path to the package directory, or null if the package was not found</returns>
        public string Retrieve(string destinationBasePath, string packageId, SemanticVersion version)
        {
            Directory.CreateDirectory(destinationBasePath);
            foreach (var repositoryPath in _repositories)
            {
                var repo = PackageRepositoryFactory.Default.CreateRepository(repositoryPath);
                var package = version == null ? repo.FindPackage(packageId) : repo.FindPackage(packageId, version);

                if (package != null)
                {
                    var packageDestinationFolder = Path.Combine(destinationBasePath, $"{package.Id}.{package.Version}");

                    var now = DateTime.Now;
                    // ultimately either this or another process will end up creating this directory
                    while (!Directory.Exists(packageDestinationFolder) && (DateTime.Now - now).TotalSeconds < 30)
                    {
                        var lockFileName = $"{packageDestinationFolder}.lock";
                        // if file does not exist then we can create it and lock 
                        if (!File.Exists(lockFileName))
                        {
                            try
                            {
                                // use this to lock the 
                                using (File.Create(lockFileName, 1024, FileOptions.DeleteOnClose))
                                {
                                    if (Directory.Exists(packageDestinationFolder))
                                        return packageDestinationFolder;
                                    try
                                    {
                                        // by now other process might have created this folder and unpacked the package
                                        package.ExtractContents(new PhysicalFileSystem(destinationBasePath),
                                            packageDestinationFolder);

                                        return packageDestinationFolder;
                                    }
                                    catch (Exception)
                                    {
                                        // Cleanup
                                        if (Directory.Exists(packageDestinationFolder))
                                            Directory.Delete(packageDestinationFolder, true);
                                        // continue --> Directory has not been created so we will run another loop
                                    }
                                }

                            }
                            catch (Exception)
                            {
                                // suppress the error. All we need to know is that we can't create lock file
                            }
                        }
                        else
                        {
                            Thread.Sleep(50);
                        }
                    }
                    return Directory.Exists(packageDestinationFolder) ? packageDestinationFolder : null;
                }
            }
            // package not found
            return null;
        }

        /// <summary>
        /// Retrieves the latest package version from a NuGet native repository, using PackageId and unpacks it into a subfolder of the Destination Path.
        /// </summary>
        /// <param name="destinationBasePath">Path to the root Impromptu Package directory</param>
        /// <param name="packageId">Package Id</param>
        /// <returns>Path to the package directory, or null if the package was not found</returns>
        public string Retrieve(string destinationBasePath, string packageId)
        {
            return Retrieve(destinationBasePath, packageId, null);
        }
    }
}
