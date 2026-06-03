using UnityEngine;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui
{
    /// <summary>
    /// Classe de base des controllers uGUI : un MonoBehaviour qui détient le ViewModel partagé.
    /// Initialize() est appelé juste après AddComponent (donc avant Start), ce qui garantit que
    /// ViewModel est disponible quand les controllers s'abonnent à ses événements dans Start().
    /// </summary>
    public abstract class BaseController : MonoBehaviour
    {
        protected BookmarksViewModel ViewModel => _viewModel;
        private BookmarksViewModel _viewModel;

        public void Initialize(BookmarksViewModel viewModel)
        {
            this._viewModel = viewModel;
        }
    }
}
