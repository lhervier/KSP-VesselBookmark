using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.sprites;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui
{
    /// <summary>
    /// Fabrique de boutons stylés (fond + hover + libellé ou icône), réutilisée partout. L'état
    /// désactivé est rendu via un CanvasGroup (alpha 0.25 + blocage des raycasts), comme le
    /// ".ka:disabled { opacity:.25 }" de la maquette.
    /// </summary>
    public class ButtonBuilder
    {
        private readonly BookmarksViewModel _viewModel;

        public ButtonBuilder(BookmarksViewModel viewModel)
        {
            this._viewModel = viewModel;
        }

        // ---- Bouton carré à libellé (glyphe / texte court) --------------------------------

        public ButtonController Create(
            string objectName,
            string buttonLabel,
            Action onClick,
            bool interactable = true
        )
        {
            return Create(
                objectName, buttonLabel, onClick, interactable,
                VesselBookmarkPalette.ButtonBgColor, VesselBookmarkPalette.ButtonHoverColor,
                VesselBookmarkPalette.TitleButtonSize, VesselBookmarkPalette.TitleButtonFontSize);
        }

        public ButtonController Create(
            string objectName,
            string buttonLabel,
            Action onClick,
            bool interactable,
            Color backgroundColor,
            Color hoverColor,
            float size,
            int fontSize
        )
        {
            var go = new GameObject(objectName, typeof(RectTransform));
            ButtonController controller = SetupBase(go, onClick, backgroundColor, hoverColor);

            var le = go.AddComponent<LayoutElement>();
            le.preferredWidth = le.minWidth = size;
            le.preferredHeight = le.minHeight = size;

            Text label = AddLabel(go, buttonLabel, fontSize);
            controller.InitLabel(label);
            AddHoverTint(go, label);

            controller.SetInteractable(interactable);
            return controller;
        }

        // ---- Bouton carré à icône (sprite PNG) --------------------------------------------

        public ButtonController CreateIcon(
            string objectName,
            Sprite icon,
            Action onClick,
            bool interactable,
            float size
        )
        {
            var go = new GameObject(objectName, typeof(RectTransform));
            ButtonController controller = SetupBase(go, onClick,
                VesselBookmarkPalette.ButtonBgColor, VesselBookmarkPalette.ButtonHoverColor);

            var le = go.AddComponent<LayoutElement>();
            le.preferredWidth = le.minWidth = size;
            le.preferredHeight = le.minHeight = size;

            if (icon != null)
            {
                var iconGo = new GameObject("Icon", typeof(RectTransform));
                iconGo.transform.SetParent(go.transform, false);
                var iconRect = iconGo.GetComponent<RectTransform>();
                iconRect.anchorMin = Vector2.zero;
                iconRect.anchorMax = Vector2.one;
                iconRect.offsetMin = new Vector2(5f, 5f);
                iconRect.offsetMax = new Vector2(-5f, -5f);
                var iconImg = iconGo.AddComponent<Image>();
                iconImg.sprite = icon;
                iconImg.type = Image.Type.Simple;
                iconImg.preserveAspect = true;
                iconImg.color = Color.white;
                iconImg.raycastTarget = false;
            }

            controller.SetInteractable(interactable);
            return controller;
        }

        // ---- Bouton texte à largeur automatique (OK / Annuler / Supprimer…) ---------------

        public ButtonController CreateTextButton(
            string objectName,
            string buttonLabel,
            Action onClick,
            bool interactable,
            Color backgroundColor,
            Color hoverColor,
            Color textColor,
            float height,
            int fontSize,
            float paddingH
        )
        {
            var go = new GameObject(objectName, typeof(RectTransform));
            ButtonController controller = SetupBase(go, onClick, backgroundColor, hoverColor);

            var le = go.AddComponent<LayoutElement>();
            le.minHeight = le.preferredHeight = height;

            var layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(Mathf.RoundToInt(paddingH), Mathf.RoundToInt(paddingH), 0, 0);
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            var fitter = go.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

            var labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(go.transform, false);
            var label = labelGo.AddComponent<Text>();
            label.text = buttonLabel;
            label.font = HighLogic.UISkin.font;
            label.fontSize = fontSize;
            label.color = textColor;
            label.alignment = TextAnchor.MiddleCenter;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.raycastTarget = false;
            controller.InitLabel(label);

            controller.SetInteractable(interactable);
            return controller;
        }

        // ---- Parties communes -------------------------------------------------------------

        private ButtonController SetupBase(GameObject go, Action onClick, Color backgroundColor, Color hoverColor)
        {
            ButtonController controller = go.AddComponent<ButtonController>();
            controller.Initialize(_viewModel);

            var image = go.AddComponent<Image>();
            image.sprite = Sprites.Fill;
            image.type = Image.Type.Simple;
            image.color = Color.white;
            image.raycastTarget = true;

            var button = go.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.normalColor = backgroundColor;
            colors.highlightedColor = hoverColor;
            colors.pressedColor = backgroundColor;
            colors.selectedColor = backgroundColor;
            colors.disabledColor = backgroundColor;
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.1f;
            button.colors = colors;
            button.onClick.AddListener(() => onClick());
            controller.InitButton(button);

            var canvasGroup = go.AddComponent<CanvasGroup>();
            controller.InitCanvasGroup(canvasGroup);

            return controller;
        }

        private static Text AddLabel(GameObject buttonGo, string text, int fontSize)
        {
            var labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(buttonGo.transform, false);
            var labelRect = labelGo.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            var label = labelGo.AddComponent<Text>();
            label.text = text;
            label.font = HighLogic.UISkin.font;
            label.fontSize = fontSize;
            label.color = VesselBookmarkPalette.ButtonTextColor;
            label.alignment = TextAnchor.MiddleCenter;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.raycastTarget = false;
            return label;
        }

        private static void AddHoverTint(GameObject go, Text label)
        {
            var trigger = go.AddComponent<EventTrigger>();
            var enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enterEntry.callback.AddListener(_ => label.color = Color.white);
            trigger.triggers.Add(enterEntry);
            var exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exitEntry.callback.AddListener(_ => label.color = VesselBookmarkPalette.ButtonTextColor);
            trigger.triggers.Add(exitEntry);
        }
    }

    public class ButtonController : BaseController
    {
        private const float DisabledAlpha = 0.25f;

        private Text _label;
        private Button _button;
        private CanvasGroup _canvasGroup;

        public void InitLabel(Text label) => this._label = label;
        public void InitButton(Button button) => this._button = button;
        public void InitCanvasGroup(CanvasGroup canvasGroup) => this._canvasGroup = canvasGroup;

        public void SetLabel(string text)
        {
            if (_label != null) _label.text = text;
        }

        public bool IsInteractable()
        {
            return _button != null && _button.interactable;
        }

        public void SetInteractable(bool interactable)
        {
            if (_button != null) _button.interactable = interactable;
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = interactable ? 1f : DisabledAlpha;
                _canvasGroup.blocksRaycasts = interactable;
                _canvasGroup.interactable = interactable;
            }
            if (_label != null) _label.color = VesselBookmarkPalette.ButtonTextColor;
        }
    }
}
