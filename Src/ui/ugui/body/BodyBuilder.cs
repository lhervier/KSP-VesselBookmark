using UnityEngine;
using UnityEngine.UI;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.sprites;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.body.list;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.body
{
    /// <summary>
    /// Corps scrollable de la fenêtre (sous la title bar). Un contenu plus grand que la zone visible
    /// produit une scrollbar verticale à droite. Squelette pour l'instant : un placeholder dans le
    /// contenu. La liste des sections/bookmarks viendra s'y greffer.
    /// </summary>
    public class BodyBuilder
    {
        public const string BODY_NAME = "Bookmarks.Body";

        private readonly BookmarksViewModel _viewModel;

        public BodyBuilder(BookmarksViewModel viewModel)
        {
            this._viewModel = viewModel;
        }

        public BodyController Create()
        {
            var bodyGo = new GameObject(BODY_NAME, typeof(RectTransform));
            var controller = bodyGo.AddComponent<BodyController>();
            controller.Initialize(_viewModel);

            // Échappe au VerticalLayoutGroup de la popupWindow
            var layoutElement = bodyGo.AddComponent<LayoutElement>();
            layoutElement.ignoreLayout = true;

            // Remplit l'intérieur de la fenêtre, moins le chrome (1px), la title bar en haut et le footer en bas
            var rect = bodyGo.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(
                VesselBookmarkPalette.WindowBorderThickness,
                VesselBookmarkPalette.WindowBorderThickness + VesselBookmarkPalette.FooterHeight);
            rect.offsetMax = new Vector2(
                -VesselBookmarkPalette.WindowBorderThickness,
                -(VesselBookmarkPalette.WindowBorderThickness + VesselBookmarkPalette.TitleBarHeight));

            // ScrollRect : relie viewport (clip) + content (défilé) + scrollbar
            var scrollRect = bodyGo.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 20f;

            // Viewport : corps moins la colonne de scrollbar à droite. RectMask2D clippe le débordement.
            var viewportGo = new GameObject("Viewport", typeof(RectTransform));
            viewportGo.transform.SetParent(bodyGo.transform, false);
            var viewportRect = viewportGo.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = new Vector2(-VesselBookmarkPalette.ScrollbarWidth, 0f);
            viewportGo.AddComponent<RectMask2D>();
            var viewportImage = viewportGo.AddComponent<Image>();
            viewportImage.sprite = Sprites.Fill;
            viewportImage.type = Image.Type.Simple;
            viewportImage.color = Color.clear;
            viewportImage.raycastTarget = true;
            scrollRect.viewport = viewportRect;

            // Content : enfant du viewport, ancré sur son bord haut. Hauteur auto (ContentSizeFitter).
            var contentGo = new GameObject("Content", typeof(RectTransform));
            contentGo.transform.SetParent(viewportGo.transform, false);
            controller.BindContent(contentGo);
            var contentRect = contentGo.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = Vector2.zero;
            scrollRect.content = contentRect;

            var contentLayout = contentGo.AddComponent<VerticalLayoutGroup>();
            contentLayout.padding = new RectOffset(0, 0, 0, 0);
            contentLayout.spacing = 0f;
            contentLayout.childAlignment = TextAnchor.UpperLeft;
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = true;
            contentLayout.childForceExpandWidth = true;
            contentLayout.childForceExpandHeight = false;

            var contentFitter = contentGo.AddComponent<ContentSizeFitter>();
            contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // La liste des sections/bookmarks (se reconstruit/rafraîchit via ses propres abonnements)
            var list = new ListBuilder(_viewModel).Create();
            list.transform.SetParent(contentGo.transform, false);

            // Scrollbar verticale à droite
            var scrollbarGo = new GameObject("Scrollbar", typeof(RectTransform));
            scrollbarGo.transform.SetParent(bodyGo.transform, false);
            var scrollbarRect = scrollbarGo.GetComponent<RectTransform>();
            scrollbarRect.anchorMin = new Vector2(1f, 0f);
            scrollbarRect.anchorMax = new Vector2(1f, 1f);
            scrollbarRect.pivot = new Vector2(1f, 0.5f);
            scrollbarRect.sizeDelta = new Vector2(VesselBookmarkPalette.ScrollbarWidth, 0f);

            var scrollbarBg = scrollbarGo.AddComponent<Image>();
            scrollbarBg.sprite = Sprites.Fill;
            scrollbarBg.type = Image.Type.Simple;
            scrollbarBg.color = VesselBookmarkPalette.SearchBgColor;
            scrollbarBg.raycastTarget = true;

            var scrollbar = scrollbarGo.AddComponent<Scrollbar>();
            scrollbar.direction = Scrollbar.Direction.BottomToTop;

            var slidingAreaGo = new GameObject("Sliding Area", typeof(RectTransform));
            slidingAreaGo.transform.SetParent(scrollbarGo.transform, false);
            var slidingAreaRect = slidingAreaGo.GetComponent<RectTransform>();
            slidingAreaRect.anchorMin = Vector2.zero;
            slidingAreaRect.anchorMax = Vector2.one;
            slidingAreaRect.offsetMin = Vector2.zero;
            slidingAreaRect.offsetMax = Vector2.zero;

            var handleGo = new GameObject("Handle", typeof(RectTransform));
            handleGo.transform.SetParent(slidingAreaGo.transform, false);
            var handleRect = handleGo.GetComponent<RectTransform>();
            handleRect.anchorMin = Vector2.zero;
            handleRect.anchorMax = Vector2.one;
            handleRect.offsetMin = Vector2.zero;
            handleRect.offsetMax = Vector2.zero;
            var handleImage = handleGo.AddComponent<Image>();
            handleImage.sprite = Sprites.Fill;
            handleImage.type = Image.Type.Simple;
            handleImage.color = Color.white;
            handleImage.raycastTarget = true;
            scrollbar.targetGraphic = handleImage;
            scrollbar.handleRect = handleRect;

            var scrollbarColors = scrollbar.colors;
            scrollbarColors.normalColor = VesselBookmarkPalette.WindowBorderColor;
            scrollbarColors.highlightedColor = VesselBookmarkPalette.ScrollbarColor;
            scrollbarColors.pressedColor = VesselBookmarkPalette.ScrollbarColor;
            scrollbarColors.selectedColor = VesselBookmarkPalette.WindowBorderColor;
            scrollbarColors.disabledColor = VesselBookmarkPalette.WindowBorderColor;
            scrollbarColors.colorMultiplier = 1f;
            scrollbarColors.fadeDuration = 0.1f;
            scrollbar.colors = scrollbarColors;

            scrollRect.verticalScrollbar = scrollbar;

            return controller;
        }

        public class BodyController : BaseController
        {
            private GameObject _content;

            /// <summary>Le conteneur défilé où la liste viendra se greffer.</summary>
            public GameObject Content => _content;

            public void BindContent(GameObject content)
            {
                this._content = content;
            }
        }
    }
}
