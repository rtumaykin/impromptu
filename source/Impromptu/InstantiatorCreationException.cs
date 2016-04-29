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

namespace Impromptu
{
    /// <summary>
    /// This exception is raised when an instantiator creation encounters a problem
    /// </summary>
    [Serializable]
    public class InstantiatorCreationException : Exception
    {
        /// <summary>
        /// Creates a new instance of <see cref="InstantiatorCreationException"/> with original exception.
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="originalException">Original Exception</param>
        public InstantiatorCreationException(string message, Exception originalException)
            : base(message, originalException)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="InstantiatorCreationException"/>.
        /// </summary>
        /// <param name="message">Message</param>
        public InstantiatorCreationException(string message)
            : base(message)
        {
        }

    }
}
