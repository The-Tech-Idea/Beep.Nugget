using Beep.Nugget.Logic;
using System.ComponentModel;
using Timer = System.Windows.Forms.Timer;

namespace Beep.Nugget.Winform
{
    public partial class MainForm : Form
    {
        private BeepNuggetListViewModel _viewModel;
        private BindingSource _bindingSource;
        private Timer _searchTimer;

        public MainForm()
        {
            InitializeComponent();
            _viewModel = new BeepNuggetListViewModel();
            _bindingSource = new BindingSource();
            _searchTimer = new Timer();
            _searchTimer.Interval = 500; // 500ms delay for search
            _searchTimer.Tick += SearchTimer_Tick;
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            SetupDataGridView();
            await RefreshPackageList();
        }

        private void SetupDataGridView()
        {
            // Configure DataGridView columns
            dataGridViewPackages.AutoGenerateColumns = false;
            dataGridViewPackages.DataSource = _bindingSource;

            // Add columns
            dataGridViewPackages.Columns.Add(new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "Installed",
                HeaderText = "Installed",
                Name = "InstalledColumn",
                Width = 80,
                ReadOnly = true
            });

            dataGridViewPackages.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "NuggetName",
                HeaderText = "Package ID",
                Name = "PackageIdColumn",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 30
            });

            dataGridViewPackages.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Name",
                HeaderText = "Name",
                Name = "NameColumn",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 25
            });

            dataGridViewPackages.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Version",
                HeaderText = "Version",
                Name = "VersionColumn",
                Width = 100
            });

            dataGridViewPackages.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Author",
                HeaderText = "Author",
                Name = "AuthorColumn",
                Width = 120
            });

            dataGridViewPackages.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Description",
                HeaderText = "Description",
                Name = "DescriptionColumn",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 45
            });
        }

        private async Task RefreshPackageList()
        {
            try
            {
                SetStatus("Loading packages...", true);
                UpdateButtonStates(false);

                await _viewModel.GetListCommand.ExecuteAsync(null);
                
                _bindingSource.DataSource = _viewModel.NuggetDefinitions;
                _bindingSource.ResetBindings(false);

                SetStatus($"Loaded {_viewModel.NuggetDefinitions?.Count ?? 0} packages", false);
                UpdateButtonStates(true);
            }
            catch (Exception ex)
            {
                SetStatus($"Error loading packages: {ex.Message}", false);
                MessageBox.Show($"Error loading packages: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateButtonStates(true);
            }
        }

        private void SetStatus(string message, bool showProgress)
        {
            statusLabel.Text = message;
            progressBar.Visible = showProgress;
            if (showProgress)
            {
                progressBar.Style = ProgressBarStyle.Marquee;
            }
            else
            {
                progressBar.Style = ProgressBarStyle.Continuous;
            }
            statusStrip.Refresh();
        }

        private void UpdateButtonStates(bool enabled)
        {
            var hasSelection = dataGridViewPackages.SelectedRows.Count > 0;
            var selectedPackage = GetSelectedPackage();
            var isInstalled = selectedPackage?.Installed ?? false;

            buttonRefresh.Enabled = enabled;
            toolStripButtonRefresh.Enabled = enabled;
            
            buttonInstall.Enabled = enabled && hasSelection && !isInstalled;
            toolStripButtonInstall.Enabled = enabled && hasSelection && !isInstalled;
            
            buttonUninstall.Enabled = enabled && hasSelection && isInstalled;
            toolStripButtonUninstall.Enabled = enabled && hasSelection && isInstalled;
        }

        private NuggetDefinition? GetSelectedPackage()
        {
            if (dataGridViewPackages.SelectedRows.Count > 0)
            {
                return dataGridViewPackages.SelectedRows[0].DataBoundItem as NuggetDefinition;
            }
            return null;
        }

        private async void ButtonRefresh_Click(object sender, EventArgs e)
        {
            await RefreshPackageList();
        }

        private async void ButtonInstall_Click(object sender, EventArgs e)
        {
            var package = GetSelectedPackage();
            if (package == null) return;

            try
            {
                SetStatus($"Installing {package.NuggetName}...", true);
                UpdateButtonStates(false);

                await _viewModel.InstallCommand.ExecuteAsync(package);
                bool success = package.Installed; // Check the result from the package itself
                
                if (success)
                {
                    SetStatus($"Successfully installed {package.NuggetName}", false);
                    _bindingSource.ResetBindings(false);
                }
                else
                {
                    SetStatus($"Failed to install {package.NuggetName}", false);
                    MessageBox.Show($"Failed to install {package.NuggetName}", "Installation Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Error installing package: {ex.Message}", false);
                MessageBox.Show($"Error installing {package.NuggetName}: {ex.Message}", "Installation Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                UpdateButtonStates(true);
            }
        }

        private async void ButtonUninstall_Click(object sender, EventArgs e)
        {
            var package = GetSelectedPackage();
            if (package == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to uninstall {package.NuggetName}?\n\n" +
                "Note: Assemblies cannot be fully unloaded from memory until the application restarts.",
                "Confirm Uninstall",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    SetStatus($"Uninstalling {package.NuggetName}...", true);
                    UpdateButtonStates(false);

                    await _viewModel.UninstallCommand.ExecuteAsync(package);
                    bool success = !package.Installed; // Check if it was marked as uninstalled
                    
                    if (success)
                    {
                        SetStatus($"Successfully uninstalled {package.NuggetName}", false);
                        _bindingSource.ResetBindings(false);
                        MessageBox.Show(
                            $"Package {package.NuggetName} has been marked as uninstalled.\n" +
                            "Note: The assemblies will remain loaded in memory until the application restarts.",
                            "Uninstall Complete",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    else
                    {
                        SetStatus($"Failed to uninstall {package.NuggetName}", false);
                        MessageBox.Show($"Failed to uninstall {package.NuggetName}", "Uninstall Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    SetStatus($"Error uninstalling package: {ex.Message}", false);
                    MessageBox.Show($"Error uninstalling {package.NuggetName}: {ex.Message}", "Uninstall Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    UpdateButtonStates(true);
                }
            }
        }

        private void DataGridViewPackages_SelectionChanged(object sender, EventArgs e)
        {
            UpdateButtonStates(true);
        }

        private void TextBoxSearch_TextChanged(object sender, EventArgs e)
        {
            _searchTimer.Stop();
            _searchTimer.Start();
        }

        private void SearchTimer_Tick(object? sender, EventArgs e)
        {
            _searchTimer.Stop();
            FilterPackages();
        }

        private void FilterPackages()
        {
            var searchText = textBoxSearch.Text?.Trim();
            
            if (string.IsNullOrEmpty(searchText))
            {
                _bindingSource.RemoveFilter();
            }
            else
            {
                _bindingSource.Filter = $"NuggetName LIKE '%{searchText}%' OR Name LIKE '%{searchText}%' OR Description LIKE '%{searchText}%'";
            }
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "Beep Nugget Manager\n\n" +
                "A runtime NuGet package manager for TheTechIdea packages.\n" +
                "This tool allows you to download and manage TheTechIdea packages at runtime.\n\n" +
                "© 2025 TheTechIdea",
                "About Beep Nugget Manager",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void ViewLoadedAssembliesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var loadedAssemblies = _viewModel.NugetManager.GetLoadedCompanyAssemblies();
                
                var message = "Loaded TheTechIdea Assemblies:\n\n";
                if (loadedAssemblies.Count == 0)
                {
                    message += "No TheTechIdea assemblies are currently loaded.";
                }
                else
                {
                    foreach (var assembly in loadedAssemblies)
                    {
                        var name = assembly.GetName();
                        message += $"• {name.Name} (v{name.Version})\n";
                        message += $"  Location: {assembly.Location}\n\n";
                    }
                }
                
                MessageBox.Show(message, "Loaded Assemblies", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error retrieving loaded assemblies: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}