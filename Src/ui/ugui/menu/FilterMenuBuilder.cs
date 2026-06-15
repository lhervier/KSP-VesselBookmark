using UnityEngine;
using UnityEngine.UI;
using TMPro;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.shared.ugui.checkbox;
using com.github.lhervier.ksp.shared;
using com.github.lhervier.ksp.shared.ugui;
using com.github.lhervier.ksp.shared.ugui.combo;
using com.github.lhervier.ksp.shared.ugui.sprites;
using com.github.lhervier.ksp.shared.ugui.styles;
using com.github.lhervier.ksp.shared.ugui.textfield;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.menu
{
    /// <summary>
    /// Menu déroulant des filtres (déclenché par le bouton « ⋯ » du title bar). Un piège à clic
    /// plein écran (ferme au clic dehors) + un panneau ancré en haut à droite contenant : recherche,
    /// combos Corps/Type, case « commentaire seulement » et réinitialisation. Piloté par FilterMenuOpen.
    /// </summary>
    public class FilterMenuBuilder : IUGUIBuilder<FilterMenuController>
    {
        // ===================================================
        // Builder parameters
        // ===================================================

        private BookmarksViewModel _viewModel;
        public FilterMenuBuilder ViewModel(BookmarksViewModel viewModel)
        {
            this._viewModel = viewModel;
            return this;
        }

        private Transform _parent;
        public FilterMenuBuilder Parent(Transform parent)
        {
            this._parent = parent;
            return this;
        }

        // =======================================
        // Build
        // =======================================

        public FilterMenuController Build()
        {
            // Racine toujours active (sans graphique) pour que le controller exécute Start() et s'abonne.
            var rootGo = new GameObject("Bookmarks.FilterMenu", typeof(RectTransform));
            rootGo.transform.SetParent(_parent, false);
            
            var rootLe = rootGo.AddComponent<LayoutElement>();
            rootLe.ignoreLayout = true;
            var rootRect = rootGo.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            // Piège à clic : couvre toute la fenêtre, ferme le menu au clic en dehors du panneau.
            var trapGo = new GameObject("ClickTrap", typeof(RectTransform));
            trapGo.transform.SetParent(rootGo.transform, false);
            var trapRect = trapGo.GetComponent<RectTransform>();
            trapRect.anchorMin = Vector2.zero;
            trapRect.anchorMax = Vector2.one;
            trapRect.offsetMin = Vector2.zero;
            trapRect.offsetMax = Vector2.zero;
            var trapImage = trapGo.AddComponent<Image>();
            trapImage.sprite = SpritesGlobal.FillSprite;
            trapImage.type = Image.Type.Simple;
            trapImage.color = Color.clear;
            trapImage.raycastTarget = true;
            var trapBtn = trapGo.AddComponent<Button>();
            trapBtn.targetGraphic = trapImage;
            trapBtn.transition = Selectable.Transition.None;
            trapBtn.onClick.AddListener(() => _viewModel.FilterMenuOpen = false);

            // Panneau, ancré en haut à droite, sous le title bar
            var panelGo = new GameObject("Panel", typeof(RectTransform));
            panelGo.transform.SetParent(rootGo.transform, false);
            var panelRect = panelGo.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(1f, 1f);
            panelRect.anchorMax = new Vector2(1f, 1f);
            panelRect.pivot = new Vector2(1f, 1f);
            panelRect.sizeDelta = new Vector2(VesselBookmarkPalette.MenuWidth, 0f);
            panelRect.anchoredPosition = new Vector2(
                -(PopupPalette.PopupBorderThickness + DefaultPalette.PaddingRight),
                -(PopupPalette.PopupBorderThickness + PopupPalette.TitleBarHeight));

            var panelImage = panelGo.AddComponent<Image>();
            panelImage.sprite = SpritesGlobal.Border(VesselBookmarkPalette.MenuBgColor, VesselBookmarkPalette.MenuBorderColor, VesselBookmarkPalette.MenuThickness);
            panelImage.type = Image.Type.Sliced;
            panelImage.color = Color.white;
            panelImage.raycastTarget = true;

            var panelLayout = panelGo.AddComponent<VerticalLayoutGroup>();
            panelLayout.padding = new RectOffset(
                Mathf.RoundToInt(VesselBookmarkPalette.MenuPaddingLeft),
                Mathf.RoundToInt(VesselBookmarkPalette.MenuPaddingRight),
                Mathf.RoundToInt(VesselBookmarkPalette.MenuPaddingTop),
                Mathf.RoundToInt(VesselBookmarkPalette.MenuPaddingBottom));
            panelLayout.spacing = VesselBookmarkPalette.MenuSpacing;
            panelLayout.childControlWidth = true;
            panelLayout.childControlHeight = true;
            panelLayout.childForceExpandWidth = true;
            panelLayout.childForceExpandHeight = false;

            var panelFitter = panelGo.AddComponent<ContentSizeFitter>();
            panelFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            panelFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Titre
            AddSimpleText(panelGo.transform, "Title",
                ModLocalization.GetString("menuFiltersTitle").ToUpperInvariant(),
                VesselBookmarkPalette.MenuTitleFontSize, VesselBookmarkPalette.MenuTitleColor);

            // Recherche
            TextFieldController search = BuildSearchField(panelGo.transform);
            
            // Combos Corps / Type
            ComboController bodyCombo = new ComboBuilder()
                .Parent(panelGo.transform)
                .Label(ModLocalization.GetString("labelBody"))
                .PreferredWidth(VesselBookmarkPalette.MenuComboLableWidth)
                .Build();
            ComboController typeCombo = new ComboBuilder()
                .Parent(panelGo.transform)
                .Label(ModLocalization.GetString("labelType"))
                .LabelFor(TranslateVesselType)
                .PreferredWidth(VesselBookmarkPalette.MenuComboLableWidth)
                .Build();
            
            // Case « commentaire seulement »
            CheckboxController checkBox = BuildCheckbox(panelGo.transform);
            
            // Séparateur + réinitialisation
            AddSeparator(panelGo.transform);
            BuildResetAction(panelGo.transform, search);

            return rootGo
                .AddComponent<FilterMenuController>()
                .ViewModel(_viewModel)
                .Search(search)
                .Combos(bodyCombo, typeCombo)
                .Checkbox(checkBox)
                .PanelAndTrap(panelGo, trapGo);
        }

        // Valeur brute du type de vaisseau → libellé traduit (la valeur « All » utilise vesselTypeAll).
        private static string TranslateVesselType(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            string key = value == "All" ? "vesselTypeAll" : "vesselType" + value;
            return ModLocalization.GetString(key);
        }

        // ---- Sous-éléments ----------------------------------------------------------------

        private static TextMeshProUGUI AddSimpleText(Transform parent, string objectName, string text, int fontSize, Color color)
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

        private static void AddSeparator(Transform parent)
        {
            var go = new GameObject("Separator", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>();
            le.minHeight = le.preferredHeight = 1f;
            var image = go.AddComponent<Image>();
            image.sprite = SpritesGlobal.FillSprite;
            image.type = Image.Type.Simple;
            image.color = VesselBookmarkPalette.MenuSeparatorColor;
            image.raycastTarget = false;
        }

        // Search field built on the shared TextField component (keyboard lock on focus is encapsulated).
        private TextFieldController BuildSearchField(Transform parent)
        {
            TextFieldController search = new TextFieldBuilder()
                .Parent(parent)
                .Placeholder(ModLocalization.GetString("menuSearchPlaceholder"))
                .Height(VesselBookmarkPalette.ComboHeight)
                .FontSize(VesselBookmarkPalette.SearchFontSize)
                .Build();
            search.OnValueChanged.Add(v => _viewModel.SearchText = v);
            return search;
        }

        private CheckboxController BuildCheckbox(Transform parent)
        {
            // Case à cocher partagée : libellé cliquable + ligne entière cliquable (Greedy).
            CheckboxController checkbox = new CheckboxBuilder()
                .Label(ModLocalization.GetString("menuFilterWithComment"))
                .Greedy(true)
                .Checked(_viewModel.FilterHasComment)
                .Build();
            checkbox.transform.SetParent(parent, false);

            // Aligne la hauteur de la ligne sur celle des combos/recherche du menu.
            var le = checkbox.GetComponent<LayoutElement>();
            le.minHeight = le.preferredHeight = VesselBookmarkPalette.ComboHeight;

            // Le clic bascule la case ; on reporte l'état vers le ViewModel (source de vérité).
            checkbox.OnToggled.Add(isChecked => _viewModel.FilterHasComment = isChecked);
            return checkbox;
        }

        private void BuildResetAction(Transform parent, TextFieldController search)
        {
            var go = new GameObject("Reset", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>();
            le.minHeight = le.preferredHeight = VesselBookmarkPalette.ComboHeight;

            var image = go.AddComponent<Image>();
            image.sprite = SpritesGlobal.FillSprite;
            image.type = Image.Type.Simple;
            image.color = Color.white;
            image.raycastTarget = true;
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = image;
            var colors = btn.colors;
            colors.normalColor = Color.clear;
            colors.highlightedColor = VesselBookmarkPalette.ComboItemHoverColor;
            colors.pressedColor = VesselBookmarkPalette.ComboItemHoverColor;
            colors.selectedColor = Color.clear;
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.1f;
            btn.colors = colors;
            btn.onClick.AddListener(() => {
                _viewModel.ClearFilters();
                if (search != null) search.SetText(string.Empty);   // le combo/checkbox se resync via events
            });

            var layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(Mathf.RoundToInt(VesselBookmarkPalette.MenuPaddingLeft), 0, 0, 0);
            layout.spacing = DefaultPalette.Spacing;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;

            // Icône ✕ devant le libellé (comme la maquette)
            var icon = AddSimpleText(go.transform, "Icon", DefaultPalette.PickGlyph("✕", "✗", "×", "x"),
                VesselBookmarkPalette.MenuLabelFontSize, VesselBookmarkPalette.MenuLabelColor);
            var iconLe = icon.gameObject.AddComponent<LayoutElement>();
            iconLe.minWidth = iconLe.preferredWidth = 14f;
            icon.alignment = TextAlignmentOptions.Center;

            var label = AddSimpleText(go.transform, "Label", ModLocalization.GetString("menuResetFilters"),
                VesselBookmarkPalette.MenuLabelFontSize, DefaultPalette.LabelColor);
            var labelLe = label.gameObject.AddComponent<LayoutElement>();
            labelLe.flexibleWidth = 1f;
        }
    }
}
