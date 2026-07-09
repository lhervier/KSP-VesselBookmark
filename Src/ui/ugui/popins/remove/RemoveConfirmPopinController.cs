using UnityEngine;
using com.github.lhervier.ksp.bookmarksmod.bookmarks;
using com.github.lhervier.ksp.shared;
using com.github.lhervier.ksp.shared.ugui.popin;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.popins.remove
{
    /// <summary>
    /// Orchestrates the remove-confirmation internal popup: on ViewModel.OnRemovalRequested, fills the
    /// shared confirm popin with the bookmark's name, wires its OK action to remove that bookmark, and shows
    /// it. Lives on the popup's always-active root so its lifecycle runs even while the popup is closed.
    /// </summary>
    public class RemoveConfirmPopinController : MonoBehaviour
    {
        private BookmarksViewModel _viewModel;
        public RemoveConfirmPopinController WithViewModel(BookmarksViewModel viewModel)
        {
            this._viewModel = viewModel;
            return this;
        }

        private ConfirmPopinController _confirm;
        public RemoveConfirmPopinController WithConfirmPopin(ConfirmPopinController confirm)
        {
            this._confirm = confirm;
            return this;
        }

        public void Start()
        {
            _viewModel.OnRemovalRequested.Add(OnRemovalRequested);
        }

        public void OnDestroy()
        {
            _viewModel?.OnRemovalRequested.Remove(OnRemovalRequested);
        }

        private void OnRemovalRequested(Bookmark bookmark)
        {
            _confirm.SetMessage(ModLocalization.GetString("dialogRemoveMessageWithName", bookmark.BookmarkTitle));
            _confirm.SetOkAction(() => _viewModel.Remove(bookmark));
            _confirm.Show();
        }
    }
}
