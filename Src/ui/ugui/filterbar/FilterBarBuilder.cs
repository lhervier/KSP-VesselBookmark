using UnityEngine;
using UnityEngine.UI;
using TMPro;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.shared;
using com.github.lhervier.ksp.shared.ugui;
using com.github.lhervier.ksp.shared.ugui.sprites;
using com.github.lhervier.ksp.shared.ugui.styles;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.filterbar
{
    /// <summary>
    /// Barre de rappel des critères de recherche actifs, entre le title bar et la liste : une pastille
    /// par critère (qui passent à la ligne si besoin) et une action « tout effacer » à droite. La barre
    /// entière disparaît quand plus rien n'est filtré — l'usage courant ne paye donc aucune place.
    ///
    /// La racine reste toujours active (son controller doit tourner pour la réafficher) ; c'est le
    /// panneau qu'on masque, et un layout vertical sans marge fait alors retomber la hauteur à zéro.
    /// </summary>
    public class FilterBarBuilder : IUGUIBuilder<FilterBarController>
    {
        private static string ClearGlyph => DefaultPalette.PickGlyph("×", "✕", "✗", "x");

        // ===========================================
        // Builder parameters
        // ===========================================

        private BookmarksViewModel _viewModel;
        public FilterBarBuilder WithViewModel(BookmarksViewModel viewModel)
        {
            this._viewModel = viewModel;
            return this;
        }

        // ==========================================
        // Build
        // ==========================================

        public FilterBarController Build()
        {
            var rootGo = new GameObject("Bookmarks.FilterBar", typeof(RectTransform));

            var rootLayout = rootGo.AddComponent<VerticalLayoutGroup>();
            rootLayout.padding = new RectOffset(0, 0, 0, 0);
            rootLayout.spacing = 0f;
            rootLayout.childAlignment = TextAnchor.UpperLeft;
            rootLayout.childControlWidth = true;
            rootLayout.childControlHeight = true;
            rootLayout.childForceExpandWidth = true;
            rootLayout.childForceExpandHeight = false;

            // Panneau (masqué quand aucun critère n'est actif)
            var panelGo = new GameObject("Panel", typeof(RectTransform));
            panelGo.transform.SetParent(rootGo.transform, false);

            var panelBg = panelGo.AddComponent<Image>();
            panelBg.sprite = SpritesGlobal.FillSprite;
            panelBg.type = Image.Type.Simple;
            panelBg.color = VesselBookmarkPalette.FilterBarBgColor;
            panelBg.raycastTarget = true;   // absorbe les clics sur les zones vides de la barre

            var panelLayout = panelGo.AddComponent<HorizontalLayoutGroup>();
            panelLayout.padding = new RectOffset(
                Mathf.RoundToInt(VesselBookmarkPalette.FilterBarPaddingH),
                Mathf.RoundToInt(VesselBookmarkPalette.FilterBarPaddingH),
                Mathf.RoundToInt(VesselBookmarkPalette.FilterBarPaddingV),
                Mathf.RoundToInt(VesselBookmarkPalette.FilterBarPaddingV));
            panelLayout.spacing = VesselBookmarkPalette.FilterBarSpacingH;
            panelLayout.childAlignment = TextAnchor.UpperLeft;
            panelLayout.childControlWidth = true;
            panelLayout.childControlHeight = true;
            panelLayout.childForceExpandWidth = false;
            panelLayout.childForceExpandHeight = false;

            BuildBottomSeparator(panelGo.transform);

            // Zone des pastilles : prend la largeur restante, les pastilles y coulent en passant à la
            // ligne. « Tout effacer » reste hors du flot pour rester collé à droite.
            var chipsGo = new GameObject("Chips", typeof(RectTransform));
            chipsGo.transform.SetParent(panelGo.transform, false);
            var chipsLe = chipsGo.AddComponent<LayoutElement>();
            chipsLe.flexibleWidth = 1f;
            var flow = chipsGo.AddComponent<FlowLayoutGroup>();
            flow.padding = new RectOffset(0, 0, 0, 0);
            flow.SpacingX = VesselBookmarkPalette.FilterBarSpacingH;
            flow.SpacingY = VesselBookmarkPalette.FilterBarSpacingV;
            flow.childAlignment = TextAnchor.UpperLeft;

            BuildClearAction(panelGo.transform);

            return rootGo
                .AddComponent<FilterBarController>()
                .WithViewModel(_viewModel)
                .WithPanel(panelGo)
                .WithChipsContainer(chipsGo.transform);
        }

        // ----------------------------------------------------------------
        //  Sous-éléments
        // ----------------------------------------------------------------

        // Trait de séparation 1px avec la liste, superposé en bas du panneau (hors layout).
        private static void BuildBottomSeparator(Transform parent)
        {
            var go = new GameObject("Separator", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>();
            le.ignoreLayout = true;
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(0f, VesselBookmarkPalette.FilterBarSeparatorHeight);
            rect.anchoredPosition = Vector2.zero;
            var image = go.AddComponent<Image>();
            image.sprite = SpritesGlobal.FillSprite;
            image.type = Image.Type.Simple;
            image.color = VesselBookmarkPalette.FilterBarSeparatorColor;
            image.raycastTarget = false;
        }

        // « ✕ Tout effacer » : raccourci vers la réinitialisation, sans rouvrir le menu.
        private void BuildClearAction(Transform parent)
        {
            var go = new GameObject("ClearAll", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>();
            le.minHeight = le.preferredHeight = VesselBookmarkPalette.FilterChipHeight;

            var image = go.AddComponent<Image>();
            image.sprite = SpritesGlobal.FillSprite;
            image.type = Image.Type.Simple;
            image.color = Color.white;
            image.raycastTarget = true;

            var button = go.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.normalColor = Color.clear;
            colors.highlightedColor = VesselBookmarkPalette.FilterClearHoverColor;
            colors.pressedColor = VesselBookmarkPalette.FilterClearHoverColor;
            colors.selectedColor = Color.clear;
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.1f;
            button.colors = colors;
            button.onClick.AddListener(() => _viewModel.ClearFilters());

            var layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(
                Mathf.RoundToInt(VesselBookmarkPalette.FilterClearPaddingH),
                Mathf.RoundToInt(VesselBookmarkPalette.FilterClearPaddingH), 0, 0);
            layout.spacing = VesselBookmarkPalette.FilterChipSpacing;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            var glyphGo = new GameObject("Glyph", typeof(RectTransform));
            glyphGo.transform.SetParent(go.transform, false);
            var glyph = UGUILabels.AddLabel(glyphGo);
            glyph.text = ClearGlyph;
            glyph.fontSize = VesselBookmarkPalette.FilterClearFontSize;
            glyph.color = VesselBookmarkPalette.FilterClearColor;
            glyph.alignment = TextAlignmentOptions.Center;

            var labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(go.transform, false);
            var label = UGUILabels.AddLabel(labelGo);
            label.text = ModLocalization.GetString("filterClearAll").ToUpperInvariant();
            label.fontSize = VesselBookmarkPalette.FilterClearFontSize;
            label.color = VesselBookmarkPalette.FilterClearColor;
            label.alignment = TextAlignmentOptions.Left;

            Tooltips.Attach(go, ModLocalization.GetString("menuResetFilters"));
        }
    }
}
