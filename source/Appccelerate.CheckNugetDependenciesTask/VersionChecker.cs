//-------------------------------------------------------------------------------
// <copyright file="VersionChecker.cs" company="Appccelerate">
//   Copyright (c) 2008-2014
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
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;

    public class VersionChecker
    {
        public VersionCheckerResult MatchVersion(string referenceVersion, string nugetVersion)
        {
            if (string.IsNullOrEmpty(referenceVersion))
            {
                return VersionCheckerResult.CreateSuccessful();
            }

            Regex regex = new Regex(@"\[(?<version>[0-9\.]*)\]");

            var match = regex.Match(nugetVersion);

            if (!match.Success)
            {
                return VersionCheckerResult.CreateFailed(FormatUnsupportedNugetVersion(nugetVersion));
            }

            Version reference;
            if (!Version.TryParse(ExpandToFullVersion(referenceVersion), out reference))
            {
                return VersionCheckerResult.CreateFailed("unable to parse version of reference: `" + referenceVersion + "`.");
            }

            Version exactVersion;
            if (!Version.TryParse(ExpandToFullVersion(match.Groups["version"].Value), out exactVersion))
            {
                return VersionCheckerResult.CreateFailed(FormatUnsupportedNugetVersion(nugetVersion));
            }

            return exactVersion != reference ? 
                VersionCheckerResult.CreateFailed(referenceVersion + " is not equal to " + exactVersion) : 
                VersionCheckerResult.CreateSuccessful();
        }

        private static string FormatUnsupportedNugetVersion(string nugetVersion)
        {
            return "unsupported nuget version format: `" + nugetVersion + "`. Only `[version]` (e.g. [1.2]) with at least major and minor version parts is supported.";
        }

        private static string ExpandToFullVersion(string version)
        {
            while (version.Count(c => c == '.') < 3)
            {
                version += ".0";
            }

            return version;
        }
    }
}