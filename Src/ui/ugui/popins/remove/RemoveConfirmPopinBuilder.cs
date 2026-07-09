using UnityEngine;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.shared;
using com.github.lhervier.ksp.shared.ugui;
using com.github.lhervier.ksp.shared.ugui.popin;
using com.github.lhervier.ksp.shared.ugui.styles;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.popins.remove
{
    /// <summary>
    /// Assembles the remove-confirmation internal popup: title + message content + Cancel/Remove footer,
    /// driven by an orchestrator bound to ViewModel.PendingRemoval.
    /// </summary>
    public class RemoveConfirmPopinBuilder : IUGUIBuilder<RemoveConfirmPopinController>
    {
        private BookmarksViewModel _viewModel;
        public RemoveConfirmPopinBuilder WithViewModel(BookmarksViewModel viewModel)
        {
            this._viewModel = viewModel;
            return this;
        }

        private Transform _parent;
        public RemoveConfirmPopinBuilder WithParent(Transform parent)
        {
            this._parent = parent;
            return this;
        }

        public RemoveConfirmPopinController Build()
        {
            // The footer is a declared button bar; its clicks are wired to the ViewModel by the shared
            // PopinButtonBarController, so the orchestrator only drives show/close and the message.
            var popupBuilder = new ButtonBarPopinBuilder<RemoveConfirmContentController>()
                .WithParent(_parent)
                .WithTitle(ModLocalization.GetString("dialogRemoveTitle"))
                .WithTitleColor(DefaultPalette.DangerColor)
                .WithContentBuilder(new RemoveConfirmContentBuilder())
                .WithButton(ModLocalization.GetString("dialogButtonCancel"), _viewModel.CancelPendingRemoval)
                .WithButton(ModLocalization.GetString("dialogButtonRemove"), _viewModel.ConfirmPendingRemoval, PopinButtonStyle.Alert);

            PopinController popup = popupBuilder.Build();
            RemoveConfirmContentController content = popupBuilder.ContentController;

            // The orchestrator lives on the popup's always-active root.
            return popup.gameObject
                .AddComponent<RemoveConfirmPopinController>()
                .WithViewModel(_viewModel)
                .WithPopupController(popup)
                .WithMessageComponent(content.GetMessage());
        }
    }
}
