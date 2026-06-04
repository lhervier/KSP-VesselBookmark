using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using com.github.lhervier.ksp.bookmarksmod.bookmarks;

namespace com.github.lhervier.ksp.bookmarksmod {
    
    /// <summary>
    /// Lookup tables built once per refresh pass. Refreshing N bookmarks against the index costs
    /// O(N + universe) instead of O(N × universe) : without it, each bookmark used to rescan every
    /// part of every vessel (loaded and unloaded) plus every alarm. Build one (via <see cref="Build"/>)
    /// at the start of a refresh and share it across every bookmark of that pass.
    /// </summary>
    public class VesselsManager : MonoBehaviour {

        private static readonly ModLogger LOGGER = new ModLogger("BookmarkRefreshIndex");

        /// <summary>flightID → loaded Part</summary>
        public Dictionary<uint, Part> PartsByFlightId = new Dictionary<uint, Part>();

        /// <summary>flightID → unloaded ProtoPartSnapshot</summary>
        public Dictionary<uint, ProtoPartSnapshot> ProtoPartsByFlightId = new Dictionary<uint, ProtoPartSnapshot>();

        /// <summary>vessel persistentId → Vessel (loaded vessels take precedence over unloaded ones)</summary>
        public Dictionary<uint, Vessel> VesselsByPersistentId = new Dictionary<uint, Vessel>();

        /// <summary>persistentId of every vessel that currently has an alarm</summary>
        public HashSet<uint> VesselsWithAlarm = new HashSet<uint>();

        public EventVoid OnVesselsChanged = new EventVoid("VesselsManager.OnVesselsChanged");
        private bool _refreshRequested = false;

        // ==================================================================================
        // Life cycle
        // ==================================================================================

        public void Awake()
        {
            
        }

        public void Start()
        {
            // Subscribe to vessel events
            GameEvents.onVesselWasModified.Add(OnVesselWasModified);
            GameEvents.onVesselDestroy.Add(OnVesselDestroy);
            GameEvents.onVesselRename.Add(OnVesselRename);

            // Subscribe to alarm events
            GameEvents.onAlarmAdded.Add(OnAlarmAdded);
            GameEvents.onAlarmRemoved.Add(OnAlarmRemoved);
            GameEvents.onAlarmTriggered.Add(OnAlarmTriggered);

            // Rebuild whenever a scene finishes loading : the vessels of the freshly-loaded game are
            // present by then (unlike at onGameState*Load time), so bookmarks loaded with the scene
            // resolve against a populated index without needing anyone to push a refresh.
            GameEvents.onLevelWasLoaded.Add(OnLevelWasLoaded);

            // Build the index once at startup too : RefreshIndex is created lazily, so onLevelWasLoaded
            // for the current scene may already have fired before this Start ran.
            this.RequestRefresh();
        }

        public void OnDestroy()
        {
            // Unsubscribe from vessel events
            GameEvents.onVesselWasModified.Remove(OnVesselWasModified);
            GameEvents.onVesselDestroy.Remove(OnVesselDestroy);
            GameEvents.onVesselRename.Remove(OnVesselRename);
            
            // Unsubscribe from alarm events
            GameEvents.onAlarmAdded.Remove(OnAlarmAdded);
            GameEvents.onAlarmRemoved.Remove(OnAlarmRemoved);
            GameEvents.onAlarmTriggered.Remove(OnAlarmTriggered);

            // Unsubscribe from scene events
            GameEvents.onLevelWasLoaded.Remove(OnLevelWasLoaded);
        }

        public void LateUpdate()
        {
            if( !_refreshRequested ) return;
            _refreshRequested = false;

            // Wipe the previous pass before re-scanning : the lookup tables are reused across refreshes,
            // so without this they would accumulate destroyed parts/vessels and never-removed alarms
            // (HasAlarm stuck true, "loaded takes precedence" inverted, unbounded memory growth).
            this.PartsByFlightId.Clear();
            this.ProtoPartsByFlightId.Clear();
            this.VesselsByPersistentId.Clear();
            this.VesselsWithAlarm.Clear();

            this.IndexLoadedVessels();
            this.IndexUnloadedVessels();
            this.IndexAlarms();

            this.OnVesselsChanged.Fire();
        }

        /// <summary>
        /// Called when a scene has finished loading
        /// </summary>
        /// <param name="scene">The scene that was loaded</param>
        private void OnLevelWasLoaded(GameScenes scene) {
            _refreshRequested = true;
        }

        /// <summary>
        /// Called when a vessel is modified
        /// </summary>
        /// <param name="vessel">The vessel that was modified</param>
        private void OnVesselWasModified(Vessel vessel) {
            _refreshRequested = true;
        }

        /// <summary>
        /// Called when a vessel is destroyed
        /// </summary>
        /// <param name="vessel">The vessel that was destroyed</param>
        private void OnVesselDestroy(Vessel vessel) {
            _refreshRequested = true;
        }

        /// <summary>
        /// Called when a vessel is renamed
        /// </summary>
        /// <param name="vessel">The vessel that was renamed</param>
        private void OnVesselRename(GameEvents.HostedFromToAction<Vessel, string> action) {
            _refreshRequested = true;
        }

        /// <summary>
        /// Called when an alarm is added
        /// </summary>
        /// <param name="alarm">The alarm that was added</param>
        private void OnAlarmAdded(AlarmTypeBase alarm) {
            _refreshRequested = true;
        }

        /// <summary>
        /// Called when an alarm is removed
        /// </summary>
        /// <param name="alarm">The alarm that was removed</param>
        private void OnAlarmRemoved(uint alarmID) {
            _refreshRequested = true;
        }

        /// <summary>
        /// Called when an alarm is triggered
        /// </summary>
        /// <param name="alarm">The alarm that was triggered</param>
        private void OnAlarmTriggered(AlarmTypeBase alarm) {
            _refreshRequested = true;
        }
        
        // ===================================================================================
        // Index update methods
        // ===================================================================================

        private void IndexLoadedVessels() {
            if (FlightGlobals.Vessels == null) return;
            foreach (Vessel vessel in FlightGlobals.Vessels) {
                if (vessel == null) continue;
                if (vessel.persistentId != 0 && !VesselsByPersistentId.ContainsKey(vessel.persistentId)) {
                    VesselsByPersistentId[vessel.persistentId] = vessel;
                }
                if (vessel.parts == null) continue;
                foreach (Part part in vessel.parts) {
                    if (part == null) continue;
                    PartsByFlightId[part.flightID] = part;
                }
            }
        }

        private void IndexUnloadedVessels() {
            if (FlightGlobals.VesselsUnloaded == null) return;
            foreach (Vessel vessel in FlightGlobals.VesselsUnloaded) {
                if (vessel == null) continue;
                if (vessel.persistentId != 0 && !VesselsByPersistentId.ContainsKey(vessel.persistentId)) {
                    VesselsByPersistentId[vessel.persistentId] = vessel;
                }
                if (vessel.protoVessel == null || vessel.protoVessel.protoPartSnapshots == null) continue;
                foreach (ProtoPartSnapshot protoPart in vessel.protoVessel.protoPartSnapshots) {
                    if (protoPart == null) continue;
                    ProtoPartsByFlightId[protoPart.flightID] = protoPart;
                }
            }
        }

        private void IndexAlarms() {
            try {
                if (AlarmClockScenario.Instance == null) return;
                foreach (AlarmTypeBase alarm in AlarmClockScenario.Instance.alarms.Values) {
                    if (alarm == null || alarm.Vessel == null) continue;
                    VesselsWithAlarm.Add(alarm.Vessel.persistentId);
                }
            } catch (Exception e) {
                LOGGER.LogError($"Error indexing alarms: {e.Message}");
            }
        }

        // ===============================================================================
        // Public API
        // ===============================================================================

        /// <summary>
        /// Request an index rebuild. The rebuild is coalesced to at most once per frame (see
        /// <see cref="LateUpdate"/>), so this is cheap to call repeatedly.
        /// </summary>
        public void RequestRefresh()
        {
            _refreshRequested = true;
        }
        
    }
}
