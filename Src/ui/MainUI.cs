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
    /// User interface for managing bookmarks
    /// </summary>
    [KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
    public class MainUI : MonoBehaviour {

        private static readonly ModLogger LOGGER = new ModLogger("MainUI");

        private ApplicationLauncherButton _toolbarButton;
                
        private UIStyles _uiStyles;

        private BookmarksListUI _bookmarksListUI;
        private EditCommentUI _editCommentUI;
        private BookmarkUI _bookmarkUI;
        private BookmarksViewModel _viewModel;

        // Fenêtre uGUI (cohabite avec l'IMGUI pendant la migration ; pilotée par ViewModel.WindowVisible)
        private BookmarksWindow _uguiWindow;

        private void Start() {
            // Subscribe to events
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

            if( this._bookmarkUI != null ) {
                this._bookmarkUI.OnDestroy();
                this._bookmarkUI = null;
            }
            if( this._editCommentUI != null ) {
                this._editCommentUI.Controller.CancelCommentEdition();
                this._editCommentUI = null;
            }
            if( this._bookmarksListUI != null ) {
                this._bookmarksListUI.Controller.OnClosed.Remove(OnBookmarksListUIClosed);
                this._bookmarksListUI.OnDestroy();
                this._bookmarksListUI = null;
            }
            if( this._uiStyles != null ) {
                this._uiStyles = null;
            }
        }

        // ==========================================================================
        // EVENTS
        // ==========================================================================
        
        private void OnBookmarksListUIClosed() {
            _editCommentUI.Controller.CancelCommentEdition();
            if (_toolbarButton != null) {
                _toolbarButton.SetFalse();
            }
        }

        /// <summary>
        /// État de visibilité unifié : montre/cache la fenêtre uGUI et, à la fermeture, resynchronise
        /// le toolbar et annule une éventuelle édition de commentaire. La fenêtre IMGUI suit déjà
        /// puisqu'elle lit ViewModel.WindowVisible.
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
        /// La fenêtre uGUI a été fermée par KSP (Échap…) : on rabat l'état partagé, ce qui ferme
        /// aussi l'IMGUI et relâche le bouton du toolbar.
        /// </summary>
        private void OnUGUIWindowClosed() {
            _viewModel.WindowVisible = false;
        }

        // ==========================================================================
        // TOOLBAR
        // ==========================================================================

        /// <summary>
        /// Called when toolbar is ready
        /// </summary>
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
        
        /// <summary>
        /// Called when toolbar is no longer ready
        /// </summary>
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
        
        /// <summary>
        /// Toolbar button click handler (activation)
        /// </summary>
        private void OnToggleOn() {
            _viewModel.WindowVisible = true;
            BookmarkManager.RefreshBookmarks();
        }

        /// <summary>
        /// Toolbar button click handler (deactivation)
        /// </summary>
        private void OnToggleOff() {
            _viewModel.WindowVisible = false;
        }

        // ==========================================================================
        // UPDATE
        // ==========================================================================
        
        private void Update() {
            // Set scroll/zoom lock at start of frame so camera doesn't zoom when scrolling the bookmark list (KSP reads Input in Update, so OnGUI was too late)
            if (_bookmarksListUI == null || !_bookmarksListUI.Controller.MainWindowsVisible) {
                InputLockManager.RemoveControlLock(BookmarksListUI.SCROLL_LOCK_ID);
                return;
            }
            Rect r = _bookmarksListUI.MainWindowRect;
            Vector2 mouseScreen = new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y);
            if (r.Contains(mouseScreen)) {
                InputLockManager.SetControlLock(ControlTypes.CAMERAMODES | ControlTypes.CAMERACONTROLS, BookmarksListUI.SCROLL_LOCK_ID);
            } else {
                InputLockManager.RemoveControlLock(BookmarksListUI.SCROLL_LOCK_ID);
            }
        }

        private void OnGUI() {
            // Initialise external components
            if( this._uiStyles == null ) {
                this._uiStyles = new UIStyles();
            }
            if( this._editCommentUI == null ) {
                this._editCommentUI = new EditCommentUI(this._uiStyles, this._viewModel);
            }
            if( this._bookmarkUI == null ) {
                this._bookmarkUI = new BookmarkUI(this._uiStyles, this._viewModel);
            }
            if( this._bookmarksListUI == null ) {
                this._bookmarksListUI = new BookmarksListUI(
                    this._uiStyles, 
                    this._editCommentUI, 
                    this._bookmarkUI,
                    this._viewModel
                );
                this._bookmarksListUI.Controller.OnClosed.Add(OnBookmarksListUIClosed);
            }

            // Window style
            GUI.skin = HighLogic.Skin;
            
            this._bookmarksListUI.OnGUI();
            this._editCommentUI.OnGUI();
        }
    }
}
