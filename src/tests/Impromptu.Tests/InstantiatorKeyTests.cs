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
using Xunit;

namespace Impromptu.Tests
{
    public class InstantiatorKeyTests : IDisposable
    {
        [Theory]
        [InlineData("SomePackageName", "1.0", "Some.Type")]
        [InlineData("SomePackageName", "1.0-pre", "Some.Type")]
        [InlineData("SomePackageName", "1.0.1", "Some.Type")]
        [InlineData("SomePackageName", "1.0.0-pre", "Some.Type")]
        [InlineData("SomePackageName", "1.0.0.1", "Some.Type")]
        [InlineData("SomePackageName", "1.0.0.1-pre", "Some.Type")]
        public void Success_NewKey(string packageId, string version, string fullTypeName)
        {
            Exception ex = null;
            try
            {
                new InstantiatorKey(packageId, version, fullTypeName);
            }
            catch (Exception e)
            {
                ex = e;
            }

            Assert.Null(ex);
        }

        [Theory]
        [InlineData("SomePackageName.1", "1.0", "Some.Type")]
        [InlineData("SomePackageName.aaa", "1.0z-pre", "Some.Type")]
        [InlineData("SomePackageName.abc", "1.0z.1", "Some.Type")]
        [InlineData("SomePackageName.abc", "1.0.0z-pre", "Some.Type")]
        [InlineData("SomePackageName.abc", "1.0z.0.1", "Some.Type")]
        [InlineData("SomePackageName.abc", "1.0.0.1z-pre", "Some.Type")]
        [InlineData("SomePackageName", "1.0.0.1", "Some.Type.1")]
        [InlineData("SomePackageName", "1.0.0.1-pre", "Some.1.Type")]
        [InlineData("SomePackageName.1.abc", "1.0.0.1", "Some.Type")]
        public void Fail_NewKey_InvalidPart(string packageId, string version, string fullTypeName)
        {
            Exception ex =
                Assert.Throws<InstantiatorException>(() => new InstantiatorKey(packageId, version, fullTypeName));
            Assert.Equal(ex.GetType(), typeof(InstantiatorException));
        }


        [Theory]
        [InlineData("SomePackageName", "1.0", "Some.Type")]
        [InlineData("SomePackageName", "1.0-pre", "Some.Type")]
        [InlineData("SomePackageName", "1.0.1", "Some.Type")]
        [InlineData("SomePackageName", "1.0.0-pre", "Some.Type")]
        [InlineData("SomePackageName", "1.0.0.1", "Some.Type")]
        [InlineData("SomePackageName", "1.0.0.1-pre", "Some.Type")]
        public void Success_Equal(string packageId, string version, string fullTypeName)
        {
            var key1 = new InstantiatorKey(packageId, version, fullTypeName);
            var key2 = new InstantiatorKey(packageId, version, fullTypeName);

            Assert.Equal(key1, key2);
        }

        [Theory]
        [InlineData("SomePackageName", "1.0", "Some.Type")]
        [InlineData("SomePackageName", "1.0-pre", "Some.Type")]
        [InlineData("SomePackageName", "1.0.1", "Some.Type")]
        [InlineData("SomePackageName", "1.0.0-pre", "Some.Type")]
        [InlineData("SomePackageName", "1.0.0.1", "Some.Type")]
        [InlineData("SomePackageName", "1.0.0.1-pre", "Some.Type")]
        public void Success_EqualToString(string packageId, string version, string fullTypeName)
        {
            var key1 = new InstantiatorKey(packageId, version, fullTypeName);
            var key2 = new InstantiatorKey(packageId, version, fullTypeName);

            Assert.Equal(key1.ToString(), key2.ToString());
        }

        [Theory]
        [InlineData("SomePackageName", "1.0", "Some.Type")]
        [InlineData("SomePackageName", "1.0-pre", "Some.Type")]
        [InlineData("SomePackageName", "1.0.1", "Some.Type")]
        [InlineData("SomePackageName", "1.0.0-pre", "Some.Type")]
        [InlineData("SomePackageName", "1.0.0.1", "Some.Type")]
        [InlineData("SomePackageName", "1.0.0.1-pre", "Some.Type")]
        public void Success_EqualHashCode(string packageId, string version, string fullTypeName)
        {
            var key1 = new InstantiatorKey(packageId, version, fullTypeName);
            var key2 = new InstantiatorKey(packageId, version, fullTypeName);

            Assert.Equal(key1.GetHashCode(), key2.GetHashCode());
        }

        [Theory]
        [InlineData("SomePackageName", "1.0", "Some.Type", "SomePackageName1", "1.0", "Some.Type")]
        [InlineData("SomePackageName", "1.0-pre", "Some.Type", "SomePackageName", "1.1-pre", "Some.Type")]
        [InlineData("SomePackageName", "1.0.1", "Some.Type", "SomePackageName", "1.0.1", "Some.Type1")]
        public void Success_NotEqual(string packageId1, string version1, string fullTypeName1, string packageId2,
            string version2, string fullTypeName2)
        {
            var key1 = new InstantiatorKey(packageId1, version1, fullTypeName1);
            var key2 = new InstantiatorKey(packageId2, version2, fullTypeName2);

            Assert.NotEqual(key1, key2);
        }

        [Theory]
        [InlineData("SomePackageName", "1.0", "Some.Type", "SomePackageName1", "1.0", "Some.Type")]
        [InlineData("SomePackageName", "1.0-pre", "Some.Type", "SomePackageName", "1.1-pre", "Some.Type")]
        [InlineData("SomePackageName", "1.0.1", "Some.Type", "SomePackageName", "1.0.1", "Some.Type1")]
        public void Success_NotEqualToString(string packageId1, string version1, string fullTypeName1, string packageId2,
            string version2, string fullTypeName2)
        {
            var key1 = new InstantiatorKey(packageId1, version1, fullTypeName1);
            var key2 = new InstantiatorKey(packageId2, version2, fullTypeName2);

            Assert.NotEqual(key1.ToString(), key2.ToString());
        }

        public void Dispose()
        {
            // nothing
        }
    }
}
