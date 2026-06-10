using UnityEngine;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.titleBar;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.menu;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.overlays.editcomment;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.overlays.remove;
using com.github.lhervier.ksp.shared;
using com.github.lhervier.ksp.shared.ugui;
using com.github.lhervier.ksp.shared.ugui.popup;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui
{
    /// <summary>
    /// Spawns the bookmarks window on top of the shared PopupBuilder: it supplies the title (left), the
    /// title bar's right column and the content (scrollable body + footer), then grafts the filter menu and
    /// the internal overlays on the window. Returns the shared PopupController the caller drives, or null if
    /// KSP failed to spawn the popup.
    /// </summary>
    public class ModPopupBuilder : IUGUIBuilder<PopupController>
    {
        private const string DIALOG_ID = "VesselBookmarksUGUI";

        // =============================================
        // Build parameters
        // =============================================

        private BookmarksViewModel _viewModel;
        public ModPopupBuilder ViewModel(BookmarksViewModel viewModel)
        {
            this._viewModel = viewModel;
            return this;
        }

        private Vector2 _position;
        private bool _hasPosition;
        public ModPopupBuilder Position(Vector2 position)
        {
            this._position = position;
            this._hasPosition = true;
            return this;
        }
        public ModPopupBuilder ResetPosition()
        {
            this._hasPosition = false;
            return this;
        }

        // =============================================
        // Builder
        // =============================================

        public PopupController Build()
        {
            var popupBuilder = new PopupBuilder<TitleBarController, ContentController>()
                .PopupID(DIALOG_ID)
                .Title(ModLocalization.GetString("windowTitle"))
                .TitleBarBuilder(
                    new TitleBarBuilder().ViewModel(_viewModel)
                )
                .ContentBuilder(
                    new ContentBuilder().ViewModel(_viewModel)
                )
                .Size(new Vector2(VesselBookmarkPalette.WindowWidth, VesselBookmarkPalette.WindowHeight));
            if (this._hasPosition)
            {
                popupBuilder = popupBuilder.Position(this._position);
            }
            PopupController popupController = popupBuilder.Build();
            if (popupController == null) return null;

            // Filter menu + internal overlays, grafted on the window above the content and title bar. They
            // self-manage their visibility through the ViewModel (FilterMenuOpen / EditingComment /
            // PendingRemoval), so nothing else needs wiring here.
            Transform windowTransform = popupController.GetGameObject().transform;

            new FilterMenuBuilder()
                .ViewModel(_viewModel)
                .Parent(windowTransform)
                .Build();

            new EditCommentOverlayBuilder()
                .ViewModel(_viewModel)
                .Parent(windowTransform)
                .Build();

            new RemoveConfirmOverlayBuilder()
                .ViewModel(_viewModel)
                .Parent(windowTransform)
                .Build();

            return popupController;
        }
    }
}
