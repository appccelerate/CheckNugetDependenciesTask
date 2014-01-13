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

namespace CheckNugetDependenciesTask
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;

    public class VersionChecker
    {
        public VersionCheckerResult MatchVersion(string referenceVersion, string nugetVersion)
        {
            Regex regex = new Regex(@"\[(?<from>.*),(?<to>.*)\)");

            var match = regex.Match(nugetVersion);

            if (!match.Success)
            {
                return VersionCheckerResult.CreateFailed(FormatUnsupportedNugetVersion(nugetVersion));
            }

            Version reference = Version.Parse(referenceVersion);

            try
            {
                Version from = Version.Parse(match.Groups["from"].Value);
                Version to = Version.Parse(match.Groups["to"].Value);

                if (from > reference || reference >= to)
                {
                    return VersionCheckerResult.CreateFailed(referenceVersion + " is outside " + nugetVersion);
                }
            }
            catch (ArgumentException)
            {
                return VersionCheckerResult.CreateFailed(FormatUnsupportedNugetVersion(nugetVersion));
            }
        
            return VersionCheckerResult.CreateSuccessful();
        }

        public static string FormatUnsupportedNugetVersion(string nugetVersion)
        {
            return "unsupported nuget version format: `" + nugetVersion + "`. Only `[from,to)` with from and to containing at least major and minor version (e.g. 1.2) is currently supported";
        }
    }
}