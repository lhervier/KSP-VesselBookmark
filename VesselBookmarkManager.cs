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
            GameEvents.onGameStateLoad.Add(OnGameStateLoad);
            GameEvents.onGameStateSave.Add(OnGameStateSave);
        }
        
        /// <summary>
        /// List of all bookmarks
        /// </summary>
        public IReadOnlyList<VesselBookmark> Bookmarks => _bookmarks.AsReadOnly();
        
        /// <summary>
        /// Load bookmarks from save file
        /// </summary>
        private void OnGameStateLoad(ConfigNode node) {
            _bookmarks.Clear();
            
            if (node.HasNode(SAVE_NODE_NAME)) {
                ConfigNode bookmarksNode = node.GetNode(SAVE_NODE_NAME);
                ConfigNode[] bookmarkNodes = bookmarksNode.GetNodes("BOOKMARK");
                
                foreach (ConfigNode bookmarkNode in bookmarkNodes) {
                    VesselBookmark bookmark = new VesselBookmark();
                    try {
                        bookmark.Load(bookmarkNode);
                        _bookmarks.Add(bookmark);
                    } catch (Exception e) {
                        Debug.LogError($"[VesselBookmarkMod] Error loading bookmark: {e.Message}");
                    }
                }
            }
            
            // Update command module names
            UpdateCommandModuleNames();
            
            Debug.Log($"[VesselBookmarkMod] {_bookmarks.Count} bookmark(s) loaded");
        }
        
        /// <summary>
        /// Save bookmarks to save file
        /// </summary>
        private void OnGameStateSave(ConfigNode node) {
            // Remove old node if it exists
            if (node.HasNode(SAVE_NODE_NAME)) {
                node.RemoveNode(SAVE_NODE_NAME);
            }
            
            ConfigNode bookmarksNode = node.AddNode(SAVE_NODE_NAME);
            
            foreach (VesselBookmark bookmark in _bookmarks) {
                ConfigNode bookmarkNode = bookmarksNode.AddNode("BOOKMARK");
                bookmark.Save(bookmarkNode);
            }
            
            Debug.Log($"[VesselBookmarkMod] {_bookmarks.Count} bookmark(s) saved");
        }
        
        /// <summary>
        /// Add a bookmark for a command module
        /// </summary>
        public bool AddBookmark(Part commandModulePart) {
            if (commandModulePart == null) {
                Debug.LogError("[VesselBookmarkMod] Attempted to add bookmark with null part");
                return false;
            }
            
            uint flightID = commandModulePart.flightID;
            
            // Check if bookmark already exists
            if (_bookmarks.Any(b => b.CommandModuleFlightID == flightID)) {
                Debug.LogWarning($"[VesselBookmarkMod] Bookmark already exists for flightID {flightID}");
                return false;
            }
            
            VesselBookmark bookmark = new VesselBookmark(flightID);
            
            // Update command module name
            bookmark.CommandModuleName = GetCommandModuleName(commandModulePart);
            
            _bookmarks.Add(bookmark);
            Debug.Log($"[VesselBookmarkMod] Bookmark added for flightID {flightID}");
            return true;
        }
        
        /// <summary>
        /// Remove a bookmark
        /// </summary>
        public bool RemoveBookmark(uint commandModuleFlightID) {
            VesselBookmark bookmark = _bookmarks.FirstOrDefault(b => b.CommandModuleFlightID == commandModuleFlightID);
            if (bookmark != null) {
                _bookmarks.Remove(bookmark);
                Debug.Log($"[VesselBookmarkMod] Bookmark removed for flightID {commandModuleFlightID}");
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
                                    // Found! Return root vessel (handles docked vessels)
                                    return FindRootVessel(part);
                                }
                            } catch (System.Exception e) {
                                Debug.LogWarning($"[VesselBookmarkMod] Error checking part {part.name}: {e.Message}");
                                continue;
                            }
                        }
                    } catch (System.Exception e) {
                        Debug.LogWarning($"[VesselBookmarkMod] Error checking vessel {vessel.vesselName}: {e.Message}");
                        continue;
                    }
                }
            } catch (System.Exception e) {
                Debug.LogError($"[VesselBookmarkMod] Error searching for vessel for bookmark: {e.Message}");
            }
            
            return null;
        }
        
        /// <summary>
        /// Find root vessel from a part (handles docked vessels)
        /// </summary>
        public Vessel FindRootVessel(Part part) {
            if (part == null) return null;
            
            try {
                // If part has a vessel, use rootPart to find root vessel
                if (part.vessel != null) {
                    // The vessel's rootPart points to the root part of the composite vessel
                    Part rootPart = part.vessel.rootPart;
                    if (rootPart != null && rootPart.vessel != null) {
                        return rootPart.vessel;
                    }
                    return part.vessel;
                }
            } catch (System.Exception e) {
                Debug.LogWarning($"[VesselBookmarkMod] Error finding root vessel: {e.Message}");
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
                                Debug.LogWarning($"[VesselBookmarkMod] Error checking part {part.name}: {e.Message}");
                                continue;
                            }
                        }
                    } catch (System.Exception e) {
                        Debug.LogWarning($"[VesselBookmarkMod] Error checking vessel: {e.Message}");
                        continue;
                    }
                }
            } catch (System.Exception e) {
                Debug.LogError($"[VesselBookmarkMod] Error searching for command module for bookmark: {e.Message}");
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
                Debug.LogWarning($"[VesselBookmarkMod] Error retrieving command module name: {e.Message}");
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
                        Debug.LogWarning($"[VesselBookmarkMod] Error updating name for bookmark: {e.Message}");
                        if (string.IsNullOrEmpty(bookmark.CommandModuleName)) {
                            bookmark.CommandModuleName = "Error";
                        }
                    }
                }
            } catch (System.Exception e) {
                Debug.LogError($"[VesselBookmarkMod] Error updating command module names: {e.Message}");
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
                                        Debug.LogWarning($"[VesselBookmarkMod] Error checking unloaded vessel parts: {e.Message}");
                                        continue;
                                    }
                                    
                                    if (found) break;
                                }
                            } catch (System.Exception e) {
                                Debug.LogWarning($"[VesselBookmarkMod] Error checking unloaded vessels: {e.Message}");
                            }
                            
                            if (!found) {
                                toRemove.Add(bookmark);
                            }
                        }
                    } catch (System.Exception e) {
                        Debug.LogWarning($"[VesselBookmarkMod] Error checking bookmark: {e.Message}");
                        // Don't remove on error, will retry later
                    }
                }
                
                foreach (VesselBookmark bookmark in toRemove) {
                    _bookmarks.Remove(bookmark);
                    Debug.Log($"[VesselBookmarkMod] Bookmark cleaned up (vessel not found): {bookmark?.CommandModuleFlightID ?? 0}");
                }
            } catch (System.Exception e) {
                Debug.LogError($"[VesselBookmarkMod] Error cleaning up bookmarks: {e.Message}");
            }
        }
    }
}
