using System;
using UnityEngine;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui
{
    /// <summary>
    /// Gère le cycle de vie de la fenêtre uGUI (PopupDialog) : spawn paresseux, show/hide, mémorisation
    /// de la position, et notification OnClosed quand KSP la ferme de lui-même (ex. Échap).
    /// </summary>
    public sealed class BookmarksWindow
    {
        private PopupDialogBuilder _popupDialogBuilder;
        private PopupDialog _popupDialog = null;
        private BookmarksViewModel _viewModel;
        private Vector2? _savedPosition;

        public EventVoid OnClosed = new EventVoid("Bookmarks.Window.OnClosed");

        /// <summary>Émis avec la position (localPosition) de la fenêtre quand elle est capturée.</summary>
        public Action<Vector2> OnPositionCaptured;

        public void Initialize(BookmarksViewModel viewModel)
        {
            this._viewModel = viewModel;
            this._popupDialogBuilder = new PopupDialogBuilder(viewModel);
        }

        /// <summary>Définit la position à restaurer (mémorisée entre sessions). Appliquée si la fenêtre existe.</summary>
        public void SetSavedPosition(Vector2 position)
        {
            _savedPosition = position;
            ApplyPosition();
        }

        public void Show()
        {
            if (_popupDialog == null)
            {
                _popupDialog = this._popupDialogBuilder.CreatePopupDialog();
                _popupDialog?.onDestroy.AddListener(OnPopupDestroyed);
            }
            _popupDialog?.gameObject.SetActive(true);
            ApplyPosition();
        }

        public void Hide()
        {
            CapturePosition();
            _popupDialog?.gameObject.SetActive(false);
        }

        public void Destroy()
        {
            CapturePosition();
            _popupDialog?.onDestroy.RemoveListener(OnPopupDestroyed);
            _popupDialog?.Dismiss();
            _popupDialog = null;
        }

        private void OnPopupDestroyed()
        {
            // Fermée par KSP (Échap…) sans passer par Hide() : on capture la position, on oublie la
            // référence et on prévient le propriétaire pour qu'il resynchronise (toolbar, etc.).
            CapturePosition();
            _popupDialog = null;
            OnClosed.Fire();
        }

        private void ApplyPosition()
        {
            if (!_savedPosition.HasValue) return;
            if (_popupDialog == null || _popupDialog.RTrf == null) return;
            Vector3 lp = _popupDialog.RTrf.localPosition;
            _popupDialog.RTrf.localPosition = new Vector3(_savedPosition.Value.x, _savedPosition.Value.y, lp.z);
        }

        private void CapturePosition()
        {
            if (_popupDialog == null || _popupDialog.RTrf == null) return;
            Vector3 lp = _popupDialog.RTrf.localPosition;
            _savedPosition = new Vector2(lp.x, lp.y);
            OnPositionCaptured?.Invoke(_savedPosition.Value);
        }
    }
}
