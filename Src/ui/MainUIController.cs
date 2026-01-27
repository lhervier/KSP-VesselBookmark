using KSP.Localization;
using KSP.UI.Screens;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using com.github.lhervier.ksp.bookmarksmod.bookmarks;
using com.github.lhervier.ksp.bookmarksmod.util;
using System;

namespace com.github.lhervier.ksp.bookmarksmod.ui {

    public class MainUIController {

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
        public MainUIController() {
            UpdateBookmarks();
        }

        /// <summary>
        /// Update the bookmarks list
        /// </summary>
        public void UpdateBookmarks() {
            try {
                ModLogger.LogDebug($"Updating bookmarks");
                _availableBookmarks.Clear();
                
                _availableBodies.Clear();
                _availableBodies.Add(ModLocalization.GetString("labelAll"));

                _availableVesselTypes.Clear();
                _availableVesselTypes.Add(ModLocalization.GetString("labelAll"));

                foreach (Bookmark bookmark in BookmarkManager.Instance.Bookmarks) {
                    // Update the list of available bodies
                    Vessel vessel = bookmark.GetVessel();
                    if( vessel != null && !_availableBodies.Contains(vessel.mainBody.bodyName) ) {
                        _availableBodies.Add(vessel.mainBody.bodyName);
                    }

                    // Update the list of available vessel types
                    if( !_availableVesselTypes.Contains(bookmark.GetBookmarkDisplayType().ToString()) ) {
                        _availableVesselTypes.Add(bookmark.GetBookmarkDisplayType().ToString());
                    }

                    // Add the bookmark to the list if it matches the selected body and vessel type
                    if( _availableBookmarks.Contains(bookmark) ) {
                        return;
                    }
                    bool addBookmark;
                    if( _selectedBodyIndex == 0 && _selectedVesselTypeIndex == 0 ) {
                        addBookmark = true;
                    } else if( _selectedBodyIndex == 0 ) {
                        addBookmark = string.Equals(
                            bookmark.VesselType.ToString(), 
                            _availableVesselTypes[_selectedVesselTypeIndex]
                        );
                    } else if( _selectedVesselTypeIndex == 0 ) {
                        addBookmark = string.Equals(
                            vessel.mainBody.bodyName, 
                            _availableBodies[_selectedBodyIndex]
                        );
                    } else {
                        addBookmark = string.Equals(
                                vessel.mainBody.bodyName, 
                                _availableBodies[_selectedBodyIndex]
                            ) 
                            && 
                            string.Equals(
                                bookmark.VesselType.ToString(), 
                                _availableVesselTypes[_selectedVesselTypeIndex]
                            );
                    }
                    if( addBookmark ) {
                        _availableBookmarks.Add(bookmark);
                    }
                }
            } catch (Exception e) {
                ModLogger.LogError($"Error updating bookmarks: {e.Message}");
            }
        }

        public string GetSelectedBody() {
            return _availableBodies[_selectedBodyIndex];
        }

        public void SetSelectedBodyIndex(int index) {
            _selectedBodyIndex = index;
            UpdateBookmarks();
        }

        public void SelectNextBody() {
            _selectedBodyIndex = (_selectedBodyIndex + 1) % _availableBodies.Count;
            UpdateBookmarks();
        }

        public string GetSelectedVesselType() {
            return _availableVesselTypes[_selectedVesselTypeIndex];
        }

        public void SetSelectedVesselTypeIndex(int index) {
            _selectedVesselTypeIndex = index;
            UpdateBookmarks();
        }

        public void SelectNextVesselType() {
            _selectedVesselTypeIndex = (_selectedVesselTypeIndex + 1) % _availableVesselTypes.Count;
            UpdateBookmarks();
        }

        public void ClearFilters() {
            _selectedBodyIndex = 0;
            _selectedVesselTypeIndex = 0;
            UpdateBookmarks();
        }
    }
}