using UnityEngine;
using UnityEngine.UI;
using com.github.lhervier.ksp.shared.ugui.button;
using com.github.lhervier.ksp.bookmarksmod.bookmarks;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.sprites;
using com.github.lhervier.ksp.shared;
using com.github.lhervier.ksp.shared.ugui;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.footer
{
    /// <summary>
    /// Barre d'actions du bas, fixée sous le corps : libellé de sélection à gauche, puis les boutons
    /// Éditer / Aller au vaisseau / Définir comme cible, actifs/grisés selon la sélection et l'état du jeu.
    /// </summary>
    public class FooterBuilder : IUGUIBuilder<FooterController>
    {
        private const string EditGlyph = "✎";    // ✎ (U+270E)
        private const string GoToGlyph = "➤";    // ➤ (U+27A4)
        private const string TargetGlyph = "◎";  // ◎ (U+25CE)

        // ================================================
        // Builder parameters
        // ================================================

        private BookmarksViewModel _viewModel;
        public FooterBuilder ViewModel(BookmarksViewModel viewModel)
        {
            this._viewModel = viewModel;
            return this;
        }

        // =====================================================
        // Build
        // =====================================================

        public FooterController Build()
        {
            var go = new GameObject("Bookmarks.Footer", typeof(RectTransform));
            
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
            edit.transform.SetParent(go.transform, false);
            Tooltips.Attach(edit.gameObject, ModLocalization.GetString("tooltipEdit"));

            ButtonController goTo = new VBMButtonBuilder()
                .ObjectName("GoTo")
                .Label(GoToGlyph)
                .Interactable(false)
                .Size(VesselBookmarkPalette.FooterButtonHeight)
                .FontSize(VesselBookmarkPalette.FooterButtonFontSize)
                .Build();
            goTo.transform.SetParent(go.transform, false);
            Tooltips.Attach(goTo.gameObject, ModLocalization.GetString("tooltipGoTo"));

            ButtonController target = new VBMButtonBuilder()
                .ObjectName("Target")
                .Label(TargetGlyph)
                .Interactable(false)
                .Size(VesselBookmarkPalette.FooterButtonHeight)
                .FontSize(VesselBookmarkPalette.FooterButtonFontSize)
                .Build();
            target.transform.SetParent(go.transform, false);
            Tooltips.Attach(target.gameObject, ModLocalization.GetString("tooltipSetTargetAs"));

            return go
                .AddComponent<FooterController>()
                .ViewModel(_viewModel)
                .SelectionLabel(selLabel)
                .Buttons(edit, goTo, target);
        }
    }
}
