using com.github.lhervier.ksp.shared;
using com.github.lhervier.ksp.shared.ugui.sprites;
using UnityEngine;

namespace com.github.lhervier.ksp.bookmarksmod {
	
	[KSPAddon(KSPAddon.Startup.PSystemSpawn, false)]
    public class Mod : MonoBehaviour {

        private static readonly ModLogger LOGGER = new ModLogger("Mod");

        private BookmarksManager _bookmarkManager;

        protected void Awake()
        {
            LOGGER.LogInfo("Awaked");
            DontDestroyOnLoad(this);

            // Footer action icons: the game SDF font cannot render them, so labels reference them
            // through <sprite name=...> tags resolved against these textures. The unicode codepoint is
            // the glyph each sprite replaces, used as a fallback when a raw character slips through.
            SpritesIcons.RegisterSprite("edit", Constants.ModName + "/Textures/edit", 0x270E);     // ✎
            SpritesIcons.RegisterSprite("goto", Constants.ModName + "/Textures/goto", 0x27A4);     // ➤
            SpritesIcons.RegisterSprite("target", Constants.ModName + "/Textures/target", 0x25CE); // ◎
        }

        public void Start() {
            LOGGER.LogInfo("Plugin started");
            ModLogger.SetLogLevel(LogLevel.Debug);

            this._bookmarkManager = this.gameObject.AddComponent<BookmarksManager>();
            
            // Subscribe to save/load events
            GameEvents.onGameStateCreated.Add(OnGameStateCreated);
            GameEvents.onGameStatePostLoad.Add(OnGameStatePostLoad);
            GameEvents.onGameStateSave.Add(OnGameStateSave);

            LOGGER.LogInfo("Events subscribed");
        }

        public void OnDestroy() {
            LOGGER.LogInfo("Plugin stopped");

            // Unsubscribe from save/load events
            GameEvents.onGameStateCreated.Remove(OnGameStateCreated);
            GameEvents.onGameStatePostLoad.Remove(OnGameStatePostLoad);
            GameEvents.onGameStateSave.Remove(OnGameStateSave);

            LOGGER.LogInfo("Events unsubscribed");
        }

        /// <summary>
        /// Called when the game has finished loading a save file.
        /// </summary>
        /// <param name="configNode"></param>
        private void OnGameStatePostLoad(ConfigNode configNode) {
            _bookmarkManager.LoadBookmarks(configNode.GetNode("GAME"));
        }

        /// <summary>
        /// Called when the game loads a save file for the first time.
        /// The game will then save the config again, <b>before calling OnGameStateLoad</b>. 
        /// So we need to load the bookmarks from the config file here.
        /// </summary>
        /// <param name="game"></param>
        private void OnGameStateCreated(Game game) {
            _bookmarkManager.LoadBookmarks(game.config);
        }

        /// <summary>
        /// Save bookmarks to save file
        /// </summary>
        private void OnGameStateSave(ConfigNode node) {
            _bookmarkManager.SaveBookmarks(node);
        }
    }
}
