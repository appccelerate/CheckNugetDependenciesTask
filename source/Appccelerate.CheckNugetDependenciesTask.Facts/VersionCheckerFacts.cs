// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VersionCheckerFacts.cs" company="Appccelerate">
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
// --------------------------------------------------------------------------------------------------------------------

namespace Appccelerate.CheckNugetDependenciesTask
{
    using FluentAssertions;
    using Xunit.Extensions;

    public class VersionCheckerFacts
    {
        private readonly VersionChecker testee;

        public VersionCheckerFacts()
        {
            this.testee = new VersionChecker();
        }

        [Theory]
        [InlineData("[1.0.0]", "1.0", true, null)]
        [InlineData("[3.5]", "3.5", true, null)]
        [InlineData("[3.5]", "3.5.0", true, null)]
        [InlineData("[3.5]", "3.5.0.0", true, null)]
        [InlineData("[1]", "1.0", true, null)]
        [InlineData("[1.0]", "1.1.0.0", false, "1.1.0.0 is not equal to 1.0.0.0")]
        [InlineData("3.5.1", "2.0.0.0", false, "unsupported nuget version format: `3.5.1`. Only `[version]` (e.g. [1.2]) with at least major and minor version parts is supported.")]
        [InlineData("[1.0,2.0)", "2.0.0.0", false, "unsupported nuget version format: `[1.0,2.0)`. Only `[version]` (e.g. [1.2]) with at least major and minor version parts is supported.")]
        [InlineData("[1.0,2.0]", "2.0.0.0", false, "unsupported nuget version format: `[1.0,2.0]`. Only `[version]` (e.g. [1.2]) with at least major and minor version parts is supported.")]
        [InlineData("[1,2)", "2.0.0.0", false, "unsupported nuget version format: `[1,2)`. Only `[version]` (e.g. [1.2]) with at least major and minor version parts is supported.")]
        public void MatchesExactVersion(string nugetVersion, string referenceVersion, bool match, string errorMessage)
        {
            VersionCheckerResult result = this.testee.MatchVersion(referenceVersion, nugetVersion);

            result.Success.Should().Be(match);
            result.ErrorMessage.Should().Be(errorMessage);
        }
    }
}