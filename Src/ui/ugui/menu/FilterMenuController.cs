using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.sprites;
using com.github.lhervier.ksp.shared.ugui.checkbox;
using com.github.lhervier.ksp.shared;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.menu
{
    public class FilterMenuController : MonoBehaviour
    {
        private const string SEARCH_LOCK_ID = "VesselBookmarkMod_Search";

        private EventTrigger.Entry _searchSelectedTriggers;
        private EventTrigger.Entry _searchDeselectedTriggers;

        // ===============================================
        // Life cycle
        // ===============================================

        private BookmarksViewModel _viewModel;
        public FilterMenuController ViewModel(BookmarksViewModel viewModel)
        {
            _viewModel = viewModel;
            return this;
        }

        private GameObject _panel;
        private GameObject _trap;
        public FilterMenuController PanelAndTrap(GameObject panel, GameObject trap) { 
            _panel = panel; 
            _trap = trap; 
            return this;
        }
        
        private InputField _search;
        private EventTrigger _searchTriggers;
        public FilterMenuController Search(InputField search, EventTrigger searchTriggers)
        {
            _search = search;
            _searchTriggers = searchTriggers;
            return this;
        }
        
        private CheckboxController _checkbox;
        public FilterMenuController Checkbox(CheckboxController checkbox) 
        {
            _checkbox = checkbox;
            return this;
        }
        
        private ComboBuilder.ComboController _bodyCombo;
        private ComboBuilder.ComboController _typeCombo;
        public FilterMenuController Combos(ComboBuilder.ComboController body, ComboBuilder.ComboController type)
        {
            _bodyCombo = body;
            _typeCombo = type;
            return this;
        }

        public void Start()
        {
            _viewModel.OnFilterMenuOpenChanged.Add(OnFilterMenuOpenChanged);
            _viewModel.OnAvailableBodiesChanged.Add(RefreshBodyCombo);
            _viewModel.OnSelectedBodyChanged.Add(RefreshBodyCombo);
            _viewModel.OnAvailableVesselTypesChanged.Add(RefreshTypeCombo);
            _viewModel.OnSelectedVesselTypeChanged.Add(RefreshTypeCombo);
            _viewModel.OnFilterHasCommentChanged.Add(RefreshCheckbox);

            // Verrou clavier au focus / déverrou au blur (comme l'overlay de commentaire)
            _searchSelectedTriggers = new EventTrigger.Entry { eventID = EventTriggerType.Select };
            _searchSelectedTriggers.callback.AddListener(OnSearchSelect);
            _searchTriggers.triggers.Add(_searchSelectedTriggers);
            
            _searchDeselectedTriggers = new EventTrigger.Entry { eventID = EventTriggerType.Deselect };
            _searchDeselectedTriggers.callback.AddListener(OnSearchDeselect);
            _searchTriggers.triggers.Add(_searchDeselectedTriggers);

            RefreshBodyCombo();
            RefreshTypeCombo();
            RefreshCheckbox();
            OnFilterMenuOpenChanged();
        }

        public void OnDestroy()
        {
            if( _searchTriggers != null )
            {
                _searchTriggers.triggers.Remove(_searchDeselectedTriggers);
                _searchTriggers.triggers.Remove(_searchSelectedTriggers);
            }

            if( _searchDeselectedTriggers != null )
            {
                _searchDeselectedTriggers.callback.RemoveListener(OnSearchDeselect);
            }
            if( _searchSelectedTriggers != null )
            {
                _searchSelectedTriggers.callback.RemoveListener(OnSearchSelect);
            }

            _viewModel?.OnFilterMenuOpenChanged.Remove(OnFilterMenuOpenChanged);
            _viewModel?.OnAvailableBodiesChanged.Remove(RefreshBodyCombo);
            _viewModel?.OnSelectedBodyChanged.Remove(RefreshBodyCombo);
            _viewModel?.OnAvailableVesselTypesChanged.Remove(RefreshTypeCombo);
            _viewModel?.OnSelectedVesselTypeChanged.Remove(RefreshTypeCombo);
            _viewModel?.OnFilterHasCommentChanged.Remove(RefreshCheckbox);
            InputLockManager.RemoveControlLock(SEARCH_LOCK_ID);
        }

        private void OnSearchSelect(BaseEventData _)
        {
            InputLockManager.SetControlLock(ControlTypes.All, SEARCH_LOCK_ID);
        }

        private void OnSearchDeselect(BaseEventData _)
        {
            InputLockManager.RemoveControlLock(SEARCH_LOCK_ID);
        }

        private void OnFilterMenuOpenChanged()
        {
            bool open = _viewModel.FilterMenuOpen;
            if (_panel != null) _panel.SetActive(open);
            if (_trap != null) _trap.SetActive(open);

            if (open)
            {
                // Synchronise l'affichage à l'ouverture
                if (_search != null) _search.text = _viewModel.SearchText ?? string.Empty;
                RefreshBodyCombo();
                RefreshTypeCombo();
                RefreshCheckbox();
            }
            else
            {
                _bodyCombo?.Collapse();
                _typeCombo?.Collapse();
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

        private void RefreshCheckbox()
        {
            if (_checkbox != null) _checkbox.SetChecked(_viewModel.FilterHasComment);
        }
    }
}
