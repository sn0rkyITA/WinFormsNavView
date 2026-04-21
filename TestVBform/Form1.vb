' Form1.vb
' Form di test per NavView. Ogni voce del menu apre un pannello
' con descrizione e controlli interattivi per modificare le proprietà
' del NavigationView e della voce "Cavia" in tempo reale.
'
' CHANGELOG:
' - ToolTipText su tutte le voci del menu
' - SortOrder dimostrabile con riordinamento effettivo
' - HasChildren e Depth mostrati come output informativo
' - IsSelectable mostrato come output nelle pagine correlate
' - Parent mostrato come output
' - NavViewFontResolver.ResolvedSource mostrato nella pagina Info

Imports NavView
Imports Pane

Public Class Form1
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        WfPaneGrip2.BringToFront()
        CARICANAV()
    End Sub
    Private Sub WfPropertyPane2_SizeChanged(sender As Object, e As EventArgs)
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs)
        ' Switch tra Minimal e Compact
        WfPropertyPane2.CollapseMode =
            If(WfPropertyPane2.CollapseMode = Pane.PaneCollapseMode.Minimal,
               PaneCollapseMode.Compact,
               PaneCollapseMode.Minimal)
    End Sub


    Private Sub CARICANAV()
        ' 1. Configurazione essenziale per abilitare l'auto-ridimensionamento
        NavViewVB.ContentMode = ContentMode.ExternalHost
        NavViewVB.PaneDisplayMode = PaneDisplayMode.LeftCompact
        NavViewVB.AutoSizePaneWidth = True
        NavViewVB.AutoSize = True

        ' ⚠️ FONDAMENTALE: NON usare DockStyle.Fill. 
        ' Se il controllo è ancorato al genitore, WinForms sovrascrive AutoSize e forza la larghezza.
        'NavViewVB.Dock = DockStyle.None
        'NavViewVB.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left

        ' 2. Aggiungi un paio di voci (una corta, una lunga per testare il calcolo dinamico)
        Dim home As New NavItem With {
            .Id = "home",
            .Label = "Home",
            .IconGlyph = ChrW(&HE10F)
        }

        Dim settings As New NavItem With {
            .Id = "settings",
            .Label = "Configurazione", ' Etichetta lunga per forzare l'allargamento
            .IconGlyph = ChrW(&HE115),
            .IsExpanded = True
        }

        settings.Children.Add(New NavItem With {.Id = "sub1", .Label = "Generali", .IconGlyph = ChrW(&HE13E)})
        settings.Children.Add(New NavItem With {.Id = "sub2", .Label = "Privacy e Sicurezza", .IconGlyph = ChrW(&HE14A)})

        NavViewVB.MenuItems.Add(home)
        NavViewVB.MenuItems.Add(settings)

        ' 3. Evento opzionale per verificare il funzionamento della selezione
        AddHandler NavViewVB.SelectionChanged, Sub(s, args)
                                                   Debug.WriteLine($"Selezionato: {args.Item?.Label}")
                                               End Sub

    End Sub







End Class

