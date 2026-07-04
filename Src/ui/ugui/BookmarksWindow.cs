using System;
using UnityEngine;
using com.github.lhervier.ksp.shared.ugui.popup;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.titleBar;
using com.github.lhervier.ksp.shared;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.overlays;

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
        
        private PopupController _popupController = null;
        
        public EventVoid OnClosed = new EventVoid("Bookmarks.Window.OnClosed");

        private BookmarksViewModel _viewModel;
        public BookmarksWindow WithViewModel(BookmarksViewModel viewModel)
        {
            this._viewModel = viewModel;
            return this;
        }

        public void Show()
        {
            // == null est sensible à la destruction Unity : après une fermeture par KSP (Échap), le
            // controller détruit vaut null ici, ce qui déclenche un nouveau spawn.
            if (_popupController == null)
            {
                var popupBuilder = new PopupBuilder<TitleBarController, ContentController, BookmarksOverlaysController>()
                .WithPopupID(DIALOG_ID)
                .WithTitle(ModLocalization.GetString("windowTitle"))
                .WithTitleBarBuilder(
                    new TitleBarBuilder().WithViewModel(_viewModel)
                )
                .WithContentBuilder(
                    new ContentBuilder().WithViewModel(_viewModel)
                )
                .WithOverlayBuilder(
                    new BookmarksOverlaysBuilder().WithViewModel(_viewModel)
                )
                .WithSize(new Vector2(VesselBookmarkPalette.WindowWidth, VesselBookmarkPalette.WindowHeight));
                _popupController = popupBuilder.Build();
                if (_popupController == null)  {
                    LOGGER.LogError("Unable to create the main popup window");
                    return;
                }

                _popupController.OnClosed.Add(OnPopupClosed);
            }
            _popupController.Show();
        }

        public void Hide()
        {
            if (_popupController != null)
            {
                _popupController.Hide();
            }
        }

        public void Destroy()
        {
            if (_popupController != null)
            {
                _popupController.Dismiss();
                _popupController = null;
            }
        }

        private void OnPopupClosed()
        {
            OnClosed.Fire();
        }
    }
}
