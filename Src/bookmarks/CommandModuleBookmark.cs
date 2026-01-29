using System;
using UnityEngine;
using com.github.lhervier.ksp;

namespace com.github.lhervier.ksp.bookmarksmod.bookmarks {
    public class CommandModuleBookmark : Bookmark {
        
        /// <summary>
        /// Unique identifier of the command module (uses Part.flightID)
        /// </summary>
        public uint CommandModuleFlightID { get; set; }
        
        /// <summary>
        /// Command module name (updated dynamically)
        /// </summary>
        public string CommandModuleName { get; set; }

        /// <summary>
        /// Command module type
        /// </summary>
        public VesselType CommandModuleType { get; set; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="commandModuleFlightID">The flight ID of the command module</param>
        public CommandModuleBookmark(uint commandModuleFlightID) : base() {
            if( commandModuleFlightID == 0 ) {
                throw new Exception("commandModuleFlightID cannot be 0");
            }
            CommandModuleFlightID = commandModuleFlightID;
            CommandModuleName = "";
            CommandModuleType = VesselType.Unknown;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="node">The ConfigNode to load the bookmark from</param>
        public CommandModuleBookmark(ConfigNode node) : base(node) {
        }

        /// <summary>
        /// Get the unique identifier of the bookmark
        /// </summary>
        /// <returns>The unique identifier of the bookmark</returns>
        public override uint GetBookmarkID() {
            return CommandModuleFlightID;
        }

        /// <summary>
        /// Returns the type of the bookmark
        /// </summary>
        /// <returns>The type of the bookmark</returns>
        public override BookmarkType GetBookmarkType() {
            return BookmarkType.CommandModule;
        }

        /// <summary>
        /// Saves the specific data of the command module bookmark to a ConfigNode
        /// </summary>
        /// <param name="node">The ConfigNode to save the specific data to</param>
        protected override void SaveSpecificData(ConfigNode node) {
            node.AddValue("commandModuleFlightID", CommandModuleFlightID);
            node.AddValue("commandModuleName", CommandModuleName);
            node.AddValue("commandModuleType", (int) CommandModuleType);
        }

        /// <summary>
        /// Loads the specific data of the command module bookmark from a ConfigNode
        /// </summary>
        /// <param name="node">The ConfigNode to load the specific data from</param>
        protected override void LoadSpecificData(ConfigNode node) {
            if (node.HasValue("commandModuleFlightID")) {
                uint.TryParse(node.GetValue("commandModuleFlightID"), out uint flightID);
                CommandModuleFlightID = flightID;
            } else {
                throw new Exception("commandModuleFlightID not found in the bookmark node");
            }

            CommandModuleName = node.GetValue("commandModuleName") ?? "";
            
            if (node.HasValue("commandModuleType")) {
                int.TryParse(node.GetValue("commandModuleType"), out int vesselType);
                CommandModuleType = (VesselType) vesselType;
            } else {
                throw new Exception("commandModuleType not found in the bookmark node");
            }
        }

        /// <summary>
        /// Get command module part for a command module
        /// </summary>
        /// <param name="commandModuleFlightID"></param>
        /// <returns>The command module part, or null if not found</returns>
        private Part GetPart(uint commandModuleFlightID) {
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
        private ProtoPartSnapshot GetProtoPartSnapshot(uint commandModuleFlightID) {
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
        /// Refresh the command module bookmark
        /// </summary>
        /// <returns>True if the command module bookmark was refreshed, false otherwise</returns>
        protected override bool RefreshSpecific() {
            try {
                uint persistentId;
                string cmName;
                VesselType cmType;
                
                Part commandModulePart = GetPart(CommandModuleFlightID);
                if (commandModulePart != null) {
                    persistentId = commandModulePart.vessel.persistentId;
                    if( commandModulePart.vesselNaming == null ) {
                        cmName = commandModulePart.vessel.vesselName;
                        cmType = commandModulePart.vessel.vesselType;
                    } else {
                        cmName = commandModulePart.vesselNaming.vesselName;
                        cmType = commandModulePart.vesselNaming.vesselType;
                    }
                } else {
                    ProtoPartSnapshot commandModuleProtoPartSnapshot = GetProtoPartSnapshot(CommandModuleFlightID);
                    if (commandModuleProtoPartSnapshot != null) {
                        persistentId = commandModuleProtoPartSnapshot.pVesselRef.persistentId;
                        if( commandModuleProtoPartSnapshot.vesselNaming == null ) {
                            cmName = commandModuleProtoPartSnapshot.pVesselRef.vesselName;
                            cmType = commandModuleProtoPartSnapshot.pVesselRef.vesselType;
                        } else {
                            cmName = commandModuleProtoPartSnapshot.vesselNaming.vesselName;
                            cmType = commandModuleProtoPartSnapshot.vesselNaming.vesselType;
                        }
                    } else {
                        ModLogger.LogWarning($"Bookmark {CommandModuleFlightID}: Command module part or protoPartSnapshot not found");
                        return false;
                    }
                }
                
                VesselPersistentID = persistentId;
                CommandModuleName = cmName;
                CommandModuleType = cmType;
                
                return true;
            } catch (Exception e) {
                ModLogger.LogError($"Error refreshing command module bookmark for bookmark {GetBookmarkID()}: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get the display name of the command module bookmark
        /// </summary>
        /// <returns>The display name of the command module bookmark</returns>
        public override string GetBookmarkTitle() {
            return CommandModuleName;
        }

        /// <summary>
        /// Get the display type of the command module bookmark
        /// </summary>
        /// <returns>The display type of the command module bookmark</returns>
        public override VesselType GetBookmarkDisplayType() {
            return CommandModuleType;
        }

        /// <summary>
        /// Should draw the part of the command module bookmark
        /// </summary>
        /// <returns>True if the part of the command module bookmark should be drawn, false otherwise</returns>
        public override bool ShouldDrawPartOf() {
            return VesselName != CommandModuleName;
        }
    }
}