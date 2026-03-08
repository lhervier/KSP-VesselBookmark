using KSP.Localization;
using KSP.UI.Screens;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using com.github.lhervier.ksp.bookmarksmod.bookmarks;
using com.github.lhervier.ksp.bookmarksmod.util;

namespace com.github.lhervier.ksp.bookmarksmod.ui {
    public class BookmarkUIController {
        private static readonly ModLogger LOGGER = new ModLogger("BookmarkUIController");
        private Bookmark _currentBookmark;
        private int _currentIndex;
        private BookmarksListUIController _bookmarksListUIController;
        private EditCommentUIController _editCommentUIController;

        public BookmarkUIController(
            Bookmark currentBookmark,
            int currentIndex,
            BookmarksListUIController bookmarksListUIController,
            EditCommentUIController editCommentUIController
        ) { 
            _bookmarksListUIController = bookmarksListUIController;
            _editCommentUIController = editCommentUIController;
            _currentBookmark = currentBookmark;
            _currentIndex = currentIndex;
        }

        public bool IsTarget() {
            if( FlightGlobals.ActiveVessel == null ) {
                return false;
            }
            ITargetable target = FlightGlobals.ActiveVessel.targetObject;
            if( target == null ) {
                return false;
            }
            Vessel targetVessel = target.GetVessel();
            if( targetVessel == null ) {
                return false;
            }
            return targetVessel.persistentId == _currentBookmark.VesselPersistentID;
        }

        public string GetBookmarkTitle() {
            if( string.IsNullOrEmpty(_currentBookmark.BookmarkTitle)) {
                return ModLocalization.GetString("labelModuleNotFound");
            }
            return _currentBookmark.BookmarkTitle;
        }

        public bool IsHovered() {
            return _bookmarksListUIController.IsHovered(_currentBookmark);
        }

        public bool IsSelected() {
            return _bookmarksListUIController.IsSelected(_currentBookmark);
        }

        public bool IsHoveredOrSelected() {
            return IsHovered() || IsSelected();
        }

        /// <summary>
        /// Ensures the current bookmark line is selected (e.g. when clicking a button on that line).
        /// </summary>
        private void SelectCurrentLine() {
            _bookmarksListUIController.SetSelected(_currentBookmark);
        }

        public bool IsActiveVessel() {
            bool isActiveVessel = false;
            if (FlightGlobals.ActiveVessel != null) {
                isActiveVessel = _currentBookmark.VesselPersistentID == FlightGlobals.ActiveVessel.persistentId;
            }
            return isActiveVessel;
        }

        public bool CanMoveUp() {
            return _currentIndex > 0;
        }

        public bool CanMoveDown() {
            return _currentIndex < _bookmarksListUIController.AvailableBookmarks[_currentBookmark.BookmarkType].Count - 1;
        }

        public void EditComment() {
            SelectCurrentLine();
            _editCommentUIController.EditComment(_currentBookmark);
        }

        public bool CanSetTargetAs() {
            if( FlightGlobals.ActiveVessel == null ) {
                return false;
            }
            if( _currentBookmark.Vessel == null ) {
                return false;
            }
            if( IsActiveVessel() ) {
                return false;
            }
            return true;
        }

        public void SetTargetAs() {
            SelectCurrentLine();
            if( !CanSetTargetAs() ) {
                LOGGER.LogWarning($"Bookmark {_currentBookmark}: Can't set target as. Current vessel or bookmark vessel not found.");
                return;
            }

            FlightGlobals flightGlobals = FlightGlobals.fetch;
            if( flightGlobals == null ) {
                LOGGER.LogWarning($"Bookmark {_currentBookmark}: FlightGlobals not found. Cannot set target as.");
                return;
            }
            flightGlobals.SetVesselTarget(_currentBookmark.Vessel);
        }

        public bool CanSwitchToVessel() {
            if( _currentBookmark.Vessel == null ) {
                return false;
            }
            if( IsActiveVessel() ) {
                return false;
            }
            return true;
        }

        public void SwitchToVessel() {
            SelectCurrentLine();
            Vessel vessel = _currentBookmark.Vessel;
            if( vessel == null ) {
                LOGGER.LogWarning($"Bookmark {_currentBookmark}: Vessel not found. Cannot switch to vessel.");
                return;
            }
            if (VesselNavigator.NavigateToVessel(vessel)) {
                _bookmarksListUIController.MainWindowsVisible = false;
                _editCommentUIController.CancelCommentEdition();
            }
        }

        public void MoveUp() {
            SelectCurrentLine();
            if( !CanMoveUp() ) {
                LOGGER.LogWarning($"Bookmark {_currentBookmark}: Can't move up. Current index {_currentIndex} is the first index for this bookmark type.");
                return;
            }
            List<Bookmark> bookmarks = _bookmarksListUIController.AvailableBookmarks[_currentBookmark.BookmarkType];
            Bookmark previousBookmark = bookmarks[_currentIndex - 1];

            while( _currentBookmark.Order > previousBookmark.Order ) {
                BookmarkManager.MoveBookmarkUp(_currentBookmark, false);
            }
            BookmarkManager.OnBookmarksUpdated.Fire();
        }

        public void MoveDown() {
            SelectCurrentLine();
            if( !CanMoveDown() ) {
                LOGGER.LogWarning($"Bookmark {_currentBookmark}: Can't move down. Current index {_currentIndex} is the last index for this bookmark type.");
                return;
            }
            List<Bookmark> bookmarks = _bookmarksListUIController.AvailableBookmarks[_currentBookmark.BookmarkType];
            Bookmark nextBookmark = bookmarks[_currentIndex + 1];

            while( _currentBookmark.Order < nextBookmark.Order ) {
                BookmarkManager.MoveBookmarkDown(_currentBookmark, false);
            }
            BookmarkManager.OnBookmarksUpdated.Fire();
        }

        public void Remove() {
            SelectCurrentLine();
            // Close main window temporarily to ensure confirmation dialog appears on top
            bool wasMainWindowVisible = _bookmarksListUIController.MainWindowsVisible;
            _bookmarksListUIController.MainWindowsVisible = false;
            
            string displayName = this.GetBookmarkTitle();
            VesselBookmarkUIDialog.ConfirmRemoval(
                () => {
                    BookmarkManager.RemoveBookmark(_currentBookmark);
                    _bookmarksListUIController.MainWindowsVisible = wasMainWindowVisible;
                },
                () => {
                    _bookmarksListUIController.MainWindowsVisible = wasMainWindowVisible;
                },
                bookmarkName: displayName
            );
        }
    }
}