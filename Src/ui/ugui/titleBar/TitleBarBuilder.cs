using UnityEngine;
using UnityEngine.UI;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.sprites;
using com.github.lhervier.ksp.bookmarksmod;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.titleBar
{
    /// <summary>
    /// Barre de titre : titre à gauche, puis badge compteur "affichés / total", boutons ＋ (ajouter le
    /// vaisseau actif) et ↻ (rafraîchir), et le bouton de fermeture ✕. Le menu filtres « ⋯ » viendra
    /// s'intercaler ensuite, avec son panneau déroulant.
    /// </summary>
    public class TitleBarBuilder
    {
        private const string AddGlyph = "+";
        private const string RefreshGlyph = "↻";   // ↻ (U+21BB) — rendu OK avec la police UISkin
        private const string CloseGlyph = "✕";      // ✕ (U+2715)

        private readonly BookmarksViewModel _viewModel;
        private readonly ButtonBuilder _buttonBuilder;

        public TitleBarBuilder(BookmarksViewModel viewModel)
        {
            this._viewModel = viewModel;
            this._buttonBuilder = new ButtonBuilder(viewModel);
        }

        public TitleBarController Create()
        {
            var go = new GameObject("Bookmarks.TitleBar", typeof(RectTransform));
            TitleBarController controller = go.AddComponent<TitleBarController>();
            controller.Initialize(_viewModel);

            // Échappe au VerticalLayoutGroup de la popupWindow : on s'ancre nous-mêmes en haut.
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

            // Fond + séparateur 1px en bas
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
                3, 3);
            layout.spacing = VesselBookmarkPalette.DefaultSpacing;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            // Titre (prend toute la largeur disponible)
            var titleGo = new GameObject("Title", typeof(RectTransform));
            titleGo.transform.SetParent(go.transform, false);
            var titleElement = titleGo.AddComponent<LayoutElement>();
            titleElement.flexibleWidth = 1f;
            var title = titleGo.AddComponent<Text>();
            title.text = ModLocalization.GetString("windowTitle");
            title.font = HighLogic.UISkin.font;
            title.fontSize = VesselBookmarkPalette.TitleFontSize;
            title.fontStyle = FontStyle.Bold;
            title.color = VesselBookmarkPalette.TitleColor;
            title.alignment = TextAnchor.MiddleLeft;
            title.horizontalOverflow = HorizontalWrapMode.Overflow;
            title.verticalOverflow = VerticalWrapMode.Overflow;
            title.raycastTarget = false;

            // Badge compteur "affichés / total"
            Text countLabel = BuildCountBadge(go.transform);
            controller.BindCountLabel(countLabel);

            // Bouton "ajouter le vaisseau actif"
            ButtonController add = _buttonBuilder.Create(
                "Add",
                AddGlyph,
                () => _viewModel.AddVesselBookmark(),
                _viewModel.CanAddVesselBookmark());
            add.transform.SetParent(go.transform, false);
            controller.BindAddButton(add);

            // Bouton "rafraîchir"
            ButtonController refresh = _buttonBuilder.Create(
                "Refresh",
                RefreshGlyph,
                () => _viewModel.RefreshBookmarks());
            refresh.transform.SetParent(go.transform, false);

            // Bouton de fermeture : ferme la fenêtre (l'IMGUI suit, via WindowVisible)
            ButtonController close = _buttonBuilder.Create(
                "Close",
                CloseGlyph,
                () => _viewModel.WindowVisible = false);
            close.transform.SetParent(go.transform, false);

            return controller;
        }

        // Chip : Image bordure accent slicée + Text accent. Taille pilotée par le contenu + padding
        // (preferredSize rapporté par le HorizontalLayoutGroup au layout parent).
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

        public class TitleBarController : BaseController
        {
            private Text _countLabel;
            private ButtonController _addButton;

            public void BindCountLabel(Text label) => this._countLabel = label;
            public void BindAddButton(ButtonController button) => this._addButton = button;

            public void Start()
            {
                this.ViewModel.OnAvailableBookmarksChanged.Add(OnAvailableBookmarksChanged);
                this.ViewModel.OnActiveOrTargetChanged.Add(OnActiveOrTargetChanged);

                UpdateCount();
                UpdateAddButton();
            }

            public void OnDestroy()
            {
                this.ViewModel?.OnAvailableBookmarksChanged.Remove(OnAvailableBookmarksChanged);
                this.ViewModel?.OnActiveOrTargetChanged.Remove(OnActiveOrTargetChanged);
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
        }
    }
}
