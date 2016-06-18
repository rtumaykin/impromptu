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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NuGet;
using Xunit;
using Xunit.Abstractions;

namespace Impromptu.Package.Tests
{
    public class PackageUnitTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly string _basePath;
        private const string NugetPackageLocation = @"..\..\..\helpers\Impromptu.Tests.Something.NugetPackage\bin";

        public PackageUnitTests(ITestOutputHelper output)
        {
            _output = output;

            _basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                $"ImpromptuPackages_Test_{Guid.NewGuid().ToString("N")}");

            Directory.CreateDirectory(_basePath);
        }

        [Theory]
        [InlineData("Impromptu.Tests.Something.NugetPackage", "1.0.0")]
        [InlineData("Impromptu.Tests.Something.NugetPackage", null)]
        public void Success_ConcurrentGet(string packageName, string packageVersion)
        {
            var start = DateTime.Now;
            var threads = new List<Task>();
            for (var i = 0; i < 1000; i++)
            {
                threads.Add(Task.Run(() => CheckRetriever(packageName, packageVersion)));
            }
            Task.WaitAll(threads.ToArray());
            Assert.True(_res.All(r => !string.IsNullOrEmpty(r) && r == Path.Combine(_basePath, "Impromptu.Tests.Something.NugetPackage.1.0.0")));
            _output.WriteLine($"Execution took {(DateTime.Now - start).TotalMilliseconds} milliseconds.");
        }

        private readonly ConcurrentBag<string> _res = new ConcurrentBag<string>();

        private void CheckRetriever(string packageName, string packageVersion)
        {
            var configName = AppDomain.CurrentDomain.BaseDirectory.Split('\\').Last();

            if (string.IsNullOrEmpty(packageVersion))
            {
                _res.Add(new NugetPackageRetriever(new[]
                {
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                        NugetPackageLocation, configName)
                }).Retrieve(
                    _basePath,
                    packageName));
            }
            else
            {
                _res.Add(new NugetPackageRetriever(new[]
                {
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                        NugetPackageLocation, configName)
                }).Retrieve(
                    _basePath,
                    packageName,
                    new SemanticVersion(packageVersion)));
            }
        }

        [Theory]
        [InlineData("Impromptu.Tests.Something.NugetPackage", "1.0.0")]
        [InlineData("Impromptu.Tests.Something.NugetPackage", null)]
        public void Success_GetOne(string packageName, string packageVersion)
        {
            string pak;
            var configName = AppDomain.CurrentDomain.BaseDirectory.Split('\\').Last();

            if (string.IsNullOrEmpty(packageVersion))
            {
                pak = new NugetPackageRetriever(new[]
                {
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                        NugetPackageLocation, configName)
                }).Retrieve(
                    _basePath,
                    packageName);
            }
            else
            {
                pak = new NugetPackageRetriever(new[]
                {
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                        NugetPackageLocation, configName)
                }).Retrieve(
                    _basePath,
                    packageName,
                    new SemanticVersion(packageVersion));
            }

            Assert.True(!string.IsNullOrEmpty(pak) && pak == Path.Combine(_basePath, "Impromptu.Tests.Something.NugetPackage.1.0.0"));
        }

        [Theory]
        [InlineData("Impromptu.Tests.Something.NugetPackage", "2.0.0")]
        [InlineData("Impromptu.Tests.Something.NugetPackagexx", null)]
        public void Fail_GetOne_InvalidPackage(string packageName, string packageVersion)
        {
            string pak;
            var configName = AppDomain.CurrentDomain.BaseDirectory.Split('\\').Last();

            if (string.IsNullOrEmpty(packageVersion))
            {
                pak = new NugetPackageRetriever(new[]
                {
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                        NugetPackageLocation, configName)
                }).Retrieve(
                    _basePath,
                    packageName);
            }
            else
            {
                pak = new NugetPackageRetriever(new[]
                {
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                        NugetPackageLocation, configName)
                }).Retrieve(
                    _basePath,
                    packageName,
                    new SemanticVersion(packageVersion));
            }

            Assert.True(string.IsNullOrEmpty(pak) || pak != Path.Combine(_basePath, "Impromptu.Tests.Something.NugetPackage.1.0.0"));
        }

        public void Dispose()
        {
            Directory.Delete(_basePath, true);
        }
    }
}
