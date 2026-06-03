using UnityEngine;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.sprites
{
    /// <summary>
    /// Sprites procéduraux partagés par les builders uGUI : un remplissage 1×1 (teinté via Image.color)
    /// et des fabriques de chrome 9-slice (fond + bordure). Pas d'assets PNG à fournir.
    /// Les sprites nommés (chrome de fenêtre…) sont mis en cache ; les fabriques génériques créent
    /// un sprite par appel (les builders les appellent une fois et réutilisent la référence).
    /// </summary>
    internal static class Sprites
    {
        // ==============================================================
        // Remplissage simple (1×1, blanc) — à teinter via Image.color
        // ==============================================================
        private static Sprite _fill;
        public static Sprite Fill
        {
            get
            {
                if (_fill != null) return _fill;

                var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                tex.SetPixel(0, 0, Color.white);
                tex.Apply();
                tex.filterMode = FilterMode.Point;
                tex.hideFlags = HideFlags.HideAndDontSave;
                _fill = Sprite.Create(tex, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 100f);
                _fill.hideFlags = HideFlags.HideAndDontSave;
                return _fill;
            }
        }

        // ==============================================================
        // Fabriques de chrome 9-slice
        // ==============================================================

        /// <summary>Sprite 9-slice : fond + bordure de <paramref name="thickness"/> px sur les 4 côtés.</summary>
        public static Sprite Border(Color fill, Color border, int thickness)
        {
            int size = 2 * thickness + 1;
            var tex = VesselBookmarkTextures.MakeBorderTexture(fill, border, thickness);
            var sprite = Sprite.Create(
                tex,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                100f,
                0u,
                SpriteMeshType.FullRect,
                new Vector4(thickness, thickness, thickness, thickness));
            sprite.hideFlags = HideFlags.HideAndDontSave;
            return sprite;
        }

        /// <summary>Sprite 9-slice : fond + bordure en haut et en bas uniquement (séparateurs horizontaux).</summary>
        public static Sprite HorizontalBorders(Color fill, Color border, int thickness)
        {
            int height = 2 * thickness + 1;
            var tex = VesselBookmarkTextures.MakeHorizontalBordersTexture(fill, border, thickness);
            var sprite = Sprite.Create(
                tex,
                new Rect(0f, 0f, 1f, height),
                new Vector2(0.5f, 0.5f),
                100f,
                0u,
                SpriteMeshType.FullRect,
                // (left, bottom, right, top) — pas de bordure horizontale, thickness en haut + bas
                new Vector4(0f, thickness, 0f, thickness));
            sprite.hideFlags = HideFlags.HideAndDontSave;
            return sprite;
        }

        // ==============================================================
        // Sprites nommés (mis en cache)
        // ==============================================================

        /// <summary>Chrome de la fenêtre principale : fond sombre + bordure 1px.</summary>
        private static Sprite _windowChrome;
        public static Sprite WindowChrome
        {
            get
            {
                if (_windowChrome == null)
                {
                    _windowChrome = Border(
                        VesselBookmarkPalette.WindowBodyColor,
                        VesselBookmarkPalette.WindowBorderColor,
                        VesselBookmarkPalette.WindowBorderThickness);
                }
                return _windowChrome;
            }
        }
    }
}
