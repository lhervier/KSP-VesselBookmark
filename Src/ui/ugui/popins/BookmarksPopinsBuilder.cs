using UnityEngine;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.menu;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.popins.editcomment;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.popins.remove;
using com.github.lhervier.ksp.shared.ugui;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.popins
{
    /// <summary>
    /// Builds the bookmarks window popins (filter menu + edit-comment and remove-confirm internal popups)
    /// as siblings of the content and title bar. Each self-manages its visibility through the ViewModel
    /// (FilterMenuOpen / EditingComment / PendingRemoval), so nothing needs wiring after Build.
    /// </summary>
    public sealed class BookmarksPopinsBuilder : IUGUIBuilder<BookmarksPopinsController>
    {
        private BookmarksViewModel _viewModel;
        public BookmarksPopinsBuilder WithViewModel(BookmarksViewModel viewModel)
        {
            this._viewModel = viewModel;
            return this;
        }

        public BookmarksPopinsController Build()
        {
            // Just assemble the popins on a root; the PopupBuilder parents it onto the window and stretches
            // it full-window (above the content and title bar). Each popin self-anchors within this root.
            var rootGo = new GameObject("Bookmarks.Popins", typeof(RectTransform));

            new FilterMenuBuilder()
                .WithViewModel(_viewModel)
                .WithParent(rootGo.transform)
                .Build();

            new EditCommentPopinBuilder()
                .WithViewModel(_viewModel)
                .WithParent(rootGo.transform)
                .Build();

            new RemoveConfirmPopinBuilder()
                .WithViewModel(_viewModel)
                .WithParent(rootGo.transform)
                .Build();

            return rootGo.AddComponent<BookmarksPopinsController>();
        }
    }
}
