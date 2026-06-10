using UnityEngine;
using UnityEngine.UI;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.sprites;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.titleBar;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.body;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.footer;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.menu;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.overlays;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui
{
    /// <summary>
    /// Construit la fenêtre via une KSP PopupDialog minimale (gérée nativement : clickthrough, zoom,
    /// échelle d'UI), puis la re-chrome aux couleurs de la maquette et y greffe le corps + la title bar.
    /// </summary>
    public class PopupDialogBuilder
    {
        private const string DIALOG_ID = "VesselBookmarksUGUI";

        private readonly BookmarksViewModel _viewModel;
        private readonly FilterMenuBuilder _filterMenuBuilder;
        private readonly EditCommentOverlayBuilder _editOverlayBuilder;
        private readonly RemoveConfirmOverlayBuilder _removeOverlayBuilder;

        public PopupDialogBuilder(BookmarksViewModel viewModel)
        {
            this._viewModel = viewModel;
            this._filterMenuBuilder = new FilterMenuBuilder(viewModel);
            this._editOverlayBuilder = new EditCommentOverlayBuilder(viewModel);
            this._removeOverlayBuilder = new RemoveConfirmOverlayBuilder(viewModel);
        }

        public PopupDialog CreatePopupDialog()
        {
            var pos = NormalizedWindowPos(
                VesselBookmarkPalette.WindowInitialPositionX,
                VesselBookmarkPalette.WindowInitialPositionY,
                VesselBookmarkPalette.WindowWidth,
                VesselBookmarkPalette.WindowHeight
            );

            // MultiOptionDialog ultra minimal : on n'utilise pas son contenu, on greffe le nôtre.
            var content = new DialogGUIVerticalLayout();
            MultiOptionDialog multiOptionDialog = new MultiOptionDialog(
                DIALOG_ID,
                string.Empty,
                string.Empty,
                HighLogic.UISkin,
                pos,
                new DialogGUIBase[]
                {
                    new DialogGUIBox(null, -1, -1, () => true, content)
                }
            );

            PopupDialog popupDialog = PopupDialog.SpawnPopupDialog(
                multiOptionDialog,
                true,
                HighLogic.UISkin,
                false,
                string.Empty
            );
            if (popupDialog == null || popupDialog.popupWindow == null)
            {
                return null;
            }

            PopupDialogController controller = popupDialog.popupWindow.AddComponent<PopupDialogController>();
            controller.Initialize(_viewModel);

            // Supprime le titre stock de KSP
            var title = popupDialog.popupWindow.transform.Find("Title");
            if (title != null)
            {
                title.gameObject.SetActive(false);
            }

            // Fond non transparent
            var canvasGroup = popupDialog.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }

            // Chrome de la fenêtre (fond + bordure 1px)
            var windowGo = popupDialog.popupWindow;
            var windowImage = windowGo.GetComponent<Image>();
            if (windowImage != null)
            {
                windowImage.sprite = Sprites.WindowChrome;
                windowImage.type = Image.Type.Sliced;
                windowImage.color = Color.white;
                windowImage.raycastTarget = true;   // bloque les clics vers le jeu
            }

            // Fond des sous-images de la fenêtre
            foreach (var image in windowGo.GetComponentsInChildren<Image>(true))
            {
                if (image == windowImage)
                {
                    continue;
                }
                image.sprite = Sprites.Fill;
                image.type = Image.Type.Simple;
                image.color = VesselBookmarkPalette.WindowBodyColor;
            }

            // Corps (scrollable) en premier dans le z-order, puis footer et title bar, enfin les
            // overlays internes par-dessus tout (ajoutés en dernier = au-dessus).
            BodyController bodyController = new BodyBuilder()
                .ViewModel(_viewModel)
                .Build();
            bodyController.transform.SetParent(windowGo.transform, false);

            FooterController footerController = new FooterBuilder()
                .ViewModel(_viewModel)
                .Build();
            footerController.transform.SetParent(windowGo.transform, false);

            TitleBarBuilder.TitleBarController titleBarController = new TitleBarBuilder(_viewModel)
                .Create();
            titleBarController.transform.SetParent(windowGo.transform, false);

            // Menu filtres (au-dessus du contenu), puis les overlays modaux (au-dessus de tout).
            this._filterMenuBuilder.Create(windowGo.transform);
            this._editOverlayBuilder.Create(windowGo.transform);
            this._removeOverlayBuilder.Create(windowGo.transform);

            return popupDialog;
        }

        /// <summary>
        /// Position normalisée depuis le coin haut-gauche de l'écran, en pourcentage de l'écran.
        /// </summary>
        private static Rect NormalizedWindowPos(float screenX, float screenYFromTop, float width, float height)
        {
            var centerX = screenX + width * 0.5f;
            var centerY = Screen.height - screenYFromTop - height * 0.5f;
            return new Rect(centerX / Screen.width, centerY / Screen.height, width, height);
        }

        public class PopupDialogController : BaseController
        {
            // Hooks à venir (menu filtres / overlays). Vide pour le squelette.
        }
    }
}
