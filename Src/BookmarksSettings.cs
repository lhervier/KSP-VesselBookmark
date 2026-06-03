using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using UnityEngine;
using com.github.lhervier.ksp.bookmarksmod.util;

namespace com.github.lhervier.ksp.bookmarksmod {

    /// <summary>
    /// Réglages globaux du mod (par installation, indépendants de la sauvegarde). Persistés dans
    /// PluginData/settings.cfg à côté de la DLL — le dossier PluginData est ignoré par GameDatabase,
    /// donc KSP ne tente pas d'interpréter ce fichier comme un config de pièce.
    /// </summary>
    public class BookmarksSettings {
        private static readonly ModLogger LOGGER = new ModLogger("BookmarksSettings");

        private const string ROOT_NODE = "VESSEL_BOOKMARKS_SETTINGS";

        /// <summary>Position (localPosition) mémorisée de la fenêtre, si elle a déjà été capturée.</summary>
        public bool HasWindowPosition { get; private set; }
        public Vector2 WindowPosition { get; private set; }

        public void SetWindowPosition(Vector2 position) {
            WindowPosition = position;
            HasWindowPosition = true;
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
                if (node.HasValue("windowX") && node.HasValue("windowY")
                    && float.TryParse(node.GetValue("windowX"), NumberStyles.Float, CultureInfo.InvariantCulture, out float x)
                    && float.TryParse(node.GetValue("windowY"), NumberStyles.Float, CultureInfo.InvariantCulture, out float y)) {
                    WindowPosition = new Vector2(x, y);
                    HasWindowPosition = true;
                }
            } catch (Exception e) {
                LOGGER.LogError($"Error loading settings: {e.Message}");
            }
        }

        public void Save() {
            try {
                var root = new ConfigNode();
                ConfigNode node = root.AddNode(ROOT_NODE);
                if (HasWindowPosition) {
                    node.AddValue("windowX", WindowPosition.x.ToString(CultureInfo.InvariantCulture));
                    node.AddValue("windowY", WindowPosition.y.ToString(CultureInfo.InvariantCulture));
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
