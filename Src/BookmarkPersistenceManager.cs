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
        /// Get an integer value from a config node
        /// </summary>
        /// <param name="node">Config node to get the value from</param>
        /// <param name="valueName">Name of the value to get</param>
        /// <returns>The value of the integer</returns>
        private static int GetIntNodeValue(ConfigNode node, string valueName) {
            if( !node.HasValue(valueName) ) {
                throw new Exception($"{valueName} not found in the bookmark node");
            }

            if( !int.TryParse(node.GetValue(valueName), out int value) ) {
                throw new Exception($"{valueName} is not a valid integer");  
            }
            return value;
        }

        /// <summary>
        /// Get an integer value from a config node
        /// </summary>
        /// <param name="node">Config node to get the value from</param>
        /// <param name="valueName">Name of the value to get</param>
        /// <returns>The value of the integer</returns>
        private static uint GetUintNodeValue(ConfigNode node, string valueName) {
            if( !node.HasValue(valueName) ) {
                throw new Exception($"{valueName} not found in the bookmark node");
            }

            if( !uint.TryParse(node.GetValue(valueName), out uint value) ) {
                throw new Exception($"{valueName} is not a valid integer");
            }
            return value;
        }

        /// <summary>
        /// Load a bookmark from a config node
        /// </summary>
        /// <param name="node"></param>
        /// <returns>The loaded bookmark</returns>
        private static Bookmark LoadBookmark(ConfigNode node) {
            try {
                ModLogger.LogDebug($"Loading bookmark from config node");

                // Load bookmark type and vessel persistent ID (mandatory fields)
                BookmarkType bookmarkType = (BookmarkType) GetIntNodeValue(node, "bookmarkType");
                uint bookmarkID = GetUintNodeValue(node, "bookmarkID");
                
                // Instanciate the bookmark
                Bookmark bookmark;
                if( bookmarkType == BookmarkType.CommandModule ) {
                    bookmark = new CommandModuleBookmark(bookmarkID);
                } else if( bookmarkType == BookmarkType.Vessel ) {
                    bookmark = new VesselBookmark(bookmarkID);
                } else {
                    throw new Exception($"Invalid bookmark type {bookmarkType}");
                }

                // Load common fields
                bookmark.Comment = node.GetValue("comment") ?? "";
                bookmark.Order = GetIntNodeValue(node, "order");        // Mandatory
                try {
                    bookmark.CreationTime = double.Parse(node.GetValue("creationTime"));
                } catch (Exception e) {
                    ModLogger.LogWarning($"creationTime not found in the bookmark node : {e.Message}");
                    bookmark.CreationTime = Planetarium.GetUniversalTime();
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

        /// <summary>
        /// Save a bookmark to a config node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="bookmark"></param>
        /// <returns>True if the bookmark was saved, false otherwise</returns>
        public static bool SaveBookmark(ConfigNode node, Bookmark bookmark) {
            node.AddValue("bookmarkID", bookmark.BookmarkID);
            node.AddValue("bookmarkType", (int) bookmark.BookmarkType);

            node.AddValue("comment", bookmark.Comment);
            node.AddValue("order", bookmark.Order);
            node.AddValue("creationTime", bookmark.CreationTime);
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
                        ModLogger.LogError($"Error saving bookmark {bookmark.BookmarkType} and {bookmark.BookmarkID} to config node");
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