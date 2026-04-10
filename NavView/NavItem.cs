// NavItem.cs
// Contiene: NavItem, NavItemCollection, PaneDisplayMode, NavViewTheme,
//           NavSelectionChangedEventArgs

using System.Collections.ObjectModel;
using System.ComponentModel;

namespace NavView
{
    // -------------------------------------------------------------------------
    // Enums
    // -------------------------------------------------------------------------

    /// <summary>
    /// Modalità di visualizzazione del pannello laterale.
    /// Left     = sempre espanso, larghezza piena.
    /// LeftCompact = icone sole; si allarga quando un padre con figli è espanso.
    /// </summary>
    public enum PaneDisplayMode
    {
        Left,
        LeftCompact
    }

    /// <summary>
    /// Tema visivo del controllo.
    /// </summary>
    public enum NavViewTheme
    {
        Light,
        Dark
    }

    // -------------------------------------------------------------------------
    // NavItem
    // -------------------------------------------------------------------------

    /// <summary>
    /// Rappresenta una singola voce nel pannello di navigazione.
    /// </summary>
    public class NavItem
    {
        // --- Identità --------------------------------------------------------

        /// <summary>
        /// Identificatore univoco. Se lasciato vuoto viene autogenerato.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// Testo visualizzato accanto all'icona.
        /// </summary>
        public string Label { get; set; } = string.Empty;

        /// <summary>
        /// Glyph Unicode Segoe Fluent Icons (es. "\uE80F" per Home).
        /// Usa FluentIcons.cs per i valori predefiniti.
        /// </summary>
        public string IconGlyph { get; set; } = string.Empty;

        /// <summary>
        /// Dati arbitrari associati alla voce (es. tipo UserControl da mostrare).
        /// Il NavigationView non lo usa: è a disposizione dell'host.
        /// </summary>
        public object? Tag { get; set; }

        // --- Stato -----------------------------------------------------------

        /// <summary>
        /// Se false la voce è visualizzata ma non selezionabile.
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Se true la voce è una linea separatrice orizzontale.
        /// Label e IconGlyph vengono ignorati.
        /// </summary>
        public bool IsSeparator { get; set; } = false;

        /// <summary>
        /// Se true la voce è un'intestazione di gruppo (testo, non cliccabile).
        /// </summary>
        public bool IsGroupHeader { get; set; } = false;

        /// <summary>
        /// Se true i figli sono visibili. Rilevante solo se Children.Count > 0.
        /// </summary>
        public bool IsExpanded { get; set; } = false;

        // --- Badge (predisposto, non renderizzato nella v1) ------------------

        /// <summary>
        /// Numero da mostrare come badge. 0 = nessun badge.
        /// Predisposto per versioni future; nella v1 non viene disegnato.
        /// </summary>
        public int BadgeCount { get; set; } = 0;

        // --- Gerarchia -------------------------------------------------------

        /// <summary>
        /// Voci figlie. Se presenti, la voce diventa un nodo espandibile.
        /// </summary>
        public List<NavItem> Children { get; set; } = new List<NavItem>();

        /// <summary>
        /// Riferimento al padre. Impostato automaticamente da NavItemCollection.
        /// Non impostare manualmente.
        /// </summary>
        [Browsable(false)]
        public NavItem? Parent { get; internal set; }

        // --- Helpers ---------------------------------------------------------

        /// <summary>
        /// True se la voce ha figli e non è separatore né group header.
        /// </summary>
        public bool HasChildren => Children.Count > 0 && !IsSeparator && !IsGroupHeader;

        /// <summary>
        /// True se la voce è selezionabile (non separatore, non group header, abilitata).
        /// </summary>
        public bool IsSelectable => !IsSeparator && !IsGroupHeader && IsEnabled;

        /// <summary>
        /// Profondità nella gerarchia. 0 = radice.
        /// </summary>
        public int Depth => Parent == null ? 0 : Parent.Depth + 1;

        // --- Factory methods -------------------------------------------------

        /// <summary>
        /// Crea una voce di navigazione standard.
        /// </summary>
        public static NavItem Create(string label, string iconGlyph, object? tag = null)
            => new NavItem { Label = label, IconGlyph = iconGlyph, Tag = tag };

        /// <summary>
        /// Crea una linea separatrice.
        /// </summary>
        public static NavItem Separator()
            => new NavItem { IsSeparator = true };

        /// <summary>
        /// Crea un'intestazione di gruppo (testo non cliccabile).
        /// </summary>
        public static NavItem GroupHeader(string label)
            => new NavItem { Label = label, IsGroupHeader = true };

        public override string ToString() => Label;
    }

    // -------------------------------------------------------------------------
    // NavItemCollection
    // -------------------------------------------------------------------------

    /// <summary>
    /// Collezione osservabile di NavItem.
    /// Imposta automaticamente il Parent sui figli aggiunti.
    /// </summary>
    public class NavItemCollection : Collection<NavItem>
    {
        /// <summary>
        /// Sollevato quando la collezione cambia (aggiunta, rimozione, reset).
        /// </summary>
        public event EventHandler? CollectionChanged;

        private readonly NavItem? _owner;

        public NavItemCollection() : this(null) { }

        internal NavItemCollection(NavItem? owner)
        {
            _owner = owner;
        }

        protected override void InsertItem(int index, NavItem item)
        {
            ArgumentNullException.ThrowIfNull(item);
            item.Parent = _owner;

            // Propaga il parent sui figli già presenti nell'item
            foreach (var child in item.Children)
                child.Parent = item;

            base.InsertItem(index, item);
            CollectionChanged?.Invoke(this, EventArgs.Empty);
        }

        protected override void RemoveItem(int index)
        {
            this[index].Parent = null;
            base.RemoveItem(index);
            CollectionChanged?.Invoke(this, EventArgs.Empty);
        }

        protected override void SetItem(int index, NavItem item)
        {
            ArgumentNullException.ThrowIfNull(item);
            this[index].Parent = null;
            item.Parent = _owner;
            base.SetItem(index, item);
            CollectionChanged?.Invoke(this, EventArgs.Empty);
        }

        protected override void ClearItems()
        {
            foreach (var item in this)
                item.Parent = null;
            base.ClearItems();
            CollectionChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Restituisce tutti gli item in modo piatto (ricorsivo su Children).
        /// Utile per cercare per Id o per iterare tutto l'albero.
        /// </summary>
        public IEnumerable<NavItem> Flatten()
        {
            foreach (var item in this)
            {
                yield return item;
                foreach (var child in FlattenItem(item))
                    yield return child;
            }
        }

        private static IEnumerable<NavItem> FlattenItem(NavItem item)
        {
            foreach (var child in item.Children)
            {
                yield return child;
                foreach (var grandchild in FlattenItem(child))
                    yield return grandchild;
            }
        }
    }

    // -------------------------------------------------------------------------
    // NavSelectionChangedEventArgs
    // -------------------------------------------------------------------------

    /// <summary>
    /// Argomenti dell'evento SelectionChanged del NavigationView.
    /// </summary>
    public class NavSelectionChangedEventArgs : EventArgs
    {
        /// <summary>
        /// La voce appena selezionata.
        /// </summary>
        public NavItem Item { get; }

        /// <summary>
        /// La voce precedentemente selezionata. Null se nessuna.
        /// </summary>
        public NavItem? PreviousItem { get; }

        public NavSelectionChangedEventArgs(NavItem item, NavItem? previousItem)
        {
            Item = item;
            PreviousItem = previousItem;
        }
    }
}