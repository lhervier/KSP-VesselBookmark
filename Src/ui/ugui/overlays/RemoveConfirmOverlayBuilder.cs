using UnityEngine;
using UnityEngine.UI;
using com.github.lhervier.ksp.shared.ugui.button;
using com.github.lhervier.ksp.bookmarksmod.bookmarks;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.shared;
using com.github.lhervier.ksp.shared.ugui;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.overlays
{
    /// <summary>
    /// Fausse popup interne de confirmation de suppression, sur le modèle de l'overlay d'édition.
    /// Pilotée par ViewModel.PendingRemoval : visible dès qu'un bookmark est en attente de suppression.
    /// </summary>
    public class RemoveConfirmOverlayBuilder : IUGUIBuilder<RemoveConfirmOverlayController>
    {
        private BookmarksViewModel _viewModel;
        public RemoveConfirmOverlayBuilder ViewModel(BookmarksViewModel viewModel)
        {
            this._viewModel = viewModel;
            return this;
        }

        private Transform _parent;
        public RemoveConfirmOverlayBuilder Parent(Transform parent)
        {
            this._parent = parent;
            return this;
        }

        public RemoveConfirmOverlayController Build()
        {
            GameObject root = OverlayCard.Build(
                _parent, 
                "Bookmarks.RemoveOverlay", 
                out GameObject panel, 
                out RectTransform card
            );
            
            OverlayCard.AddText(
                card, 
                "Title",
                ModLocalization.GetString("dialogRemoveTitle"),
                VesselBookmarkPalette.CardTitleFontSize, 
                VesselBookmarkPalette.DangerColor, 
                FontStyle.Bold
            );

            Text message = OverlayCard.AddText(
                card, 
                "Message", 
                string.Empty,
                VesselBookmarkPalette.CardMsgFontSize, 
                VesselBookmarkPalette.CardMsgColor
            );
            
            GameObject foot = OverlayCard.AddFootRow(card);

            // Cancel keeps the default button colors (VBMButtonBuilder defaults); only the auto-width
            // text-button shape, height and font size are set here.
            ButtonController cancel = new VBMButtonBuilder()
                .ObjectName("Cancel")
                .Label(ModLocalization.GetString("dialogButtonCancel"))
                .AutoWidth(VesselBookmarkPalette.CardButtonPaddingH)
                .Size(VesselBookmarkPalette.CardButtonHeight)
                .FontSize(VesselBookmarkPalette.CardButtonFontSize)
                .Build();
            cancel.transform.SetParent(foot.transform, false);

            ButtonController remove = new VBMButtonBuilder()
                .ObjectName("Remove")
                .Label(ModLocalization.GetString("dialogButtonRemove"))
                .AutoWidth(VesselBookmarkPalette.CardButtonPaddingH)
                .Size(VesselBookmarkPalette.CardButtonHeight)
                .FontSize(VesselBookmarkPalette.CardButtonFontSize)
                .BackgroundColor(VesselBookmarkPalette.CardButtonDangerBgColor)
                .HoverColor(VesselBookmarkPalette.CardButtonDangerBgColor)
                .TextColor(VesselBookmarkPalette.CardButtonDangerTextColor)
                .Build();
            remove.transform.SetParent(foot.transform, false);

            return root
                .AddComponent<RemoveConfirmOverlayController>()
                .ViewModel(_viewModel)
                .Panel(panel)
                .Message(message)
                .CancelButtonController(cancel)
                .RemoveButtonController(remove);
        }
    }
}
