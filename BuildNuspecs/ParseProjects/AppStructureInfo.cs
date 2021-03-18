﻿// Copyright (c) 2021 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using BuildNuspecs.Helpers;
using Microsoft.Extensions.Logging;

namespace BuildNuspecs.ParseProjects
{
    public class AppStructureInfo
    {
        public AppStructureInfo(string rootName, Dictionary<string, ProjectInfo> allProjects, ConsoleOutput consoleOut)
        {
            RootName = rootName;
            AllProjects = allProjects.Values.ToList();
            SetupAllNuGetInfosDistinctWithChecks(consoleOut);

            foreach (var project in allProjects.Values)
            {
                foreach (var lookForLinks in allProjects.Values
                    .Where(lookForLinks => lookForLinks.ChildProjects.Select(x => x.ProjectName).Contains(project.ProjectName)))
                {
                    project.ParentProjects.Add(lookForLinks);
                }
            }

            RootProjects = allProjects.Values.Where(x => !x.ParentProjects.Any())
                .OrderBy(x => x.ProjectName.Length).ToList();
        }

        public string RootName { get; private set; }

        public List<ProjectInfo> RootProjects { get; private set; }

        public List<ProjectInfo> AllProjects { get; private set; }

        public List<NuGetInfo> AllNuGetInfosDistinct { get; private set; }

        private void SetupAllNuGetInfosDistinctWithChecks(ConsoleOutput consoleOut)
        {
            var groupedNuGets = AllProjects.SelectMany(x => x.NuGetPackages)
                .GroupBy(x => x.NuGetId);

            var allNuGets = new List<NuGetInfo>();
            foreach (var groupedNuGet in groupedNuGets
                .GroupBy(x => x.ToList().Select(z => z.Version) ))
            {
                var versionDistinct = groupedNuGet.Key.Distinct().ToList();
                if (versionDistinct.Count > 1)
                    consoleOut.LogMessage($"{groupedNuGet.Key} NuGet has multiple versions: \n {string.Join("\n", versionDistinct)}", LogLevel.Error);
                allNuGets.Add(groupedNuGet.Single().First());
            }

            AllNuGetInfosDistinct = allNuGets;
        }

        public override string ToString()
        {
            return
                $"Found {AllProjects.Count} projects starting with {RootName}, with {AllNuGetInfosDistinct.Count} NuGet packages.";
        }
    }
}