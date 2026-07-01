using UnityEngine;
using TMPro;
using com.github.lhervier.ksp.bookmarksmod.bookmarks;
using com.github.lhervier.ksp.shared.ugui.button;
using com.github.lhervier.ksp.shared.ugui.internalpopup;
using com.github.lhervier.ksp.shared.ugui.textfield;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.overlays.editcomment
{
    /// <summary>
    /// Orchestrates the edit-comment internal popup: shows/closes it on ViewModel.EditingComment, binds
    /// the text area to ViewModel.Comment, and wires the footer buttons (Save / Cancel). Lives on the
    /// popup's always-active root so its lifecycle runs even while the popup is closed.
    /// </summary>
    public class EditCommentOverlayController : MonoBehaviour
    {
        private BookmarksViewModel _viewModel;
        public EditCommentOverlayController WithViewModel(BookmarksViewModel viewModel)
        {
            this._viewModel = viewModel;
            return this;
        }

        private InternalPopupController _popup;
        public EditCommentOverlayController WithPopupController(InternalPopupController popup)
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

        private ButtonController _cancelButtonController;
        public EditCommentOverlayController WithCancelButtonController(ButtonController cancelButtonController)
        {
            this._cancelButtonController = cancelButtonController;
            return this;
        }

        private ButtonController _okButtonController;
        public EditCommentOverlayController WithOkButtonController(ButtonController okButtonController)
        {
            this._okButtonController = okButtonController;
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
            if( _cancelButtonController != null )
            {
                _cancelButtonController.OnClick.Add(_viewModel.CancelBookmarkCommentEdition);
            }
            if( _okButtonController != null )
            {
                _okButtonController.OnClick.Add(_viewModel.SaveBookmarkComment);
            }
        }

        public void OnDestroy()
        {
            if( _okButtonController != null )
            {
                _okButtonController.OnClick.Remove(_viewModel.SaveBookmarkComment);
            }
            if( _cancelButtonController != null )
            {
                _cancelButtonController.OnClick.Remove(_viewModel.CancelBookmarkCommentEdition);
            }
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
