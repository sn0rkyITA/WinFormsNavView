Imports Pane

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
        TableLayoutPanel1 = New TableLayoutPanel()
        WffTabView1 = New WinFormsFluentTabView.WFFTabView()
        NavViewVB = New NavView.NavigationView()
        WfPropertyPane3 = New Pane.WFPropertyPane()
        WfPaneGrip2 = New WFPaneGrip()
        TableLayoutPanel2 = New TableLayoutPanel()
        TableLayoutPanel1.SuspendLayout()
        TableLayoutPanel2.SuspendLayout()
        SuspendLayout()
        ' 
        ' TableLayoutPanel1
        ' 
        TableLayoutPanel1.BackColor = SystemColors.ButtonFace
        TableLayoutPanel1.ColumnCount = 3
        TableLayoutPanel1.ColumnStyles.Add(New ColumnStyle())
        TableLayoutPanel1.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        TableLayoutPanel1.ColumnStyles.Add(New ColumnStyle())
        TableLayoutPanel1.Controls.Add(WffTabView1, 1, 0)
        TableLayoutPanel1.Controls.Add(NavViewVB, 0, 0)
        TableLayoutPanel1.Controls.Add(WfPropertyPane3, 2, 0)
        TableLayoutPanel1.Dock = DockStyle.Fill
        TableLayoutPanel1.Location = New Point(0, 0)
        TableLayoutPanel1.Margin = New Padding(0)
        TableLayoutPanel1.Name = "TableLayoutPanel1"
        TableLayoutPanel1.RowCount = 1
        TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        TableLayoutPanel1.RowStyles.Add(New RowStyle(SizeType.Absolute, 20F))
        TableLayoutPanel1.Size = New Size(941, 503)
        TableLayoutPanel1.TabIndex = 4
        ' 
        ' WffTabView1
        ' 
        WffTabView1.BackColor = Color.FromArgb(CByte(255), CByte(192), CByte(128))
        WffTabView1.CornerRadius = 6
        WffTabView1.Dock = DockStyle.Fill
        WffTabView1.Location = New Point(64, 0)
        WffTabView1.Margin = New Padding(0)
        WffTabView1.Name = "WffTabView1"
        WffTabView1.Size = New Size(732, 503)
        WffTabView1.StripHeight = 48
        WffTabView1.TabIndex = 5
        ' 
        ' NavViewVB
        ' 
        NavViewVB.AppTitle = ""
        NavViewVB.CompactPaneWidth = 64
        NavViewVB.ContentHeader = ""
        NavViewVB.ContentMode = NavView.ContentMode.ExternalHost
        NavViewVB.Dock = DockStyle.Left
        NavViewVB.IsPaneOpen = False
        NavViewVB.Location = New Point(0, 0)
        NavViewVB.Margin = New Padding(0)
        NavViewVB.Name = "NavViewVB"
        NavViewVB.PaneDisplayMode = NavView.PaneDisplayMode.LeftCompact
        NavViewVB.PaneWidth = 200
        NavViewVB.Size = New Size(64, 503)
        NavViewVB.TabIndex = 5
        NavViewVB.Text = "NavigationView3"
        NavViewVB.Theme = NavView.NavViewTheme.Light
        ' 
        ' WfPropertyPane3
        ' 
        WfPropertyPane3.BackColor = SystemColors.ActiveBorder
        WfPropertyPane3.CollapseMode = PaneCollapseMode.Compact
        WfPropertyPane3.Dock = DockStyle.Right
        WfPropertyPane3.ForeColor = SystemColors.ActiveCaption
        WfPropertyPane3.Location = New Point(796, 0)
        WfPropertyPane3.Margin = New Padding(0)
        WfPropertyPane3.Name = "WfPropertyPane3"
        WfPropertyPane3.Size = New Size(145, 503)
        WfPropertyPane3.TabIndex = 5
        WfPropertyPane3.Text = "WfPropertyPane3"
        ' 
        ' WfPaneGrip2
        ' 
        WfPaneGrip2.BackColor = Color.Transparent
        WfPaneGrip2.Location = New Point(931, 0)
        WfPaneGrip2.Name = "WfPaneGrip2"
        WfPaneGrip2.Size = New Size(10, 48)
        WfPaneGrip2.TabIndex = 6
        WfPaneGrip2.TargetPane = WfPropertyPane3
        WfPaneGrip2.Text = "WfPaneGrip2"
        WfPaneGrip2.Visible = False
        ' 
        ' TableLayoutPanel2
        ' 
        TableLayoutPanel2.ColumnCount = 1
        TableLayoutPanel2.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100F))
        TableLayoutPanel2.Controls.Add(TableLayoutPanel1, 0, 0)
        TableLayoutPanel2.Dock = DockStyle.Fill
        TableLayoutPanel2.Location = New Point(0, 0)
        TableLayoutPanel2.Margin = New Padding(0)
        TableLayoutPanel2.Name = "TableLayoutPanel2"
        TableLayoutPanel2.RowCount = 2
        TableLayoutPanel2.RowStyles.Add(New RowStyle(SizeType.Percent, 100F))
        TableLayoutPanel2.RowStyles.Add(New RowStyle(SizeType.Absolute, 30F))
        TableLayoutPanel2.Size = New Size(941, 533)
        TableLayoutPanel2.TabIndex = 7
        ' 
        ' Form1
        ' 
        AutoScaleDimensions = New SizeF(7F, 17F)
        AutoScaleMode = AutoScaleMode.Font
        BackColor = SystemColors.AppWorkspace
        ClientSize = New Size(941, 533)
        Controls.Add(TableLayoutPanel2)
        Controls.Add(WfPaneGrip2)
        Name = "Form1"
        Text = "Form1"
        TableLayoutPanel1.ResumeLayout(False)
        TableLayoutPanel1.PerformLayout()
        TableLayoutPanel2.ResumeLayout(False)
        ResumeLayout(False)
    End Sub

    Friend WithEvents NavigationView1 As NavView.NavigationView
    Friend WithEvents WfPropertyPane1 As Pane.WFPropertyPane
    Friend WithEvents WfPropertyPane2 As Pane.WFPropertyPane
    Friend WithEvents WfPaneGrip1 As WFPaneGrip
    Friend WithEvents TableLayoutPanel1 As TableLayoutPanel
    Friend WithEvents WfTabView2 As WinFormsFluentTabView.WFFTabView
    Friend WithEvents NavigationView2 As NavView.NavigationView
    Friend WithEvents NavViewVB As NavView.NavigationView
    Friend WithEvents WffTabView1 As WinFormsFluentTabView.WFFTabView
    Friend WithEvents WfPropertyPane3 As Pane.WFPropertyPane
    Friend WithEvents WfPaneGrip2 As WFPaneGrip
    Friend WithEvents TableLayoutPanel2 As TableLayoutPanel

End Class
