using System.Collections.Generic;
using UnityEngine;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.sprites
{
    /// <summary>
    /// Charge les PNG du mod (icônes de type de vaisseau, alarme…) en sprites uGUI, avec cache.
    /// Renvoie null si la texture est introuvable (le builder gère gracieusement l'absence d'icône).
    /// </summary>
    internal static class Icons
    {
        private static readonly Dictionary<string, Sprite> _cache = new Dictionary<string, Sprite>();

        /// <summary>Icône correspondant au type de vaisseau du bookmark (Base, Station, Ship…).</summary>
        public static Sprite VesselType(string type)
        {
            string key = string.IsNullOrEmpty(type) ? "ship" : type.ToLowerInvariant();
            return Load("VesselBookmarkMod/vessel_types/" + key);
        }

        /// <summary>Icône d'alarme.</summary>
        public static Sprite Alarm => Load("VesselBookmarkMod/buttons/alarm");

        private static Sprite Load(string url)
        {
            if (_cache.TryGetValue(url, out Sprite cached))
            {
                return cached;
            }

            Texture2D tex = GameDatabase.Instance != null ? GameDatabase.Instance.GetTexture(url, false) : null;
            Sprite sprite = null;
            if (tex != null)
            {
                sprite = Sprite.Create(
                    tex,
                    new Rect(0f, 0f, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f),
                    100f);
                sprite.hideFlags = HideFlags.HideAndDontSave;
            }
            _cache[url] = sprite;
            return sprite;
        }
    }
}
