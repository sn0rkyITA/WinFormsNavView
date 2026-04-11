# 🧭 NavigationView per WinForms (WinFormsNavView)
WinUI3-style NavigationView control for WinForms (.NET 10)

![.NET](https://img.shields.io/badge/.NET-10%2B-blue)
![License](https://img.shields.io/badge/license-MIT-green)
![WinForms](https://img.shields.io/badge/UI-WinForms-orange)

Un controllo di navigazione laterale moderno, ispirato a **Fluent Design / WinUI 3**, scritto interamente in C# per **Windows Forms**.  
Offre un'esperienza utente pulita, prestazioni elevate e un'architettura modulare pronta per l'estensione.

---

## 📑 Indice

- [✨ Funzionalità](#-funzionalità)
- [🖥️ Anteprima](#️-anteprima)
- [📦 Requisiti](#-requisiti)
- [🚀 Installazione & Utilizzo](#-installazione--utilizzo)
- [🏗️ Architettura](#️-architettura)
- [🎨 Personalizzazione](#-personalizzazione)
- [🛠️ Note Tecniche](#️-note-tecniche)
- [🤝 Contribuire](#-contribuire)
- [📄 Licenza](#-licenza)
- [Changelog](#-changelog)

---

## ✨ Funzionalità

- 🎨 **Tema Chiaro/Scuro** integrato, con palette colori centralizzata e modificabile
- 📐 **Modalità di visualizzazione**: `Left` (fisso) e `LeftCompact` (solo icone, si espande dinamicamente)
- 🌳 **Menu gerarchico** con supporto accordion (espansione/collasso ricorsivo)
- 📌 **Footer ancorato** stabilmente in basso, indipendente dal numero di voci
- 🖼️ **Area contenuto neutrale** con ridimensionamento automatico e header opzionale
- ⚡ **Rendering ottimizzato**: doppio buffer GDI+, hit-testing preciso, zero flickering
- 🔌 **Architettura estensibile**: separazione netta tra layout, rendering e modello dati (`INavViewRenderer`)

---

## 🖥️ Anteprima

![Preview](preview.png)

---

## 📦 Requisiti

- **.NET 10**
- **Windows** (WinForms)
- Font: `Segoe Fluent Icons`

---

## 🚀 Installazione & Utilizzo

1. Clona la repository o scarica i file `.cs`
2. Aggiungi il progetto `NavView` alla tua soluzione
3. Compila e usa il controllo

### Esempio Rapido

```csharp
var nav = new NavigationView
{
    AppTitle = "La Mia App",
    Theme = NavViewTheme.Light,
    PaneDisplayMode = PaneDisplayMode.LeftCompact
};

nav.MenuItems.Add(new NavItem { Label = "Dashboard", IconGlyph = FluentIcons.Dashboard });

this.Controls.Add(nav);
nav.Dock = DockStyle.Fill;
```

---

## 🏗️ Architettura

| File | Responsabilità |
|------|----------------|
| `NavigationView.cs` | Controller principale |
| `NavViewRenderer.cs` | Rendering |
| `NavViewColors.cs` | Temi |
| `NavItem.cs` | Modello dati |

---

## 🎨 Personalizzazione

```csharp
var myColors = NavViewColors.Dark();
myColors.ItemSelectedAccent = Color.Gold;
nav.Renderer.Colors = myColors;
```

---

## 🛠️ Note Tecniche

- Performance ottimizzate
- Rendering fluido
- Layout modulare

---

## 🤝 Contribuire

1. Fork
2. Branch
3. Commit
4. Push
5. Pull Request

---

## 📄 Licenza

MIT License

---

💡 Realizzato per la community WinForms

## Changelog

### [0.2.0] - 2026-04-11

#### Aggiunto
- **Scroll custom** area menu con scrollbar visuale proporzionale (6px),
  rotella mouse, clip region. Header e footer rimangono fissi.
- **NavItem.IsVisible** — nasconde una voce senza rimuoverla dalla collezione.
- **NavItem.HasNotification** — pallino colorato sovrapposto all'icona.
- **NavItem.CustomIcon** — immagine raster in sostituzione del glyph.
- **NavItem.DeselectOnClick** — deseleziona la voce già selezionata al click.
- **NavItem.ExecuteAction** — callback inline eseguita alla selezione.
- **NavItem.ContextMenuStrip** — menu contestuale al tasto destro sulla voce.
- **NavItem.ToolTipText** — tooltip nativo WinForms al passaggio del mouse.
- **NavItem.SortOrder** — peso per ordinamento gestito dall'host.
- **NavViewFontResolver** — risoluzione font icone a tre livelli:
  Segoe Fluent Icons di sistema (Win11), embedded nella DLL (Win10),
  Segoe MDL2 Assets come fallback finale.
- **NavViewColors.NotificationDotColor** — colore dot badge in tema chiaro e scuro.
- **FluentIcons** — aggiunte categorie pratiche/workflow, immobili/catasto,
  finanza/computo, scadenze/tempo, utility UI.

#### Corretto
- Glyph errati nel dizionario FluentIcons: `Error`, `Clock`, `Compass`,
  `Visibility`, `Apps`, `Alarm`, `Image`.
- Duplicati risolti: `Chart`/`Table`, `Report`/`BarChart`, `Copy`/`MailUnread`.
- `DocumentAdd` rimosso (identico a `Document`).
- Footer: separatore strutturale sempre presente, voci mai tagliate dal layout.
- Hover su item disabilitato non disegna più lo sfondo hover.
- Hamburger hover usa correttamente `HamburgerHoverBackground`.
- Cast forzato `(NavViewRenderer)` nel setter `Renderer` rimosso.
- Doppio click non genera più azioni indesiderate sul menu.
- Comportamento `LeftCompact`: il pane si richiude dopo selezione figlio
  solo se era stato aperto da voce, non da hamburger.
- Selezione dopo chiusura accordion ripristina il padre come voce evidenziata.
