<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        NavigationView1 = New NavView.NavigationView()
        Button1 = New Button()
        SuspendLayout()
        ' 
        ' NavigationView1
        ' 
        NavigationView1.AppTitle = "App di test"
        NavigationView1.AutoSizePaneWidth = True
        NavigationView1.CompactPaneWidth = 64
        NavigationView1.ContentHeader = "Pippoooooooo"
        NavigationView1.Dock = DockStyle.Fill
        NavigationView1.IsPaneOpen = False
        NavigationView1.Location = New Point(0, 0)
        NavigationView1.Name = "NavigationView1"
        NavigationView1.PaneDisplayMode = NavView.PaneDisplayMode.LeftCompact
        NavigationView1.PaneWidth = 100
        NavigationView1.Size = New Size(800, 450)
        NavigationView1.TabIndex = 0
        NavigationView1.Text = "NavigationView1"
        NavigationView1.Theme = NavView.NavViewTheme.Light
        ' 
        ' Button1
        ' 
        Button1.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        Button1.Location = New Point(613, 388)
        Button1.Name = "Button1"
        Button1.Size = New Size(175, 50)
        Button1.TabIndex = 1
        Button1.Text = "Button1"
        Button1.UseVisualStyleBackColor = True
        ' 
        ' Form1
        ' 
        AutoScaleDimensions = New SizeF(7F, 17F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(800, 450)
        Controls.Add(Button1)
        Controls.Add(NavigationView1)
        Name = "Form1"
        Text = "Form1"
        ResumeLayout(False)
    End Sub

    Friend WithEvents NavigationView1 As NavView.NavigationView
    Friend WithEvents Button1 As Button

End Class
