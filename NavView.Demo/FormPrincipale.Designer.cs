namespace NavView.Demo
{
    partial class FormPrincipale
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            wfPropertyPane1 = new Pane.WFPropertyPane();
            navigationView1 = new NavigationView();
            SuspendLayout();
            // 
            // wfPropertyPane1
            // 
            wfPropertyPane1.Dock = DockStyle.Right;
            wfPropertyPane1.Location = new Point(790, 0);
            wfPropertyPane1.Name = "wfPropertyPane1";
            wfPropertyPane1.Size = new Size(256, 388);
            wfPropertyPane1.TabIndex = 0;
            wfPropertyPane1.Text = "wfPropertyPane1";
            // 
            // navigationView1
            // 
            navigationView1.AppTitle = "";
            navigationView1.AutoSizePaneWidth = false;
            navigationView1.CompactPaneWidth = 52;
            navigationView1.ContentHeader = "";
            navigationView1.Dock = DockStyle.Fill;
            navigationView1.IsPaneOpen = false;
            navigationView1.Location = new Point(0, 0);
            navigationView1.Name = "navigationView1";
            navigationView1.PaneDisplayMode = PaneDisplayMode.LeftCompact;
            navigationView1.PaneWidth = 240;
            navigationView1.Size = new Size(790, 388);
            navigationView1.TabIndex = 1;
            navigationView1.Text = "navigationView1";
            navigationView1.Theme = NavViewTheme.Light;
            // 
            // FormPrincipale
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1046, 388);
            Controls.Add(navigationView1);
            Controls.Add(wfPropertyPane1);
            Name = "FormPrincipale";
            Text = "FormPrincipale";
            ResumeLayout(false);
        }

        #endregion

        private Pane.WFPropertyPane wfPropertyPane1;
        private NavigationView navigationView1;
    }
}