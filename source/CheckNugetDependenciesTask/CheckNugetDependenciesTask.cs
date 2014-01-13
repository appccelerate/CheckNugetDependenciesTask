//-------------------------------------------------------------------------------
// <copyright file="CheckNugetDependenciesTask.cs" company="Appccelerate">
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
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;

    public class CheckNugetDependenciesTask : Task
    {
        private readonly VersionChecker versionChecker = new VersionChecker();

        public string ProjectFileFullPath
        {
            get;
            set;
        }

        public string NuspecFileFullPath
        {
            get;
            set;
        }

        public override bool Execute()
        {
            if (string.IsNullOrEmpty(this.ProjectFileFullPath))
            {
                this.WriteError("ProjectFileFullPath is not set.");
                return false;
            }

            if (string.IsNullOrEmpty(this.NuspecFileFullPath))
            {
                this.WriteError("NuspecFileFullPath is not set.");
                return false;
            }

            this.WriteInfo("checking nuspec `" + this.NuspecFileFullPath + "` and project file `" + this.ProjectFileFullPath + "`");

            bool succeeded = true;

            XDocument nuspec = this.ReadXmlFile(this.NuspecFileFullPath);
            XDocument project = this.ReadXmlFile(this.ProjectFileFullPath);
            
            List<Tuple<string, string>> neededReferences = GetNeededReferences(project);

            var frameworkReferences = this.GetFrameworkReferences(nuspec);
            var nugetReferences = this.GetNugetReferences(nuspec);

            foreach (Tuple<string, string> neededReference in neededReferences)
            {
                if (frameworkReferences.Any(r => r.Item1 == neededReference.Item1))
                {
                    continue;    
                }

                if (nugetReferences.Any(r => r.Item1 == neededReference.Item1))
                {
                    string nugetVersion = nugetReferences.First().Item2;
                    string referenceVersion = neededReference.Item2;

                    VersionCheckerResult checkerResult = this.versionChecker.MatchVersion(referenceVersion, nugetVersion);

                    if (checkerResult.Success)
                    {
                        continue;
                    }
                    
                    this.WriteError("wrong version found for `" + neededReference.Item1 + "`: " + checkerResult.ErrorMessage);
                    succeeded = false;
                    continue;
                }

                this.WriteError("missing reference in nuspec file `" + neededReference.Item1 + "`.");
                succeeded = false;
            }

            return succeeded;
        }

        protected virtual XDocument ReadXmlFile(string path)
        {
            return XDocument.Load(path);
        }

        protected virtual void WriteError(string message)
        {
            this.Log.LogError(message);
        }

        protected virtual void WriteInfo(string message)
        {
            this.Log.LogMessage(MessageImportance.Low, message);
        }

        private List<Tuple<string, string>> GetNeededReferences(XDocument project)
        {
            this.WriteInfo("needed references:");

            var ns = XNamespace.Get("http://schemas.microsoft.com/developer/msbuild/2003");

            IEnumerable<XElement> references = project.Descendants(ns + "Reference");

            var neededReferences = new List<Tuple<string, string>>();
            foreach (XElement reference in references)
            {
                string r = reference.GetAttributeValue("Include");

                string version = null;

                int indexOfComma = r.IndexOf(',');
                if (indexOfComma >= 0)
                {
                    Regex regex = new Regex("(?<assembly>.*), Version=(?<version>.*), Culture.*");
                    Match match = regex.Match(r);
                    if (match.Success)
                    {
                        r = match.Groups["assembly"].Value;
                        version = match.Groups["version"].Value;
                    }
                    else
                    {
                        this.WriteError("couldn't identify reference " + reference.GetAttributeValue("Include"));
                    }
                }

                neededReferences.Add(new Tuple<string, string>(r, version));

                this.WriteInfo(r + " " + version);
            }

            return neededReferences;
        }

        private List<Tuple<string, string>> GetFrameworkReferences(XDocument nuspec)
        {
            this.WriteInfo("framework references:");

            List<Tuple<string, string>> references = new List<Tuple<string, string>>();

            var ns = XNamespace.Get("http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd");

            var frameworkAssemblies = nuspec.Descendants(ns + "frameworkAssembly");
            
            foreach (var frameworkAssembly in frameworkAssemblies)
            {
                string assemblyName = frameworkAssembly.GetAttributeValue("assemblyName");

                string targetFramework = frameworkAssembly.GetAttributeValue("targetFramework");

                references.Add(new Tuple<string, string>(assemblyName, targetFramework));

                this.WriteInfo(assemblyName + " " + targetFramework);
            }

            return references;
        }

        private List<Tuple<string, string>> GetNugetReferences(XDocument nuspec)
        {
            this.WriteInfo("nuget references:");

            List<Tuple<string, string>> references = new List<Tuple<string, string>>();
            
            var ns = XNamespace.Get("http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd");

            var nugetDependencies = nuspec.Descendants(ns + "dependency");

            foreach (var nugetDependency in nugetDependencies)
            {
                string id = nugetDependency.GetAttributeValue("Id");

                string version = nugetDependency.GetAttributeValue("version");

                references.Add(new Tuple<string, string>(id, version));

                this.WriteInfo(id + " " + version);
            }

            return references;
        }

    }

    public static class XmlExtensions
    {
        public static string GetAttributeValue(this XElement element, string name)
        {
            var attribute = element.Attributes().SingleOrDefault(
                a => String.Equals(a.Name.LocalName, name, StringComparison.InvariantCultureIgnoreCase));

            return attribute != null ? attribute.Value : null;
        }
    }
}