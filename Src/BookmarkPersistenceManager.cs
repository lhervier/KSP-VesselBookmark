using System;
using System.Collections.Generic;
using UnityEngine;
using com.github.lhervier.ksp.bookmarksmod.bookmarks;
using com.github.lhervier.ksp.bookmarksmod.util;
using System.Linq;

namespace com.github.lhervier.ksp.bookmarksmod {
    public class BookmarkPersistenceManager {
        private static readonly ModLogger LOGGER = new ModLogger("BookmarkPersistenceManager");
        
        /// <summary>
        /// Name of the node in the config file where bookmarks are saved
        /// </summary>
        private const string SAVE_NODE_NAME = "VESSEL_BOOKMARKS";
        private const string BOOKMARK_NODE_NAME = "BOOKMARK";
        
        // ====================================================
        //      Loading bookmarks
        // ====================================================

        /// <summary>
        /// Load a bookmark from a config node
        /// </summary>
        /// <param name="node"></param>
        /// <returns>The loaded bookmark</returns>
        private static Bookmark LoadBookmark(ConfigNode node) {
            try {
                LOGGER.LogDebug($"Loading bookmark from config node");

                // Load bookmark id and type (mandatory fields)
                uint bookmarkID = node.GetMandatoryUintValue("bookmarkID");
                BookmarkType bookmarkType = node.GetMandatoryEnumValue<BookmarkType>("bookmarkType");

                // Instanciate the bookmark from its type
                Bookmark bookmark;
                if( bookmarkType == BookmarkType.CommandModule ) {
                    bookmark = new CommandModuleBookmark(bookmarkID);
                } else if( bookmarkType == BookmarkType.Vessel ) {
                    bookmark = new VesselBookmark(bookmarkID);
                } else {
                    throw new Exception($"Invalid bookmark type {bookmarkType}");
                }

                // Load common fields
                bookmark.Comment = node.GetStringValue("comment");
                bookmark.Order = node.GetMandatoryIntValue("order");
                bookmark.CreationTime = node.GetDoubleValue("creationTime", Planetarium.GetUniversalTime());
                
                // Load cached fields (need if vessel doesn't exist)
                bookmark.BookmarkTitle = node.GetStringValue("bookmarkTitle");
                bookmark.BookmarkVesselType = node.GetStringValue("bookmarkVesselType");
                
                bookmark.VesselPersistentID = node.GetUintValue("vesselPersistentID");
                bookmark.VesselName = node.GetStringValue("vesselName");
                bookmark.VesselType = node.GetStringValue("vesselType");
                bookmark.VesselBodyName = node.GetStringValue("vesselBodyName");
                bookmark.VesselSituation = node.GetStringValue("vesselSituation");
                bookmark.VesselSituationLabel = node.GetStringValue("vesselSituationLabel");
                bookmark.HasAlarm = node.GetBoolValue("hasAlarm");

                if( bookmarkType == BookmarkType.CommandModule ) {
                    CommandModuleBookmark commandModuleBookmark = (CommandModuleBookmark) bookmark;
                    commandModuleBookmark.CommandModuleFlightID = node.GetUintValue("commandModuleFlightID");
                    commandModuleBookmark.CommandModuleName = node.GetStringValue("commandModuleName");
                    commandModuleBookmark.CommandModuleType = node.GetStringValue("commandModuleType");
                } else if( bookmarkType == BookmarkType.Vessel ) {
                    // Nothing more
                }

                LOGGER.LogInfo($"Bookmark {bookmark} loaded from config node");
                return bookmark;
            } catch (Exception e) {
                LOGGER.LogError($"Error loading bookmark from config node: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Load bookmarks from config node
        /// </summary>
        /// <param name="node"></param>
        /// <returns>List of loaded bookmarks</returns>
        public static List<Bookmark> LoadBookmarks(ConfigNode node) {
            LOGGER.LogDebug($"Loading bookmarks from config node");
            
            List<Bookmark> bookmarks = new List<Bookmark>();
            if (!node.HasNode(SAVE_NODE_NAME)) {
                return bookmarks;
            }
            ConfigNode bookmarksNode = node.GetNode(SAVE_NODE_NAME);
            ConfigNode[] bookmarkNodes = bookmarksNode.GetNodes(BOOKMARK_NODE_NAME);
            
            foreach (ConfigNode bookmarkNode in bookmarkNodes) {
                Bookmark bookmark = LoadBookmark(bookmarkNode);
                if( bookmark == null ) {
                    LOGGER.LogWarning($"Bookmark not loaded from config node: Let's continue to next one...");
                    continue;
                }
                bookmarks.Add(bookmark);
            }

            LOGGER.LogInfo($"{bookmarks.Count} bookmark(s) loaded");
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
            node.AddUintValue("bookmarkID", bookmark.BookmarkID);
            node.AddEnumValue("bookmarkType", bookmark.BookmarkType);

            node.AddStringValue("comment", bookmark.Comment);
            node.AddIntValue("order", bookmark.Order);
            node.AddDoubleValue("creationTime", bookmark.CreationTime);

            // Save cached fields
            node.AddStringValue("bookmarkTitle", bookmark.BookmarkTitle);
            node.AddStringValue("bookmarkVesselType", bookmark.BookmarkVesselType);
            
            node.AddUintValue("vesselPersistentID", bookmark.VesselPersistentID);
            node.AddStringValue("vesselName", bookmark.VesselName);
            node.AddStringValue("vesselType", bookmark.VesselType);
            node.AddStringValue("vesselBodyName", bookmark.VesselBodyName);
            node.AddStringValue("vesselSituation", bookmark.VesselSituation);
            node.AddStringValue("vesselSituationLabel", bookmark.VesselSituationLabel);
            node.AddBoolValue("hasAlarm", bookmark.HasAlarm);

            if( bookmark is CommandModuleBookmark commandModuleBookmark ) {
                node.AddUintValue("commandModuleFlightID", commandModuleBookmark.CommandModuleFlightID);
                node.AddStringValue("commandModuleName", commandModuleBookmark.CommandModuleName);
                node.AddStringValue("commandModuleType", commandModuleBookmark.CommandModuleType);
            } else {
                // Nothing more
            }
            return true;
        }

        /// <summary>
        /// Save bookmarks to config node
        /// </summary>
        /// <param name="node"></param>
        public static bool SaveBookmarks(ConfigNode node, List<Bookmark> bookmarks) {
            try {
                LOGGER.LogDebug($"Saving bookmarks to config node");

                if (node.HasNode(SAVE_NODE_NAME)) {
                    node.RemoveNode(SAVE_NODE_NAME);
                }
                
                ConfigNode bookmarksNode = node.AddNode(SAVE_NODE_NAME);
                foreach (Bookmark bookmark in bookmarks) {
                    ConfigNode bookmarkNode = bookmarksNode.AddNode(BOOKMARK_NODE_NAME);
                    if( !SaveBookmark(bookmarkNode, bookmark) ) {
                        LOGGER.LogError($"Error saving bookmark {bookmark} to config node: Let's continue to next one...");
                        return false;
                    }
                }
                LOGGER.LogInfo($"{bookmarks.Count} bookmark(s) saved");
                return true;
            } catch (Exception e) {
                LOGGER.LogError($"Error saving bookmarks: {e.Message}");
                return false;
            }
        }
    }
}