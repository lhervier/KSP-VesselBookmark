using UnityEngine;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.shared;
using com.github.lhervier.ksp.shared.ugui;
using com.github.lhervier.ksp.shared.ugui.internalpopup;
using com.github.lhervier.ksp.shared.ugui.styles;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.overlays.remove
{
    /// <summary>
    /// Assembles the remove-confirmation internal popup: title + message content + Cancel/Remove footer,
    /// driven by an orchestrator bound to ViewModel.PendingRemoval.
    /// </summary>
    public class RemoveConfirmOverlayBuilder : IUGUIBuilder<RemoveConfirmOverlayController>
    {
        private BookmarksViewModel _viewModel;
        public RemoveConfirmOverlayBuilder WithViewModel(BookmarksViewModel viewModel)
        {
            this._viewModel = viewModel;
            return this;
        }

        private Transform _parent;
        public RemoveConfirmOverlayBuilder WithParent(Transform parent)
        {
            this._parent = parent;
            return this;
        }

        public RemoveConfirmOverlayController Build()
        {
            var popupBuilder = new InternalPopupBuilder<RemoveConfirmContentController, RemoveConfirmFooterController>()
                .WithParent(_parent)
                .WithTitle(ModLocalization.GetString("dialogRemoveTitle"))
                .WithTitleColor(DefaultPalette.DangerColor)
                .WithContentBuilder(new RemoveConfirmContentBuilder())
                .WithFooterBuilder(new RemoveConfirmFooterBuilder());

            InternalPopupController popup = popupBuilder.Build();
            RemoveConfirmContentController content = popupBuilder.ContentController;
            RemoveConfirmFooterController footer = popupBuilder.FooterController;

            // The orchestrator lives on the popup's always-active root.
            return popup.gameObject
                .AddComponent<RemoveConfirmOverlayController>()
                .WithViewModel(_viewModel)
                .WithPopupController(popup)
                .WithMessageComponent(content.GetMessage())
                .WithCancelButtonController(footer.GetCancelButtonController())
                .WithRemoveButtonController(footer.GetRemoveButtonController());
        }
    }
}
