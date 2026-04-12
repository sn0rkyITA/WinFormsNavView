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

Public Class Form1

    ' Voce "cavia" su cui testare le proprietà NavItem
    Private _cavia As NavItem

    ' -------------------------------------------------------------------------
    ' Inizializzazione
    ' -------------------------------------------------------------------------

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Me.Text = "NavView — Test Interattivo"
        Me.Size = New Size(1100, 700)
        Me.StartPosition = FormStartPosition.CenterScreen

        ConfigureNavView()
        PopulateMenu()
        NavigationView1.FitPaneWidth()
        NavigationView1.Navigate(NavigationView1.MenuItems(0))
    End Sub

    Private Sub ConfigureNavView()
        NavigationView1.AppTitle = "NavView Test"
        NavigationView1.PaneDisplayMode = PaneDisplayMode.LeftCompact
        NavigationView1.Theme = NavViewTheme.Light
        NavigationView1.PaneWidth = 240
        NavigationView1.CompactPaneWidth = 56
        AddHandler NavigationView1.SelectionChanged, AddressOf OnSelectionChanged
    End Sub

    Private Sub PopulateMenu()

        ' --- Gruppo: NavigationView ------------------------------------------
        NavigationView1.MenuItems.Add(NavItem.GroupHeader("NavigationView"))

        Dim itemAppTitle = NavItem.Create("AppTitle", FluentIcons.Edit, tag:="apptitle")
        itemAppTitle.ToolTipText = "Testo nell'header del pane quando è aperto"
        NavigationView1.MenuItems.Add(itemAppTitle)

        Dim itemPaneWidth = NavItem.Create("PaneWidth", FluentIcons.Expand, tag:="panewidth")
        itemPaneWidth.ToolTipText = "Larghezza del pane aperto in pixel"
        NavigationView1.MenuItems.Add(itemPaneWidth)

        Dim itemCompactWidth = NavItem.Create("CompactPaneWidth", FluentIcons.Collapse, tag:="compactpanewidth")
        itemCompactWidth.ToolTipText = "Larghezza del pane compatto in pixel"
        NavigationView1.MenuItems.Add(itemCompactWidth)

        Dim itemDisplayMode = NavItem.Create("PaneDisplayMode", FluentIcons.MoreVertical, tag:="panedisplaymode")
        itemDisplayMode.ToolTipText = "Left = sempre espanso, LeftCompact = compatto"
        NavigationView1.MenuItems.Add(itemDisplayMode)

        Dim itemPaneOpen = NavItem.Create("IsPaneOpen", FluentIcons.ChevronRight, tag:="ispaneopen")
        itemPaneOpen.ToolTipText = "Apre o chiude il pane programmaticamente"
        NavigationView1.MenuItems.Add(itemPaneOpen)

        Dim itemTheme = NavItem.Create("Theme", FluentIcons.View, tag:="theme")
        itemTheme.ToolTipText = "Tema visivo: Light o Dark"
        NavigationView1.MenuItems.Add(itemTheme)

        Dim itemContentHeader = NavItem.Create("ContentHeader", FluentIcons.Document, tag:="contentheader")
        itemContentHeader.ToolTipText = "Testo nell'header dell'area contenuto"
        NavigationView1.MenuItems.Add(itemContentHeader)

        NavigationView1.MenuItems.Add(NavItem.Separator())

        ' --- Gruppo: NavItem -------------------------------------------------
        NavigationView1.MenuItems.Add(NavItem.GroupHeader("NavItem (Cavia)"))

        Dim itemLabel = NavItem.Create("Label", FluentIcons.Edit, tag:="label")
        itemLabel.ToolTipText = "Testo visualizzato accanto all'icona"
        NavigationView1.MenuItems.Add(itemLabel)

        Dim itemIconGlyph = NavItem.Create("IconGlyph", FluentIcons.Star, tag:="iconglyph")
        itemIconGlyph.ToolTipText = "Glyph Unicode Segoe Fluent Icons"
        NavigationView1.MenuItems.Add(itemIconGlyph)

        Dim itemCustomIcon = NavItem.Create("CustomIcon", FluentIcons.Image, tag:="customicon")
        itemCustomIcon.ToolTipText = "Immagine raster al posto del glyph"
        NavigationView1.MenuItems.Add(itemCustomIcon)

        Dim itemTag = NavItem.Create("Tag", FluentIcons.Tag, tag:="tag")
        itemTag.ToolTipText = "Dato arbitrario associato alla voce"
        NavigationView1.MenuItems.Add(itemTag)

        Dim itemIsEnabled = NavItem.Create("IsEnabled", FluentIcons.Checkmark, tag:="isenabled")
        itemIsEnabled.ToolTipText = "Se False la voce è visibile ma non interagibile"
        NavigationView1.MenuItems.Add(itemIsEnabled)

        Dim itemIsVisible = NavItem.Create("IsVisible", FluentIcons.Visibility, tag:="isvisible")
        itemIsVisible.ToolTipText = "Se False la voce è esclusa dal layout"
        NavigationView1.MenuItems.Add(itemIsVisible)

        Dim itemIsExpanded = NavItem.Create("IsExpanded", FluentIcons.ChevronDown, tag:="isexpanded")
        itemIsExpanded.ToolTipText = "Se True i figli sono visibili (richiede HasChildren)"
        NavigationView1.MenuItems.Add(itemIsExpanded)

        Dim itemDeselect = NavItem.Create("DeselectOnClick", FluentIcons.Remove, tag:="deselectonclick")
        itemDeselect.ToolTipText = "Se True ri-cliccando deseleziona la voce"
        NavigationView1.MenuItems.Add(itemDeselect)

        Dim itemNotif = NavItem.Create("HasNotification", FluentIcons.Info, tag:="hasnotification")
        itemNotif.ToolTipText = "Mostra un pallino colorato sull'icona"
        NavigationView1.MenuItems.Add(itemNotif)

        Dim itemBadge = NavItem.Create("BadgeCount", FluentIcons.Flag, tag:="badgecount")
        itemBadge.ToolTipText = "Numero badge (predisposto, non ancora renderizzato)"
        NavigationView1.MenuItems.Add(itemBadge)

        Dim itemSort = NavItem.Create("SortOrder", FluentIcons.Sort, tag:="sortorder")
        itemSort.ToolTipText = "Peso per ordinamento gestito dall'host"
        NavigationView1.MenuItems.Add(itemSort)

        Dim itemTooltip = NavItem.Create("ToolTipText", FluentIcons.Table, tag:="tooltiptext")
        itemTooltip.ToolTipText = "Tooltip mostrato al passaggio del mouse"
        NavigationView1.MenuItems.Add(itemTooltip)

        Dim itemExecute = NavItem.Create("ExecuteAction", FluentIcons.Send, tag:="executeaction")
        itemExecute.ToolTipText = "Callback inline eseguita alla selezione"
        NavigationView1.MenuItems.Add(itemExecute)

        Dim itemCtx = NavItem.Create("ContextMenuStrip", FluentIcons.More, tag:="contextmenustrip")
        itemCtx.ToolTipText = "Menu contestuale al tasto destro sulla voce"
        NavigationView1.MenuItems.Add(itemCtx)

        ' --- Proprietà calcolate ---------------------------------------------
        NavigationView1.MenuItems.Add(NavItem.Separator())
        NavigationView1.MenuItems.Add(NavItem.GroupHeader("Proprietà calcolate"))

        Dim itemCalc = NavItem.Create("HasChildren / Depth / IsSelectable / Parent", FluentIcons.Database, tag:="calcolate")
        itemCalc.ToolTipText = "Proprietà readonly derivate dallo stato della voce"
        NavigationView1.MenuItems.Add(itemCalc)

        ' --- Footer ----------------------------------------------------------
        Dim itemInfo = NavItem.Create("Info", FluentIcons.Info, tag:="info")
        itemInfo.ToolTipText = "Informazioni sul form di test e sul font resolver"
        NavigationView1.FooterMenuItems.Add(itemInfo)

        _cavia = NavItem.Create("⬤ Cavia", FluentIcons.Person, tag:="cavia")
        _cavia.ToolTipText = "Questa voce è il soggetto dei test NavItem"
        NavigationView1.FooterMenuItems.Add(_cavia)

    End Sub

    ' -------------------------------------------------------------------------
    ' Selezione
    ' -------------------------------------------------------------------------

    Private Sub OnSelectionChanged(sender As Object, e As NavSelectionChangedEventArgs)
        If e.Item Is Nothing Then
            NavigationView1.SetContent(Nothing)
            Return
        End If

        Dim page As Control = BuildPage(If(e.Item.Tag?.ToString(), String.Empty))
        NavigationView1.SetContent(page)
    End Sub

    ' -------------------------------------------------------------------------
    ' Factory pagine
    ' -------------------------------------------------------------------------

    Private Function BuildPage(tag As String) As UserControl
        Select Case tag

            Case "apptitle"
                Return PageBuilder.Create(
                    "AppTitle",
                    "Testo mostrato nell'header del pane quando è aperto." & vbLf &
                    "Stringa vuota = nessun testo visualizzato.",
                    Sub(p)
                        Dim tb = PageBuilder.AddTextBox(p, "Valore corrente:", NavigationView1.AppTitle)
                        AddHandler tb.TextChanged, Sub(s, ev)
                                                       NavigationView1.AppTitle = tb.Text
                                                   End Sub
                    End Sub)

            Case "panewidth"
                Return PageBuilder.Create(
                    "PaneWidth",
                    "Larghezza del pane in pixel quando è completamente aperto." & vbLf &
                    "Minimo 100px. Modificare con il pane aperto per vedere l'effetto immediato.",
                    Sub(p)
                        Dim nud = PageBuilder.AddNumericUpDown(p, "Valore (px):", NavigationView1.PaneWidth, 100, 600)
                        AddHandler nud.ValueChanged, Sub(s, ev)
                                                         NavigationView1.PaneWidth = CInt(nud.Value)
                                                     End Sub
                    End Sub)

            Case "compactpanewidth"
                Return PageBuilder.Create(
                    "CompactPaneWidth",
                    "Larghezza del pane in modalità compatta (solo icone)." & vbLf &
                    "Minimo 32px. Modificare con il pane chiuso per vedere l'effetto immediato.",
                    Sub(p)
                        Dim nud = PageBuilder.AddNumericUpDown(p, "Valore (px):", NavigationView1.CompactPaneWidth, 32, 120)
                        AddHandler nud.ValueChanged, Sub(s, ev)
                                                         NavigationView1.CompactPaneWidth = CInt(nud.Value)
                                                     End Sub
                    End Sub)

            Case "panedisplaymode"
                Return PageBuilder.Create(
                    "PaneDisplayMode",
                    "Controlla il comportamento del pannello laterale." & vbLf &
                    "Left = pane sempre espanso e fisso, larghezza piena." & vbLf &
                    "LeftCompact = pane compatto (solo icone), si apre con hamburger o click su padre.",
                    Sub(p)
                        Dim cb = PageBuilder.AddComboBox(p, "Modalità:",
                            {"Left", "LeftCompact"},
                            If(NavigationView1.PaneDisplayMode = PaneDisplayMode.Left, 0, 1))
                        AddHandler cb.SelectedIndexChanged, Sub(s, ev)
                                                                NavigationView1.PaneDisplayMode =
                                                                    If(cb.SelectedIndex = 0,
                                                                       PaneDisplayMode.Left,
                                                                       PaneDisplayMode.LeftCompact)
                                                            End Sub
                    End Sub)

            Case "ispaneopen"
                Return PageBuilder.Create(
                    "IsPaneOpen",
                    "Apre o chiude il pane programmaticamente." & vbLf &
                    "Ha effetto solo in modalità LeftCompact." & vbLf &
                    "In modalità Left il pane è sempre aperto.",
                    Sub(p)
                        Dim chk = PageBuilder.AddCheckBox(p, "Pane aperto", NavigationView1.IsPaneOpen)
                        AddHandler chk.CheckedChanged, Sub(s, ev)
                                                           NavigationView1.IsPaneOpen = chk.Checked
                                                       End Sub
                    End Sub)

            Case "theme"
                Return PageBuilder.Create(
                    "Theme",
                    "Tema visivo del controllo." & vbLf &
                    "Light = stile Windows 11 chiaro (sfondo #F3F3F3, accento blu)." & vbLf &
                    "Dark = stile Windows 11 scuro (sfondo #202020, accento azzurro).",
                    Sub(p)
                        Dim cb = PageBuilder.AddComboBox(p, "Tema:",
                            {"Light", "Dark"},
                            If(NavigationView1.Theme = NavViewTheme.Light, 0, 1))
                        AddHandler cb.SelectedIndexChanged, Sub(s, ev)
                                                                NavigationView1.Theme =
                                                                    If(cb.SelectedIndex = 0,
                                                                       NavViewTheme.Light,
                                                                       NavViewTheme.Dark)
                                                            End Sub
                    End Sub)

            Case "contentheader"
                Return PageBuilder.Create(
                    "ContentHeader",
                    "Testo mostrato nell'header dell'area contenuto (a destra del pane)." & vbLf &
                    "Stringa vuota = header nascosto, il contenuto occupa tutta l'altezza." & vbLf &
                    "Viene aggiornato automaticamente alla selezione di una voce.",
                    Sub(p)
                        Dim tb = PageBuilder.AddTextBox(p, "Valore corrente:", NavigationView1.ContentHeader)
                        AddHandler tb.TextChanged, Sub(s, ev)
                                                       NavigationView1.ContentHeader = tb.Text
                                                   End Sub
                        PageBuilder.AddNote(p, "La selezione di una voce sovrascrive questo valore con il Label della voce.")
                    End Sub)

            Case "label"
                Return PageBuilder.Create(
                    "NavItem.Label",
                    "Testo visualizzato accanto all'icona quando il pane è aperto." & vbLf &
                    "In modalità compatta il testo non è visibile, solo l'icona.",
                    Sub(p)
                        Dim tb = PageBuilder.AddTextBox(p, "Label della Cavia:", _cavia.Label)
                        AddHandler tb.TextChanged, Sub(s, ev)
                                                       _cavia.Label = tb.Text
                                                       NavigationView1.Invalidate()
                                                   End Sub
                        PageBuilder.AddNote(p, "Apri il pane per vedere il Label della Cavia nel footer.")
                    End Sub)

            Case "iconglyph"
                Return PageBuilder.Create(
                    "NavItem.IconGlyph",
                    "Glyph Unicode del font Segoe Fluent Icons (o MDL2 Assets su Win10)." & vbLf &
                    "Usa FluentIcons.NomeIcona per i valori predefiniti." & vbLf &
                    "Ignorato se CustomIcon è impostato.",
                    Sub(p)
                        Dim cb = PageBuilder.AddComboBox(p, "Icona predefinita:",
                            {"Home", "Settings", "Map", "Person", "Star", "Flag",
                             "Calendar", "Calculator", "Globe", "Compass", "Layers",
                             "Document", "Folder", "Database", "Chart", "Dashboard"}, 0)
                        AddHandler cb.SelectedIndexChanged, Sub(s, ev)
                                                                _cavia.IconGlyph = FluentIcons.Get(If(cb.SelectedItem?.ToString(), ""))
                                                                _cavia.CustomIcon = Nothing
                                                                NavigationView1.Invalidate()
                                                            End Sub
                        Dim tb = PageBuilder.AddTextBox(p, "Oppure glyph diretto (es. \uE80F):", _cavia.IconGlyph)
                        AddHandler tb.TextChanged, Sub(s, ev)
                                                       If tb.Text.Length > 0 Then
                                                           _cavia.IconGlyph = tb.Text
                                                           _cavia.CustomIcon = Nothing
                                                           NavigationView1.Invalidate()
                                                       End If
                                                   End Sub
                    End Sub)

            Case "customicon"
                Return PageBuilder.Create(
                    "NavItem.CustomIcon",
                    "Immagine raster (PNG, BMP, JPG) che sostituisce il glyph IconGlyph." & vbLf &
                    "Se impostata, IconGlyph viene completamente ignorato." & vbLf &
                    "Il controllo non gestisce il Dispose: responsabilità dell'host.",
                    Sub(p)
                        Dim btnLoad = PageBuilder.AddButton(p, "Carica immagine da file...")
                        Dim btnClear = PageBuilder.AddButton(p, "Rimuovi CustomIcon (torna a glyph)")
                        Dim lblStatus = PageBuilder.AddOutputLabel(p, "Nessuna immagine caricata.")
                        AddHandler btnLoad.Click, Sub(s, ev)
                                                      Using ofd As New OpenFileDialog()
                                                          ofd.Filter = "Immagini|*.png;*.bmp;*.jpg;*.gif"
                                                          If ofd.ShowDialog() = DialogResult.OK Then
                                                              _cavia.CustomIcon = Image.FromFile(ofd.FileName)
                                                              lblStatus.Text = $"Caricata: {System.IO.Path.GetFileName(ofd.FileName)}"
                                                              NavigationView1.Invalidate()
                                                          End If
                                                      End Using
                                                  End Sub
                        AddHandler btnClear.Click, Sub(s, ev)
                                                       _cavia.CustomIcon = Nothing
                                                       lblStatus.Text = "CustomIcon rimossa. Ripristinato IconGlyph."
                                                       NavigationView1.Invalidate()
                                                   End Sub
                    End Sub)

            Case "tag"
                Return PageBuilder.Create(
                    "NavItem.Tag",
                    "Dato arbitrario associato alla voce. Il NavigationView non lo usa." & vbLf &
                    "Tipicamente contiene un identificatore o il tipo di UserControl da mostrare." & vbLf &
                    "Accessibile nell'evento SelectionChanged tramite e.Item.Tag.",
                    Sub(p)
                        Dim tb = PageBuilder.AddTextBox(p, "Tag (stringa):", If(_cavia.Tag?.ToString(), ""))
                        AddHandler tb.TextChanged, Sub(s, ev)
                                                       _cavia.Tag = tb.Text
                                                   End Sub
                    End Sub)

            Case "isenabled"
                Return PageBuilder.Create(
                    "NavItem.IsEnabled",
                    "Se False la voce è visualizzata ma non selezionabile né cliccabile." & vbLf &
                    "Il colore diventa attenuato (ItemDisabledForeground)." & vbLf &
                    "Nessun hover, nessun click destro, nessuna selezione.",
                    Sub(p)
                        Dim lblIs = PageBuilder.AddOutputLabel(p, $"IsSelectable = {_cavia.IsSelectable}")
                        Dim chk = PageBuilder.AddCheckBox(p, "Cavia abilitata", _cavia.IsEnabled)
                        AddHandler chk.CheckedChanged, Sub(s, ev)
                                                           _cavia.IsEnabled = chk.Checked
                                                           lblIs.Text = $"IsSelectable = {_cavia.IsSelectable}"
                                                           NavigationView1.Invalidate()
                                                       End Sub
                        PageBuilder.AddNote(p, "IsSelectable dipende da IsEnabled, IsSeparator, IsGroupHeader e IsVisible.")
                    End Sub)

            Case "isvisible"
                Return PageBuilder.Create(
                    "NavItem.IsVisible",
                    "Se False la voce è esclusa dal layout e dal rendering." & vbLf &
                    "Rimane nella collezione e può essere resa visibile in qualsiasi momento." & vbLf &
                    "Diverso da IsEnabled: la voce è completamente assente visivamente.",
                    Sub(p)
                        Dim lblIs = PageBuilder.AddOutputLabel(p, $"IsSelectable = {_cavia.IsSelectable}")
                        Dim chk = PageBuilder.AddCheckBox(p, "Cavia visibile", _cavia.IsVisible)
                        AddHandler chk.CheckedChanged, Sub(s, ev)
                                                           _cavia.IsVisible = chk.Checked
                                                           lblIs.Text = $"IsSelectable = {_cavia.IsSelectable}"
                                                           NavigationView1.Invalidate()
                                                       End Sub
                        PageBuilder.AddNote(p, "La Cavia è nel footer — osserva il footer mentre cambi il valore.")
                    End Sub)

            Case "isexpanded"
                Return PageBuilder.Create(
                    "NavItem.IsExpanded",
                    "Se True i figli della voce sono visibili nel pane." & vbLf &
                    "Rilevante solo se HasChildren = True." & vbLf &
                    "HasChildren è calcolato: True se Children.Count > 0 e non è separatore/header.",
                    Sub(p)
                        Dim lblInfo = PageBuilder.AddOutputLabel(p, GetCaviaCalcolate())
                        Dim btnAdd = PageBuilder.AddButton(p, "Aggiungi figlio alla Cavia")
                        Dim btnRemove = PageBuilder.AddButton(p, "Rimuovi tutti i figli")
                        Dim chk = PageBuilder.AddCheckBox(p, "IsExpanded", _cavia.IsExpanded)

                        AddHandler btnAdd.Click, Sub(s, ev)
                                                     _cavia.Children.Add(NavItem.Create(
                                                         "Figlio " & (_cavia.Children.Count + 1).ToString(),
                                                         FluentIcons.ChevronRight))
                                                     lblInfo.Text = GetCaviaCalcolate()
                                                     NavigationView1.Invalidate()
                                                 End Sub
                        AddHandler btnRemove.Click, Sub(s, ev)
                                                        _cavia.Children.Clear()
                                                        _cavia.IsExpanded = False
                                                        chk.Checked = False
                                                        lblInfo.Text = GetCaviaCalcolate()
                                                        NavigationView1.Invalidate()
                                                    End Sub
                        AddHandler chk.CheckedChanged, Sub(s, ev)
                                                           _cavia.IsExpanded = chk.Checked
                                                           NavigationView1.Invalidate()
                                                       End Sub
                    End Sub)

            Case "deselectonclick"
                Return PageBuilder.Create(
                    "NavItem.DeselectOnClick",
                    "Se True, cliccando la voce già selezionata la deseleziona." & vbLf &
                    "SelectionChanged viene sparato con e.Item = Nothing." & vbLf &
                    "Default False — il secondo click non ha effetto.",
                    Sub(p)
                        Dim chk = PageBuilder.AddCheckBox(p, "DeselectOnClick attivo sulla Cavia", _cavia.DeselectOnClick)
                        AddHandler chk.CheckedChanged, Sub(s, ev)
                                                           _cavia.DeselectOnClick = chk.Checked
                                                       End Sub
                        PageBuilder.AddNote(p, "Seleziona la Cavia nel footer, poi riclicca per vedere la deselezione.")
                    End Sub)

            Case "hasnotification"
                Return PageBuilder.Create(
                    "NavItem.HasNotification",
                    "Se True mostra un pallino colorato sovrapposto all'icona in alto a destra." & vbLf &
                    "Il colore è definito da NavViewColors.NotificationDotColor." & vbLf &
                    "Light: blu (#0078D4)   Dark: azzurro (#4CC2FF).",
                    Sub(p)
                        Dim chk = PageBuilder.AddCheckBox(p, "Mostra pallino notifica sulla Cavia", _cavia.HasNotification)
                        AddHandler chk.CheckedChanged, Sub(s, ev)
                                                           _cavia.HasNotification = chk.Checked
                                                           NavigationView1.Invalidate()
                                                       End Sub
                        PageBuilder.AddNote(p, "Il pallino è visibile sia in modalità compatta che aperta.")
                    End Sub)

            Case "badgecount"
                Return PageBuilder.Create(
                    "NavItem.BadgeCount",
                    "Numero intero predisposto per un badge numerico futuro." & vbLf &
                    "Attualmente NON viene renderizzato dal controllo." & vbLf &
                    "È un placeholder per versioni future del renderer.",
                    Sub(p)
                        Dim nud = PageBuilder.AddNumericUpDown(p, "Valore:", _cavia.BadgeCount, 0, 999)
                        AddHandler nud.ValueChanged, Sub(s, ev)
                                                         _cavia.BadgeCount = CInt(nud.Value)
                                                     End Sub
                        PageBuilder.AddNote(p, "BadgeCount non produce effetti visivi nella versione corrente.")
                    End Sub)

            Case "sortorder"
                Return PageBuilder.Create(
                    "NavItem.SortOrder",
                    "Peso numerico per l'ordinamento. Non applicato automaticamente." & vbLf &
                    "L'host usa questo valore per costruire la collezione nell'ordine voluto." & vbLf &
                    "Il pulsante sotto ricostruisce il footer ordinando per SortOrder.",
                    Sub(p)
                        Dim lblStatus = PageBuilder.AddOutputLabel(p, GetSortOrderStatus())
                        Dim nud = PageBuilder.AddNumericUpDown(p, "SortOrder della Cavia:", _cavia.SortOrder, -999, 999)
                        AddHandler nud.ValueChanged, Sub(s, ev)
                                                         _cavia.SortOrder = CInt(nud.Value)
                                                         lblStatus.Text = GetSortOrderStatus()
                                                     End Sub
                        Dim btnSort = PageBuilder.AddButton(p, "Riordina FooterMenuItems per SortOrder")
                        AddHandler btnSort.Click, Sub(s, ev)
                                                      SortFooterBySortOrder()
                                                      lblStatus.Text = GetSortOrderStatus() & " [riordinato]"
                                                  End Sub
                        PageBuilder.AddNote(p, "Il riordinamento modifica la collezione FooterMenuItems in modo permanente.")
                    End Sub)

            Case "tooltiptext"
                Return PageBuilder.Create(
                    "NavItem.ToolTipText",
                    "Testo del tooltip nativo WinForms mostrato al passaggio del mouse sulla voce." & vbLf &
                    "Stringa vuota = nessun tooltip." & vbLf &
                    "Il ritardo e la durata seguono le impostazioni di sistema.",
                    Sub(p)
                        Dim tb = PageBuilder.AddTextBox(p, "ToolTipText della Cavia:", _cavia.ToolTipText)
                        AddHandler tb.TextChanged, Sub(s, ev)
                                                       _cavia.ToolTipText = tb.Text
                                                   End Sub
                        PageBuilder.AddNote(p, "Passa il mouse sulla Cavia nel footer per vedere il tooltip.")
                    End Sub)

            Case "executeaction"
                Return PageBuilder.Create(
                    "NavItem.ExecuteAction",
                    "Callback Action(Of NavItem) eseguita alla selezione della voce," & vbLf &
                    "in aggiunta all'evento SelectionChanged del NavigationView." & vbLf &
                    "Utile per logica inline. Nothing = nessuna azione aggiuntiva.",
                    Sub(p)
                        Dim lblOutput = PageBuilder.AddOutputLabel(p, "Seleziona la Cavia per vedere l'output qui.")
                        Dim chk = PageBuilder.AddCheckBox(p, "Attiva ExecuteAction sulla Cavia", _cavia.ExecuteAction IsNot Nothing)
                        AddHandler chk.CheckedChanged, Sub(s, ev)
                                                           If chk.Checked Then
                                                               _cavia.ExecuteAction = Sub(item)
                                                                                          lblOutput.Text = $"ExecuteAction! Label={item.Label}  Tag={item.Tag}  {DateTime.Now:HH:mm:ss}"
                                                                                      End Sub
                                                           Else
                                                               _cavia.ExecuteAction = Nothing
                                                               lblOutput.Text = "ExecuteAction rimossa."
                                                           End If
                                                       End Sub
                    End Sub)

            Case "contextmenustrip"
                Return PageBuilder.Create(
                    "NavItem.ContextMenuStrip",
                    "Menu contestuale mostrato al click destro sulla voce." & vbLf &
                    "Nothing = nessun menu contestuale al tasto destro." & vbLf &
                    "Il ContextMenuStrip è un componente WinForms standard.",
                    Sub(p)
                        Dim lblOutput = PageBuilder.AddOutputLabel(p, "Tasto destro sulla Cavia per testare.")
                        Dim chk = PageBuilder.AddCheckBox(p, "Attiva ContextMenuStrip sulla Cavia", _cavia.ContextMenuStrip IsNot Nothing)
                        AddHandler chk.CheckedChanged, Sub(s, ev)
                                                           If chk.Checked Then
                                                               Dim ctx As New ContextMenuStrip()
                                                               ctx.Items.Add("Opzione A")
                                                               ctx.Items.Add("Opzione B")
                                                               ctx.Items.Add(New ToolStripSeparator())
                                                               ctx.Items.Add("Elimina")
                                                               AddHandler ctx.ItemClicked, Sub(s2, ev2)
                                                                                               lblOutput.Text = $"Cliccato: {ev2.ClickedItem.Text}  {DateTime.Now:HH:mm:ss}"
                                                                                           End Sub
                                                               _cavia.ContextMenuStrip = ctx
                                                           Else
                                                               _cavia.ContextMenuStrip = Nothing
                                                               lblOutput.Text = "ContextMenuStrip rimosso."
                                                           End If
                                                       End Sub
                        PageBuilder.AddNote(p, "Tasto destro sulla Cavia nel footer per aprire il menu.")
                    End Sub)

            Case "calcolate"
                Return PageBuilder.Create(
                    "Proprietà calcolate (readonly)",
                    "Queste proprietà sono derivate dallo stato della voce e non sono impostabili direttamente." & vbLf &
                    "Si aggiornano automaticamente al cambiare delle proprietà da cui dipendono.",
                    Sub(p)
                        Dim lblOut = PageBuilder.AddOutputLabel(p, GetCaviaCalcolate())
                        lblOut.Height = 80
                        Dim btnRefresh = PageBuilder.AddButton(p, "Aggiorna valori")
                        AddHandler btnRefresh.Click, Sub(s, ev)
                                                         lblOut.Text = GetCaviaCalcolate()
                                                     End Sub
                        PageBuilder.AddNote(p, "HasChildren: True se Children.Count > 0 e non è separatore né group header.")
                        PageBuilder.AddNote(p, "IsSelectable: True se IsEnabled, IsVisible, non separatore, non group header.")
                        PageBuilder.AddNote(p, "Depth: 0 per voci radice, +1 per ogni livello di gerarchia.")
                        PageBuilder.AddNote(p, "Parent: riferimento al NavItem padre. Nothing se voce radice.")
                    End Sub)

            Case "info"
                Return PageBuilder.Create(
                    "NavView Test — Info",
                    "Form di test interattivo per tutte le proprietà di NavigationView e NavItem." & vbLf &
                    "La voce '⬤ Cavia' nel footer è il soggetto di tutti i test NavItem.",
                    Sub(p)
                        Dim fontSource = NavViewFontResolver.ResolvedSource.ToString()
                        Dim fontName = NavViewFontResolver.FontName
                        PageBuilder.AddOutputLabel(p,
                            $"Font icone: {fontName}" & vbLf &
                            $"Origine: {fontSource}" & vbLf &
                            $"System = Segoe Fluent installato (Win11)" & vbLf &
                            $"Embedded = caricato dalla DLL (Win10)" & vbLf &
                            $"Mdl2Fallback = Segoe MDL2 Assets (fallback finale)")
                        PageBuilder.AddNote(p, "Versione NavView: 0.2.0")
                    End Sub)

            Case Else
                Return PageBuilder.Create(tag, "Pagina non trovata.",
                                          Sub(p)
                                          End Sub)
        End Select
    End Function

    ' -------------------------------------------------------------------------
    ' Helpers
    ' -------------------------------------------------------------------------

    Private Function GetCaviaCalcolate() As String
        Return $"HasChildren  = {_cavia.HasChildren}  (figli: {_cavia.Children.Count})" & vbLf &
               $"IsSelectable = {_cavia.IsSelectable}" & vbLf &
               $"Depth        = {_cavia.Depth}" & vbLf &
               $"Parent       = {If(_cavia.Parent IsNot Nothing, _cavia.Parent.Label, "Nothing (voce radice)")}"
    End Function

    Private Function GetSortOrderStatus() As String
        Dim sb As New System.Text.StringBuilder()
        For Each item In NavigationView1.FooterMenuItems
            sb.AppendLine($"[{item.SortOrder,4}] {item.Label}")
        Next
        Return sb.ToString().TrimEnd()
    End Function

    Private Sub SortFooterBySortOrder()
        Dim sorted = NavigationView1.FooterMenuItems.OrderBy(Function(i) i.SortOrder).ToList()
        NavigationView1.FooterMenuItems.Clear()
        For Each item In sorted
            NavigationView1.FooterMenuItems.Add(item)
        Next
    End Sub

End Class

' =============================================================================
' PageBuilder — helper per costruire i pannelli di test
' =============================================================================

Friend Module PageBuilder

    Private Const LabelColor As Integer = &H1A1A1A
    Private Const NoteColor As Integer = &HA0A0A0
    Private Const DescColor As Integer = &H505050
    Private Const Margin As Integer = 24
    Private Const ControlSpacing As Integer = 12

    Public Function Create(title As String, description As String,
                           buildAction As Action(Of FlowLayoutPanel)) As UserControl

        Dim uc As New UserControl() With {.BackColor = Color.White}

        Dim lblTitle As New Label() With {
            .Text = title,
            .Font = New Font("Segoe UI", 16.0F, FontStyle.Bold),
            .ForeColor = Color.FromArgb(LabelColor),
            .AutoSize = True,
            .Location = New Point(Margin, Margin)
        }
        uc.Controls.Add(lblTitle)

        Dim lblDesc As New Label() With {
            .Text = description,
            .Font = New Font("Segoe UI", 10.0F),
            .ForeColor = Color.FromArgb(DescColor),
            .AutoSize = False,
            .Width = 620,
            .Height = 90,
            .Location = New Point(Margin, Margin + lblTitle.PreferredHeight + 8)
        }
        uc.Controls.Add(lblDesc)

        Dim sep As New Panel() With {
            .BackColor = Color.FromArgb(220, 220, 220),
            .Height = 1,
            .Width = 660,
            .Location = New Point(Margin, lblDesc.Bottom + 12)
        }
        uc.Controls.Add(sep)

        Dim flow As New FlowLayoutPanel() With {
            .FlowDirection = FlowDirection.TopDown,
            .WrapContents = False,
            .AutoSize = True,
            .Location = New Point(Margin, sep.Bottom + 16),
            .Padding = New Padding(0)
        }
        uc.Controls.Add(flow)

        buildAction(flow)

        Return uc
    End Function

    Public Function AddTextBox(parent As FlowLayoutPanel, labelText As String,
                               currentValue As String) As TextBox
        Dim lbl As New Label() With {
            .Text = labelText,
            .Font = New Font("Segoe UI", 9.0F),
            .ForeColor = Color.FromArgb(LabelColor),
            .AutoSize = True,
            .Margin = New Padding(0, 8, 0, 2)
        }
        Dim tb As New TextBox() With {
            .Text = currentValue,
            .Font = New Font("Segoe UI", 10.0F),
            .Width = 320,
            .Margin = New Padding(0, 0, 0, ControlSpacing)
        }
        parent.Controls.Add(lbl)
        parent.Controls.Add(tb)
        Return tb
    End Function

    Public Function AddNumericUpDown(parent As FlowLayoutPanel, labelText As String,
                                     currentValue As Integer, min As Integer,
                                     max As Integer) As NumericUpDown
        Dim lbl As New Label() With {
            .Text = labelText,
            .Font = New Font("Segoe UI", 9.0F),
            .ForeColor = Color.FromArgb(LabelColor),
            .AutoSize = True,
            .Margin = New Padding(0, 8, 0, 2)
        }
        Dim nud As New NumericUpDown() With {
            .Minimum = min,
            .Maximum = max,
            .Value = Math.Max(min, Math.Min(max, currentValue)),
            .Font = New Font("Segoe UI", 10.0F),
            .Width = 120,
            .Margin = New Padding(0, 0, 0, ControlSpacing)
        }
        parent.Controls.Add(lbl)
        parent.Controls.Add(nud)
        Return nud
    End Function

    Public Function AddCheckBox(parent As FlowLayoutPanel, labelText As String,
                                currentValue As Boolean) As CheckBox
        Dim chk As New CheckBox() With {
            .Text = labelText,
            .Font = New Font("Segoe UI", 10.0F),
            .ForeColor = Color.FromArgb(LabelColor),
            .Checked = currentValue,
            .AutoSize = True,
            .Margin = New Padding(0, 8, 0, ControlSpacing)
        }
        parent.Controls.Add(chk)
        Return chk
    End Function

    Public Function AddComboBox(parent As FlowLayoutPanel, labelText As String,
                                items As String(), selectedIndex As Integer) As ComboBox
        Dim lbl As New Label() With {
            .Text = labelText,
            .Font = New Font("Segoe UI", 9.0F),
            .ForeColor = Color.FromArgb(LabelColor),
            .AutoSize = True,
            .Margin = New Padding(0, 8, 0, 2)
        }
        Dim cb As New ComboBox() With {
            .Font = New Font("Segoe UI", 10.0F),
            .Width = 220,
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .Margin = New Padding(0, 0, 0, ControlSpacing)
        }
        cb.Items.AddRange(items)
        cb.SelectedIndex = Math.Max(0, Math.Min(items.Length - 1, selectedIndex))
        parent.Controls.Add(lbl)
        parent.Controls.Add(cb)
        Return cb
    End Function

    Public Function AddButton(parent As FlowLayoutPanel, labelText As String) As Button
        Dim btn As New Button() With {
            .Text = labelText,
            .Font = New Font("Segoe UI", 9.0F),
            .AutoSize = True,
            .Margin = New Padding(0, 4, 0, ControlSpacing),
            .Padding = New Padding(12, 4, 12, 4)
        }
        parent.Controls.Add(btn)
        Return btn
    End Function

    Public Function AddOutputLabel(parent As FlowLayoutPanel, initialText As String) As Label
        Dim lbl As New Label() With {
            .Text = initialText,
            .Font = New Font("Segoe UI", 9.0F, FontStyle.Italic),
            .ForeColor = Color.FromArgb(0, 100, 180),
            .AutoSize = False,
            .Width = 580,
            .Height = 56,
            .Margin = New Padding(0, 8, 0, ControlSpacing)
        }
        parent.Controls.Add(lbl)
        Return lbl
    End Function

    Public Sub AddNote(parent As FlowLayoutPanel, text As String)
        Dim lbl As New Label() With {
            .Text = "ℹ " & text,
            .Font = New Font("Segoe UI", 9.0F, FontStyle.Italic),
            .ForeColor = Color.FromArgb(NoteColor),
            .AutoSize = False,
            .Width = 580,
            .Height = 30,
            .Margin = New Padding(0, 4, 0, 0)
        }
        parent.Controls.Add(lbl)
    End Sub

End Module