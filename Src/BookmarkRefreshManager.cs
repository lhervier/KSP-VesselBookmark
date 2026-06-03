using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using com.github.lhervier.ksp.bookmarksmod.bookmarks;

namespace com.github.lhervier.ksp.bookmarksmod {
    public class BookmarkRefreshManager {

        private static readonly ModLogger LOGGER = new ModLogger("BookmarkRefreshManager");

        /// <summary>
        /// Lookup tables built once per refresh pass. Refreshing N bookmarks against the index costs
        /// O(N + universe) instead of O(N × universe) : without it, each bookmark used to rescan every
        /// part of every vessel (loaded and unloaded) plus every alarm. Build one (via <see cref="Build"/>)
        /// at the start of a refresh and share it across every bookmark of that pass.
        /// </summary>
        public class RefreshIndex {

            /// <summary>flightID → loaded Part</summary>
            public readonly Dictionary<uint, Part> PartsByFlightId = new Dictionary<uint, Part>();

            /// <summary>flightID → unloaded ProtoPartSnapshot</summary>
            public readonly Dictionary<uint, ProtoPartSnapshot> ProtoPartsByFlightId = new Dictionary<uint, ProtoPartSnapshot>();

            /// <summary>vessel persistentId → Vessel (loaded vessels take precedence over unloaded ones)</summary>
            public readonly Dictionary<uint, Vessel> VesselsByPersistentId = new Dictionary<uint, Vessel>();

            /// <summary>persistentId of every vessel that currently has an alarm</summary>
            public readonly HashSet<uint> VesselsWithAlarm = new HashSet<uint>();

            /// <summary>
            /// Scan the universe once and fill the lookup tables.
            /// </summary>
            public static RefreshIndex Build() {
                var index = new RefreshIndex();
                index.IndexLoadedVessels();
                index.IndexUnloadedVessels();
                index.IndexAlarms();
                return index;
            }

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
        }

        // ===================================================================================
        //  Index lookups (O(1)) replacing the former linear scans
        // ===================================================================================

        /// <summary>
        /// Get the command module part for a flightID, or null if it is not a loaded command module.
        /// </summary>
        private static Part GetPart(uint commandModuleFlightID, RefreshIndex index) {
            if (!index.PartsByFlightId.TryGetValue(commandModuleFlightID, out Part part) || part == null) {
                return null;
            }
            if (part.FindModuleImplementing<ModuleCommand>() == null) {
                LOGGER.LogError($"Bookmark {commandModuleFlightID}: Target part is not a command module");
                return null;
            }
            return part;
        }

        /// <summary>
        /// Get the command module protoPartSnapshot for a flightID, or null if it is not an unloaded
        /// command module.
        /// </summary>
        private static ProtoPartSnapshot GetProtoPartSnapshot(uint commandModuleFlightID, RefreshIndex index) {
            if (!index.ProtoPartsByFlightId.TryGetValue(commandModuleFlightID, out ProtoPartSnapshot protoPart) || protoPart == null) {
                return null;
            }
            if (protoPart.FindModule("ModuleCommand") == null) {
                LOGGER.LogError($"Command module protoPartSnapshot {protoPart} for flightID {commandModuleFlightID} is not a command module");
                return null;
            }
            return protoPart;
        }

        /// <summary>
        /// Find the vessel for the bookmark (loaded vessels take precedence over unloaded ones).
        /// </summary>
        private static Vessel FindVessel(Bookmark bookmark, RefreshIndex index) {
            if (bookmark.VesselPersistentID == 0) {
                LOGGER.LogError($"Bookmark {bookmark}: Vessel persistent ID is empty");
                return null;
            }
            index.VesselsByPersistentId.TryGetValue(bookmark.VesselPersistentID, out Vessel vessel);
            return vessel;
        }

        /// <summary>
        /// Check if a vessel has an alarm.
        /// </summary>
        private static bool CheckHasAlarm(Vessel vessel, RefreshIndex index) {
            if (vessel == null) {
                return false;
            }
            return index.VesselsWithAlarm.Contains(vessel.persistentId);
        }

        /// <summary>
        /// Gets a textual description of vessel situation
        /// </summary>
        /// <param name="bodyName">The name of the body of the vessel</param>
        /// <param name="situation">The situation of the vessel</param>
        /// <returns>The label for the situation</returns>
        private static string GetSituationLabel(string bodyName, Vessel.Situations situation) {
            try {
                if( string.IsNullOrEmpty(bodyName) ) {
                    LOGGER.LogError($"Getting situation: Body is null");
                    return ModLocalization.GetString("situationUnknown");
                }

                switch (situation) {
                    case Vessel.Situations.LANDED:
                        return ModLocalization.GetString("situationLanded", bodyName);

                    case Vessel.Situations.SPLASHED:
                        return ModLocalization.GetString("situationSplashed", bodyName);

                    case Vessel.Situations.PRELAUNCH:
                        return ModLocalization.GetString("situationPrelaunch", bodyName);

                    case Vessel.Situations.SUB_ORBITAL:
                        return ModLocalization.GetString("situationSuborbital", bodyName);

                    case Vessel.Situations.ORBITING:
                        return ModLocalization.GetString("situationOrbiting", bodyName);

                    case Vessel.Situations.ESCAPING:
                        return ModLocalization.GetString("situationEscaping", bodyName);

                    default:
                        return ModLocalization.GetString("situationInFlight", bodyName);
                }
            } catch (System.Exception e) {
                LOGGER.LogError($"Error getting situation{situation} for vessel on body {bodyName}: {e.Message}");
                return ModLocalization.GetString("situationUnknown");
            }
        }

        // ===================================================================================
        //  Refresh
        // ===================================================================================

        /// <summary>
        /// Refresh a command module bookmark
        /// </summary>
        /// <param name="bookmark">The command module bookmark to refresh</param>
        /// <param name="index">The lookup tables for this refresh pass</param>
        /// <returns>True if the command module bookmark was refreshed, false otherwise</returns>
        private static bool RefreshCommandModuleBookmark(CommandModuleBookmark bookmark, RefreshIndex index) {
            try {
                bookmark.CommandModuleFlightID = bookmark.BookmarkID;

                uint vesselPersistentId;
                string cmName;
                string cmType;

                Part commandModulePart = GetPart(bookmark.CommandModuleFlightID, index);
                if (commandModulePart != null) {
                    vesselPersistentId = commandModulePart.vessel.persistentId;
                    if( commandModulePart.vesselNaming == null ) {
                        cmName = commandModulePart.vessel.vesselName;
                        cmType = commandModulePart.vessel.vesselType.ToString();
                    } else {
                        cmName = commandModulePart.vesselNaming.vesselName;
                        cmType = commandModulePart.vesselNaming.vesselType.ToString();
                    }
                } else {
                    ProtoPartSnapshot commandModuleProtoPartSnapshot = GetProtoPartSnapshot(bookmark.CommandModuleFlightID, index);
                    if (commandModuleProtoPartSnapshot != null) {
                        vesselPersistentId = commandModuleProtoPartSnapshot.pVesselRef.persistentId;
                        if( commandModuleProtoPartSnapshot.vesselNaming == null ) {
                            cmName = commandModuleProtoPartSnapshot.pVesselRef.vesselName;
                            cmType = commandModuleProtoPartSnapshot.pVesselRef.vesselType.ToString();
                        } else {
                            cmName = commandModuleProtoPartSnapshot.vesselNaming.vesselName;
                            cmType = commandModuleProtoPartSnapshot.vesselNaming.vesselType.ToString();
                        }
                    } else {
                        vesselPersistentId = bookmark.VesselPersistentID;
                        cmName = bookmark.VesselName;
                        cmType = bookmark.VesselType;
                    }
                }

                bookmark.VesselPersistentID = vesselPersistentId;
                bookmark.CommandModuleName = cmName;
                bookmark.CommandModuleType = cmType;

                bookmark.BookmarkTitle = bookmark.CommandModuleName;
                bookmark.BookmarkVesselType = bookmark.CommandModuleType;

                return true;
            } catch (Exception e) {
                LOGGER.LogError($"Error refreshing command module bookmark {bookmark}: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Refresh a single bookmark, building a one-off index. Use the
        /// <see cref="RefreshBookmark(Bookmark, RefreshIndex)"/> overload when refreshing several
        /// bookmarks in a row to share a single index.
        /// </summary>
        public static bool RefreshBookmark(Bookmark bookmark) {
            return RefreshBookmark(bookmark, RefreshIndex.Build());
        }

        /// <summary>
        /// Refresh a bookmark's transient (display) fields using a shared lookup index.
        /// </summary>
        public static bool RefreshBookmark(Bookmark bookmark, RefreshIndex index) {
            try {
                // Checking for mandatory values
                if( bookmark.BookmarkID == 0 ) {
                    LOGGER.LogWarning($"Bookmark {bookmark}: Bookmark ID is 0");
                    return false;
                }
                if( bookmark.BookmarkType == BookmarkType.Unknown ) {
                    LOGGER.LogWarning($"Bookmark {bookmark}: Bookmark type is unknown");
                    return false;
                }

                if( bookmark is CommandModuleBookmark commandModuleBookmark ) {
                    if( !RefreshCommandModuleBookmark(commandModuleBookmark, index) ) {
                        LOGGER.LogDebug($"Bookmark {commandModuleBookmark}: Failed to refresh command module bookmark. Let's continue with next one...");
                        return false;
                    }
                } else if( bookmark is VesselBookmark vesselBookmark ) {
                    vesselBookmark.VesselPersistentID = bookmark.BookmarkID;
                } else {
                    LOGGER.LogError($"Bookmark {bookmark}: Unknown bookmark type");
                    return false;
                }

                Vessel vessel = null;
                if( bookmark.VesselPersistentID != 0 ) {    // May happen if we were unable to refresh a command module bookmark.
                    vessel = FindVessel(bookmark, index);
                }
                if( vessel == null ) {
                    bookmark.Vessel = null;
                } else {
                    bookmark.Vessel = vessel;
                    bookmark.VesselName = vessel.vesselName;
                    bookmark.VesselType = vessel.vesselType.ToString();

                    bookmark.VesselSituation = vessel.situation.ToString();
                    bookmark.VesselBodyName = vessel.mainBody?.bodyName ?? "";
                    bookmark.VesselSituationLabel = GetSituationLabel(bookmark.VesselBodyName, vessel.situation);

                    bookmark.HasAlarm = CheckHasAlarm(vessel, index);

                    if( bookmark is CommandModuleBookmark ) {
                        // Nothing more
                    } else if( bookmark is VesselBookmark vesselBookmark ) {
                        vesselBookmark.BookmarkTitle = vesselBookmark.VesselName;
                        vesselBookmark.BookmarkVesselType = vesselBookmark.VesselType;
                    }
                }
                if( string.IsNullOrEmpty(bookmark.BookmarkTitle)) {
                    bookmark.BookmarkTitle = ModLocalization.GetString("labelModuleNotFound");
                }

                return true;
            } catch (Exception e) {
                LOGGER.LogError($"Error refreshing bookmark {bookmark}: {e.Message}");
                return false;
            }
        }
    }
}
