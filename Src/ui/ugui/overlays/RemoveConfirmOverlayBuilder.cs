using UnityEngine;
using UnityEngine.UI;
using com.github.lhervier.ksp.bookmarksmod.bookmarks;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.bookmarksmod;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.overlays
{
    /// <summary>
    /// Fausse popup interne de confirmation de suppression, sur le modèle de l'overlay d'édition.
    /// Pilotée par ViewModel.PendingRemoval : visible dès qu'un bookmark est en attente de suppression.
    /// </summary>
    public class RemoveConfirmOverlayBuilder
    {
        private readonly BookmarksViewModel _viewModel;
        private readonly ButtonBuilder _buttonBuilder;

        public RemoveConfirmOverlayBuilder(BookmarksViewModel viewModel)
        {
            this._viewModel = viewModel;
            this._buttonBuilder = new ButtonBuilder(viewModel);
        }

        public RemoveConfirmOverlayController Create(Transform parent)
        {
            GameObject panel;
            RectTransform card;
            GameObject root = OverlayCard.Build(parent, "Bookmarks.RemoveOverlay", out panel, out card);
            RemoveConfirmOverlayController controller = root.AddComponent<RemoveConfirmOverlayController>();
            controller.Initialize(_viewModel);
            controller.BindPanel(panel);

            OverlayCard.AddText(card, "Title",
                ModLocalization.GetString("dialogRemoveTitle"),
                VesselBookmarkPalette.CardTitleFontSize, VesselBookmarkPalette.DangerColor, FontStyle.Bold);

            Text message = OverlayCard.AddText(card, "Message", string.Empty,
                VesselBookmarkPalette.CardMsgFontSize, VesselBookmarkPalette.CardMsgColor);
            controller.BindMessage(message);

            GameObject foot = OverlayCard.AddFootRow(card);

            ButtonController cancel = _buttonBuilder.CreateTextButton(
                "Cancel", ModLocalization.GetString("dialogButtonCancel"),
                () => _viewModel.CancelPendingRemoval(), true,
                VesselBookmarkPalette.ButtonBgColor, VesselBookmarkPalette.ButtonHoverColor, VesselBookmarkPalette.ButtonTextColor,
                VesselBookmarkPalette.CardButtonHeight, VesselBookmarkPalette.CardButtonFontSize, VesselBookmarkPalette.CardButtonPaddingH);
            cancel.transform.SetParent(foot.transform, false);

            ButtonController remove = _buttonBuilder.CreateTextButton(
                "Remove", ModLocalization.GetString("dialogButtonRemove"),
                () => _viewModel.ConfirmPendingRemoval(), true,
                VesselBookmarkPalette.CardButtonDangerBgColor, VesselBookmarkPalette.CardButtonDangerBgColor, VesselBookmarkPalette.CardButtonDangerTextColor,
                VesselBookmarkPalette.CardButtonHeight, VesselBookmarkPalette.CardButtonFontSize, VesselBookmarkPalette.CardButtonPaddingH);
            remove.transform.SetParent(foot.transform, false);

            return controller;
        }

        public class RemoveConfirmOverlayController : BaseController
        {
            private GameObject _panel;
            private Text _message;

            public void BindPanel(GameObject panel) => this._panel = panel;
            public void BindMessage(Text message) => this._message = message;

            public void Start()
            {
                ViewModel.OnPendingRemovalChanged.Add(OnPendingRemovalChanged);
                OnPendingRemovalChanged();
            }

            public void OnDestroy()
            {
                ViewModel?.OnPendingRemovalChanged.Remove(OnPendingRemovalChanged);
            }

            private void OnPendingRemovalChanged()
            {
                Bookmark pending = ViewModel.PendingRemoval;
                if (pending != null && _message != null)
                {
                    _message.text = ModLocalization.GetString("dialogRemoveMessageWithName", pending.BookmarkTitle);
                }
                if (_panel != null) _panel.SetActive(pending != null);
            }
        }
    }
}
