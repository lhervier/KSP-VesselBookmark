using UnityEngine;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.menu;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.overlays.editcomment;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.overlays.remove;
using com.github.lhervier.ksp.shared.ugui;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.overlays
{
    /// <summary>
    /// Builds the bookmarks window overlays (filter menu + edit-comment and remove-confirm internal popups)
    /// as siblings of the content and title bar. Each self-manages its visibility through the ViewModel
    /// (FilterMenuOpen / EditingComment / PendingRemoval), so nothing needs wiring after Build.
    /// </summary>
    public sealed class BookmarksOverlaysBuilder : IUGUIBuilder<BookmarksOverlaysController>
    {
        private BookmarksViewModel _viewModel;
        public BookmarksOverlaysBuilder WithViewModel(BookmarksViewModel viewModel)
        {
            this._viewModel = viewModel;
            return this;
        }

        public BookmarksOverlaysController Build()
        {
            // Just assemble the overlays on a root; the PopupBuilder parents it onto the window and stretches
            // it full-window (above the content and title bar). Each overlay self-anchors within this root.
            var rootGo = new GameObject("Bookmarks.Overlays", typeof(RectTransform));

            new FilterMenuBuilder()
                .WithViewModel(_viewModel)
                .WithParent(rootGo.transform)
                .Build();

            new EditCommentOverlayBuilder()
                .WithViewModel(_viewModel)
                .WithParent(rootGo.transform)
                .Build();

            new RemoveConfirmOverlayBuilder()
                .WithViewModel(_viewModel)
                .WithParent(rootGo.transform)
                .Build();

            return rootGo.AddComponent<BookmarksOverlaysController>();
        }
    }
}
