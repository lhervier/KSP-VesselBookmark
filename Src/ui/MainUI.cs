using KSP.Localization;
using KSP.UI.Screens;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using com.github.lhervier.ksp.bookmarksmod.bookmarks;
using com.github.lhervier.ksp.bookmarksmod.util;

namespace com.github.lhervier.ksp.bookmarksmod.ui {
    
    /// <summary>
    /// User interface for managing bookmarks
    /// </summary>
    [KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
    public class MainUI : MonoBehaviour {
        
        private ApplicationLauncherButton _toolbarButton;
                
        private UIStyles _uiStyles;

        private BookmarksListUI _bookmarksListUI;
        private EditCommentUI _editCommentUI;
        private BookmarkUI _bookmarkUI;

        private void Awake() {
            GameEvents.onGUIApplicationLauncherReady.Add(OnLauncherReady);
            BookmarkManager.OnBookmarksUpdated.Add(OnBookmarksUpdated);
        }
        
        private void OnBookmarksUpdated() {
            ModLogger.LogDebug($"OnBookmarksUpdated");
            if( this._bookmarksListUI != null ) {
                this._bookmarksListUI.Controller.UpdateBookmarksSelection();
            }
        }

        private void OnDestroy() {
            GameEvents.onGUIApplicationLauncherReady.Remove(OnLauncherReady);
            OnLauncherUnready();
            BookmarkManager.OnBookmarksUpdated.Remove(OnBookmarksUpdated);
            
            if( this._bookmarkUI != null ) {
                this._bookmarkUI.OnDestroy();
            }
        }
        
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
                    ModLogger.LogError($"Error creating Toolbar button: {e.Message}");
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
                    ModLogger.LogError($"Error removing Toolbar button: {e.Message}");
                }
                _toolbarButton = null;
            }
        }
        
        /// <summary>
        /// Toolbar button click handler (activation)
        /// </summary>
        private void OnToggleOn() {
            if( this._bookmarksListUI != null ) {
                this._bookmarksListUI.Controller.MainWindowsVisible = true;
            }
            if( this._editCommentUI != null ) {
                this._editCommentUI.Controller.CancelCommentEdition();
            }
            BookmarkManager.RefreshBookmarksInAnyInstance();
        }
        
        /// <summary>
        /// Toolbar button click handler (deactivation)
        /// </summary>
        private void OnToggleOff() {
            if( this._bookmarksListUI != null ) {
                this._bookmarksListUI.Controller.MainWindowsVisible = false;
            }
            if( this._editCommentUI != null ) {
                this._editCommentUI.Controller.CancelCommentEdition();
            }
        }
        
        private void OnGUI() {
            // Initialise external components
            if( this._uiStyles == null ) {
                this._uiStyles = new UIStyles();
            }
            if( this._editCommentUI == null ) {
                this._editCommentUI = new EditCommentUI();
            }
            if( this._bookmarkUI == null ) {
                this._bookmarkUI = new BookmarkUI();
            }
            if( this._bookmarksListUI == null ) {
                this._bookmarksListUI = new BookmarksListUI();
                this._bookmarksListUI.OnClosed.Add(() => {
                    if (_toolbarButton != null) {
                        _toolbarButton.SetFalse();
                    }
                });
            }

            // Inject dependencies
            this._bookmarksListUI.Initialize(
                this._uiStyles, 
                this._editCommentUI.Controller, 
                this._bookmarkUI
            );
            this._editCommentUI.Initialize(this._uiStyles);
            this._bookmarkUI.Initialize(
                this._uiStyles, 
                this._bookmarksListUI.Controller, 
                this._editCommentUI.Controller
            );

            // Window style
            GUI.skin = HighLogic.Skin;
            
            this._bookmarksListUI.OnGUI();
            this._editCommentUI.OnGUI();
        }
    }
}
