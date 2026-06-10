using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.sprites;
using com.github.lhervier.ksp.shared.ugui.overlay;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.menu
{
    public class ComboController : MonoBehaviour
    {
        private readonly List<GameObject> _items = new List<GameObject>();

        /// <summary>Callback invoqué quand l'utilisateur choisit une option (reçoit la VALEUR brute).</summary>
        public EventData<string> OnSelect = new EventData<string>("Combobox.OnSelect");

        // =======================================
        // Life cycle
        // =======================================

        private Text _value;
        public ComboController Value(Text value)
        {
            _value = value;
            return this;
        }

        private RectTransform _headerRect;
        public ComboController HeaderRect(RectTransform headerRect)
        {
            _headerRect = headerRect;
            return this;
        }

        private GameObject _dropdown;
        private RectTransform _dropdownRect;
        private RectTransform _content;
        public ComboController DropDown(GameObject dropDown, RectTransform content)
        {
            this._dropdown = dropDown;
            this._dropdownRect = _dropdown.GetComponent<RectTransform>();
            this._content = content;
            return this;
        }

        private Button _button;
        public ComboController Button(Button button)
        {
            _button = button;
            return this;
        }

        private OverlayController _overlayController;
        public ComboController OverlayController(OverlayController overlay)
        {
            _overlayController = overlay;
            return this;
        }

        
        private Func<string, string> _labelFor;
        public ComboController LabelFor(Func<string, string> labelFor)
        {
            this._labelFor = labelFor;
            return this;
        }

        public void Start()
        {
            // A click on the overlay (anywhere outside the dropdown) collapses the combo.
            if (_overlayController != null) {
                _overlayController.OnClose.Add(Collapse);
            }

            if( _button != null )
            {
                _button.onClick.AddListener(Toggle);
            }
        }

        public void OnDestroy()
        {
            
            if( _button != null )
            {
                _button.onClick.RemoveListener(Toggle);
            }
            if (_overlayController != null) {
                _overlayController.OnClose.Remove(Collapse);
            }
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

            // Le piège passe au premier plan, puis le dropdown par-dessus le piège.
            if (_overlayController != null) { _overlayController.gameObject.SetActive(true); _overlayController.transform.SetAsLastSibling(); }
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
            if (_overlayController != null) _overlayController.gameObject.SetActive(false);
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
            button.onClick.AddListener(() => { OnSelect?.Fire(text); Collapse(); });

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

        // =============================================
        // Helpers
        // =============================================

        private string Label(string value)
        {
            return _labelFor != null ? _labelFor(value) : (value ?? string.Empty);
        }
    }
}
