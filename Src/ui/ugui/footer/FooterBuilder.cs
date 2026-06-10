using UnityEngine;
using UnityEngine.UI;
using com.github.lhervier.ksp.shared.ugui.button;
using com.github.lhervier.ksp.bookmarksmod.bookmarks;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.sprites;
using com.github.lhervier.ksp.shared;

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

        public FooterBuilder(BookmarksViewModel viewModel)
        {
            this._viewModel = viewModel;
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

            // Mockup glyphs: ✎ edit, ➤ go to, ◎ target (square buttons, like the title bar).
            // Background/hover colors come from the VBMButtonBuilder defaults; only the footer-specific
            // size and font size are overridden here.
            ButtonController edit = new VBMButtonBuilder()
                .ObjectName("Edit")
                .Label(EditGlyph)
                .Interactable(false)
                .Size(VesselBookmarkPalette.FooterButtonHeight)
                .FontSize(VesselBookmarkPalette.FooterButtonFontSize)
                .Build();
            edit.OnClick.Add(() => _viewModel.BeginCommentEdition());
            edit.transform.SetParent(go.transform, false);
            Tooltips.Attach(edit.gameObject, ModLocalization.GetString("tooltipEdit"));

            ButtonController goTo = new VBMButtonBuilder()
                .ObjectName("GoTo")
                .Label(GoToGlyph)
                .Interactable(false)
                .Size(VesselBookmarkPalette.FooterButtonHeight)
                .FontSize(VesselBookmarkPalette.FooterButtonFontSize)
                .Build();
            goTo.OnClick.Add(() => _viewModel.SwitchToSelectedVessel());
            goTo.transform.SetParent(go.transform, false);
            Tooltips.Attach(goTo.gameObject, ModLocalization.GetString("tooltipGoTo"));

            ButtonController target = new VBMButtonBuilder()
                .ObjectName("Target")
                .Label(TargetGlyph)
                .Interactable(false)
                .Size(VesselBookmarkPalette.FooterButtonHeight)
                .FontSize(VesselBookmarkPalette.FooterButtonFontSize)
                .Build();
            target.OnClick.Add(() => _viewModel.SetCurrentBookmarkVesselAsTarget());
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
