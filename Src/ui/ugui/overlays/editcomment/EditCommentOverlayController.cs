using UnityEngine;
using TMPro;
using com.github.lhervier.ksp.bookmarksmod.bookmarks;
using com.github.lhervier.ksp.shared.ugui.popin;
using com.github.lhervier.ksp.shared.ugui.textfield;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.overlays.editcomment
{
    /// <summary>
    /// Orchestrates the edit-comment internal popup: shows/closes it on ViewModel.EditingComment and binds
    /// the text area to ViewModel.Comment. The footer buttons (Save / Cancel) are wired to the ViewModel by
    /// the shared button bar. Lives on the popup's always-active root so its lifecycle runs even while the
    /// popup is closed.
    /// </summary>
    public class EditCommentOverlayController : MonoBehaviour
    {
        private BookmarksViewModel _viewModel;
        public EditCommentOverlayController WithViewModel(BookmarksViewModel viewModel)
        {
            this._viewModel = viewModel;
            return this;
        }

        private PopinController _popup;
        public EditCommentOverlayController WithPopupController(PopinController popup)
        {
            this._popup = popup;
            return this;
        }

        private TextMeshProUGUI _sub;
        public EditCommentOverlayController WithSubComponent(TextMeshProUGUI sub)
        {
            this._sub = sub;
            return this;
        }

        private TextFieldController _textFieldController;
        public EditCommentOverlayController WithTextFieldController(TextFieldController textFieldController)
        {
            this._textFieldController = textFieldController;
            return this;
        }

        public void Start()
        {
            _viewModel.OnEditingCommentChanged.Add(OnEditingCommentChanged);
            OnEditingCommentChanged();

            if( _textFieldController != null )
            {
                _textFieldController.OnValueChanged.Add(OnInputChanged);
            }
        }

        public void OnDestroy()
        {
            if( _textFieldController != null )
            {
                _textFieldController.OnValueChanged.Remove(OnInputChanged);
            }
            _viewModel?.OnEditingCommentChanged.Remove(OnEditingCommentChanged);
        }

        public void OnInputChanged(string value)
        {
            _viewModel.Comment = value;
        }

        private void OnEditingCommentChanged()
        {
            bool editing = _viewModel.EditingComment;

            if (!editing) {
                // Closing the popup deactivates the field; TextFieldController.OnDisable releases the
                // keyboard lock even if it still held the focus (onDeselect may not fire on deactivation).
                _popup?.Close();
                return;
            }

            _popup?.Show();

            Bookmark sel = _viewModel.SelectedBookmark;
            if (_sub != null) _sub.text = sel != null ? sel.BookmarkTitle : string.Empty;

            // SetText syncs the view without firing OnValueChanged, so no re-entrancy guard is needed.
            if (_textFieldController != null) _textFieldController.SetText(_viewModel.Comment ?? string.Empty);

            if (_textFieldController != null) _textFieldController.Activate();
        }
    }
}
