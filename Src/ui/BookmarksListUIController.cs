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
        private List<Bookmark> _availableBookmarks = new List<Bookmark>();
        public IReadOnlyList<Bookmark> AvailableBookmarks => _availableBookmarks.AsReadOnly();
        
        /// <summary>
        /// The list of available bodies
        /// </summary>
        private List<string> _availableBodies = new List<string>();
        public IReadOnlyList<string> AvailableBodies => _availableBodies.AsReadOnly();
        private int _selectedBodyIndex = 0;
        
        /// <summary>
        /// The list of available vessel types
        /// </summary>
        private List<string> _availableVesselTypes = new List<string>();
        public IReadOnlyList<string> AvailableVesselTypes => _availableVesselTypes.AsReadOnly();
        private int _selectedVesselTypeIndex = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        public BookmarksListUIController() {
            UpdateBookmarks();
        }

        // ======================================================================

        /// <summary>
        /// Get the selected body
        /// </summary>
        /// <returns>The selected body</returns>
        public string GetSelectedBody() {
            if( _selectedBodyIndex >= 0 && _selectedBodyIndex < _availableBodies.Count ) {
                return _availableBodies[_selectedBodyIndex];
            } else {
                return ModLocalization.GetString("labelAll");
            }
        }

        /// <summary>
        /// Select a body
        /// </summary>
        /// <param name="bodyName">The name of the body to select</param>
        /// <param name="updateBookmarks">Whether to update the bookmarks list</param>
        public void SelectBody(string bodyName, bool updateBookmarks = true) {
            if( this._availableBodies.Contains(bodyName) ) {
                this._selectedBodyIndex = this._availableBodies.IndexOf(bodyName);
            } else {
                this._selectedBodyIndex = 0;
            }
            if( updateBookmarks ) {
                UpdateBookmarks();
            }
        }

        /// <summary>
        /// Select the next body
        /// </summary>
        public void SelectNextBody() {
            _selectedBodyIndex = (_selectedBodyIndex + 1) % _availableBodies.Count;
            UpdateBookmarks();
        }

        /// <summary>
        /// Update the available bodies
        /// </summary>
        private void UpdateAvailableBodies() {
            try {
                ModLogger.LogDebug($"Updating available bodies");
                string selectedBody = GetSelectedBody();
                this._availableBodies.Clear();
                foreach (Bookmark bookmark in BookmarkManager.Instance.Bookmarks) {
                    Vessel vessel = bookmark.GetVessel();
                    if( vessel == null ) {
                        continue;
                    }
                    string vesselBodyName = vessel.mainBody.bodyName;
                    if( !_availableBodies.Contains(vesselBodyName) ) {
                        _availableBodies.Add(vesselBodyName);
                    }
                }
                this._availableBodies.Sort();
                this._availableBodies.Insert(0, ModLocalization.GetString("labelAll"));
                this.SelectBody(selectedBody, false);
            } catch (Exception e) {
                ModLogger.LogError($"Error updating available bodies: {e.Message}");
            }
        }

        // ======================================================================

        /// <summary>
        /// Get the selected vessel type
        /// </summary>
        /// <returns>The selected vessel type</returns>
        public string GetSelectedVesselType() {
            if( _selectedVesselTypeIndex >= 0 && _selectedVesselTypeIndex < _availableVesselTypes.Count ) {
                return _availableVesselTypes[_selectedVesselTypeIndex];
            } else {
                return ModLocalization.GetString("labelAll");
            }
        }

        /// <summary>
        /// Select a vessel type
        /// </summary>
        /// <param name="vesselTypeName">The name of the vessel type to select</param>
        /// <param name="updateBookmarks">Whether to update the bookmarks list</param>
        public void SelectVesselType(string vesselTypeName, bool updateBookmarks = true) {
            if( this._availableVesselTypes.Contains(vesselTypeName) ) {
                this._selectedVesselTypeIndex = this._availableVesselTypes.IndexOf(vesselTypeName);
            } else {
                this._selectedVesselTypeIndex = 0;
            }
            if( updateBookmarks ) {
                UpdateBookmarks();
            }
        }

        /// <summary>
        /// Select the next vessel type
        /// </summary>
        public void SelectNextVesselType() {
            _selectedVesselTypeIndex = (_selectedVesselTypeIndex + 1) % _availableVesselTypes.Count;
            UpdateBookmarks();
        }

        private void UpdateAvailableVesselTypes() {
            try {
                ModLogger.LogDebug($"Updating available vessel types");
                string selectedVesselType = GetSelectedVesselType();
                this._availableVesselTypes.Clear();
                foreach (Bookmark bookmark in BookmarkManager.Instance.Bookmarks) {
                    VesselType vesselType = bookmark.GetBookmarkDisplayType();
                    string vesselTypeName = vesselType.ToString();
                    if( !_availableVesselTypes.Contains(vesselTypeName) ) {
                        _availableVesselTypes.Add(vesselTypeName);
                    }
                }
                this._availableVesselTypes.Sort();
                this._availableVesselTypes.Insert(0, ModLocalization.GetString("labelAll"));
                this.SelectVesselType(selectedVesselType, false);
            } catch (Exception e) {
                ModLogger.LogError($"Error updating available vessel types: {e.Message}");
            }
        }

        // ======================================================================

        /// <summary>
        /// Update the available bookmarks
        /// </summary>
        private void UpdateAvailableBookmarks() {
            try {
                ModLogger.LogDebug($"Updating available bookmarks");
                this._availableBookmarks.Clear();
                
                string selectedBody = GetSelectedBody();
                string selectedVesselType = GetSelectedVesselType();
                string all = ModLocalization.GetString("labelAll");
                
                foreach (Bookmark bookmark in BookmarkManager.Instance.Bookmarks) {
                    Vessel vessel = bookmark.GetVessel();
                    if( vessel == null ) {
                        continue;
                    }

                    bool addBookmark;
                    if( string.Equals(selectedBody, all) && string.Equals(selectedVesselType, all) ) {
                        addBookmark = true;
                    } else if( string.Equals(selectedBody, all) ) {
                        addBookmark = string.Equals(
                            bookmark.GetBookmarkDisplayType().ToString(), 
                            selectedVesselType
                        );
                    } else if( string.Equals(selectedVesselType, all) ) {
                        addBookmark = string.Equals(
                            vessel.mainBody.bodyName, 
                            selectedBody
                        );
                    } else {
                        addBookmark = string.Equals(
                                vessel.mainBody.bodyName, 
                                selectedBody
                            ) 
                            && 
                            string.Equals(
                                bookmark.GetBookmarkDisplayType().ToString(), 
                                selectedVesselType
                            );
                    }
                    if( addBookmark ) {
                        _availableBookmarks.Add(bookmark);
                    }
                }
                this._availableBookmarks.Sort((a, b) => a.Order.CompareTo(b.Order));
            } catch (Exception e) {
                ModLogger.LogError($"Error updating available bookmarks: {e.Message}");
            }
        }

        /// <summary>
        /// Update the bookmarks list
        /// </summary>
        public void UpdateBookmarks() {
            try {
                ModLogger.LogDebug($"Updating bookmarks");
                
                this.UpdateAvailableBodies();
                this.UpdateAvailableVesselTypes();
                this.UpdateAvailableBookmarks();
            } catch (Exception e) {
                ModLogger.LogError($"Error updating bookmarks: {e.Message}");
            }
        }

        /// <summary>
        /// Clear the filters
        /// </summary>
        public void ClearFilters() {
            _selectedBodyIndex = 0;
            _selectedVesselTypeIndex = 0;
            UpdateBookmarks();
        }
    }
}