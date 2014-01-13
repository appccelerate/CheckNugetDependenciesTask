//-------------------------------------------------------------------------------
// <copyright file=""CheckNugetDependenciesTaskFacts.cs"" company=""Appccelerate"">
//   Copyright (c) 2008-2014
//
//   Licensed under the Apache License, Version 2.0 (the ""License"");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an ""AS IS"" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>
//-------------------------------------------------------------------------------

namespace CheckNugetDependenciesTask
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using FluentAssertions;
    using Xunit;

    public class CheckNugetDependenciesTaskFacts
    {
        private const string NuspecFileFullPath = @"c:\nuspec.nuspec";
        private const string ProjectFileFullPath = @"c:\project.csproj";

        private readonly TestableCheckNugetDependenciesTask testee;

        public CheckNugetDependenciesTaskFacts()
        {
            this.testee = new TestableCheckNugetDependenciesTask
                              {
                                  NuspecFileFullPath = NuspecFileFullPath,
                                  ProjectFileFullPath = ProjectFileFullPath
                              };
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
                        .AddNugetReference("Appccelerate.Fundamentals", "1.5.3")
                .Build();

            this.testee.Files.Add(NuspecFileFullPath, nuspec);
            this.testee.Files.Add(ProjectFileFullPath, project);

            bool result = this.testee.Execute();

            result.Should().BeTrue();
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
                    .WithNugetReferences()
                        .AddNugetReference("Appccelerate.Fundamentals", "1.5.3")
                        .AddNugetReference("Ninject", "3.4.5")
                .Build();

            this.testee.Files.Add(NuspecFileFullPath, nuspec);
            this.testee.Files.Add(ProjectFileFullPath, project);

            bool result = this.testee.Execute();
             
            result.Should().BeFalse();
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

            this.testee.Files.Add(NuspecFileFullPath, nuspec);
            this.testee.Files.Add(ProjectFileFullPath, project);

            bool result = this.testee.Execute();

            result.Should().BeFalse();
        }

        [Fact]
        public void Fails_WhenANugetDependencyHasWrongVersion()
        {
            XDocument nuspec = NuspecBuilder
                .Create()
                    .WithNugetDependencies()
                        .AddNugetDependency("Appccelerate.Fundamentals", "[1.0,2.0)")
                .Build();

            XDocument project = ProjectBuilder
                .Create()
                    .WithNugetReferences()
                        .AddNugetReference("Appccelerate.Fundamentals", "3.0")
                .Build();

            this.testee.Files.Add(NuspecFileFullPath, nuspec);
            this.testee.Files.Add(ProjectFileFullPath, project);

            bool result = this.testee.Execute();

            result.Should().BeFalse();
        }

        [Fact]
        public void Fails_WhenNoProjectPathIsSpecified()
        {
            this.testee.ProjectFileFullPath = null;

            bool result = this.testee.Execute();

            result.Should().BeFalse();
            this.testee.ErrorMessage.Should().Be("ProjectFileFullPath is not set.");
        }

        [Fact]
        public void Fails_WhenNoNuspecPathIsSpecified()
        {
            this.testee.NuspecFileFullPath = null;

            bool result = this.testee.Execute();

            result.Should().BeFalse();
            this.testee.ErrorMessage.Should().Be("NuspecFileFullPath is not set.");
        }

        public class TestableCheckNugetDependenciesTask : CheckNugetDependenciesTask
        {
            public TestableCheckNugetDependenciesTask()
            {
                this.Files = new Dictionary<string, XDocument>();
            }

            public string ErrorMessage { get; private set; }

            public IDictionary<string, XDocument> Files
            {
                get;
                private set;
            }

            protected override XDocument ReadXmlFile(string path)
            {
                return this.Files[path];
            }

            protected override void WriteError(string message)
            {
                this.ErrorMessage = message;
            }

            protected override void WriteInfo(string message)
            {
            }
        }
    }
}