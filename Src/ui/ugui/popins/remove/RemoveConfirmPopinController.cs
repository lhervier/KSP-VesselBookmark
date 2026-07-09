using UnityEngine;
using TMPro;
using com.github.lhervier.ksp.bookmarksmod.bookmarks;
using com.github.lhervier.ksp.shared;
using com.github.lhervier.ksp.shared.ugui.popin;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.popins.remove
{
    /// <summary>
    /// Orchestrates the remove-confirmation internal popup: shows/closes it on ViewModel.PendingRemoval and
    /// fills the message with the pending bookmark's name. The footer buttons are wired to the ViewModel by
    /// the shared button bar. Lives on the popup's always-active root so its lifecycle runs even while the
    /// popup is closed.
    /// </summary>
    public class RemoveConfirmPopinController : MonoBehaviour
    {
        private BookmarksViewModel _viewModel;
        public RemoveConfirmPopinController WithViewModel(BookmarksViewModel viewModel)
        {
            this._viewModel = viewModel;
            return this;
        }

        private PopinController _popup;
        public RemoveConfirmPopinController WithPopupController(PopinController popup)
        {
            this._popup = popup;
            return this;
        }

        private TextMeshProUGUI _message;
        public RemoveConfirmPopinController WithMessageComponent(TextMeshProUGUI message)
        {
            this._message = message;
            return this;
        }

        public void Start()
        {
            _viewModel.OnPendingRemovalChanged.Add(OnPendingRemovalChanged);
            OnPendingRemovalChanged();
        }

        public void OnDestroy()
        {
            _viewModel?.OnPendingRemovalChanged.Remove(OnPendingRemovalChanged);
        }

        private void OnPendingRemovalChanged()
        {
            Bookmark pending = _viewModel.PendingRemoval;
            if (pending != null)
            {
                if (_message != null)
                {
                    _message.text = ModLocalization.GetString("dialogRemoveMessageWithName", pending.BookmarkTitle);
                }
                _popup?.Show();
            }
            else
            {
                _popup?.Close();
            }
        }
    }
}
