using UnityEngine;
using UnityEngine.UI;
using TMPro;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.bookmarksmod.util;
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
    /// Drop-down filter menu (opened by the title bar's "⋯" button). A full-window click trap (closes
    /// on a click outside) plus a panel anchored top-right holding: search field, Body/Type/Situation/
    /// Alarm combos, "with a comment only" checkbox and reset action. Driven by FilterMenuOpen.
    /// </summary>
    public class FilterMenuBuilder : IUGUIBuilder<FilterMenuController>
    {
        // ===================================================
        // Builder parameters
        // ===================================================

        private Transform _parent;
        public FilterMenuBuilder WithParent(Transform parent)
        {
            this._parent = parent;
            return this;
        }

        private BookmarksViewModel _viewModel;
        public FilterMenuBuilder WithViewModel(BookmarksViewModel viewModel)
        {
            this._viewModel = viewModel;
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
            
            // Combos Corps / Type / Situation. Chaque combo liste TOUTES les valeurs possibles ; le
            // content builder grise celles qui ne correspondent à aucun bookmark (prédicat IsXPopulated
            // du ViewModel), tout en les gardant sélectionnables.
            ComboController bodyCombo = new ComboBuilder()
                .WithParent(panelGo.transform)
                .WithLabel(ModLocalization.GetString("labelBody"))
                .WithLabelFor(_viewModel.LabelForBody)
                .WithItemContentBuilder(new FilterComboItemContentBuilder()
                    .WithLabelFor(_viewModel.LabelForBody)
                    .WithEnabledFor(_viewModel.IsBodyPopulated)
                    .WithIndentFor(CelestialBodySorter.GetIndentLevel, VesselBookmarkPalette.ComboItemIndentStep))
                .WithPreferredWidth(VesselBookmarkPalette.MenuComboLableWidth)
                .Build();
            ComboController typeCombo = new ComboBuilder()
                .WithParent(panelGo.transform)
                .WithLabel(ModLocalization.GetString("labelType"))
                .WithLabelFor(_viewModel.LabelForVesselType)
                .WithItemContentBuilder(new FilterComboItemContentBuilder()
                    .WithLabelFor(_viewModel.LabelForVesselType)
                    .WithEnabledFor(_viewModel.IsVesselTypePopulated))
                .WithPreferredWidth(VesselBookmarkPalette.MenuComboLableWidth)
                .Build();
            ComboController situationCombo = new ComboBuilder()
                .WithParent(panelGo.transform)
                .WithLabel(ModLocalization.GetString("labelSituation"))
                .WithLabelFor(_viewModel.LabelForSituation)
                .WithItemContentBuilder(new FilterComboItemContentBuilder()
                    .WithLabelFor(_viewModel.LabelForSituation)
                    .WithEnabledFor(_viewModel.IsSituationPopulated))
                .WithPreferredWidth(VesselBookmarkPalette.MenuComboLableWidth)
                .Build();
            // Alarme : critère à trois états (les deux / avec / sans), d'où un combo et non une case
            // à cocher, qui ne saurait exprimer le « sans ».
            ComboController alarmCombo = new ComboBuilder()
                .WithParent(panelGo.transform)
                .WithLabel(ModLocalization.GetString("labelAlarm"))
                .WithLabelFor(_viewModel.LabelForAlarm)
                .WithItemContentBuilder(new FilterComboItemContentBuilder()
                    .WithLabelFor(_viewModel.LabelForAlarm)
                    .WithEnabledFor(_viewModel.IsAlarmPopulated))
                .WithPreferredWidth(VesselBookmarkPalette.MenuComboLableWidth)
                .Build();

            // Case « commentaire seulement »
            CheckboxController checkBox = BuildCheckbox(panelGo.transform);
            
            // Séparateur + réinitialisation
            AddSeparator(panelGo.transform);
            BuildResetAction(panelGo.transform, search);

            return rootGo
                .AddComponent<FilterMenuController>()
                .WithViewModel(_viewModel)
                .WithSearchFieldController(search)
                .WithComboControllers(bodyCombo, typeCombo, situationCombo, alarmCombo)
                .WithCheckboxController(checkBox)
                .WithPanelAndTrap(panelGo, trapGo);
        }

        // (Raw filter value -> displayed label: see BookmarksViewModel.LabelForXxx. Shared with the
        // filter bar so a criterion is named identically wherever it shows up.)

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
                .WithParent(parent)
                .WithPlaceholder(ModLocalization.GetString("menuSearchPlaceholder"))
                .WithHeight(VesselBookmarkPalette.ComboHeight)
                .WithFontSize(VesselBookmarkPalette.SearchFontSize)
                .WithClearButtonState(true)
                .Build();
            search.OnValueChanged.Add(v => _viewModel.SearchText = v);
            return search;
        }

        private CheckboxController BuildCheckbox(Transform parent)
        {
            // Case à cocher partagée : libellé cliquable + ligne entière cliquable (Greedy).
            CheckboxController checkbox = new CheckboxBuilder()
                .WithLabel(ModLocalization.GetString("menuFilterWithComment"))
                .WithGreedyState(true)
                .WithCheckedState(_viewModel.FilterHasComment)
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

            // Icône ✕ devant le libellé (comme la maquette). On privilégie "×" (U+00D7, présent en dur
            // dans la police du jeu, comme le bouton de fermeture) : les variantes "ballot X" (✕/✗) ne
            // sont ajoutées que dynamiquement depuis un fallback et rendent un glyphe illisible à 11px.
            var icon = AddSimpleText(go.transform, "Icon", DefaultPalette.PickGlyph("×", "✕", "✗", "x"),
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
