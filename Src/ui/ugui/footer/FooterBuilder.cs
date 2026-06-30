using UnityEngine;
using UnityEngine.UI;
using TMPro;
using com.github.lhervier.ksp.shared.ugui.button;
using com.github.lhervier.ksp.bookmarksmod.bookmarks;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.sprites;
using com.github.lhervier.ksp.shared;
using com.github.lhervier.ksp.shared.ugui;
using com.github.lhervier.ksp.shared.ugui.sprites;
using com.github.lhervier.ksp.shared.ugui.styles;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.footer
{
    /// <summary>
    /// Barre d'actions du bas, fixée sous le corps : libellé de sélection à gauche, puis les boutons
    /// Éditer / Aller au vaisseau / Définir comme cible, actifs/grisés selon la sélection et l'état du jeu.
    /// </summary>
    public class FooterBuilder : IUGUIBuilder<FooterController>
    {
        // Action icons. The game SDF font does not render these glyphs reliably, so each prefers a
        // dedicated sprite (registered at startup), falling back to a text glyph when the texture is
        // missing.
        private static string EditLabel => SpriteOrGlyph("edit", "✎", "✏", "E");
        private static string GoToLabel => SpriteOrGlyph("goto", "➤", "►", "▶", "→", ">");
        private static string TargetLabel => SpriteOrGlyph("target", "◎", "◉", "⊙", "○", "o");

        private static string SpriteOrGlyph(string spriteName, params string[] glyphs)
        {
            return SpritesIcons.HasSprite(spriteName)
                ? "<sprite name=\"" + spriteName + "\" tint=1>"
                : DefaultPalette.PickGlyph(glyphs);
        }

        // ================================================
        // Builder parameters
        // ================================================

        private BookmarksViewModel _viewModel;
        public FooterBuilder WithViewModel(BookmarksViewModel viewModel)
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

            // Fixed-height child of the content's vertical layout: the layout stacks it under the
            // scrollable body and pins it at the bottom. Width is driven by the parent layout.
            var layoutElement = go.AddComponent<LayoutElement>();
            layoutElement.minHeight = layoutElement.preferredHeight = VesselBookmarkPalette.FooterHeight;

            // Fond + séparateur 1px en haut
            var image = go.AddComponent<Image>();
            image.sprite = SpritesGlobal.HorizontalBorders(
                VesselBookmarkPalette.FooterBgColor,
                VesselBookmarkPalette.FooterBorderColor,
                VesselBookmarkPalette.FooterBorderThickness);
            image.type = Image.Type.Sliced;
            image.color = Color.white;
            image.raycastTarget = true;

            var layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(
                Mathf.RoundToInt(VesselBookmarkPalette.FooterPaddingH),
                // Extra right padding = scrollbar width: the footer spans the full width while the
                // scrollable list above reserves that strip for its scrollbar, so this keeps the
                // footer buttons aligned with the list content instead of overhanging the scrollbar.
                Mathf.RoundToInt(VesselBookmarkPalette.FooterPaddingH + VesselBookmarkPalette.ScrollbarWidth),
                Mathf.RoundToInt(VesselBookmarkPalette.FooterPaddingV),
                Mathf.RoundToInt(VesselBookmarkPalette.FooterPaddingV));
            layout.spacing = VesselBookmarkPalette.FooterSpacing;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            // Libellé de sélection : occupe l'espace restant après les boutons, sans jamais les pousser.
            // min/preferredWidth = 0 (sinon le label TMP réclamerait la largeur de son texte, qui peut
            // dépasser la place dispo et écraser/pousser les boutons à taille fixe) ; flexibleWidth = 1
            // pour s'étendre dans le reste ; ellipsis pour tronquer un nom trop long au lieu de déborder.
            var selGo = new GameObject("Selection", typeof(RectTransform));
            selGo.transform.SetParent(go.transform, false);
            var selLe = selGo.AddComponent<LayoutElement>();
            selLe.minWidth = 0f;
            selLe.preferredWidth = 0f;
            selLe.flexibleWidth = 1f;
            var selLabel = UGUILabels.AddLabel(selGo);
            selLabel.fontSize = VesselBookmarkPalette.FooterSelFontSize;
            selLabel.fontStyle = FontStyles.Italic;
            selLabel.color = VesselBookmarkPalette.FooterSelColor;
            selLabel.alignment = TextAlignmentOptions.Left;
            selLabel.overflowMode = TextOverflowModes.Ellipsis;
            
            // Mockup glyphs: ✎ edit, ➤ go to, ◎ target (square buttons, like the title bar).
            // Background/hover colors come from the VBMButtonBuilder defaults; only the footer-specific
            // size and font size are overridden here.
            ButtonController edit = new VBMButtonBuilder()
                .WithObjectName("Edit")
                .WithLabel(EditLabel)
                .WithInteractableState(false)
                .WithSize(VesselBookmarkPalette.FooterButtonHeight)
                .WithFontSize(VesselBookmarkPalette.FooterButtonFontSize)
                .Build();
            edit.transform.SetParent(go.transform, false);
            Tooltips.Attach(edit.gameObject, ModLocalization.GetString("VBM_tooltipEdit"));

            ButtonController goTo = new VBMButtonBuilder()
                .WithObjectName("GoTo")
                .WithLabel(GoToLabel)
                .WithInteractableState(false)
                .WithSize(VesselBookmarkPalette.FooterButtonHeight)
                .WithFontSize(VesselBookmarkPalette.FooterButtonFontSize)
                .Build();
            goTo.transform.SetParent(go.transform, false);
            Tooltips.Attach(goTo.gameObject, ModLocalization.GetString("VBM_tooltipGoTo"));

            ButtonController target = new VBMButtonBuilder()
                .WithObjectName("Target")
                .WithLabel(TargetLabel)
                .WithInteractableState(false)
                .WithSize(VesselBookmarkPalette.FooterButtonHeight)
                .WithFontSize(VesselBookmarkPalette.FooterButtonFontSize)
                .Build();
            target.transform.SetParent(go.transform, false);
            Tooltips.Attach(target.gameObject, ModLocalization.GetString("VBM_tooltipSetTargetAs"));

            return go
                .AddComponent<FooterController>()
                .WithViewModel(_viewModel)
                .WithSelectionLabelComponent(selLabel)
                .WithButtonControllers(edit, goTo, target);
        }
    }
}
