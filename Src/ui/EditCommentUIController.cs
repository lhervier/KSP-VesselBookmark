using KSP.Localization;
using KSP.UI.Screens;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using com.github.lhervier.ksp.bookmarksmod.bookmarks;
using com.github.lhervier.ksp.bookmarksmod.util;
using System;

namespace com.github.lhervier.ksp.bookmarksmod.ui {

    public class EditCommentUIController {

        /// <summary>
        /// Whether the edit window is visible
        /// </summary>
        private bool _editingComment = false;
        
        // Edit window
        private Bookmark _editedBookmark = null;
        public string EditedComment { get; set; } = "";

        public EditCommentUIController() {
            _editedBookmark = null;
            EditedComment = "";
        }

        /// <summary>
        /// Whether the comment is being edited
        /// </summary>
        public bool IsEditingComment() {
            return _editingComment;
        }

        /// <summary>
        /// Save the edited comment, closing the window.
        /// This will also trigger the bookmarks updated event.
        /// </summary>
        public void SaveComment() {
            try {
                if (_editedBookmark != null) {
                    _editedBookmark.Comment = EditedComment;
                    BookmarkManager.OnBookmarksUpdated.Fire();
                }
            } catch (Exception e) {
                ModLogger.LogError($"Error saving edit window: {e.Message}");
            }
            _editingComment = false;
        }

        /// <summary>
        /// Cancel the editition od the comment, closing the window.
        /// </summary>
        public void CancelCommentEdition() {
            _editedBookmark = null;
            EditedComment = "";
            _editingComment = false;
        }

        /// <summary>
        /// Start editing the comment of the given bookmark.
        /// </summary>
        /// <param name="bookmark">The bookmark to edit the comment of</param>
        public void EditComment(Bookmark bookmark) {
            _editedBookmark = bookmark;
            EditedComment = bookmark.Comment;
            _editingComment = true;
        }
    }
}