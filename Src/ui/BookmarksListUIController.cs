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
        private BookmarksViewModel _viewModel;

        public EventVoid OnClosed = new EventVoid("BookmarksListUIController.OnClosed");

        /// <summary>
        /// Whether the main windows are visible
        /// </summary>
        public bool MainWindowsVisible { get; set; } = false;

        // Bookmarks list to display in the UI (cached for performance)
        // Only for read access
        public IReadOnlyDictionary<BookmarkType, List<Bookmark>> AvailableBookmarks => _viewModel.AvailableBookmarks;
        
        /// <summary>
        /// The list of available bodies
        /// </summary>
        public IReadOnlyList<string> AvailableBodies => _viewModel.AvailableBodies;
        
        /// <summary>
        /// The selected body
        /// </summary>
        public string SelectedBody
        {
            get => _viewModel.SelectedBody;
            set { _viewModel.SelectedBody = value; }
        }
        
        
        /// <summary>
        /// The list of available vessel types
        /// </summary>
        public IReadOnlyList<string> AvailableVesselTypes => _viewModel.AvailableVesselTypes;
        
        /// <summary>
        /// The selected vessel type
        /// </summary>
        public string SelectedVesselType
        {
            get => _viewModel.SelectedVesselType;
            set
            {
                _viewModel.SelectedVesselType = value;
            }
        }
        
        
        /// <summary>
        /// Filtre : n'afficher que les bookmarks qui ont un commentaire (pour usage futur).
        /// </summary>
        public bool FilterHasComment
        {
            get => _viewModel.FilterHasComment;
            set
            {
                _viewModel.FilterHasComment = value;
            }
        }

        /// <summary>
        /// Texte saisi dans la zone de recherche/filtre (pour usage futur).
        /// Au changement, déclenche un rafraîchissement des signets après 200 ms sans frappe.
        /// </summary>
        public string SearchText
        {
            get => _viewModel.SearchText;
            set
            {
                _viewModel.SearchText = value;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public BookmarksListUIController(BookmarksViewModel viewModel) {
            this._viewModel = viewModel;
        }

        // ======================================================================
        //  Main window actions
        // ======================================================================

        /// <summary>
        /// Whether to show the add vessel bookmark button
        /// </summary>
        /// <returns>Whether to show the add vessel bookmark button</returns>
        public bool CanAddVesselBookmark() {
            return _viewModel.CanAddVesselBookmark();
        }

        /// <summary>
        /// Add a vessel bookmark
        /// </summary>
        public void AddVesselBookmark() {
            this._viewModel.AddVesselBookmark();
        }

        public void RefreshBookmarks() {
            this._viewModel.RefreshBookmarks();
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
        /// Clear the filters
        /// </summary>
        public void ClearFilters() {
            this._viewModel.ClearFilters();
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
            return _viewModel.IsHovered(bookmark);
        }

        /// <summary>
        /// Set the hovered bookmark
        /// </summary>
        /// <param name="bookmark">The bookmark to set as hovered</param>
        public void SetHovered(Bookmark bookmark) {
            _viewModel.HoveredBookmark = bookmark;
        }

        /// <summary>
        /// Returns the currently hovered bookmark, or null if none.
        /// </summary>
        public Bookmark GetHoveredBookmark() {
            return _viewModel.HoveredBookmark;
        }

        /// <summary>
        /// Whether the given bookmark is selected
        /// </summary>
        /// <param name="bookmark">The bookmark to check</param>
        /// <returns>Whether the given bookmark is selected</returns>
        public bool IsSelected(Bookmark bookmark) {
            return _viewModel.IsSelected(bookmark);
        }

        /// <summary>
        /// Set the selected bookmark
        /// </summary>
        /// <param name="bookmark">The bookmark to set as selected</param>
        public void SetSelected(Bookmark bookmark) {
            _viewModel.SelectedBookmark = bookmark;
        }

        /// <summary>
        /// Returns the currently selected bookmark, or null if none.
        /// </summary>
        public Bookmark GetSelectedBookmark() {
            return _viewModel.SelectedBookmark;
        }

        /// <summary>
        /// Whether the selected bookmark can be edited
        /// </summary>
        /// <returns>Whether the selected bookmark can be edited</returns>
        public bool CanEditCurrentVesselComment() {
            return _viewModel.CanEditCurrentVesselComment();
        }

        /// <summary>
        /// Whether "Set target as" is available for the given bookmark (active vessel exists, bookmark has vessel, not active vessel).
        /// </summary>
        public bool CanSetCurrentVesselAsTarget() {
            return _viewModel.CanSetCurrentBookmarkVesselAsTarget();
        }

        /// <summary>
        /// Whether "Switch to vessel" is available for the given bookmark (vessel exists, not active vessel).
        /// </summary>
        public bool CanSwitchToCurrentVessel() {
            return _viewModel.CanSwitchToCurrentBookmarkVessel();
        }
        
        // ======================================================================
        //  Current selected bookmark actions
        // ======================================================================

        /// <summary>
        /// Set the target as the given bookmark
        /// </summary>
        public void SetCurrentVesselAsTarget() {
            this._viewModel.SetCurrentBookmarkVesselAsTarget();
        }

        /// <summary>
        /// Switch to the given bookmark
        /// </summary>
        public void SwitchToCurrentVessel() { 
            this._viewModel.SwitchToSelectedVessel();
        }

        // ======================================================================
        //  Bookmark actions
        // ======================================================================

        /// <summary>
        /// Move the given bookmark up
        /// </summary>
        /// <param name="bookmark">The bookmark to move up</param>
        public void MoveUp(Bookmark bookmark) {
            this._viewModel.MoveUp(bookmark);
        }

        /// <summary>
        /// Move the given bookmark down
        /// </summary>
        /// <param name="bookmark">The bookmark to move down</param>
        public void MoveDown(Bookmark bookmark) {
            this._viewModel.MoveDown(bookmark);
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
                    this._viewModel.Remove(bookmark);
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