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
using Impromptu.Tests;
using Xunit;
using Xunit.Abstractions;

namespace Impromptu.BaselneTest
{
    public class BaselineTest
    {
        private readonly ITestOutputHelper _output;

        public BaselineTest(ITestOutputHelper output)
        {
            _output = output;
        }

        /// <summary>
        /// Helps to measure performance of a regular class instantiation
        /// </summary>
        [Fact]
        public void LocalInvocation()
        {
            var start = DateTime.Now;
            for (int i = 0; i < 1000000; i++)
            {
                var z = (ISomething)new Something1();
                var x = z.DoSomething();
            }
            var elapsed = DateTime.Now.Subtract(start).TotalMilliseconds;
            _output.WriteLine($"elapsed {elapsed} ms");
        }
    }
}
