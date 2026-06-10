using UnityEngine;
using UnityEngine.UI;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.shared.ugui;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.overlays.remove
{
    /// <summary>
    /// Builds the content of the remove-confirmation internal popup: the confirmation message (filled
    /// with the pending bookmark's name by the orchestrator).
    /// </summary>
    public class RemoveConfirmContentBuilder : IUGUIBuilder<RemoveConfirmContentController>
    {
        public RemoveConfirmContentController Build()
        {
            var go = new GameObject("RemoveConfirmContent", typeof(RectTransform));

            var message = go.AddComponent<Text>();
            message.text = string.Empty;
            message.font = HighLogic.UISkin.font;
            message.fontSize = VesselBookmarkPalette.CardMsgFontSize;
            message.color = VesselBookmarkPalette.CardMsgColor;
            message.alignment = TextAnchor.UpperLeft;
            message.horizontalOverflow = HorizontalWrapMode.Wrap;
            message.verticalOverflow = VerticalWrapMode.Overflow;
            message.raycastTarget = false;

            return go
                .AddComponent<RemoveConfirmContentController>()
                .Message(message);
        }
    }
}
