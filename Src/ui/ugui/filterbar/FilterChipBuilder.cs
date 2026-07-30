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
    /// One criterion chip of the filter bar: an accent-bordered box holding "KEY value" on the left and
    /// a ✕ on the right. Clicking the left part asks to change the criterion (the filter menu opens on
    /// the matching control), clicking the ✕ drops that criterion alone.
    ///
    /// Rebuilt from scratch on every filter change, so it holds no state and needs no controller.
    /// </summary>
    public class FilterChipBuilder
    {
        // "×" (U+00D7) is baked in the game font, unlike the ballot X variants which only come from a
        // fallback and render illegibly at this size. Same choice as the filter menu's reset action.
        private static string RemoveGlyph => DefaultPalette.PickGlyph("×", "✕", "✗", "x");

        // ===========================================
        // Builder parameters
        // ===========================================

        private BookmarksViewModel _viewModel;
        public FilterChipBuilder WithViewModel(BookmarksViewModel viewModel)
        {
            this._viewModel = viewModel;
            return this;
        }

        private FilterCriterion _criterion;
        public FilterChipBuilder WithCriterion(FilterCriterion criterion)
        {
            this._criterion = criterion;
            return this;
        }

        private Transform _parent;
        public FilterChipBuilder WithParent(Transform parent)
        {
            this._parent = parent;
            return this;
        }

        // ==========================================
        // Build
        // ==========================================

        public GameObject Build()
        {
            var chipGo = new GameObject("Chip." + _criterion.Id, typeof(RectTransform));
            chipGo.transform.SetParent(_parent, false);

            var le = chipGo.AddComponent<LayoutElement>();
            le.minHeight = le.preferredHeight = VesselBookmarkPalette.FilterChipHeight;

            var background = chipGo.AddComponent<Image>();
            background.sprite = SpritesGlobal.Border(
                DefaultPalette.AccentBgColor,
                DefaultPalette.AccentBorderColor,
                VesselBookmarkPalette.FilterChipBorderThickness);
            background.type = Image.Type.Sliced;
            background.color = Color.white;
            background.raycastTarget = false;

            var layout = chipGo.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.spacing = 0f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;

            BuildLabelZone(chipGo.transform);
            BuildSeparator(chipGo.transform);
            BuildRemoveZone(chipGo.transform);

            return chipGo;
        }

        // ----------------------------------------------------------------
        //  Sub-elements
        // ----------------------------------------------------------------

        // "KEY value", clickable to reopen the menu on that criterion.
        private void BuildLabelZone(Transform parent)
        {
            GameObject zoneGo = NewClickableZone(parent, "Label", VesselBookmarkPalette.FilterChipHoverColor,
                () => _viewModel.RequestFilterEdition(_criterion.Id));

            var layout = zoneGo.GetComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(
                Mathf.RoundToInt(VesselBookmarkPalette.FilterChipPaddingH),
                Mathf.RoundToInt(VesselBookmarkPalette.FilterChipPaddingH), 0, 0);
            layout.spacing = VesselBookmarkPalette.FilterChipSpacing;
            layout.childAlignment = TextAnchor.MiddleLeft;

            AddText(zoneGo.transform, "Key", _criterion.Key.ToUpperInvariant(),
                VesselBookmarkPalette.FilterChipKeyFontSize, VesselBookmarkPalette.FilterChipKeyColor);

            // The value is what gives up room first: a chip clamped to the bar width ellipsizes it
            // rather than pushing the ✕ out of the window.
            TextMeshProUGUI value = AddText(zoneGo.transform, "Value", _criterion.Value,
                VesselBookmarkPalette.FilterChipValueFontSize, VesselBookmarkPalette.FilterChipValueColor);
            value.overflowMode = TextOverflowModes.Ellipsis;
            var valueLe = value.gameObject.AddComponent<LayoutElement>();
            valueLe.flexibleWidth = 1f;

            Tooltips.Attach(zoneGo, ModLocalization.GetString("filterTooltipEdit"));
        }

        private void BuildSeparator(Transform parent)
        {
            var go = new GameObject("Separator", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>();
            le.minWidth = le.preferredWidth = 1f;
            var image = go.AddComponent<Image>();
            image.sprite = SpritesGlobal.FillSprite;
            image.type = Image.Type.Simple;
            image.color = DefaultPalette.AccentBorderColor;
            image.raycastTarget = false;
        }

        // The ✕, clickable to drop this criterion alone.
        private void BuildRemoveZone(Transform parent)
        {
            GameObject zoneGo = NewClickableZone(parent, "Remove", VesselBookmarkPalette.FilterChipRemoveHoverColor,
                () => _viewModel.ClearFilter(_criterion.Id));

            var le = zoneGo.AddComponent<LayoutElement>();
            le.minWidth = le.preferredWidth = VesselBookmarkPalette.FilterChipRemoveWidth;

            var layout = zoneGo.GetComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;

            TextMeshProUGUI glyph = AddText(zoneGo.transform, "Glyph", RemoveGlyph,
                VesselBookmarkPalette.FilterChipValueFontSize, VesselBookmarkPalette.FilterChipKeyColor);
            glyph.alignment = TextAlignmentOptions.Center;

            Tooltips.Attach(zoneGo, ModLocalization.GetString("filterTooltipRemove"));
        }

        /// <summary>
        /// A transparent clickable area tinted on hover, laid out horizontally. Same recipe as the
        /// filter menu's reset action: a Button whose normal color is clear, so only the hover shows.
        /// </summary>
        private static GameObject NewClickableZone(Transform parent, string objectName, Color hoverColor, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject(objectName, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var image = go.AddComponent<Image>();
            image.sprite = SpritesGlobal.FillSprite;
            image.type = Image.Type.Simple;
            image.color = Color.white;
            image.raycastTarget = true;

            var button = go.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.normalColor = Color.clear;
            colors.highlightedColor = hoverColor;
            colors.pressedColor = hoverColor;
            colors.selectedColor = Color.clear;
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.1f;
            button.colors = colors;
            button.onClick.AddListener(onClick);

            var layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.spacing = 0f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            return go;
        }

        private static TextMeshProUGUI AddText(Transform parent, string objectName, string text, int fontSize, Color color)
        {
            var go = new GameObject(objectName, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var label = UGUILabels.AddLabel(go);
            label.text = text;
            label.fontSize = fontSize;
            label.color = color;
            label.alignment = TextAlignmentOptions.Left;
            return label;
        }
    }
}
