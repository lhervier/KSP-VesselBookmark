using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using com.github.lhervier.ksp.bookmarksmod.bookmarks;

namespace com.github.lhervier.ksp.bookmarksmod {
    public class BookmarkPersistenceManager {

        /// <summary>
        /// Name of the node in the config file where bookmarks are saved
        /// </summary>
        private const string SAVE_NODE_NAME = "VESSEL_BOOKMARKS";
        private const string BOOKMARK_NODE_NAME = "BOOKMARK";
        
        // ====================================================
        //      Loading bookmarks
        // ====================================================

        /// <summary>
        /// Load a command module bookmark from a config node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="bookmark"></param>
        /// <returns>True if the command module bookmark was loaded, false otherwise</returns>
        private static bool LoadCommandModuleBookmark(ConfigNode node, CommandModuleBookmark bookmark) {
            if (node.HasValue("commandModuleFlightID")) {
                uint.TryParse(node.GetValue("commandModuleFlightID"), out uint flightID);
                bookmark.CommandModuleFlightID = flightID;
            } else {
                ModLogger.LogError("commandModuleFlightID not found in the bookmark node");
                return false;
            }

            bookmark.CommandModuleName = node.GetValue("commandModuleName") ?? "";
            
            if (node.HasValue("commandModuleType")) {
                int.TryParse(node.GetValue("commandModuleType"), out int vesselType);
                bookmark.CommandModuleType = (VesselType) vesselType;
            } else {
                ModLogger.LogError("commandModuleType not found in the bookmark node");
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// Load a bookmark from a config node
        /// </summary>
        /// <param name="node"></param>
        /// <returns>The loaded bookmark</returns>
        private static Bookmark LoadBookmark(ConfigNode node) {
            try {
                ModLogger.LogDebug($"Loading bookmark from config node");

                // Instantiate the bookmark
                Bookmark bookmark;
                if( node.HasValue("bookmarkType") ) {
                    int.TryParse(node.GetValue("bookmarkType"), out int bookmarkTypeInt);
                    BookmarkType bookmarkType = (BookmarkType) bookmarkTypeInt;
                    if( bookmarkType == BookmarkType.CommandModule ) {
                        bookmark = new CommandModuleBookmark();
                    } else if( bookmarkType == BookmarkType.Vessel ) {
                        bookmark = new VesselBookmark();
                    } else {
                        ModLogger.LogError($"Invalid bookmark type {bookmarkType}");
                        return null;
                    }
                } else {
                    ModLogger.LogError("bookmarkType not found in the bookmark node");
                    return null;
                }
                
                // Mandatory fields
                
                if( node.HasValue("vesselPersistentID") ) {
                    uint.TryParse(node.GetValue("vesselPersistentID"), out uint persistentID);
                    bookmark.VesselPersistentID = persistentID;
                } else {
                    ModLogger.LogError("vesselPersistentID not found in the bookmark node");
                    return null;
                }

                if (node.HasValue("vesselType")) {
                    int.TryParse(node.GetValue("vesselType"), out int vesselType);
                    bookmark.VesselType = (VesselType) vesselType;
                } else {
                    ModLogger.LogError("vesselType not found in the bookmark node");
                    return null;
                }
                
                if (node.HasValue("order")) {
                    int.TryParse(node.GetValue("order"), out int order);
                    bookmark.Order = order;
                } else {
                    ModLogger.LogError("order not found in the bookmark node");
                    return null;
                }

                // Optional fields

                bookmark.Comment = node.GetValue("comment") ?? "";
                
                bookmark.VesselSituation = Vessel.Situations.PRELAUNCH;
                if( node.HasValue("vesselSituation") ) {
                    string situation = node.GetValue("vesselSituation");
                    if( Enum.TryParse<Vessel.Situations>(situation, out Vessel.Situations parsedSituation) ) {
                        bookmark.VesselSituation = parsedSituation;
                    } else {
                        ModLogger.LogWarning($"Invalid vesselSituation value '{situation}' in the bookmark node, using PRELAUNCH as default");
                    }
                } else {
                    ModLogger.LogWarning("vesselSituation not found in the bookmark node");
                }

                if( node.HasValue("vesselBody") ) {
                    string bodyName = node.GetValue("vesselBody");
                    bookmark.VesselBody = FlightGlobals.Bodies.FirstOrDefault(b => b.bodyName == bodyName);
                    if( bookmark.VesselBody == null ) {
                        ModLogger.LogWarning($"Vessel body {bodyName} not found");
                    }
                } else {
                    ModLogger.LogWarning("vesselBody not found in the bookmark node");
                    bookmark.VesselBody = null;
                }

                bookmark.VesselName = node.GetValue("vesselName") ?? "";
                
                if (node.HasValue("hasAlarm")) {
                    bool.TryParse(node.GetValue("hasAlarm"), out bool hasAlarm);
                    bookmark.HasAlarm = hasAlarm;
                } else {
                    bookmark.HasAlarm = false;
                }

                if (node.HasValue("creationTime")) {
                    double.TryParse(node.GetValue("creationTime"), out double time);
                    bookmark.CreationTime = time;
                } else {
                    ModLogger.LogWarning("creationTime not found in the bookmark node");
                    bookmark.CreationTime = Planetarium.GetUniversalTime();
                }
                
                // ========================== Load specific data ==========================

                if( bookmark is CommandModuleBookmark commandModuleBookmark ) {
                    bool success = LoadCommandModuleBookmark(node, commandModuleBookmark);
                    if( !success ) {
                        ModLogger.LogError("Error loading command module bookmark from config node");
                        return null;
                    }
                } else if( bookmark is VesselBookmark vesselBookmark ) {
                    // Nothing...
                }

                return bookmark;
            } catch (Exception e) {
                ModLogger.LogError($"Error loading bookmark from config node: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Load bookmarks from config node
        /// </summary>
        /// <param name="node"></param>
        /// <returns>List of loaded bookmarks</returns>
        public static List<Bookmark> LoadBookmarks(ConfigNode node) {
            ModLogger.LogDebug($"Loading bookmarks from config node");
            
            List<Bookmark> bookmarks = new List<Bookmark>();
            if (!node.HasNode(SAVE_NODE_NAME)) {
                return bookmarks;
            }
            ConfigNode bookmarksNode = node.GetNode(SAVE_NODE_NAME);
            ConfigNode[] bookmarkNodes = bookmarksNode.GetNodes(BOOKMARK_NODE_NAME);
            
            foreach (ConfigNode bookmarkNode in bookmarkNodes) {
                Bookmark bookmark = LoadBookmark(bookmarkNode);
                if( bookmark == null ) {
                    ModLogger.LogError($"Error loading bookmark from config node");
                    continue;
                }
                bookmarks.Add(bookmark);
            }

            ModLogger.LogInfo($"{bookmarks.Count} bookmark(s) loaded");
            return bookmarks;
        }

        // ====================================================
        //      Saving bookmarks
        // ====================================================

        public static bool SaveCommandModuleBookmark(ConfigNode node, CommandModuleBookmark bookmark) {
            node.AddValue("commandModuleFlightID", bookmark.CommandModuleFlightID);
            node.AddValue("commandModuleName", bookmark.CommandModuleName);
            node.AddValue("commandModuleType", (int) bookmark.CommandModuleType);
            return true;
        }

        /// <summary>
        /// Save a bookmark to a config node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="bookmark"></param>
        /// <returns>True if the bookmark was saved, false otherwise</returns>
        public static bool SaveBookmark(ConfigNode node, Bookmark bookmark) {
            node.AddValue("bookmarkType", (int) bookmark.BookmarkType);
            node.AddValue("comment", bookmark.Comment);
            node.AddValue("creationTime", bookmark.CreationTime);
            node.AddValue("vesselSituation", bookmark.VesselSituation.ToString());
            node.AddValue("vesselBody", bookmark.VesselBody.bodyName);
            node.AddValue("vesselPersistentID", bookmark.VesselPersistentID);
            node.AddValue("vesselName", bookmark.VesselName);
            node.AddValue("vesselType", (int) bookmark.VesselType);
            node.AddValue("order", bookmark.Order);
            node.AddValue("hasAlarm", bookmark.HasAlarm);
            
            if( bookmark is CommandModuleBookmark commandModuleBookmark ) {
                bool success = SaveCommandModuleBookmark(node, commandModuleBookmark);
                if( !success ) {
                    ModLogger.LogError("Error saving command module bookmark to config node");
                    return false;
                }
            } else if( bookmark is VesselBookmark vesselBookmark ) {
                // Nothing...
            }
            
            return true;
        }

        /// <summary>
        /// Save bookmarks to config node
        /// </summary>
        /// <param name="node"></param>
        public static bool SaveBookmarks(ConfigNode node, List<Bookmark> bookmarks) {
            try {
                ModLogger.LogDebug($"Saving bookmarks to config node");

                if (node.HasNode(SAVE_NODE_NAME)) {
                    node.RemoveNode(SAVE_NODE_NAME);
                }
                
                ConfigNode bookmarksNode = node.AddNode(SAVE_NODE_NAME);
                foreach (Bookmark bookmark in bookmarks) {
                    ConfigNode bookmarkNode = bookmarksNode.AddNode(BOOKMARK_NODE_NAME);
                    if( !SaveBookmark(bookmarkNode, bookmark) ) {
                        ModLogger.LogError($"Error saving bookmark {bookmark.BookmarkType} and {bookmark.GetBookmarkID()} to config node");
                        return false;
                    }
                }
                ModLogger.LogInfo($"{bookmarks.Count} bookmark(s) saved");
                return true;
            } catch (Exception e) {
                ModLogger.LogError($"Error saving bookmarks: {e.Message}");
                return false;
            }
        }
    }
}