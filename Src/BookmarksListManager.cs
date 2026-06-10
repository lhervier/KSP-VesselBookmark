using System;
using System.Collections.Generic;
using UnityEngine;
using com.github.lhervier.ksp.bookmarksmod.bookmarks;
using com.github.lhervier.ksp.shared;

namespace com.github.lhervier.ksp.bookmarksmod {
    
    /// <summary>
    /// Central bookmark manager for a given bookmark type
    /// </summary>
    public class BookmarksListManager : MonoBehaviour {

        private static readonly ModLogger LOGGER = new ModLogger("BookmarkDaemon");

        /// <summary>
        /// Event fired when the bookmarks are updated
        /// </summary>
        public readonly EventVoid OnBookmarksUpdated = new EventVoid("BookmarkDaemon.OnBookmarksUpdated");
        private bool _sendOnBookmarkUpdated = false;
        private bool _sceneLoading = false;

        /// <summary>
        /// List of all bookmarks
        /// </summary>
        private List<Bookmark> _bookmarks = new List<Bookmark>();
        public IReadOnlyList<Bookmark> Bookmarks => _bookmarks.AsReadOnly();
        private List<uint> _bookmarksIDs = new List<uint>();
        private Dictionary<uint, Bookmark> _bookmarksById = new Dictionary<uint, Bookmark>();

        // =======================================================================================
        // Life cycle
        // =======================================================================================

        public void Awake()
        {
            LOGGER.LogInfo("Awaked");
            DontDestroyOnLoad(this);
        }

        public void Start()
        {
            GameEvents.onGameSceneLoadRequested.Add(OnGameSceneLoadRequested);
            GameEvents.onLevelWasLoaded.Add(OnLevelWasLoaded);
        }

        public void LateUpdate()
        {
            if( _sendOnBookmarkUpdated ) 
            {
                _sendOnBookmarkUpdated = false;
                if( !_sceneLoading )
                {
                    this.OnBookmarksUpdated.Fire();
                }
            }
        }

        public void OnDestroy()
        {
            GameEvents.onGameSceneLoadRequested.Remove(OnGameSceneLoadRequested);
            GameEvents.onLevelWasLoaded.Remove(OnLevelWasLoaded);
        }

        /// <summary>
        /// A scene load has been requested: the current scene is about to be torn down (every vessel
        /// destroyed). Suppress vessel/alarm-driven refreshes until the new scene has loaded.
        /// </summary>
        private void OnGameSceneLoadRequested(GameScenes scene) {
            _sceneLoading = true;
        }

        /// <summary>
        /// The new scene has finished loading: refreshes may resume.
        /// </summary>
        private void OnLevelWasLoaded(GameScenes scene) {
            _sceneLoading = false;
        }

        // ==============================================================================================
        // Public API
        // ==============================================================================================
        
        /// <summary>
        /// Get a bookmark by its unique identifier
        /// </summary>
        /// <param name="bookmarkID">The unique identifier of the bookmark</param>
        /// <returns>The bookmark, or null if not found</returns>
        public Bookmark GetBookmark(uint bookmarkID) {
            _bookmarksById.TryGetValue(bookmarkID, out Bookmark bookmark);
            return bookmark;
        }
        
        /// <summary>
        /// Check if a bookmark exists for a command module.
        /// Note: This method has no cost and can be called frequently.
        /// </summary>
        /// <param name="bookmarkID">The unique identifier of the bookmark</param>
        /// <returns></returns>
        public bool HasBookmark(uint bookmarkID) {
            return _bookmarksIDs.Contains(bookmarkID);
        }

        /// <summary>
        /// Add a bookmark
        /// </summary>
        /// <param name="bookmark">The bookmark to add</param>
        /// <returns>True if the bookmark was added, false otherwise</returns>
        public bool AddBookmark(Bookmark bookmark) {
            try {
                if( bookmark == null ) {
                    LOGGER.LogError("Attempted to add null bookmark");
                    return false;
                }
                LOGGER.LogDebug($"Adding bookmark {bookmark}");

                // Check if bookmark already exists
                if (this.HasBookmark(bookmark.BookmarkID)) {
                    LOGGER.LogDebug($"Bookmark {bookmark} Already exists. Nothing to do...");
                    return false;
                }

                // Bookmark will be added to the end of the list
                bookmark.Order = _bookmarks.Count;

                // Add bookmark to the list
                _bookmarks.Add(bookmark);
                _bookmarksIDs.Add(bookmark.BookmarkID);
                _bookmarksById[bookmark.BookmarkID] = bookmark;

                // Fire event
                this._sendOnBookmarkUpdated = true;
                
                return true;
            } catch (Exception e) {
                LOGGER.LogError($"Error adding bookmark {bookmark}: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Clear all bookmarks from the instance
        /// </summary>
        public void ClearBookmarks() {
            LOGGER.LogDebug($"Clearing bookmarks");
            _bookmarks.Clear();
            _bookmarksIDs.Clear();
            _bookmarksById.Clear();

            this._sendOnBookmarkUpdated = true;
        }

        /// <summary>
        /// Remove a bookmark from the list
        /// </summary>
        /// <param name="bookmarkID">The unique identifier of the bookmark to remove</param>
        /// <returns>True if the bookmark was removed, false otherwise</returns>
        public bool RemoveBookmark(Bookmark bookmark) {
            try {
                if( bookmark == null ) {
                    LOGGER.LogWarning($"Bookmark: Cannot remove null bookmark");
                    return false;
                }
                LOGGER.LogDebug($"Removing bookmark {bookmark}");

                _bookmarks.Remove(bookmark);
                _bookmarksIDs.Remove(bookmark.BookmarkID);
                _bookmarksById.Remove(bookmark.BookmarkID);

                for( int i = 0; i < _bookmarks.Count; i++ ) {
                    _bookmarks[i].Order = i;
                }
                
                // Fire event
                this._sendOnBookmarkUpdated = true;
                
                return true;
            } catch (Exception e) {
                LOGGER.LogError($"Error removing bookmark {bookmark}: {e.Message}");
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
                    LOGGER.LogWarning($"Unable to move bookmark up: Null bookmark");
                    return false;
                }
                LOGGER.LogDebug($"Moving bookmark {bookmark} up");
                
                _bookmarks.Remove(bookmark);
                _bookmarks.Insert(bookmark.Order - 1, bookmark);

                for( int i = 0; i < _bookmarks.Count; i++ ) {
                    _bookmarks[i].Order = i;
                }
                
                this._sendOnBookmarkUpdated = true;
                
                LOGGER.LogInfo($"Bookmark {bookmark} moved up");
                return true;
            } catch (Exception e) {
                LOGGER.LogError($"Error moving bookmark {bookmark} up: {e.Message}");
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
                    LOGGER.LogWarning($"Unable to move bookmark down: Null bookmark");
                    return false;
                }
                LOGGER.LogDebug($"Moving bookmark {bookmark} down");

                _bookmarks.Remove(bookmark);
                _bookmarks.Insert(bookmark.Order + 1, bookmark);

                for( int i = 0; i < _bookmarks.Count; i++ ) {
                    _bookmarks[i].Order = i;
                }

                this._sendOnBookmarkUpdated = true;
                
                LOGGER.LogInfo($"Bookmark {bookmark} moved down");
                return true;
            } catch (Exception e) {
                LOGGER.LogError($"Error moving bookmark {bookmark} down: {e.Message}");
                return false;
            }
        }
    }
}
