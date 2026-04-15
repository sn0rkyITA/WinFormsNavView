# NavigationView – Analisi Architetturale Completa e Piano di Evoluzione
Questo documento è un **file di contesto** pensato per accompagnare il refactoring di `NavigationView` e dei relativi componenti (`NavItem`, `NavViewRenderer`, `NavViewColors`, `NavViewFontResolver`).  
È scritto per essere **leggibile, tecnico e operativo**, in modo da poter guidare in modo affidabile la riscrittura o l’evoluzione del codice.
---
## 1. Ruolo del controllo e scenario d’uso
### 1.1. Scopo del `NavigationView`
Il controllo `NavigationView` implementa un **pannello laterale di navigazione** in stile “WinUI/Fluent” per WinForms, con:
- **Menu principale** (gerarchico, con gruppi, separatori, icone, notifiche)
- **Footer** (voci fisse in basso, con separatore strutturale)
- **Pane collassabile** (Left / LeftCompact)
- **Area contenuto** a destra, con header opzionale
- **Tema chiaro/scuro** tramite `NavViewTheme` e `NavViewColors`
- **Renderer pluggable** tramite `INavViewRenderer`
### 1.2. Scenari d’uso principali
Dalla struttura attuale e dalle tue esigenze emergono due scenari distinti:
1. **Internal Host**  
   Il `NavigationView` gestisce direttamente:
   - il contenuto (`SetContent(Control)`)
   - il layout dell’area contenuto
   - l’header del contenuto (`ContentHeader`)
2. **External Host / Menu Puro**  
   Il `NavigationView` viene usato solo come **menu laterale**:
   - l’applicazione (o un plugin) gestisce pannelli, form, user control
   - il NavView emette eventi di selezione
   - l’area contenuto interna non è necessaria o è indesiderata
Attualmente il codice è progettato principalmente per lo scenario 1, ma **lo scenario 2 è già possibile** (ignorando `SetContent` e usando solo `SelectionChanged`), anche se non è trattato come “cittadino di prima classe”.
---
## 2. Punti di forza dell’implementazione attuale
### 2.1. Layout e rendering
- **Separazione tra controllo e renderer**  
  - `NavigationView` calcola layout, hit testing, scroll, visibilità.
  - `INavViewRenderer` + `NavViewRenderer` si occupano del disegno GDI+.
- **Gestione avanzata del layout del pane**:
  - Header fisso con hamburger + AppTitle.
  - Footer fisso in basso, con separatore strutturale sempre presente.
  - Solo i menu item scrollano (viewport dedicata).
- **Scroll personalizzato**:
  - Scroll con rotella e scrollbar sottile (6px).
  - Calcolo di `_menuVirtualHeight`, `_menuViewportHeight`, `_scrollOffset`.
  - Clip region per evitare sconfinamenti su header/footer.
### 2.2. Modello dati del menu (`NavItem`)
- Supporto per:
  - separatori (`IsSeparator`)
  - group header (`IsGroupHeader`)
  - gerarchia (`Children`, `Parent`, `Depth`)
  - visibilità (`IsVisible`)
  - abilitazione (`IsEnabled`)
  - icone glyph (`IconGlyph`) o raster (`CustomIcon`)
  - dati associati (`Tag`)
  - notifiche (`HasNotification`)
  - callback inline (`ExecuteAction`)
  - tooltip e context menu
- `NavItemCollection`:
  - mantiene `Parent` coerente
  - espone `Flatten()` per iterare l’intero albero
### 2.3. Tema e icone
- `NavViewColors` centralizza la palette per tema chiaro/scuro.
- `FluentIcons` fornisce un dizionario di glyph Unicode coerente con Segoe Fluent Icons.
- `NavViewFontResolver` gestisce in modo robusto:
  - font di sistema (Segoe Fluent Icons)
  - font embedded
  - fallback su Segoe MDL2 Assets
---
## 3. Lacune strutturali e rischi tecnici
### 3.1. Ciclo di vita del contenuto
**Problema:**
`csharp'
public void SetContent(Control? control)
{
    if (_content != null)
    {
        Controls.Remove(_content);
        _content = null;
    }
    _content = control;
    if (_content != null)
    {
        _content.TabStop = true;
        Controls.Add(_content);
        RecalcLayout();
    }
}

Il controllo precedente viene rimosso dai Controls, ma: 
non viene chiamato Dispose()
non viene emesso alcun evento di “unload”
In scenari con: 
UserControl pesanti
GDI handle
Timer
Connessioni si rischiano memory leak garantiti.
Conseguenze:
Navigazione frequente → accumulo di risorse non rilasciate.
Difficoltà a implementare caching controllato (non c’è una policy esplicita).
### 3.2. Header del contenuto accoppiato alla selezione
In TogglePane, alla chiusura del pane:
if (_selectedItem != null)
{
    var parent = _selectedItem.Parent;
    while (parent != null && !parent.IsSelectable)
        parent = parent.Parent;
    if (parent != null)
    {
        var prev = _selectedItem;
        _selectedItem = parent;
        _contentHeader = parent.Label;
        SelectionChanged?.Invoke(this,
            new NavSelectionChangedEventArgs(_selectedItem, prev));
    }
}
Problemi:
_contentHeader viene sovrascritto in modo silenzioso con parent.Label.
Non esiste un modo per: 
avere header vuoto
avere header personalizzato non legato alla label
gestire header dinamici lato host
Conseguenza: 
L’header del contenuto è rigidamente accoppiato alla selezione, in modo non configurabile.
### 3.3. Assenza di routing/factory per il contenuto
Attualmente:
Navigate(NavItem item) chiama solo SelectItem(item).
SetContent deve essere chiamato dall’host manualmente.
Mancano:
un meccanismo dichiarativo per dire: 
“quando selezioni questo NavItem, crea/mostra questo Control”
un punto unico dove: 
risolvere il contenuto
applicare caching
gestire il ciclo di vita
Questo rende più difficile:
usare Tag come chiave di routing
centralizzare la logica di navigazione
testare il comportamento in modo isolato
###3.4. Layout rigido dell’area contenuto
private Rectangle ContentAreaBounds => new Rectangle(
    _currentPaneWidth, 0,
    Math.Max(0, Width - _currentPaneWidth), Height);
L’area contenuto è sempre presente, anche se non usata.
Non esiste una modalità in cui: 
il NavView sia solo pane/menu
l’area contenuto sia completamente disattivata
Questo è un limite per lo scenario “menu puro / external host”.
###3.5. Separatore strutturale e null! in RendererItemInfo.Source
In BuildVisibleItems:
_visibleItems.Add(new RendererItemInfo
{
    IsSeparator = true,
    Bounds = new Rectangle(
        0,
        footerStartY - NavViewMetrics.SeparatorHeight - 2,
        paneW,
        NavViewMetrics.SeparatorHeight),
    Source = null!,
    IsFooterItem = false // separatore strutturale, non scrollabile
});
Source = null! viola la semantica del tipo non-nullable.
Il renderer deve distinguere: 
separatori “logici” (derivati da NavItem.IsSeparator)
separatori “strutturali” (come quello sopra il footer)
L’uso di null! è un code smell e rende fragile il contratto tra controllo e renderer.
###3.6. Metriche e DPI
NavViewMetrics è una classe statica con valori fissi:
altezze
font size
padding
larghezze
Non è:
DPI-aware
configurabile a runtime
Su schermi ad alta densità o con scaling > 100%:
testi e icone possono risultare troppo piccoli o mal allineati
il layout non si adatta dinamicamente
##4. Scenario “menu puro / external host”
###4.1. È già possibile oggi?
Sì, a livello pratico:
puoi ignorare SetContent
puoi ignorare ContentHeader
puoi usare solo: 
SelectionChanged
Navigate(...)
SelectedItem
NavItem.Tag per routing custom
L’host può:
intercettare SelectionChanged
decidere cosa mostrare in un proprio Panel o altro container
###4.2. Cosa non è ottimale
L’area contenuto viene comunque calcolata e disegnata.
L’header del contenuto può comparire se ContentHeader non è vuoto.
Non esiste una proprietà che dichiari esplicitamente: 
“questo NavView è solo un menu, non gestisce contenuto interno”.
###4.3. Obiettivo
Rendere lo scenario “menu puro”:
esplicito
supportato ufficialmente
pulito a livello di API
senza area contenuto interna
##5. Direzioni di refactoring proposte
###5.1. Modalità di contenuto: NavViewContentMode
Introdurre un enum:
public enum NavViewContentMode
{
    InternalHost, // come oggi: il NavView gestisce l’area contenuto
    ExternalHost  // il NavView è solo menu; nessuna area contenuto interna
}
Con proprietà:
public NavViewContentMode ContentMode { get; set; }
Effetti:
In ExternalHost: 
ContentAreaBounds → Rectangle.Empty
OnPaint non disegna l’area contenuto né l’header
SetContent è ignorato o vietato
il pane occupa tutta la larghezza del controllo
###5.2. Ciclo di vita del contenuto e caching
Introdurre una policy esplicita:
public enum ContentLifecyclePolicy
{
    Default,        // Dispose del contenuto quando viene sostituito
    KeepAlive,      // Non dispose, pensato per caching
    ExternalCleanup // L’host gestisce il ciclo di vita
}
Con proprietà:
public ContentLifecyclePolicy ContentLifecyclePolicy { get; set; }
public Control? CurrentContent { get; }
public void ClearContent();
E eventi:
public event EventHandler<NavItem>? ContentRequested;
public event EventHandler<Control>? ViewLoaded;
public event EventHandler<Control>? ViewUnloaded;
Obiettivo:
evitare leak
permettere caching controllato
dare all’host visibilità sul ciclo di vita delle view
###5.3. Header del contenuto disaccoppiato
Introdurre:
public bool AutoContentHeader { get; set; } // default: false
public bool ShowContentHeader { get; set; } // default: true
Comportamento:
Se AutoContentHeader = true: 
all’atto della selezione, ContentHeader = SelectedItem.Label
Se AutoContentHeader = false: 
il controllo non modifica mai ContentHeader da solo
ShowContentHeader controlla se l’header viene disegnato e considerato nel layout.
Questo elimina l’override silenzioso di _contentHeader in TogglePane.
###5.4. Routing e factory per il contenuto
Introdurre un delegato:
public Func<NavItem, Control?>? ContentFactory { get; set; }
Usato in Navigate(NavItem item) (solo in InternalHost):
ContentRequested?.Invoke(this, item);
se ContentFactory != null: 
var control = ContentFactory(item);
SetContent(control);
Questo permette:
routing basato su: 
NavItem.Tag
NavItem.Id
tipi specifici
caching opzionale (in combinazione con ContentLifecyclePolicy)
###5.5. Layout più flessibile
Estendere il layout del contenuto con:
Padding ContentPadding
eventuale ContentFillMode (Fill / Dock / None)
In ExternalHost:
il layout del contenuto interno è disattivato
il controllo si comporta come un puro pane/menu
###5.6. Renderer e separatori
Per eliminare null! e chiarire i ruoli, introdurre:
NavItem? Source { get; set; } (nullable)
un enum:
public enum RendererItemKind
{
    MenuItem,
    FooterItem,
    Separator,
    StructuralSeparator,
    GroupHeader
}
Oppure almeno un flag esplicito per il separatore strutturale.
Il renderer può così:
distinguere separatori logici da quelli strutturali
evitare hack basati su Source == null!
###5.7. DPI-awareness e metriche
Evolvere NavViewMetrics da:
classe statica con costanti fisse
a:
oggetto configurabile, eventualmente con: 
factory DPI-aware
override per personalizzazioni
Esempio di direzione:
INavViewMetricsProvider o simile
ricalcolo delle metriche al cambio DPI
ricostruzione dei font in NavViewRenderer quando necessario
##6. Impatto sugli scenari d’uso
###6.1. InternalHost (come oggi, ma più robusto)
ContentMode = InternalHost
ContentLifecyclePolicy configurabile
ContentFactory per routing dichiarativo
AutoContentHeader opzionale
ViewLoaded / ViewUnloaded per gestire risorse
###6.2. ExternalHost (menu puro)
ContentMode = ExternalHost
ShowContentHeader = false
SetContent ignorato
l’host usa: 
SelectionChanged
ContentRequested (se utile)
il NavView non occupa spazio per l’area contenuto interna
##7. Conclusione
L’implementazione attuale di NavigationView è già solida e ben strutturata su molti aspetti (layout, renderer, modello dati, tema, icone), ma presenta alcune lacune architetturali che emergono soprattutto quando:
la navigazione è frequente e le view sono pesanti
si vuole usare il controllo come menu puro
si desidera un comportamento più dichiarativo e testabile
Le direttrici di evoluzione proposte:
Modalità di contenuto (InternalHost / ExternalHost)
Ciclo di vita esplicito del contenuto (policy + eventi)
Header disaccoppiato e configurabile
Routing/factory per il contenuto
Layout più flessibile
Contratto renderer più pulito (no null!)
Metriche DPI-aware e configurabili
costituiscono una base coerente per un refactoring che:
non stravolge il modello mentale del controllo
rende chiari i contratti tra host, controllo e renderer
prepara il terreno per scenari più complessi (plugin, caching, test, high DPI)
Questo file può essere usato come documento di riferimento per:
pianificare il refactoring
discutere le scelte architetturali
guidare la riscrittura del codice in modo controllato