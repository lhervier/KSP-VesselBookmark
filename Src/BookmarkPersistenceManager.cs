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
                uint bookmarkID = ConfigNodeUtils.GetUintNodeValue(node, "bookmarkID", true);
                string sBookmarkType = node.GetValue("bookmarkType");
                if( sBookmarkType == null ) {
                    LOGGER.LogError($"Bookmark {bookmarkID}: Bookmark type is mandatory");
                    return null;
                }
                BookmarkType bookmarkType = (BookmarkType) Enum.Parse(typeof(BookmarkType), sBookmarkType);
                if( bookmarkType == BookmarkType.Unknown ) {
                    LOGGER.LogError($"Bookmark {bookmarkID}: Invalid bookmark type {sBookmarkType}");
                    return null;
                }

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
                bookmark.Comment = node.GetValue("comment") ?? "";
                bookmark.Order = ConfigNodeUtils.GetIntNodeValue(node, "order", true);
                bookmark.CreationTime = ConfigNodeUtils.GetDoubleNodeValue(node, "creationTime", false, Planetarium.GetUniversalTime());
                
                // Load cached fields (need if vessel doesn't exist)
                bookmark.BookmarkTitle = node.GetValue("bookmarkTitle") ?? "";
                bookmark.BookmarkVesselType = node.GetValue("bookmarkVesselType") ?? "";
                
                bookmark.VesselPersistentID = ConfigNodeUtils.GetUintNodeValue(node, "vesselPersistentID", false, 0);
                bookmark.VesselName = node.GetValue("vesselName") ?? "";
                bookmark.VesselType = node.GetValue("vesselType") ?? "";
                bookmark.VesselBodyName = node.GetValue("vesselBodyName") ?? "";
                bookmark.VesselSituation = node.GetValue("vesselSituation") ?? "";
                bookmark.VesselSituationLabel = node.GetValue("vesselSituationLabel") ?? "";
                string sHasAlarm = node.GetValue("hasAlarm") ?? bool.FalseString;
                bookmark.HasAlarm = bool.Parse(sHasAlarm);

                if( bookmarkType == BookmarkType.CommandModule ) {
                    CommandModuleBookmark commandModuleBookmark = (CommandModuleBookmark) bookmark;
                    commandModuleBookmark.CommandModuleFlightID = ConfigNodeUtils.GetUintNodeValue(node, "commandModuleFlightID", false, 0);
                    commandModuleBookmark.CommandModuleName = node.GetValue("commandModuleName") ?? "";
                    commandModuleBookmark.CommandModuleType = node.GetValue("commandModuleType") ?? "";
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
            node.AddValue("bookmarkID", bookmark.BookmarkID);
            node.AddValue("bookmarkType", bookmark.BookmarkType.ToString());

            node.AddValue("comment", bookmark.Comment);
            node.AddValue("order", bookmark.Order);
            node.AddValue("creationTime", bookmark.CreationTime);

            // Save cached fields
            node.AddValue("bookmarkTitle", bookmark.BookmarkTitle ?? "");
            node.AddValue("bookmarkVesselType", bookmark.BookmarkVesselType ?? "");
            
            node.AddValue("vesselPersistentID", bookmark.VesselPersistentID);
            node.AddValue("vesselName", bookmark.VesselName ?? "");
            node.AddValue("vesselType", bookmark.VesselType.ToString());
            node.AddValue("vesselBodyName", bookmark.VesselBodyName ?? "");
            node.AddValue("vesselSituation", bookmark.VesselSituation.ToString());
            node.AddValue("vesselSituationLabel", bookmark.VesselSituationLabel ?? "");
            node.AddValue("hasAlarm", bookmark.HasAlarm.ToString());

            if( bookmark is CommandModuleBookmark commandModuleBookmark ) {
                node.AddValue("commandModuleFlightID", commandModuleBookmark.CommandModuleFlightID);
                node.AddValue("commandModuleName", commandModuleBookmark.CommandModuleName);
                node.AddValue("commandModuleType", commandModuleBookmark.CommandModuleType.ToString());
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