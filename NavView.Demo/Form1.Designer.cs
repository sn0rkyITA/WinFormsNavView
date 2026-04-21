namespace NavView.Demo
{
    partial class Form1
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
            navigationView1 = new NavigationView();
            SuspendLayout();
            // 
            // navigationView1
            // 
            navigationView1.AppTitle = "";
            navigationView1.CompactPaneWidth = 66;
            navigationView1.ContentHeader = "";
            navigationView1.EnableInternalContent = true;
            navigationView1.IsPaneOpen = false;
            navigationView1.Location = new Point(152, 30);
            navigationView1.Name = "navigationView1";
            navigationView1.PaneDisplayMode = PaneDisplayMode.LeftCompact;
            navigationView1.PaneWidth = 100;
            navigationView1.Size = new Size(384, 384);
            navigationView1.TabIndex = 0;
            navigationView1.Text = "navigationView1";
            navigationView1.Theme = NavViewTheme.Light;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(navigationView1);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private NavigationView navigationView1;
    }
}