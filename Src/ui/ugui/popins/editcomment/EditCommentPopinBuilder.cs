using UnityEngine;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.shared;
using com.github.lhervier.ksp.shared.ugui;
using com.github.lhervier.ksp.shared.ugui.popin;
using com.github.lhervier.ksp.shared.ugui.styles;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.popins.editcomment
{
    /// <summary>
    /// Assembles the edit-comment internal popup: title + comment content + Cancel/Save footer, driven
    /// by an orchestrator bound to ViewModel.EditingComment / ViewModel.Comment.
    /// </summary>
    public class EditCommentPopinBuilder : IUGUIBuilder<EditCommentPopinController>
    {
        // ==========================================
        // Builder parameters
        // ==========================================

        private BookmarksViewModel _viewModel;
        public EditCommentPopinBuilder WithViewModel(BookmarksViewModel viewModel)
        {
            this._viewModel = viewModel;
            return this;
        }

        private Transform _parent;
        public EditCommentPopinBuilder WithParent(Transform parent)
        {
            this._parent = parent;
            return this;
        }

        // ==========================================
        // Build
        // ==========================================

        public EditCommentPopinController Build()
        {
            // The footer is a declared button bar; its clicks are wired to the ViewModel by the shared
            // PopinButtonBarController, so the orchestrator only drives show/close and the content binding.
            var popupBuilder = new ButtonBarPopinBuilder<EditCommentContentController>()
                .WithParent(_parent)
                .WithTitle(ModLocalization.GetString("editWindowTitle"))
                .WithTitleColor(DefaultPalette.AccentColor)
                .WithContentBuilder(new EditCommentContentBuilder())
                .WithButton(ModLocalization.GetString("buttonCancel"), _viewModel.CancelBookmarkCommentEdition)
                .WithButton(ModLocalization.GetString("buttonSave"), _viewModel.SaveBookmarkComment, PopinButtonStyle.Confirm);

            PopinController popup = popupBuilder.Build();
            EditCommentContentController content = popupBuilder.ContentController;

            // The orchestrator lives on the popup's always-active root.
            return popup.gameObject
                .AddComponent<EditCommentPopinController>()
                .WithViewModel(_viewModel)
                .WithPopupController(popup)
                .WithSubComponent(content.GetSub())
                .WithTextFieldController(content.GetTextFieldController());
        }
    }
}
