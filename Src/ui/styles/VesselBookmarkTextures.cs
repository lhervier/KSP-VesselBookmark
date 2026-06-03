using UnityEngine;

namespace com.github.lhervier.ksp.bookmarksmod.ui.styles
{
    /// <summary>
    /// Petits générateurs de textures procédurales, utilisés pour fabriquer des sprites 9-slice
    /// (fond + bordure) sans avoir à fournir d'assets PNG. Repris du pattern du mod frère.
    /// </summary>
    internal sealed class VesselBookmarkTextures
    {
        /// <summary>
        /// Texture carrée (2*thickness+1) avec une bordure de <paramref name="thickness"/> px sur les
        /// quatre côtés et un remplissage au centre. À utiliser en sprite 9-slice avec
        /// border = (thickness, thickness, thickness, thickness).
        /// </summary>
        public static Texture2D MakeBorderTexture(Color fill, Color border, int thickness)
        {
            var size = 2 * thickness + 1;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var isBorder = x < thickness || x >= size - thickness
                                || y < thickness || y >= size - thickness;
                    tex.SetPixel(x, y, isBorder ? border : fill);
                }
            }
            tex.Apply();
            tex.filterMode = FilterMode.Point;
            tex.hideFlags = HideFlags.HideAndDontSave;
            return tex;
        }

        /// <summary>
        /// Texture 1×(2*thickness+1) avec bordure en haut et en bas uniquement (pas de gauche/droite).
        /// À utiliser en sprite 9-slice avec border = (0, thickness, 0, thickness) pour des séparateurs
        /// horizontaux (en-têtes de section, par ex.).
        /// </summary>
        public static Texture2D MakeHorizontalBordersTexture(Color fill, Color border, int thickness)
        {
            var height = 2 * thickness + 1;
            var tex = new Texture2D(1, height, TextureFormat.RGBA32, false);
            for (var y = 0; y < height; y++)
            {
                var isBorder = y < thickness || y >= height - thickness;
                tex.SetPixel(0, y, isBorder ? border : fill);
            }
            tex.Apply();
            tex.filterMode = FilterMode.Point;
            tex.hideFlags = HideFlags.HideAndDontSave;
            return tex;
        }
    }
}
