using UnityEngine;
using UnityEngine.UI;
using com.github.lhervier.ksp.shared.ugui.badge;
using com.github.lhervier.ksp.shared.ugui.button;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.sprites;
using com.github.lhervier.ksp.shared;
using com.github.lhervier.ksp.shared.ugui.sprites;
using com.github.lhervier.ksp.shared.ugui.styles;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.titleBar
{
    public class TitleBarController : MonoBehaviour
    {
        private BookmarksViewModel _viewModel;
        public TitleBarController WithViewModel(BookmarksViewModel viewModel)
        {
            this._viewModel = viewModel;
            return this;
        }
        
        private BadgeController _countBadge;
        public TitleBarController WithCountBadge(BadgeController badge)
        {
            this._countBadge = badge;
            return this;
        }
        
        private ButtonController _addButton;
        public TitleBarController WithAddButtonController(ButtonController button)
        {
            this._addButton = button;
            return this;
        }
        
        private Image _filterDot;
        public TitleBarController WithFilterDot(Image dot)
        {
            this._filterDot = dot;
            return this;
        }

        public void Start()
        {
            if( _viewModel != null )
            {
                this._viewModel.OnAvailableBookmarksChanged.Add(OnAvailableBookmarksChanged);
                this._viewModel.OnActiveOrTargetChanged.Add(OnActiveOrTargetChanged);

                // "Active filter" dot: refreshes whenever a filter changes. The aggregated event covers
                // every criterion at once — subscribing to them one by one had silently left the
                // situation filter out, and the dot then ignored it.
                this._viewModel.OnFiltersChanged.Add(UpdateFilterDot);

                UpdateCount();
                UpdateAddButton();
                UpdateFilterDot();
            }
        }

        public void OnDestroy()
        {
            if( _viewModel != null )
            {
                this._viewModel.OnAvailableBookmarksChanged.Remove(OnAvailableBookmarksChanged);
                this._viewModel.OnActiveOrTargetChanged.Remove(OnActiveOrTargetChanged);
                this._viewModel.OnFiltersChanged.Remove(UpdateFilterDot);
            }
        }

        private void OnAvailableBookmarksChanged() => UpdateCount();
        private void OnActiveOrTargetChanged() => UpdateAddButton();

        private void UpdateCount()
        {
            if (_countBadge == null) return;
            // Accent only while the filtering actually hides bookmarks: the badge then reads as "some
            // are missing from the list", and stays neutral the rest of the time.
            if (_viewModel.IsFilteringOut)
            {
                _countBadge.SetState(
                    $"{_viewModel.AvailableBookmarksCount} / {_viewModel.TotalBookmarksCount}",
                    DefaultPalette.AccentColor, DefaultPalette.AccentBgColor, DefaultPalette.AccentBorderColor);
            }
            else
            {
                _countBadge.SetState(
                    $"{_viewModel.AvailableBookmarksCount} / {_viewModel.TotalBookmarksCount}",
                    VesselBookmarkPalette.CountIdleTextColor,
                    VesselBookmarkPalette.CountIdleBgColor,
                    VesselBookmarkPalette.CountIdleBorderColor);
            }
        }

        private void UpdateAddButton()
        {
            if (_addButton == null) return;
            _addButton.SetInteractable(_viewModel.CanAddVesselBookmark());
        }

        private void UpdateFilterDot()
        {
            if (_filterDot != null) _filterDot.enabled = _viewModel.HasActiveFilters;
        }
    }
}
