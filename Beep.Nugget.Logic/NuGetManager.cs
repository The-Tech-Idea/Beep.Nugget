using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using NuGet.Configuration;
using NuGet.ProjectModel;
using NuGet.Commands;
using NuGet.Frameworks;
using System.Reflection;
using System.Runtime.Versioning;
using NuGet.Packaging.Signing;
using NuGet.Repositories;
using System.Collections.ObjectModel;
using NuGet.Packaging;

namespace Beep.Nugget.Logic
{
    /// <summary>
    /// NuGetManager is responsible for managing NuGet packages, 
    /// including searching, downloading, installing, and adding them to a running application.
    /// </summary>
    public class NuGetManager
    {
        private readonly string _repositoryUrl;
        private readonly SourceCacheContext _sourceCacheContext;
        private readonly ILogger _logger;
        private readonly PackageSource _packageSource;
        private readonly string _runtimeFramework;
        public ObservableCollection<NuggetDefinition> _definitions;

        /// <summary>
        /// Initializes a new instance of the <see cref="NuGetManager"/> class with a specified repository URL.
        /// </summary>
        /// <param name="repositoryUrl">The URL of the NuGet repository (default is the NuGet official repository).</param>
        public NuGetManager(string repositoryUrl = "https://api.nuget.org/v3/index.json")
        {
            _repositoryUrl = repositoryUrl;
            _sourceCacheContext = new SourceCacheContext();
            _logger = NullLogger.Instance;
            _packageSource = new PackageSource(_repositoryUrl);
            _runtimeFramework = GetRuntimeFramework();
        }

        /// <summary>
        /// Determines the runtime framework of the current application.
        /// </summary>
        /// <returns>A string representing the runtime framework.</returns>
        private string GetRuntimeFramework()
        {
            string currentFramework = Assembly.GetEntryAssembly()?
                .GetCustomAttribute<TargetFrameworkAttribute>()?
                .FrameworkName ?? throw new Exception("Unable to determine the runtime framework.");

            return currentFramework;
        }

        /// <summary>
        /// Determines the compatible framework folder based on the runtime framework and available framework folders.
        /// </summary>
        /// <param name="runtimeFramework">The runtime framework of the current application.</param>
        /// <param name="availableFrameworks">A list of available framework folders.</param>
        /// <returns>The folder path for the compatible framework.</returns>
        private string GetNuGetCompatibleFramework(string runtimeFramework, string[] availableFrameworks)
        {
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

        /// <summary>
        /// Extracts the framework version from a framework name string (e.g., "net6.0").
        /// </summary>
        /// <param name="frameworkName">The name of the framework.</param>
        /// <returns>An integer representing the version number of the framework.</returns>
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

        /// <summary>
        /// Searches for NuGet packages in the repository based on a search term.
        /// </summary>
        /// <param name="searchTerm">The search term to look for in package metadata.</param>
        /// <param name="take">The number of search results to return.</param>
        /// <returns>A list of package metadata matching the search term.</returns>
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

        /// <summary>
        /// Downloads a NuGet package by ID and version.
        /// If the version is "latest", the latest available version will be downloaded.
        /// </summary>
        /// <param name="packageId">The ID of the NuGet package to download.</param>
        /// <param name="version">The version of the NuGet package to download (default is "latest").</param>
        /// <returns>The file path of the downloaded package.</returns>
        public async Task<string> DownloadNuGetAsync(string packageId, string version = "latest")
        {
            try
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
                        throw new Exception($"Failed to download the NuGet package: {packageId} (v{nuGetVersion})");
                    }
                }

                // Recursively download dependencies if they exist
                var packageReader = new PackageArchiveReader(packagePath);
                var nuspec = await packageReader.GetNuspecReaderAsync(CancellationToken.None);
                var dependencies = nuspec.GetDependencyGroups();

                foreach (var group in dependencies)
                {
                    foreach (var dependency in group.Packages)
                    {
                        _logger.LogInformation($"Downloading dependency: {dependency.Id} (Version: {dependency.VersionRange})");
                        await DownloadNuGetAsync(dependency.Id, dependency.VersionRange.MinVersion.ToString());
                    }
                }

                _logger.LogInformation($"Successfully downloaded package: {packageId} (v{nuGetVersion})");
                return packagePath;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error downloading package {packageId}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Adds a NuGet package to a project (.csproj) by downloading the package and restoring it to the project.
        /// </summary>
        /// <param name="packageId">The ID of the package to add.</param>
        /// <param name="version">The version of the package to add.</param>
        /// <param name="projectPath">The path to the project (.csproj) to add the package to.</param>
        /// <returns>True if the package was added successfully, otherwise false.</returns>
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

        /// <summary>
        /// Loads a NuGet package and its dependencies into the running application by loading its assemblies at runtime.
        /// </summary>
        /// <param name="packagePath">The path to the downloaded NuGet package.</param>
        public async Task AddNuGetToRunningApplication(string packagePath)
        {
            try
            {
                string libFolderPath = Path.Combine(packagePath, "lib");
                if (!Directory.Exists(libFolderPath))
                {
                    throw new DirectoryNotFoundException($"The package at {packagePath} does not contain a lib folder.");
                }

                string[] frameworkFolders = Directory.GetDirectories(libFolderPath);
                string compatibleFramework = GetNuGetCompatibleFramework(_runtimeFramework, frameworkFolders);

                if (compatibleFramework == null)
                {
                    throw new Exception($"No compatible framework found for {_runtimeFramework} in package {packagePath}.");
                }

                // Load assemblies from the package
                string[] dllFiles = Directory.GetFiles(compatibleFramework, "*.dll");
                foreach (var dll in dllFiles)
                {
                    try
                    {
                        var assembly = Assembly.LoadFrom(dll);
                        if (assembly != null)
                        {
                            _logger.LogInformation($"Successfully loaded assembly: {assembly.FullName} from {dll}");
                        }
                    }
                    catch (Exception dllEx)
                    {
                        _logger.LogError($"Failed to load assembly from {dll}: {dllEx.Message}");
                    }
                }

                // Recursively load dependencies
                await LoadDependencies(packagePath);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading NuGet package from {packagePath}: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads all dependencies of a NuGet package by reading the package's metadata and recursively adding the dependencies to the running application.
        /// </summary>
        /// <param name="packagePath">The path to the downloaded NuGet package.</param>
        private async Task LoadDependencies(string packagePath)
        {
            try
            {
                using var packageReader = new PackageArchiveReader(packagePath);
                var nuspecReader = await packageReader.GetNuspecReaderAsync(CancellationToken.None);
                var dependencies = nuspecReader.GetDependencyGroups();

                foreach (var group in dependencies)
                {
                    foreach (var dependency in group.Packages)
                    {
                        _logger.LogInformation($"Processing dependency: {dependency.Id} (Version: {dependency.VersionRange})");

                        var dependencyPackagePath = GetPackagePath(dependency.Id, dependency.VersionRange.MinVersion.ToString());
                        if (!string.IsNullOrEmpty(dependencyPackagePath))
                        {
                            await AddNuGetToRunningApplication(dependencyPackagePath);
                        }
                        else
                        {
                            _logger.LogWarning($"Dependency {dependency.Id} not found.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading dependencies for package at {packagePath}: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves nuggets (packages) from the repository based on a search term and adds them to the local nugget definitions.
        /// </summary>
        /// <param name="searchTerm">The search term used to find packages.</param>
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

        /// <summary>
        /// Checks whether a package is installed in the runtime application by checking the loaded assemblies in the current AppDomain.
        /// </summary>
        /// <param name="packageId">The ID of the package to check.</param>
        /// <param name="version">The version of the package to check.</param>
        /// <returns>True if the package is installed, otherwise false.</returns>
        private bool IsPackageInstalled(string packageId, string version)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            return assemblies.Any(a => a.GetName().Name.Equals(packageId, StringComparison.OrdinalIgnoreCase)
                                       && a.GetName().Version.ToString() == version);
        }

        /// <summary>
        /// Retrieves the file path of a package by ID and version.
        /// </summary>
        /// <param name="packageId">The ID of the package.</param>
        /// <param name="version">The version of the package.</param>
        /// <returns>The path to the downloaded package.</returns>
        private string GetPackagePath(string packageId, string version)
        {
            string packageFolder = Path.Combine(Path.GetTempPath(), $"{packageId}.{version}.nupkg");
            if (Directory.Exists(packageFolder))
            {
                return packageFolder;
            }

            throw new FileNotFoundException($"Package {packageId} (v{version}) not found.");
        }
    }
}
