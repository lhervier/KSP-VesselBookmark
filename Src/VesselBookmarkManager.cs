using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using com.github.lhervier.ksp.bookmarks;

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
        private List<Bookmark> _bookmarks = new List<Bookmark>();
        public IReadOnlyList<Bookmark> Bookmarks => _bookmarks.AsReadOnly();
        private List<object> _bookmarksIDs = new List<object>();

        public readonly EventVoid OnBookmarksUpdated = new EventVoid("VesselBookmarkManager.OnBookmarksUpdated");
 
        // =======================================================================================

        public Bookmark GetBookmark(object bookmarkID) {
            return _bookmarks.First(b => object.Equals(b.GetBookmarkID(), bookmarkID));
        }
        
        /// <summary>
        /// Check if a bookmark exists for a command module.
        /// Note: This method has no cost and can be called frequently.
        /// </summary>
        /// <param name="commandModuleFlightID"></param>
        /// <returns></returns>
        public bool HasBookmark(object bookmarkID) {
            try {
                return _bookmarksIDs.Contains(bookmarkID);
            } catch (Exception e) {
                ModLogger.LogError($"Error checking if bookmark exists for bookmarkID {bookmarkID}: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Add a bookmark for a command module
        /// </summary>
        /// <param name="bookmark">The bookmark to add</param>
        /// <param name="sendEvent">True if the OnBookmarksUpdated event should be fired, false otherwise</param>
        /// <returns>True if the bookmark was added, false otherwise</returns>
        public bool AddBookmark(Bookmark bookmark, bool sendEvent = true) {
            try {
                if( bookmark == null ) {
                    ModLogger.LogError("Attempted to add null bookmark");
                    return false;
                }
                ModLogger.LogDebug($"Adding bookmark for bookmarkID {bookmark.GetBookmarkID()}");
                
                // Check if bookmark already exists
                if (this.HasBookmark(bookmark.GetBookmarkID())) {
                    ModLogger.LogWarning($"Bookmark already exists for bookmarkID {bookmark.GetBookmarkID()}");
                    return false;
                }
                
                // Assign order (max + 1, or 0 if list is empty)
                if (_bookmarks.Count > 0) {
                    bookmark.Order = _bookmarks.Max(b => b.Order) + 1;
                } else {
                    bookmark.Order = 0;
                }

                _bookmarks.Add(bookmark);
                _bookmarksIDs.Add(bookmark.GetBookmarkID());

                bookmark.Refresh(sendEvent);

                return true;
            } catch (Exception e) {
                ModLogger.LogError($"Error adding bookmark for bookmarkID {bookmark.GetBookmarkID()}: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Remove a bookmark from the list
        /// </summary>
        /// <param name="bookmarkID">The unique identifier of the bookmark to remove</param>
        /// <returns>True if the bookmark was removed, false otherwise</returns>
        public bool RemoveBookmark(object bookmarkID) {
            try {
                ModLogger.LogDebug($"Removing bookmark for bookmarkID {bookmarkID}");
                Bookmark bookmark = this.GetBookmark(bookmarkID);
                if( bookmark == null ) {
                    ModLogger.LogWarning($"Bookmark for bookmarkID {bookmarkID}: Not found");
                    return false;
                }

                _bookmarks.Remove(bookmark);
                _bookmarksIDs.Remove(bookmark.GetBookmarkID());
                
                OnBookmarksUpdated.Fire();
                return true;
            } catch (Exception e) {
                ModLogger.LogError($"Error removing bookmark for bookmarkID {bookmarkID}: {e.Message}");
                return false;
            }
        }

        // =======================================================================================

        /// <summary>
        /// Move a bookmark up in the order (decrease Order value)
        /// </summary>
        /// <param name="bookmarkID">The unique identifier of the bookmark to move up</param>
        /// <returns>True if the bookmark was moved up, false otherwise</returns>
        public bool MoveBookmarkUp(object bookmarkID) {
            try {
                ModLogger.LogDebug($"Moving bookmark up for bookmarkID {bookmarkID}");

                Bookmark bookmark = this.GetBookmark(bookmarkID);
                if (bookmark == null) {
                    ModLogger.LogWarning($"Bookmark for bookmarkID {bookmarkID}: Not found");
                    return false;
                }

                if( bookmark.Order <= 0 ) {
                    ModLogger.LogError($"Bookmark for bookmarkID {bookmarkID} cannot be moved up: Order is {bookmark.Order}");
                    return false;
                }

                // Find the bookmark with previous Order
                Bookmark previous = _bookmarks.Find(b => b.Order == bookmark.Order - 1);
                if( previous == null ) {
                    ModLogger.LogWarning($"Bookmark for bookmarkID {bookmarkID}: Previous bookmark not found");
                    return false;
                }

                // Swap orders
                int temp = bookmark.Order;
                bookmark.Order = previous.Order;
                previous.Order = temp;
                
                // Re-order bookmarks
                this._bookmarks = this._bookmarks.OrderBy(b => b.Order).ToList();
                
                OnBookmarksUpdated.Fire();
                return true;
            } catch (Exception e) {
                ModLogger.LogError($"Error moving bookmark up for bookmarkID {bookmarkID}: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Move a bookmark down in the order (increase Order value)
        /// </summary>
        /// <param name="bookmarkID">The unique identifier of the bookmark to move down</param>
        /// <returns>True if the bookmark was moved down, false otherwise</returns>
        public bool MoveBookmarkDown(object bookmarkID) {
            try {
                ModLogger.LogDebug($"Moving bookmark down for bookmarkID {bookmarkID}");

                Bookmark bookmark = this.GetBookmark(bookmarkID);
                if (bookmark == null) {
                    ModLogger.LogWarning($"Bookmark for bookmarkID {bookmarkID}: Not found");
                    return false;
                }

                if( bookmark.Order >= _bookmarks.Max(b => b.Order) ) {
                    ModLogger.LogWarning($"Bookmark for bookmarkID {bookmarkID} cannot be moved down: Order is {bookmark.Order}");
                    return false;
                }
                
                // Find the bookmark with next Order
                Bookmark next = _bookmarks.Find(b => b.Order == bookmark.Order + 1);
                if( next == null ) {
                    ModLogger.LogWarning($"Bookmark for bookmarkID {bookmarkID}: Next bookmark not found");
                    return false;
                }
                
                // Swap orders
                int temp = bookmark.Order;
                bookmark.Order = next.Order;
                next.Order = temp;

                // Re-order bookmarks
                this._bookmarks = this._bookmarks.OrderBy(b => b.Order).ToList();
                
                OnBookmarksUpdated.Fire();
                return true;
            } catch (Exception e) {
                ModLogger.LogError($"Error moving bookmark down for bookmarkID {bookmarkID}: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Swap the order of two bookmarks
        /// </summary>
        /// <param name="bookmarkID1">The unique identifier of the first bookmark to swap</param>
        /// <param name="bookmarkID2">The unique identifier of the second bookmark to swap</param>
        /// <returns>True if the bookmarks were swapped, false otherwise</returns>
        public bool SwapBookmarks(object bookmarkID1, object bookmarkID2) {
            try {
                ModLogger.LogDebug($"Swapping bookmarks for bookmarkID1 {bookmarkID1} and bookmarkID2 {bookmarkID2}");

                Bookmark bookmark1 = this.GetBookmark(bookmarkID1);
                if (bookmark1 == null) {
                    ModLogger.LogWarning($"Bookmark for bookmarkID {bookmarkID1}: Not found");
                    return false;
                }

                Bookmark bookmark2 = this.GetBookmark(bookmarkID2);
                if (bookmark2 == null) {
                    ModLogger.LogWarning($"Bookmark for bookmarkID {bookmarkID2}: Not found");
                    return false;
                }

                int temp = bookmark1.Order;
                bookmark1.Order = bookmark2.Order;
                bookmark2.Order = temp;

                this._bookmarks = this._bookmarks.OrderBy(b => b.Order).ToList();
                OnBookmarksUpdated.Fire();
                return true;
            } catch (Exception e) {
                ModLogger.LogError($"Error swapping bookmarks for bookmarkID1 {bookmarkID1} and bookmarkID2 {bookmarkID2}: {e.Message}");
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
                _bookmarksIDs.Clear();
                
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
                    BookmarkType bookmarkType = BookmarkType.Unknown;
                    if (bookmarkNode.HasValue("bookmarkType")) {
                        int.TryParse(bookmarkNode.GetValue("bookmarkType"), out int bookmarkTypeInt);
                        bookmarkType = (BookmarkType) bookmarkTypeInt;
                    }

                    Bookmark bookmark = null;
                    if( bookmarkType == BookmarkType.CommandModule ) {
                        bookmark = new CommandModuleBookmark(bookmarkNode);
                    } else if( bookmarkType == BookmarkType.Vessel ) {
                        bookmark = new VesselBookmark(bookmarkNode);
                    } else {
                        ModLogger.LogError($"Bookmark type {bookmarkType} not supported");
                        continue;
                    }
                    
                    this.AddBookmark(bookmark, false);
                }

                OnBookmarksUpdated.Fire();
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
                foreach (Bookmark bookmark in _bookmarks) {
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
        /// Refresh command module names for all bookmarks
        /// </summary>
        public void RefreshBookmarks() {
            try {
                ModLogger.LogDebug($"Refreshing bookmarks");

                foreach (Bookmark bookmark in _bookmarks) {
                    if( !bookmark.Refresh(false) ) {
                        ModLogger.LogWarning($"Bookmark {bookmark.GetBookmarkID()}: Not found");
                    }
                }

                OnBookmarksUpdated.Fire();
            } catch (Exception e) {
                ModLogger.LogError($"Error refreshing bookmarks: {e.Message}");
            }
        }
    }
}
