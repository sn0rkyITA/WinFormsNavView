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
        NavigationView2 = New NavView.NavigationView()
        WfPropertyPane2 = New Pane.WFPropertyPane()
        SuspendLayout()
        ' 
        ' NavigationView2
        ' 
        NavigationView2.AppTitle = ""
        NavigationView2.AutoSizePaneWidth = False
        NavigationView2.CompactPaneWidth = 52
        NavigationView2.ContentHeader = ""
        NavigationView2.IsPaneOpen = False
        NavigationView2.Location = New Point(12, 12)
        NavigationView2.Name = "NavigationView2"
        NavigationView2.PaneDisplayMode = NavView.PaneDisplayMode.LeftCompact
        NavigationView2.PaneWidth = 240
        NavigationView2.Size = New Size(200, 297)
        NavigationView2.TabIndex = 0
        NavigationView2.Text = "NavigationView2"
        NavigationView2.Theme = NavView.NavViewTheme.Light
        ' 
        ' WfPropertyPane2
        ' 
        WfPropertyPane2.CollapseMode = Pane.PaneCollapseMode.Compact
        WfPropertyPane2.Dock = DockStyle.Right
        WfPropertyPane2.Location = New Point(218, 0)
        WfPropertyPane2.Name = "WfPropertyPane2"
        WfPropertyPane2.Size = New Size(246, 321)
        WfPropertyPane2.TabIndex = 1
        WfPropertyPane2.Text = "WfPropertyPane2"
        ' 
        ' Form1
        ' 
        AutoScaleDimensions = New SizeF(7F, 17F)
        AutoScaleMode = AutoScaleMode.Font
        ClientSize = New Size(464, 321)
        Controls.Add(WfPropertyPane2)
        Controls.Add(NavigationView2)
        Name = "Form1"
        Text = "Form1"
        ResumeLayout(False)
    End Sub

    Friend WithEvents NavigationView1 As NavView.NavigationView
    Friend WithEvents WfPropertyPane1 As Pane.WFPropertyPane
    Friend WithEvents NavigationView2 As NavView.NavigationView
    Friend WithEvents WfPropertyPane2 As Pane.WFPropertyPane

End Class
