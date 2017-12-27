using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ExtractorConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            string rootFolder = args[0];
            string outputFilePath = args[1];
            var packageUrls = new List<string>();

            ProcessFolderRecur(packageUrls, rootFolder);

            File.WriteAllLines(outputFilePath, packageUrls.OrderBy(x => x).ToArray());
        }

        private static void ProcessFolderRecur(List<string> packageUrls, string folderPath)
        {
            foreach (var subFolderPath in Directory.GetDirectories(folderPath))
                ProcessFolderRecur(packageUrls, subFolderPath);

            var packagesConfigFilePath = Path.Combine(folderPath, "packages.config");
            if (File.Exists(packagesConfigFilePath))
                ProcessPackagesConfigFile(packageUrls, packagesConfigFilePath);

            // TODO: Look for project file to examine project package references.
        }

        private static void ProcessPackagesConfigFile(List<string> packageUrls, string packagesConfigFilePath)
        {
            var contents = File.ReadAllLines(packagesConfigFilePath);
            foreach (var line in contents)
            {
                if (line.Trim().StartsWith("<package id="))
                {
                    var packageName = line.FindFirstBetween(" id=\"", "\"");
                    var packageVersion = line.FindFirstBetween(" version=\"", "\"");
                    RecordPackage(packageUrls, packageName, packageVersion);
                }
            }
        }

        private static void RecordPackage(List<string> packageUrls, string packageName, string packageVersion)
        {
            if (!String.IsNullOrEmpty(packageName) && !String.IsNullOrEmpty(packageVersion))
            {
                var packageUrl = $@"https://www.nuget.org/packages/{packageName}/{packageVersion}";
                if (!packageUrls.Contains(packageUrl))
                    packageUrls.Add(packageUrl);
            }
        }
    }
}
