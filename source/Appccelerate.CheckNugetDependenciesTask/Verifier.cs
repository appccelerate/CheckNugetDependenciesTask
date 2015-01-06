// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Verifier.cs" company="Appccelerate">
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

    public class Verifier
    {
        private readonly VersionChecker versionChecker;

        public Verifier(VersionChecker versionChecker)
        {
            this.versionChecker = versionChecker;
        }

        public static Violation CreateMissingFrameworkReferenceViolation(string frameworkReference)
        {
            return new Violation("missing framework reference `" + frameworkReference + "`.");
        }

        public static Violation CreateVersionMismatchViolation(string nugetReference, string versionCheckerErrorMessage)
        {
            return new Violation("wrong version found for `" + nugetReference + "`: " + versionCheckerErrorMessage);
        }
        
        public static Violation CreateMissingNugetReferenceViolation(string nugetReference)
        {
            return new Violation("missing reference in nuspec file `" + nugetReference + "`.");
        }

        public IEnumerable<Violation> Verify(XDocument project, XDocument nuspec, XDocument packages)
        {
            if (this.IsDevelopmentDependency(nuspec))
            {
                yield break;
            }

            IEnumerable<string> neededFrameworkReferences = this.GetNeededFrameworkReferences(project);
            IEnumerable<Tuple<string, string>> neededNuspecReferences = this.GetNeededNugetReferences(packages);

            List<Tuple<string, string>> frameworkReferences = this.GetFrameworkReferences(nuspec);
            List<Tuple<string, string>> nugetReferences = this.GetNugetReferences(nuspec);

            foreach (string neededFrameworkReference in neededFrameworkReferences)
            {
                if (frameworkReferences.All(r => r.Item1 != neededFrameworkReference))
                {
                    yield return CreateMissingFrameworkReferenceViolation(neededFrameworkReference);
                }
            }

            foreach (Tuple<string, string> neededNugetReference in neededNuspecReferences)
            {
                Tuple<string, string> match = nugetReferences.FirstOrDefault(r => r.Item1 == neededNugetReference.Item1);
                if (match != null)
                {
                    string nugetVersion = match.Item2;
                    string referenceVersion = neededNugetReference.Item2;

                    VersionCheckerResult checkerResult = this.versionChecker.MatchVersion(referenceVersion, nugetVersion);

                    if (checkerResult.Success)
                    {
                        continue;
                    }

                    yield return CreateVersionMismatchViolation(neededNugetReference.Item1, checkerResult.ErrorMessage);
                }
                else
                {
                    yield return CreateMissingNugetReferenceViolation(neededNugetReference.Item1);
                }
            }
        }

        private bool IsDevelopmentDependency(XDocument nuspec)
        {
            var ns = XNamespace.Get("http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd");

            return nuspec.Descendants(ns + "developmentDependency").Any(dd => dd.Value == "true");
        }

        private IEnumerable<string> GetNeededFrameworkReferences(XDocument project)
        {
            var ns = XNamespace.Get("http://schemas.microsoft.com/developer/msbuild/2003");

            IEnumerable<XElement> references = project.Descendants(ns + "Reference");

            foreach (XElement reference in references)
            {
                string id = reference.GetAttributeValue("Include");

                bool existsHintPath = reference.Element(ns + "HintPath") != null;

                if (!existsHintPath)
                {
                    yield return id;
                }
            }
        }

        private IEnumerable<Tuple<string, string>> GetNeededNugetReferences(XDocument packagesConfig)
        {
            var packages = packagesConfig.Descendants("package");

            foreach (XElement package in packages)
            {
                string developmentDependency = package.GetAttributeValue("developmentDependency");

                if (developmentDependency != "true")
                {
                    string id = package.GetAttributeValue("id");
                    string version = package.GetAttributeValue("version");
                    yield return new Tuple<string, string>(id, version);
                }
            }
        }

        private List<Tuple<string, string>> GetFrameworkReferences(XDocument nuspec)
        {
            List<Tuple<string, string>> references = new List<Tuple<string, string>>();

            var ns = XNamespace.Get("http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd");

            var frameworkAssemblies = nuspec.Descendants(ns + "frameworkAssembly");

            foreach (var frameworkAssembly in frameworkAssemblies)
            {
                string assemblyName = frameworkAssembly.GetAttributeValue("assemblyName");

                string targetFramework = frameworkAssembly.GetAttributeValue("targetFramework");

                references.Add(new Tuple<string, string>(assemblyName, targetFramework));
            }

            return references;
        }

        private List<Tuple<string, string>> GetNugetReferences(XDocument nuspec)
        {
            List<Tuple<string, string>> references = new List<Tuple<string, string>>();

            var ns = XNamespace.Get("http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd");

            var nugetDependencies = nuspec.Descendants(ns + "dependency");

            foreach (var nugetDependency in nugetDependencies)
            {
                string id = nugetDependency.GetAttributeValue("Id");

                string version = nugetDependency.GetAttributeValue("version");

                references.Add(new Tuple<string, string>(id, version));
            }

            return references;
        }
    }
}