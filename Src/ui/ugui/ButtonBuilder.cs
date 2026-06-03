using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.sprites;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui
{
    /// <summary>
    /// Fabrique de boutons stylés (fond + hover + libellé centré), réutilisée partout (close, add,
    /// refresh, actions…). L'état désactivé est rendu via un CanvasGroup (alpha 0.25 + blocage des
    /// raycasts), comme le ".ka:disabled { opacity:.25 }" de la maquette.
    /// </summary>
    public class ButtonBuilder
    {
        private readonly BookmarksViewModel _viewModel;

        public ButtonBuilder(BookmarksViewModel viewModel)
        {
            this._viewModel = viewModel;
        }

        public ButtonController Create(
            string objectName,
            string buttonLabel,
            Action onClick,
            bool interactable = true
        )
        {
            return Create(
                objectName,
                buttonLabel,
                onClick,
                interactable,
                VesselBookmarkPalette.ButtonBgColor,
                VesselBookmarkPalette.ButtonHoverColor,
                VesselBookmarkPalette.TitleButtonSize,
                VesselBookmarkPalette.TitleButtonFontSize
            );
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
            var buttonGo = new GameObject(objectName, typeof(RectTransform));
            ButtonController controller = buttonGo.AddComponent<ButtonController>();
            controller.Initialize(_viewModel);

            var layoutElement = buttonGo.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = size;
            layoutElement.preferredHeight = size;
            layoutElement.minWidth = size;
            layoutElement.minHeight = size;

            // Fond blanc : la teinte du Button s'applique telle quelle (pas de multiplication)
            var image = buttonGo.AddComponent<Image>();
            image.sprite = Sprites.Fill;
            image.type = Image.Type.Simple;
            image.color = Color.white;
            image.raycastTarget = true;

            var button = buttonGo.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.normalColor = backgroundColor;
            colors.highlightedColor = hoverColor;
            colors.pressedColor = backgroundColor;
            colors.selectedColor = backgroundColor;
            colors.disabledColor = backgroundColor;   // le fade désactivé est géré par le CanvasGroup
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.1f;
            button.colors = colors;
            button.onClick.AddListener(() => onClick());
            controller.InitButton(button);

            // CanvasGroup : alpha global + blocage des raycasts quand désactivé
            var canvasGroup = buttonGo.AddComponent<CanvasGroup>();
            controller.InitCanvasGroup(canvasGroup);

            // Libellé centré
            var labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(buttonGo.transform, false);
            var labelRect = labelGo.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            var label = labelGo.AddComponent<Text>();
            label.text = buttonLabel;
            label.font = HighLogic.UISkin.font;
            label.fontSize = fontSize;
            label.color = VesselBookmarkPalette.ButtonTextColor;
            label.alignment = TextAnchor.MiddleCenter;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.raycastTarget = false;
            controller.InitLabel(label);

            // Libellé en blanc au survol
            var trigger = buttonGo.AddComponent<EventTrigger>();
            var enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enterEntry.callback.AddListener(_ => label.color = Color.white);
            trigger.triggers.Add(enterEntry);
            var exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exitEntry.callback.AddListener(_ => label.color = VesselBookmarkPalette.ButtonTextColor);
            trigger.triggers.Add(exitEntry);

            controller.SetInteractable(interactable);

            return controller;
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
