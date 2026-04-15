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
        Button1 = New Button()
        NavigationView1 = New NavView.NavigationView()
        WfPropertyPane1 = New STD.Controls.WFPropertyPane()
        SuspendLayout()
        ' 
        ' Button1
        ' 
        Button1.Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        Button1.Location = New Point(317, 493)
        Button1.Name = "Button1"
        Button1.Size = New Size(141, 50)
        Button1.TabIndex = 1
        Button1.Text = "Button1"
        Button1.UseVisualStyleBackColor = True
        ' 
        ' NavigationView1
        ' 
        NavigationView1.AppTitle = ""
        NavigationView1.AutoSizePaneWidth = False
        NavigationView1.CompactPaneWidth = 52
        NavigationView1.ContentHeader = ""
        NavigationView1.IsPaneOpen = False
        NavigationView1.Location = New Point(12, 12)
        NavigationView1.Name = "NavigationView1"
        NavigationView1.PaneDisplayMode = NavView.PaneDisplayMode.LeftCompact
        NavigationView1.PaneWidth = 240
        NavigationView1.Size = New Size(299, 400)
        NavigationView1.TabIndex = 2
        NavigationView1.Text = "NavigationView1"
        NavigationView1.Theme = NavView.NavViewTheme.Light
        ' 
        ' WfPropertyPane1
        ' 
        WfPropertyPane1.BackColor = Color.FromArgb(CByte(30), CByte(30), CByte(28))
        WfPropertyPane1.Dock = DockStyle.Right
        WfPropertyPane1.Location = New Point(488, 0)
        WfPropertyPane1.Name = "WfPropertyPane1"
        WfPropertyPane1.Size = New Size(312, 555)
        WfPropertyPane1.TabIndex = 3
        WfPropertyPane1.Text = "WfPropertyPane1"
        ' 
        ' Form1
        ' 
        AutoScaleDimensions = New SizeF(7F, 17F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(800, 555)
        Controls.Add(WfPropertyPane1)
        Controls.Add(NavigationView1)
        Controls.Add(Button1)
        Name = "Form1"
        Text = "Form1"
        ResumeLayout(False)
    End Sub
    Friend WithEvents Button1 As Button
    Friend WithEvents NavigationView1 As NavView.NavigationView
    Friend WithEvents WfPropertyPane1 As STD.Controls.WFPropertyPane

End Class
