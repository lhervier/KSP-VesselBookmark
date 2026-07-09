using UnityEngine;
using UnityEngine.UI;
using TMPro;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.shared.ugui;
using com.github.lhervier.ksp.shared.ugui.textfield;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.popins.editcomment
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
            TextFieldController input = BuildTextArea(rootGo.transform);

            return rootGo
                .AddComponent<EditCommentContentController>()
                .WithSubComponent(sub)
                .WithTextFieldController(input);
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

        // Multiline comment text area, built on the shared TextField component (bordered background,
        // clipped viewport, keyboard lock on focus — all encapsulated by the builder/controller).
        private TextFieldController BuildTextArea(Transform parent)
        {
            return new TextFieldBuilder()
                .WithParent(parent)
                .WithMultilineState(true)
                .WithHeight(VesselBookmarkPalette.TextAreaHeight)
                .WithFontSize(VesselBookmarkPalette.TextAreaFontSize)
                .Build();
        }
    }
}
