using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using com.github.lhervier.ksp.bookmarksmod.bookmarks;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.shared.ugui;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.body.list
{
    /// <summary>
    /// Liste des bookmarks, empilée verticalement : pour chaque section (module de commande, vaisseau)
    /// un en-tête + une aide, puis les lignes. Reconstruit le contenu sur OnAvailableBookmarksChanged ;
    /// rafraîchit le rendu des lignes (sélection / survol / actif / cible) sur les events correspondants.
    /// </summary>
    public class ListBuilder : IUGUIBuilder<ListController>
    {
        // ========================================
        // Builder parameters
        // ========================================

        private BookmarksViewModel _viewModel;
        public ListBuilder ViewModel(BookmarksViewModel viewModel)
        {
            this._viewModel = viewModel;
            return this;
        }

        // ======================================
        // Build
        // ======================================

        public ListController Build()
        {
            var go = new GameObject("Bookmarks.List", typeof(RectTransform));
            
            var layout = go.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.spacing = 0f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            return go
                .AddComponent<ListController>()
                .ViewModel(_viewModel);
        }
    }
}
