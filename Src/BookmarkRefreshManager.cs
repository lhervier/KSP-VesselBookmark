using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using com.github.lhervier.ksp.bookmarksmod.bookmarks;

namespace com.github.lhervier.ksp.bookmarksmod {
    public class BookmarkRefreshManager {
        
        /// <summary>
        /// Get command module part for a command module
        /// </summary>
        /// <param name="commandModuleFlightID"></param>
        /// <returns>The command module part, or null if not found</returns>
        private static Part GetPart(uint commandModuleFlightID) {
            try {
                ModLogger.LogDebug($"Getting command module part for flightID {commandModuleFlightID}");
                foreach (Vessel vessel in FlightGlobals.Vessels) {
                    if (vessel == null || vessel.parts == null) continue;
                    foreach (Part part in vessel.parts) {
                        if (part == null) continue;
                        if (part.flightID == commandModuleFlightID) {
                            ModuleCommand commandModule = part.FindModuleImplementing<ModuleCommand>();
                            if (commandModule != null) {
                                return part;
                            } else {
                                ModLogger.LogWarning($"Bookmark {commandModuleFlightID}: Target part is not a command module");
                                return null;
                            }
                        }
                    }
                }
                return null;
            } catch (Exception e) {
                ModLogger.LogError($"Error getting command module part for flightID {commandModuleFlightID}: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get command module protoPartSnapshot for a command module
        /// </summary>
        /// <param name="commandModuleFlightID"></param>
        /// <returns>The command module protoPartSnapshot, or null if not found</returns>
        private static ProtoPartSnapshot GetProtoPartSnapshot(uint commandModuleFlightID) {
            try {
                ModLogger.LogDebug($"Getting command module protoPartSnapshot for flightID {commandModuleFlightID}");
                if( FlightGlobals.VesselsUnloaded == null || FlightGlobals.VesselsUnloaded.Count == 0 ) {
                    ModLogger.LogWarning($"Bookmark {commandModuleFlightID}: No unloaded vessels found. May happen when first loading a save game. Another event should be emitted soon...");
                    return null;
                }
                foreach (Vessel vessel in FlightGlobals.VesselsUnloaded) {
                    if (vessel == null ) continue;
                    if (vessel.protoVessel == null ) continue;
                    if (vessel.protoVessel.protoPartSnapshots == null) continue;
                    
                    foreach (ProtoPartSnapshot protoPart in vessel.protoVessel.protoPartSnapshots) {
                        if (protoPart == null) continue;
                        
                        if (protoPart.flightID == commandModuleFlightID) {
                            if (protoPart.FindModule("ModuleCommand") != null) {
                                return protoPart;
                            } else {
                                return null;
                            }
                        }
                    }
                }
                return null;
            } catch (Exception e) {
                ModLogger.LogError($"Error getting command module protoPartSnapshot for flightID {commandModuleFlightID}: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Find the vessel for the bookmark
        /// </summary>
        /// <returns>The vessel for the bookmark</returns>
        private static Vessel FindVessel(Bookmark bookmark) {
            try {
                ModLogger.LogDebug($"Getting vessel for bookmark {bookmark.BookmarkID}");
                if( bookmark.VesselPersistentID == 0 ) {
                    ModLogger.LogWarning($"Bookmark {bookmark.BookmarkID} and {bookmark.BookmarkType}: Vessel persistent ID is empty");
                    return null;
                }
                foreach (Vessel vessel in FlightGlobals.Vessels) {
                    if (vessel == null || vessel.persistentId != bookmark.VesselPersistentID) continue;
                    return vessel;
                }
                foreach (Vessel vessel in FlightGlobals.VesselsUnloaded) {
                    if (vessel == null || vessel.persistentId != bookmark.VesselPersistentID) continue;
                    return vessel;
                }
                return null;
            } catch (Exception e) {
                ModLogger.LogError($"Error getting vessel for bookmark {bookmark.BookmarkID} and {bookmark.BookmarkType}: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets a textual description of vessel situation
        /// </summary>
        /// <param name="body">The body of the vessel</param>
        /// <param name="situation">The situation of the vessel</param>
        /// <returns>The label for the situation</returns>
        private static string GetSituationLabel(CelestialBody body, Vessel.Situations situation) {
            try {
                if( body == null ) {
                    ModLogger.LogError($"Getting situation: Body is null");
                    return ModLocalization.GetString("situationUnknown");
                }
                ModLogger.LogDebug($"Getting situation labeled as {situation} for vessel on body {body.bodyName}");
                
                switch (situation) {
                    case Vessel.Situations.LANDED:
                        return ModLocalization.GetString("situationLanded", body.bodyName);
                        
                    case Vessel.Situations.SPLASHED:
                        return ModLocalization.GetString("situationSplashed", body.bodyName);
                        
                    case Vessel.Situations.PRELAUNCH:
                        return ModLocalization.GetString("situationPrelaunch", body.bodyName);
                        
                    case Vessel.Situations.SUB_ORBITAL:
                        return ModLocalization.GetString("situationSuborbital", body.bodyName);
                        
                    case Vessel.Situations.ORBITING:
                        return ModLocalization.GetString("situationOrbiting", body.bodyName);
                        
                    case Vessel.Situations.ESCAPING:
                        return ModLocalization.GetString("situationEscaping", body.bodyName);
                        
                    default:
                        return ModLocalization.GetString("situationInFlight", body.bodyName);
                }
            } catch (System.Exception e) {
                ModLogger.LogError($"Error getting situation labeled as {situation} for vessel on body {body.bodyName}: {e.Message}");
                return ModLocalization.GetString("situationUnknown");
            }
        }

        /// <summary>
        /// Check if a vessel has an alarm
        /// </summary>
        /// <param name="vessel">The vessel to check</param>
        /// <returns>True if the vessel has an alarm, false otherwise</returns>
        private static bool CheckHasAlarm(Vessel vessel) {
            try {
                if( vessel == null ) {
                    ModLogger.LogWarning($"Checking if vessel has an alarm: Vessel not found");
                    return false;
                }
                ModLogger.LogDebug($"Checking if vessel {vessel.vesselName} has an alarm");

                DictionaryValueList<uint, AlarmTypeBase> alarms = AlarmClockScenario.Instance.alarms;
                foreach (AlarmTypeBase alarm in alarms.Values) {
                    if( alarm.Vessel == null ) {
                        continue;
                    }
                    if( alarm.Vessel.persistentId == vessel.persistentId ) {
                        return true;
                    }
                }
                return false;
            } catch (Exception e) {
                ModLogger.LogError($"Error checking if vessel {vessel.vesselName} has an alarm: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Refresh a command module bookmark
        /// </summary>
        /// <param name="bookmark">The command module bookmark to refresh</param>
        /// <returns>True if the command module bookmark was refreshed, false otherwise</returns>
        private static bool RefreshCommandModuleBookmark(CommandModuleBookmark bookmark) {
            try {
                bookmark.CommandModuleFlightID = bookmark.BookmarkID;
                
                uint vesselPersistentId;
                string cmName;
                VesselType cmType;
                
                Part commandModulePart = GetPart(bookmark.CommandModuleFlightID);
                if (commandModulePart != null) {
                    vesselPersistentId = commandModulePart.vessel.persistentId;
                    if( commandModulePart.vesselNaming == null ) {
                        cmName = commandModulePart.vessel.vesselName;
                        cmType = commandModulePart.vessel.vesselType;
                    } else {
                        cmName = commandModulePart.vesselNaming.vesselName;
                        cmType = commandModulePart.vesselNaming.vesselType;
                    }
                } else {
                    ProtoPartSnapshot commandModuleProtoPartSnapshot = GetProtoPartSnapshot(bookmark.CommandModuleFlightID);
                    if (commandModuleProtoPartSnapshot != null) {
                        vesselPersistentId = commandModuleProtoPartSnapshot.pVesselRef.persistentId;
                        if( commandModuleProtoPartSnapshot.vesselNaming == null ) {
                            cmName = commandModuleProtoPartSnapshot.pVesselRef.vesselName;
                            cmType = commandModuleProtoPartSnapshot.pVesselRef.vesselType;
                        } else {
                            cmName = commandModuleProtoPartSnapshot.vesselNaming.vesselName;
                            cmType = commandModuleProtoPartSnapshot.vesselNaming.vesselType;
                        }
                    } else {
                        ModLogger.LogWarning($"Bookmark {bookmark.CommandModuleFlightID}: Command module part or protoPartSnapshot not found. May happen when first loading a save game. Another event should be emitted soon...");
                        vesselPersistentId = bookmark.VesselPersistentID;
                        cmName = bookmark.VesselName;
                        cmType = bookmark.VesselType;
                    }
                }
                
                bookmark.VesselPersistentID = vesselPersistentId;
                bookmark.CommandModuleName = cmName;
                bookmark.CommandModuleType = cmType;

                bookmark.BookmarkTitle = bookmark.CommandModuleName;
                
                return true;
            } catch (Exception e) {
                ModLogger.LogError($"Error refreshing command module bookmark {bookmark.BookmarkType} and {bookmark.BookmarkID}: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Refresh command module names for all bookmarks
        /// </summary>
        public static bool RefreshBookmark(Bookmark bookmark) {
            try {
                ModLogger.LogDebug($"Refreshing bookmark {bookmark.BookmarkType} and {bookmark.BookmarkID}");

                if( bookmark is CommandModuleBookmark commandModuleBookmark ) {
                    if( !RefreshCommandModuleBookmark(commandModuleBookmark) ) {
                        ModLogger.LogWarning($"Bookmark {commandModuleBookmark.BookmarkID}: Failed to refresh command module bookmark");
                        return false;
                    }
                } else if( bookmark is VesselBookmark vesselBookmark ) {
                    vesselBookmark.VesselPersistentID = bookmark.BookmarkID;
                } else {
                    ModLogger.LogWarning($"Bookmark {bookmark.BookmarkType} and {bookmark.BookmarkID}: Unknown bookmark type");
                    return false;
                }
                
                Vessel vessel = FindVessel(bookmark);
                if( vessel == null ) {
                    ModLogger.LogWarning($"Bookmark {bookmark.BookmarkID}: Cannot refresh vessel bookmark properties: Vessel not found. May happen when first loading a save game. Another event should be emitted soon...");
                } else {
                    bookmark.Vessel = vessel;
                    bookmark.VesselName = vessel.vesselName;
                    bookmark.VesselType = vessel.vesselType;
                    
                    bookmark.VesselSituation = vessel.situation;
                    bookmark.VesselBody = vessel.mainBody;
                    bookmark.VesselSituationLabel = GetSituationLabel(vessel.mainBody, vessel.situation);
                    
                    bookmark.HasAlarm = CheckHasAlarm(vessel);

                    if( bookmark is CommandModuleBookmark ) {
                        // Nothing more
                    } else if( bookmark is VesselBookmark vesselBookmark ) {
                        vesselBookmark.BookmarkTitle = vesselBookmark.VesselName;
                    }
                }

                return true;
            } catch (Exception e) {
                ModLogger.LogError($"Error refreshing bookmark {bookmark.BookmarkType} and {bookmark.BookmarkID}: {e.Message}");
                return false;
            }
        }
    }
}