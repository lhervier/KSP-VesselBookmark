using System;
using System.Collections.Generic;
using System.IO;
using Expansions.Missions.Editor;
using UnityEngine;

namespace com.github.lhervier.ksp {
	
	[KSPAddon(KSPAddon.Startup.PSystemSpawn, false)]
    public class VesselBookmarkMod : MonoBehaviour {
        
        private VesselBookmarkManager _manager;
        
        protected void Awake() 
        {
            ModLogger.LogInfo("Awaked");
            DontDestroyOnLoad(this);
        }

        public void Start() {
            ModLogger.LogInfo("Plugin started");
            
            // Initialize the bookmark manager
            // The singleton will be created automatically on first access
            this._manager = VesselBookmarkManager.Instance;
            ModLogger.LogInfo("VesselBookmarkManager initialized");

            // Subscribe to save/load events
            GameEvents.onGameStateCreated.Add(OnGameStateCreated);
            GameEvents.onGameStateLoad.Add(OnGameStateLoad);
            GameEvents.onGameStateSave.Add(OnGameStateSave);
            ModLogger.LogInfo("Events subscribed");
        }

        public void OnDestroy() {
            ModLogger.LogInfo("Plugin stopped");

            // Unsubscribe from save/load events
            GameEvents.onGameStateCreated.Remove(OnGameStateCreated);
            GameEvents.onGameStateLoad.Remove(OnGameStateLoad);
            GameEvents.onGameStateSave.Remove(OnGameStateSave);
            ModLogger.LogInfo("Events unsubscribed");
        }

        /// <summary>
        /// Called when the game loads a save file for the first time.
        /// The game will then save the config again, <b>before calling OnGameStateLoad</b>. 
        /// So we need to load the bookmarks from the config file here.
        /// </summary>
        /// <param name="game"></param>
        private void OnGameStateCreated(Game game) {
            ModLogger.LogDebug("On Game state created");
            this._manager.LoadBookmarks(game.config);
        }

        /// <summary>
        /// Called when the game loads a save file.
        /// </summary>
        /// <param name="node"></param>
        private void OnGameStateLoad(ConfigNode node) {
            ModLogger.LogDebug("On Game state load");
            this._manager.LoadBookmarks(node);
        }

        /// <summary>
        /// Save bookmarks to save file
        /// </summary>
        private void OnGameStateSave(ConfigNode node) {
            ModLogger.LogDebug("On Game state save");
            this._manager.SaveBookmarks(node);
        }
    }
}
