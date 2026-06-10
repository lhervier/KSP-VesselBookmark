using System;
using UnityEngine;
using com.github.lhervier.ksp.shared.ugui.popup;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui
{
    /// <summary>
    /// Gère le cycle de vie de la fenêtre uGUI : spawn paresseux via ModPopupBuilder, show/hide,
    /// mémorisation de la position, et notification OnClosed. La mécanique bas niveau (PopupDialog,
    /// position, fermeture par Échap, changement de scène) est déléguée au PopupController partagé.
    /// </summary>
    public sealed class BookmarksWindow
    {
        private PopupController _popup = null;
        private BookmarksViewModel _viewModel;
        private Vector2? _savedPosition;

        public EventVoid OnClosed = new EventVoid("Bookmarks.Window.OnClosed");

        /// <summary>Émis avec la position (localPosition) de la fenêtre quand elle est capturée.</summary>
        public Action<Vector2> OnPositionCaptured;

        public void Initialize(BookmarksViewModel viewModel)
        {
            this._viewModel = viewModel;
        }

        /// <summary>Définit la position à restaurer (mémorisée entre sessions). Appliquée au prochain spawn.</summary>
        public void SetSavedPosition(Vector2 position)
        {
            _savedPosition = position;
        }

        public void Show()
        {
            // == null est sensible à la destruction Unity : après une fermeture par KSP (Échap), le
            // controller détruit vaut null ici, ce qui déclenche un nouveau spawn.
            if (_popup == null)
            {
                var builder = new ModPopupBuilder().ViewModel(_viewModel);
                if (_savedPosition.HasValue)
                {
                    builder = builder.Position(_savedPosition.Value);
                }
                _popup = builder.Build();
                if (_popup == null) return;   // Spawn KSP échoué
                _popup.OnClosed.Add(OnPopupClosed);
                _popup.OnPositionCaptured.Add(OnPopupPositionCaptured);
            }
            _popup.Show();
        }

        public void Hide()
        {
            if (_popup != null)
            {
                _popup.Hide();
            }
        }

        public void Destroy()
        {
            if (_popup != null)
            {
                _popup.Dismiss();
                _popup = null;
            }
        }

        private void OnPopupClosed()
        {
            OnClosed.Fire();
        }

        private void OnPopupPositionCaptured(Vector2 position)
        {
            _savedPosition = position;
            OnPositionCaptured?.Invoke(position);
        }
    }
}
