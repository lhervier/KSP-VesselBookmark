using UnityEngine;
using UnityEngine.UI;
using TMPro;
using com.github.lhervier.ksp.shared.ugui.button;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.sprites;
using com.github.lhervier.ksp.shared;
using com.github.lhervier.ksp.shared.ugui.sprites;

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
        
        private TextMeshProUGUI _countLabel;
        public TitleBarController WithCountLabel(TextMeshProUGUI label)
        {
            this._countLabel = label;
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

                // "Active filter" dot: refreshes whenever a filter changes
                this._viewModel.OnSelectedBodyChanged.Add(UpdateFilterDot);
                this._viewModel.OnSelectedVesselTypeChanged.Add(UpdateFilterDot);
                this._viewModel.OnSearchTextChanged.Add(UpdateFilterDot);
                this._viewModel.OnFilterHasCommentChanged.Add(UpdateFilterDot);

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
                this._viewModel.OnSelectedBodyChanged.Remove(UpdateFilterDot);
                this._viewModel.OnSelectedVesselTypeChanged.Remove(UpdateFilterDot);
                this._viewModel.OnSearchTextChanged.Remove(UpdateFilterDot);
                this._viewModel.OnFilterHasCommentChanged.Remove(UpdateFilterDot);
            }
        }

        private void OnAvailableBookmarksChanged() => UpdateCount();
        private void OnActiveOrTargetChanged() => UpdateAddButton();

        private void UpdateCount()
        {
            if (_countLabel == null) return;
            _countLabel.text = $"{_viewModel.AvailableBookmarksCount} / {_viewModel.TotalBookmarksCount}";
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
