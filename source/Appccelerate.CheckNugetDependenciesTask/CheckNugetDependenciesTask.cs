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

namespace Appccelerate.CheckNugetDependenciesTask
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;

    public class CheckNugetDependenciesTask : Task
    {
        [Required]
        public string ProjectFileFullPath { get; set; }

        [Required]
        public string NuspecFileFullPath { get; set; }

        [Required]
        public string PackagesConfigFullPath { get; set; }

        public override bool Execute()
        {
            try
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

                if (string.IsNullOrEmpty(this.PackagesConfigFullPath))
                {
                    this.WriteError("PackagesConfigFullPath is not set.");
                    return false;
                }

                this.WriteInfo("checking nuspec `" + this.NuspecFileFullPath + "` and project file `" + this.ProjectFileFullPath + "`");

                XDocument nuspec = this.ReadXmlFile(this.NuspecFileFullPath);
                XDocument project = this.ReadXmlFile(this.ProjectFileFullPath);
                XDocument packages = this.ReadXmlFile(this.PackagesConfigFullPath);

                var verifier = new Verifier(new VersionChecker());

                List<Violation> violations = verifier.Verify(project, nuspec, packages).ToList();

                foreach (Violation violation in violations)
                {
                    this.WriteError(violation.Message);
                }

                this.WriteInfo("done checking nuget package dependencies. Found " + violations.Count() + " violations.");

                return !violations.Any();

            }
            catch (Exception exception)
            {
                this.WriteError(exception.ToString());
                return false;
            }
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
    }
}