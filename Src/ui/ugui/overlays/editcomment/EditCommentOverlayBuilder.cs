using UnityEngine;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.shared;
using com.github.lhervier.ksp.shared.ugui;
using com.github.lhervier.ksp.shared.ugui.internalpopup;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.overlays.editcomment
{
    /// <summary>
    /// Assembles the edit-comment internal popup: title + comment content + Cancel/Save footer, driven
    /// by an orchestrator bound to ViewModel.EditingComment / ViewModel.Comment.
    /// </summary>
    public class EditCommentOverlayBuilder : IUGUIBuilder<EditCommentOverlayController>
    {
        // ==========================================
        // Builder parameters
        // ==========================================

        private BookmarksViewModel _viewModel;
        public EditCommentOverlayBuilder ViewModel(BookmarksViewModel viewModel)
        {
            this._viewModel = viewModel;
            return this;
        }

        private Transform _parent;
        public EditCommentOverlayBuilder Parent(Transform parent)
        {
            this._parent = parent;
            return this;
        }

        // ==========================================
        // Build
        // ==========================================

        public EditCommentOverlayController Build()
        {
            var popupBuilder = new InternalPopupBuilder<EditCommentContentController, EditCommentFooterController>()
                .Parent(_parent)
                .Title(ModLocalization.GetString("editWindowTitle"))
                .TitleColor(VesselBookmarkPalette.AccentColor)
                .Content(new EditCommentContentBuilder())
                .Footer(new EditCommentFooterBuilder());

            InternalPopupController popup = popupBuilder.Build();
            EditCommentContentController content = popupBuilder.ContentController;
            EditCommentFooterController footer = popupBuilder.FooterController;

            // The orchestrator lives on the popup's always-active root.
            return popup.gameObject
                .AddComponent<EditCommentOverlayController>()
                .ViewModel(_viewModel)
                .Popup(popup)
                .Sub(content.GetSub())
                .Input(content.GetInput())
                .CancelButtonController(footer.GetCancelButtonController())
                .OkButtonController(footer.GetOkButtonController());
        }
    }
}
