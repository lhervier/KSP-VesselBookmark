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
        private const string BOOKMARK_NODE_NAME = "BOOKMARK";
        
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
        private List<uint> _bookmarksFlightIDs = new List<uint>();

        public readonly EventVoid OnBookmarksUpdated = new EventVoid("VesselBookmarkManager.OnBookmarksUpdated");
 
        // =======================================================================================

        /// <summary>
        /// Get a bookmark from the list
        /// </summary>
        /// <param name="commandModuleFlightID"></param>
        /// <returns></returns>
        public VesselBookmark GetBookmark(uint commandModuleFlightID) {
            try {
                ModLogger.LogDebug($"Getting bookmark for flightID {commandModuleFlightID}");
                return _bookmarks.First(b => b.CommandModuleFlightID == commandModuleFlightID);
            } catch (Exception e) {
                ModLogger.LogError($"Error getting bookmark for flightID {commandModuleFlightID}: {e.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Get a bookmark from the list
        /// </summary>
        /// <param name="commandModulePart"></param>
        /// <returns></returns>
        public VesselBookmark GetBookmark(Part commandModulePart) {
            try {
                if (commandModulePart == null) {
                    ModLogger.LogError("Attempted to get bookmark with null part");
                    return null;
                }
                ModLogger.LogDebug($"Getting bookmark for part {commandModulePart.flightID}");
                return this.GetBookmark(commandModulePart.flightID);
            } catch (Exception e) {
                ModLogger.LogError($"Error getting bookmark for part {commandModulePart.flightID}: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get a bookmark from the list
        /// </summary>
        /// <param name="protoPartSnapshot"></param>
        /// <returns></returns>
        public VesselBookmark GetBookmark(ProtoPartSnapshot protoPartSnapshot) {
            try {
                if (protoPartSnapshot == null) {
                    ModLogger.LogError("Attempted to get bookmark with null protoPartSnapshot");
                    return null;
                }
                ModLogger.LogDebug($"Getting bookmark for protoPartSnapshot {protoPartSnapshot.flightID}");
                return this.GetBookmark(protoPartSnapshot.flightID);
            } catch (Exception e) {
                ModLogger.LogError($"Error getting bookmark for protoPartSnapshot {protoPartSnapshot.flightID}: {e.Message}");
                return null;
            }
        }

        // =======================================================================================

        /// <summary>
        /// Check if a bookmark exists for a command module.
        /// Note: This method has no cost and can be called frequently.
        /// </summary>
        /// <param name="commandModuleFlightID"></param>
        /// <returns></returns>
        public bool HasBookmark(uint commandModuleFlightID) {
            try {
                return _bookmarksFlightIDs.Contains(commandModuleFlightID);
            } catch (Exception e) {
                ModLogger.LogError($"Error checking if bookmark exists for flightID {commandModuleFlightID}: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Check if a bookmark exists
        /// </summary>
        /// <param name="bookmark"></param>
        /// <returns></returns>
        public bool HasBookmark(VesselBookmark bookmark) {
            try {
                if (bookmark == null) {
                    ModLogger.LogError("Attempted to check if bookmark exists with null bookmark");
                    return false;
                }
                ModLogger.LogDebug($"Checking if bookmark exists for bookmark {bookmark.CommandModuleFlightID}");
                return this.HasBookmark(bookmark.CommandModuleFlightID);
            } catch (Exception e) {
                ModLogger.LogError($"Error checking if bookmark exists for bookmark {bookmark.CommandModuleFlightID}: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Check if a bookmark exists
        /// </summary>
        /// <param name="commandModulePart"></param>
        /// <returns></returns>
        public bool HasBookmark(Part commandModulePart) {
            try {
                if (commandModulePart == null) {
                    ModLogger.LogError("Attempted to check if bookmark exists with null part");
                    return false;
                }
                ModLogger.LogDebug($"Checking if bookmark exists for part {commandModulePart.flightID}");
                return this.HasBookmark(commandModulePart.flightID);
            } catch (Exception e) {
                ModLogger.LogError($"Error checking if bookmark exists for part {commandModulePart.flightID}: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Check if a bookmark exists
        /// </summary>
        /// <param name="protoPartSnapshot"></param>
        /// <returns></returns>
        public bool HasBookmark(ProtoPartSnapshot protoPartSnapshot) {
            try {
                if (protoPartSnapshot == null) {
                    ModLogger.LogError("Attempted to check if bookmark exists with null protoPartSnapshot");
                    return false;
                }
                ModLogger.LogDebug($"Checking if bookmark exists for protoPartSnapshot {protoPartSnapshot.flightID}");
                return this.HasBookmark(protoPartSnapshot.flightID);
            } catch (Exception e) {
                ModLogger.LogError($"Error checking if bookmark exists for protoPartSnapshot {protoPartSnapshot.flightID}: {e.Message}");
                return false;
            }
        }

        // =======================================================================================

        /// <summary>
        /// Add a bookmark for a command module
        /// </summary>
        /// <param name="commandModuleFlightID"></param>
        public bool AddBookmark(uint commandModuleFlightID) {
            try {
                ModLogger.LogDebug($"Adding bookmark for flightID {commandModuleFlightID}");
                VesselBookmark bookmark = new VesselBookmark(commandModuleFlightID);
                
                Part commandModulePart = this.GetPart(commandModuleFlightID);
                if (commandModulePart != null) {
                    bookmark.CommandModuleName = commandModulePart.vesselNaming.vesselName;
                    bookmark.VesselType = commandModulePart.vesselNaming.vesselType;
                }
                ProtoPartSnapshot commandModuleProtoPartSnapshot = this.GetProtoPartSnapshot(commandModuleFlightID);
                if (commandModuleProtoPartSnapshot != null) {
                    bookmark.CommandModuleName = commandModuleProtoPartSnapshot.vesselNaming.vesselName;
                    bookmark.VesselType = commandModuleProtoPartSnapshot.vesselNaming.vesselType;
                }

                return this.AddBookmark(bookmark);
            } catch (Exception e) {
                ModLogger.LogError($"Error adding bookmark for flightID {commandModuleFlightID}: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Add a bookmark to the list
        /// </summary>
        /// <param name="bookmark"></param>
        public bool AddBookmark(VesselBookmark bookmark) {
            try {
                if (bookmark == null) {
                    ModLogger.LogError("Attempted to add null bookmark");
                    return false;
                }
                ModLogger.LogDebug($"Adding bookmark for bookmark {bookmark.CommandModuleFlightID}");
                
                // Check if bookmark already exists
                if (this.HasBookmark(bookmark)) {
                    ModLogger.LogWarning($"Bookmark already exists for flightID {bookmark.CommandModuleFlightID}");
                    return false;
                }
                
                // Assign order (max + 1, or 0 if list is empty)
                if (_bookmarks.Count > 0) {
                    bookmark.Order = _bookmarks.Max(b => b.Order) + 1;
                } else {
                    bookmark.Order = 0;
                }

                _bookmarks.Add(bookmark);
                _bookmarksFlightIDs.Add(bookmark.CommandModuleFlightID);

                ModLogger.LogDebug($"Bookmark added for flightID {bookmark.CommandModuleFlightID}");
                OnBookmarksUpdated.Fire();
                return true;
            } catch (Exception e) {
                ModLogger.LogError($"Error adding bookmark: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Add a bookmark for a command module
        /// </summary>
        /// <param name="commandModulePart"></param>
        public bool AddBookmark(Part commandModulePart) {
            try {
                if (commandModulePart == null) {
                    ModLogger.LogError("Attempted to add bookmark with null part");
                    return false;
                }
                ModLogger.LogDebug($"Adding bookmark for part {commandModulePart.flightID}");
                
                VesselBookmark bookmark = new VesselBookmark(commandModulePart.flightID);
                bookmark.CommandModuleName = commandModulePart.vesselNaming.vesselName;
                bookmark.VesselType = commandModulePart.vesselNaming.vesselType;
                
                return this.AddBookmark(bookmark);
            } catch (Exception e) {
                ModLogger.LogError($"Error adding bookmark for part {commandModulePart.flightID}: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Add a bookmark for a protoPartSnapshot
        /// </summary>
        /// <param name="protoPartSnapshot"></param>
        public bool AddBookmark(ProtoPartSnapshot protoPartSnapshot) {
            try {
                if (protoPartSnapshot == null) {
                    ModLogger.LogError("Attempted to add bookmark with null protoPartSnapshot");
                    return false;
                }
                ModLogger.LogDebug($"Adding bookmark for protoPartSnapshot {protoPartSnapshot.flightID}");

                VesselBookmark bookmark = new VesselBookmark(protoPartSnapshot.flightID);
                bookmark.CommandModuleName = protoPartSnapshot.vesselNaming.vesselName;
                bookmark.VesselType = protoPartSnapshot.vesselNaming.vesselType;
                
                return this.AddBookmark(bookmark);
            } catch (Exception e) {
                ModLogger.LogError($"Error adding bookmark for protoPartSnapshot {protoPartSnapshot.flightID}: {e.Message}");
                return false;
            }
        }

        // =======================================================================================

        /// <summary>
        /// Remove a bookmark from the list
        /// </summary>
        /// <param name="commandModuleFlightID"></param>
        public bool RemoveBookmark(uint commandModuleFlightID) {
            try {
                ModLogger.LogDebug($"Removing bookmark for flightID {commandModuleFlightID}");
                VesselBookmark bookmark = this.GetBookmark(commandModuleFlightID);
                if (bookmark != null) {
                    return this.RemoveBookmark(bookmark);
                }
                return false;
            } catch (Exception e) {
                ModLogger.LogError($"Error removing bookmark for flightID {commandModuleFlightID}: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Remove a bookmark
        /// </summary>
        /// <param name="bookmark"></param>
        public bool RemoveBookmark(VesselBookmark bookmark) {
            try {
                if (bookmark == null) {
                    ModLogger.LogError("Attempted to remove null bookmark");
                    return false;
                }
                ModLogger.LogDebug($"Removing bookmark for bookmark {bookmark.CommandModuleFlightID}");

                _bookmarks.Remove(bookmark);
                _bookmarksFlightIDs.Remove(bookmark.CommandModuleFlightID);
                
                ModLogger.LogDebug($"Bookmark removed for flightID {bookmark.CommandModuleFlightID}");
                OnBookmarksUpdated.Fire();
                return true;
            } catch (Exception e) {
                ModLogger.LogError($"Error removing bookmark for bookmark {bookmark.CommandModuleFlightID}: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Remove a bookmark
        /// </summary>
        /// <param name="commandModulePart"></param>
        public bool RemoveBookmark(Part commandModulePart) {
            try {
                if (commandModulePart == null) {
                    ModLogger.LogError("Attempted to remove bookmark with null part");
                    return false;
                }
                ModLogger.LogDebug($"Removing bookmark for part {commandModulePart.flightID}");
                return this.RemoveBookmark(commandModulePart.flightID);
            } catch (Exception e) {
                ModLogger.LogError($"Error removing bookmark for part {commandModulePart.flightID}: {e.Message}");
                return false;
            }
        }

        // =======================================================================================

        /// <summary>
        /// Move a bookmark up in the order (decrease Order value)
        /// </summary>
        /// <param name="bookmark"></param>
        /// <returns>True if the bookmark was moved up, false otherwise</returns>
        public bool MoveBookmarkUp(uint commandModuleFlightID) {
            try {
                ModLogger.LogDebug($"Moving bookmark up for bookmark {commandModuleFlightID}");

                VesselBookmark bookmark = this.GetBookmark(commandModuleFlightID);
                if (bookmark == null) {
                    ModLogger.LogWarning($"Bookmark {commandModuleFlightID}: Not found");
                    return false;
                }

                if( bookmark.Order <= 0 ) {
                    ModLogger.LogError($"Bookmark {commandModuleFlightID} cannot be moved up: Order is {bookmark.Order}");
                    return false;
                }

                // Find the bookmark with previous Order
                VesselBookmark previous = _bookmarks.Find(b => b.Order == bookmark.Order - 1);
                if( previous == null ) {
                    ModLogger.LogWarning($"Bookmark {commandModuleFlightID}: Previous bookmark not found");
                    return false;
                }

                // Swap orders
                int temp = bookmark.Order;
                bookmark.Order = previous.Order;
                previous.Order = temp;
                
                this._bookmarks = this._bookmarks.OrderBy(b => b.Order).ToList();
                OnBookmarksUpdated.Fire();
                return true;
            } catch (Exception e) {
                ModLogger.LogError($"Error moving bookmark up for bookmark {commandModuleFlightID}: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Move a bookmark down in the order (increase Order value)
        /// </summary>
        public bool MoveBookmarkDown(uint commandModuleFlightID) {
            try {
                ModLogger.LogDebug($"Moving bookmark down for bookmark {commandModuleFlightID}");

                VesselBookmark bookmark = this.GetBookmark(commandModuleFlightID);
                if (bookmark == null) {
                    ModLogger.LogWarning($"Bookmark {commandModuleFlightID}: Not found");
                    return false;
                }

                if( bookmark.Order >= _bookmarks.Max(b => b.Order) ) {
                    ModLogger.LogWarning($"Bookmark {commandModuleFlightID} cannot be moved down: Order is {bookmark.Order}");
                    return false;
                }
                
                // Find the bookmark with next Order
                VesselBookmark next = _bookmarks.Find(b => b.Order == bookmark.Order + 1);
                if( next == null ) {
                    ModLogger.LogWarning($"Bookmark {commandModuleFlightID}: Next bookmark not found");
                    return false;
                }
                
                // Swap orders
                int temp = bookmark.Order;
                bookmark.Order = next.Order;
                next.Order = temp;

                this._bookmarks = this._bookmarks.OrderBy(b => b.Order).ToList();
                OnBookmarksUpdated.Fire();
                return true;
            } catch (Exception e) {
                ModLogger.LogError($"Error moving bookmark down for bookmark {commandModuleFlightID}: {e.Message}");
                return false;
            }
        }

        public bool SwapBookmarks(uint commandModuleFlightID1, uint commandModuleFlightID2) {
            try {
                ModLogger.LogDebug($"Swapping bookmarks for flightIDs {commandModuleFlightID1} and {commandModuleFlightID2}");

                VesselBookmark bookmark1 = this.GetBookmark(commandModuleFlightID1);
                if (bookmark1 == null) {
                    ModLogger.LogWarning($"Bookmark {commandModuleFlightID1}: Not found");
                    return false;
                }

                VesselBookmark bookmark2 = this.GetBookmark(commandModuleFlightID2);
                if (bookmark2 == null) {
                    ModLogger.LogWarning($"Bookmark {commandModuleFlightID2}: Not found");
                    return false;
                }

                int temp = bookmark1.Order;
                bookmark1.Order = bookmark2.Order;
                bookmark2.Order = temp;

                this._bookmarks = this._bookmarks.OrderBy(b => b.Order).ToList();
                OnBookmarksUpdated.Fire();
                return true;
            } catch (Exception e) {
                ModLogger.LogError($"Error swapping bookmarks for flightIDs {commandModuleFlightID1} and {commandModuleFlightID2}: {e.Message}");
                return false;
            }
        }
        
        // =======================================================================================

        /// <summary>
        /// Load bookmarks from config node
        /// </summary>
        /// <param name="node"></param>
        public void LoadBookmarks(ConfigNode node) {
            try {
                ModLogger.LogDebug($"Loading bookmarks from config node");
                _bookmarks.Clear();
                _bookmarksFlightIDs.Clear();
                
                if (!node.HasNode(SAVE_NODE_NAME)) {
                    return;
                }
                ConfigNode bookmarksNode = node.GetNode(SAVE_NODE_NAME);
                ConfigNode[] bookmarkNodes = bookmarksNode.GetNodes(BOOKMARK_NODE_NAME);
                
                // Sort nodes by order, so we will add them in the correct order
                bookmarkNodes = bookmarkNodes.OrderBy(b => {
                    int order = 0;
                    if (b.HasValue("order")) {
                        int.TryParse(b.GetValue("order"), out order);
                    }
                    return order;
                }).ToArray();

                foreach (ConfigNode bookmarkNode in bookmarkNodes) {
                    VesselBookmark bookmark = new VesselBookmark();
                    bookmark.Load(bookmarkNode);
                    this.AddBookmark(bookmark);
                }

                ModLogger.LogInfo($"{_bookmarks.Count} bookmark(s) loaded");
            } catch (Exception e) {
                ModLogger.LogError($"Error loading bookmarks: {e.Message}");
            }
        }

        /// <summary>
        /// Save bookmarks to config node
        /// </summary>
        /// <param name="node"></param>
        public void SaveBookmarks(ConfigNode node) {
            try {
                ModLogger.LogDebug($"Saving bookmarks to config node");

                if (node.HasNode(SAVE_NODE_NAME)) {
                    node.RemoveNode(SAVE_NODE_NAME);
                }
                
                ConfigNode bookmarksNode = node.AddNode(SAVE_NODE_NAME);
                foreach (VesselBookmark bookmark in _bookmarks) {
                    ConfigNode bookmarkNode = bookmarksNode.AddNode(BOOKMARK_NODE_NAME);
                    bookmark.Save(bookmarkNode);
                }
                ModLogger.LogInfo($"{_bookmarks.Count} bookmark(s) saved");
            } catch (Exception e) {
                ModLogger.LogError($"Error saving bookmarks: {e.Message}");
            }
        }

        // =======================================================================================

        /// <summary>
        /// Get the current vessel for a bookmark (handles docked vessels)
        /// Searches in both loaded and unloaded vessels
        /// </summary>
        public Vessel GetVessel(VesselBookmark bookmark) {
            try {
                if (bookmark == null) {
                    ModLogger.LogError("Attempted to get vessel with null bookmark");
                    return null;
                }
                ModLogger.LogDebug($"Getting vessel for bookmark {bookmark.CommandModuleFlightID}");

                foreach (Vessel vessel in FlightGlobals.Vessels) {
                    if (vessel == null || vessel.parts == null) continue;
                    foreach (Part part in vessel.parts) {
                        if (part == null) continue;
                        if (part.flightID == bookmark.CommandModuleFlightID) {
                            return vessel;
                        }
                    }
                }
                foreach (Vessel vessel in FlightGlobals.VesselsUnloaded) {
                    if (vessel == null || vessel.protoVessel == null) continue;
                    foreach (ProtoPartSnapshot protoPart in vessel.protoVessel.protoPartSnapshots) {
                        if (protoPart == null) continue;
                        if (protoPart.flightID == bookmark.CommandModuleFlightID) {
                            return vessel;
                        }
                    }
                }
                return null;
            } catch (Exception e) {
                ModLogger.LogError($"Error getting vessel for bookmark {bookmark.CommandModuleFlightID}: {e.Message}");
                return null;
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

        // =======================================================================================

        /// <summary>
        /// Get command module for a bookmark
        /// </summary>
        /// <param name="bookmark"></param>
        /// <returns>True if the bookmark was refreshed, false otherwise</returns>
        private bool RefreshBookmark(VesselBookmark bookmark) {
            try {
                if (bookmark == null) {
                    ModLogger.LogError("Attempted to refresh bookmark with null bookmark");
                    return false;
                }
                ModLogger.LogDebug($"Refreshing bookmark for bookmark {bookmark.CommandModuleFlightID}");
                
                Part commandModulePart = GetPart(bookmark.CommandModuleFlightID);
                if (commandModulePart != null) {
                    bookmark.CommandModuleName = commandModulePart.vesselNaming.vesselName;
                    bookmark.VesselType = commandModulePart.vesselNaming.vesselType;
                    return true;
                }

                ProtoPartSnapshot commandModuleProtoPartSnapshot = GetProtoPartSnapshot(bookmark.CommandModuleFlightID);
                if (commandModuleProtoPartSnapshot != null) {
                    bookmark.CommandModuleName = commandModuleProtoPartSnapshot.vesselNaming.vesselName;
                    bookmark.VesselType = commandModuleProtoPartSnapshot.vesselNaming.vesselType;
                    return true;
                }

                return false;
            } catch (Exception e) {
                ModLogger.LogError($"Error refreshing bookmark for bookmark {bookmark.CommandModuleFlightID}: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Refresh command module names for all bookmarks
        /// </summary>
        public void RefreshBookmarks() {
            try {
                ModLogger.LogDebug($"Refreshing bookmarks");

                foreach (VesselBookmark bookmark in _bookmarks) {
                    if( !this.RefreshBookmark(bookmark) ) {
                        ModLogger.LogWarning($"Bookmark {bookmark.CommandModuleFlightID}: Not found");
                    }
                }

                OnBookmarksUpdated.Fire();
            } catch (Exception e) {
                ModLogger.LogError($"Error refreshing bookmarks: {e.Message}");
            }
        }
    }
}
