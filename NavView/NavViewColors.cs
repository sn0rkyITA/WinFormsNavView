// NavViewColors.cs
// Contiene: NavViewColors (palette light/dark), FluentIcons (dizionario glyph)

namespace NavView
{
    // -------------------------------------------------------------------------
    // NavViewColors
    // -------------------------------------------------------------------------

    /// <summary>
    /// Palette colori del NavigationView per tema chiaro e scuro.
    /// Tutti i colori sono centralizzati qui: per personalizzare il tema
    /// è sufficiente modificare questa classe o derivarla.
    /// </summary>
    public class NavViewColors
    {
        // --- Pane (sidebar) --------------------------------------------------

        /// <summary>Sfondo del pannello laterale.</summary>
        public Color PaneBackground { get; set; }

        /// <summary>Bordo destro del pannello laterale.</summary>
        public Color PaneBorder { get; set; }

        // --- Voci di menu ----------------------------------------------------

        /// <summary>Testo delle voci normali.</summary>
        public Color ItemForeground { get; set; }

        /// <summary>Sfondo hover su una voce.</summary>
        public Color ItemHoverBackground { get; set; }

        /// <summary>Sfondo della voce selezionata.</summary>
        public Color ItemSelectedBackground { get; set; }

        /// <summary>Testo della voce selezionata.</summary>
        public Color ItemSelectedForeground { get; set; }

        /// <summary>Accento laterale (barra verticale sinistra sulla voce selezionata).</summary>
        public Color ItemSelectedAccent { get; set; }

        /// <summary>Testo delle voci disabilitate.</summary>
        public Color ItemDisabledForeground { get; set; }

        /// <summary>Colore della linea separatrice.</summary>
        public Color SeparatorColor { get; set; }

        /// <summary>Testo dei group header.</summary>
        public Color GroupHeaderForeground { get; set; }

        // --- Icone -----------------------------------------------------------

        /// <summary>Colore icona voce normale.</summary>
        public Color IconForeground { get; set; }

        /// <summary>Colore icona voce selezionata.</summary>
        public Color IconSelectedForeground { get; set; }

        /// <summary>Colore icona voce disabilitata.</summary>
        public Color IconDisabledForeground { get; set; }

        // --- Header del pane (cima sidebar) ----------------------------------

        /// <summary>Sfondo dell'area header in cima al pane.</summary>
        public Color PaneHeaderBackground { get; set; }

        /// <summary>Testo AppTitle nell'header del pane.</summary>
        public Color PaneHeaderForeground { get; set; }

        /// <summary>Colore del bottone hamburger (≡).</summary>
        public Color HamburgerForeground { get; set; }

        /// <summary>Sfondo hover del bottone hamburger.</summary>
        public Color HamburgerHoverBackground { get; set; }

        // --- Content area ----------------------------------------------------

        /// <summary>Sfondo dell'area contenuto.</summary>
        public Color ContentBackground { get; set; }

        /// <summary>Sfondo dell'area header del contenuto.</summary>
        public Color ContentHeaderBackground { get; set; }

        /// <summary>Testo dell'header del contenuto (titolo sezione).</summary>
        public Color ContentHeaderForeground { get; set; }

        // --- Freccia espansione figli ----------------------------------------

        /// <summary>Colore della freccia ▸ / ▾ sui nodi con figli.</summary>
        public Color ChevronForeground { get; set; }

        // -------------------------------------------------------------------------
        // Factory: tema chiaro
        // -------------------------------------------------------------------------

        /// <summary>
        /// Restituisce la palette predefinita tema chiaro (stile Windows 11 Light).
        /// </summary>
        public static NavViewColors Light() => new NavViewColors
        {
            // Pane
            PaneBackground = Color.FromArgb(243, 243, 243),
            PaneBorder = Color.FromArgb(220, 220, 220),

            // Voci
            ItemForeground = Color.FromArgb(26, 26, 26),
            ItemHoverBackground = Color.FromArgb(218, 218, 218),
            ItemSelectedBackground = Color.FromArgb(205, 228, 255),
            ItemSelectedForeground = Color.FromArgb(0, 95, 184),
            ItemSelectedAccent = Color.FromArgb(0, 95, 184),
            ItemDisabledForeground = Color.FromArgb(160, 160, 160),
            SeparatorColor = Color.FromArgb(210, 210, 210),
            GroupHeaderForeground = Color.FromArgb(100, 100, 100),

            // Icone
            IconForeground = Color.FromArgb(50, 50, 50),
            IconSelectedForeground = Color.FromArgb(0, 95, 184),
            IconDisabledForeground = Color.FromArgb(180, 180, 180),

            // Header pane
            PaneHeaderBackground = Color.FromArgb(243, 243, 243),
            PaneHeaderForeground = Color.FromArgb(26, 26, 26),
            HamburgerForeground = Color.FromArgb(50, 50, 50),
            HamburgerHoverBackground = Color.FromArgb(218, 218, 218),

            // Content
            ContentBackground = Color.FromArgb(255, 255, 255),
            ContentHeaderBackground = Color.FromArgb(255, 255, 255),
            ContentHeaderForeground = Color.FromArgb(26, 26, 26),

            // Chevron
            ChevronForeground = Color.FromArgb(100, 100, 100),
        };

        // -------------------------------------------------------------------------
        // Factory: tema scuro
        // -------------------------------------------------------------------------

        /// <summary>
        /// Restituisce la palette predefinita tema scuro (stile Windows 11 Dark).
        /// </summary>
        public static NavViewColors Dark() => new NavViewColors
        {
            // Pane
            PaneBackground = Color.FromArgb(32, 32, 32),
            PaneBorder = Color.FromArgb(55, 55, 55),

            // Voci
            ItemForeground = Color.FromArgb(255, 255, 255),
            ItemHoverBackground = Color.FromArgb(55, 55, 55),
            ItemSelectedBackground = Color.FromArgb(0, 70, 140),
            ItemSelectedForeground = Color.FromArgb(255, 255, 255),
            ItemSelectedAccent = Color.FromArgb(76, 194, 255),
            ItemDisabledForeground = Color.FromArgb(100, 100, 100),
            SeparatorColor = Color.FromArgb(60, 60, 60),
            GroupHeaderForeground = Color.FromArgb(160, 160, 160),

            // Icone
            IconForeground = Color.FromArgb(210, 210, 210),
            IconSelectedForeground = Color.FromArgb(76, 194, 255),
            IconDisabledForeground = Color.FromArgb(90, 90, 90),

            // Header pane
            PaneHeaderBackground = Color.FromArgb(32, 32, 32),
            PaneHeaderForeground = Color.FromArgb(255, 255, 255),
            HamburgerForeground = Color.FromArgb(210, 210, 210),
            HamburgerHoverBackground = Color.FromArgb(55, 55, 55),

            // Content
            ContentBackground = Color.FromArgb(25, 25, 25),
            ContentHeaderBackground = Color.FromArgb(25, 25, 25),
            ContentHeaderForeground = Color.FromArgb(255, 255, 255),

            // Chevron
            ChevronForeground = Color.FromArgb(160, 160, 160),
        };
    }

    // -------------------------------------------------------------------------
    // FluentIcons
    // -------------------------------------------------------------------------

    /// <summary>
    /// Dizionario dei glyph Unicode Segoe Fluent Icons più comuni.
    /// Uso: FluentIcons.Get("Home")  →  "\uE80F"
    /// oppure direttamente: FluentIcons.Home
    ///
    /// Font richiesto: "Segoe Fluent Icons" (incluso in Windows 11).
    /// Per Windows 10: distribuire il font come risorsa embedded.
    /// </summary>
    public static class FluentIcons
    {
        // -------------------------------------------------------------------------
        // Navigazione e UI generica
        // -------------------------------------------------------------------------
        public const string Home = "\uE80F";
        public const string Menu = "\uE700"; // hamburger ≡
        public const string Back = "\uE72B";
        public const string Forward = "\uE72A";
        public const string Up = "\uE74A";
        public const string Down = "\uE74B";
        public const string ChevronRight = "\uE76C";
        public const string ChevronLeft = "\uE76B";
        public const string ChevronDown = "\uE70D";
        public const string ChevronUp = "\uE70E";
        public const string Close = "\uE711";
        public const string Settings = "\uE713";
        public const string Search = "\uE721";
        public const string Filter = "\uE71C";
        public const string Sort = "\uE8CB";

        // -------------------------------------------------------------------------
        // Persone e identità
        // -------------------------------------------------------------------------
        public const string Person = "\uE77B";
        public const string People = "\uE716";
        public const string Contact = "\uE8AA";
        public const string ContactCard = "\uE8C7";

        // -------------------------------------------------------------------------
        // File e documenti
        // -------------------------------------------------------------------------
        public const string Document = "\uE8A5";
        public const string DocumentAdd = "\uE8A5"; // alias
        public const string Folder = "\uE8B7";
        public const string FolderOpen = "\uE838";
        public const string Save = "\uE74E";
        public const string SaveAs = "\uE792";
        public const string Delete = "\uE74D";
        public const string Copy = "\uE8C8";
        public const string Paste = "\uE77F";
        public const string Cut = "\uE8C6";
        public const string Attach = "\uE723";
        public const string Download = "\uE896";
        public const string Upload = "\uE898";
        public const string Print = "\uE749";
        public const string PDF = "\uEA90";

        // -------------------------------------------------------------------------
        // Azioni comuni
        // -------------------------------------------------------------------------
        public const string Add = "\uE710";
        public const string Remove = "\uE738";
        public const string Edit = "\uE70F";
        public const string Rename = "\uE8AC";
        public const string Refresh = "\uE72C";
        public const string Sync = "\uE895";
        public const string Undo = "\uE7A7";
        public const string Redo = "\uE7A6";
        public const string Share = "\uE72D";
        public const string Link = "\uE71B";
        public const string Pin = "\uE718";
        public const string Unpin = "\uE77A";
        public const string Star = "\uE734";
        public const string StarFull = "\uE735";
        public const string Like = "\uE8E1";
        public const string Flag = "\uE7C1";
        public const string Tag = "\uE8EC";
        public const string Lock = "\uE72E";
        public const string Unlock = "\uE785";
        public const string Visibility = "\uE7B3";
        public const string VisibilityOff = "\uED1A";

        // -------------------------------------------------------------------------
        // Stato e notifiche
        // -------------------------------------------------------------------------
        public const string Info = "\uE946";
        public const string Warning = "\uE7BA";
        public const string Error = "\uEA39";
        public const string Checkmark = "\uE73E";
        public const string CheckboxChecked = "\uE8FB";
        public const string Dismiss = "\uE711";
        public const string Help = "\uE897";
        public const string Feedback = "\uED15";

        // -------------------------------------------------------------------------
        // Dashboard e dati
        // -------------------------------------------------------------------------
        public const string Dashboard = "\uF246";
        public const string Chart = "\uE9D2";
        public const string BarChart = "\uE9F9";
        public const string List = "\uE8FD";
        public const string Table = "\uE9D2";
        public const string Database = "\uF196";
        public const string Report = "\uE9F9";
        public const string Statistics = "\uE9D9";

        // -------------------------------------------------------------------------
        // Mappa e geolocalizzazione (utile per Geodron/Studio Tecnico)
        // -------------------------------------------------------------------------
        public const string Map = "\uE707";
        public const string Location = "\uE81D";
        public const string Globe = "\uE774";
        public const string Compass = "\uE803";
        public const string Layers = "\uE81E";
        public const string Zoom = "\uE71E";
        public const string ZoomIn = "\uE8A3";
        public const string ZoomOut = "\uE71F";
        public const string Measure = "\uE9BC"; // righello

        // -------------------------------------------------------------------------
        // Comunicazione
        // -------------------------------------------------------------------------
        public const string Mail = "\uE715";
        public const string MailUnread = "\uE8C8";
        public const string Chat = "\uE8BD";
        public const string Phone = "\uE717";
        public const string Calendar = "\uE787";
        public const string Alarm = "\uE7E8";
        public const string Clock = "\uE823";

        // -------------------------------------------------------------------------
        // Sistema e app
        // -------------------------------------------------------------------------
        public const string Apps = "\uE71A";
        public const string AppFolder = "\uE8D5";
        public const string Cloud = "\uE753";
        public const string CloudUpload = "\uEC8E";
        public const string CloudDownload = "\uEBD3";
        public const string Wifi = "\uE701";
        public const string Bluetooth = "\uE702";
        public const string Battery = "\uE83F";
        public const string Camera = "\uE722";
        public const string Image = "\uEB9F";
        public const string Video = "\uE8B2";
        public const string Music = "\uEC4F";
        public const string Microphone = "\uE720";

        // -------------------------------------------------------------------------
        // Lookup per nome stringa
        // -------------------------------------------------------------------------

        private static readonly Dictionary<string, string> _lookup =
            new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase)
        {
            { "Home",           Home },
            { "Menu",           Menu },
            { "Back",           Back },
            { "Forward",        Forward },
            { "ChevronRight",   ChevronRight },
            { "ChevronLeft",    ChevronLeft },
            { "ChevronDown",    ChevronDown },
            { "ChevronUp",      ChevronUp },
            { "Close",          Close },
            { "Settings",       Settings },
            { "Search",         Search },
            { "Filter",         Filter },
            { "Sort",           Sort },
            { "Person",         Person },
            { "People",         People },
            { "Contact",        Contact },
            { "ContactCard",    ContactCard },
            { "Document",       Document },
            { "Folder",         Folder },
            { "FolderOpen",     FolderOpen },
            { "Save",           Save },
            { "Delete",         Delete },
            { "Copy",           Copy },
            { "Add",            Add },
            { "Remove",         Remove },
            { "Edit",           Edit },
            { "Refresh",        Refresh },
            { "Share",          Share },
            { "Star",           Star },
            { "Lock",           Lock },
            { "Info",           Info },
            { "Warning",        Warning },
            { "Error",          Error },
            { "Checkmark",      Checkmark },
            { "Help",           Help },
            { "Dashboard",      Dashboard },
            { "Chart",          Chart },
            { "List",           List },
            { "Database",       Database },
            { "Report",         Report },
            { "Map",            Map },
            { "Location",       Location },
            { "Globe",          Globe },
            { "Compass",        Compass },
            { "Layers",         Layers },
            { "Zoom",           Zoom },
            { "ZoomIn",         ZoomIn },
            { "ZoomOut",        ZoomOut },
            { "Measure",        Measure },
            { "Mail",           Mail },
            { "Chat",           Chat },
            { "Phone",          Phone },
            { "Calendar",       Calendar },
            { "Clock",          Clock },
            { "Apps",           Apps },
            { "Cloud",          Cloud },
            { "Camera",         Camera },
            { "Image",          Image },
        };

        /// <summary>
        /// Restituisce il glyph Unicode dal nome (case-insensitive).
        /// Restituisce stringa vuota se il nome non esiste.
        /// </summary>
        public static string Get(string name)
            => _lookup.TryGetValue(name, out var glyph) ? glyph : string.Empty;

        /// <summary>
        /// True se il nome esiste nel dizionario.
        /// </summary>
        public static bool Contains(string name)
            => _lookup.ContainsKey(name);

        /// <summary>
        /// Restituisce tutti i nomi disponibili.
        /// </summary>
        public static IEnumerable<string> AllNames()
            => _lookup.Keys;
    }
}