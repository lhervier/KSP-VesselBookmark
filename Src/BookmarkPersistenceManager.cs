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
                uint bookmarkID = ConfigNodeUtils.GetMandatoryUintNodeValue(node, "bookmarkID");
                BookmarkType bookmarkType = ConfigNodeUtils.GetMandatoryEnumNodeValue<BookmarkType>(node, "bookmarkType");

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
                bookmark.Comment = ConfigNodeUtils.GetStringNodeValue(node, "comment");
                bookmark.Order = ConfigNodeUtils.GetMandatoryIntNodeValue(node, "order");
                bookmark.CreationTime = ConfigNodeUtils.GetDoubleNodeValue(node, "creationTime", Planetarium.GetUniversalTime());
                
                // Load cached fields (need if vessel doesn't exist)
                bookmark.BookmarkTitle = ConfigNodeUtils.GetStringNodeValue(node, "bookmarkTitle");
                bookmark.BookmarkVesselType = ConfigNodeUtils.GetStringNodeValue(node, "bookmarkVesselType");
                
                bookmark.VesselPersistentID = ConfigNodeUtils.GetUintNodeValue(node, "vesselPersistentID");
                bookmark.VesselName = ConfigNodeUtils.GetStringNodeValue(node, "vesselName");
                bookmark.VesselType = ConfigNodeUtils.GetStringNodeValue(node, "vesselType");
                bookmark.VesselBodyName = ConfigNodeUtils.GetStringNodeValue(node, "vesselBodyName");
                bookmark.VesselSituation = ConfigNodeUtils.GetStringNodeValue(node, "vesselSituation");
                bookmark.VesselSituationLabel = ConfigNodeUtils.GetStringNodeValue(node, "vesselSituationLabel");
                bookmark.HasAlarm = ConfigNodeUtils.GetBoolNodeValue(node, "hasAlarm");

                if( bookmarkType == BookmarkType.CommandModule ) {
                    CommandModuleBookmark commandModuleBookmark = (CommandModuleBookmark) bookmark;
                    commandModuleBookmark.CommandModuleFlightID = ConfigNodeUtils.GetUintNodeValue(node, "commandModuleFlightID");
                    commandModuleBookmark.CommandModuleName = ConfigNodeUtils.GetStringNodeValue(node, "commandModuleName");
                    commandModuleBookmark.CommandModuleType = ConfigNodeUtils.GetStringNodeValue(node, "commandModuleType");
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
            ConfigNodeUtils.AddUintNodeValue(node, "bookmarkID", bookmark.BookmarkID);
            ConfigNodeUtils.AddEnumNodeValue(node, "bookmarkType", bookmark.BookmarkType);

            ConfigNodeUtils.AddStringNodeValue(node, "comment", bookmark.Comment);
            ConfigNodeUtils.AddIntNodeValue(node, "order", bookmark.Order);
            ConfigNodeUtils.AddDoubleNodeValue(node, "creationTime", bookmark.CreationTime);

            // Save cached fields
            ConfigNodeUtils.AddStringNodeValue(node, "bookmarkTitle", bookmark.BookmarkTitle);
            ConfigNodeUtils.AddStringNodeValue(node, "bookmarkVesselType", bookmark.BookmarkVesselType);
            
            ConfigNodeUtils.AddUintNodeValue(node, "vesselPersistentID", bookmark.VesselPersistentID);
            ConfigNodeUtils.AddStringNodeValue(node, "vesselName", bookmark.VesselName);
            ConfigNodeUtils.AddStringNodeValue(node, "vesselType", bookmark.VesselType);
            ConfigNodeUtils.AddStringNodeValue(node, "vesselBodyName", bookmark.VesselBodyName);
            ConfigNodeUtils.AddStringNodeValue(node, "vesselSituation", bookmark.VesselSituation);
            ConfigNodeUtils.AddStringNodeValue(node, "vesselSituationLabel", bookmark.VesselSituationLabel);
            ConfigNodeUtils.AddBoolNodeValue(node, "hasAlarm", bookmark.HasAlarm);

            if( bookmark is CommandModuleBookmark commandModuleBookmark ) {
                ConfigNodeUtils.AddUintNodeValue(node, "commandModuleFlightID", commandModuleBookmark.CommandModuleFlightID);
                ConfigNodeUtils.AddStringNodeValue(node, "commandModuleName", commandModuleBookmark.CommandModuleName);
                ConfigNodeUtils.AddStringNodeValue(node, "commandModuleType", commandModuleBookmark.CommandModuleType);
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