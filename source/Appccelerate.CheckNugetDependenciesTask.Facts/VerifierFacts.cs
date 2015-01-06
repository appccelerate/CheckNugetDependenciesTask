// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VerifierFacts.cs" company="Appccelerate">
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
// --------------------------------------------------------------------------------------------------------------------

namespace Appccelerate.CheckNugetDependenciesTask
{
    using System.Collections.Generic;
    using System.Xml.Linq;
    using FakeItEasy;
    using FluentAssertions;
    using Xunit;

    public class VerifierFacts
    {
        private readonly Verifier testee;

        private readonly VersionChecker versionChecker;

        public VerifierFacts()
        {
            this.versionChecker = A.Fake<VersionChecker>();
            A.CallTo(() => this.versionChecker.MatchVersion(A<string>._, A<string>._)).Returns(VersionCheckerResult.CreateSuccessful());

            this.testee = new Verifier(this.versionChecker);
        }

        [Fact]
        public void Succeeds_WhenAllReferencesAreFoundInNuspec()
        {
            XDocument nuspec = NuspecBuilder
                .Create()
                    .WithFrameworkAssemblies()
                        .AddFrameworkAssembly("System", "net45")
                        .AddFrameworkAssembly("System.Core", "net45")
                    .WithNugetDependencies()
                        .AddNugetDependency("Appccelerate.Fundamentals", "[1.0,2.0)")
                .Build();

            XDocument project = ProjectBuilder
                .Create()
                    .WithFrameworkReferences()
                        .AddFrameworkReference("System")
                        .AddFrameworkReference("System.Core")
                    .WithNugetReferences()
                        .AddNugetReference("MustBeIgnored")
                .Build();

            XDocument packagesConfig = PackageConfigBuilder
                .Create()
                    .AddReference("Appccelerate.Fundamentals", "1.1.0")
                .Build();

            IEnumerable<Violation> result = this.testee.Verify(project, nuspec, packagesConfig);

            result.Should().BeEmpty();
        }

        [Fact]
        public void IgnoresDevelopmentDependencies()
        {
            XDocument nuspec = NuspecBuilder
                .Create()
                .Build();

            XDocument project = ProjectBuilder
                .Create()
                .Build();

            XDocument packagesConfig = PackageConfigBuilder
                .Create()
                    .AddDevelopmentReference("Appccelerate.Development", "1.0")
                .Build();

            IEnumerable<Violation> result = this.testee.Verify(project, nuspec, packagesConfig);

            result.Should().BeEmpty();
        }

        [Fact]
        public void Fails_WhenANugetDependencyIsMissingInNuspecFile()
        {
            XDocument nuspec = NuspecBuilder
                .Create()
                    .WithNugetDependencies()
                        .AddNugetDependency("Appccelerate.Fundamentals", "[1,2)")
                .Build();

            XDocument project = ProjectBuilder
                .Create()
                .Build();

            XDocument packagesConfig = PackageConfigBuilder
                .Create()
                    .AddReference("Appccelerate.Fundamentals", "1.1.0")
                    .AddReference("Ninject", "3.1.2")
                .Build();

            IEnumerable<Violation> result = this.testee.Verify(project, nuspec, packagesConfig);

            result.Should().BeEquivalentTo(Verifier.CreateMissingNugetReferenceViolation("Ninject"));
        }

        [Fact]
        public void Fails_WhenAFrameworkReferenceIsMissingInNuspecFile()
        {
            XDocument nuspec = NuspecBuilder
                .Create()
                    .WithFrameworkAssemblies()
                        .AddFrameworkAssembly("System", "net45")
                .Build();

            XDocument project = ProjectBuilder
                .Create()
                    .WithFrameworkReferences()
                        .AddFrameworkReference("System")
                        .AddFrameworkReference("System.Core")
                .Build();

            XDocument packagesConfig = PackageConfigBuilder
                .Create()
                .Build();

            IEnumerable<Violation> result = this.testee.Verify(project, nuspec, packagesConfig);

            result.Should().BeEquivalentTo(Verifier.CreateMissingFrameworkReferenceViolation("System.Core"));
        }

        [Fact]
        public void Fails_WhenANugetDependencyHasWrongVersion()
        {
            const string VersionCheckerErrorMessage = "mismatch";
            const string NugetReference = "Appccelerate.Fundamentals";
            const string NugetVersion = "[1.0,2.0)";
            const string ReferenceVersion = "3.0";

            XDocument nuspec = NuspecBuilder
                .Create()
                    .WithNugetDependencies()
                        .AddNugetDependency(NugetReference, NugetVersion)
                .Build();
            
            XDocument project = ProjectBuilder
                .Create()
                    .WithNugetReferences()
                        .AddNugetReference(NugetReference)
                .Build();

            XDocument packagesConfig = PackageConfigBuilder
                .Create()
                    .AddReference(NugetReference, ReferenceVersion)
                .Build();

            A.CallTo(() => this.versionChecker.MatchVersion(ReferenceVersion, NugetVersion))
                .Returns(VersionCheckerResult.CreateFailed(VersionCheckerErrorMessage));

            IEnumerable<Violation> result = this.testee.Verify(project, nuspec, packagesConfig);

            result.Should().BeEquivalentTo(Verifier.CreateVersionMismatchViolation(NugetReference, VersionCheckerErrorMessage));
        }

        [Fact]
        public void Succeeds_WhenPackageIsMarkedAsDevelopmentDependency()
        {
            const string NugetReference = "Appccelerate.Fundamentals";
            const string ReferenceVersion = "3.0";

            XDocument nuspec = NuspecBuilder
                .Create()
                    .AsDevelopmentDependency()
                .Build();

            XDocument project = ProjectBuilder
                .Create()
                    .WithNugetReferences()
                        .AddNugetReference(NugetReference)
                .Build();

            XDocument packagesConfig = PackageConfigBuilder
                .Create()
                    .AddReference(NugetReference, ReferenceVersion)
                .Build();

            IEnumerable<Violation> result = this.testee.Verify(project, nuspec, packagesConfig);

            result.Should().BeEmpty();
        }
    }
}