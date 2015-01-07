//-------------------------------------------------------------------------------
// <copyright file="VersionChecker.cs" company="Appccelerate">
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

            Regex regex = new Regex(@"\[(?<from>.*),(?<to>.*)\)");

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

            Version from;
            if (!Version.TryParse(ExpandToFullVersion(match.Groups["from"].Value), out from))
            {
                return VersionCheckerResult.CreateFailed(FormatUnsupportedNugetVersion(nugetVersion));
            }

            Version to;
            if (!Version.TryParse(ExpandToFullVersion(match.Groups["to"].Value), out to))
            {
                return VersionCheckerResult.CreateFailed(FormatUnsupportedNugetVersion(nugetVersion));
            }

            if (reference < from)
            {
                return VersionCheckerResult.CreateFailed("reference version `" + referenceVersion + "` is lower than lower bound of nuget version `" + nugetVersion + "`.");
            }

            if (reference != from)
            {
                return VersionCheckerResult.CreateFailed("lower bound of nuget version `" + nugetVersion + "` should equal reference version `" + referenceVersion + "` (otherwise there might be build problems due to different referenced versions of the dependency).");
            }

            return VersionCheckerResult.CreateSuccessful();
        }

        private static string FormatUnsupportedNugetVersion(string nugetVersion)
        {
            return "unsupported nuget version format: `" + nugetVersion + "`. Only `[from,to)` with from and to containing at least major version is currently supported.";
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