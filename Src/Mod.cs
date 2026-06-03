using System;
using System.Collections.Generic;
using System.IO;
using Expansions.Missions.Editor;
using UnityEngine;

namespace com.github.lhervier.ksp.bookmarksmod {
	
	[KSPAddon(KSPAddon.Startup.PSystemSpawn, false)]
    public class Mod : MonoBehaviour {

        private static readonly ModLogger LOGGER = new ModLogger("Mod");

        /// <summary>
        /// True while a scene change is in progress. The game destroys every vessel during scene
        /// teardown, which fires onVesselDestroy en masse. Honoring those refreshes would rebuild the
        /// uGUI window against a canvas that is being torn down (degenerate geometry) and hang the main
        /// thread. Bookmarks are reloaded anyway by OnGameStatePostLoad on the new scene.
        /// </summary>
        private bool _sceneLoading = false;

        /// <summary>
        /// Set when a vessel/alarm event asks for a refresh. The actual (potentially expensive) refresh
        /// is coalesced to at most once per frame in <see cref="LateUpdate"/> : a single in-game action
        /// (docking, staging…) often fires a burst of events in the same frame, and we only want to
        /// rebuild the bookmark data once.
        /// </summary>
        private bool _refreshRequested = false;

        protected void Awake()
        {
            LOGGER.LogInfo("Awaked");
            DontDestroyOnLoad(this);
        }

        /// <summary>
        /// Coalesced refresh : runs at most one RefreshBookmarks per frame, and never during a scene
        /// change (the new scene reloads the bookmarks itself).
        /// </summary>
        public void LateUpdate()
        {
            if( !_refreshRequested ) {
                return;
            }
            _refreshRequested = false;
            if( _sceneLoading ) {
                return;
            }
            BookmarkManager.RefreshBookmarks();
        }

        public void Start() {
            LOGGER.LogInfo("Plugin started");
            
            // Subscribe to scene transition events (used to suppress refreshes during scene teardown)
            GameEvents.onGameSceneLoadRequested.Add(OnGameSceneLoadRequested);
            GameEvents.onLevelWasLoaded.Add(OnLevelWasLoaded);

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

            LOGGER.LogInfo("Events subscribed");
        }

        public void OnDestroy() {
            LOGGER.LogInfo("Plugin stopped");

            // Unsubscribe from scene transition events
            GameEvents.onGameSceneLoadRequested.Remove(OnGameSceneLoadRequested);
            GameEvents.onLevelWasLoaded.Remove(OnLevelWasLoaded);

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

            LOGGER.LogInfo("Events unsubscribed");
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
        /// A scene load has been requested: the current scene is about to be torn down (every vessel
        /// destroyed). Suppress vessel/alarm-driven refreshes until the new scene has loaded.
        /// </summary>
        private void OnGameSceneLoadRequested(GameScenes scene) {
            _sceneLoading = true;
        }

        /// <summary>
        /// The new scene has finished loading: refreshes may resume.
        /// </summary>
        private void OnLevelWasLoaded(GameScenes scene) {
            _sceneLoading = false;
        }

        /// <summary>
        /// Request a coalesced refresh (performed at most once per frame by <see cref="LateUpdate"/>).
        /// Ignored while a scene change is in progress (see <see cref="_sceneLoading"/>).
        /// </summary>
        private void RequestRefresh() {
            if( _sceneLoading ) {
                return;
            }
            _refreshRequested = true;
        }

        /// <summary>
        /// Called when a vessel is modified
        /// </summary>
        /// <param name="vessel">The vessel that was modified</param>
        private void OnVesselWasModified(Vessel vessel) {
            RequestRefresh();
        }

        /// <summary>
        /// Called when a vessel is destroyed
        /// </summary>
        /// <param name="vessel">The vessel that was destroyed</param>
        private void OnVesselDestroy(Vessel vessel) {
            RequestRefresh();
        }

        /// <summary>
        /// Called when a vessel is renamed
        /// </summary>
        /// <param name="vessel">The vessel that was renamed</param>
        private void OnVesselRename(GameEvents.HostedFromToAction<Vessel, string> action) {
            RequestRefresh();
        }

        /// <summary>
        /// Called when an alarm is added
        /// </summary>
        /// <param name="alarm">The alarm that was added</param>
        private void OnAlarmAdded(AlarmTypeBase alarm) {
            RequestRefresh();
        }

        /// <summary>
        /// Called when an alarm is removed
        /// </summary>
        /// <param name="alarm">The alarm that was removed</param>
        private void OnAlarmRemoved(uint alarmID) {
            RequestRefresh();
        }

        /// <summary>
        /// Called when an alarm is triggered
        /// </summary>
        /// <param name="alarm">The alarm that was triggered</param>
        private void OnAlarmTriggered(AlarmTypeBase alarm) {
            RequestRefresh();
        }
    }
}
