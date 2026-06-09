using UnityEngine;
using UnityEngine.UI;
using com.github.lhervier.ksp.shared.ugui.button;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.sprites;
using com.github.lhervier.ksp.bookmarksmod;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.titleBar
{
    /// <summary>
    /// Two-column title bar: the title on the left (flexible), and on the right a group holding first
    /// the "shown / total" count badge, then the ＋ (add the active vessel), ↻ (refresh), ⋯ (filter
    /// menu, with its green "active filter" dot) and ✕ (close) buttons.
    /// </summary>
    public class TitleBarBuilder
    {
        private const string AddGlyph = "+";
        private const string RefreshGlyph = "↻";   // ↻ (U+21BB) — renders fine with the UISkin font
        private const string MenuGlyph = "⋯";       // ⋯ (U+22EF) — fallback to "≡" / "..." if not rendered
        private const string CloseGlyph = "×";      // × (U+00D7) — matches the shared popup close button

        private readonly BookmarksViewModel _viewModel;

        public TitleBarBuilder(BookmarksViewModel viewModel)
        {
            this._viewModel = viewModel;
        }

        public TitleBarController Create()
        {
            var go = new GameObject("Bookmarks.TitleBar", typeof(RectTransform));
            TitleBarController controller = go.AddComponent<TitleBarController>();
            controller.Initialize(_viewModel);

            // Escape the popupWindow's VerticalLayoutGroup: we anchor ourselves to the top.
            var layoutElement = go.AddComponent<LayoutElement>();
            layoutElement.ignoreLayout = true;

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(
                -2f * VesselBookmarkPalette.WindowBorderThickness,
                VesselBookmarkPalette.TitleBarHeight);
            rect.anchoredPosition = new Vector2(0f, -VesselBookmarkPalette.WindowBorderThickness);

            // Background + 1px separator at the bottom
            var image = go.AddComponent<Image>();
            image.sprite = Sprites.HorizontalBorders(
                VesselBookmarkPalette.TitleBarBgColor,
                VesselBookmarkPalette.TitleBarBorderColor,
                VesselBookmarkPalette.TitleBarBorderThickness);
            image.type = Image.Type.Sliced;
            image.color = Color.white;
            image.raycastTarget = true;

            var layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(
                Mathf.RoundToInt(VesselBookmarkPalette.DefaultPaddingLeft),
                Mathf.RoundToInt(VesselBookmarkPalette.DefaultPaddingRight),
                5, 5);   // top/bottom mirror the shared popup title bar padding (DefaultPalette.Padding{Top,Bottom})
            layout.spacing = VesselBookmarkPalette.DefaultSpacing;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            // Left column: the title (takes all available width and pushes the right column against
            // the right edge).
            var titleGo = new GameObject("Title", typeof(RectTransform));
            titleGo.transform.SetParent(go.transform, false);
            var titleElement = titleGo.AddComponent<LayoutElement>();
            titleElement.flexibleWidth = 1f;
            var title = titleGo.AddComponent<Text>();
            title.text = ModLocalization.GetString("windowTitle").ToUpperInvariant();
            title.font = HighLogic.UISkin.font;
            title.fontSize = VesselBookmarkPalette.TitleFontSize;
            title.fontStyle = FontStyle.Bold;
            title.color = VesselBookmarkPalette.TitleColor;
            title.alignment = TextAnchor.MiddleLeft;
            title.horizontalOverflow = HorizontalWrapMode.Overflow;
            title.verticalOverflow = VerticalWrapMode.Overflow;
            title.raycastTarget = false;

            // Right column: count badge (first) then the ＋ ↻ ⋯ ✕ buttons. Width is driven by the
            // content (no flexibleWidth), so the group stays pinned to the right.
            var rightColumnGo = new GameObject("RightColumn", typeof(RectTransform));
            rightColumnGo.transform.SetParent(go.transform, false);
            var rightLayout = rightColumnGo.AddComponent<HorizontalLayoutGroup>();
            rightLayout.spacing = VesselBookmarkPalette.DefaultSpacing;
            rightLayout.childAlignment = TextAnchor.MiddleLeft;
            rightLayout.childControlWidth = true;
            rightLayout.childControlHeight = true;
            rightLayout.childForceExpandWidth = false;
            rightLayout.childForceExpandHeight = false;
            Transform right = rightColumnGo.transform;

            // "shown / total" count badge — first element of the right column
            Text countLabel = BuildCountBadge(right);
            controller.BindCountLabel(countLabel);

            // "Add the active vessel" button
            ButtonController add = new VBMButtonBuilder()
                .ObjectName("Add")
                .Label(AddGlyph)
                .Interactable(_viewModel.CanAddVesselBookmark())
                .Build();
            add.OnClick.Add(() => _viewModel.AddVesselBookmark());
            add.transform.SetParent(right, false);
            Tooltips.Attach(add.gameObject, ModLocalization.GetString("buttonAdd"));
            controller.BindAddButton(add);

            // "Refresh" button
            ButtonController refresh = new VBMButtonBuilder()
                .ObjectName("Refresh")
                .Label(RefreshGlyph)
                .Build();
            refresh.OnClick.Add(() => _viewModel.ForceReload());
            refresh.transform.SetParent(right, false);
            Tooltips.Attach(refresh.gameObject, ModLocalization.GetString("buttonRefresh"));

            // Filter menu button "⋯" (toggles FilterMenuOpen) + green "active filter" dot
            ButtonController menu = new VBMButtonBuilder()
                .ObjectName("FilterMenu")
                .Label(MenuGlyph)
                .Build();
            menu.OnClick.Add(() => _viewModel.FilterMenuOpen = !_viewModel.FilterMenuOpen);
            menu.transform.SetParent(right, false);
            Tooltips.Attach(menu.gameObject, ModLocalization.GetString("menuFiltersTitle"));
            controller.BindFilterDot(BuildFilterDot(menu.gameObject));

            // Close button: closes the window (IMGUI follows, via WindowVisible)
            ButtonController close = new VBMButtonBuilder()
                .ObjectName("Close")
                .Label(CloseGlyph)
                .Build();
            close.OnClick.Add(() => _viewModel.WindowVisible = false);
            close.transform.SetParent(right, false);
            Tooltips.Attach(close.gameObject, ModLocalization.GetString("buttonClose"));

            return controller;
        }

        // Chip: sliced accent-border Image + accent Text. Size driven by the content + padding
        // (preferredSize reported by the HorizontalLayoutGroup to the parent layout).
        private Text BuildCountBadge(Transform parent)
        {
            var badgeGo = new GameObject("Count", typeof(RectTransform));
            badgeGo.transform.SetParent(parent, false);

            var image = badgeGo.AddComponent<Image>();
            image.sprite = Sprites.Border(
                VesselBookmarkPalette.AccentBgColor,
                VesselBookmarkPalette.AccentBorderColor,
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
            var label = labelGo.AddComponent<Text>();
            label.font = HighLogic.UISkin.font;
            label.fontSize = VesselBookmarkPalette.CountFontSize;
            label.color = VesselBookmarkPalette.AccentColor;
            label.alignment = TextAnchor.MiddleCenter;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.raycastTarget = false;

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
            img.sprite = Sprites.Fill;
            img.type = Image.Type.Simple;
            img.color = VesselBookmarkPalette.AccentColor;
            img.raycastTarget = false;
            img.enabled = false;
            return img;
        }

        public class TitleBarController : BaseController
        {
            private Text _countLabel;
            private ButtonController _addButton;
            private Image _filterDot;

            public void BindCountLabel(Text label) => this._countLabel = label;
            public void BindAddButton(ButtonController button) => this._addButton = button;
            public void BindFilterDot(Image dot) => this._filterDot = dot;

            public void Start()
            {
                this.ViewModel.OnAvailableBookmarksChanged.Add(OnAvailableBookmarksChanged);
                this.ViewModel.OnActiveOrTargetChanged.Add(OnActiveOrTargetChanged);

                // "Active filter" dot: refreshes whenever a filter changes
                this.ViewModel.OnSelectedBodyChanged.Add(UpdateFilterDot);
                this.ViewModel.OnSelectedVesselTypeChanged.Add(UpdateFilterDot);
                this.ViewModel.OnSearchTextChanged.Add(UpdateFilterDot);
                this.ViewModel.OnFilterHasCommentChanged.Add(UpdateFilterDot);

                UpdateCount();
                UpdateAddButton();
                UpdateFilterDot();
            }

            public void OnDestroy()
            {
                this.ViewModel?.OnAvailableBookmarksChanged.Remove(OnAvailableBookmarksChanged);
                this.ViewModel?.OnActiveOrTargetChanged.Remove(OnActiveOrTargetChanged);
                this.ViewModel?.OnSelectedBodyChanged.Remove(UpdateFilterDot);
                this.ViewModel?.OnSelectedVesselTypeChanged.Remove(UpdateFilterDot);
                this.ViewModel?.OnSearchTextChanged.Remove(UpdateFilterDot);
                this.ViewModel?.OnFilterHasCommentChanged.Remove(UpdateFilterDot);
            }

            private void OnAvailableBookmarksChanged() => UpdateCount();
            private void OnActiveOrTargetChanged() => UpdateAddButton();

            private void UpdateCount()
            {
                if (_countLabel == null) return;
                _countLabel.text = $"{ViewModel.AvailableBookmarksCount} / {ViewModel.TotalBookmarksCount}";
            }

            private void UpdateAddButton()
            {
                if (_addButton == null) return;
                _addButton.SetInteractable(ViewModel.CanAddVesselBookmark());
            }

            private void UpdateFilterDot()
            {
                if (_filterDot != null) _filterDot.enabled = ViewModel.HasActiveFilters;
            }
        }
    }
}
