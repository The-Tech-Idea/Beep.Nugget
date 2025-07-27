using Beep.Nugget.Logic;

namespace Beep.Nugget.Winform
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _searchTimer?.Dispose();
                _bindingSource?.Dispose();
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.dataGridViewPackages = new DataGridView();
            this.buttonRefresh = new Button();
            this.buttonInstall = new Button();
            this.buttonUninstall = new Button();
            this.textBoxSearch = new TextBox();
            this.labelSearch = new Label();
            this.statusStrip = new StatusStrip();
            this.statusLabel = new ToolStripStatusLabel();
            this.progressBar = new ToolStripProgressBar();
            this.menuStrip = new MenuStrip();
            this.fileToolStripMenuItem = new ToolStripMenuItem();
            this.exitToolStripMenuItem = new ToolStripMenuItem();
            this.helpToolStripMenuItem = new ToolStripMenuItem();
            this.viewLoadedAssembliesToolStripMenuItem = new ToolStripMenuItem();
            this.toolStripSeparator2 = new ToolStripSeparator();
            this.aboutToolStripMenuItem = new ToolStripMenuItem();
            this.toolStrip = new ToolStrip();
            this.toolStripButtonRefresh = new ToolStripButton();
            this.toolStripSeparator1 = new ToolStripSeparator();
            this.toolStripButtonInstall = new ToolStripButton();
            this.toolStripButtonUninstall = new ToolStripButton();
            
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewPackages)).BeginInit();
            this.statusStrip.SuspendLayout();
            this.menuStrip.SuspendLayout();
            this.toolStrip.SuspendLayout();
            this.SuspendLayout();
            
            // 
            // dataGridViewPackages
            // 
            this.dataGridViewPackages.AllowUserToAddRows = false;
            this.dataGridViewPackages.AllowUserToDeleteRows = false;
            this.dataGridViewPackages.Anchor = ((AnchorStyles)((((AnchorStyles.Top | AnchorStyles.Bottom) 
            | AnchorStyles.Left) 
            | AnchorStyles.Right)));
            this.dataGridViewPackages.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewPackages.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewPackages.Location = new Point(12, 90);
            this.dataGridViewPackages.MultiSelect = false;
            this.dataGridViewPackages.Name = "dataGridViewPackages";
            this.dataGridViewPackages.ReadOnly = true;
            this.dataGridViewPackages.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            this.dataGridViewPackages.Size = new Size(760, 400);
            this.dataGridViewPackages.TabIndex = 0;
            this.dataGridViewPackages.SelectionChanged += this.DataGridViewPackages_SelectionChanged;
            
            // 
            // buttonRefresh
            // 
            this.buttonRefresh.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Left)));
            this.buttonRefresh.Location = new Point(12, 510);
            this.buttonRefresh.Name = "buttonRefresh";
            this.buttonRefresh.Size = new Size(75, 23);
            this.buttonRefresh.TabIndex = 1;
            this.buttonRefresh.Text = "&Refresh";
            this.buttonRefresh.UseVisualStyleBackColor = true;
            this.buttonRefresh.Click += this.ButtonRefresh_Click;
            
            // 
            // buttonInstall
            // 
            this.buttonInstall.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Left)));
            this.buttonInstall.Enabled = false;
            this.buttonInstall.Location = new Point(105, 510);
            this.buttonInstall.Name = "buttonInstall";
            this.buttonInstall.Size = new Size(75, 23);
            this.buttonInstall.TabIndex = 2;
            this.buttonInstall.Text = "&Install";
            this.buttonInstall.UseVisualStyleBackColor = true;
            this.buttonInstall.Click += this.ButtonInstall_Click;
            
            // 
            // buttonUninstall
            // 
            this.buttonUninstall.Anchor = ((AnchorStyles)((AnchorStyles.Bottom | AnchorStyles.Left)));
            this.buttonUninstall.Enabled = false;
            this.buttonUninstall.Location = new Point(198, 510);
            this.buttonUninstall.Name = "buttonUninstall";
            this.buttonUninstall.Size = new Size(75, 23);
            this.buttonUninstall.TabIndex = 3;
            this.buttonUninstall.Text = "&Uninstall";
            this.buttonUninstall.UseVisualStyleBackColor = true;
            this.buttonUninstall.Click += this.ButtonUninstall_Click;
            
            // 
            // textBoxSearch
            // 
            this.textBoxSearch.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) 
            | AnchorStyles.Right)));
            this.textBoxSearch.Location = new Point(60, 55);
            this.textBoxSearch.Name = "textBoxSearch";
            this.textBoxSearch.PlaceholderText = "Search packages...";
            this.textBoxSearch.Size = new Size(712, 23);
            this.textBoxSearch.TabIndex = 4;
            this.textBoxSearch.TextChanged += this.TextBoxSearch_TextChanged;
            
            // 
            // labelSearch
            // 
            this.labelSearch.AutoSize = true;
            this.labelSearch.Location = new Point(12, 58);
            this.labelSearch.Name = "labelSearch";
            this.labelSearch.Size = new Size(42, 15);
            this.labelSearch.TabIndex = 5;
            this.labelSearch.Text = "Search:";
            
            // 
            // statusStrip
            // 
            this.statusStrip.Items.AddRange(new ToolStripItem[] {
            this.statusLabel,
            this.progressBar});
            this.statusStrip.Location = new Point(0, 548);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new Size(784, 22);
            this.statusStrip.TabIndex = 6;
            this.statusStrip.Text = "statusStrip1";
            
            // 
            // statusLabel
            // 
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new Size(669, 17);
            this.statusLabel.Spring = true;
            this.statusLabel.Text = "Ready";
            this.statusLabel.TextAlign = ContentAlignment.MiddleLeft;
            
            // 
            // progressBar
            // 
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new Size(100, 16);
            this.progressBar.Visible = false;
            
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip.Location = new Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new Size(784, 24);
            this.menuStrip.TabIndex = 7;
            this.menuStrip.Text = "menuStrip1";
            
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new Size(93, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += this.ExitToolStripMenuItem_Click;
            
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
            this.viewLoadedAssembliesToolStripMenuItem,
            this.toolStripSeparator2,
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new Size(44, 20);
            this.helpToolStripMenuItem.Text = "&Help";
            
            // 
            // viewLoadedAssembliesToolStripMenuItem
            // 
            this.viewLoadedAssembliesToolStripMenuItem.Name = "viewLoadedAssembliesToolStripMenuItem";
            this.viewLoadedAssembliesToolStripMenuItem.Size = new Size(190, 22);
            this.viewLoadedAssembliesToolStripMenuItem.Text = "&View Loaded Assemblies";
            this.viewLoadedAssembliesToolStripMenuItem.Click += this.ViewLoadedAssembliesToolStripMenuItem_Click;
            
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new Size(187, 6);
            
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new Size(190, 22);
            this.aboutToolStripMenuItem.Text = "&About";
            this.aboutToolStripMenuItem.Click += this.AboutToolStripMenuItem_Click;
            
            // 
            // toolStrip
            // 
            this.toolStrip.Items.AddRange(new ToolStripItem[] {
            this.toolStripButtonRefresh,
            this.toolStripSeparator1,
            this.toolStripButtonInstall,
            this.toolStripButtonUninstall});
            this.toolStrip.Location = new Point(0, 24);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new Size(784, 25);
            this.toolStrip.TabIndex = 8;
            this.toolStrip.Text = "toolStrip1";
            
            // 
            // toolStripButtonRefresh
            // 
            this.toolStripButtonRefresh.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.toolStripButtonRefresh.Image = SystemIcons.Application.ToBitmap();
            this.toolStripButtonRefresh.ImageTransparentColor = Color.Magenta;
            this.toolStripButtonRefresh.Name = "toolStripButtonRefresh";
            this.toolStripButtonRefresh.Size = new Size(23, 22);
            this.toolStripButtonRefresh.Text = "Refresh";
            this.toolStripButtonRefresh.Click += this.ButtonRefresh_Click;
            
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new Size(6, 25);
            
            // 
            // toolStripButtonInstall
            // 
            this.toolStripButtonInstall.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.toolStripButtonInstall.Enabled = false;
            this.toolStripButtonInstall.Image = SystemIcons.Application.ToBitmap();
            this.toolStripButtonInstall.ImageTransparentColor = Color.Magenta;
            this.toolStripButtonInstall.Name = "toolStripButtonInstall";
            this.toolStripButtonInstall.Size = new Size(23, 22);
            this.toolStripButtonInstall.Text = "Install";
            this.toolStripButtonInstall.Click += this.ButtonInstall_Click;
            
            // 
            // toolStripButtonUninstall
            // 
            this.toolStripButtonUninstall.DisplayStyle = ToolStripItemDisplayStyle.Image;
            this.toolStripButtonUninstall.Enabled = false;
            this.toolStripButtonUninstall.Image = SystemIcons.Application.ToBitmap();
            this.toolStripButtonUninstall.ImageTransparentColor = Color.Magenta;
            this.toolStripButtonUninstall.Name = "toolStripButtonUninstall";
            this.toolStripButtonUninstall.Size = new Size(23, 22);
            this.toolStripButtonUninstall.Text = "Uninstall";
            this.toolStripButtonUninstall.Click += this.ButtonUninstall_Click;
            
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(784, 570);
            this.Controls.Add(this.toolStrip);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.labelSearch);
            this.Controls.Add(this.textBoxSearch);
            this.Controls.Add(this.buttonUninstall);
            this.Controls.Add(this.buttonInstall);
            this.Controls.Add(this.buttonRefresh);
            this.Controls.Add(this.dataGridViewPackages);
            this.Controls.Add(this.menuStrip);
            this.MainMenuStrip = this.menuStrip;
            this.Name = "MainForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Beep Nugget Manager";
            this.Load += this.MainForm_Load;
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewPackages)).EndInit();
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private DataGridView dataGridViewPackages;
        private Button buttonRefresh;
        private Button buttonInstall;
        private Button buttonUninstall;
        private TextBox textBoxSearch;
        private Label labelSearch;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;
        private ToolStripProgressBar progressBar;
        private MenuStrip menuStrip;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem viewLoadedAssembliesToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem aboutToolStripMenuItem;
        private ToolStrip toolStrip;
        private ToolStripButton toolStripButtonRefresh;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripButton toolStripButtonInstall;
        private ToolStripButton toolStripButtonUninstall;
    }
}