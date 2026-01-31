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
                                ModLogger.LogDebug($"Command module part {part} found for flightID {commandModuleFlightID}");
                                return part;
                            } else {
                                ModLogger.LogError($"Bookmark {commandModuleFlightID}: Target part is not a command module");
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
                    ModLogger.LogDebug($"No unloaded vessels found. May happen when first loading a save game, or in other contexts... Another event should be emitted soon...");
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
                                ModLogger.LogDebug($"Command module protoPartSnapshot {protoPart} found for flightID {commandModuleFlightID}");
                                return protoPart;
                            } else {
                                ModLogger.LogError($"Command module protoPartSnapshot {protoPart} for flightID {commandModuleFlightID} is not a command module");
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
                ModLogger.LogDebug($"Getting vessel for bookmark {bookmark}");
                if( bookmark.VesselPersistentID == 0 ) {
                    ModLogger.LogError($"Bookmark {bookmark}: Vessel persistent ID is empty");
                    return null;
                }
                foreach (Vessel vessel in FlightGlobals.Vessels) {
                    if (vessel == null || vessel.persistentId != bookmark.VesselPersistentID) continue;
                    ModLogger.LogDebug($"Vessel {vessel} found for bookmark {bookmark} in loaded vessels");
                    return vessel;
                }
                foreach (Vessel vessel in FlightGlobals.VesselsUnloaded) {
                    if (vessel == null || vessel.persistentId != bookmark.VesselPersistentID) continue;
                    ModLogger.LogDebug($"Vessel {vessel} found for bookmark {bookmark} in unloaded vessels");
                    return vessel;
                }
                ModLogger.LogDebug($"No vessel found for bookmark {bookmark}");
                return null;
            } catch (Exception e) {
                ModLogger.LogError($"Error getting vessel for bookmark {bookmark}: {e.Message}");
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
                ModLogger.LogError($"Error getting situation{situation} for vessel on body {body.bodyName}: {e.Message}");
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
                        ModLogger.LogDebug($"Vessel {vessel.vesselName} has alarm {alarm.Id}");
                        return true;
                    }
                }
                ModLogger.LogDebug($"Vessel {vessel.vesselName} has no alarm");
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
                ModLogger.LogDebug($"Refreshing command module bookmark {bookmark}");
                bookmark.CommandModuleFlightID = bookmark.BookmarkID;
                
                uint vesselPersistentId;
                string cmName;
                VesselType cmType;
                
                Part commandModulePart = GetPart(bookmark.CommandModuleFlightID);
                if (commandModulePart != null) {
                    ModLogger.LogDebug($"- Command module part {bookmark.CommandModuleFlightID} found");
                    vesselPersistentId = commandModulePart.vessel.persistentId;
                    if( commandModulePart.vesselNaming == null ) {
                        ModLogger.LogDebug($"- Command module part {bookmark.CommandModuleFlightID} has no naming. Using vessel name and type from the vessel");
                        cmName = commandModulePart.vessel.vesselName;
                        cmType = commandModulePart.vessel.vesselType;
                    } else {
                        ModLogger.LogDebug($"- Command module part {bookmark.CommandModuleFlightID} has a naming. Using it...");
                        cmName = commandModulePart.vesselNaming.vesselName;
                        cmType = commandModulePart.vesselNaming.vesselType;
                    }
                } else {
                    ModLogger.LogDebug($"- Command module part {bookmark.CommandModuleFlightID} not found. Trying to get it from the protoPartSnapshot");
                    ProtoPartSnapshot commandModuleProtoPartSnapshot = GetProtoPartSnapshot(bookmark.CommandModuleFlightID);
                    if (commandModuleProtoPartSnapshot != null) {
                        ModLogger.LogDebug($"- Command module protoPartSnapshot {bookmark.CommandModuleFlightID} found");
                        vesselPersistentId = commandModuleProtoPartSnapshot.pVesselRef.persistentId;
                        if( commandModuleProtoPartSnapshot.vesselNaming == null ) {
                            ModLogger.LogDebug($"- Command module protoPartSnapshot {bookmark.CommandModuleFlightID} has no naming. Using name and type from the protoVessel");
                            cmName = commandModuleProtoPartSnapshot.pVesselRef.vesselName;
                            cmType = commandModuleProtoPartSnapshot.pVesselRef.vesselType;
                        } else {
                            ModLogger.LogDebug($"- Command module protoPartSnapshot {bookmark.CommandModuleFlightID} has a naming. Using it...");
                            cmName = commandModuleProtoPartSnapshot.vesselNaming.vesselName;
                            cmType = commandModuleProtoPartSnapshot.vesselNaming.vesselType;
                        }
                    } else {
                        ModLogger.LogDebug($"- Command module part or protoPartSnapshot {bookmark.CommandModuleFlightID} not found. May happen... Keeping value stored int the bookmark itself...");                        vesselPersistentId = bookmark.VesselPersistentID;
                        cmName = bookmark.VesselName;
                        cmType = bookmark.VesselType;
                    }
                }
                
                bookmark.VesselPersistentID = vesselPersistentId;
                bookmark.CommandModuleName = cmName;
                bookmark.CommandModuleType = cmType;

                bookmark.BookmarkTitle = bookmark.CommandModuleName;
                bookmark.BookmarkVesselType = bookmark.CommandModuleType;
                
                ModLogger.LogDebug($"Command module bookmark {bookmark} refreshed");
                return true;
            } catch (Exception e) {
                ModLogger.LogError($"Error refreshing command module bookmark {bookmark}: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Refresh command module names for all bookmarks
        /// </summary>
        public static bool RefreshBookmark(Bookmark bookmark) {
            try {
                ModLogger.LogDebug($"Refreshing bookmark {bookmark}");

                // Checking for mandatory values
                if( bookmark.BookmarkID == 0 ) {
                    ModLogger.LogWarning($"Bookmark {bookmark}: Bookmark ID is 0");
                    return false;
                }
                if( bookmark.BookmarkType == BookmarkType.Unknown ) {
                    ModLogger.LogWarning($"Bookmark {bookmark}: Bookmark type is unknown");
                    return false;
                }

                if( bookmark is CommandModuleBookmark commandModuleBookmark ) {
                    if( !RefreshCommandModuleBookmark(commandModuleBookmark) ) {
                        ModLogger.LogDebug($"Bookmark {commandModuleBookmark}: Failed to refresh command module bookmark. Let's continue with next one...");
                        return false;
                    }
                } else if( bookmark is VesselBookmark vesselBookmark ) {
                    vesselBookmark.VesselPersistentID = bookmark.BookmarkID;
                } else {
                    ModLogger.LogError($"Bookmark {bookmark}: Unknown bookmark type");
                    return false;
                }
                
                Vessel vessel = null;
                if( bookmark.VesselPersistentID != 0 ) {    // May happen if we were unable to refresh a command module bookmark.
                    vessel = FindVessel(bookmark);
                }
                if( vessel == null ) {
                    ModLogger.LogDebug($"Bookmark {bookmark}: Cannot refresh vessel bookmark properties: Vessel not found. May happen when first loading a save game, but not only... Another event should be emitted soon...");
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
                        vesselBookmark.BookmarkVesselType = vesselBookmark.VesselType;
                    }
                }

                ModLogger.LogDebug($"Bookmark {bookmark} refreshed");
                return true;
            } catch (Exception e) {
                ModLogger.LogError($"Error refreshing bookmark {bookmark}: {e.Message}");
                return false;
            }
        }
    }
}