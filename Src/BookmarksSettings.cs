using System;
using System.IO;
using System.Reflection;
using com.github.lhervier.ksp.shared;

namespace com.github.lhervier.ksp.bookmarksmod {

    /// <summary>
    /// Réglages globaux du mod (par installation, indépendants de la sauvegarde). Persistés dans
    /// PluginData/settings.cfg à côté de la DLL — le dossier PluginData est ignoré par GameDatabase,
    /// donc KSP ne tente pas d'interpréter ce fichier comme un config de pièce.
    /// </summary>
    public class BookmarksSettings {
        private static readonly ModLogger LOGGER = new ModLogger("BookmarksSettings");

        private const string ROOT_NODE = "VESSEL_BOOKMARKS_SETTINGS";

        // The window's open/closed state is no longer stored here: it now lives alongside the window
        // position in the shared PopupSettings (PluginData/[popupID].cfg), owned by the PopupController.

        /// <summary>
        /// Memorized search criteria, if they have already been captured. Values are kept verbatim
        /// (including the "All"/"CURRENT" tokens) : the settings do not know their semantics, they
        /// only persist whatever the view model hands them.
        /// </summary>
        public bool HasCriteria { get; private set; }
        public string SelectedBody { get; private set; }
        public string SelectedVesselType { get; private set; }
        public string SelectedSituation { get; private set; }
        public string SelectedAlarm { get; private set; }
        public string SearchText { get; private set; }
        public bool FilterHasComment { get; private set; }

        public void SetCriteria(string selectedBody, string selectedVesselType, string selectedSituation, string selectedAlarm, string searchText, bool filterHasComment) {
            SelectedBody = selectedBody;
            SelectedVesselType = selectedVesselType;
            SelectedSituation = selectedSituation;
            SelectedAlarm = selectedAlarm;
            SearchText = searchText;
            FilterHasComment = filterHasComment;
            HasCriteria = true;
        }

        private static string SettingsPath {
            get {
                string dir = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "PluginData");
                return Path.Combine(dir, "settings.cfg");
            }
        }

        public void Load() {
            try {
                string path = SettingsPath;
                if (!File.Exists(path)) {
                    return;
                }
                ConfigNode root = ConfigNode.Load(path);
                ConfigNode node = root?.GetNode(ROOT_NODE);
                if (node == null) {
                    return;
                }
                // "selectedBody" acts as the presence marker for the whole criteria group : the values
                // are always written together, so its presence means the group was saved. A criterion
                // added later is absent from an older file : it reads back empty, and the caller is the
                // one that knows the default to fall back on.
                if (node.HasValue("selectedBody")) {
                    SelectedBody = node.GetValue("selectedBody") ?? string.Empty;
                    SelectedVesselType = node.GetValue("selectedVesselType") ?? string.Empty;
                    SelectedSituation = node.GetValue("selectedSituation") ?? string.Empty;
                    SelectedAlarm = node.GetValue("selectedAlarm") ?? string.Empty;
                    SearchText = node.GetValue("searchText") ?? string.Empty;
                    bool.TryParse(node.GetValue("filterHasComment"), out bool filterHasComment);
                    FilterHasComment = filterHasComment;
                    HasCriteria = true;
                }
            } catch (Exception e) {
                LOGGER.LogError($"Error loading settings: {e.Message}");
            }
        }

        public void Save() {
            try {
                var root = new ConfigNode();
                ConfigNode node = root.AddNode(ROOT_NODE);
                if (HasCriteria) {
                    node.AddValue("selectedBody", SelectedBody ?? string.Empty);
                    node.AddValue("selectedVesselType", SelectedVesselType ?? string.Empty);
                    node.AddValue("selectedSituation", SelectedSituation ?? string.Empty);
                    node.AddValue("selectedAlarm", SelectedAlarm ?? string.Empty);
                    node.AddValue("searchText", SearchText ?? string.Empty);
                    node.AddValue("filterHasComment", FilterHasComment.ToString());
                }
                string path = SettingsPath;
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                root.Save(path);
            } catch (Exception e) {
                LOGGER.LogError($"Error saving settings: {e.Message}");
            }
        }
    }
}
