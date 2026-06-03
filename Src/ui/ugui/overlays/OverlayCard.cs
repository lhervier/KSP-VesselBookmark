using UnityEngine;
using UnityEngine.UI;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.sprites;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.overlays
{
    /// <summary>
    /// Construit une « fausse popup » interne : un calque assombri qui remplit l'intérieur de la
    /// fenêtre (bloque les clics derrière) avec une carte centrée. Les builders spécifiques (édition
    /// de commentaire, confirmation de suppression) y greffent leur contenu. Le calque démarre inactif.
    /// </summary>
    internal static class OverlayCard
    {
        /// <summary>
        /// Crée le calque parenté à <paramref name="parent"/> (la popupWindow). La RACINE reste
        /// toujours active (sans graphique → invisible et non bloquante) pour que le controller qu'on
        /// y attache exécute bien son Start() et s'abonne ; seul le <paramref name="panel"/> (fond
        /// assombri + carte) est affiché/masqué. Renvoie la racine ; <paramref name="card"/> est à
        /// peupler par l'appelant (titre, texte, boutons…).
        /// </summary>
        public static GameObject Build(Transform parent, string name, out GameObject panel, out RectTransform card)
        {
            var rootGo = new GameObject(name, typeof(RectTransform));
            rootGo.transform.SetParent(parent, false);

            var le = rootGo.AddComponent<LayoutElement>();
            le.ignoreLayout = true;

            var rootRect = rootGo.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = new Vector2(VesselBookmarkPalette.WindowBorderThickness, VesselBookmarkPalette.WindowBorderThickness);
            rootRect.offsetMax = new Vector2(-VesselBookmarkPalette.WindowBorderThickness, -VesselBookmarkPalette.WindowBorderThickness);

            // Panneau affichable (le seul qu'on toggle) qui remplit la racine
            panel = new GameObject("Panel", typeof(RectTransform));
            panel.transform.SetParent(rootGo.transform, false);
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // Fond assombri, bloque les clics vers le contenu en dessous (modal)
            var dim = panel.AddComponent<Image>();
            dim.sprite = Sprites.Fill;
            dim.type = Image.Type.Simple;
            dim.color = VesselBookmarkPalette.OverlayDimColor;
            dim.raycastTarget = true;

            // Carte centrée, largeur fixe, hauteur ajustée au contenu
            var cardGo = new GameObject("Card", typeof(RectTransform));
            cardGo.transform.SetParent(panel.transform, false);
            card = cardGo.GetComponent<RectTransform>();
            card.anchorMin = new Vector2(0.5f, 0.5f);
            card.anchorMax = new Vector2(0.5f, 0.5f);
            card.pivot = new Vector2(0.5f, 0.5f);
            card.sizeDelta = new Vector2(VesselBookmarkPalette.WindowWidth * VesselBookmarkPalette.CardWidthRatio, 0f);

            var cardImage = cardGo.AddComponent<Image>();
            cardImage.sprite = Sprites.Border(
                VesselBookmarkPalette.CardBgColor,
                VesselBookmarkPalette.CardBorderColor,
                VesselBookmarkPalette.CardBorderThickness);
            cardImage.type = Image.Type.Sliced;
            cardImage.color = Color.white;
            cardImage.raycastTarget = true;

            var layout = cardGo.AddComponent<VerticalLayoutGroup>();
            int pad = Mathf.RoundToInt(VesselBookmarkPalette.CardPadding);
            layout.padding = new RectOffset(pad, pad, pad, pad);
            layout.spacing = 9f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var fitter = cardGo.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            panel.SetActive(false);   // la racine reste active ; on ne masque que le panneau
            return rootGo;
        }

        /// <summary>Ajoute un libellé simple à la carte (titre, sous-titre, message…).</summary>
        public static Text AddText(Transform card, string objectName, string text, int fontSize, Color color, FontStyle style = FontStyle.Normal)
        {
            var go = new GameObject(objectName, typeof(RectTransform));
            go.transform.SetParent(card, false);
            var label = go.AddComponent<Text>();
            label.text = text;
            label.font = HighLogic.UISkin.font;
            label.fontSize = fontSize;
            label.fontStyle = style;
            label.color = color;
            label.alignment = TextAnchor.UpperLeft;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.raycastTarget = false;
            return label;
        }

        /// <summary>Crée la rangée de boutons en bas de carte (alignée à droite).</summary>
        public static GameObject AddFootRow(Transform card)
        {
            var go = new GameObject("Foot", typeof(RectTransform));
            go.transform.SetParent(card, false);
            var layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.spacing = VesselBookmarkPalette.CardFootSpacing;
            layout.childAlignment = TextAnchor.MiddleRight;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            return go;
        }
    }
}
