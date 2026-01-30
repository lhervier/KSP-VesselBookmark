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

        public string GetBookmarkTitle() {
            string bookmarkName;
            if( !string.IsNullOrEmpty(_currentBookmark.GetBookmarkTitle())) {
                bookmarkName = _currentBookmark.GetBookmarkTitle();
            } else {
                bookmarkName = ModLocalization.GetString("labelModuleNotFound");
            }
            return bookmarkName;
        }

        public bool IsHovered() {
            return (_bookmarksListUIController.HoveredBookmarkID == _currentBookmark.GetBookmarkID()) && (_bookmarksListUIController.HoveredBookmarkType == _currentBookmark.BookmarkType);
        }

        public bool IsActiveVessel() {
            bool isActiveVessel = false;
            if (FlightGlobals.ActiveVessel != null) {
                isActiveVessel = _currentBookmark.VesselPersistentID == FlightGlobals.ActiveVessel.persistentId;
            }
            return isActiveVessel;
        }

        public bool CanMoveUp() {
            if( !_bookmarksListUIController.AvailableBookmarks.ContainsKey(_currentBookmark.BookmarkType) ) {
                ModLogger.LogWarning($"Bookmark {_currentBookmark.GetBookmarkID()}: Bookmark type {_currentBookmark.BookmarkType} not found");
                return false;
            }
            return _currentIndex > 0;
        }

        public bool CanMoveDown() {
            if( !_bookmarksListUIController.AvailableBookmarks.ContainsKey(_currentBookmark.BookmarkType) ) {
                ModLogger.LogWarning($"Bookmark {_currentBookmark.GetBookmarkID()}: Bookmark type {_currentBookmark.BookmarkType} not found");
                return false;
            }
            return _currentIndex < _bookmarksListUIController.AvailableBookmarks[_currentBookmark.BookmarkType].Count - 1;
        }

        public void EditComment() {
            _editCommentUIController.EditComment(_currentBookmark);
        }

        public void SwitchToVessel() {
            Vessel vessel = _currentBookmark.Vessel;
            if( vessel == null ) {
                ModLogger.LogWarning($"Bookmark {_currentBookmark.GetBookmarkID()}: Vessel not found");
                return;
            }
            if (VesselNavigator.NavigateToVessel(vessel)) {
                _bookmarksListUIController.MainWindowsVisible = false;
                _editCommentUIController.CancelCommentEdition();
            }
        }

        public void MoveUp() {
            if( !CanMoveUp() ) {
                ModLogger.LogWarning($"Bookmark {_currentBookmark.GetBookmarkID()}: Cannot move up");
                return;
            }
            List<Bookmark> bookmarks = _bookmarksListUIController.AvailableBookmarks[_currentBookmark.BookmarkType];
            Bookmark previousBookmark = bookmarks[_currentIndex - 1];
            BookmarkManager.Instance.SwapBookmarks(
                _currentBookmark, 
                previousBookmark
            );
        }

        public void MoveDown() {
            if( !CanMoveDown() ) {
                ModLogger.LogWarning($"Bookmark {_currentBookmark.GetBookmarkID()}: Bookmark type {_currentBookmark.BookmarkType} not found");
                return;
            }
            List<Bookmark> bookmarks = _bookmarksListUIController.AvailableBookmarks[_currentBookmark.BookmarkType];
            Bookmark nextBookmark = bookmarks[_currentIndex + 1];
            BookmarkManager.Instance.SwapBookmarks(
                _currentBookmark, 
                nextBookmark
            );
        }

        public void Remove() {
            // Close main window temporarily to ensure confirmation dialog appears on top
            bool wasMainWindowVisible = _bookmarksListUIController.MainWindowsVisible;
            _bookmarksListUIController.MainWindowsVisible = false;
            
            string displayName = this.GetBookmarkTitle();
            VesselBookmarkUIDialog.ConfirmRemoval(
                () => {
                    BookmarkManager.Instance.RemoveBookmark(_currentBookmark);
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