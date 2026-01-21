using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.github.lhervier.ksp {
    
    /// <summary>
    /// Central bookmark manager
    /// </summary>
    public class VesselBookmarkManager {
        
        private static VesselBookmarkManager _instance;
        public static VesselBookmarkManager Instance {
            get {
                if (_instance == null) {
                    _instance = new VesselBookmarkManager();
                }
                return _instance;
            }
        }
        
        private List<VesselBookmark> _bookmarks = new List<VesselBookmark>();
        private const string SAVE_NODE_NAME = "VESSEL_BOOKMARKS";
        
        private VesselBookmarkManager() {
            // Subscribe to save/load events
            GameEvents.onGameStateCreated.Add(OnGameStateCreated);
            GameEvents.onGameStateLoad.Add(OnGameStateLoad);
            GameEvents.onGameStatePostLoad.Add(OnGameStatePostLoad);
            GameEvents.onGameStateSave.Add(OnGameStateSave);
            GameEvents.onGameStateSaved.Add(OnGameStateSaved);
        }
        
        /// <summary>
        /// List of all bookmarks
        /// </summary>
        public IReadOnlyList<VesselBookmark> Bookmarks => _bookmarks.AsReadOnly();
        
        private void LogConfigNode(ConfigNode configNode) {
            ModLogger.LogDebug($"Logging config node :");
            if( configNode == null ) {
                ModLogger.LogDebug($"  => No config node to log");
                return;
            }

            ConfigNode[] allNodes = configNode.GetNodes();
            ModLogger.LogDebug($"  => Nodes present in the config {configNode.name}: {allNodes.Length}");
            foreach( ConfigNode node in allNodes ) {
                ModLogger.LogDebug($"    - Node: {node.name}");
            }
        }

        private void LogBookmarks(ConfigNode configNode) {
            ModLogger.LogDebug($"Logging bookmarks present in the config node :");
            
            if( configNode == null ) {
                ModLogger.LogDebug($"  => No config node to log bookmarks from");
                return;
            }

            ConfigNode[] bookmarksNodes = configNode.GetNodes(SAVE_NODE_NAME);
            if (bookmarksNodes == null || bookmarksNodes.Length == 0) {
                ModLogger.LogDebug($"  => Bookmarks configuration node not present in the current game");
                return;
            }

            if( bookmarksNodes.Length > 1 ) {
                ModLogger.LogWarning($"  !! Multiple bookmarks configuration nodes present in the current game. Using first...");
            }

            ConfigNode bookmarksNode = bookmarksNodes[0];
            ConfigNode[] bookmarkNodes = bookmarksNode.GetNodes("BOOKMARK");

            if (bookmarkNodes == null || bookmarkNodes.Length == 0) {
                ModLogger.LogDebug($"  => No bookmarks nodes present in the saved game");
                return;
            }

            foreach (ConfigNode bookmarkNode in bookmarkNodes) {
                ModLogger.LogDebug($"  - Bookmark node: {bookmarkNode.name}");
            }
        }

        private void LoadBookmarks(ConfigNode node) {
            ModLogger.LogDebug($"Loading bookmarks from config node");
            try {
                _bookmarks.Clear();
                
                if (!node.HasNode(SAVE_NODE_NAME)) {
                    return;
                }
                ConfigNode bookmarksNode = node.GetNode(SAVE_NODE_NAME);
                ConfigNode[] bookmarkNodes = bookmarksNode.GetNodes("BOOKMARK");
                
                foreach (ConfigNode bookmarkNode in bookmarkNodes) {
                    VesselBookmark bookmark = new VesselBookmark();
                    try {
                        bookmark.Load(bookmarkNode);
                        _bookmarks.Add(bookmark);
                    } catch (Exception e) {
                        ModLogger.LogError($"Error loading bookmark: {e.Message}");
                    }
                }
            } finally {
                ModLogger.LogDebug($"{_bookmarks.Count} bookmark(s) loaded");
            }
        }

        private void SaveBookmarks(ConfigNode node) {
            ModLogger.LogDebug($"Saving bookmarks to config node");

            try {
                // Remove old node if it exists
                if (node.HasNode(SAVE_NODE_NAME)) {
                    node.RemoveNode(SAVE_NODE_NAME);
                }
                
                ConfigNode bookmarksNode = node.AddNode(SAVE_NODE_NAME);
                
                foreach (VesselBookmark bookmark in _bookmarks) {
                    ConfigNode bookmarkNode = bookmarksNode.AddNode("BOOKMARK");
                    bookmark.Save(bookmarkNode);
                }
            } finally {
                ModLogger.LogDebug($"{_bookmarks.Count} bookmark(s) saved");
            }
        }

        // =====================================================================================

        private void OnGameStateCreated(Game game) {
            ModLogger.LogDebug("======================> On Game state created");
            LogConfigNode(game.config);
            LogBookmarks(game.config);

            LoadBookmarks(game.config);
        }

        private void OnGameStateLoad(ConfigNode node) {
            ModLogger.LogDebug("======================> On Game state load");
            LogConfigNode(node);
            LogBookmarks(node);

            LoadBookmarks(node);
        }

        /// <summary>
        /// Save bookmarks to save file
        /// </summary>
        private void OnGameStateSave(ConfigNode node) {
            ModLogger.LogDebug("======================> On Game state save");
            LogConfigNode(node);
            LogBookmarks(node);

            SaveBookmarks(node);
        }
        
        private void OnGameStateSaved(Game game) {
            ModLogger.LogDebug("======================> On Game state saved");
            LogConfigNode(game.config);
            LogBookmarks(game.config);
        }

        /// <summary>
        /// Load bookmarks from save file
        /// </summary>
        private void OnGameStatePostLoad(ConfigNode node) {
            ModLogger.LogDebug("======================> On Game state post load");

            // In this event, the node is the parent of the "game" node
            ConfigNode gameNode = node.GetNode("GAME");

            LogConfigNode(gameNode);
            LogBookmarks(gameNode);
        }
        
        // =====================================================================================
        
        /// <summary>
        /// Add a bookmark for a command module
        /// </summary>
        public bool AddBookmark(Part commandModulePart) {
            if (commandModulePart == null) {
                ModLogger.LogError("Attempted to add bookmark with null part");
                return false;
            }
            
            uint flightID = commandModulePart.flightID;
            
            // Check if bookmark already exists
            if (_bookmarks.Any(b => b.CommandModuleFlightID == flightID)) {
                ModLogger.LogWarning($"Bookmark already exists for flightID {flightID}");
                return false;
            }
            
            VesselBookmark bookmark = new VesselBookmark(flightID);
            
            // Update command module name
            bookmark.CommandModuleName = GetCommandModuleName(commandModulePart);
            
            _bookmarks.Add(bookmark);
            ModLogger.LogDebug($"Bookmark added for flightID {flightID}");
            return true;
        }
        
        /// <summary>
        /// Remove a bookmark
        /// </summary>
        public bool RemoveBookmark(uint commandModuleFlightID) {
            VesselBookmark bookmark = _bookmarks.FirstOrDefault(b => b.CommandModuleFlightID == commandModuleFlightID);
            if (bookmark != null) {
                _bookmarks.Remove(bookmark);
                ModLogger.LogDebug($"Bookmark removed for flightID {commandModuleFlightID}");
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Check if a bookmark exists for a command module
        /// </summary>
        public bool HasBookmark(Part commandModulePart) {
            if (commandModulePart == null) return false;
            return _bookmarks.Any(b => b.CommandModuleFlightID == commandModulePart.flightID);
        }
        
        /// <summary>
        /// Get the current vessel for a bookmark (handles docked vessels)
        /// Searches in both loaded and unloaded vessels
        /// </summary>
        public Vessel GetVesselForBookmark(VesselBookmark bookmark) {
            if (bookmark == null) return null;
            
            try {
                // Search in all loaded vessels
                foreach (Vessel vessel in FlightGlobals.Vessels) {
                    if (vessel == null || vessel.parts == null) continue;
                    
                    try {
                        // Search for command module with matching flightID
                        foreach (Part part in vessel.parts) {
                            if (part == null) continue;
                            
                            try {
                                if (part.flightID == bookmark.CommandModuleFlightID) {
                                    // Found! Return the vessel (part.vessel already points to the composite vessel if docked)
                                    return part.vessel;
                                }
                            } catch (System.Exception e) {
                                ModLogger.LogWarning($"Error checking part {part.name}: {e.Message}");
                                continue;
                            }
                        }
                    } catch (System.Exception e) {
                        ModLogger.LogWarning($"Error checking vessel {vessel.vesselName}: {e.Message}");
                        continue;
                    }
                }
                
                // If not found in loaded vessels, search in unloaded vessels
                foreach (Vessel vessel in FlightGlobals.VesselsUnloaded) {
                    if (vessel == null || vessel.protoVessel == null) continue;
                    
                    try {
                        // Search for command module with matching flightID in protoPartSnapshots
                        foreach (ProtoPartSnapshot protoPart in vessel.protoVessel.protoPartSnapshots) {
                            if (protoPart == null) continue;
                            
                            try {
                                if (protoPart.flightID == bookmark.CommandModuleFlightID) {
                                    // Found! Return the vessel
                                    return vessel;
                                }
                            } catch (System.Exception e) {
                                ModLogger.LogWarning($"Error checking protoPart {protoPart.partName}: {e.Message}");
                                continue;
                            }
                        }
                    } catch (System.Exception e) {
                        ModLogger.LogWarning($"Error checking unloaded vessel {vessel.vesselName}: {e.Message}");
                        continue;
                    }
                }
            } catch (System.Exception e) {
                ModLogger.LogError($"Error searching for vessel for bookmark: {e.Message}");
            }
            
            return null;
        }
        
        /// <summary>
        /// Get command module for a bookmark
        /// </summary>
        public Part GetCommandModuleForBookmark(VesselBookmark bookmark) {
            if (bookmark == null) return null;
            
            try {
                // Search in all loaded vessels
                foreach (Vessel vessel in FlightGlobals.Vessels) {
                    if (vessel == null || vessel.parts == null) continue;
                    
                    try {
                        // Search for command module with matching flightID
                        foreach (Part part in vessel.parts) {
                            if (part == null) continue;
                            
                            try {
                                if (part.flightID == bookmark.CommandModuleFlightID) {
                                    // Verify it's actually a command module
                                    ModuleCommand commandModule = part.FindModuleImplementing<ModuleCommand>();
                                    if (commandModule != null) {
                                        return part;
                                    }
                                }
                            } catch (System.Exception e) {
                                ModLogger.LogWarning($"Error checking part {part.name}: {e.Message}");
                                continue;
                            }
                        }
                    } catch (System.Exception e) {
                        ModLogger.LogWarning($"Error checking vessel: {e.Message}");
                        continue;
                    }
                }
            } catch (System.Exception e) {
                ModLogger.LogError($"Error searching for command module for bookmark: {e.Message}");
            }
            
            return null;
        }
        
        /// <summary>
        /// Get command module name
        /// </summary>
        private string GetCommandModuleName(Part commandModulePart) {
            if (commandModulePart == null) return "Module not found";
            
            try {
                // Use part name (partInfo.title)
                if (commandModulePart.partInfo != null && !string.IsNullOrEmpty(commandModulePart.partInfo.title)) {
                    return commandModulePart.partInfo.title;
                }
                
                // Otherwise use part name
                if (!string.IsNullOrEmpty(commandModulePart.partName)) {
                    return commandModulePart.partName;
                }
                
                return "Command Module";
            } catch (System.Exception e) {
                ModLogger.LogWarning($"Error retrieving command module name: {e.Message}");
                return "Command Module";
            }
        }
        
        /// <summary>
        /// Update command module names for all bookmarks
        /// </summary>
        public void UpdateCommandModuleNames() {
            try {
                foreach (VesselBookmark bookmark in _bookmarks) {
                    if (bookmark == null) continue;
                    
                    try {
                        Part commandModulePart = GetCommandModuleForBookmark(bookmark);
                        if (commandModulePart != null) {
                            bookmark.CommandModuleName = GetCommandModuleName(commandModulePart);
                        } else {
                            // Command module is not loaded or no longer exists
                            if (string.IsNullOrEmpty(bookmark.CommandModuleName)) {
                                bookmark.CommandModuleName = "Module not found";
                            }
                        }
                    } catch (System.Exception e) {
                        ModLogger.LogWarning($"Error updating name for bookmark: {e.Message}");
                        if (string.IsNullOrEmpty(bookmark.CommandModuleName)) {
                            bookmark.CommandModuleName = "Error";
                        }
                    }
                }
            } catch (System.Exception e) {
                ModLogger.LogError($"Error updating command module names: {e.Message}");
            }
        }
        
        /// <summary>
        /// Update vessel names for all bookmarks (deprecated, use UpdateCommandModuleNames)
        /// </summary>
        [System.Obsolete("Use UpdateCommandModuleNames() instead")]
        public void UpdateVesselNames() {
            UpdateCommandModuleNames();
        }
        
        /// <summary>
        /// Clean up bookmarks for vessels that no longer exist
        /// </summary>
        public void CleanupInvalidBookmarks() {
            List<VesselBookmark> toRemove = new List<VesselBookmark>();
            
            try {
                foreach (VesselBookmark bookmark in _bookmarks) {
                    if (bookmark == null) {
                        toRemove.Add(bookmark);
                        continue;
                    }
                    
                    try {
                        Vessel vessel = GetVesselForBookmark(bookmark);
                        if (vessel == null) {
                            // Check if vessel exists in unloaded vessels
                            // (in Tracking Station for example)
                            bool found = false;
                            try {
                                foreach (Vessel v in FlightGlobals.VesselsUnloaded) {
                                    if (v == null || v.parts == null) continue;
                                    
                                    try {
                                        foreach (Part p in v.parts) {
                                            if (p != null && p.flightID == bookmark.CommandModuleFlightID) {
                                                found = true;
                                                break;
                                            }
                                        }
                                    } catch (System.Exception e) {
                                        ModLogger.LogWarning($"Error checking unloaded vessel parts: {e.Message}");
                                        continue;
                                    }
                                    
                                    if (found) break;
                                }
                            } catch (System.Exception e) {
                                ModLogger.LogWarning($"Error checking unloaded vessels: {e.Message}");
                            }
                            
                            if (!found) {
                                toRemove.Add(bookmark);
                            }
                        }
                    } catch (System.Exception e) {
                        ModLogger.LogWarning($"Error checking bookmark: {e.Message}");
                        // Don't remove on error, will retry later
                    }
                }
                
                foreach (VesselBookmark bookmark in toRemove) {
                    _bookmarks.Remove(bookmark);
                    ModLogger.LogDebug($"Bookmark cleaned up (vessel not found): {bookmark?.CommandModuleFlightID ?? 0}");
                }
            } catch (System.Exception e) {
                ModLogger.LogError($"Error cleaning up bookmarks: {e.Message}");
            }
        }
    }
}
