using System;
using System.Collections.Generic;
using UnityEngine;
using com.github.lhervier.ksp.bookmarksmod.bookmarks;
using Smooth.Collections;
using com.github.lhervier.ksp.shared;

namespace com.github.lhervier.ksp.bookmarksmod {
    
    /// <summary>
    /// Central bookmark manager
    /// </summary>
    public class BookmarksManager : MonoBehaviour {
 
        private static readonly ModLogger LOGGER = new ModLogger("BookmarkManager");

        public static BookmarksManager Instance => _instance;
        private static BookmarksManager _instance = null;

        /// <summary>
        /// Dictionary of instances of BookmarksMaemon for each bookmark type
        /// </summary>
        private Dictionary<BookmarkType, BookmarksListManager> _bookmarksListManagers = new Dictionary<BookmarkType, BookmarksListManager>();
        
        /// <summary>
        /// Event fired when the bookmarks are updated
        /// </summary>
        public readonly EventVoid OnBookmarksUpdated = new EventVoid("BookmarkManager.OnBookmarksUpdated");

        private BookmarkRefreshManager _bookmarkRefreshManager;
        private VesselsManager _vesselsManager;

        // =========================================================================
        // Life cycle
        // =========================================================================

        public void Awake()
        {
            _instance = this;
        }

        public void Start()
        {
            this._vesselsManager = this.gameObject.AddComponent<VesselsManager>();
            this._vesselsManager.OnVesselsChanged.Add(OnVesselsChanged);

            this._bookmarkRefreshManager = this.gameObject.AddComponent<BookmarkRefreshManager>();
            this._bookmarkRefreshManager.Initialize(this._vesselsManager);
            
            this._bookmarksListManagers[BookmarkType.CommandModule] = this.gameObject.AddComponent<BookmarksListManager>();
            this._bookmarksListManagers[BookmarkType.Vessel] = this.gameObject.AddComponent<BookmarksListManager>();
            
            foreach( BookmarksListManager daemon in this._bookmarksListManagers.Values )
            {
                daemon.OnBookmarksUpdated.Add(OnBookmarksUpdatedFromDaemon);
            }
        }

        public void OnDestroy()
        {
            foreach( BookmarksListManager manager in this._bookmarksListManagers.Values )
            {
                manager.OnBookmarksUpdated.Remove(OnBookmarksUpdatedFromDaemon);
            }
            this._vesselsManager.OnVesselsChanged.Remove(OnVesselsChanged);
            _instance = null;
        }

        private void OnBookmarksUpdatedFromDaemon()
        {
            OnBookmarksUpdated.Fire();
        }

        private void OnVesselsChanged()
        {
            this.RefreshAllBookmarks();
        }

        // ================================================================================
        // Public API
        // ================================================================================
        
        /// <summary>
        /// Get all bookmarks in any instance
        /// </summary>
        /// <returns>A list of all bookmarks</returns>
        public List<Bookmark> GetAllBookmarks() {
            List<Bookmark> bookmarks = new List<Bookmark>();
            foreach( var instance in _bookmarksListManagers ) {
                bookmarks.AddAll(instance.Value.Bookmarks);
            }
            return bookmarks;
        }

        public int GetBookmarksCount()
        {
            int n = 0;
            foreach( var instance in _bookmarksListManagers.Values ) n += instance.Bookmarks.Count;
            return n;
        }

        /// <summary>
        /// Check if a bookmark exists
        /// </summary>
        /// <param name="bookmarkType">The type of the bookmark</param>
        /// <param name="bookmarkID">The unique identifier of the bookmark</param>
        /// <returns>True if the bookmark exists, false otherwise</returns>
        public bool HasBookmark(BookmarkType bookmarkType, uint bookmarkID) {
            return _bookmarksListManagers[bookmarkType].HasBookmark(bookmarkID);
        }

        /// <summary>
        /// Get a bookmark by its unique identifier
        /// </summary>
        /// <param name="bookmarkType">The type of the bookmark</param>
        /// <param name="bookmarkID">The unique identifier of the bookmark</param>
        /// <returns>The bookmark, or null if not found</returns>
        public Bookmark GetBookmark(BookmarkType bookmarkType, uint bookmarkID) {
            return _bookmarksListManagers[bookmarkType].GetBookmark(bookmarkID);
        }

        /// <summary>
        /// Add a bookmark
        /// </summary>
        /// <param name="bookmark">The bookmark to add</param>
        /// <returns>True if the bookmark was added, false otherwise</returns>
        public bool AddBookmark(Bookmark bookmark) {
            return _bookmarksListManagers[bookmark.BookmarkType].AddBookmark(bookmark);
        }

        /// <summary>
        /// Clear all bookmarks from any instance
        /// </summary>
        public void ClearBookmarks() {
            LOGGER.LogDebug($"Clearing all bookmarks");
            foreach( var instance in _bookmarksListManagers ) {
                instance.Value.ClearBookmarks();
            }
        }

        /// <summary>
        /// Remove a bookmark from any instance
        /// </summary>
        /// <param name="bookmark">The bookmark to remove</param>
        /// <returns>True if the bookmark was removed, false otherwise</returns>
        public bool RemoveBookmark(Bookmark bookmark) {
            return _bookmarksListManagers[bookmark.BookmarkType].RemoveBookmark(bookmark);
        }

        /// <summary>
        /// Move a bookmark up in the order (decrease Order value)
        /// </summary>
        /// <param name="bookmark">The bookmark to move up</param>
        /// <returns>True if the bookmark was moved up, false otherwise</returns>
        public bool MoveBookmarkUp(Bookmark bookmark) {
            return _bookmarksListManagers[bookmark.BookmarkType].MoveBookmarkUp(bookmark);
        }

        /// <summary>
        /// Move a bookmark down in the order (increase Order value)
        /// </summary>
        /// <param name="bookmark">The bookmark to move down</param>
        /// <returns>True if the bookmark was moved down, false otherwise</returns>
        public bool MoveBookmarkDown(Bookmark bookmark) {
            return _bookmarksListManagers[bookmark.BookmarkType].MoveBookmarkDown(bookmark);
        }

        /// <summary>
        /// Request a rebuild of the lookup index. The rebuild (coalesced to once per frame) refreshes
        /// every bookmark and notifies the UI through <see cref="RefreshAllBookmarks"/>. Use this for
        /// on-demand refreshes (opening the window, loading bookmarks) so we never refresh against a
        /// stale index.
        /// </summary>
        public void ForceReload()
        {
            if( this._vesselsManager != null )
            {
                this._vesselsManager.RequestRefresh();
            }
        }

        // =========================================================================================================
        // Loading and saving
        // =========================================================================================================

        /// <summary>
        /// Load bookmarks from config node
        /// </summary>
        /// <param name="node"></param>
        public void LoadBookmarks(ConfigNode node) {
            try {
                LOGGER.LogInfo($"Loading bookmarks");
                
                // Load bookmarks from config node
                List<Bookmark> bookmarks = BookmarksPersistenceManager.LoadBookmarks(node);
                
                // Sort bookmarks, so we will add them in the correct order
                // Two bookmarks of two different types can have the same order here !
                bookmarks.Sort((a, b) => a.Order.CompareTo(b.Order));

                // Add bookmarks to the list (without refreshing each one : we batch a single indexed
                // refresh below instead of rescanning the universe once per bookmark).
                // Best-effort resolution against the current index (useful when reloading mid-session,
                // where the index is already warm). On a cold load the index is still empty here, but
                // RefreshIndex rebuilds on onLevelWasLoaded and re-resolves everything (+ notifies the
                // UI) once the scene's vessels are present.
                ClearBookmarks();
                foreach (Bookmark bookmark in bookmarks) {
                    _bookmarkRefreshManager.RefreshBookmark(bookmark);
                    AddBookmark(bookmark);
                }

                LOGGER.LogInfo($"{bookmarks.Count} bookmark(s) loaded");
            } catch (Exception e) {
                LOGGER.LogError($"Error loading bookmarks: {e.Message}");
            }
        }

        /// <summary>
        /// Save bookmarks to config node
        /// </summary>
        /// <param name="node"></param>
        public void SaveBookmarks(ConfigNode node) {
            try {
                LOGGER.LogInfo($"Saving bookmarks");
                List<Bookmark> bookmarks = GetAllBookmarks();
                BookmarksPersistenceManager.SaveBookmarks(node, bookmarks);
                LOGGER.LogInfo($"{bookmarks.Count} bookmark(s) saved");
            } catch (Exception e) {
                LOGGER.LogError($"Error saving bookmarks: {e.Message}");
            }
        }

        // =========================================================================
        // Private helpers
        // =========================================================================

        /// <summary>
        /// Refresh every bookmark against the current index, then notify the UI. Called once per frame
        /// by <see cref="OnVesselsChanged"/> whenever the index has been rebuilt.
        /// </summary>
        private void RefreshAllBookmarks()
        {
            foreach( BookmarksListManager daemon in _bookmarksListManagers.Values )
            {
                foreach( Bookmark bookmark in daemon.Bookmarks )
                {
                    this._bookmarkRefreshManager.RefreshBookmark(bookmark);
                }
            }
            OnBookmarksUpdated.Fire();
        }
    }
}
