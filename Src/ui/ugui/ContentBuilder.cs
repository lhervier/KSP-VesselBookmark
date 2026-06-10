using UnityEngine;
using UnityEngine.UI;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.body;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.footer;
using com.github.lhervier.ksp.shared.ugui;
using com.github.lhervier.ksp.shared.ugui.scrollableview;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui
{
    /// <summary>
    /// Popup content (everything below the shared title bar): a vertical layout filling the content host,
    /// with the scrollable bookmarks body taking all the remaining height on top and the footer action bar
    /// pinned at the bottom. Mounted by the shared PopupBuilder, which stretches it to fill the host.
    /// </summary>
    public class ContentBuilder : IUGUIBuilder<ContentController>
    {
        // ================================================
        // Builder parameters
        // ================================================

        private BookmarksViewModel _viewModel;
        public ContentBuilder ViewModel(BookmarksViewModel viewModel)
        {
            this._viewModel = viewModel;
            return this;
        }

        // =====================================================
        // Build
        // =====================================================

        public ContentController Build()
        {
            var go = new GameObject("Bookmarks.Content", typeof(RectTransform));

            // Vertical layout: the body stretches (flexibleHeight), the footer keeps its fixed height.
            var layout = go.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.spacing = 0f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            // Scrollable body — greedy on height so it fills the host minus the footer.
            ScrollableViewController body = new BodyBuilder()
                .ViewModel(_viewModel)
                .Build();
            body.transform.SetParent(go.transform, false);
            var bodyLayoutElement = body.gameObject.AddComponent<LayoutElement>();
            bodyLayoutElement.flexibleHeight = 1f;
            bodyLayoutElement.minHeight = 0f;

            // Footer — fixed height, pinned at the bottom by the layout (its own LayoutElement sizes it).
            FooterController footer = new FooterBuilder()
                .ViewModel(_viewModel)
                .Build();
            footer.transform.SetParent(go.transform, false);

            return go.AddComponent<ContentController>();
        }
    }
}
