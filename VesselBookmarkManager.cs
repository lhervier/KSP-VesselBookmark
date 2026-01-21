using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.github.lhervier.ksp {
    
    /// <summary>
    /// Central bookmark manager
    /// </summary>
    public class VesselBookmarkManager {
        
        /// <summary>
        /// Name of the node in the config file where bookmarks are saved
        /// </summary>
        private const string SAVE_NODE_NAME = "VESSEL_BOOKMARKS";
        
        /// <summary>
        /// Singleton instance
        /// </summary>
        private static VesselBookmarkManager _instance;
        public static VesselBookmarkManager Instance {
            get {
                if (_instance == null) {
                    _instance = new VesselBookmarkManager();
                }
                return _instance;
            }
        }
        
        /// <summary>
        /// List of all bookmarks
        /// </summary>
        private List<VesselBookmark> _bookmarks = new List<VesselBookmark>();
        public IReadOnlyList<VesselBookmark> Bookmarks => _bookmarks.AsReadOnly();

        /// <summary>
        /// List of available celestial bodies
        /// </summary>
        private List<CelestialBody> _availableBodies = new List<CelestialBody>();
        public IReadOnlyList<CelestialBody> AvailableBodies => _availableBodies.AsReadOnly();
        
        /// <summary>
        /// List of available vessel types
        /// </summary>
        private List<VesselType> _availableVesselTypes = new List<VesselType>();
        public IReadOnlyList<VesselType> AvailableVesselTypes => _availableVesselTypes.AsReadOnly();
        
        /// <summary>
        /// Load bookmarks from config node
        /// </summary>
        /// <param name="node"></param>
        public void LoadBookmarks(ConfigNode node) {
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

                RefreshBookmarks();
            } finally {
                ModLogger.LogDebug($"{_bookmarks.Count} bookmark(s) loaded");
            }
        }

        /// <summary>
        /// Save bookmarks to config node
        /// </summary>
        /// <param name="node"></param>
        public void SaveBookmarks(ConfigNode node) {
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
            if ( HasBookmark(commandModulePart) ) {
                ModLogger.LogWarning($"Bookmark already exists for flightID {flightID}");
                return false;
            }
            
            VesselBookmark bookmark = new VesselBookmark(flightID);
            
            // Update command module name and vessel type
            bookmark.CommandModuleName = GetCommandModuleName(commandModulePart);
            bookmark.VesselType = GetCommandModuleVesselType(commandModulePart);

            // Assign order (max + 1, or 0 if list is empty)
            if (_bookmarks.Count > 0) {
                bookmark.Order = _bookmarks.Max(b => b.Order) + 1;
            } else {
                bookmark.Order = 0;
            }
            
            _bookmarks.Add(bookmark);
            UpdateAvailableCelestialBodies();
            UpdateAvailableVesselTypes();
            ModLogger.LogDebug($"Bookmark added for flightID {flightID}");
            return true;
        }
        
        /// <summary>
        /// Remove a bookmark
        /// </summary>
        public bool RemoveBookmark(uint commandModuleFlightID) {
            VesselBookmark bookmark = _bookmarks.FirstOrDefault(b => b.CommandModuleFlightID == commandModuleFlightID);
            if (bookmark == null) {
                return false;
            } 
            _bookmarks.Remove(bookmark);
            UpdateAvailableCelestialBodies();
            UpdateAvailableVesselTypes();
            ModLogger.LogDebug($"Bookmark removed for flightID {commandModuleFlightID}");
            return true;
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
            if( commandModulePart.vesselNaming == null ) return "Part is not a CommandModule";
            return commandModulePart.vesselNaming.vesselName;
        }

        /// <summary>
        /// Get command module vessel type
        /// </summary>
        private VesselType GetCommandModuleVesselType(Part commandModulePart) {
            if (commandModulePart == null) return VesselType.Unknown;
            return commandModulePart.vesselNaming.vesselType;
        }
        
        /// <summary>
        /// Get filtered bookmarks by celestial body and/or vessel type
        /// </summary>
        public IEnumerable<VesselBookmark> GetFilteredBookmarks(CelestialBody bodyFilter, VesselType? typeFilter) {
            IEnumerable<VesselBookmark> filtered = _bookmarks;
            
            if (bodyFilter != null) {
                filtered = filtered.Where(b => {
                    Vessel vessel = GetVesselForBookmark(b);
                    return vessel != null && vessel.mainBody == bodyFilter;
                });
            }
            
            if (typeFilter.HasValue) {
                filtered = filtered.Where(b => {
                    Vessel vessel = GetVesselForBookmark(b);
                    return vessel != null && vessel.vesselType == typeFilter.Value;
                });
            }
            
            // Sort by Order, then by CreationTime
            return filtered.OrderBy(b => b.Order).ThenBy(b => b.CreationTime);
        }
        
        /// <summary>
        /// Move a bookmark up in the order (decrease Order value)
        /// </summary>
        public bool MoveBookmarkUp(VesselBookmark bookmark) {
            if (bookmark == null || !_bookmarks.Contains(bookmark)) {
                return false;
            }
            
            // Find the bookmark with the highest Order that is less than this one's Order
            VesselBookmark previous = _bookmarks
                .Where(b => b.Order < bookmark.Order)
                .OrderByDescending(b => b.Order)
                .FirstOrDefault();
            
            if (previous != null) {
                // Swap orders
                int temp = bookmark.Order;
                bookmark.Order = previous.Order;
                previous.Order = temp;
                return true;
            }
            
            return false; // Already at the top
        }
        
        /// <summary>
        /// Move a bookmark down in the order (increase Order value)
        /// </summary>
        public bool MoveBookmarkDown(VesselBookmark bookmark) {
            if (bookmark == null || !_bookmarks.Contains(bookmark)) {
                return false;
            }
            
            // Find the bookmark with the lowest Order that is greater than this one's Order
            VesselBookmark next = _bookmarks
                .Where(b => b.Order > bookmark.Order)
                .OrderBy(b => b.Order)
                .FirstOrDefault();
            
            if (next != null) {
                // Swap orders
                int temp = bookmark.Order;
                bookmark.Order = next.Order;
                next.Order = temp;
                return true;
            }
            
            return false; // Already at the bottom
        }
        
        /// <summary>
        /// Refresh command module names for all bookmarks
        /// </summary>
        public void RefreshBookmarks() {
            try {
                foreach (VesselBookmark bookmark in _bookmarks) {
                    if (bookmark == null) continue;
                    
                    try {
                        Part commandModulePart = GetCommandModuleForBookmark(bookmark);
                        if (commandModulePart != null) {
                            bookmark.CommandModuleName = GetCommandModuleName(commandModulePart);
                            bookmark.VesselType = GetCommandModuleVesselType(commandModulePart);
                        } else {
                            // Command module is not loaded or no longer exists
                            if (string.IsNullOrEmpty(bookmark.CommandModuleName)) {
                                bookmark.CommandModuleName = "Module not found";
                                bookmark.VesselType = VesselType.Unknown;
                            }
                        }
                    } catch (System.Exception e) {
                        ModLogger.LogWarning($"Error updating name for bookmark: {e.Message}");
                        if (string.IsNullOrEmpty(bookmark.CommandModuleName)) {
                            bookmark.CommandModuleName = "Error";
                        }
                    }
                }

                UpdateAvailableCelestialBodies();
                UpdateAvailableVesselTypes();
            } catch (System.Exception e) {
                ModLogger.LogError($"Error updating command module names and vessel types: {e.Message}");
            }
        }

        /// <summary>
        /// Get list of available celestial bodies from all bookmarks
        /// </summary>
        public void UpdateAvailableCelestialBodies() {
            HashSet<CelestialBody> bodies = new HashSet<CelestialBody>();
            
            foreach (VesselBookmark bookmark in _bookmarks) {
                Vessel vessel = GetVesselForBookmark(bookmark);
                if (vessel != null && vessel.mainBody != null) {
                    bodies.Add(vessel.mainBody);
                }
            }
            
            _availableBodies = bodies.OrderBy(b => b.displayName).ToList();
        }

        public void UpdateAvailableVesselTypes() {
            HashSet<VesselType> types = new HashSet<VesselType>();
            foreach (VesselBookmark bookmark in _bookmarks) {
                types.Add(bookmark.VesselType);
            }
            _availableVesselTypes = types.OrderBy(t => t.ToString()).ToList();
        }
    }
}
