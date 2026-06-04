using System;
using System.Collections.Generic;
using System.IO;
using com.github.lhervier.ksp.bookmarksmod.ui;
using Expansions.Missions.Editor;
using UnityEngine;

namespace com.github.lhervier.ksp.bookmarksmod {
	
	[KSPAddon(KSPAddon.Startup.PSystemSpawn, false)]
    public class Mod : MonoBehaviour {

        private static readonly ModLogger LOGGER = new ModLogger("Mod");

        private BookmarksManager _bookmarkManager;
        private MainUI _mainUI;

        protected void Awake()
        {
            LOGGER.LogInfo("Awaked");
            DontDestroyOnLoad(this);
        }

        public void Start() {
            LOGGER.LogInfo("Plugin started");

            this._bookmarkManager = this.gameObject.AddComponent<BookmarksManager>();
            this._mainUI = this.gameObject.AddComponent<MainUI>();
            this._mainUI.Initialize(this._bookmarkManager);

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
