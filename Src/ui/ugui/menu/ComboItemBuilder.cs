using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.sprites;
using com.github.lhervier.ksp.shared.ugui;
using UnityEngine;
using UnityEngine.UI;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.menu
{
    public class ComboItemBuilder : IUGUIBuilder<ComboItemController>
    {
        private Transform _parent;
        public ComboItemBuilder Parent(Transform parent)
        {
            _parent = parent;
            return this;
        }

        private string _id;
        public ComboItemBuilder Id(string id)
        {
            this._id = id;
            return this;
        }

        private bool _selected = false;
        public ComboItemBuilder Selected(bool selected)
        {
            _selected = selected;
            return this;
        }

        private string _label;
        public ComboItemBuilder Label(string label)
        {
            _label = label;
            return this;
        }

        public ComboItemController Build()
        {
            var itemGo = new GameObject("Item", typeof(RectTransform));
            itemGo.transform.SetParent(_parent, false);
            var le = itemGo.AddComponent<LayoutElement>();
            le.minHeight = le.preferredHeight = VesselBookmarkPalette.ComboHeight;

            var image = itemGo.AddComponent<Image>();
            image.sprite = Sprites.Fill;
            image.type = Image.Type.Simple;
            image.color = Color.white;
            image.raycastTarget = true;

            var button = itemGo.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.normalColor = _selected ? VesselBookmarkPalette.ComboItemSelectedBgColor : Color.clear;
            colors.highlightedColor = VesselBookmarkPalette.ComboItemHoverColor;
            colors.pressedColor = VesselBookmarkPalette.ComboItemHoverColor;
            colors.selectedColor = _selected ? VesselBookmarkPalette.ComboItemSelectedBgColor : Color.clear;
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.1f;
            button.colors = colors;

            var layout = itemGo.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(Mathf.RoundToInt(VesselBookmarkPalette.ComboPaddingH), Mathf.RoundToInt(VesselBookmarkPalette.ComboPaddingH), 0, 0);
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            var labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(itemGo.transform, false);
            var label = labelGo.AddComponent<Text>();
            label.text = _label;
            label.font = HighLogic.UISkin.font;
            label.fontSize = VesselBookmarkPalette.ComboFontSize;
            label.color = _selected ? VesselBookmarkPalette.ComboItemSelectedColor : VesselBookmarkPalette.ComboItemColor;
            label.alignment = TextAnchor.MiddleLeft;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.raycastTarget = false;

            return itemGo
                .AddComponent<ComboItemController>()
                .Id(_id)
                .Button(button);
        }
    }
}