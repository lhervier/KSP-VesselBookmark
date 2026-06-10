using UnityEngine;
using UnityEngine.UI;
using com.github.lhervier.ksp.bookmarksmod.bookmarks;
using com.github.lhervier.ksp.shared;
using com.github.lhervier.ksp.shared.ugui.button;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.overlays
{
    public class RemoveConfirmOverlayController : MonoBehaviour
    {
        private BookmarksViewModel _viewModel;
        public RemoveConfirmOverlayController ViewModel(BookmarksViewModel viewModel)
        {
            this._viewModel = viewModel;
            return this;
        }

        private GameObject _panel;
        public RemoveConfirmOverlayController Panel(GameObject panel)
        {
            this._panel = panel;
            return this;
        }
        
        private Text _message;
        public RemoveConfirmOverlayController Message(Text message)
        {
            this._message = message;
            return this;
        }

        private ButtonController _cancelButtonController;
        public RemoveConfirmOverlayController CancelButtonController(ButtonController cancelButtonController)
        {
            _cancelButtonController = cancelButtonController;
            return this;
        }

        private ButtonController _removeButtonController;
        public RemoveConfirmOverlayController RemoveButtonController(ButtonController removeButtonController)
        {
            _removeButtonController = removeButtonController;
            return this;
        }

        public void Start()
        {
            _viewModel.OnPendingRemovalChanged.Add(OnPendingRemovalChanged);
            OnPendingRemovalChanged();

            if( _cancelButtonController != null )
            {
                _cancelButtonController.OnClick.Add(_viewModel.CancelPendingRemoval);
            }
            if( _removeButtonController != null )
            {
                _removeButtonController.OnClick.Add(_viewModel.ConfirmPendingRemoval);
            }
        }

        public void OnDestroy()
        {
            if( _removeButtonController != null )
            {
                _removeButtonController.OnClick.Remove(_viewModel.ConfirmPendingRemoval);
            }
            if( _cancelButtonController != null )
            {
                _cancelButtonController.OnClick.Remove(_viewModel.CancelPendingRemoval);
            }

            _viewModel?.OnPendingRemovalChanged.Remove(OnPendingRemovalChanged);
        }

        private void OnPendingRemovalChanged()
        {
            Bookmark pending = _viewModel.PendingRemoval;
            if (pending != null && _message != null)
            {
                _message.text = ModLocalization.GetString("dialogRemoveMessageWithName", pending.BookmarkTitle);
            }
            if (_panel != null) _panel.SetActive(pending != null);
        }
    }
}
