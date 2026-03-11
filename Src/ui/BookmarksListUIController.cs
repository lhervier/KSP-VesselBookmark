using KSP.Localization;
using KSP.UI.Screens;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using com.github.lhervier.ksp.bookmarksmod.bookmarks;
using com.github.lhervier.ksp.bookmarksmod.util;
using System;
using KSP.UI.Screens.DebugToolbar.Screens.Cheats;

namespace com.github.lhervier.ksp.bookmarksmod.ui {

    public class BookmarksListUIController {
        private static readonly ModLogger LOGGER = new ModLogger("BookmarksListUIController");
        private static readonly float SEARCH_DEBOUNCE_SECONDS = 0.2f;
        private static readonly string ALL_VESSEL_TYPES = "All";

        public EventVoid OnClosed = new EventVoid("BookmarksListUIController.OnClosed");

        /// <summary>
        /// Whether the main windows are visible
        /// </summary>
        public bool MainWindowsVisible { get; set; } = false;

        /// <summary>
        /// The currently hovered bookmark (null if none).
        /// </summary>
        private Bookmark _hoveredBookmark = null;

        /// <summary>
        /// The currently selected bookmark (stays selected when clicking the row). Null if none.
        /// </summary>
        private Bookmark _selectedBookmark = null;
        
        // Bookmarks list to display in the UI (cached for performance)
        // Only for read access
        private Dictionary<BookmarkType, List<Bookmark>> _availableBookmarks = new Dictionary<BookmarkType, List<Bookmark>>();
        public IReadOnlyDictionary<BookmarkType, List<Bookmark>> AvailableBookmarks => _availableBookmarks;
        
        /// <summary>
        /// The list of available bodies
        /// </summary>
        public List<string> AvailableBodies = new List<string>();
        
        /// <summary>
        /// The selected body
        /// </summary>
        private string _selectedBody = ModLocalization.GetString("labelAll");
        public string SelectedBody { 
            get => _selectedBody; 
            set {
                string previousValue = _selectedBody;
                _selectedBody = value;
                if( previousValue != _selectedBody ) {
                    UpdateBookmarksSelection();
                }
            }
        }
        
        /// <summary>
        /// The list of available vessel types
        /// </summary>
        public List<string> AvailableVesselTypes = new List<string>();
        
        /// <summary>
        /// The selected vessel type
        /// </summary>
        private string _selectedVesselType = ALL_VESSEL_TYPES;
        public string SelectedVesselType { 
            get => _selectedVesselType; 
            set {
                string previousValue = _selectedVesselType;
                _selectedVesselType = value;
                if( previousValue != _selectedVesselType ) {
                    UpdateBookmarksSelection();
                }
            }
        }

        /// <summary>
        /// Filtre : n'afficher que les bookmarks qui ont un commentaire (pour usage futur).
        /// </summary>
        private bool _filterHasComment = false;
        public bool FilterHasComment { 
            get => _filterHasComment;
            set {
                if( _filterHasComment != value ) {
                    _filterHasComment = value;
                    UpdateBookmarksSelection();
                }
            }
        }

        /// <summary>
        /// The text in the search box
        /// </summary>
        private string _searchText = string.Empty;
        private float _searchTextChangeTime = -1f;

        /// <summary>
        /// Texte saisi dans la zone de recherche/filtre (pour usage futur).
        /// Au changement, déclenche un rafraîchissement des signets après 200 ms sans frappe.
        /// </summary>
        public string SearchText {
            get => _searchText;
            set {
                if (value == _searchText) return;
                _searchText = value ?? string.Empty;
                _searchTextChangeTime = Time.realtimeSinceStartup;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public BookmarksListUIController() {
            UpdateBookmarksSelection();
        }

        /// <summary>
        /// Update the bookmarks selection
        /// </summary>
        public void UpdateBookmarksSelection() {
            try {
                LOGGER.LogDebug($"Updating bookmarks");
                
                this.UpdateAvailableBodies();
                this.UpdateAvailableVesselTypes();
                this.UpdateAvailableBookmarks();
            } catch (Exception e) {
                LOGGER.LogError($"Error updating bookmarks: {e.Message}");
            }
        }

        // ======================================================================
        //  Update of the available bookmarks, bodies and vessel types
        // ======================================================================

        /// <summary>
        /// Update the available bodies
        /// </summary>
        private void UpdateAvailableBodies() {
            try {
                LOGGER.LogDebug($"Updating available bodies");
                string selectedBody = _selectedBody;
                this.AvailableBodies.Clear();
                foreach (Bookmark bookmark in BookmarkManager.GetAllBookmarks()) {
                    Vessel vessel = bookmark.Vessel;
                    if( vessel == null ) {
                        continue;
                    }
                    string vesselBodyName = vessel.mainBody.bodyName;
                    if( !AvailableBodies.Contains(vesselBodyName) ) {
                        AvailableBodies.Add(vesselBodyName);
                    }
                }
                // Sort bodies by distance to Kerbol with moons interleaved
                this.AvailableBodies = CelestialBodySorter.SortBodyNames(this.AvailableBodies);
                this.AvailableBodies.Insert(0, ModLocalization.GetString("labelAll"));
                
                if( !this.AvailableBodies.Contains(selectedBody) ) {
                    _selectedBody = this.AvailableBodies[0];
                } else {
                    _selectedBody = selectedBody;
                }
            } catch (Exception e) {
                LOGGER.LogError($"Error updating available bodies: {e.Message}");
            }
        }

        private void UpdateAvailableVesselTypes() {
            try {
                LOGGER.LogDebug($"Updating available vessel types");
                string selectedVesselType = _selectedVesselType;
                this.AvailableVesselTypes.Clear();
                foreach (Bookmark bookmark in BookmarkManager.GetAllBookmarks()) {
                    if( !AvailableVesselTypes.Contains(bookmark.BookmarkVesselType) ) {
                        AvailableVesselTypes.Add(bookmark.BookmarkVesselType);
                    }
                }
                this.AvailableVesselTypes.Sort();
                this.AvailableVesselTypes.Insert(0, ALL_VESSEL_TYPES);

                if( !this.AvailableVesselTypes.Contains(selectedVesselType) ) {
                    _selectedVesselType = this.AvailableVesselTypes[0];
                } else {
                    _selectedVesselType = selectedVesselType;
                }
            } catch (Exception e) {
                LOGGER.LogError($"Error updating available vessel types: {e.Message}");
            }
        }

        /// <summary>
        /// Update the available bookmarks
        /// </summary>
        private void UpdateAvailableBookmarks() {
            try {
                LOGGER.LogDebug($"Updating available bookmarks");
                this._availableBookmarks.Clear();
                
                string all = ModLocalization.GetString("labelAll");
                
                foreach( var instance in BookmarkManager.Instances.Values ) {
                    List<Bookmark> selectionedBookmarks = new List<Bookmark>();
                    foreach( var bookmark in instance.Bookmarks ) {
                        bool addBookmark;
                        if( string.Equals(_selectedBody, all) && string.Equals(_selectedVesselType, ALL_VESSEL_TYPES) ) {
                            addBookmark = true;
                        } else if( string.Equals(_selectedBody, all) ) {
                            addBookmark = string.Equals(
                                bookmark.BookmarkVesselType, 
                                _selectedVesselType
                            );
                        } else if( string.Equals(_selectedVesselType, ALL_VESSEL_TYPES) ) {
                            addBookmark = string.Equals(
                                bookmark.VesselBodyName, 
                                _selectedBody
                            );
                        } else {
                            addBookmark = string.Equals(
                                    bookmark.VesselBodyName, 
                                    _selectedBody
                                ) 
                                && 
                                string.Equals(
                                    bookmark.BookmarkVesselType, 
                                    _selectedVesselType
                                );
                        }

                        if( addBookmark && !string.IsNullOrEmpty(SearchText) ) {
                            string fullSearchText = bookmark.BookmarkTitle + " ";
                            fullSearchText += bookmark.VesselSituationLabel + " ";   // Situation contains celestial body name
                            fullSearchText += bookmark.VesselName + " ";
                            fullSearchText += ModLocalization.GetString("vesselType" + bookmark.BookmarkVesselType) + " ";
                            fullSearchText += bookmark.Comment + " ";
                            if( !fullSearchText.ToLower().Contains(SearchText.ToLower()) ) {
                                addBookmark = false;
                            }
                        }

                        if( addBookmark && FilterHasComment ) {
                            if( string.IsNullOrEmpty(bookmark.Comment) ) {
                                addBookmark = false;
                            }
                        }

                        if( addBookmark ) {
                            selectionedBookmarks.Add(bookmark);
                        }
                    }
                    _availableBookmarks[instance.BookmarkType] = selectionedBookmarks;
                }

                // Clear selection if the selected bookmark is no longer in the filtered list
                if (_selectedBookmark != null && !_availableBookmarks.Values.Any(list => list.Contains(_selectedBookmark))) {
                    _selectedBookmark = null;
                }
            } catch (Exception e) {
                LOGGER.LogError($"Error updating available bookmarks: {e.Message}");
            }
        }

        // ======================================================================
        //  Main window actions
        // ======================================================================

        /// <summary>
        /// Whether to show the add vessel bookmark button
        /// </summary>
        /// <returns>Whether to show the add vessel bookmark button</returns>
        public bool CanAddVesselBookmark() {
            return FlightGlobals.ActiveVessel != null;
        }

        /// <summary>
        /// Add a vessel bookmark
        /// </summary>
        public void AddVesselBookmark() {
            if( !CanAddVesselBookmark() ) {
                LOGGER.LogWarning("Cannot add vessel bookmark: no active vessel");
                return;
            }

            uint vesselPersistentID = FlightGlobals.ActiveVessel.persistentId;
            
            BookmarkManager manager = BookmarkManager.GetInstance(BookmarkType.Vessel);
            if (manager.HasBookmark(vesselPersistentID)) {
                ScreenMessages.PostScreenMessage(
                    ModLocalization.GetString("messageBookmarkAlreadyExists"),
                    2f,
                    ScreenMessageStyle.UPPER_CENTER
                );
                return;
            }

            VesselBookmark bookmark = new VesselBookmark(vesselPersistentID);
            if (BookmarkManager.AddBookmark(bookmark)) {
                ScreenMessages.PostScreenMessage(
                    ModLocalization.GetString("messageBookmarkAdded"),
                    2f,
                    ScreenMessageStyle.UPPER_CENTER
                );
            }
        }

        public void RefreshBookmarks() {
            BookmarkManager.RefreshBookmarks();
        }

        /// <summary>
        /// Close the main windows
        /// </summary>
        public void CloseMainWindows() {
            MainWindowsVisible = false;
            this.OnClosed.Fire();
        }

        // =============================================================================
        //  Filters
        // =============================================================================

        /// <summary>
        /// À appeler chaque frame (ex. depuis OnGUI). Déclenche RefreshBookmarks 200 ms après la dernière modification de SearchText.
        /// </summary>
        public void ProcessSearchDebounce() {
            if (_searchTextChangeTime < 0f) return;
            if (Time.realtimeSinceStartup - _searchTextChangeTime < SEARCH_DEBOUNCE_SECONDS) return;
            _searchTextChangeTime = -1f;
            
            this.UpdateBookmarksSelection();
        }

        /// <summary>
        /// Clear the filters
        /// </summary>
        public void ClearFilters() {
            _selectedBody = ModLocalization.GetString("labelAll");
            _selectedVesselType = ALL_VESSEL_TYPES;
            _searchText = string.Empty;
            _filterHasComment = false;
            UpdateBookmarksSelection();
        }

        // ======================================================================
        //  Bookmark states
        // ======================================================================

        /// <summary>
        /// Whether the given bookmark is hovered
        /// </summary>
        /// <param name="bookmark">The bookmark to check</param>
        /// <returns>Whether the given bookmark is hovered</returns>
        public bool IsHovered(Bookmark bookmark) {
            return bookmark != null && _hoveredBookmark == bookmark;
        }

        /// <summary>
        /// Set the hovered bookmark
        /// </summary>
        /// <param name="bookmark">The bookmark to set as hovered</param>
        public void SetHovered(Bookmark bookmark) {
            _hoveredBookmark = bookmark;
        }

        /// <summary>
        /// Returns the currently hovered bookmark, or null if none.
        /// </summary>
        public Bookmark GetHoveredBookmark() {
            return _hoveredBookmark;
        }

        /// <summary>
        /// Whether the given bookmark is selected
        /// </summary>
        /// <param name="bookmark">The bookmark to check</param>
        /// <returns>Whether the given bookmark is selected</returns>
        public bool IsSelected(Bookmark bookmark) {
            return bookmark != null && _selectedBookmark == bookmark;
        }

        /// <summary>
        /// Set the selected bookmark
        /// </summary>
        /// <param name="bookmark">The bookmark to set as selected</param>
        public void SetSelected(Bookmark bookmark) {
            _selectedBookmark = bookmark;
        }

        /// <summary>
        /// Returns the currently selected bookmark, or null if none.
        /// </summary>
        public Bookmark GetSelectedBookmark() {
            return _selectedBookmark;
        }

        /// <summary>
        /// Whether the selected bookmark can be edited
        /// </summary>
        /// <returns>Whether the selected bookmark can be edited</returns>
        public bool CanEditCurrentVesselComment() {
            return _selectedBookmark != null;
        }

        /// <summary>
        /// Whether "Set target as" is available for the given bookmark (active vessel exists, bookmark has vessel, not active vessel).
        /// </summary>
        public bool CanSetCurrentVesselAsTarget() {
            if (_selectedBookmark == null || FlightGlobals.ActiveVessel == null || _selectedBookmark.Vessel == null) {
                return false;
            }
            return _selectedBookmark.VesselPersistentID != FlightGlobals.ActiveVessel.persistentId;
        }

        /// <summary>
        /// Whether "Switch to vessel" is available for the given bookmark (vessel exists, not active vessel).
        /// </summary>
        public bool CanSwitchToCurrentVessel() {
            if (_selectedBookmark == null || _selectedBookmark.Vessel == null) {
                return false;
            }
            return _selectedBookmark.VesselPersistentID != FlightGlobals.ActiveVessel?.persistentId;
        }
        
        // ======================================================================
        //  Current selected bookmark actions
        // ======================================================================

        /// <summary>
        /// Set the target as the given bookmark
        /// </summary>
        public void SetCurrentVesselAsTarget() {
            if( _selectedBookmark == null ) {
                LOGGER.LogWarning("Cannot set current vessel as target: no selected bookmark");
                return;
            }
            FlightGlobals flightGlobals = FlightGlobals.fetch;
            if( flightGlobals == null ) {
                LOGGER.LogWarning($"Bookmark {_selectedBookmark}: FlightGlobals not found. Cannot set target as.");
                return;
            }
            flightGlobals.SetVesselTarget(_selectedBookmark.Vessel);
        }

        /// <summary>
        /// Switch to the given bookmark
        /// </summary>
        public void SwitchToCurrentVessel() { 
            if( _selectedBookmark == null ) {
                LOGGER.LogWarning("Cannot switch to current vessel: no selected bookmark");
                return;
            }
            Vessel vessel = _selectedBookmark.Vessel;
            if( vessel == null ) {
                LOGGER.LogWarning($"Bookmark {_selectedBookmark}: Vessel not found. Cannot switch to vessel.");
                return;
            }
            VesselNavigator.NavigateToVessel(_selectedBookmark.Vessel);
        }

        // ======================================================================
        //  Bookmark actions
        // ======================================================================

        /// <summary>
        /// Move the given bookmark up
        /// </summary>
        /// <param name="bookmark">The bookmark to move up</param>
        public void MoveUp(Bookmark bookmark) {
            this.SetSelected(bookmark);
            List<Bookmark> bookmarks = AvailableBookmarks[bookmark.BookmarkType];

            int index = bookmarks.IndexOf(bookmark);
            Bookmark previousBookmark = bookmarks[index - 1];

            while( bookmark.Order > previousBookmark.Order ) {
                BookmarkManager.MoveBookmarkUp(bookmark, false);
            }
            BookmarkManager.OnBookmarksUpdated.Fire();
        }

        /// <summary>
        /// Move the given bookmark down
        /// </summary>
        /// <param name="bookmark">The bookmark to move down</param>
        public void MoveDown(Bookmark bookmark) {
            this.SetSelected(bookmark);
            List<Bookmark> bookmarks = AvailableBookmarks[bookmark.BookmarkType];
            int index = bookmarks.IndexOf(bookmark);
            Bookmark nextBookmark = bookmarks[index + 1];

            while( bookmark.Order < nextBookmark.Order ) {
                BookmarkManager.MoveBookmarkDown(bookmark, false);
            }
            BookmarkManager.OnBookmarksUpdated.Fire();
        }

        /// <summary>
        /// Remove the given bookmark
        /// </summary>
        /// <param name="bookmark">The bookmark to remove</param>
        public void Remove(Bookmark bookmark) {
            if( this.IsSelected(bookmark) ) {
                this.SetSelected(null);
            }

            // Close main window temporarily to ensure confirmation dialog appears on top
            bool wasMainWindowVisible = MainWindowsVisible;
            MainWindowsVisible = false;
            
            string displayName = bookmark.BookmarkTitle;
            VesselBookmarkUIDialog.ConfirmRemoval(
                () => {
                    BookmarkManager.RemoveBookmark(bookmark);
                    MainWindowsVisible = wasMainWindowVisible;
                },
                () => {
                    MainWindowsVisible = wasMainWindowVisible;
                },
                bookmarkName: displayName
            );
        }
    }
}