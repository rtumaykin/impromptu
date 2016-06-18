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
using System.Text.RegularExpressions;
using NuGet;

namespace Impromptu
{
    public class InstantiatorKey
    {
        public string PackageId { get; }

        public string Version { get; }

        public string FullTypeName { get; }

        public override bool Equals(object obj)
        {
            var typedObj = obj as InstantiatorKey;

            return typedObj != null && typedObj.PackageId == PackageId && typedObj.Version == Version && typedObj.FullTypeName == FullTypeName;
        }

        public override string ToString()
        {
            return $"{PackageId}.{Version}.{FullTypeName}";
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public InstantiatorKey(string packageId, string version, string fullTypeName)
        {
            if (!Regex.IsMatch(packageId, @"^(@?[a-z_A-Z]\w+(?:\.@?[a-z_A-Z]\w+)*)$"))
                throw new InstantiatorException($"\"{packageId}\" is not a valid Package Name", null);

            if (!Regex.IsMatch(fullTypeName, @"^(@?[a-z_A-Z]\w+(?:\.@?[a-z_A-Z]\w+)*)$"))
                throw new InstantiatorException($"\"{fullTypeName}\" is not a valid C# Type Full Name", null);
            try
            {
                Version = SemanticVersion.Parse(version).ToNormalizedString();
            }
            catch (Exception e)
            {
                throw new InstantiatorException($"\"{version}\" is not a valid version", e);
            }

            PackageId = packageId;
            FullTypeName = fullTypeName;
        }
    }
}
