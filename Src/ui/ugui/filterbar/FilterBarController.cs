using System.Collections.Generic;
using UnityEngine;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.filterbar
{
    /// <summary>
    /// Keeps the filter bar in sync with the active criteria: rebuilds one chip per criterion, and
    /// hides the whole bar when there is none.
    /// </summary>
    public class FilterBarController : MonoBehaviour
    {
        // ===============================================
        // Life cycle
        // ===============================================

        private BookmarksViewModel _viewModel;
        public FilterBarController WithViewModel(BookmarksViewModel viewModel)
        {
            this._viewModel = viewModel;
            return this;
        }

        private GameObject _panel;
        public FilterBarController WithPanel(GameObject panel)
        {
            this._panel = panel;
            return this;
        }

        private Transform _chipsContainer;
        public FilterBarController WithChipsContainer(Transform chipsContainer)
        {
            this._chipsContainer = chipsContainer;
            return this;
        }

        public void Start()
        {
            if( _viewModel != null )
            {
                _viewModel.OnFiltersChanged.Add(OnFiltersChanged);
            }
            Rebuild();
        }

        public void OnDestroy()
        {
            if( _viewModel != null )
            {
                _viewModel.OnFiltersChanged.Remove(OnFiltersChanged);
            }
        }

        public void OnEnable()
        {
            // AddComponent déclenche OnEnable AVANT que le builder n'ait câblé le controller : on ne
            // fait rien tant que ce n'est pas fait (Start s'en chargera).
            if( _viewModel == null || _chipsContainer == null )
            {
                return;
            }
            // Sinon, la fenêtre vient d'être rouverte : les critères ont pu changer entre-temps.
            Rebuild();
        }

        // ============================================
        // Methods bound to events
        // ============================================

        private void OnFiltersChanged()
        {
            // Inutile de reconstruire une fenêtre masquée : OnEnable s'en charge à la réouverture.
            if( !isActiveAndEnabled )
            {
                return;
            }
            Rebuild();
        }

        // =======================================
        // Internal Helpers
        // =======================================

        private void Rebuild()
        {
            for( int i = _chipsContainer.childCount - 1; i >= 0; i-- )
            {
                // Destroy() ne prend effet qu'en fin de frame : on désactive d'abord, sinon les
                // anciennes pastilles compteraient encore dans le flot et pourraient ajouter une ligne.
                GameObject chip = _chipsContainer.GetChild(i).gameObject;
                chip.SetActive(false);
                Destroy(chip);
            }

            List<FilterCriterion> criteria = _viewModel.ActiveCriteria;
            foreach( FilterCriterion criterion in criteria )
            {
                new FilterChipBuilder()
                    .WithViewModel(_viewModel)
                    .WithCriterion(criterion)
                    .WithParent(_chipsContainer)
                    .Build();
            }

            _panel.SetActive(criteria.Count > 0);
        }
    }
}
