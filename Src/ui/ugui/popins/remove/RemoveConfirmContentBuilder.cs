using UnityEngine;
using TMPro;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.shared.ugui;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.popins.remove
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

            var message = UGUILabels.AddLabel(go);
            message.text = string.Empty;
            message.fontSize = VesselBookmarkPalette.CardMsgFontSize;
            message.color = VesselBookmarkPalette.CardMsgColor;
            message.alignment = TextAlignmentOptions.TopLeft;
            message.enableWordWrapping = true;

            return go
                .AddComponent<RemoveConfirmContentController>()
                .WithMessageComponent(message);
        }
    }
}
