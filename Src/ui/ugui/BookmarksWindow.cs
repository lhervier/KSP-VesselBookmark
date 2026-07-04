using System;
using UnityEngine;
using com.github.lhervier.ksp.shared.ugui.popup;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.titleBar;
using com.github.lhervier.ksp.shared;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.menu;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.overlays.editcomment;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.overlays.remove;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui
{
    /// <summary>
    /// Gère le cycle de vie de la fenêtre uGUI : spawn paresseux, show/hide,
    /// mémorisation de la position, et notification OnClosed. La mécanique bas niveau (PopupDialog,
    /// position, fermeture par Échap, changement de scène) est déléguée au PopupController partagé.
    /// </summary>
    public sealed class BookmarksWindow
    {
        private ModLogger LOGGER = new ModLogger("BookmarksWindow");
        private const string DIALOG_ID = "VesselBookmarksUGUI";
        
        private PopupController _popup = null;
        private BookmarksViewModel _viewModel;
        private bool _hasPosition = false;
        private Vector2 _savedPosition;
        
        public EventVoid OnClosed = new EventVoid("Bookmarks.Window.OnClosed");

        /// <summary>Émis avec la position (localPosition) de la fenêtre quand elle est capturée.</summary>
        public EventData<Vector2> OnPositionCaptured = new EventData<Vector2>("Bookmarks.Window.OnPositionCaptured");

        public void Initialize(BookmarksViewModel viewModel)
        {
            this._viewModel = viewModel;
        }

        /// <summary>Définit la position à restaurer (mémorisée entre sessions). Appliquée au prochain spawn.</summary>
        public void SetSavedPosition(Vector2 position)
        {
            _savedPosition = position;
            _hasPosition = true;
        }

        public void Show()
        {
            // == null est sensible à la destruction Unity : après une fermeture par KSP (Échap), le
            // controller détruit vaut null ici, ce qui déclenche un nouveau spawn.
            if (_popup == null)
            {
                var popupBuilder = new PopupBuilder<TitleBarController, ContentController>()
                .WithPopupID(DIALOG_ID)
                .WithTitle(ModLocalization.GetString("windowTitle"))
                .WithTitleBarBuilder(
                    new TitleBarBuilder().WithViewModel(_viewModel)
                )
                .WithContentBuilder(
                    new ContentBuilder().WithViewModel(_viewModel)
                )
                .WithSize(new Vector2(VesselBookmarkPalette.WindowWidth, VesselBookmarkPalette.WindowHeight));
                if (this._hasPosition)
                {
                    popupBuilder = popupBuilder.WithPosition(this._savedPosition);
                }
                _popup = popupBuilder.Build();
                if (_popup == null)  {
                    LOGGER.LogError("Unable to create the main popup window");
                    return;
                }

                // Filter menu + internal overlays, grafted on the window above the content and title bar. They
                // self-manage their visibility through the ViewModel (FilterMenuOpen / EditingComment /
                // PendingRemoval), so nothing else needs wiring here.
                Transform windowTransform = _popup.GetGameObject().transform;

                new FilterMenuBuilder()
                    .WithViewModel(_viewModel)
                    .WithParent(windowTransform)
                    .Build();

                new EditCommentOverlayBuilder()
                    .WithViewModel(_viewModel)
                    .WithParent(windowTransform)
                    .Build();

                new RemoveConfirmOverlayBuilder()
                    .WithViewModel(_viewModel)
                    .WithParent(windowTransform)
                    .Build();
                    
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
            _hasPosition = true;
            OnPositionCaptured.Fire(position);
        }
    }
}
