using UnityEngine;
using UnityEngine.UI;
using com.github.lhervier.ksp.shared.ugui.button;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.shared;
using com.github.lhervier.ksp.shared.ugui;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.overlays.editcomment
{
    /// <summary>
    /// Builds the footer of the edit-comment internal popup: a right-aligned row with Cancel and Save.
    /// </summary>
    public class EditCommentFooterBuilder : IUGUIBuilder<EditCommentFooterController>
    {
        public EditCommentFooterController Build()
        {
            var rootGo = new GameObject("EditCommentFooter", typeof(RectTransform));

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
                .WithLabel(ModLocalization.GetString("VBM_buttonCancel"))
                .WithAutoWidth(VesselBookmarkPalette.CardButtonPaddingH)
                .WithSize(VesselBookmarkPalette.CardButtonHeight)
                .WithFontSize(VesselBookmarkPalette.CardButtonFontSize)
                .Build();
            cancel.transform.SetParent(rootGo.transform, false);

            ButtonController ok = new VBMButtonBuilder()
                .WithObjectName("OK")
                .WithLabel(ModLocalization.GetString("VBM_buttonSave"))
                .WithAutoWidth(VesselBookmarkPalette.CardButtonPaddingH)
                .WithSize(VesselBookmarkPalette.CardButtonHeight)
                .WithFontSize(VesselBookmarkPalette.CardButtonFontSize)
                .WithBackgroundColor(VesselBookmarkPalette.CardButtonOkBgColor)
                .WithHoverColor(VesselBookmarkPalette.CardButtonOkBgColor)
                .WithTextColor(VesselBookmarkPalette.CardButtonOkTextColor)
                .Build();
            ok.transform.SetParent(rootGo.transform, false);

            return rootGo
                .AddComponent<EditCommentFooterController>()
                .WithCancelButtonController(cancel)
                .WithOkButtonController(ok);
        }
    }
}
