//-------------------------------------------------------------------------------
// <copyright file="VersionCheckerResult.cs" company="Appccelerate">
//   Copyright (c) 2008-2015
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
// </copyright>
//-------------------------------------------------------------------------------

namespace Appccelerate.CheckNugetDependenciesTask
{
    public class VersionCheckerResult
    {
        private VersionCheckerResult()
        {
        }

        public bool Success { get; private set; }

        public string ErrorMessage { get; private set; }

        public static VersionCheckerResult CreateSuccessful()
        {
            return new VersionCheckerResult
                       {
                           Success = true,
                           ErrorMessage = null
                       };
        }

        public static VersionCheckerResult CreateFailed(string errorMessage)
        {
            return new VersionCheckerResult
                       {
                           Success = false,
                           ErrorMessage = errorMessage
                       };
        }
    }
}