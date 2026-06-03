using UnityEngine;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui
{
    /// <summary>
    /// Gère le cycle de vie de la fenêtre uGUI (PopupDialog) : spawn paresseux, show/hide, et
    /// notification OnClosed quand KSP la ferme de lui-même (ex. Échap), pour resynchroniser
    /// l'état (toolbar, fenêtre IMGUI) via WindowVisible.
    /// </summary>
    public sealed class BookmarksWindow
    {
        private PopupDialogBuilder _popupDialogBuilder;
        private PopupDialog _popupDialog = null;
        private BookmarksViewModel _viewModel;

        public EventVoid OnClosed = new EventVoid("Bookmarks.Window.OnClosed");

        public void Initialize(BookmarksViewModel viewModel)
        {
            this._viewModel = viewModel;
            this._popupDialogBuilder = new PopupDialogBuilder(viewModel);
        }

        public void Show()
        {
            if (_popupDialog == null)
            {
                _popupDialog = this._popupDialogBuilder.CreatePopupDialog();
                _popupDialog?.onDestroy.AddListener(OnPopupDestroyed);
            }
            _popupDialog?.gameObject.SetActive(true);
        }

        public void Hide()
        {
            _popupDialog?.gameObject.SetActive(false);
        }

        public void Destroy()
        {
            _popupDialog?.onDestroy.RemoveListener(OnPopupDestroyed);
            _popupDialog?.Dismiss();
            _popupDialog = null;
        }

        private void OnPopupDestroyed()
        {
            // Fermée par KSP (Échap…) sans passer par Hide() : on oublie la référence et on prévient
            // le propriétaire pour qu'il resynchronise (toolbar, fenêtre IMGUI).
            _popupDialog = null;
            OnClosed.Fire();
        }
    }
}
