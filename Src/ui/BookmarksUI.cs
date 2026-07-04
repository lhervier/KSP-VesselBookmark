using KSP.UI.Screens;
using UnityEngine;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui;
using com.github.lhervier.ksp.shared;

namespace com.github.lhervier.ksp.bookmarksmod.ui {

    /// <summary>
    /// Point d'entrée de l'UI : bouton toolbar + fenêtre uGUI, pilotés par ViewModel.WindowVisible.
    /// (L'ancienne UI IMGUI est débranchée ; ses fichiers restent présents le temps de finir le uGUI.)
    /// </summary>
    [KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
    public class BookmarksUI : MonoBehaviour {

        private static readonly ModLogger LOGGER = new ModLogger("MainUI");

        private ApplicationLauncherButton _toolbarButton;

        private BookmarksViewModel _viewModel;
        private BookmarksWindow _uguiWindow;
        private BookmarksSettings _settings;
        
        private void Start() {
            GameEvents.onGUIApplicationLauncherReady.Add(OnLauncherReady);

            _viewModel = this.gameObject.AddComponent<BookmarksViewModel>();
            _viewModel.Initialize(BookmarksManager.Instance);

            _settings = new BookmarksSettings();
            _settings.Load();

            // Restore the persisted search criteria into the view model before wiring the save
            // handler, so replaying the restored values does not trigger a save.
            if (_settings.HasCriteria) {
                _viewModel.SelectedBody = _settings.SelectedBody;
                _viewModel.SelectedVesselType = _settings.SelectedVesselType;
                _viewModel.SelectedSituation = _settings.SelectedSituation;
                _viewModel.SearchText = _settings.SearchText;
                _viewModel.FilterHasComment = _settings.FilterHasComment;
            }
            _viewModel.OnSelectedBodyChanged.Add(OnCriteriaChanged);
            _viewModel.OnSelectedVesselTypeChanged.Add(OnCriteriaChanged);
            _viewModel.OnSelectedSituationChanged.Add(OnCriteriaChanged);
            _viewModel.OnSearchTextChanged.Add(OnCriteriaChanged);
            _viewModel.OnFilterHasCommentChanged.Add(OnCriteriaChanged);

            _uguiWindow = new BookmarksWindow().WithViewModel(_viewModel);
            _uguiWindow.OnClosed.Add(OnUGUIWindowClosed);
            
            _viewModel.OnWindowVisibleChanged.Add(OnWindowVisibleChanged);
            _viewModel.OnWindowVisibleChanged.Add(OnWindowVisibleChangedPersist);

            // Restore the persisted open/closed state : setting WindowVisible replays through
            // OnWindowVisibleChanged, spawning the window if it was open when KSP last quit.
            if (_settings.HasWindowVisible && _settings.WindowVisible) {
                _viewModel.WindowVisible = true;
                _viewModel.ForceReload();
            }
        }

        private void OnDestroy() {
            GameEvents.onGUIApplicationLauncherReady.Remove(OnLauncherReady);
            OnLauncherUnready();

            if( this._viewModel != null ) {
                this._viewModel.OnWindowVisibleChanged.Remove(OnWindowVisibleChanged);
                this._viewModel.OnWindowVisibleChanged.Remove(OnWindowVisibleChangedPersist);
                this._viewModel.OnSelectedBodyChanged.Remove(OnCriteriaChanged);
                this._viewModel.OnSelectedVesselTypeChanged.Remove(OnCriteriaChanged);
                this._viewModel.OnSelectedSituationChanged.Remove(OnCriteriaChanged);
                this._viewModel.OnSearchTextChanged.Remove(OnCriteriaChanged);
                this._viewModel.OnFilterHasCommentChanged.Remove(OnCriteriaChanged);
            }
            if( this._uguiWindow != null ) {
                this._uguiWindow.OnClosed.Remove(OnUGUIWindowClosed);
                this._uguiWindow.Destroy();
                this._uguiWindow = null;
            }
        }

        // ==========================================================================
        // VISIBILITY
        // ==========================================================================

        /// <summary>
        /// Montre/cache la fenêtre uGUI et, à la fermeture, resynchronise le toolbar et annule une
        /// éventuelle édition de commentaire.
        /// </summary>
        private void OnWindowVisibleChanged() {
            bool visible = _viewModel.WindowVisible;
            if (_uguiWindow != null) {
                if (visible) {
                    _uguiWindow.Show();
                } else {
                    _uguiWindow.Hide();
                }
            }
            // Keep the toolbar button pressed state in sync, notably when visibility is driven
            // programmatically (restore at scene load) rather than by a click on the button.
            // SetTrue/SetFalse(false) : do not re-fire the toggle callbacks.
            if (_toolbarButton != null) {
                if (visible) {
                    _toolbarButton.SetTrue(false);
                } else {
                    _toolbarButton.SetFalse(false);
                }
            }
            if (!visible) {
                _viewModel.CancelBookmarkCommentEdition();
            }
        }

        /// <summary>
        /// La visibilité a changé : on mémorise l'état d'ouverture dans les réglages globaux.
        /// </summary>
        private void OnWindowVisibleChangedPersist() {
            _settings.SetWindowVisible(_viewModel.WindowVisible);
            _settings.Save();
        }

        /// <summary>
        /// La fenêtre uGUI a été fermée par KSP (Échap…) : on rabat l'état partagé, ce qui relâche
        /// le bouton du toolbar.
        /// </summary>
        private void OnUGUIWindowClosed() {
            _viewModel.WindowVisible = false;
        }

        /// <summary>
        /// A search criterion changed : memorize the whole current criteria set into the global settings.
        /// </summary>
        private void OnCriteriaChanged() {
            _settings.SetCriteria(
                _viewModel.SelectedBody,
                _viewModel.SelectedVesselType,
                _viewModel.SelectedSituation,
                _viewModel.SearchText,
                _viewModel.FilterHasComment);
            _settings.Save();
        }

        // ==========================================================================
        // TOOLBAR
        // ==========================================================================

        private void OnLauncherReady() {
            if (_toolbarButton == null) {
                try {
                    _toolbarButton = ApplicationLauncher.Instance.AddModApplication(
                        OnToggleOn,
                        OnToggleOff,
                        null,
                        null,
                        null,
                        null,
                        ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW | ApplicationLauncher.AppScenes.TRACKSTATION | ApplicationLauncher.AppScenes.SPACECENTER,
                        GameDatabase.Instance.GetTexture("VesselBookmarkMod/icon", false) ?? Texture2D.whiteTexture
                    );
                } catch (System.Exception e) {
                    LOGGER.LogError($"Error creating Toolbar button: {e.Message}");
                }
                // The launcher may become ready after the window state was restored at scene load :
                // press the button now to reflect an already-open window (false : no callback).
                if (_toolbarButton != null && _viewModel != null && _viewModel.WindowVisible) {
                    _toolbarButton.SetTrue(false);
                }
            }
        }

        private void OnLauncherUnready() {
            if (_toolbarButton != null) {
                try {
                    ApplicationLauncher.Instance.RemoveModApplication(_toolbarButton);
                } catch (System.Exception e) {
                    LOGGER.LogError($"Error removing Toolbar button: {e.Message}");
                }
                _toolbarButton = null;
            }
        }

        private void OnToggleOn() {
            _viewModel.WindowVisible = true;
            _viewModel.ForceReload();
        }

        private void OnToggleOff() {
            _viewModel.WindowVisible = false;
        }
    }
}
