using UnityEngine;
using UnityEngine.UI;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.sprites;
using com.github.lhervier.ksp.shared.ugui.checkbox;
using com.github.lhervier.ksp.shared;
using com.github.lhervier.ksp.shared.ugui.combo;
using com.github.lhervier.ksp.shared.ugui.textfield;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.menu
{
    public class FilterMenuController : MonoBehaviour
    {
        // ===============================================
        // Life cycle
        // ===============================================

        private BookmarksViewModel _viewModel;
        public FilterMenuController WithViewModel(BookmarksViewModel viewModel)
        {
            _viewModel = viewModel;
            return this;
        }

        private GameObject _panel;
        private GameObject _trap;
        public FilterMenuController WithPanelAndTrap(GameObject panel, GameObject trap) { 
            _panel = panel; 
            _trap = trap; 
            return this;
        }
        
        private TextFieldController _search;
        public FilterMenuController WithSearchFieldController(TextFieldController search)
        {
            _search = search;
            return this;
        }
        
        private CheckboxController _checkbox;
        public FilterMenuController WithCheckboxController(CheckboxController checkbox) 
        {
            _checkbox = checkbox;
            return this;
        }
        
        private ComboController _bodyCombo;
        private ComboController _typeCombo;
        private ComboController _situationCombo;
        public FilterMenuController WithComboControllers(ComboController body, ComboController type, ComboController situation)
        {
            _bodyCombo = body;
            _typeCombo = type;
            _situationCombo = situation;
            return this;
        }

        public void Start()
        {
            if( _viewModel != null )
            {
                _viewModel.OnFilterMenuOpenChanged.Add(OnFilterMenuOpenChanged);
                _viewModel.OnAvailableBodiesChanged.Add(RefreshBodyCombo);
                _viewModel.OnSelectedBodyChanged.Add(RefreshBodyCombo);
                _viewModel.OnAvailableVesselTypesChanged.Add(RefreshTypeCombo);
                _viewModel.OnSelectedVesselTypeChanged.Add(RefreshTypeCombo);
                _viewModel.OnAvailableSituationsChanged.Add(RefreshSituationCombo);
                _viewModel.OnSelectedSituationChanged.Add(RefreshSituationCombo);
                _viewModel.OnFilterHasCommentChanged.Add(RefreshCheckbox);

                RefreshBodyCombo();
                RefreshTypeCombo();
                RefreshSituationCombo();
                RefreshCheckbox();
                OnFilterMenuOpenChanged();
            }

            if( _bodyCombo != null )
            {
                _bodyCombo.OnSelect.Add(OnBodySelected);
            }
            if( _typeCombo != null )
            {
                _typeCombo.OnSelect.Add(OnTypeSelected);
            }
            if( _situationCombo != null )
            {
                _situationCombo.OnSelect.Add(OnSituationSelected);
            }
        }

        public void OnDestroy()
        {
            if( _bodyCombo != null )
            {
                _bodyCombo.OnSelect.Remove(OnBodySelected);
            }
            if( _typeCombo != null )
            {
                _typeCombo.OnSelect.Remove(OnTypeSelected);
            }
            if( _situationCombo != null )
            {
                _situationCombo.OnSelect.Remove(OnSituationSelected);
            }

            if( _viewModel != null )
            {
                _viewModel.OnFilterMenuOpenChanged.Remove(OnFilterMenuOpenChanged);
                _viewModel.OnAvailableBodiesChanged.Remove(RefreshBodyCombo);
                _viewModel.OnSelectedBodyChanged.Remove(RefreshBodyCombo);
                _viewModel.OnAvailableVesselTypesChanged.Remove(RefreshTypeCombo);
                _viewModel.OnSelectedVesselTypeChanged.Remove(RefreshTypeCombo);
                _viewModel.OnAvailableSituationsChanged.Remove(RefreshSituationCombo);
                _viewModel.OnSelectedSituationChanged.Remove(RefreshSituationCombo);
                _viewModel.OnFilterHasCommentChanged.Remove(RefreshCheckbox);
            }
        }

        private void OnBodySelected(string body)
        {
            _viewModel.SelectedBody = body;
        }

        private void OnTypeSelected(string type)
        {
            _viewModel.SelectedVesselType = type;
        }

        private void OnSituationSelected(string situation)
        {
            _viewModel.SelectedSituation = situation;
        }

        private void OnFilterMenuOpenChanged()
        {
            bool open = _viewModel.FilterMenuOpen;
            if (_panel != null) _panel.SetActive(open);
            if (_trap != null) _trap.SetActive(open);

            if (open)
            {
                // Synchronise l'affichage à l'ouverture
                if (_search != null) _search.SetText(_viewModel.SearchText ?? string.Empty);
                RefreshBodyCombo();
                RefreshTypeCombo();
                RefreshSituationCombo();
                RefreshCheckbox();
            }
            else
            {
                _bodyCombo?.Collapse();
                _typeCombo?.Collapse();
                _situationCombo?.Collapse();
            }
        }

        private void RefreshBodyCombo()
        {
            _bodyCombo?.SetOptions(_viewModel.AvailableBodies, _viewModel.SelectedBody);
        }

        private void RefreshTypeCombo()
        {
            _typeCombo?.SetOptions(_viewModel.AvailableVesselTypes, _viewModel.SelectedVesselType);
        }

        private void RefreshSituationCombo()
        {
            _situationCombo?.SetOptions(_viewModel.AvailableSituations, _viewModel.SelectedSituation);
        }

        private void RefreshCheckbox()
        {
            if (_checkbox != null) _checkbox.SetChecked(_viewModel.FilterHasComment);
        }
    }
}
