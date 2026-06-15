using UnityEngine;
using UnityEngine.UI;
using TMPro;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.shared;
using com.github.lhervier.ksp.shared.ugui.sprites;
using com.github.lhervier.ksp.shared.ugui;
using com.github.lhervier.ksp.shared.ugui.styles;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.body.list
{
    /// <summary>
    /// En-tête de section (titre + compteur) et texte d'aide associé, ajoutés à un conteneur.
    /// Sections : signets par module de commande / par vaisseau.
    /// </summary>
    public class SectionHeaderBuilder : IUGUIBuilder<SectionHeaderController>
    {
        // ================================================
        // Builder parameters
        // ================================================

        private Transform _parent;
        public SectionHeaderBuilder WithParent(Transform parent)
        {
            this._parent = parent;
            return this;
        }

        private string _titleKey;
        public SectionHeaderBuilder WithTitleKey(string titleKey)
        {
            this._titleKey = titleKey;
            return this;
        }

        private int _count;
        public SectionHeaderBuilder WithCount(int count)
        {
            this._count = count;
            return this;
        }

        // =====================================================
        // Build
        // =====================================================

        public SectionHeaderController Build()
        {
            var go = new GameObject("SectionHeader", typeof(RectTransform));
            go.transform.SetParent(_parent, false);

            var le = go.AddComponent<LayoutElement>();
            le.minHeight = le.preferredHeight = VesselBookmarkPalette.SectionHeaderHeight;

            var image = go.AddComponent<Image>();
            image.sprite = SpritesGlobal.HorizontalBorders(
                VesselBookmarkPalette.SectionHeaderBgColor,
                VesselBookmarkPalette.SectionHeaderBorderColor,
                VesselBookmarkPalette.SectionHeaderBorderThickness);
            image.type = Image.Type.Sliced;
            image.color = Color.white;
            image.raycastTarget = false;

            var layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(
                Mathf.RoundToInt(VesselBookmarkPalette.RowPaddingH),
                Mathf.RoundToInt(VesselBookmarkPalette.RowPaddingH),
                0, 0);
            layout.spacing = DefaultPalette.Spacing;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            var nameGo = new GameObject("Name", typeof(RectTransform));
            nameGo.transform.SetParent(go.transform, false);
            var nameLe = nameGo.AddComponent<LayoutElement>();
            nameLe.flexibleWidth = 1f;
            var name = UGUILabels.AddLabel(nameGo);
            name.text = ModLocalization.GetString(_titleKey).ToUpperInvariant();
            name.fontSize = VesselBookmarkPalette.SectionNameFontSize;
            name.fontStyle = FontStyles.Bold;
            name.color = VesselBookmarkPalette.SectionNameColor;
            name.alignment = TextAlignmentOptions.Left;

            var countGo = new GameObject("Count", typeof(RectTransform));
            countGo.transform.SetParent(go.transform, false);
            var count2 = UGUILabels.AddLabel(countGo);
            count2.text = _count.ToString();
            count2.fontSize = VesselBookmarkPalette.SectionCountFontSize;
            count2.color = VesselBookmarkPalette.SectionCountColor;
            count2.alignment = TextAlignmentOptions.Right;

            return go.AddComponent<SectionHeaderController>();
        }
    }
}
