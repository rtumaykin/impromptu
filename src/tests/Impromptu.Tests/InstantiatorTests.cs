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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Impromptu.Package;
using Xunit;
using Xunit.Abstractions;

namespace Impromptu.Tests
{
    public class InstantiatorTests : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private readonly string _basePath;
        private const string NugetPackageLocation = @"..\..\..\helpers\Impromptu.Tests.Something\bin";

        public InstantiatorTests(ITestOutputHelper output)
        {
            _output = output;

            _basePath = Path.Combine(Path.GetTempPath(),
                $"Impromptu_Tests_{Guid.NewGuid().ToString("N")}");

            // clean up previous runs (cant do this at the end because the files are locked by the appdomain
            foreach (var directory in Directory.EnumerateDirectories(Path.GetTempPath(), "Impromptu_Tests_*"))
            {
                try
                {
                    Directory.Delete(directory, true);
                }
                catch
                {
                    // ignored
                }
            }
        }
#if DEBUG
        [Fact]
        public void Should_Successfully_Instantiate()
        {
            var configName = AppDomain.CurrentDomain.BaseDirectory.Split('\\').Last();
            var fp = new NugetPackageRetriever(new[] {Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                        NugetPackageLocation, configName)});
            var ha = new InstantiatorFactory<ISomething>(fp, _basePath);
            {
                // let it jit compile
                var z = ha.Instantiate(new InstantiatorKey("Impromptu.Tests.Something", "1.0.0", "Impromptu.Tests.Something1"));
            }

            var start = DateTime.Now;
            for (var i = 0; i < 1000000; i++)
            {
                var z = ha.Instantiate(new InstantiatorKey("Impromptu.Tests.Something", "1.0.0", "Impromptu.Tests.Something1"));
                var x = z.DoSomething();
            }
            var elapsed = DateTime.Now.Subtract(start).TotalMilliseconds;
            _output.WriteLine($"Total elapsed {elapsed} ms.");
        }

        [Fact]
        public void Should_Successfully_Instantiate_Multithreaded()
        {
            var configName = AppDomain.CurrentDomain.BaseDirectory.Split('\\').Last();
            var fp = new NugetPackageRetriever(new[] {Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                        NugetPackageLocation, configName)});

            var ha = new InstantiatorFactory<ISomething>(fp);
            var tasks = new List<Task>();

            var start = DateTime.Now;
            for (var i = 0; i < 1000000; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    var z = ha.Instantiate(new InstantiatorKey("Impromptu.Tests.Something", "1.0.0", "Impromptu.Tests.Something1"));
                    var x = z.DoSomething();
                }));
            }
            Task.WaitAll(tasks.ToArray());

            var elapsed = DateTime.Now.Subtract(start).TotalMilliseconds;
            _output.WriteLine($"Total elapsed {elapsed} ms.");
        }

        [Fact]
        public void Should_Fail_No_Ctor()
        {
            var configName = AppDomain.CurrentDomain.BaseDirectory.Split('\\').Last();
            var fp = new NugetPackageRetriever(new[] {Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                    NugetPackageLocation, configName)});

            var ha = new InstantiatorFactory<ISomething>(fp);
            Exception ex = Assert.Throws<InstantiatorException>(() =>
            {
                var z =
                    ha.Instantiate(
                        new InstantiatorKey("Impromptu.Tests.Something", "1.0.0",
                            "Impromptu.Tests.Something1"), 100);
            }
        );

            Assert.Equal(ex.GetType(), typeof(InstantiatorException));
        }


        [Fact]
        public void Should_Pass_One()
        {
            var configName = AppDomain.CurrentDomain.BaseDirectory.Split('\\').Last();
            var fp = new NugetPackageRetriever(new[] {Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                    NugetPackageLocation, configName)});
            var ha = new InstantiatorFactory<ISomething>(fp);
            var z =
                ha.Instantiate(new InstantiatorKey("Impromptu.Tests.Something", "1.0.0",
                    "Impromptu.Tests.Something1"));
            var x = z.DoSomething();
            var z1 =
                ha.Instantiate(new InstantiatorKey("Impromptu.Tests.Something", "1.0.0",
                    "Impromptu.Tests.Something2"));
            var x1 = z1.DoSomething();
        }
#endif
        public void Dispose()
        {
            //if (Directory.Exists(_basePath))
            //    Directory.Delete(_basePath, true);            
        }
    }
    public class DummyTests : IClassFixture<InstantiatorTests>
    {
        public void SetFixture(InstantiatorTests data)
        {
        }
    }
}

