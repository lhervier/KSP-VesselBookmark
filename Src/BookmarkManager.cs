using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using com.github.lhervier.ksp.bookmarksmod.bookmarks;
using Smooth.Collections;

namespace com.github.lhervier.ksp.bookmarksmod {
    
    /// <summary>
    /// Central bookmark manager
    /// </summary>
    public class BookmarkManager {
        
        /// <summary>
        /// Dictionary of instances of BookmarkManager for each bookmark type
        /// </summary>
        private static Dictionary<BookmarkType, BookmarkManager> _instances = new Dictionary<BookmarkType, BookmarkManager>();
        public static Dictionary<BookmarkType, BookmarkManager> Instances => _instances;
        public static BookmarkManager GetInstance(BookmarkType bookmarkType) {
            if( !_instances.ContainsKey(bookmarkType) ) {
                _instances[bookmarkType] = new BookmarkManager(bookmarkType);
            }
            return _instances[bookmarkType];
        }
        

        /// <summary>
        /// Event fired when the bookmarks are updated
        /// </summary>
        public static readonly EventVoid OnBookmarksUpdated = new EventVoid("VesselBookmarkManager.OnBookmarksUpdated");

        /// <summary>
        /// Get all bookmarks in any instance
        /// </summary>
        /// <returns>A list of all bookmarks</returns>
        public static List<Bookmark> GetAllBookmarks() {
            List<Bookmark> bookmarks = new List<Bookmark>();
            foreach( var instance in _instances ) {
                bookmarks.AddAll(instance.Value.Bookmarks);
            }
            return bookmarks;
        }

        /// <summary>
        /// Check if a bookmark exists
        /// </summary>
        /// <param name="bookmarkType">The type of the bookmark</param>
        /// <param name="bookmarkID">The unique identifier of the bookmark</param>
        /// <returns>True if the bookmark exists, false otherwise</returns>
        public static bool HasBookmark(BookmarkType bookmarkType, uint bookmarkID) {
            return GetInstance(bookmarkType).HasBookmark(bookmarkID);
        }

        /// <summary>
        /// Get a bookmark by its unique identifier
        /// </summary>
        /// <param name="bookmarkType">The type of the bookmark</param>
        /// <param name="bookmarkID">The unique identifier of the bookmark</param>
        /// <returns>The bookmark, or null if not found</returns>
        public static Bookmark GetBookmark(BookmarkType bookmarkType, uint bookmarkID) {
            return GetInstance(bookmarkType).GetBookmark(bookmarkID);
        }

        /// <summary>
        /// Add a bookmark
        /// </summary>
        /// <param name="bookmark">The bookmark to add</param>
        /// <param name="sendEvent">True if the OnBookmarksUpdated event should be fired, false otherwise</param>
        /// <returns>True if the bookmark was added, false otherwise</returns>
        public static bool AddBookmark(Bookmark bookmark, bool sendEvent = true) {
            return GetInstance(bookmark.BookmarkType)._AddBookmark(bookmark, sendEvent);
        }

        /// <summary>
        /// Remove a bookmark from any instance
        /// </summary>
        /// <param name="bookmark">The bookmark to remove</param>
        /// <param name="sendEvent">True if the OnBookmarksUpdated event should be fired, false otherwise</param>
        /// <returns>True if the bookmark was removed, false otherwise</returns>
        public static bool RemoveBookmark(Bookmark bookmark, bool sendEvent = true) {
            return GetInstance(bookmark.BookmarkType)._RemoveBookmark(bookmark, sendEvent);
        }

        /// <summary>
        /// Move a bookmark up in the order (decrease Order value)
        /// </summary>
        /// <param name="bookmark">The bookmark to move up</param>
        /// <param name="sendEvent">True if the OnBookmarksUpdated event should be fired, false otherwise</param>
        /// <returns>True if the bookmark was moved up, false otherwise</returns>
        public static bool MoveBookmarkUp(Bookmark bookmark, bool sendEvent = true) {
            return GetInstance(bookmark.BookmarkType)._MoveBookmarkUp(bookmark, sendEvent);
        }

        /// <summary>
        /// Move a bookmark down in the order (increase Order value)
        /// </summary>
        /// <param name="bookmark">The bookmark to move down</param>
        /// <param name="sendEvent">True if the OnBookmarksUpdated event should be fired, false otherwise</param>
        /// <returns>True if the bookmark was moved down, false otherwise</returns>
        public static bool MoveBookmarkDown(Bookmark bookmark, bool sendEvent = true) {
            return GetInstance(bookmark.BookmarkType)._MoveBookmarkDown(bookmark, sendEvent);
        }

        /// <summary>
        /// Refresh all bookmarks in any instance
        /// </summary>
        /// <param name="sendEvent">True if the OnBookmarksUpdated event should be fired, false otherwise</param>
        public static void RefreshBookmarks(bool sendEvent = true) {
            foreach( var instance in _instances ) {
                instance.Value._RefreshBookmarks(false);
            }
            if( sendEvent ) {
                OnBookmarksUpdated.Fire();
            }
        }

        /// <summary>
        /// Load bookmarks from config node
        /// </summary>
        /// <param name="node"></param>
        public static void LoadBookmarks(ConfigNode node) {
            try {
                ModLogger.LogDebug($"Loading bookmarks from config node");
                
                // Load bookmarks from config node
                List<Bookmark> bookmarks = BookmarkPersistenceManager.LoadBookmarks(node);
                
                // Sort bookmarks, so we will add them in the correct order
                // Two bookmarks of two different types can have the same order here !
                bookmarks.Sort((a, b) => a.Order.CompareTo(b.Order));

                // Add bookmarks to the list
                foreach (Bookmark bookmark in bookmarks) {
                    GetInstance(bookmark.BookmarkType)._AddBookmark(bookmark, false);
                }

                OnBookmarksUpdated.Fire();
                ModLogger.LogInfo($"{bookmarks.Count} bookmark(s) loaded");    
            } catch (Exception e) {
                ModLogger.LogError($"Error loading bookmarks: {e.Message}");
            }
        }

        /// <summary>
        /// Save bookmarks to config node
        /// </summary>
        /// <param name="node"></param>
        public static void SaveBookmarks(ConfigNode node) {
            List<Bookmark> bookmarks = GetAllBookmarks();
            BookmarkPersistenceManager.SaveBookmarks(node, bookmarks);
        }

        // ==============================================================================================
        
        /// <summary>
        /// Type of the bookmarks managed by this instance
        /// </summary>
        private BookmarkType _bookmarkType;
        public BookmarkType BookmarkType => _bookmarkType;

        /// <summary>
        /// List of all bookmarks
        /// </summary>
        private List<Bookmark> _bookmarks = new List<Bookmark>();
        public IReadOnlyList<Bookmark> Bookmarks => _bookmarks.AsReadOnly();
        private List<uint> _bookmarksIDs = new List<uint>();

        // =======================================================================================

        private BookmarkManager(BookmarkType bookmarkType) {
            _bookmarkType = bookmarkType;
        }

        /// <summary>
        /// Get a bookmark by its unique identifier
        /// </summary>
        /// <param name="bookmarkID">The unique identifier of the bookmark</param>
        /// <returns>The bookmark, or null if not found</returns>
        public Bookmark GetBookmark(uint bookmarkID) {
            return _bookmarks.FirstOrDefault(
                b => b.BookmarkID == bookmarkID
            );
        }
        
        /// <summary>
        /// Check if a bookmark exists for a command module.
        /// Note: This method has no cost and can be called frequently.
        /// </summary>
        /// <param name="bookmarkID">The unique identifier of the bookmark</param>
        /// <returns></returns>
        public bool HasBookmark(uint bookmarkID) {
            try {
                return _bookmarksIDs.Contains(bookmarkID);
            } catch (Exception e) {
                ModLogger.LogError($"Error checking if bookmark exists for bookmarkType {_bookmarkType} and bookmarkID {bookmarkID}: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Add a bookmark
        /// </summary>
        /// <param name="bookmark">The bookmark to add</param>
        /// <param name="sendEvent">True if the OnBookmarksUpdated event should be fired, false otherwise</param>
        /// <returns>True if the bookmark was added, false otherwise</returns>
        private bool _AddBookmark(Bookmark bookmark, bool sendEvent = true) {
            try {
                if( bookmark == null ) {
                    ModLogger.LogError("Attempted to add null bookmark");
                    return false;
                }
                ModLogger.LogDebug($"Adding bookmark for bookmarkType {bookmark.BookmarkType} and bookmarkID {bookmark.BookmarkID}");
                
                if( bookmark.BookmarkType != _bookmarkType ) {
                    ModLogger.LogError($"Attempted to add bookmark with bookmarkType {bookmark.BookmarkType} to bookmark manager with bookmarkType {_bookmarkType}");
                    return false;
                }
                
                // Check if bookmark already exists
                if (this.HasBookmark(bookmark.BookmarkID)) {
                    ModLogger.LogWarning($"Bookmark already exists for bookmarkType {bookmark.BookmarkType} and bookmarkID {bookmark.BookmarkID}");
                    return false;
                }
                
                // Bookmark will be added to the end of the list
                bookmark.Order = _bookmarks.Count;
                
                // Refresh bookmark to load transient fields
                if( !BookmarkRefreshManager.RefreshBookmark(bookmark) ) {
                    ModLogger.LogWarning($"Bookmark {bookmark.BookmarkType} and {bookmark.BookmarkID}: Failed to refresh bookmark");
                    return false;
                }

                // Add bookmark to the list
                _bookmarks.Add(bookmark);
                _bookmarksIDs.Add(bookmark.BookmarkID);

                // Fire event if requested
                if( sendEvent ) {
                    OnBookmarksUpdated.Fire();
                }

                return true;
            } catch (Exception e) {
                ModLogger.LogError($"Error adding bookmark for bookmarkType {bookmark.BookmarkType} and bookmarkID {bookmark.BookmarkID}: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Remove a bookmark from the list
        /// </summary>
        /// <param name="bookmarkID">The unique identifier of the bookmark to remove</param>
        /// <returns>True if the bookmark was removed, false otherwise</returns>
        private bool _RemoveBookmark(Bookmark bookmark, bool sendEvent = true) {
            try {
                if( bookmark == null ) {
                    ModLogger.LogWarning($"Bookmark: Not found");
                    return false;
                }
                ModLogger.LogDebug($"Removing bookmark for bookmarkType {bookmark.BookmarkType} and bookmarkID {bookmark.BookmarkID}");

                if( bookmark.BookmarkType != _bookmarkType ) {
                    ModLogger.LogError($"Attempted to remove bookmark with bookmarkType {bookmark.BookmarkType} from bookmark manager with bookmarkType {_bookmarkType}");
                    return false;
                }
                
                _bookmarks.Remove(bookmark);
                _bookmarksIDs.Remove(bookmark.BookmarkID);

                for( int i = 0; i < _bookmarks.Count; i++ ) {
                    _bookmarks[i].Order = i;
                }
                
                // Fire event if requested
                if( sendEvent ) {
                    OnBookmarksUpdated.Fire();
                }

                return true;
            } catch (Exception e) {
                ModLogger.LogError($"Error removing bookmark for bookmarkType {bookmark.BookmarkType} and bookmarkID {bookmark.BookmarkID}: {e.Message}");
                return false;
            }
        }

        // =======================================================================================

        /// <summary>
        /// Move a bookmark up in the order (decrease Order value)
        /// </summary>
        /// <param name="bookmark">The bookmark to move up</param>
        /// <returns>True if the bookmark was moved up, false otherwise</returns>
        private bool _MoveBookmarkUp(Bookmark bookmark, bool sendEvent = true) {
            try {
                if (bookmark == null) {
                    ModLogger.LogWarning($"Bookmark: Null");
                    return false;
                }
                ModLogger.LogDebug($"Moving bookmark up for bookmarkType {bookmark.BookmarkType} and bookmarkID {bookmark.BookmarkID}");
                
                if( bookmark.BookmarkType != _bookmarkType ) {
                    ModLogger.LogError($"Attempted to move bookmark with bookmarkType {bookmark.BookmarkType} up in bookmark manager with bookmarkType {_bookmarkType}");
                    return false;
                }

                _bookmarks.Remove(bookmark);
                _bookmarks.Insert(bookmark.Order - 1, bookmark);

                for( int i = 0; i < _bookmarks.Count; i++ ) {
                    _bookmarks[i].Order = i;
                }
                
                if( sendEvent ) {
                    OnBookmarksUpdated.Fire();
                }

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
        private bool _MoveBookmarkDown(Bookmark bookmark, bool sendEvent = true) {
            try {
                if (bookmark == null) {
                    ModLogger.LogWarning($"Bookmark: Null");
                    return false;
                }
                ModLogger.LogDebug($"Moving bookmark down for bookmarkType {bookmark.BookmarkType} and bookmarkID {bookmark.BookmarkID}");

                if( bookmark.BookmarkType != _bookmarkType ) {
                    ModLogger.LogError($"Attempted to move bookmark with bookmarkType {bookmark.BookmarkType} down in bookmark manager with bookmarkType {_bookmarkType}");
                    return false;
                }
                
                _bookmarks.Remove(bookmark);
                _bookmarks.Insert(bookmark.Order + 1, bookmark);

                for( int i = 0; i < _bookmarks.Count; i++ ) {
                    _bookmarks[i].Order = i;
                }

                if( sendEvent ) {
                    OnBookmarksUpdated.Fire();
                }

                return true;
            } catch (Exception e) {
                ModLogger.LogError($"Error moving bookmark down: {e.Message}");
                return false;
            }
        }

        // =======================================================================================
        
        /// <summary>
        /// Refresh all bookmarks in the instance
        /// </summary>
        /// <param name="sendEvent">True if the OnBookmarksUpdated event should be fired, false otherwise</param>
        private void _RefreshBookmarks(bool sendEvent = true) {
            try {
                ModLogger.LogDebug($"Refreshing bookmarks for bookmarkType {_bookmarkType}");

                foreach (Bookmark bookmark in _bookmarks) {
                    if( !BookmarkRefreshManager.RefreshBookmark(bookmark) ) {
                        ModLogger.LogWarning($"Bookmark {bookmark.BookmarkType} and {bookmark.BookmarkID}: Not found");
                    }
                }

                if( sendEvent ) {
                    OnBookmarksUpdated.Fire();
                }
            } catch (Exception e) {
                ModLogger.LogError($"Error refreshing bookmarks: {e.Message}");
            }
        }
    }
}
