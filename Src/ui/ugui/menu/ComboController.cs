using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.sprites;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.menu
{
    public class ComboController : MonoBehaviour
    {
        private Text _value;
        private RectTransform _headerRect;
        private GameObject _dropdown;
        private RectTransform _dropdownRect;
        private RectTransform _content;
        private GameObject _trap;
        private readonly List<GameObject> _items = new List<GameObject>();

        /// <summary>Callback invoqué quand l'utilisateur choisit une option (reçoit la VALEUR brute).</summary>
        public Action<string> OnSelect;

        /// <summary>Invoqué juste avant l'ouverture (ex. fermer les autres combos).</summary>
        public Action OnBeforeOpen;

        /// <summary>
        /// Conversion valeur → libellé affiché (ex. type de vaisseau brut → libellé traduit).
        /// Null = identité (le libellé affiché est la valeur).
        /// </summary>
        public Func<string, string> LabelFor;

        private string Label(string value)
        {
            return LabelFor != null ? LabelFor(value) : (value ?? string.Empty);
        }

        public void Bind(Text value, RectTransform headerRect, GameObject dropdown, RectTransform content, GameObject trap)
        {
            this._value = value;
            this._headerRect = headerRect;
            this._dropdown = dropdown;
            this._dropdownRect = dropdown.GetComponent<RectTransform>();
            this._content = content;
            this._trap = trap;
        }

        public void SetOptions(IReadOnlyList<string> options, string current)
        {
            if (_value != null) _value.text = Label(current);

            foreach (var item in _items) Destroy(item);
            _items.Clear();
            if (options == null) return;

            foreach (string opt in options)
            {
                _items.Add(BuildItem(opt, opt == current));
            }
        }

        public void Toggle()
        {
            if (_dropdown == null) return;
            if (_dropdown.activeSelf) Collapse();
            else Open();
        }

        public void Open()
        {
            if (_dropdown == null) return;
            OnBeforeOpen?.Invoke();

            // Le piège passe au premier plan, puis le dropdown par-dessus le piège.
            if (_trap != null) { _trap.SetActive(true); _trap.transform.SetAsLastSibling(); }
            _dropdown.SetActive(true);
            _dropdown.transform.SetAsLastSibling();   // au premier plan, par-dessus le contenu du menu

            // Hauteur = contenu, plafonnée (au-delà → scroll) ; largeur = celle de l'en-tête.
            LayoutRebuilder.ForceRebuildLayoutImmediate(_content);
            float height = Mathf.Min(LayoutUtility.GetPreferredHeight(_content), VesselBookmarkPalette.ComboDropdownMaxHeight);

            var corners = new Vector3[4];
            _headerRect.GetWorldCorners(corners);   // 0 = bas-gauche
            _dropdownRect.anchorMin = new Vector2(0f, 1f);
            _dropdownRect.anchorMax = new Vector2(0f, 1f);
            _dropdownRect.pivot = new Vector2(0f, 1f);
            _dropdownRect.sizeDelta = new Vector2(_headerRect.rect.width, height);
            _dropdownRect.position = corners[0];
        }

        public void Collapse()
        {
            if (_dropdown != null) _dropdown.SetActive(false);
            if (_trap != null) _trap.SetActive(false);
        }

        private GameObject BuildItem(string text, bool selected)
        {
            var itemGo = new GameObject("Item", typeof(RectTransform));
            itemGo.transform.SetParent(_content, false);
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
            colors.normalColor = selected ? VesselBookmarkPalette.ComboItemSelectedBgColor : Color.clear;
            colors.highlightedColor = VesselBookmarkPalette.ComboItemHoverColor;
            colors.pressedColor = VesselBookmarkPalette.ComboItemHoverColor;
            colors.selectedColor = selected ? VesselBookmarkPalette.ComboItemSelectedBgColor : Color.clear;
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.1f;
            button.colors = colors;
            button.onClick.AddListener(() => { OnSelect?.Invoke(text); Collapse(); });

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
            label.text = Label(text);
            label.font = HighLogic.UISkin.font;
            label.fontSize = VesselBookmarkPalette.ComboFontSize;
            label.color = selected ? VesselBookmarkPalette.ComboItemSelectedColor : VesselBookmarkPalette.ComboItemColor;
            label.alignment = TextAnchor.MiddleLeft;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.raycastTarget = false;

            return itemGo;
        }
    }
}
