﻿// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BuildNuspecs.Helpers;
using Microsoft.Extensions.Logging;

namespace BuildNuspecs.ParseProjects
{
    public static class ProjectsParser
    {
        public static AppStructureInfo ParseModularMonolithApp(this string solutionDir, Settings settings, ConsoleOutput consoleOut)
        {
            string FormProjectName(string partialName)
            {
                return partialName.StartsWith('.')
                    ? settings.RootName + partialName
                    : partialName;
            }

            var projFilePaths = Directory.GetDirectories(solutionDir)
                    .Where(dir => Path.GetFileNameWithoutExtension(dir).StartsWith(settings.RootName))
                    .SelectMany(dir =>
                Directory.GetFiles(dir, "*.csproj")).ToList();

            var folderNotSame = projFilePaths.Where(x =>
                !x.Contains(Path.GetFileNameWithoutExtension(x))).ToList();
            if (folderNotSame.Any())
            {
                foreach (var path in folderNotSame)
                {
                    consoleOut.LogMessage($"the project {Path.GetFileName(path)} isn't in a folder of the same name", LogLevel.Warning);
                }
                consoleOut.LogMessage($"This code relies on the project file to be in a folder of the same name.", LogLevel.Error);
            }

            var excludedProjectNames = settings.ExcludeProjects?.Split(',')
                .Select(x => FormProjectName(x.Trim())).ToList() ?? new List<string>();
            foreach (var projectName in excludedProjectNames)
            {
                var excludedPath =
                    projFilePaths.SingleOrDefault(x => Path.GetFileNameWithoutExtension(x) == projectName);
                if (excludedPath != null)
                {
                    projFilePaths.Remove(excludedPath);
                    consoleOut.LogMessage($"Excluded project '{projectName}'", LogLevel.Information);
                }
                else
                    consoleOut.LogMessage($"Could not find a project called '{projectName}' to exclude", LogLevel.Warning);
            }



            var pInfo = projFilePaths
                .Select(path => new ProjectInfo(path))
                .ToDictionary(x => x.ProjectName);

            //Now we look at each csproj in turn and fill in
            //- What target framework it has (used to build Nuspec)
            //- What NuGet packages it uses (used to build Nuspec)
            //- What it links to (used in displaying links)
            foreach (var projFilePath in projFilePaths)
            {
                var filename = Path.GetFileNameWithoutExtension(projFilePath);
                var projectDecoded = DeserializeToObject<Project>(projFilePath);

                var projectToUpdate = pInfo[filename];

                //get target framework
                projectToUpdate.TargetFramework = projectDecoded.PropertyGroup.TargetFramework;

                //get NuGet packages

                projectToUpdate.NuGetPackages = projectDecoded.ItemGroup
                    ?.SingleOrDefault(x => x?.PackageReference?.Any() == true)
                    ?.PackageReference
                    .Select(x => new NuGetInfo(x))
                    .ToList() ?? new List<NuGetInfo>();

                // Fill in references to other packages
                projectToUpdate.ChildProjects = projectDecoded.ItemGroup
                    ?.SingleOrDefault(x => x?.ProjectReference?.Any() == true)
                    ?.ProjectReference
                    .Select(x => pInfo[Path.GetFileNameWithoutExtension(x.Include)])
                    .ToList() ?? new List<ProjectInfo>();
            }

            return new AppStructureInfo(settings.RootName, pInfo, consoleOut);
        }

        private static T DeserializeToObject<T>(this string filepath) where T : class
        {
            System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(typeof(T));

            using StreamReader sr = new StreamReader(filepath);
            return (T)ser.Deserialize(sr);
        }
    }
}
