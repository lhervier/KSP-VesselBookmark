using KSP.Localization;
using KSP.UI.Screens;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using com.github.lhervier.ksp.bookmarksmod.bookmarks;
using com.github.lhervier.ksp.bookmarksmod.util;

namespace com.github.lhervier.ksp.bookmarksmod.ui {
    public class BookmarkUIController {
        private Bookmark _currentBookmark;
        private int _currentIndex;
        private MainUIController _mainUIController;
        private EditCommentUIController _editCommentUIController;

        public BookmarkUIController(
            Bookmark currentBookmark,
            int currentIndex,
            MainUIController mainUIController,
            EditCommentUIController editCommentUIController
        ) { 
            _mainUIController = mainUIController;
            _editCommentUIController = editCommentUIController;
            _currentBookmark = currentBookmark;
            _currentIndex = currentIndex;
        }

        public string GetBookmarkTitle() {
            string bookmarkName;
            if( !string.IsNullOrEmpty(_currentBookmark.GetBookmarkDisplayName())) {
                bookmarkName = _currentBookmark.GetBookmarkDisplayName();
            } else {
                bookmarkName = ModLocalization.GetString("labelModuleNotFound");
            }
            return bookmarkName;
        }

        public string GetVesselSituation() {
            if (!string.IsNullOrEmpty(_currentBookmark.VesselSituation)) {
                return _currentBookmark.VesselSituation;
            } else {
                return ModLocalization.GetString("labelUnknownSituation");
            }
        }

        public bool IsHovered() {
            return (_mainUIController.HoveredBookmarkID == _currentBookmark.GetBookmarkID()) && (_mainUIController.HoveredBookmarkType == _currentBookmark.GetBookmarkType());
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
            return _currentIndex < _mainUIController.AvailableBookmarks.Count - 1;
        }

        public void EditComment() {
            _editCommentUIController.EditComment(_currentBookmark);
        }

        public void SwitchToVessel() {
            Vessel vessel = _currentBookmark.GetVessel();
            if( vessel == null ) {
                ModLogger.LogWarning($"Bookmark {_currentBookmark.GetBookmarkID()}: Vessel not found");
                return;
            }
            if (VesselNavigator.NavigateToVessel(vessel)) {
                _mainUIController.MainWindowsVisible = false;
                _editCommentUIController.CancelCommentEdition();
            }
        }

        public void MoveUp() {
            Bookmark previousBookmark = _mainUIController.AvailableBookmarks[_currentIndex - 1];
            BookmarkManager.Instance.SwapBookmarks(
                _currentBookmark, 
                previousBookmark
            );
        }

        public void MoveDown() {
            Bookmark nextBookmark = _mainUIController.AvailableBookmarks[_currentIndex + 1];
            BookmarkManager.Instance.SwapBookmarks(
                _currentBookmark, 
                nextBookmark
            );
        }

        public void Remove() {
            // Close main window temporarily to ensure confirmation dialog appears on top
            bool wasMainWindowVisible = _mainUIController.MainWindowsVisible;
            _mainUIController.MainWindowsVisible = false;
            
            VesselBookmarkUIDialog.ConfirmRemoval(
                () => {
                    BookmarkManager.Instance.RemoveBookmark(_currentBookmark);
                    _mainUIController.MainWindowsVisible = wasMainWindowVisible;
                },
                () => {
                    _mainUIController.MainWindowsVisible = wasMainWindowVisible;
                }
            );
        }
    }
}