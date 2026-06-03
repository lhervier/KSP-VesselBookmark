using KSP.Localization;
using KSP.UI.Screens;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using com.github.lhervier.ksp.bookmarksmod.bookmarks;
using com.github.lhervier.ksp.bookmarksmod.util;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui;

namespace com.github.lhervier.ksp.bookmarksmod.ui {

    /// <summary>
    /// Point d'entrée de l'UI : bouton toolbar + fenêtre uGUI, pilotés par ViewModel.WindowVisible.
    /// (L'ancienne UI IMGUI est débranchée ; ses fichiers restent présents le temps de finir le uGUI.)
    /// </summary>
    [KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
    public class MainUI : MonoBehaviour {

        private static readonly ModLogger LOGGER = new ModLogger("MainUI");

        private ApplicationLauncherButton _toolbarButton;

        private BookmarksViewModel _viewModel;
        private BookmarksWindow _uguiWindow;

        private void Start() {
            GameEvents.onGUIApplicationLauncherReady.Add(OnLauncherReady);

            _viewModel = this.gameObject.AddComponent<BookmarksViewModel>();

            _uguiWindow = new BookmarksWindow();
            _uguiWindow.Initialize(_viewModel);
            _uguiWindow.OnClosed.Add(OnUGUIWindowClosed);
            _viewModel.OnWindowVisibleChanged.Add(OnWindowVisibleChanged);
        }

        private void OnDestroy() {
            GameEvents.onGUIApplicationLauncherReady.Remove(OnLauncherReady);
            OnLauncherUnready();

            if( this._viewModel != null ) {
                this._viewModel.OnWindowVisibleChanged.Remove(OnWindowVisibleChanged);
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
            if (!visible) {
                _viewModel.CancelBookmarkCommentEdition();
                if (_toolbarButton != null) {
                    _toolbarButton.SetFalse();
                }
            }
        }

        /// <summary>
        /// La fenêtre uGUI a été fermée par KSP (Échap…) : on rabat l'état partagé, ce qui relâche
        /// le bouton du toolbar.
        /// </summary>
        private void OnUGUIWindowClosed() {
            _viewModel.WindowVisible = false;
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
            BookmarkManager.RefreshBookmarks();
        }

        private void OnToggleOff() {
            _viewModel.WindowVisible = false;
        }
    }
}
