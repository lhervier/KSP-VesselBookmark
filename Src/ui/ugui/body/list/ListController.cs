using System.Collections.Generic;
using com.github.lhervier.ksp.bookmarksmod.bookmarks;
using UnityEngine;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.body.list
{
    public class ListController : MonoBehaviour
    {
        private static readonly (BookmarkType type, string titleKey, string hintKey)[] SECTIONS =
        {
            (BookmarkType.CommandModule, "sectionCommandModule", "labelAddCommandModuleBookmark"),
            (BookmarkType.Vessel, "sectionVessel", "labelAddVesselBookmark"),
        };

        // Shared stand-in for a section that holds no bookmark. Read-only here (only its Count is
        // read and it is never indexed into), so a single shared instance is safe.
        private static readonly List<Bookmark> EmptySection = new List<Bookmark>();

        private readonly List<BookmarkRowController> _rows = new List<BookmarkRowController>();

        // ========================================
        // Life cycle
        // ========================================

        private BookmarksViewModel _viewModel;
        public ListController WithViewModel(BookmarksViewModel viewModel)
        {
            this._viewModel = viewModel;
            return this;
        }

        public void Start()
        {
            if( _viewModel != null )
            {
                _viewModel.OnAvailableBookmarksChanged.Add(OnAvailableBookmarksChanged);
                _viewModel.OnSelectedBookmarkChanged.Add(RefreshAll);
                _viewModel.OnHoveredBookmarkChanged.Add(RefreshAll);
                _viewModel.OnActiveOrTargetChanged.Add(RefreshAll);
            }

            Rebuild();
        }

        public void OnDestroy()
        {
            if( _viewModel != null )
            {
                _viewModel.OnAvailableBookmarksChanged.Remove(OnAvailableBookmarksChanged);
                _viewModel.OnSelectedBookmarkChanged.Remove(RefreshAll);
                _viewModel.OnHoveredBookmarkChanged.Remove(RefreshAll);
                _viewModel.OnActiveOrTargetChanged.Remove(RefreshAll);
            }
        }

        // ============================================
        // Methods bound to events
        // ============================================

        private void OnAvailableBookmarksChanged()
        {
            // Fenêtre masquée : on diffère (cf. _dirty / OnEnable) pour ne pas reconstruire dans le vide.
            if (!isActiveAndEnabled)
            {
                return;
            }
            Rebuild();
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

        // =======================================
        // Internal Helpers
        // =======================================

        private void Rebuild()
        {
            // Vide le contenu existant
            _rows.Clear();
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }

            var available = _viewModel.AvailableBookmarks;
            foreach (var section in SECTIONS)
            {
                // Every section is always rendered (header + "how to add" hint), even when it holds
                // no bookmark: an absent type is treated as an empty list, so the header shows a 0
                // count and the hint stays visible to tell the user how to populate it.
                if (!available.TryGetValue(section.type, out List<Bookmark> bookmarks) || bookmarks == null)
                {
                    bookmarks = EmptySection;
                }

                new SectionHeaderBuilder()
                    .WithParent(transform)
                    .WithTitleKey(section.titleKey)
                    .WithCount(bookmarks.Count)
                    .Build();
                new SectionHintBuilder()
                    .WithParent(transform)
                    .WithHintKey(section.hintKey)
                    .Build();

                for (int i = 0; i < bookmarks.Count; i++)
                {
                    bool isFirst = i == 0;
                    bool isLast = i == bookmarks.Count - 1;
                    var row = new BookmarkRowBuilder()
                        .WithViewModel(_viewModel)
                        .WithBookmark(bookmarks[i])
                        .WithFirstState(isFirst)
                        .WithLastState(isLast)
                        .Build();
                    row.transform.SetParent(transform, false);
                    _rows.Add(row);
                }
            }

            RefreshAll();
        }
    }
}
