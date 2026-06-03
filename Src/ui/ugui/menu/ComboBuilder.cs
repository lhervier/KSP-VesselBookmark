using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.sprites;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.menu
{
    /// <summary>
    /// Combo réutilisable : une ligne [libellé][valeur ▼] et un dropdown qui FLOTTE par-dessus le
    /// contenu en dessous (positionné en absolu sous l'en-tête, passé au premier plan via
    /// SetAsLastSibling, hauteur plafonnée + ScrollRect si la liste est longue). Combo « bête » :
    /// l'appelant fournit les options via SetOptions(...) et reçoit le choix via OnSelect.
    /// </summary>
    public class ComboBuilder
    {
        private const string CaretGlyph = "▼";   // ▼ (U+25BC) — glyphe confirmé rendu par la police UISkin

        public ComboController Create(Transform parent, string labelText)
        {
            var rootGo = new GameObject("Combo", typeof(RectTransform));
            rootGo.transform.SetParent(parent, false);
            ComboController controller = rootGo.AddComponent<ComboController>();

            var rowLayout = rootGo.AddComponent<HorizontalLayoutGroup>();
            rowLayout.padding = new RectOffset(0, 0, 0, 0);
            rowLayout.spacing = VesselBookmarkPalette.DefaultSpacing;
            rowLayout.childAlignment = TextAnchor.MiddleLeft;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = false;

            var labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(rootGo.transform, false);
            var labelLe = labelGo.AddComponent<LayoutElement>();
            labelLe.minWidth = labelLe.preferredWidth = 46f;
            var label = labelGo.AddComponent<Text>();
            label.text = labelText;
            label.font = HighLogic.UISkin.font;
            label.fontSize = VesselBookmarkPalette.MenuLabelFontSize;
            label.color = VesselBookmarkPalette.MenuLabelColor;
            label.alignment = TextAnchor.MiddleLeft;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.raycastTarget = false;

            // En-tête : bouton plein largeur [valeur (flexible)] [caret]
            var headerGo = new GameObject("Header", typeof(RectTransform));
            headerGo.transform.SetParent(rootGo.transform, false);
            var headerLe = headerGo.AddComponent<LayoutElement>();
            headerLe.flexibleWidth = 1f;
            headerLe.minHeight = headerLe.preferredHeight = VesselBookmarkPalette.ComboHeight;
            var headerRect = headerGo.GetComponent<RectTransform>();

            var headerImage = headerGo.AddComponent<Image>();
            headerImage.sprite = Sprites.Border(VesselBookmarkPalette.ComboBgColor, VesselBookmarkPalette.ComboBorderColor, 1);
            headerImage.type = Image.Type.Sliced;
            headerImage.color = Color.white;
            headerImage.raycastTarget = true;

            var headerBtn = headerGo.AddComponent<Button>();
            headerBtn.targetGraphic = headerImage;
            var hColors = headerBtn.colors;
            hColors.normalColor = VesselBookmarkPalette.ComboBgColor;
            hColors.highlightedColor = VesselBookmarkPalette.ComboHoverColor;
            hColors.pressedColor = VesselBookmarkPalette.ComboHoverColor;
            hColors.selectedColor = VesselBookmarkPalette.ComboBgColor;
            hColors.colorMultiplier = 1f;
            hColors.fadeDuration = 0.1f;
            headerBtn.colors = hColors;
            headerBtn.onClick.AddListener(() => controller.Toggle());

            var headerLayout = headerGo.AddComponent<HorizontalLayoutGroup>();
            headerLayout.padding = new RectOffset(Mathf.RoundToInt(VesselBookmarkPalette.ComboPaddingH), Mathf.RoundToInt(VesselBookmarkPalette.ComboPaddingH), 0, 0);
            headerLayout.spacing = VesselBookmarkPalette.DefaultSpacing;
            headerLayout.childAlignment = TextAnchor.MiddleLeft;
            headerLayout.childControlWidth = true;
            headerLayout.childControlHeight = true;
            headerLayout.childForceExpandWidth = false;
            headerLayout.childForceExpandHeight = true;

            var valueGo = new GameObject("Value", typeof(RectTransform));
            valueGo.transform.SetParent(headerGo.transform, false);
            var valueLe = valueGo.AddComponent<LayoutElement>();
            valueLe.flexibleWidth = 1f;
            var value = valueGo.AddComponent<Text>();
            value.font = HighLogic.UISkin.font;
            value.fontSize = VesselBookmarkPalette.ComboFontSize;
            value.color = VesselBookmarkPalette.ComboTextColor;
            value.alignment = TextAnchor.MiddleLeft;
            value.horizontalOverflow = HorizontalWrapMode.Overflow;
            value.verticalOverflow = VerticalWrapMode.Overflow;
            value.raycastTarget = false;

            var caretGo = new GameObject("Caret", typeof(RectTransform));
            caretGo.transform.SetParent(headerGo.transform, false);
            var caret = caretGo.AddComponent<Text>();
            caret.text = CaretGlyph;
            caret.font = HighLogic.UISkin.font;
            caret.fontSize = VesselBookmarkPalette.ComboCaretFontSize;
            caret.color = VesselBookmarkPalette.ComboCaretColor;
            caret.alignment = TextAnchor.MiddleCenter;
            caret.horizontalOverflow = HorizontalWrapMode.Overflow;
            caret.verticalOverflow = VerticalWrapMode.Overflow;
            caret.raycastTarget = false;

            // Dropdown flottant : enfant du MÊME parent (le panneau du menu), positionné en absolu
            // sous l'en-tête à l'ouverture. ignoreLayout pour ne pas occuper de place dans le menu.
            RectTransform content;
            GameObject dropdown = BuildDropdown(parent, out content);

            // Piège à clic propre au combo : referme le dropdown dès qu'on clique en dehors.
            GameObject trap = BuildTrap(parent, controller);

            controller.Bind(value, headerRect, dropdown, content, trap);
            return controller;
        }

        // Piège à clic plein écran (transparent), placé juste sous le dropdown à l'ouverture.
        private GameObject BuildTrap(Transform parent, ComboController controller)
        {
            var go = new GameObject("ComboTrap", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>();
            le.ignoreLayout = true;
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(4000f, 4000f);   // couvre tout l'écran quelle que soit la position du menu
            rect.anchoredPosition = Vector2.zero;
            var img = go.AddComponent<Image>();
            img.sprite = Sprites.Fill;
            img.type = Image.Type.Simple;
            img.color = Color.clear;
            img.raycastTarget = true;
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.transition = Selectable.Transition.None;
            btn.onClick.AddListener(() => controller.Collapse());
            go.SetActive(false);
            return go;
        }

        // Panneau scrollable du dropdown (construit détaché et masqué ; positionné/affiché à l'ouverture).
        private GameObject BuildDropdown(Transform parent, out RectTransform content)
        {
            var dropdownGo = new GameObject("ComboDropdown", typeof(RectTransform));
            dropdownGo.transform.SetParent(parent, false);

            var le = dropdownGo.AddComponent<LayoutElement>();
            le.ignoreLayout = true;

            var image = dropdownGo.AddComponent<Image>();
            image.sprite = Sprites.Border(VesselBookmarkPalette.MenuBgColor, VesselBookmarkPalette.ComboBorderColor, 1);
            image.type = Image.Type.Sliced;
            image.color = Color.white;
            image.raycastTarget = true;

            var scrollRect = dropdownGo.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 18f;

            var viewportGo = new GameObject("Viewport", typeof(RectTransform));
            viewportGo.transform.SetParent(dropdownGo.transform, false);
            var viewportRect = viewportGo.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            viewportGo.AddComponent<RectMask2D>();
            var viewportImage = viewportGo.AddComponent<Image>();
            viewportImage.sprite = Sprites.Fill;
            viewportImage.type = Image.Type.Simple;
            viewportImage.color = Color.clear;
            viewportImage.raycastTarget = true;
            scrollRect.viewport = viewportRect;

            var contentGo = new GameObject("Content", typeof(RectTransform));
            contentGo.transform.SetParent(viewportGo.transform, false);
            content = contentGo.GetComponent<RectTransform>();
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = Vector2.zero;
            scrollRect.content = content;

            var layout = contentGo.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(0, 0, 3, 3);
            layout.spacing = 0f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var fitter = contentGo.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            dropdownGo.SetActive(false);
            return dropdownGo;
        }

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
}
