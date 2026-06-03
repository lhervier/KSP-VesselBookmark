using KSP.UI.TooltipTypes;
using UnityEngine;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui
{
    /// <summary>
    /// Attache des tooltips natifs KSP (TooltipController_Text) à un GameObject. KSP gère le survol,
    /// le positionnement et l'affichage sur SON canvas de tooltip dédié — donc le tooltip déborde
    /// naturellement hors de la fenêtre, n'est jamais clippé, et adopte le look du jeu.
    /// Pré-requis : le GameObject doit avoir un Graphic raycastTarget (le cas des boutons/icônes).
    /// </summary>
    internal static class Tooltips
    {
        private static Tooltip_Text _prefab;
        private static Tooltip_Text Prefab
        {
            get
            {
                if (_prefab == null)
                {
                    _prefab = AssetBase.GetPrefab<Tooltip_Text>("Tooltip_Text");
                }
                return _prefab;
            }
        }

        public static void Attach(GameObject go, string text)
        {
            if (go == null || string.IsNullOrEmpty(text)) return;
            Tooltip_Text prefab = Prefab;
            if (prefab == null) return;

            var controller = go.AddComponent<TooltipController_Text>();
            controller.prefab = prefab;
            controller.RequireInteractable = false;   // affiche même sur un élément non-interactable / grisé
            controller.textString = text;
        }
    }
}
