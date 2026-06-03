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
        
        private static readonly ModLogger LOGGER = new ModLogger("EditCommentUIController");

        private readonly BookmarksViewModel _viewModel;

        /// <summary>
        /// Whether the edit window is visible
        /// </summary>
        private bool _editingComment = false;

        /// <summary>
        /// The edited comment
        /// </summary>
        public string EditedComment {
            get {
                if( _viewModel == null ) return string.Empty;
                return _viewModel.Comment;
            }
            set {
                if( _viewModel == null ) return;
                _viewModel.Comment = value;
            }
        }
        
        public EditCommentUIController(BookmarksViewModel viewModel) {
            this._viewModel = viewModel;
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
            this._viewModel.SaveBookmarkComment();
            _editingComment = false;
        }

        /// <summary>
        /// Cancel the editition od the comment, closing the window.
        /// </summary>
        public void CancelCommentEdition() {
            this._viewModel.CancelBookmarkCommentEdition();
            _editingComment = false;
        }

        /// <summary>
        /// Start editing the comment of the given bookmark.
        /// </summary>
        /// <param name="bookmark">The bookmark to edit the comment of</param>
        public void EditComment(Bookmark bookmark) {
            _editingComment = true;
        }
    }
}