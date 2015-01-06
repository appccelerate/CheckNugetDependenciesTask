// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NuspecBuilder.cs" company="Appccelerate">
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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;

    public class NuspecBuilder : IFrameworkAssemblies, INugetDependencies
    {
        private readonly List<Tuple<string, string>> frameworkAssemblies = new List<Tuple<string, string>>();
        private readonly List<Tuple<string, string>> nugetDependencies = new List<Tuple<string, string>>();

        private bool developmentDependency;

        public static INuspecBuilder Create()
        {
            return new NuspecBuilder();
        }

        public IFrameworkAssemblies WithFrameworkAssemblies()
        {
            return this;
        }

        public IFrameworkAssemblies AddFrameworkAssembly(string assemblyName, string targetFramework)
        {
            this.frameworkAssemblies.Add(new Tuple<string, string>(assemblyName, targetFramework));
            return this;
        }

        public INugetDependencies WithNugetDependencies()
        {
            return this;
        }

        public INugetDependencies AddNugetDependency(string id, string version)
        {
            this.nugetDependencies.Add(new Tuple<string, string>(id, version));
            return this;
        }

        public INuspecBuilder AsDevelopmentDependency()
        {
            this.developmentDependency = true;
            return this;
        }

        public XDocument Build()
        {
            return XDocument.Parse(
                @"<?xml version=""1.0""?>
<package xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <metadata xmlns=""http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd"">
    <id>Appccelerate.IO</id>
    <version>0.0.0</version>
    <title>Appccelerate.IO</title>
    <authors>Appccelerate team</authors>
    <owners>Appccelerate team</owners>
    <licenseUrl>http://www.apache.org/licenses/LICENSE-2.0.html</licenseUrl>
    <projectUrl>https://github.com/appccelerate/io</projectUrl>
    <iconUrl>https://github.com/appccelerate/appccelerate/raw/master/docs/nuget.png</iconUrl>
    <requireLicenseAcceptance>true</requireLicenseAcceptance>
    <summary>Evaluation of rules, computations, validations and much more.</summary>
    <description>description</description>
    <releaseNotes>notes</releaseNotes>
    <copyright>Copyright 2012-2014</copyright>
    <tags>Appccelerate</tags>" +
    (this.developmentDependency ? "<developmentDependency>true</developmentDependency>" : string.Empty) +
    "<frameworkAssemblies>" +
                (this.frameworkAssemblies.Any() ?
                     this.frameworkAssemblies
                         .Select(a => "<frameworkAssembly assemblyName=\"" + a.Item1 + "\" targetFramework=\"" + a.Item2 + "\" />")
                         .Aggregate((a, b) => a + Environment.NewLine + b) :
                     string.Empty) +
                @"</frameworkAssemblies>
    <dependencies>" +
                (this.nugetDependencies.Any() ?
                     this.nugetDependencies
                         .Select(d => "<dependency Id=\"" + d.Item1 + "\" Version=\"" + d.Item2 + "\" />")
                         .Aggregate((a, b) => a + Environment.NewLine + b) :
                     string.Empty) +
                @"</dependencies>	
  </metadata>
  <files>
    <file src=""Appccelerate.IO\bin\Release\Appccelerate.IO.dll"" target=""lib\net45"" />
    <file src=""Appccelerate.IO\bin\Release\Appccelerate.IO.pdb"" target=""lib\net45"" />
    <file src=""Appccelerate.IO\bin\Release\Appccelerate.IO.xml"" target=""lib\net45"" />
    <file src=""Appccelerate.IO\**\*.cs"" target=""src"" />
  </files>  
</package>");
        }
    }

    public interface INuspecBuilder
    {
        IFrameworkAssemblies WithFrameworkAssemblies();

        INugetDependencies WithNugetDependencies();

        XDocument Build();

        INuspecBuilder AsDevelopmentDependency();
    }

    public interface IFrameworkAssemblies : INuspecBuilder
    {
        IFrameworkAssemblies AddFrameworkAssembly(string assemblyName, string targetFramework);
    }

    public interface INugetDependencies : INuspecBuilder
    {
        INugetDependencies AddNugetDependency(string id, string version);
    }
}