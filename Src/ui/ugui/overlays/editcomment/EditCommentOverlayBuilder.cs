using UnityEngine;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.shared;
using com.github.lhervier.ksp.shared.ugui;
using com.github.lhervier.ksp.shared.ugui.internalpopup;
using com.github.lhervier.ksp.shared.ugui.styles;

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
        public EditCommentOverlayBuilder WithViewModel(BookmarksViewModel viewModel)
        {
            this._viewModel = viewModel;
            return this;
        }

        private Transform _parent;
        public EditCommentOverlayBuilder WithParent(Transform parent)
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
                .WithParent(_parent)
                .WithTitle(ModLocalization.GetString("editWindowTitle"))
                .WithTitleColor(DefaultPalette.AccentColor)
                .WithContentBuilder(new EditCommentContentBuilder())
                .WithFooterBuilder(new EditCommentFooterBuilder());

            InternalPopupController popup = popupBuilder.Build();
            EditCommentContentController content = popupBuilder.ContentController;
            EditCommentFooterController footer = popupBuilder.FooterController;

            // The orchestrator lives on the popup's always-active root.
            return popup.gameObject
                .AddComponent<EditCommentOverlayController>()
                .WithViewModel(_viewModel)
                .WithPopupController(popup)
                .WithSubComponent(content.GetSub())
                .WithInputField(content.GetInput())
                .WithCancelButtonController(footer.GetCancelButtonController())
                .WithOkButtonController(footer.GetOkButtonController());
        }
    }
}
