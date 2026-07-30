using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.body.list;
using com.github.lhervier.ksp.shared.ugui;
using com.github.lhervier.ksp.shared.ugui.scrollableview;
using com.github.lhervier.ksp.shared.ugui.styles;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.body
{
    /// <summary>
    /// Corps scrollable de la fenêtre : une vue défilante (composant partagé) dont le contenu est la liste
    /// des bookmarks. Un contenu plus grand que la zone visible produit une scrollbar verticale à droite.
    /// </summary>
    public class BodyBuilder : IUGUIBuilder<ScrollableViewController>
    {
        // Scroll offset of the list. Static because the window — hence the scrollable view, hence this
        // builder — is destroyed and rebuilt on every scene change, and the list must reopen where it was
        // left, like the window's position and open state already do. Deliberately lost when the game
        // restarts: nothing is written to disk.
        private static float _scrollOffset = 0f;

        // ===================================================
        // Builder parameters
        // ===================================================

        private BookmarksViewModel _viewModel;
        public BodyBuilder WithViewModel(BookmarksViewModel viewModel)
        {
            this._viewModel = viewModel;
            return this;
        }

        // =========================================
        // Build
        // =========================================

        public ScrollableViewController Build()
        {
            ScrollableViewController body = new ScrollableViewBuilder<ListController>()
                .WithObjectName("Bookmarks.Body")
                .WithInitialScrollOffset(_scrollOffset)
                .WithContentBuilder(new ListBuilder().WithViewModel(_viewModel))
                .WithScrollbarWidth(VesselBookmarkPalette.ScrollbarWidth)
                .WithScrollbarBackgroundColor(VesselBookmarkPalette.SearchBgColor)
                .WithHandleColor(PopupPalette.PopupBorderColor)
                .WithHandleHoverColor(VesselBookmarkPalette.ScrollbarColor)
                .Build();

            // No unsubscription: the event belongs to the controller, which dies with the view (and takes
            // the delegate, hence this builder, with it).
            body.OnScrollOffsetChanged.Add(OnScrollOffsetChanged);
            return body;
        }

        /// <summary>
        /// The user scrolled the list : remember where, for the next time the view is built.
        /// </summary>
        /// <param name="scrollOffset">The new scroll offset, in pixels from the top of the list</param>
        // Instance method although it only writes a static field: KSP's EventData.Add reads
        // evt.Target.GetType(), which throws on the null Target of a static handler.
        private void OnScrollOffsetChanged(float scrollOffset)
        {
            _scrollOffset = scrollOffset;
        }
    }
}
