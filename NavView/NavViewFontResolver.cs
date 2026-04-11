// NavViewFontResolver.cs
// Risolve il font icone con tre livelli di fallback:
//   1. Segoe Fluent Icons installato nel sistema (Windows 11)
//   2. Segoe Fluent Icons embedded nella DLL    (Windows 10)
//   3. Segoe MDL2 Assets                        (fallback finale)

using System.Drawing.Text;
using System.Reflection;
using System.Runtime.InteropServices;

namespace NavView
{
    /// <summary>
    /// Gestisce la risoluzione e il caricamento del font icone.
    /// Tenta Segoe Fluent Icons di sistema, poi embedded, poi MDL2 Assets.
    /// Thread-safe: la risoluzione avviene una sola volta (lazy).
    /// </summary>
    public static class NavViewFontResolver
    {
        // -------------------------------------------------------------------------
        // Costanti
        // -------------------------------------------------------------------------

        private const string FluentFontName = "Segoe Fluent Icons";
        private const string Mdl2FontName = "Segoe MDL2 Assets";

        /// <summary>Nome della risorsa embedded nel formato namespace.cartella.file.</summary>
        private const string EmbeddedResourceName = "NavView.Resources.SegoeFluentIcons.ttf";

        // -------------------------------------------------------------------------
        // Stato (lazy, thread-safe con Lazy<T>)
        // -------------------------------------------------------------------------

        private static readonly Lazy<ResolvedFont> _resolved =
            new Lazy<ResolvedFont>(Resolve, LazyThreadSafetyMode.ExecutionAndPublication);

        // -------------------------------------------------------------------------
        // API pubblica
        // -------------------------------------------------------------------------

        /// <summary>
        /// Nome del font da usare per le icone.
        /// Può essere "Segoe Fluent Icons" o "Segoe MDL2 Assets".
        /// </summary>
        public static string FontName => _resolved.Value.Name;

        /// <summary>
        /// Crea un Font con le dimensioni richieste usando il font risolto.
        /// Il chiamante è responsabile del Dispose.
        /// </summary>
        public static Font CreateIconFont(float size)
        {
            var r = _resolved.Value;

            // Se abbiamo una PrivateFontCollection (font embedded) la usiamo
            if (r.PrivateCollection != null)
            {
                var family = r.PrivateCollection.Families
                               .FirstOrDefault(f => f.Name == FluentFontName);
                if (family != null)
                    return new Font(family, size, FontStyle.Regular, GraphicsUnit.Point);
            }

            // Font di sistema (Fluent installato o MDL2)
            return new Font(r.Name, size, FontStyle.Regular, GraphicsUnit.Point);
        }

        /// <summary>
        /// Origine del font risolto. Utile per diagnostica.
        /// </summary>
        public static FontSource ResolvedSource => _resolved.Value.Source;

        // -------------------------------------------------------------------------
        // Logica di risoluzione
        // -------------------------------------------------------------------------

        private static ResolvedFont Resolve()
        {
            // --- Livello 1: Segoe Fluent Icons nel sistema -------------------
            if (IsFontInstalled(FluentFontName))
                return new ResolvedFont(FluentFontName, FontSource.System, null);

            // --- Livello 2: font embedded nella DLL -------------------------
            var pfc = TryLoadEmbedded();
            if (pfc != null)
                return new ResolvedFont(FluentFontName, FontSource.Embedded, pfc);

            // --- Livello 3: fallback su Segoe MDL2 Assets -------------------
            return new ResolvedFont(Mdl2FontName, FontSource.Mdl2Fallback, null);
        }

        /// <summary>
        /// Controlla se un font è installato nel sistema usando InstalledFontCollection.
        /// </summary>
        private static bool IsFontInstalled(string fontName)
        {
            using var ifc = new InstalledFontCollection();
            return ifc.Families.Any(f =>
                string.Equals(f.Name, fontName, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Tenta di caricare il font dalla risorsa embedded.
        /// Restituisce null se la risorsa non esiste o il caricamento fallisce.
        /// </summary>
        private static PrivateFontCollection? TryLoadEmbedded()
        {
            try
            {
                var asm = Assembly.GetExecutingAssembly();
                using var stream = asm.GetManifestResourceStream(EmbeddedResourceName);
                if (stream == null) return null;

                // Legge i byte del font
                var fontData = new byte[stream.Length];
                _ = stream.Read(fontData, 0, fontData.Length);

                // Alloca memoria non gestita e ci copia i dati
                // (richiesto da PrivateFontCollection.AddMemoryFont)
                var handle = Marshal.AllocCoTaskMem(fontData.Length);
                try
                {
                    Marshal.Copy(fontData, 0, handle, fontData.Length);
                    var pfc = new PrivateFontCollection();
                    pfc.AddMemoryFont(handle, fontData.Length);
                    return pfc;
                }
                finally
                {
                    // La memoria può essere liberata subito dopo AddMemoryFont:
                    // GDI+ copia i dati internamente.
                    Marshal.FreeCoTaskMem(handle);
                }
            }
            catch
            {
                // Qualsiasi errore (risorsa mancante, font corrotto, ecc.)
                // → il chiamante scende al livello 3
                return null;
            }
        }

        // -------------------------------------------------------------------------
        // Tipi interni
        // -------------------------------------------------------------------------

        private sealed class ResolvedFont
        {
            public string Name { get; }
            public FontSource Source { get; }

            /// <summary>
            /// Non null solo quando Source == Embedded.
            /// Tenuta viva per tutta la durata dell'app perché GDI+
            /// richiede che la PrivateFontCollection rimanga in memoria
            /// finché i font derivati sono in uso.
            /// </summary>
            public PrivateFontCollection? PrivateCollection { get; }

            public ResolvedFont(string name, FontSource source, PrivateFontCollection? pfc)
            {
                Name = name;
                Source = source;
                PrivateCollection = pfc;
            }
        }
    }

    /// <summary>
    /// Indica l'origine del font icone risolto.
    /// </summary>
    public enum FontSource
    {
        /// <summary>Segoe Fluent Icons installato nel sistema (Windows 11).</summary>
        System,

        /// <summary>Segoe Fluent Icons caricato dalla risorsa embedded (Windows 10).</summary>
        Embedded,

        /// <summary>Fallback su Segoe MDL2 Assets.</summary>
        Mdl2Fallback
    }
}