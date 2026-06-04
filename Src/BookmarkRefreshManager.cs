using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using com.github.lhervier.ksp.bookmarksmod.bookmarks;

namespace com.github.lhervier.ksp.bookmarksmod {
    public class BookmarkRefreshManager : MonoBehaviour {

        private static readonly ModLogger LOGGER = new ModLogger("BookmarkRefreshManager");

        public static BookmarkRefreshManager Instance => _instance;
        private static BookmarkRefreshManager _instance = null;

        private VesselsManager _vesselsManager = null;

        // ===================================================================================
        // Life cycle
        // ===================================================================================

        public void Awake()
        {
            _instance = this;
        }

        public void Initialize(VesselsManager vesselsManager)
        {
            this._vesselsManager = vesselsManager;
        }

        public void Start()
        {

        }

        public void OnDestroy()
        {
            _instance = null;
        }

        // ===================================================================================
        //  Index lookups (O(1))
        // ===================================================================================

        /// <summary>
        /// Get the command module part for a flightID, or null if it is not a loaded command module.
        /// </summary>
        private Part GetPart(uint commandModuleFlightID) {
            if (!_vesselsManager.PartsByFlightId.TryGetValue(commandModuleFlightID, out Part part) || part == null) {
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
        private ProtoPartSnapshot GetProtoPartSnapshot(uint commandModuleFlightID) {
            if (!_vesselsManager.ProtoPartsByFlightId.TryGetValue(commandModuleFlightID, out ProtoPartSnapshot protoPart) || protoPart == null) {
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
        private Vessel FindVessel(Bookmark bookmark) {
            if (bookmark.VesselPersistentID == 0) {
                LOGGER.LogError($"Bookmark {bookmark}: Vessel persistent ID is empty");
                return null;
            }
            _vesselsManager.VesselsByPersistentId.TryGetValue(bookmark.VesselPersistentID, out Vessel vessel);
            return vessel;
        }

        /// <summary>
        /// Check if a vessel has an alarm.
        /// </summary>
        private bool CheckHasAlarm(Vessel vessel) {
            if (vessel == null) {
                return false;
            }
            return _vesselsManager.VesselsWithAlarm.Contains(vessel.persistentId);
        }

        /// <summary>
        /// Gets a textual description of vessel situation
        /// </summary>
        /// <param name="bodyName">The name of the body of the vessel</param>
        /// <param name="situation">The situation of the vessel</param>
        /// <returns>The label for the situation</returns>
        private string GetSituationLabel(string bodyName, Vessel.Situations situation) {
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
        private bool RefreshCommandModuleBookmark(CommandModuleBookmark bookmark) {
            try {
                bookmark.CommandModuleFlightID = bookmark.BookmarkID;

                uint vesselPersistentId;
                string cmName;
                string cmType;

                Part commandModulePart = GetPart(bookmark.CommandModuleFlightID);
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
                    ProtoPartSnapshot commandModuleProtoPartSnapshot = GetProtoPartSnapshot(bookmark.CommandModuleFlightID);
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
        /// Refresh a bookmark's transient (display) fields using a shared lookup index.
        /// </summary>
        public bool RefreshBookmark(Bookmark bookmark) {
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
                    if( !RefreshCommandModuleBookmark(commandModuleBookmark) ) {
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
                    vessel = FindVessel(bookmark);
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

                    bookmark.HasAlarm = CheckHasAlarm(vessel);

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
