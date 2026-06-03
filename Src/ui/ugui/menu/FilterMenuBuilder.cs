using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.sprites;
using com.github.lhervier.ksp.bookmarksmod;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.menu
{
    /// <summary>
    /// Menu déroulant des filtres (déclenché par le bouton « ⋯ » du title bar). Un piège à clic
    /// plein écran (ferme au clic dehors) + un panneau ancré en haut à droite contenant : recherche,
    /// combos Corps/Type, case « commentaire seulement » et réinitialisation. Piloté par FilterMenuOpen.
    /// </summary>
    public class FilterMenuBuilder
    {
        private const string SEARCH_LOCK_ID = "VesselBookmarkMod_Search";

        private readonly BookmarksViewModel _viewModel;
        private readonly ComboBuilder _comboBuilder = new ComboBuilder();

        public FilterMenuBuilder(BookmarksViewModel viewModel)
        {
            this._viewModel = viewModel;
        }

        public FilterMenuController Create(Transform parent)
        {
            // Racine toujours active (sans graphique) pour que le controller exécute Start() et s'abonne.
            var rootGo = new GameObject("Bookmarks.FilterMenu", typeof(RectTransform));
            rootGo.transform.SetParent(parent, false);
            var rootLe = rootGo.AddComponent<LayoutElement>();
            rootLe.ignoreLayout = true;
            var rootRect = rootGo.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            FilterMenuController controller = rootGo.AddComponent<FilterMenuController>();
            controller.Initialize(_viewModel);

            // Piège à clic : couvre toute la fenêtre, ferme le menu au clic en dehors du panneau.
            var trapGo = new GameObject("ClickTrap", typeof(RectTransform));
            trapGo.transform.SetParent(rootGo.transform, false);
            var trapRect = trapGo.GetComponent<RectTransform>();
            trapRect.anchorMin = Vector2.zero;
            trapRect.anchorMax = Vector2.one;
            trapRect.offsetMin = Vector2.zero;
            trapRect.offsetMax = Vector2.zero;
            var trapImage = trapGo.AddComponent<Image>();
            trapImage.sprite = Sprites.Fill;
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
                -(VesselBookmarkPalette.WindowBorderThickness + VesselBookmarkPalette.DefaultPaddingRight),
                -(VesselBookmarkPalette.WindowBorderThickness + VesselBookmarkPalette.TitleBarHeight));

            var panelImage = panelGo.AddComponent<Image>();
            panelImage.sprite = Sprites.Border(VesselBookmarkPalette.MenuBgColor, VesselBookmarkPalette.MenuBorderColor, VesselBookmarkPalette.MenuThickness);
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
            InputField search = BuildSearchField(panelGo.transform);
            controller.BindSearch(search);

            // Combos Corps / Type
            ComboBuilder.ComboController bodyCombo = _comboBuilder.Create(panelGo.transform, ModLocalization.GetString("labelBody"));
            bodyCombo.OnSelect = v => _viewModel.SelectedBody = v;
            ComboBuilder.ComboController typeCombo = _comboBuilder.Create(panelGo.transform, ModLocalization.GetString("labelType"));
            typeCombo.OnSelect = v => _viewModel.SelectedVesselType = v;
            // Une seule combo ouverte à la fois : ouvrir l'une ferme l'autre.
            bodyCombo.OnBeforeOpen = () => typeCombo.Collapse();
            typeCombo.OnBeforeOpen = () => bodyCombo.Collapse();
            controller.BindCombos(bodyCombo, typeCombo);

            // Case « commentaire seulement »
            Image checkBox = BuildCheckbox(panelGo.transform, controller);
            controller.BindCheckbox(checkBox);

            // Séparateur + réinitialisation
            AddSeparator(panelGo.transform);
            BuildResetAction(panelGo.transform, search);

            controller.BindPanelAndTrap(panelGo, trapGo);
            return controller;
        }

        // ---- Sous-éléments ----------------------------------------------------------------

        private static Text AddSimpleText(Transform parent, string objectName, string text, int fontSize, Color color)
        {
            var go = new GameObject(objectName, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var label = go.AddComponent<Text>();
            label.text = text;
            label.font = HighLogic.UISkin.font;
            label.fontSize = fontSize;
            label.color = color;
            label.alignment = TextAnchor.MiddleLeft;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.raycastTarget = false;
            return label;
        }

        private static void AddSeparator(Transform parent)
        {
            var go = new GameObject("Separator", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>();
            le.minHeight = le.preferredHeight = 1f;
            var image = go.AddComponent<Image>();
            image.sprite = Sprites.Fill;
            image.type = Image.Type.Simple;
            image.color = VesselBookmarkPalette.MenuSeparatorColor;
            image.raycastTarget = false;
        }

        private InputField BuildSearchField(Transform parent)
        {
            var inputGo = new GameObject("Search", typeof(RectTransform));
            inputGo.transform.SetParent(parent, false);
            var le = inputGo.AddComponent<LayoutElement>();
            le.minHeight = le.preferredHeight = VesselBookmarkPalette.ComboHeight;

            var bg = inputGo.AddComponent<Image>();
            bg.sprite = Sprites.Border(VesselBookmarkPalette.SearchBgColor, VesselBookmarkPalette.SearchBorderColor, 1);
            bg.type = Image.Type.Sliced;
            bg.color = Color.white;
            bg.raycastTarget = true;

            inputGo.AddComponent<RectMask2D>();

            var input = inputGo.AddComponent<InputField>();
            input.lineType = InputField.LineType.SingleLine;

            int pad = Mathf.RoundToInt(VesselBookmarkPalette.SearchPaddingH);

            var placeholder = NewFieldText(inputGo.transform, "Placeholder", pad);
            placeholder.fontStyle = FontStyle.Italic;
            placeholder.color = VesselBookmarkPalette.SearchPlaceholderColor;
            placeholder.text = ModLocalization.GetString("menuSearchPlaceholder");

            var text = NewFieldText(inputGo.transform, "Text", pad);
            text.color = VesselBookmarkPalette.SearchTextColor;

            input.textComponent = text;
            input.placeholder = placeholder;
            input.onValueChanged.AddListener(v => _viewModel.SearchText = v);

            // Verrou clavier au focus / déverrou au blur (comme l'overlay de commentaire)
            var trigger = inputGo.AddComponent<EventTrigger>();
            var sel = new EventTrigger.Entry { eventID = EventTriggerType.Select };
            sel.callback.AddListener(_ => InputLockManager.SetControlLock(ControlTypes.All, SEARCH_LOCK_ID));
            trigger.triggers.Add(sel);
            var desel = new EventTrigger.Entry { eventID = EventTriggerType.Deselect };
            desel.callback.AddListener(_ => InputLockManager.RemoveControlLock(SEARCH_LOCK_ID));
            trigger.triggers.Add(desel);

            return input;
        }

        private static Text NewFieldText(Transform parent, string objectName, int pad)
        {
            var go = new GameObject(objectName, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(pad, 0f);
            rect.offsetMax = new Vector2(-pad, 0f);
            var text = go.AddComponent<Text>();
            text.font = HighLogic.UISkin.font;
            text.fontSize = VesselBookmarkPalette.SearchFontSize;
            text.alignment = TextAnchor.MiddleLeft;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.supportRichText = false;
            text.raycastTarget = false;
            return text;
        }

        private Image BuildCheckbox(Transform parent, FilterMenuController controller)
        {
            var rowGo = new GameObject("CommentFilter", typeof(RectTransform));
            rowGo.transform.SetParent(parent, false);
            var le = rowGo.AddComponent<LayoutElement>();
            le.minHeight = le.preferredHeight = VesselBookmarkPalette.ComboHeight;

            var rowImage = rowGo.AddComponent<Image>();
            rowImage.sprite = Sprites.Fill;
            rowImage.type = Image.Type.Simple;
            rowImage.color = Color.clear;
            rowImage.raycastTarget = true;
            var rowBtn = rowGo.AddComponent<Button>();
            rowBtn.targetGraphic = rowImage;
            rowBtn.transition = Selectable.Transition.None;
            rowBtn.onClick.AddListener(() => _viewModel.FilterHasComment = !_viewModel.FilterHasComment);

            var layout = rowGo.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.spacing = VesselBookmarkPalette.DefaultSpacing;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            // Boîte de la case (bordure) + remplissage accent quand cochée (toggle de l'enfant)
            var boxGo = new GameObject("Box", typeof(RectTransform));
            boxGo.transform.SetParent(rowGo.transform, false);
            var boxLe = boxGo.AddComponent<LayoutElement>();
            boxLe.minWidth = boxLe.preferredWidth = VesselBookmarkPalette.CheckboxSize;
            boxLe.minHeight = boxLe.preferredHeight = VesselBookmarkPalette.CheckboxSize;
            var boxImage = boxGo.AddComponent<Image>();
            boxImage.sprite = Sprites.Border(VesselBookmarkPalette.SearchBgColor, VesselBookmarkPalette.ComboBorderColor, 1);
            boxImage.type = Image.Type.Sliced;
            boxImage.color = Color.white;
            boxImage.raycastTarget = false;

            var checkGo = new GameObject("Check", typeof(RectTransform));
            checkGo.transform.SetParent(boxGo.transform, false);
            var checkRect = checkGo.GetComponent<RectTransform>();
            checkRect.anchorMin = Vector2.zero;
            checkRect.anchorMax = Vector2.one;
            checkRect.offsetMin = new Vector2(2f, 2f);
            checkRect.offsetMax = new Vector2(-2f, -2f);
            var checkImage = checkGo.AddComponent<Image>();
            checkImage.sprite = Sprites.Fill;
            checkImage.type = Image.Type.Simple;
            checkImage.color = VesselBookmarkPalette.AccentColor;
            checkImage.raycastTarget = false;

            var labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(rowGo.transform, false);
            var labelLe = labelGo.AddComponent<LayoutElement>();
            labelLe.flexibleWidth = 1f;
            var label = labelGo.AddComponent<Text>();
            label.text = ModLocalization.GetString("menuFilterWithComment");
            label.font = HighLogic.UISkin.font;
            label.fontSize = VesselBookmarkPalette.MenuLabelFontSize;
            label.color = VesselBookmarkPalette.LabelColor;
            label.alignment = TextAnchor.MiddleLeft;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.raycastTarget = false;

            return checkImage;
        }

        private void BuildResetAction(Transform parent, InputField search)
        {
            var go = new GameObject("Reset", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>();
            le.minHeight = le.preferredHeight = VesselBookmarkPalette.ComboHeight;

            var image = go.AddComponent<Image>();
            image.sprite = Sprites.Fill;
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
                if (search != null) search.text = string.Empty;   // le combo/checkbox se resync via events
            });

            var layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(Mathf.RoundToInt(VesselBookmarkPalette.MenuPaddingLeft), 0, 0, 0);
            layout.spacing = VesselBookmarkPalette.DefaultSpacing;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;

            // Icône ✕ devant le libellé (comme la maquette)
            var icon = AddSimpleText(go.transform, "Icon", "✕",
                VesselBookmarkPalette.MenuLabelFontSize, VesselBookmarkPalette.MenuLabelColor);
            var iconLe = icon.gameObject.AddComponent<LayoutElement>();
            iconLe.minWidth = iconLe.preferredWidth = 14f;
            icon.alignment = TextAnchor.MiddleCenter;

            var label = AddSimpleText(go.transform, "Label", ModLocalization.GetString("menuResetFilters"),
                VesselBookmarkPalette.MenuLabelFontSize, VesselBookmarkPalette.LabelColor);
            var labelLe = label.gameObject.AddComponent<LayoutElement>();
            labelLe.flexibleWidth = 1f;
        }

        public class FilterMenuController : BaseController
        {
            private GameObject _panel;
            private GameObject _trap;
            private InputField _search;
            private Image _checkbox;
            private ComboBuilder.ComboController _bodyCombo;
            private ComboBuilder.ComboController _typeCombo;

            public void BindPanelAndTrap(GameObject panel, GameObject trap) { _panel = panel; _trap = trap; }
            public void BindSearch(InputField search) => _search = search;
            public void BindCheckbox(Image checkbox) => _checkbox = checkbox;
            public void BindCombos(ComboBuilder.ComboController body, ComboBuilder.ComboController type)
            {
                _bodyCombo = body;
                _typeCombo = type;
            }

            public void Start()
            {
                ViewModel.OnFilterMenuOpenChanged.Add(OnFilterMenuOpenChanged);
                ViewModel.OnAvailableBodiesChanged.Add(RefreshBodyCombo);
                ViewModel.OnSelectedBodyChanged.Add(RefreshBodyCombo);
                ViewModel.OnAvailableVesselTypesChanged.Add(RefreshTypeCombo);
                ViewModel.OnSelectedVesselTypeChanged.Add(RefreshTypeCombo);
                ViewModel.OnFilterHasCommentChanged.Add(RefreshCheckbox);

                RefreshBodyCombo();
                RefreshTypeCombo();
                RefreshCheckbox();
                OnFilterMenuOpenChanged();
            }

            public void OnDestroy()
            {
                ViewModel?.OnFilterMenuOpenChanged.Remove(OnFilterMenuOpenChanged);
                ViewModel?.OnAvailableBodiesChanged.Remove(RefreshBodyCombo);
                ViewModel?.OnSelectedBodyChanged.Remove(RefreshBodyCombo);
                ViewModel?.OnAvailableVesselTypesChanged.Remove(RefreshTypeCombo);
                ViewModel?.OnSelectedVesselTypeChanged.Remove(RefreshTypeCombo);
                ViewModel?.OnFilterHasCommentChanged.Remove(RefreshCheckbox);
                InputLockManager.RemoveControlLock(SEARCH_LOCK_ID);
            }

            private void OnFilterMenuOpenChanged()
            {
                bool open = ViewModel.FilterMenuOpen;
                if (_panel != null) _panel.SetActive(open);
                if (_trap != null) _trap.SetActive(open);

                if (open)
                {
                    // Synchronise l'affichage à l'ouverture
                    if (_search != null) _search.text = ViewModel.SearchText ?? string.Empty;
                    RefreshBodyCombo();
                    RefreshTypeCombo();
                    RefreshCheckbox();
                }
                else
                {
                    _bodyCombo?.Collapse();
                    _typeCombo?.Collapse();
                }
            }

            private void RefreshBodyCombo()
            {
                _bodyCombo?.SetOptions(ViewModel.AvailableBodies, ViewModel.SelectedBody);
            }

            private void RefreshTypeCombo()
            {
                _typeCombo?.SetOptions(ViewModel.AvailableVesselTypes, ViewModel.SelectedVesselType);
            }

            private void RefreshCheckbox()
            {
                if (_checkbox != null) _checkbox.enabled = ViewModel.FilterHasComment;
            }
        }
    }
}
