using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using com.github.lhervier.ksp.bookmarksmod.bookmarks;

namespace com.github.lhervier.ksp.bookmarksmod {
    
    /// <summary>
    /// Central bookmark manager
    /// </summary>
    public class BookmarkManager {
        
        /// <summary>
        /// Name of the node in the config file where bookmarks are saved
        /// </summary>
        private const string SAVE_NODE_NAME = "VESSEL_BOOKMARKS";
        private const string BOOKMARK_NODE_NAME = "BOOKMARK";
        
        /// <summary>
        /// Singleton instance
        /// </summary>
        private static BookmarkManager _instance;
        public static BookmarkManager Instance {
            get {
                if (_instance == null) {
                    _instance = new BookmarkManager();
                }
                return _instance;
            }
        }
        
        /// <summary>
        /// List of all bookmarks
        /// </summary>
        private List<Bookmark> _bookmarks = new List<Bookmark>();
        public IReadOnlyList<Bookmark> Bookmarks => _bookmarks.AsReadOnly();
        private BookmarkIds _bookmarksIDs = new BookmarkIds();

        public readonly EventVoid OnBookmarksUpdated = new EventVoid("VesselBookmarkManager.OnBookmarksUpdated");

        // =======================================================================================

        /// <summary>
        /// Get a bookmark by its unique identifier
        /// </summary>
        /// <param name="bookmarkType">The type of the bookmark</param>
        /// <param name="bookmarkID">The unique identifier of the bookmark</param>
        /// <returns>The bookmark, or null if not found</returns>
        public Bookmark GetBookmark(BookmarkType bookmarkType, uint bookmarkID) {
            return _bookmarks.FirstOrDefault(
                b => (b.GetBookmarkType() == bookmarkType) && (b.GetBookmarkID() == bookmarkID)
            );
        }
        
        /// <summary>
        /// Check if a bookmark exists for a command module.
        /// Note: This method has no cost and can be called frequently.
        /// </summary>
        /// <param name="bookmarkType">The type of the bookmark</param>
        /// <param name="bookmarkID">The unique identifier of the bookmark</param>
        /// <returns></returns>
        public bool HasBookmark(BookmarkType bookmarkType, uint bookmarkID) {
            try {
                return _bookmarksIDs.hasId(bookmarkType, bookmarkID);
            } catch (Exception e) {
                ModLogger.LogError($"Error checking if bookmark exists for bookmarkType {bookmarkType} and bookmarkID {bookmarkID}: {e.Message}");
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
                ModLogger.LogDebug($"Adding bookmark for bookmarkType {bookmark.GetBookmarkType()} and bookmarkID {bookmark.GetBookmarkID()}");
                
                // Check if bookmark already exists
                if (this.HasBookmark(bookmark.GetBookmarkType(), bookmark.GetBookmarkID())) {
                    ModLogger.LogWarning($"Bookmark already exists for bookmarkType {bookmark.GetBookmarkType()} and bookmarkID {bookmark.GetBookmarkID()}");
                    return false;
                }
                
                // Assign order (max + 1, or 0 if list is empty)
                if (_bookmarks.Count > 0) {
                    bookmark.Order = _bookmarks.Max(b => b.Order) + 1;
                } else {
                    bookmark.Order = 0;
                }

                _bookmarks.Add(bookmark);
                _bookmarksIDs.addId(bookmark.GetBookmarkType(), bookmark.GetBookmarkID());

                bookmark.Refresh(sendEvent);

                return true;
            } catch (Exception e) {
                ModLogger.LogError($"Error adding bookmark for bookmarkType {bookmark.GetBookmarkType()} and bookmarkID {bookmark.GetBookmarkID()}: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Remove a bookmark from the list
        /// </summary>
        /// <param name="bookmarkID">The unique identifier of the bookmark to remove</param>
        /// <returns>True if the bookmark was removed, false otherwise</returns>
        public bool RemoveBookmark(Bookmark bookmark) {
            try {
                if( bookmark == null ) {
                    ModLogger.LogWarning($"Bookmark: Not found");
                    return false;
                }
                ModLogger.LogDebug($"Removing bookmark for bookmarkType {bookmark.GetBookmarkType()} and bookmarkID {bookmark.GetBookmarkID()}");
                
                _bookmarks.Remove(bookmark);
                _bookmarksIDs.removeId(bookmark.GetBookmarkType(), bookmark.GetBookmarkID());
                
                OnBookmarksUpdated.Fire();
                return true;
            } catch (Exception e) {
                ModLogger.LogError($"Error removing bookmark for bookmarkType {bookmark.GetBookmarkType()} and bookmarkID {bookmark.GetBookmarkID()}: {e.Message}");
                return false;
            }
        }

        // =======================================================================================

        /// <summary>
        /// Move a bookmark up in the order (decrease Order value)
        /// </summary>
        /// <param name="bookmark">The bookmark to move up</param>
        /// <returns>True if the bookmark was moved up, false otherwise</returns>
        public bool MoveBookmarkUp(Bookmark bookmark) {
            try {
                if (bookmark == null) {
                    ModLogger.LogWarning($"Bookmark: Null");
                    return false;
                }
                ModLogger.LogDebug($"Moving bookmark up for bookmarkType {bookmark.GetBookmarkType()} and bookmarkID {bookmark.GetBookmarkID()}");
                
                if( bookmark.Order <= 0 ) {
                    ModLogger.LogError($"Bookmark: Cannot be moved up: Order is {bookmark.Order}");
                    return false;
                }

                // Find the bookmark with previous Order
                Bookmark previous = _bookmarks.Find(b => b.Order == bookmark.Order - 1);
                if( previous == null ) {
                    ModLogger.LogWarning($"Bookmark: Previous bookmark not found");
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
                ModLogger.LogError($"Error moving bookmark up: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Move a bookmark down in the order (increase Order value)
        /// </summary>
        /// <param name="bookmark">The bookmark to move down</param>
        /// <returns>True if the bookmark was moved down, false otherwise</returns>
        public bool MoveBookmarkDown(Bookmark bookmark) {
            try {
                if (bookmark == null) {
                    ModLogger.LogWarning($"Bookmark: Null");
                    return false;
                }
                ModLogger.LogDebug($"Moving bookmark down for bookmarkType {bookmark.GetBookmarkType()} and bookmarkID {bookmark.GetBookmarkID()}");

                if( bookmark.Order >= _bookmarks.Max(b => b.Order) ) {
                    ModLogger.LogWarning($"Bookmark: Cannot be moved down: Order is {bookmark.Order}");
                    return false;
                }
                
                // Find the bookmark with next Order
                Bookmark next = _bookmarks.Find(b => b.Order == bookmark.Order + 1);
                if( next == null ) {
                    ModLogger.LogWarning($"Bookmark: Next bookmark not found");
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
                ModLogger.LogError($"Error moving bookmark down: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Swap the order of two bookmarks
        /// </summary>
        /// <param name="bookmark1">The first bookmark to swap</param>
        /// <param name="bookmark2">The second bookmark to swap</param>
        /// <returns>True if the bookmarks were swapped, false otherwise</returns>
        public bool SwapBookmarks(Bookmark bookmark1, Bookmark bookmark2) {
            try {
                if( bookmark1 == null || bookmark2 == null ) {
                    ModLogger.LogWarning($"Bookmark: Null");
                    return false;
                }
                ModLogger.LogDebug($"Swapping bookmarks for bookmarkType1 {bookmark1.GetBookmarkType()} and bookmarkID1 {bookmark1.GetBookmarkID()} and bookmarkType2 {bookmark2.GetBookmarkType()} and bookmarkID2 {bookmark2.GetBookmarkID()}");

                int temp = bookmark1.Order;
                bookmark1.Order = bookmark2.Order;
                bookmark2.Order = temp;

                this._bookmarks = this._bookmarks.OrderBy(b => b.Order).ToList();
                OnBookmarksUpdated.Fire();
                return true;
            } catch (Exception e) {
                ModLogger.LogError($"Error swapping bookmarks: {e.Message}");
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
                _bookmarksIDs.clearIds();
                
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
                        ModLogger.LogWarning($"Bookmark {bookmark.GetBookmarkType()} and {bookmark.GetBookmarkID()}: Not found");
                    }
                }

                OnBookmarksUpdated.Fire();
            } catch (Exception e) {
                ModLogger.LogError($"Error refreshing bookmarks: {e.Message}");
            }
        }
    }

    /// <summary>
    /// Class to manage the IDs of the bookmarks
    /// </summary>
    class BookmarkIds {
        private Dictionary<BookmarkType, List<uint>> _bookmarksIDs = new Dictionary<BookmarkType, List<uint>>();

        public bool hasId(BookmarkType bookmarkType, uint bookmarkID) {
            return _bookmarksIDs.ContainsKey(bookmarkType) && _bookmarksIDs[bookmarkType].Contains(bookmarkID);
        }
        public void addId(BookmarkType bookmarkType, uint bookmarkID) {
            if( !_bookmarksIDs.ContainsKey(bookmarkType) ) {
                _bookmarksIDs[bookmarkType] = new List<uint>();
            }
            _bookmarksIDs[bookmarkType].Add(bookmarkID);
        }
        public void removeId(BookmarkType bookmarkType, uint bookmarkID) {
            if( _bookmarksIDs.ContainsKey(bookmarkType) ) {
                _bookmarksIDs[bookmarkType].Remove(bookmarkID);
            }
        }
        public void clearIds() {
            _bookmarksIDs.Clear();
        }
    }
}
