using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.sprites;
using com.github.lhervier.ksp.shared;
using com.github.lhervier.ksp.shared.ugui;
using com.github.lhervier.ksp.shared.ugui.sprites;
using com.github.lhervier.ksp.shared.ugui.styles;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.overlays.editcomment
{
    /// <summary>
    /// Builds the content of the edit-comment internal popup: a subtitle (the bookmark title) above a
    /// multiline comment text area. The text area locks the game keyboard while it has the focus.
    /// </summary>
    public class EditCommentContentBuilder : IUGUIBuilder<EditCommentContentController>
    {
        public EditCommentContentController Build()
        {
            var rootGo = new GameObject("EditCommentContent", typeof(RectTransform));

            var layout = rootGo.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.spacing = 9f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            TextMeshProUGUI sub = BuildSub(rootGo.transform);
            TMP_InputField input = BuildTextArea(rootGo.transform);

            return rootGo
                .AddComponent<EditCommentContentController>()
                .WithSubComponent(sub)
                .WithInputField(input);
        }

        // Greyed subtitle showing the edited bookmark's title.
        private static TextMeshProUGUI BuildSub(Transform parent)
        {
            var go = new GameObject("Sub", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var label = UGUILabels.AddLabel(go);
            label.text = string.Empty;
            label.fontSize = VesselBookmarkPalette.CardSubFontSize;
            label.color = VesselBookmarkPalette.CardSubColor;
            label.alignment = TextAlignmentOptions.TopLeft;
            label.enableWordWrapping = true;
            return label;
        }

        // Multiline text area (TMP_InputField + clipped viewport + text + placeholder).
        private TMP_InputField BuildTextArea(Transform parent)
        {
            var inputGo = new GameObject("TextArea", typeof(RectTransform));
            inputGo.transform.SetParent(parent, false);
            var le = inputGo.AddComponent<LayoutElement>();
            le.minHeight = le.preferredHeight = VesselBookmarkPalette.TextAreaHeight;

            var bg = inputGo.AddComponent<Image>();
            bg.sprite = SpritesGlobal.Border(VesselBookmarkPalette.TextAreaBgColor, VesselBookmarkPalette.TextAreaBorderColor, 1);
            bg.type = Image.Type.Sliced;
            bg.color = Color.white;
            bg.raycastTarget = true;

            // RectMask2D sur le champ pour clipper ; Text + Placeholder enfants avec marges, et le champ
            // lui-même sert de textViewport.
            inputGo.AddComponent<RectMask2D>();

            var input = inputGo.AddComponent<TMP_InputField>();
            input.lineType = TMP_InputField.LineType.MultiLineNewline;

            int pad = Mathf.RoundToInt(VesselBookmarkPalette.TextAreaPadding);

            var placeholder = NewAreaText(inputGo.transform, "Placeholder", pad);
            placeholder.fontStyle = FontStyles.Italic;
            placeholder.color = VesselBookmarkPalette.SearchPlaceholderColor;
            placeholder.text = string.Empty;

            var text = NewAreaText(inputGo.transform, "Text", pad);
            text.color = VesselBookmarkPalette.TextAreaTextColor;

            input.textViewport = inputGo.GetComponent<RectTransform>();
            input.textComponent = text;
            input.placeholder = placeholder;
            input.fontAsset = DefaultPalette.Font;

            // Lock clavier au focus, unlock au blur (Unity envoie Select/Deselect au champ sélectionné)
            var trigger = inputGo.AddComponent<EventTrigger>();
            var selectEntry = new EventTrigger.Entry { eventID = EventTriggerType.Select };
            selectEntry.callback.AddListener(_ => InputLockManager.SetControlLock(ControlTypes.All, EditCommentOverlayController.LOCK_ID));
            trigger.triggers.Add(selectEntry);
            var deselectEntry = new EventTrigger.Entry { eventID = EventTriggerType.Deselect };
            deselectEntry.callback.AddListener(_ => InputLockManager.RemoveControlLock(EditCommentOverlayController.LOCK_ID));
            trigger.triggers.Add(deselectEntry);

            return input;
        }

        private static TextMeshProUGUI NewAreaText(Transform parent, string objectName, int pad)
        {
            var go = new GameObject(objectName, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(pad, pad);
            rect.offsetMax = new Vector2(-pad, -pad);
            var text = UGUILabels.AddLabel(go);
            text.fontSize = VesselBookmarkPalette.TextAreaFontSize;
            text.alignment = TextAlignmentOptions.TopLeft;
            text.enableWordWrapping = true;
            text.richText = false;
            return text;
        }
    }
}
