using NuGet.Common;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using NuGet.Configuration;
using NuGet.ProjectModel;
using NuGet.Commands;
using NuGet.Frameworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Packaging.Signing;
using NuGet.Repositories;
using System.Collections.ObjectModel;


namespace Beep.Nugget.Logic
{
 

    public class NuGetManager
    {
        private readonly string _repositoryUrl;
        private readonly SourceCacheContext _sourceCacheContext;
        private readonly ILogger _logger;
        private readonly PackageSource _packageSource;
        private readonly string _runtimeFramework;
        public ObservableCollection<NuggetDefinition> _definitions;
        public NuGetManager(string repositoryUrl = "https://api.nuget.org/v3/index.json")
        {
            _repositoryUrl = repositoryUrl;
            _sourceCacheContext = new SourceCacheContext();
            _logger = NullLogger.Instance;
            _packageSource = new PackageSource(_repositoryUrl);
            _runtimeFramework = GetRuntimeFramework();
        }

        private string GetRuntimeFramework()
        {
            string currentFramework = Assembly.GetEntryAssembly()?
                .GetCustomAttribute<TargetFrameworkAttribute>()?
                .FrameworkName ?? throw new Exception("Unable to determine the runtime framework.");

            return currentFramework;
        }

        private string GetNuGetCompatibleFramework(string runtimeFramework, string[] availableFrameworks)
        {
            // Map available frameworks to a version number to allow compatibility checks
            var runtimeVersion = ExtractFrameworkVersion(runtimeFramework);
            var compatibleFrameworks = availableFrameworks
                .Select(f => new { Framework = f, Version = ExtractFrameworkVersion(f) })
                .Where(f => f.Version <= runtimeVersion)
                .OrderByDescending(f => f.Version)
                .ToList();

            if (compatibleFrameworks.Any())
            {
                return compatibleFrameworks.First().Framework;
            }

            throw new Exception($"No compatible folder found for target framework {runtimeFramework}.");
        }

        private int ExtractFrameworkVersion(string frameworkName)
        {
            if (frameworkName.Contains("net"))
            {
                var versionPart = frameworkName.Replace("net", "").Replace(".", "");
                if (int.TryParse(versionPart, out int version))
                {
                    return version;
                }
            }
            throw new Exception($"Unable to parse framework version from {frameworkName}");
        }

        public async Task<IEnumerable<IPackageSearchMetadata>> SearchPackagesAsync(string searchTerm, int take = 10)
        {
            var repository = Repository.Factory.GetCoreV3(_repositoryUrl);
            var resource = await repository.GetResourceAsync<PackageSearchResource>();
            var searchFilter = new SearchFilter(includePrerelease: false);

            return await resource.SearchAsync(
                searchTerm,
                searchFilter,
                skip: 0,
                take: take,
                log: _logger,
                cancellationToken: CancellationToken.None);
        }

        public async Task<string> DownloadNuGetAsync(string packageId, string version = "latest")
        {
            var repository = Repository.Factory.GetCoreV3(_repositoryUrl);
            var resource = await repository.GetResourceAsync<FindPackageByIdResource>();

            NuGetVersion nuGetVersion;
            if (version.ToLower() == "latest")
            {
                var versions = await resource.GetAllVersionsAsync(packageId, _sourceCacheContext, _logger, CancellationToken.None);
                nuGetVersion = versions.Max();
            }
            else
            {
                nuGetVersion = NuGetVersion.Parse(version);
            }

            var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempFolder);

            var packagePath = Path.Combine(tempFolder, $"{packageId}.{nuGetVersion}.nupkg");
            using (var packageStream = new FileStream(packagePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                if (!await resource.CopyNupkgToStreamAsync(packageId, nuGetVersion, packageStream, _sourceCacheContext, _logger, CancellationToken.None))
                {
                    throw new Exception("Package download failed.");
                }
            }

            return packagePath;
        }

        public async Task<bool> AddNuGetToProjectAsync(string packageId, string version = "latest", string projectPath = "./")
        {
            try
            {
                var packagePath = await DownloadNuGetAsync(packageId, version);

                var projectDir = new DirectoryInfo(projectPath);
                if (!projectDir.Exists)
                {
                    throw new DirectoryNotFoundException("The specified project path does not exist.");
                }

                var projectFile = projectDir.GetFiles("*.csproj").FirstOrDefault();
                if (projectFile == null)
                {
                    throw new FileNotFoundException("No .csproj file found in the specified project directory.");
                }

                string libFolderPath = Path.Combine(packagePath, "lib");
                if (!Directory.Exists(libFolderPath))
                {
                    throw new DirectoryNotFoundException("The package does not contain a lib folder.");
                }

                string[] frameworkFolders = Directory.GetDirectories(libFolderPath);
                string compatibleFramework = GetNuGetCompatibleFramework(_runtimeFramework, frameworkFolders);

                var settings = Settings.LoadDefaultSettings(projectPath);
                var packageFolderPath = Path.Combine(projectPath, "packages");

                var projectPackageSpec = new PackageSpec(new List<TargetFrameworkInformation>
            {
                new TargetFrameworkInformation { FrameworkName = NuGetFramework.Parse(compatibleFramework) }
            })
                {
                    Name = projectFile.Name,
                    FilePath = projectFile.FullName,
                    RestoreMetadata = new ProjectRestoreMetadata
                    {
                        ProjectUniqueName = projectFile.FullName,
                        ProjectName = projectFile.Name,
                        OutputPath = Path.Combine(projectPath, "obj"),
                        ProjectStyle = ProjectStyle.PackageReference,
                        OriginalTargetFrameworks = new List<string> { compatibleFramework },
                        TargetFrameworks = new List<ProjectRestoreMetadataFrameworkInfo>
                    {
                        new ProjectRestoreMetadataFrameworkInfo(NuGetFramework.Parse(compatibleFramework))
                    },
                        PackagesPath = packageFolderPath,
                        Sources = new List<PackageSource> { _packageSource }
                    }
                };

                var dependencyProviders = new RestoreCommandProviders(new NuGetv3LocalRepository(packageFolderPath), null, null, null, new LocalPackageFileCache());
                var clientPolicyContext = ClientPolicyContext.GetClientPolicy(settings, _logger);
                var lockFileBuilderCache = new LockFileBuilderCache();

                var request = new RestoreRequest(projectPackageSpec, dependencyProviders, _sourceCacheContext, clientPolicyContext, null, _logger, lockFileBuilderCache)
                {
                    LockFilePath = Path.Combine(projectPath, "obj", "project.assets.json")
                };

                var command = new RestoreCommand(request);
                var result = await command.ExecuteAsync(CancellationToken.None);

                if (result.Success)
                {
                    _logger.LogInformation($"Package {packageId} added to project successfully.");
                    return true;
                }
                else
                {
                    _logger.LogError($"Failed to add package {packageId} to project.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adding package to project: {ex.Message}");
                return false;
            }
        }

        public void AddNuGetToRunningApplication(string packagePath)
        {
            try
            {
                string libFolderPath = Path.Combine(packagePath, "lib");
                if (!Directory.Exists(libFolderPath))
                {
                    throw new DirectoryNotFoundException("The package does not contain a lib folder.");
                }

                string[] frameworkFolders = Directory.GetDirectories(libFolderPath);
                string compatibleFramework = GetNuGetCompatibleFramework(_runtimeFramework, frameworkFolders);

                string[] dllFiles = Directory.GetFiles(compatibleFramework, "*.dll");
                foreach (var dll in dllFiles)
                {
                    var assembly = Assembly.LoadFrom(dll);
                    if (assembly != null)
                    {
                        _logger.LogInformation($"Loaded assembly at runtime: {assembly.FullName}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading package at runtime: {ex.Message}");
            }
        }

        public async Task<bool> UpdatePackageAsync(string packageId, string projectPath = "./")
        {
            try
            {
                var repository = Repository.Factory.GetCoreV3(_repositoryUrl);
                var resource = await repository.GetResourceAsync<FindPackageByIdResource>();
                var versions = await resource.GetAllVersionsAsync(packageId, _sourceCacheContext, _logger, CancellationToken.None);
                var latestVersion = versions.Max();

                var projectDir = new DirectoryInfo(projectPath);
                if (!projectDir.Exists)
                {
                    throw new DirectoryNotFoundException("The specified project path does not exist.");
                }

                var projectFile = projectDir.GetFiles("*.csproj").FirstOrDefault();
                if (projectFile == null)
                {
                    throw new FileNotFoundException("No .csproj file found in the specified project directory.");
                }

                // Check if the package is already up to date
                var existingPackagePath = Path.Combine(projectPath, "packages", $"{packageId}.{latestVersion}.nupkg");
                if (File.Exists(existingPackagePath))
                {
                    _logger.LogInformation($"Package {packageId} is already up to date.");
                    return true;
                }

                return await AddNuGetToProjectAsync(packageId, latestVersion.ToString(), projectPath);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating package: {ex.Message}");
                return false;
            }
        }

        public bool RemovePackage(string packageId, string version, string destinationFolder = "packages")
        {
            try
            {
                var packagePath = Path.Combine(destinationFolder, $"{packageId}.{version}.nupkg");
                if (File.Exists(packagePath))
                {
                    File.Delete(packagePath);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error removing package: {ex.Message}");
                return false;
            }
        }
        public async Task RetrieveCompanyNuggetsAsync(string searchTerm = "")
        {
            var repository = Repository.Factory.GetCoreV3(_repositoryUrl);
            var resource = await repository.GetResourceAsync<PackageSearchResource>();
            var searchFilter = new SearchFilter(includePrerelease: false);

            var results = await resource.SearchAsync(searchTerm, searchFilter, 0, 50, _logger, CancellationToken.None);

            foreach (var package in results)
            {
                var nuggetDefinition = new NuggetDefinition
                {
                    NuggetName = package.Identity.Id,
                    Name = package.Title,
                    Description = package.Description,
                    Version = package.Identity.Version.ToString(),
                    Author = package.Authors,
                    Installed = IsPackageInstalled(package.Identity.Id, package.Identity.Version.ToString())
                };

                _definitions.Add(nuggetDefinition);
            }
        }

        private bool IsPackageInstalled(string packageId, string version)
        {
            // Logic to check if the package is installed in the runtime app
            // For example, checking the AppDomain's loaded assemblies
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            return assemblies.Any(a => a.GetName().Name.Equals(packageId, StringComparison.OrdinalIgnoreCase)
                                       && a.GetName().Version.ToString() == version);
        }

    }
}
