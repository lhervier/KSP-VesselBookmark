using UnityEngine;
using UnityEngine.UI;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.sprites;
using com.github.lhervier.ksp.shared;
using com.github.lhervier.ksp.shared.ugui.sprites;
using com.github.lhervier.ksp.shared.ugui;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.body.list
{
    /// <summary>
    /// En-tête de section (titre + compteur) et texte d'aide associé, ajoutés à un conteneur.
    /// Sections : signets par module de commande / par vaisseau.
    /// </summary>
    public class SectionHintBuilder : IUGUIBuilder<SectionHintController>
    {
        // ================================================
        // Builder parameters
        // ================================================

        private Transform _parent;
        public SectionHintBuilder Parent(Transform parent)
        {
            this._parent = parent;
            return this;
        }

        private string _hintKey;
        public SectionHintBuilder HintKey(string titleKey)
        {
            this._hintKey = titleKey;
            return this;
        }
        
        // =======================================
        // Build
        // =======================================

        public SectionHintController Build()
        {
            var go = new GameObject("SectionHint", typeof(RectTransform));
            go.transform.SetParent(_parent, false);

            var image = go.AddComponent<Image>();
            image.sprite = SpritesGlobal.FillSprite;
            image.type = Image.Type.Simple;
            image.color = VesselBookmarkPalette.SectionHintBgColor;
            image.raycastTarget = false;

            var layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(
                Mathf.RoundToInt(VesselBookmarkPalette.SectionHintPaddingH),
                Mathf.RoundToInt(VesselBookmarkPalette.SectionHintPaddingH),
                Mathf.RoundToInt(VesselBookmarkPalette.SectionHintPaddingV),
                Mathf.RoundToInt(VesselBookmarkPalette.SectionHintPaddingV));
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var textGo = new GameObject("Text", typeof(RectTransform));
            textGo.transform.SetParent(go.transform, false);
            var textLe = textGo.AddComponent<LayoutElement>();
            textLe.flexibleWidth = 1f;
            var text = textGo.AddComponent<Text>();
            text.text = ModLocalization.GetString(_hintKey);
            text.font = HighLogic.UISkin.font;
            text.fontSize = VesselBookmarkPalette.SectionHintFontSize;
            text.fontStyle = FontStyle.Italic;
            text.color = VesselBookmarkPalette.SectionHintTextColor;
            text.alignment = TextAnchor.MiddleLeft;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;

            return go.AddComponent<SectionHintController>();
        }
    }
}
