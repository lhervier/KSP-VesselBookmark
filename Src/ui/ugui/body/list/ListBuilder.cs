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

            public void Start()
            {
                _rowBuilder = new BookmarkRowBuilder(ViewModel);

                ViewModel.OnAvailableBookmarksChanged.Add(Rebuild);
                ViewModel.OnSelectedBookmarkChanged.Add(RefreshAll);
                ViewModel.OnHoveredBookmarkChanged.Add(RefreshAll);
                ViewModel.OnActiveOrTargetChanged.Add(RefreshAll);

                Rebuild();
            }

            public void OnDestroy()
            {
                ViewModel?.OnAvailableBookmarksChanged.Remove(Rebuild);
                ViewModel?.OnSelectedBookmarkChanged.Remove(RefreshAll);
                ViewModel?.OnHoveredBookmarkChanged.Remove(RefreshAll);
                ViewModel?.OnActiveOrTargetChanged.Remove(RefreshAll);
            }

            private void Rebuild()
            {
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
                foreach (var row in _rows)
                {
                    if (row != null) row.Refresh();
                }
            }
        }
    }
}
