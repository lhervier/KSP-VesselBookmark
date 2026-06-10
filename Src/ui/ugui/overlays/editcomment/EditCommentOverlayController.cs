using UnityEngine;
using UnityEngine.UI;
using com.github.lhervier.ksp.bookmarksmod.bookmarks;
using com.github.lhervier.ksp.shared;
using com.github.lhervier.ksp.shared.ugui.button;
using com.github.lhervier.ksp.shared.ugui.internalpopup;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.overlays.editcomment
{
    /// <summary>
    /// Orchestrates the edit-comment internal popup: shows/closes it on ViewModel.EditingComment, binds
    /// the text area to ViewModel.Comment, and wires the footer buttons (Save / Cancel). Lives on the
    /// popup's always-active root so its lifecycle runs even while the popup is closed.
    /// </summary>
    public class EditCommentOverlayController : MonoBehaviour
    {
        // Verrou des commandes de jeu pendant que le champ a le focus : sinon KSP lit le clavier en
        // parallèle (« c » bascule la caméra, etc.). Le champ Unity reçoit les frappes quoi qu'il arrive.
        public const string LOCK_ID = "VesselBookmarkMod_EditComment";

        private bool _loading;

        private BookmarksViewModel _viewModel;
        public EditCommentOverlayController ViewModel(BookmarksViewModel viewModel)
        {
            this._viewModel = viewModel;
            return this;
        }

        private InternalPopupController _popup;
        public EditCommentOverlayController Popup(InternalPopupController popup)
        {
            this._popup = popup;
            return this;
        }

        private Text _sub;
        public EditCommentOverlayController Sub(Text sub)
        {
            this._sub = sub;
            return this;
        }

        private InputField _input;
        public EditCommentOverlayController Input(InputField input)
        {
            this._input = input;
            return this;
        }

        private ButtonController _cancelButtonController;
        public EditCommentOverlayController CancelButtonController(ButtonController cancelButtonController)
        {
            this._cancelButtonController = cancelButtonController;
            return this;
        }

        private ButtonController _okButtonController;
        public EditCommentOverlayController OkButtonController(ButtonController okButtonController)
        {
            this._okButtonController = okButtonController;
            return this;
        }

        public void Start()
        {
            _viewModel.OnEditingCommentChanged.Add(OnEditingCommentChanged);
            OnEditingCommentChanged();

            if( _input != null )
            {
                _input.onValueChanged.AddListener(OnInputChanged);
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
            if( _input != null )
            {
                _input.onValueChanged.RemoveListener(OnInputChanged);
            }
            _viewModel?.OnEditingCommentChanged.Remove(OnEditingCommentChanged);
            InputLockManager.RemoveControlLock(LOCK_ID);   // sécurité si détruit en cours d'édition
        }

        public void OnInputChanged(string value)
        {
            if (_loading) return;
            _viewModel.Comment = value;
        }

        private void OnEditingCommentChanged()
        {
            bool editing = _viewModel.EditingComment;

            if (!editing) {
                _popup?.Close();
                // Sécurité : si on ferme alors que le champ avait encore le focus (le Deselect
                // peut ne pas partir quand on désactive le panneau).
                InputLockManager.RemoveControlLock(LOCK_ID);
                return;
            }

            _popup?.Show();

            Bookmark sel = _viewModel.SelectedBookmark;
            if (_sub != null) _sub.text = sel != null ? sel.BookmarkTitle : string.Empty;

            _loading = true;
            if (_input != null) _input.text = _viewModel.Comment ?? string.Empty;
            _loading = false;

            if (_input != null) _input.ActivateInputField();
        }
    }
}
