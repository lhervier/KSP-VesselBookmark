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

        private const string ALL_VESSEL_TYPES = "All";

        /// <summary>
        /// Whether the main windows are visible
        /// </summary>
        public bool MainWindowsVisible { get; set; } = false;

        /// <summary>
        /// The ID of the hovered bookmark
        /// </summary>
        public uint HoveredBookmarkID { get; set; } = 0;
        public BookmarkType HoveredBookmarkType { get; set; } = BookmarkType.Unknown;
        
        // Bookmarks list to display in the UI (cached for performance)
        // Only for read access
        private Dictionary<BookmarkType, List<Bookmark>> _availableBookmarks = new Dictionary<BookmarkType, List<Bookmark>>();
        public IReadOnlyDictionary<BookmarkType, List<Bookmark>> AvailableBookmarks => _availableBookmarks;
        
        /// <summary>
        /// The list of available bodies
        /// </summary>
        public List<string> AvailableBodies = new List<string>();
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

        private const float SEARCH_DEBOUNCE_SECONDS = 0.2f;
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
        /// À appeler chaque frame (ex. depuis OnGUI). Déclenche RefreshBookmarks 200 ms après la dernière modification de SearchText.
        /// </summary>
        public void ProcessSearchDebounce() {
            if (_searchTextChangeTime < 0f) return;
            if (Time.realtimeSinceStartup - _searchTextChangeTime < SEARCH_DEBOUNCE_SECONDS) return;
            _searchTextChangeTime = -1f;
            
            this.UpdateBookmarksSelection();
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

        // ======================================================================

        private void UpdateAvailableVesselTypes() {
            try {
                LOGGER.LogDebug($"Updating available vessel types");
                string selectedVesselType = _selectedVesselType;
                this.AvailableVesselTypes.Clear();
                foreach (Bookmark bookmark in BookmarkManager.GetAllBookmarks()) {
                    VesselType vesselType = bookmark.BookmarkVesselType;
                    string vesselTypeName = vesselType.ToString();
                    if( !AvailableVesselTypes.Contains(vesselTypeName) ) {
                        AvailableVesselTypes.Add(vesselTypeName);
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

        // ======================================================================

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
                        Vessel vessel = bookmark.Vessel;
                        if( vessel == null ) {
                            continue;
                        }

                        bool addBookmark;
                        if( string.Equals(_selectedBody, all) && string.Equals(_selectedVesselType, ALL_VESSEL_TYPES) ) {
                            addBookmark = true;
                        } else if( string.Equals(_selectedBody, all) ) {
                            addBookmark = string.Equals(
                                bookmark.BookmarkVesselType.ToString(), 
                                _selectedVesselType
                            );
                        } else if( string.Equals(_selectedVesselType, ALL_VESSEL_TYPES) ) {
                            addBookmark = string.Equals(
                                vessel.mainBody.bodyName, 
                                _selectedBody
                            );
                        } else {
                            addBookmark = string.Equals(
                                    vessel.mainBody.bodyName, 
                                    _selectedBody
                                ) 
                                && 
                                string.Equals(
                                    bookmark.BookmarkVesselType.ToString(), 
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
            } catch (Exception e) {
                LOGGER.LogError($"Error updating available bookmarks: {e.Message}");
            }
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

        /// <summary>
        /// Whether to show the add vessel bookmark button
        /// </summary>
        /// <returns>Whether to show the add vessel bookmark button</returns>
        public bool CanAddVesselBookmark() {
            return FlightGlobals.ActiveVessel != null;
        }

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
    }
}