using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using com.github.lhervier.ksp.bookmarksmod.bookmarks;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.body.list
{
    /// <summary>
    /// Liste des bookmarks, empilée verticalement : pour chaque section (module de commande, vaisseau)
    /// un en-tête + une aide, puis les lignes. Reconstruit le contenu sur OnAvailableBookmarksChanged ;
    /// rafraîchit le rendu des lignes (sélection / survol / actif / cible) sur les events correspondants.
    /// </summary>
    public class ListBuilder
    {
        // Ordre d'affichage des sections + clés de localisation (titre / aide).
        private static readonly (BookmarkType type, string titleKey, string hintKey)[] SECTIONS =
        {
            (BookmarkType.CommandModule, "sectionCommandModule", "labelAddCommandModuleBookmark"),
            (BookmarkType.Vessel, "sectionVessel", "labelAddVesselBookmark"),
        };

        private readonly BookmarksViewModel _viewModel;

        public ListBuilder(BookmarksViewModel viewModel)
        {
            this._viewModel = viewModel;
        }

        public ListController Create()
        {
            var go = new GameObject("Bookmarks.List", typeof(RectTransform));
            ListController controller = go.AddComponent<ListController>();
            controller.Initialize(_viewModel);

            var layout = go.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(0, 0, 0, 0);
            layout.spacing = 0f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            return controller;
        }

        public class ListController : BaseController
        {
            private readonly SectionBuilder _sectionBuilder = new SectionBuilder();
            private BookmarkRowBuilder _rowBuilder;
            private readonly List<BookmarkRowBuilder.BookmarkRowController> _rows = new List<BookmarkRowBuilder.BookmarkRowController>();

            // La reconstruction est coûteuse (destruction/recréation de tous les GameObjects de lignes).
            // Quand la fenêtre est masquée (SetActive(false) sur la hiérarchie du popup → OnDisable ici),
            // on diffère la reconstruction et on l'applique à la réouverture (OnEnable). _dirty mémorise
            // qu'un changement de bookmarks a eu lieu pendant que la fenêtre était masquée.
            private bool _dirty = false;

            public void Start()
            {
                _rowBuilder = new BookmarkRowBuilder(ViewModel);

                ViewModel.OnAvailableBookmarksChanged.Add(OnAvailableBookmarksChanged);
                ViewModel.OnSelectedBookmarkChanged.Add(RefreshAll);
                ViewModel.OnHoveredBookmarkChanged.Add(RefreshAll);
                ViewModel.OnActiveOrTargetChanged.Add(RefreshAll);

                Rebuild();
            }

            public void OnEnable()
            {
                // Fenêtre ré-affichée : applique la reconstruction différée, ou réévalue au minimum le
                // rendu des lignes (vaisseau actif / cible ont pu changer pendant qu'on était masqué).
                if (_dirty)
                {
                    Rebuild();
                }
                else
                {
                    RefreshAll();
                }
            }

            public void OnDestroy()
            {
                ViewModel?.OnAvailableBookmarksChanged.Remove(OnAvailableBookmarksChanged);
                ViewModel?.OnSelectedBookmarkChanged.Remove(RefreshAll);
                ViewModel?.OnHoveredBookmarkChanged.Remove(RefreshAll);
                ViewModel?.OnActiveOrTargetChanged.Remove(RefreshAll);
            }

            private void OnAvailableBookmarksChanged()
            {
                // Fenêtre masquée : on diffère (cf. _dirty / OnEnable) pour ne pas reconstruire dans le vide.
                if (!isActiveAndEnabled)
                {
                    _dirty = true;
                    return;
                }
                Rebuild();
            }

            private void Rebuild()
            {
                _dirty = false;

                // Vide le contenu existant
                _rows.Clear();
                for (int i = transform.childCount - 1; i >= 0; i--)
                {
                    Destroy(transform.GetChild(i).gameObject);
                }

                var available = ViewModel.AvailableBookmarks;
                foreach (var section in SECTIONS)
                {
                    if (!available.TryGetValue(section.type, out List<Bookmark> bookmarks))
                    {
                        continue;
                    }

                    _sectionBuilder.CreateHeader(transform, section.titleKey, bookmarks.Count);
                    _sectionBuilder.CreateHint(transform, section.hintKey);

                    for (int i = 0; i < bookmarks.Count; i++)
                    {
                        bool isFirst = i == 0;
                        bool isLast = i == bookmarks.Count - 1;
                        var row = _rowBuilder.Create(bookmarks[i], isFirst, isLast);
                        row.transform.SetParent(transform, false);
                        _rows.Add(row);
                    }
                }

                RefreshAll();
            }

            private void RefreshAll()
            {
                // Inutile de réévaluer le rendu d'une fenêtre masquée : OnEnable s'en charge à la réouverture.
                if (!isActiveAndEnabled)
                {
                    return;
                }
                foreach (var row in _rows)
                {
                    if (row != null) row.Refresh();
                }
            }
        }
    }
}
