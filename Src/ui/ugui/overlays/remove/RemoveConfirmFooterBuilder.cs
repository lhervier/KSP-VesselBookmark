using UnityEngine;
using UnityEngine.UI;
using com.github.lhervier.ksp.shared.ugui.button;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.shared;
using com.github.lhervier.ksp.shared.ugui;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.overlays.remove
{
    /// <summary>
    /// Builds the footer of the remove-confirmation internal popup: a right-aligned row with Cancel and
    /// a danger-styled Remove button.
    /// </summary>
    public class RemoveConfirmFooterBuilder : IUGUIBuilder<RemoveConfirmFooterController>
    {
        public RemoveConfirmFooterController Build()
        {
            var rootGo = new GameObject("RemoveConfirmFooter", typeof(RectTransform));

            var layout = rootGo.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.spacing = VesselBookmarkPalette.CardFootSpacing;
            layout.childAlignment = TextAnchor.MiddleRight;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            // Cancel keeps the default button colors (VBMButtonBuilder defaults); only the auto-width
            // text-button shape, height and font size are set here.
            ButtonController cancel = new VBMButtonBuilder()
                .WithObjectName("Cancel")
                .WithLabel(ModLocalization.GetString("VBM_dialogButtonCancel"))
                .WithAutoWidth(VesselBookmarkPalette.CardButtonPaddingH)
                .WithSize(VesselBookmarkPalette.CardButtonHeight)
                .WithFontSize(VesselBookmarkPalette.CardButtonFontSize)
                .Build();
            cancel.transform.SetParent(rootGo.transform, false);

            ButtonController remove = new VBMButtonBuilder()
                .WithObjectName("Remove")
                .WithLabel(ModLocalization.GetString("VBM_dialogButtonRemove"))
                .WithAutoWidth(VesselBookmarkPalette.CardButtonPaddingH)
                .WithSize(VesselBookmarkPalette.CardButtonHeight)
                .WithFontSize(VesselBookmarkPalette.CardButtonFontSize)
                .WithBackgroundColor(VesselBookmarkPalette.CardButtonDangerBgColor)
                .WithHoverColor(VesselBookmarkPalette.CardButtonDangerBgColor)
                .WithTextColor(VesselBookmarkPalette.CardButtonDangerTextColor)
                .Build();
            remove.transform.SetParent(rootGo.transform, false);

            return rootGo
                .AddComponent<RemoveConfirmFooterController>()
                .WithCancelButtonController(cancel)
                .WithRemoveButtonController(remove);
        }
    }
}
