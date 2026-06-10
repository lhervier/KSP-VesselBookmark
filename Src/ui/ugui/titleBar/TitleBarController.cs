using UnityEngine;
using UnityEngine.UI;
using com.github.lhervier.ksp.shared.ugui.button;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.sprites;
using com.github.lhervier.ksp.shared;
using com.github.lhervier.ksp.shared.ugui.sprites;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.titleBar
{
    public class TitleBarController : BaseController
    {
        private Text _countLabel;
        private ButtonController _addButton;
        private Image _filterDot;

        public void BindCountLabel(Text label) => this._countLabel = label;
        public void BindAddButton(ButtonController button) => this._addButton = button;
        public void BindFilterDot(Image dot) => this._filterDot = dot;

        public void Start()
        {
            this.ViewModel.OnAvailableBookmarksChanged.Add(OnAvailableBookmarksChanged);
            this.ViewModel.OnActiveOrTargetChanged.Add(OnActiveOrTargetChanged);

            // "Active filter" dot: refreshes whenever a filter changes
            this.ViewModel.OnSelectedBodyChanged.Add(UpdateFilterDot);
            this.ViewModel.OnSelectedVesselTypeChanged.Add(UpdateFilterDot);
            this.ViewModel.OnSearchTextChanged.Add(UpdateFilterDot);
            this.ViewModel.OnFilterHasCommentChanged.Add(UpdateFilterDot);

            UpdateCount();
            UpdateAddButton();
            UpdateFilterDot();
        }

        public void OnDestroy()
        {
            this.ViewModel?.OnAvailableBookmarksChanged.Remove(OnAvailableBookmarksChanged);
            this.ViewModel?.OnActiveOrTargetChanged.Remove(OnActiveOrTargetChanged);
            this.ViewModel?.OnSelectedBodyChanged.Remove(UpdateFilterDot);
            this.ViewModel?.OnSelectedVesselTypeChanged.Remove(UpdateFilterDot);
            this.ViewModel?.OnSearchTextChanged.Remove(UpdateFilterDot);
            this.ViewModel?.OnFilterHasCommentChanged.Remove(UpdateFilterDot);
        }

        private void OnAvailableBookmarksChanged() => UpdateCount();
        private void OnActiveOrTargetChanged() => UpdateAddButton();

        private void UpdateCount()
        {
            if (_countLabel == null) return;
            _countLabel.text = $"{ViewModel.AvailableBookmarksCount} / {ViewModel.TotalBookmarksCount}";
        }

        private void UpdateAddButton()
        {
            if (_addButton == null) return;
            _addButton.SetInteractable(ViewModel.CanAddVesselBookmark());
        }

        private void UpdateFilterDot()
        {
            if (_filterDot != null) _filterDot.enabled = ViewModel.HasActiveFilters;
        }
    }
}
