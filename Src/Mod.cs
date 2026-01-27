using System;
using System.Collections.Generic;
using System.IO;
using Expansions.Missions.Editor;
using UnityEngine;

namespace com.github.lhervier.ksp.bookmarksmod {
	
	[KSPAddon(KSPAddon.Startup.PSystemSpawn, false)]
    public class Mod : MonoBehaviour {
        
        private BookmarkManager _manager;
        
        protected void Awake() 
        {
            ModLogger.LogInfo("Awaked");
            DontDestroyOnLoad(this);
        }

        public void Start() {
            ModLogger.LogInfo("Plugin started");
            
            // Initialize the bookmark manager
            // The singleton will be created automatically on first access
            this._manager = BookmarkManager.Instance;
            ModLogger.LogInfo("VesselBookmarkManager initialized");

            // Subscribe to save/load events
            GameEvents.onGameStateCreated.Add(OnGameStateCreated);
            GameEvents.onGameStatePostLoad.Add(OnGameStatePostLoad);
            GameEvents.onGameStateSave.Add(OnGameStateSave);
            GameEvents.onVesselWasModified.Add(OnVesselWasModified);

            ModLogger.LogInfo("Events subscribed");
        }

        public void OnDestroy() {
            ModLogger.LogInfo("Plugin stopped");

            // Unsubscribe from save/load events
            GameEvents.onGameStateCreated.Remove(OnGameStateCreated);
            GameEvents.onGameStatePostLoad.Remove(OnGameStatePostLoad);
            GameEvents.onGameStateSave.Remove(OnGameStateSave);
            GameEvents.onVesselWasModified.Remove(OnVesselWasModified);

            _manager = null;

            ModLogger.LogInfo("Events unsubscribed");
        }

        /// <summary>
        /// Called when the game has finished loading a save file.
        /// </summary>
        /// <param name="configNode"></param>
        private void OnGameStatePostLoad(ConfigNode configNode) {
            this._manager.LoadBookmarks(configNode.GetNode("GAME"));
        }

        /// <summary>
        /// Called when the game loads a save file for the first time.
        /// The game will then save the config again, <b>before calling OnGameStateLoad</b>. 
        /// So we need to load the bookmarks from the config file here.
        /// </summary>
        /// <param name="game"></param>
        private void OnGameStateCreated(Game game) {
            this._manager.LoadBookmarks(game.config);
        }

        /// <summary>
        /// Save bookmarks to save file
        /// </summary>
        private void OnGameStateSave(ConfigNode node) {
            this._manager.SaveBookmarks(node);
        }

        /// <summary>
        /// Called when a vessel is modified
        /// </summary>
        /// <param name="vessel">The vessel that was modified</param>
        private void OnVesselWasModified(Vessel vessel) {
            this._manager.RefreshBookmarks();
        }
    }
}
