using System;
using System.Collections.Generic;
using System.IO;
using Expansions.Missions.Editor;
using UnityEngine;

namespace com.github.lhervier.ksp.bookmarksmod {
	
	[KSPAddon(KSPAddon.Startup.PSystemSpawn, false)]
    public class Mod : MonoBehaviour {
        
        protected void Awake() 
        {
            ModLogger.LogInfo("Awaked");
            DontDestroyOnLoad(this);
        }

        public void Start() {
            ModLogger.LogInfo("Plugin started");
            
            // Subscribe to save/load events
            GameEvents.onGameStateCreated.Add(OnGameStateCreated);
            GameEvents.onGameStatePostLoad.Add(OnGameStatePostLoad);
            GameEvents.onGameStateSave.Add(OnGameStateSave);
            
            // Subscribe to vessel events
            GameEvents.onVesselWasModified.Add(OnVesselWasModified);
            GameEvents.onVesselDestroy.Add(OnVesselDestroy);
            GameEvents.onVesselRename.Add(OnVesselRename);

            // Subscribe to alarm events
            GameEvents.onAlarmAdded.Add(OnAlarmAdded);
            GameEvents.onAlarmRemoved.Add(OnAlarmRemoved);
            GameEvents.onAlarmTriggered.Add(OnAlarmTriggered);

            ModLogger.LogInfo("Events subscribed");
        }

        public void OnDestroy() {
            ModLogger.LogInfo("Plugin stopped");

            // Unsubscribe from save/load events
            GameEvents.onGameStateCreated.Remove(OnGameStateCreated);
            GameEvents.onGameStatePostLoad.Remove(OnGameStatePostLoad);
            GameEvents.onGameStateSave.Remove(OnGameStateSave);

            // Unsubscribe from vessel events
            GameEvents.onVesselWasModified.Remove(OnVesselWasModified);
            GameEvents.onVesselDestroy.Remove(OnVesselDestroy);
            GameEvents.onVesselRename.Remove(OnVesselRename);
            
            // Unsubscribe from alarm events
            GameEvents.onAlarmAdded.Remove(OnAlarmAdded);
            GameEvents.onAlarmRemoved.Remove(OnAlarmRemoved);
            GameEvents.onAlarmTriggered.Remove(OnAlarmTriggered);

            ModLogger.LogInfo("Events unsubscribed");
        }

        /// <summary>
        /// Called when the game has finished loading a save file.
        /// </summary>
        /// <param name="configNode"></param>
        private void OnGameStatePostLoad(ConfigNode configNode) {
            BookmarkManager.LoadBookmarks(configNode.GetNode("GAME"));
        }

        /// <summary>
        /// Called when the game loads a save file for the first time.
        /// The game will then save the config again, <b>before calling OnGameStateLoad</b>. 
        /// So we need to load the bookmarks from the config file here.
        /// </summary>
        /// <param name="game"></param>
        private void OnGameStateCreated(Game game) {
            BookmarkManager.LoadBookmarks(game.config);
        }

        /// <summary>
        /// Save bookmarks to save file
        /// </summary>
        private void OnGameStateSave(ConfigNode node) {
            BookmarkManager.SaveBookmarks(node);
        }

        /// <summary>
        /// Called when a vessel is modified
        /// </summary>
        /// <param name="vessel">The vessel that was modified</param>
        private void OnVesselWasModified(Vessel vessel) {
            BookmarkManager.RefreshBookmarksInAnyInstance();
        }

        /// <summary>
        /// Called when a vessel is destroyed
        /// </summary>
        /// <param name="vessel">The vessel that was destroyed</param>
        private void OnVesselDestroy(Vessel vessel) {
            BookmarkManager.RefreshBookmarksInAnyInstance();
        }

        /// <summary>
        /// Called when a vessel is renamed
        /// </summary>
        /// <param name="vessel">The vessel that was renamed</param>
        private void OnVesselRename(GameEvents.HostedFromToAction<Vessel, string> action) {
            BookmarkManager.RefreshBookmarksInAnyInstance();
        }

        /// <summary>
        /// Called when an alarm is added
        /// </summary>
        /// <param name="alarm">The alarm that was added</param>
        private void OnAlarmAdded(AlarmTypeBase alarm) {
            BookmarkManager.RefreshBookmarksInAnyInstance();
        }

        /// <summary>
        /// Called when an alarm is removed
        /// </summary>
        /// <param name="alarm">The alarm that was removed</param>
        private void OnAlarmRemoved(uint alarmID) {
            BookmarkManager.RefreshBookmarksInAnyInstance();
        }

        /// <summary>
        /// Called when an alarm is triggered
        /// </summary>
        /// <param name="alarm">The alarm that was triggered</param>
        private void OnAlarmTriggered(AlarmTypeBase alarm) {
            BookmarkManager.RefreshBookmarksInAnyInstance();
        }
    }
}
