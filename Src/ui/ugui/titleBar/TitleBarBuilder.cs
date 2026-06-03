using UnityEngine;
using UnityEngine.UI;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.sprites;
using com.github.lhervier.ksp.bookmarksmod;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.titleBar
{
    /// <summary>
    /// Barre de titre de la fenêtre : icône + titre à gauche, boutons à droite.
    /// Squelette pour l'instant : titre + bouton de fermeture (qui pilote ViewModel.WindowVisible).
    /// Sera enrichie ensuite (badge compteur, boutons ＋ / ⟳ / menu ⋯).
    /// </summary>
    public class TitleBarBuilder
    {
        private readonly BookmarksViewModel _viewModel;
        private readonly ButtonBuilder _buttonBuilder;

        public TitleBarBuilder(BookmarksViewModel viewModel)
        {
            this._viewModel = viewModel;
            this._buttonBuilder = new ButtonBuilder(viewModel);
        }

        public TitleBarController Create()
        {
            var go = new GameObject("Bookmarks.TitleBar", typeof(RectTransform));
            TitleBarController controller = go.AddComponent<TitleBarController>();
            controller.Initialize(_viewModel);

            // Échappe au VerticalLayoutGroup de la popupWindow : on s'ancre nous-mêmes en haut.
            var layoutElement = go.AddComponent<LayoutElement>();
            layoutElement.ignoreLayout = true;

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(
                -2f * VesselBookmarkPalette.WindowBorderThickness,
                VesselBookmarkPalette.TitleBarHeight);
            rect.anchoredPosition = new Vector2(0f, -VesselBookmarkPalette.WindowBorderThickness);

            // Fond + séparateur 1px en bas
            var image = go.AddComponent<Image>();
            image.sprite = Sprites.HorizontalBorders(
                VesselBookmarkPalette.TitleBarBgColor,
                VesselBookmarkPalette.TitleBarBorderColor,
                VesselBookmarkPalette.TitleBarBorderThickness);
            image.type = Image.Type.Sliced;
            image.color = Color.white;
            image.raycastTarget = true;

            var layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(
                Mathf.RoundToInt(VesselBookmarkPalette.DefaultPaddingLeft),
                Mathf.RoundToInt(VesselBookmarkPalette.DefaultPaddingRight),
                3, 3);
            layout.spacing = VesselBookmarkPalette.DefaultSpacing;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            // Titre (prend toute la largeur disponible)
            var titleGo = new GameObject("Title", typeof(RectTransform));
            titleGo.transform.SetParent(go.transform, false);
            var titleElement = titleGo.AddComponent<LayoutElement>();
            titleElement.flexibleWidth = 1f;
            var title = titleGo.AddComponent<Text>();
            title.text = ModLocalization.GetString("windowTitle");
            title.font = HighLogic.UISkin.font;
            title.fontSize = VesselBookmarkPalette.TitleFontSize;
            title.fontStyle = FontStyle.Bold;
            title.color = VesselBookmarkPalette.TitleColor;
            title.alignment = TextAnchor.MiddleLeft;
            title.horizontalOverflow = HorizontalWrapMode.Overflow;
            title.verticalOverflow = VerticalWrapMode.Overflow;
            title.raycastTarget = false;

            // Bouton de fermeture : ferme la fenêtre (et l'IMGUI suit, via WindowVisible)
            ButtonController close = _buttonBuilder.Create(
                "Close",
                "✕", // ✕
                () => _viewModel.WindowVisible = false);
            close.transform.SetParent(go.transform, false);

            return controller;
        }

        public class TitleBarController : BaseController
        {
        }
    }
}
