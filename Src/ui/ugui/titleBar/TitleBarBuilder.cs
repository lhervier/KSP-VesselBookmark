using UnityEngine;
using UnityEngine.UI;
using TMPro;
using com.github.lhervier.ksp.shared.ugui.button;
using com.github.lhervier.ksp.shared.ugui.styles;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.sprites;
using com.github.lhervier.ksp.shared;
using com.github.lhervier.ksp.shared.ugui;
using com.github.lhervier.ksp.shared.ugui.sprites;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.titleBar
{
    /// <summary>
    /// Right-side content of the popup's title bar: the "shown / total" count badge, then the ＋ (add the
    /// active vessel), ↻ (refresh) and ⋯ (filter menu, with its green "active filter" dot) buttons. The
    /// title bar frame, the title on the left and the ✕ close button are provided by the shared PopupBuilder.
    /// </summary>
    public class TitleBarBuilder : IUGUIBuilder<TitleBarController>
    {
        private const string AddGlyph = "+";

        // ====================================
        // Builder parameters
        // ====================================

        private BookmarksViewModel _viewModel;
        public TitleBarBuilder ViewModel(BookmarksViewModel viewModel)
        {
            this._viewModel = viewModel;
            return this;
        }

        // ===================================
        // Build
        // ===================================

        public TitleBarController Build()
        {
            var rightColumnGo = new GameObject("Bookmarks.TitleBar.RightColumn", typeof(RectTransform));
            
            // Right column: count badge (first) then the ＋ ↻ ⋯ buttons. Width is driven by the content
            // (no flexibleWidth), so the group stays pinned to the right of the shared title bar.
            var rightLayout = rightColumnGo.AddComponent<HorizontalLayoutGroup>();
            rightLayout.spacing = DefaultPalette.Spacing;
            rightLayout.childAlignment = TextAnchor.MiddleLeft;
            rightLayout.childControlWidth = true;
            rightLayout.childControlHeight = true;
            rightLayout.childForceExpandWidth = false;
            rightLayout.childForceExpandHeight = false;
            Transform right = rightColumnGo.transform;

            // "shown / total" count badge — first element of the right column
            TextMeshProUGUI countLabel = BuildCountBadge(right);

            // "Add the active vessel" button
            ButtonController add = NewButton("Add", AddGlyph, _viewModel.CanAddVesselBookmark());
            add.OnClick.Add(() => _viewModel.AddVesselBookmark());
            add.transform.SetParent(right, false);
            Tooltips.Attach(add.gameObject, ModLocalization.GetString("buttonAdd"));

            // "Refresh" button. No circular-arrow glyph in the game SDF font: use the shared "refresh"
            // sprite, falling back to a text glyph if the texture is missing.
            string refreshLabel = SpritesIcons.HasSprite("refresh")
                ? "<sprite name=\"refresh\" tint=1>"
                : DefaultPalette.PickGlyph("↻", "⟳", "↺", "R");
            ButtonController refresh = NewButton("Refresh", refreshLabel, true);
            refresh.OnClick.Add(() => _viewModel.ForceReload());
            refresh.transform.SetParent(right, false);
            Tooltips.Attach(refresh.gameObject, ModLocalization.GetString("buttonRefresh"));

            // Filter menu button "⋯" (toggles FilterMenuOpen) + green "active filter" dot
            ButtonController menu = NewButton("FilterMenu", DefaultPalette.PickGlyph("⋯", "…", "≡", "..."), true);
            menu.OnClick.Add(() => _viewModel.FilterMenuOpen = !_viewModel.FilterMenuOpen);
            menu.transform.SetParent(right, false);
            Tooltips.Attach(menu.gameObject, ModLocalization.GetString("menuFiltersTitle"));
            
            return rightColumnGo
                .AddComponent<TitleBarController>()
                .ViewModel(_viewModel)
                .CountLabel(countLabel)
                .AddButtonController(add)
                .FilterDot(
                    BuildFilterDot(menu.gameObject)
                );
        }

        // Square title-bar button matching the shared ✕ close button (same size and colors), so the four
        // buttons of the title bar stay homogeneous.
        private static ButtonController NewButton(string objectName, string glyph, bool interactable)
        {
            return new ButtonBuilder()
                .ObjectName(objectName)
                .Label(glyph)
                .Interactable(interactable)
                .BackgroundColor(PopupPalette.TitleBarButtonColor)
                .HoverColor(PopupPalette.TitleBarButtonHoverColor)
                .Build();
        }

        // Chip: sliced accent-border Image + accent Text. Size driven by the content + padding
        // (preferredSize reported by the HorizontalLayoutGroup to the parent layout).
        private TextMeshProUGUI BuildCountBadge(Transform parent)
        {
            var badgeGo = new GameObject("Count", typeof(RectTransform));
            badgeGo.transform.SetParent(parent, false);

            var image = badgeGo.AddComponent<Image>();
            image.sprite = SpritesGlobal.Border(
                DefaultPalette.AccentBgColor,
                DefaultPalette.AccentBorderColor,
                1);
            image.type = Image.Type.Sliced;
            image.color = Color.white;
            image.raycastTarget = false;

            var layout = badgeGo.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(
                Mathf.RoundToInt(VesselBookmarkPalette.CountPaddingH),
                Mathf.RoundToInt(VesselBookmarkPalette.CountPaddingH),
                2, 2);
            layout.spacing = 0f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            var labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(badgeGo.transform, false);
            var label = UGUILabels.AddLabel(labelGo);
            label.fontSize = VesselBookmarkPalette.CountFontSize;
            label.color = DefaultPalette.AccentColor;
            label.alignment = TextAlignmentOptions.Center;

            return label;
        }

        // Small green dot in the top-right corner of the ⋯ button, hidden by default.
        private Image BuildFilterDot(GameObject buttonGo)
        {
            var dotGo = new GameObject("FilterDot", typeof(RectTransform));
            dotGo.transform.SetParent(buttonGo.transform, false);
            var rect = dotGo.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.sizeDelta = new Vector2(VesselBookmarkPalette.FilterDotSize, VesselBookmarkPalette.FilterDotSize);
            rect.anchoredPosition = new Vector2(-1f, -1f);
            var img = dotGo.AddComponent<Image>();
            img.sprite = SpritesGlobal.FillSprite;
            img.type = Image.Type.Simple;
            img.color = DefaultPalette.AccentColor;
            img.raycastTarget = false;
            img.enabled = false;
            return img;
        }
    }
}
