using UnityEngine.UI;
using com.github.lhervier.ksp.shared.ugui.button;
using com.github.lhervier.ksp.bookmarksmod.bookmarks;
using com.github.lhervier.ksp.shared;
using UnityEngine;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.footer
{
    public class FooterController : MonoBehaviour
    {
        // ======================================
        // Life cycle
        // ======================================

        private BookmarksViewModel _viewModel;
        public FooterController ViewModel(BookmarksViewModel viewModel)
        {
            this._viewModel = viewModel;
            return this;
        }

        private Text _selectionLabel;
        public FooterController SelectionLabel(Text label)
        {
            this._selectionLabel = label; 
            return this;
        }

        private ButtonController _edit;
        private ButtonController _goTo;
        private ButtonController _target;
        public FooterController Buttons(ButtonController edit, ButtonController goTo, ButtonController target)
        {
            this._edit = edit;
            this._goTo = goTo;
            this._target = target;
            return this;
        }

        public void Start()
        {
            _viewModel.OnSelectedBookmarkChanged.Add(Refresh);
            _viewModel.OnActiveOrTargetChanged.Add(Refresh);
            Refresh();

            if( _target != null )
            {
                _target.OnClick.Add(_viewModel.SetCurrentBookmarkVesselAsTarget);
            }
            if( _goTo != null )
            {
                _goTo.OnClick.Add(_viewModel.SwitchToSelectedVessel);
            }
            if( _edit != null )
            {
                _edit.OnClick.Add(_viewModel.BeginCommentEdition);
            }
        }

        public void OnDestroy()
        {
            if( _edit != null )
            {
                _edit.OnClick.Remove(_viewModel.BeginCommentEdition);
            }
            if( _goTo != null )
            {
                _goTo.OnClick.Remove(_viewModel.SwitchToSelectedVessel);
            }
            if( _target != null )
            {
                _target.OnClick.Remove(_viewModel.SetCurrentBookmarkVesselAsTarget);
            }

            _viewModel?.OnSelectedBookmarkChanged.Remove(Refresh);
            _viewModel?.OnActiveOrTargetChanged.Remove(Refresh);
        }

        // ========================================
        // Methods bound to events
        // ========================================

        private void Refresh()
        {
            Bookmark sel = _viewModel.SelectedBookmark;
            if (_selectionLabel != null)
            {
                _selectionLabel.text = sel != null
                    ? ModLocalization.GetString("footerSelection", sel.BookmarkTitle)
                    : ModLocalization.GetString("footerNoSelection");
            }
            if (_edit != null) _edit.SetInteractable(_viewModel.CanEditCurrentVesselComment());
            if (_goTo != null) _goTo.SetInteractable(_viewModel.CanSwitchToCurrentBookmarkVessel());
            if (_target != null) _target.SetInteractable(_viewModel.CanSetCurrentBookmarkVesselAsTarget());
        }
    }
}
