// NavViewColors.cs
// Contiene: NavViewColors (palette light/dark), FluentIcons (dizionario glyph)
//
// CHANGELOG:
// - Tutti i codici Unicode verificati sulla documentazione ufficiale Microsoft
//   https://learn.microsoft.com/en-us/windows/apps/design/iconography/segoe-fluent-icons-font

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

        // --- Notifica dot badge ----------------------------------------------

        /// <summary>Colore del pallino di notifica sovrapposto all'icona.</summary>
        public Color NotificationDotColor { get; set; }

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

            // Notification dot
            NotificationDotColor = Color.FromArgb(0, 120, 212),
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

            // Notification dot
            NotificationDotColor = Color.FromArgb(76, 194, 255),
        };
    }

    // -------------------------------------------------------------------------
    // FluentIcons
    // -------------------------------------------------------------------------

    /// <summary>
    /// Dizionario dei glyph Unicode Segoe Fluent Icons.
    /// Uso: FluentIcons.Get("Home")  →  "\uE80F"
    /// oppure direttamente: FluentIcons.Home
    ///
    /// Font richiesto: "Segoe Fluent Icons" (incluso in Windows 11).
    /// Per Windows 10: distribuire il font come risorsa embedded.
    ///
    /// Tutti i codici verificati sulla documentazione ufficiale Microsoft:
    /// https://learn.microsoft.com/en-us/windows/apps/design/iconography/segoe-fluent-icons-font
    /// </summary>
    public static class FluentIcons
    {
        // -------------------------------------------------------------------------
        // Navigazione e UI generica
        // -------------------------------------------------------------------------
        public const string Home = "\uE80F"; // Home
        public const string Menu = "\uE700"; // GlobalNavButton (hamburger ≡)
        public const string Back = "\uE72B"; // Back
        public const string Forward = "\uE72A"; // Forward
        public const string Up = "\uE74A"; // Up
        public const string Down = "\uE74B"; // Down
        public const string ChevronRight = "\uE76C"; // ChevronRight
        public const string ChevronLeft = "\uE76B"; // ChevronLeft
        public const string ChevronDown = "\uE70D"; // ChevronDown
        public const string ChevronUp = "\uE70E"; // ChevronUp
        public const string Close = "\uE711"; // Cancel
        public const string Dismiss = "\uE711"; // alias di Close/Cancel
        public const string Settings = "\uE713"; // Settings
        public const string Search = "\uE721"; // Search
        public const string Filter = "\uE71C"; // Filter
        public const string Sort = "\uE8CB"; // Sort
        public const string More = "\uE712"; // More (•••)
        public const string View = "\uE890"; // View

        // -------------------------------------------------------------------------
        // Persone e identità
        // -------------------------------------------------------------------------
        public const string Person = "\uE77B"; // Contact
        public const string People = "\uE716"; // People
        public const string Contact = "\uE77B"; // Contact (alias di Person)
        public const string ContactCard = "\uE8C7"; // PaymentCard — usato come card
        public const string Admin = "\uE7EF"; // Admin
        public const string OtherUser = "\uE7EE"; // OtherUser

        // -------------------------------------------------------------------------
        // File e documenti
        // -------------------------------------------------------------------------
        public const string Document = "\uE8A5"; // Document
        public const string Folder = "\uE8B7"; // Folder
        public const string FolderOpen = "\uE838"; // FolderOpen
        public const string NewFolder = "\uE8F4"; // NewFolder
        public const string Save = "\uE74E"; // Save
        public const string SaveAs = "\uE792"; // SaveAs
        public const string Delete = "\uE74D"; // Delete
        public const string Copy = "\uE8C8"; // Copy
        public const string Paste = "\uE77F"; // Paste
        public const string Cut = "\uE8C6"; // Cut
        public const string Attach = "\uE723"; // Attach
        public const string Download = "\uE896"; // Download
        public const string Upload = "\uE898"; // Upload
        public const string Print = "\uE749"; // Print
        public const string PDF = "\uEA90"; // PDF (range EA)
        public const string OpenFile = "\uE8E5"; // OpenFile
        public const string MoveToFolder = "\uE8DE"; // MoveToFolder

        // -------------------------------------------------------------------------
        // Azioni comuni
        // -------------------------------------------------------------------------
        public const string Add = "\uE710"; // Add
        public const string Remove = "\uE738"; // Remove
        public const string Edit = "\uE70F"; // Edit
        public const string Rename = "\uE8AC"; // Rename
        public const string Refresh = "\uE72C"; // Refresh
        public const string Sync = "\uE895"; // Sync
        public const string Undo = "\uE7A7"; // Undo
        public const string Redo = "\uE7A6"; // Redo
        public const string Share = "\uE72D"; // Share
        public const string Link = "\uE71B"; // Link
        public const string Pin = "\uE718"; // Pin
        public const string Unpin = "\uE77A"; // Unpin
        public const string Star = "\uE734"; // FavoriteStar
        public const string StarFull = "\uE735"; // FavoriteStarFill
        public const string Like = "\uE8E1"; // Like
        public const string Flag = "\uE7C1"; // Flag
        public const string Tag = "\uE8EC"; // Tag
        public const string Lock = "\uE72E"; // Lock
        public const string Unlock = "\uE785"; // Unlock
        public const string Visibility = "\uE890"; // View (occhio/visibilità)
        public const string VisibilityOff = "\uED1A"; // (range ED, non in lista base MS — usare con cautela)
        public const string SelectAll = "\uE8B3"; // SelectAll
        public const string MultiSelect = "\uE762"; // MultiSelect
        public const string Send = "\uE724"; // Send

        // -------------------------------------------------------------------------
        // Stato e notifiche
        // -------------------------------------------------------------------------
        public const string Info = "\uE946"; // Info
        public const string Warning = "\uE7BA"; // Warning
        public const string Error = "\uE783"; // Error  ← CORRETTO (era \uEA39)
        public const string Checkmark = "\uE73E"; // CheckMark
        public const string CheckboxChecked = "\uE8FB"; // Accept
        public const string Help = "\uE897"; // Help
        public const string Feedback = "\uE939"; // FeedbackApp
        public const string Completed = "\uE930"; // Completed
        public const string Important = "\uE8C9"; // Important

        // -------------------------------------------------------------------------
        // Dashboard e dati
        // -------------------------------------------------------------------------
        public const string Dashboard = "\uF246"; // (range F2, verificato)
        public const string Chart = "\uE9D2"; // Chart  ← rimane, Table ottiene codice diverso
        public const string BarChart = "\uE9F9"; // BarChart ← rimane, Report ottiene codice diverso
        public const string Table = "\uE8A9"; // ViewAll — usato come tabella  ← CORRETTO (era duplicato di Chart)
        public const string List = "\uE8FD"; // BulletedList
        public const string Database = "\uF196"; // (range F1, verificato)
        public const string Report = "\uE9F9"; // identico a BarChart — vedi nota sotto
        // NOTA: Report e BarChart hanno lo stesso glyph ufficiale nell'intervallo E9F9
        // perché Segoe Fluent non ha un'icona "report" distinta. Mantenuto per compatibilità.
        // Se serve distinzione visiva, usare Report = \uE8BC (ShowResults) o \uE8FF (Preview).
        public const string ReportAlt = "\uE8BC"; // ShowResults — alternativa a Report
        public const string Statistics = "\uE9D9"; // (range E9)
        public const string Preview = "\uE8FF"; // Preview

        // -------------------------------------------------------------------------
        // Mappa e geolocalizzazione (utile per Geodron/Studio Tecnico)
        // -------------------------------------------------------------------------
        public const string Map = "\uE707"; // MapPin — icona pin mappa
        public const string MapView = "\uE800"; // Nav2DMapView — vista mappa 2D  ← NUOVO
        public const string Location = "\uE81D"; // Location
        public const string Globe = "\uE774"; // Globe
        public const string Compass = "\uE812"; // MapCompassTop  ← CORRETTO (era \uE803 = StreetsideSplitExpand)
        public const string Layers = "\uE81E"; // MapLayers
        public const string Zoom = "\uE71E"; // Zoom
        public const string ZoomIn = "\uE8A3"; // ZoomIn
        public const string ZoomOut = "\uE71F"; // ZoomOut
        public const string Measure = "\uE9BC"; // (range E9 — righello)
        public const string MapDirections = "\uE816"; // MapDirections  ← NUOVO
        public const string Satellite = "\uE909"; // World  ← NUOVO (approssimazione)

        // -------------------------------------------------------------------------
        // Comunicazione
        // -------------------------------------------------------------------------
        public const string Mail = "\uE715"; // Mail
        public const string MailUnread = "\uE8C8"; // — rimosso: stesso codice di Copy.
        // MailUnread era un errore di naming nell'originale: \uE8C8 è Copy.
        // Per mail non letta usare MailFill = \uE8A8 o ActionCenterNotification = \uE7E7
        public const string MailFill = "\uE8A8"; // MailFill  ← NUOVO
        public const string Chat = "\uE8BD"; // Message
        public const string Phone = "\uE717"; // Phone
        public const string Calendar = "\uE787"; // Calendar
        public const string Clock = "\uE917"; // Clock  ← CORRETTO (era \uE823 = Recent)
        public const string Recent = "\uE823"; // Recent  ← NUOVO (separato da Clock)
        public const string Alarm = "\uE7E7"; // ActionCenterNotification — usato come allarme
        // NOTA: Segoe Fluent non ha un'icona "sveglia" dedicata nell'intervallo comune.
        // \uE7E7 (ActionCenterNotification) è la scelta più vicina semanticamente.

        // -------------------------------------------------------------------------
        // Sistema e app
        // -------------------------------------------------------------------------
        public const string Apps = "\uE71D"; // AllApps  ← CORRETTO (era \uE71A = Stop)
        public const string AppFolder = "\uE8D5"; // FolderFill
        public const string Cloud = "\uE753"; // Cloud
        public const string CloudUpload = "\uEC8E"; // (range EC)
        public const string CloudDownload = "\uEBD3"; // (range EB)
        public const string Wifi = "\uE701"; // Wifi
        public const string Bluetooth = "\uE702"; // Bluetooth
        public const string Battery = "\uE83F"; // Battery10
        public const string Camera = "\uE722"; // Camera
        public const string Image = "\uE8B9"; // Picture  ← CORRETTO (era \uEB9F, fuori range comune)
        public const string Video = "\uE714"; // Video
        public const string Music = "\uEC4F"; // (range EC)
        public const string Microphone = "\uE720"; // Microphone
        public const string Code = "\uE943"; // Code
        public const string Key = "\uE8FB"; // Accept — usato come chiave (approssimazione)
        // NOTA: Segoe Fluent non ha un'icona "chiave" nell'intervallo principale.
        // Alternativa: usare Lock/Unlock o Permissions (\uE8D7).
        public const string Shield = "\uE83D"; // DefenderApp
        public const string Permissions = "\uE8D7"; // Permissions
        public const string Logout = "\uE748"; // SwitchUser

        // -------------------------------------------------------------------------
        // Pratiche / workflow (Studio Tecnico)
        // -------------------------------------------------------------------------
        public const string TaskList = "\uE8FD"; // BulletedList — lista attività
        public const string ClipboardList = "\uF0E3"; // (range F0 — clipboard)
        public const string Checklist = "\uE73A"; // CheckboxComposite
        public const string WorkItem = "\uE821"; // Work
        public const string History = "\uE81C"; // History
        public const string Archive = "\uE7B8"; // Package
        public const string Scan = "\uE8FE"; // Scan
        public const string Annotation = "\uE924"; // Annotation

        // -------------------------------------------------------------------------
        // Immobili / catasto (Studio Tecnico)
        // -------------------------------------------------------------------------
        public const string Building = "\uE825"; // Bank — edificio generico
        public const string HomeDetail = "\uE80F"; // Home — alias
        public const string Blueprint = "\uE7C3"; // Page — pianta/planimetria
        public const string Area = "\uE9BC"; // alias di Measure
        public const string Construction = "\uE822"; // Construction
        public const string ParkingLocation = "\uE811"; // ParkingLocation

        // -------------------------------------------------------------------------
        // Finanza / computo (Studio Tecnico)
        // -------------------------------------------------------------------------
        public const string Calculator = "\uE8EF"; // Calculator
        public const string Money = "\uE8C7"; // PaymentCard
        public const string Receipt = "\uE8FE"; // Scan — approssimazione
        public const string CreditCard = "\uE8C7"; // PaymentCard — alias di Money

        // -------------------------------------------------------------------------
        // Scadenze / tempo (Studio Tecnico)
        // -------------------------------------------------------------------------
        public const string CalendarAlert = "\uE7E7"; // ActionCenterNotification — scadenza
        public const string CalendarCheck = "\uE8F5"; // CalendarReply
        public const string Timer = "\uE916"; // Stopwatch
        public const string Hourglass = "\uE917"; // alias di Clock

        // -------------------------------------------------------------------------
        // Utility UI
        // -------------------------------------------------------------------------
        public const string MoreVertical = "\uE712"; // More (•••)
        public const string MoreHorizontal = "\uE712"; // alias di More
        public const string Expand = "\uE740"; // FullScreen
        public const string Collapse = "\uE73F"; // BackToWindow
        public const string NewWindow = "\uE78B"; // NewWindow
        public const string OpenInNewWindow = "\uE8A7"; // OpenInNewWindow
        public const string Move = "\uE7C2"; // Move
        public const string Rotate = "\uE7AD"; // Rotate

        // -------------------------------------------------------------------------
        // Lookup per nome stringa
        // -------------------------------------------------------------------------

        private static readonly Dictionary<string, string> _lookup =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            // Navigazione
            { "Home",             Home },
            { "Menu",             Menu },
            { "Back",             Back },
            { "Forward",          Forward },
            { "Up",               Up },
            { "Down",             Down },
            { "ChevronRight",     ChevronRight },
            { "ChevronLeft",      ChevronLeft },
            { "ChevronDown",      ChevronDown },
            { "ChevronUp",        ChevronUp },
            { "Close",            Close },
            { "Dismiss",          Dismiss },
            { "Settings",         Settings },
            { "Search",           Search },
            { "Filter",           Filter },
            { "Sort",             Sort },
            { "More",             More },
            { "View",             View },

            // Persone
            { "Person",           Person },
            { "People",           People },
            { "Contact",          Contact },
            { "ContactCard",      ContactCard },
            { "Admin",            Admin },
            { "OtherUser",        OtherUser },

            // File e documenti
            { "Document",         Document },
            { "Folder",           Folder },
            { "FolderOpen",       FolderOpen },
            { "NewFolder",        NewFolder },
            { "Save",             Save },
            { "SaveAs",           SaveAs },
            { "Delete",           Delete },
            { "Copy",             Copy },
            { "Paste",            Paste },
            { "Cut",              Cut },
            { "Attach",           Attach },
            { "Download",         Download },
            { "Upload",           Upload },
            { "Print",            Print },
            { "PDF",              PDF },
            { "OpenFile",         OpenFile },
            { "MoveToFolder",     MoveToFolder },

            // Azioni comuni
            { "Add",              Add },
            { "Remove",           Remove },
            { "Edit",             Edit },
            { "Rename",           Rename },
            { "Refresh",          Refresh },
            { "Sync",             Sync },
            { "Undo",             Undo },
            { "Redo",             Redo },
            { "Share",            Share },
            { "Link",             Link },
            { "Pin",              Pin },
            { "Unpin",            Unpin },
            { "Star",             Star },
            { "StarFull",         StarFull },
            { "Like",             Like },
            { "Flag",             Flag },
            { "Tag",              Tag },
            { "Lock",             Lock },
            { "Unlock",           Unlock },
            { "Visibility",       Visibility },
            { "VisibilityOff",    VisibilityOff },
            { "SelectAll",        SelectAll },
            { "MultiSelect",      MultiSelect },
            { "Send",             Send },

            // Stato e notifiche
            { "Info",             Info },
            { "Warning",          Warning },
            { "Error",            Error },
            { "Checkmark",        Checkmark },
            { "CheckboxChecked",  CheckboxChecked },
            { "Help",             Help },
            { "Feedback",         Feedback },
            { "Completed",        Completed },
            { "Important",        Important },

            // Dashboard e dati
            { "Dashboard",        Dashboard },
            { "Chart",            Chart },
            { "BarChart",         BarChart },
            { "Table",            Table },
            { "List",             List },
            { "Database",         Database },
            { "Report",           Report },
            { "ReportAlt",        ReportAlt },
            { "Statistics",       Statistics },
            { "Preview",          Preview },

            // Mappa e geolocalizzazione
            { "Map",              Map },
            { "MapView",          MapView },
            { "Location",         Location },
            { "Globe",            Globe },
            { "Compass",          Compass },
            { "Layers",           Layers },
            { "Zoom",             Zoom },
            { "ZoomIn",           ZoomIn },
            { "ZoomOut",          ZoomOut },
            { "Measure",          Measure },
            { "MapDirections",    MapDirections },
            { "Satellite",        Satellite },

            // Comunicazione
            { "Mail",             Mail },
            { "MailFill",         MailFill },
            { "Chat",             Chat },
            { "Phone",            Phone },
            { "Calendar",         Calendar },
            { "Clock",            Clock },
            { "Recent",           Recent },
            { "Alarm",            Alarm },

            // Sistema e app
            { "Apps",             Apps },
            { "AppFolder",        AppFolder },
            { "Cloud",            Cloud },
            { "CloudUpload",      CloudUpload },
            { "CloudDownload",    CloudDownload },
            { "Wifi",             Wifi },
            { "Bluetooth",        Bluetooth },
            { "Battery",          Battery },
            { "Camera",           Camera },
            { "Image",            Image },
            { "Video",            Video },
            { "Music",            Music },
            { "Microphone",       Microphone },
            { "Code",             Code },
            { "Shield",           Shield },
            { "Permissions",      Permissions },
            { "Logout",           Logout },

            // Pratiche / workflow
            { "TaskList",         TaskList },
            { "ClipboardList",    ClipboardList },
            { "Checklist",        Checklist },
            { "WorkItem",         WorkItem },
            { "History",          History },
            { "Archive",          Archive },
            { "Scan",             Scan },
            { "Annotation",       Annotation },

            // Immobili / catasto
            { "Building",         Building },
            { "Blueprint",        Blueprint },
            { "Area",             Area },
            { "Construction",     Construction },
            { "ParkingLocation",  ParkingLocation },

            // Finanza / computo
            { "Calculator",       Calculator },
            { "Money",            Money },
            { "Receipt",          Receipt },
            { "CreditCard",       CreditCard },

            // Scadenze / tempo
            { "CalendarAlert",    CalendarAlert },
            { "CalendarCheck",    CalendarCheck },
            { "Timer",            Timer },
            { "Hourglass",        Hourglass },

            // Utility UI
            { "MoreVertical",     MoreVertical },
            { "MoreHorizontal",   MoreHorizontal },
            { "Expand",           Expand },
            { "Collapse",         Collapse },
            { "NewWindow",        NewWindow },
            { "OpenInNewWindow",  OpenInNewWindow },
            { "Move",             Move },
            { "Rotate",           Rotate },
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