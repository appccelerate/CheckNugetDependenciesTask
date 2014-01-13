// -------------------------------------------------------------------------------
//  <copyright file="ProjectBuilder.cs" company="Appccelerate">
//    Copyright (c) 2008-2014
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
//  </copyright>
// -------------------------------------------------------------------------------
namespace CheckNugetDependenciesTask
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;

    public class ProjectBuilder : IFrameworkReferences, INugetReferences
    {
        private readonly List<string> references = new List<string>();

        public static IProjectBuilder Create()
        {
            return new ProjectBuilder();
        }

        public IFrameworkReferences WithFrameworkReferences()
        {
            return this;
        }

        public INugetReferences WithNugetReferences()
        {
            return this;
        }

        public IFrameworkReferences AddFrameworkReference(string assembly)
        {
            this.references.Add("<Reference Include=\"" + assembly + "\" />");
            return this;
        }

        public INugetReferences AddNugetReference(string id, string version)
        {
            this.references.Add(
                "<Reference Include=\"" + id + ", Version=" + version + ", Culture=neutral, PublicKeyToken=917bca444d1f2b4c, processorArchitecture=MSIL\">\n\r" +  
                @"<SpecificVersion>False</SpecificVersion>
    </Reference>");
            return this;
        }

        public XDocument Build()
        {
            return XDocument.Parse(
                @"<?xml version=""1.0"" encoding=""utf-8""?>
<Project ToolsVersion=""12.0"" DefaultTargets=""Build"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <Import Project=""$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props"" Condition=""Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')"" />
  <PropertyGroup>
    <Configuration Condition="" '$(Configuration)' == '' "">Debug</Configuration>
    <Platform Condition="" '$(Platform)' == '' "">AnyCPU</Platform>
    <ProjectGuid>{040E4BA4-3EC0-4D60-B82F-2F375BF0DD29}</ProjectGuid>
    <OutputType>Library</OutputType>
    <AppDesignerFolder>Properties</AppDesignerFolder>
    <RootNamespace>Appccelerate.IO</RootNamespace>
    <AssemblyName>Appccelerate.IO</AssemblyName>
    <TargetFrameworkVersion>v4.5</TargetFrameworkVersion>
    <FileAlignment>512</FileAlignment>
  </PropertyGroup>
  <PropertyGroup Condition="" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' "">
    <DebugSymbols>true</DebugSymbols>
    <DebugType>full</DebugType>
    <Optimize>false</Optimize>
    <OutputPath>bin\Debug\</OutputPath>
    <DefineConstants>DEBUG;TRACE</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
  </PropertyGroup>
  <PropertyGroup Condition="" '$(Configuration)|$(Platform)' == 'Release|AnyCPU' "">
    <DebugType>pdbonly</DebugType>
    <Optimize>true</Optimize>
    <OutputPath>bin\Release\</OutputPath>
    <DefineConstants>TRACE</DefineConstants>
    <ErrorReport>prompt</ErrorReport>
    <WarningLevel>4</WarningLevel>
    <DocumentationFile>bin\Release\Appccelerate.IO.XML</DocumentationFile>
    <NoWarn>1591</NoWarn>
  </PropertyGroup>
  <ItemGroup>" +
                this.references.Aggregate((a, b) => a + Environment.NewLine + b) +
                @"</ItemGroup>
  <ItemGroup>
    <Compile Include=""AbsoluteFilePath.cs"" />
  </ItemGroup>
  <ItemGroup>
    <None Include=""Appccelerate.Public.snk"" />
    <None Include=""Appccelerate.snk"" />
    <None Include=""NuGet.config"" />
    <None Include=""packages.config"" />
    <None Include=""Settings.stylecop"" />
  </ItemGroup>
  <ItemGroup>
    <Content Include=""Access\IDirectory.doc.xml"" />
    <Content Include=""Access\IDirectoryInfo.doc.xml"" />
    <Content Include=""Access\IDriveInfo.doc.xml"" />
    <Content Include=""Access\IEnvironment.doc.xml"" />
    <Content Include=""Access\IFile.doc.xml"" />
    <Content Include=""Access\IFileInfo.doc.xml"" />
    <Content Include=""Access\IFileSystemInfo.doc.xml"" />
    <Content Include=""Access\IPath.doc.xml"" />
  </ItemGroup>
  <Import Project=""$(MSBuildToolsPath)\Microsoft.CSharp.targets"" />
  <Import Project=""..\packages\GitFlowVersionTask.0.14.0\Build\GitFlowVersionTask.targets"" Condition=""Exists('..\packages\GitFlowVersionTask.0.14.0\Build\GitFlowVersionTask.targets')"" />
  <Import Project=""..\packages\Appccelerate.Development.0.14\build\Appccelerate.development.targets"" Condition=""Exists('..\packages\Appccelerate.Development.0.14\build\Appccelerate.development.targets')"" />
  <Import Project=""..\packages\Appccelerate.Development.ProductionCode.0.14\build\Appccelerate.development.ProductionCode.targets"" Condition=""Exists('..\packages\Appccelerate.Development.ProductionCode.0.14\build\Appccelerate.development.ProductionCode.targets')"" />
  <!-- To modify your build process, add your task inside one of the targets below and uncomment it. 
       Other similar extension points exist, see Microsoft.Common.targets.
  <Target Name=""BeforeBuild"">
  </Target>
  <Target Name=""AfterBuild"">
  </Target>
  -->
</Project>");
        }
    }

    public interface IProjectBuilder
    {
        IFrameworkReferences WithFrameworkReferences();
        
        INugetReferences WithNugetReferences();

        XDocument Build();
    }

    public interface IFrameworkReferences : IProjectBuilder
    {
        IFrameworkReferences AddFrameworkReference(string assembly);
    }

    public interface INugetReferences : IProjectBuilder
    {
        INugetReferences AddNugetReference(string id, string version);
    }
}