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
            return new ScrollableViewBuilder<ListController>()
                .WithObjectName("Bookmarks.Body")
                .WithContentBuilder(new ListBuilder().WithViewModel(_viewModel))
                .WithScrollbarWidth(VesselBookmarkPalette.ScrollbarWidth)
                .WithScrollbarBackgroundColor(VesselBookmarkPalette.SearchBgColor)
                .WithHandleColor(PopupPalette.PopupBorderColor)
                .WithHandleHoverColor(VesselBookmarkPalette.ScrollbarColor)
                .Build();
        }
    }
}
