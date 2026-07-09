using UnityEngine;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.shared;
using com.github.lhervier.ksp.shared.ugui;
using com.github.lhervier.ksp.shared.ugui.popin;
using com.github.lhervier.ksp.shared.ugui.styles;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.popins.remove
{
    /// <summary>
    /// Assembles the remove-confirmation internal popup: a shared <see cref="ConfirmPopinBuilder"/> (danger
    /// title + Cancel/Remove footer), driven by an orchestrator bound to ViewModel.OnRemovalRequested.
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
            // The confirm popin owns its message and OK action; the orchestrator (re)fills them per request.
            ConfirmPopinController confirm = new ConfirmPopinBuilder()
                .WithParent(_parent)
                .WithTitle(ModLocalization.GetString("dialogRemoveTitle"))
                .WithTitleColor(DefaultPalette.DangerColor)
                .WithCancelLabel(ModLocalization.GetString("dialogButtonCancel"))
                .WithOkLabel(ModLocalization.GetString("dialogButtonRemove"))
                .WithOkStyle(PopinButtonStyle.Alert)
                .Build();

            // The orchestrator lives on the popup's always-active root.
            return confirm.gameObject
                .AddComponent<RemoveConfirmPopinController>()
                .WithViewModel(_viewModel)
                .WithConfirmPopin(confirm);
        }
    }
}
