using System.Collections.Generic;
using UnityEngine;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.sprites
{
    /// <summary>
    /// Sprites procéduraux partagés par les builders uGUI : un remplissage 1×1 (teinté via Image.color)
    /// et des fabriques de chrome 9-slice (fond + bordure). Pas d'assets PNG à fournir.
    /// Tous les sprites/textures générés portent HideAndDontSave : Unity ne les libère donc jamais
    /// automatiquement. Les fabriques 9-slice sont mises en cache par (fond, bordure, épaisseur) pour
    /// éviter de fuiter une Texture2D neuve à chaque reconstruction de la liste de bookmarks.
    /// </summary>
    internal static class Sprites
    {
        // Cache des sprites 9-slice, indexé par leurs paramètres. Le nombre de combinaisons distinctes
        // est petit (couleurs de la palette) : le cache plafonne donc à quelques entrées.
        private static readonly Dictionary<string, Sprite> _borderCache = new Dictionary<string, Sprite>();

        private static string BorderKey(string kind, Color fill, Color border, int thickness)
        {
            return $"{kind}|{fill}|{border}|{thickness}";
        }

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
            string key = BorderKey("border", fill, border, thickness);
            if (_borderCache.TryGetValue(key, out Sprite cached) && cached != null)
            {
                return cached;
            }

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
            _borderCache[key] = sprite;
            return sprite;
        }

        /// <summary>Sprite 9-slice : fond + bordure en haut et en bas uniquement (séparateurs horizontaux).</summary>
        public static Sprite HorizontalBorders(Color fill, Color border, int thickness)
        {
            string key = BorderKey("hborder", fill, border, thickness);
            if (_borderCache.TryGetValue(key, out Sprite cached) && cached != null)
            {
                return cached;
            }

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
            _borderCache[key] = sprite;
            return sprite;
        }

        // ==============================================================
        // Sprites nommés
        // ==============================================================

        /// <summary>Chrome de la fenêtre principale : fond sombre + bordure 1px (mis en cache par Border).</summary>
        public static Sprite WindowChrome => Border(
            VesselBookmarkPalette.WindowBodyColor,
            VesselBookmarkPalette.WindowBorderColor,
            VesselBookmarkPalette.WindowBorderThickness);
    }
}
