using UnityEngine;
using UnityEngine.UI;
using com.github.lhervier.ksp.bookmarksmod.bookmarks;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.sprites;
using com.github.lhervier.ksp.bookmarksmod;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.footer
{
    /// <summary>
    /// Barre d'actions du bas, fixée sous le corps : libellé de sélection à gauche, puis les boutons
    /// Éditer / Aller au vaisseau / Définir comme cible, actifs/grisés selon la sélection et l'état du jeu.
    /// </summary>
    public class FooterBuilder
    {
        private const string EditGlyph = "✎";    // ✎ (U+270E)
        private const string GoToGlyph = "➤";    // ➤ (U+27A4)
        private const string TargetGlyph = "◎";  // ◎ (U+25CE)

        private readonly BookmarksViewModel _viewModel;
        private readonly ButtonBuilder _buttonBuilder;

        public FooterBuilder(BookmarksViewModel viewModel)
        {
            this._viewModel = viewModel;
            this._buttonBuilder = new ButtonBuilder(viewModel);
        }

        public FooterController Create()
        {
            var go = new GameObject("Bookmarks.Footer", typeof(RectTransform));
            FooterController controller = go.AddComponent<FooterController>();
            controller.Initialize(_viewModel);

            var layoutElement = go.AddComponent<LayoutElement>();
            layoutElement.ignoreLayout = true;

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(-2f * VesselBookmarkPalette.WindowBorderThickness, VesselBookmarkPalette.FooterHeight);
            rect.anchoredPosition = new Vector2(0f, VesselBookmarkPalette.WindowBorderThickness);

            // Fond + séparateur 1px en haut
            var image = go.AddComponent<Image>();
            image.sprite = Sprites.HorizontalBorders(
                VesselBookmarkPalette.FooterBgColor,
                VesselBookmarkPalette.FooterBorderColor,
                VesselBookmarkPalette.FooterBorderThickness);
            image.type = Image.Type.Sliced;
            image.color = Color.white;
            image.raycastTarget = true;

            var layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(
                Mathf.RoundToInt(VesselBookmarkPalette.FooterPaddingH),
                Mathf.RoundToInt(VesselBookmarkPalette.FooterPaddingH),
                Mathf.RoundToInt(VesselBookmarkPalette.FooterPaddingV),
                Mathf.RoundToInt(VesselBookmarkPalette.FooterPaddingV));
            layout.spacing = VesselBookmarkPalette.FooterSpacing;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            // Libellé de sélection (prend la largeur disponible)
            var selGo = new GameObject("Selection", typeof(RectTransform));
            selGo.transform.SetParent(go.transform, false);
            var selLe = selGo.AddComponent<LayoutElement>();
            selLe.flexibleWidth = 1f;
            var selLabel = selGo.AddComponent<Text>();
            selLabel.font = HighLogic.UISkin.font;
            selLabel.fontSize = VesselBookmarkPalette.FooterSelFontSize;
            selLabel.fontStyle = FontStyle.Italic;
            selLabel.color = VesselBookmarkPalette.FooterSelColor;
            selLabel.alignment = TextAnchor.MiddleLeft;
            selLabel.horizontalOverflow = HorizontalWrapMode.Overflow;
            selLabel.verticalOverflow = VerticalWrapMode.Overflow;
            selLabel.raycastTarget = false;
            controller.BindSelectionLabel(selLabel);

            // Glyphes de la maquette : ✎ éditer, ➤ aller, ◎ cibler (boutons carrés, comme la title bar).
            ButtonController edit = _buttonBuilder.Create(
                "Edit", EditGlyph, () => _viewModel.BeginCommentEdition(), false,
                VesselBookmarkPalette.ButtonBgColor, VesselBookmarkPalette.ButtonHoverColor,
                VesselBookmarkPalette.FooterButtonHeight, VesselBookmarkPalette.FooterButtonFontSize);
            edit.transform.SetParent(go.transform, false);
            Tooltips.Attach(edit.gameObject, ModLocalization.GetString("tooltipEdit"));

            ButtonController goTo = _buttonBuilder.Create(
                "GoTo", GoToGlyph, () => _viewModel.SwitchToSelectedVessel(), false,
                VesselBookmarkPalette.ButtonBgColor, VesselBookmarkPalette.ButtonHoverColor,
                VesselBookmarkPalette.FooterButtonHeight, VesselBookmarkPalette.FooterButtonFontSize);
            goTo.transform.SetParent(go.transform, false);
            Tooltips.Attach(goTo.gameObject, ModLocalization.GetString("tooltipGoTo"));

            ButtonController target = _buttonBuilder.Create(
                "Target", TargetGlyph, () => _viewModel.SetCurrentBookmarkVesselAsTarget(), false,
                VesselBookmarkPalette.ButtonBgColor, VesselBookmarkPalette.ButtonHoverColor,
                VesselBookmarkPalette.FooterButtonHeight, VesselBookmarkPalette.FooterButtonFontSize);
            target.transform.SetParent(go.transform, false);
            Tooltips.Attach(target.gameObject, ModLocalization.GetString("tooltipSetTargetAs"));

            controller.BindButtons(edit, goTo, target);
            return controller;
        }

        public class FooterController : BaseController
        {
            private Text _selLabel;
            private ButtonController _edit;
            private ButtonController _goTo;
            private ButtonController _target;

            public void BindSelectionLabel(Text label) => this._selLabel = label;

            public void BindButtons(ButtonController edit, ButtonController goTo, ButtonController target)
            {
                this._edit = edit;
                this._goTo = goTo;
                this._target = target;
            }

            public void Start()
            {
                ViewModel.OnSelectedBookmarkChanged.Add(Refresh);
                ViewModel.OnActiveOrTargetChanged.Add(Refresh);
                Refresh();
            }

            public void OnDestroy()
            {
                ViewModel?.OnSelectedBookmarkChanged.Remove(Refresh);
                ViewModel?.OnActiveOrTargetChanged.Remove(Refresh);
            }

            private void Refresh()
            {
                Bookmark sel = ViewModel.SelectedBookmark;
                if (_selLabel != null)
                {
                    _selLabel.text = sel != null
                        ? ModLocalization.GetString("footerSelection", sel.BookmarkTitle)
                        : ModLocalization.GetString("footerNoSelection");
                }
                if (_edit != null) _edit.SetInteractable(ViewModel.CanEditCurrentVesselComment());
                if (_goTo != null) _goTo.SetInteractable(ViewModel.CanSwitchToCurrentBookmarkVessel());
                if (_target != null) _target.SetInteractable(ViewModel.CanSetCurrentBookmarkVesselAsTarget());
            }
        }
    }
}
