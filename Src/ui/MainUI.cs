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
        private Rect _mainWindowRect = new Rect(100, 100, 500, 600);
        private int _mainWindowID;
        
        private Vector2 _scrollPosition = Vector2.zero;
        
        private UIStyles _uiStyles;

        private MainUIController _mainUIController;
        private EditCommentUI _editCommentUI;
        private BookmarkUI _bookmarkUI;

        private void Awake() {
            _mainWindowID = UnityEngine.Random.Range(1000, 2000);
            GameEvents.onGUIApplicationLauncherReady.Add(OnLauncherReady);
            
            BookmarkManager.Instance.OnBookmarksUpdated.Add(OnBookmarksUpdated);
        }
        
        private void OnBookmarksUpdated() {
            ModLogger.LogDebug($"OnBookmarksUpdated");
            if( this._mainUIController != null ) {
                this._mainUIController.UpdateBookmarks();
            }
        }

        private void OnDestroy() {
            GameEvents.onGUIApplicationLauncherReady.Remove(OnLauncherReady);
            OnLauncherUnready();
            BookmarkManager.Instance.OnBookmarksUpdated.Remove(OnBookmarksUpdated);
            
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
            if( this._mainUIController != null ) {
                this._mainUIController.MainWindowsVisible = true;
            }
            if( this._editCommentUI != null ) {
                this._editCommentUI.Controller.CancelCommentEdition();
            }
            BookmarkManager.Instance.RefreshBookmarks();
        }
        
        /// <summary>
        /// Toolbar button click handler (deactivation)
        /// </summary>
        private void OnToggleOff() {
            if( this._mainUIController != null ) {
                this._mainUIController.MainWindowsVisible = false;
            }
            if( this._editCommentUI != null ) {
                this._editCommentUI.Controller.CancelCommentEdition();
            }
        }
        
        private void OnGUI() {
            // Initialise main UI controller
            if( this._mainUIController == null ) {
                this._mainUIController = new MainUIController();
            }

            // Initialise UI styles
            if( this._uiStyles == null ) {
                this._uiStyles = new UIStyles();
            }

            // Initialise edit comment UI
            if( this._editCommentUI == null ) {
                this._editCommentUI = new EditCommentUI(_uiStyles);
            }

            // Initialise bookmark UI
            if( this._bookmarkUI == null ) {
                this._bookmarkUI = new BookmarkUI(_uiStyles, this._mainUIController, this._editCommentUI.Controller);
            }

            // Window style
            GUI.skin = HighLogic.Skin;
            
            if (_mainUIController.MainWindowsVisible) {
                _mainWindowRect = GUILayout.Window(
                    _mainWindowID,
                    _mainWindowRect,
                    DrawMainWindow,
                    ModLocalization.GetString("windowTitle"),
                    GUILayout.MinWidth(500),
                    GUILayout.MinHeight(400)
                );
                
                // Prevent window from going off screen
                _mainWindowRect.x = Mathf.Clamp(_mainWindowRect.x, 0, Screen.width - _mainWindowRect.width);
                _mainWindowRect.y = Mathf.Clamp(_mainWindowRect.y, 0, Screen.height - _mainWindowRect.height);
            }
            
            this._editCommentUI.OnGUI();
        }
        
        /// <summary>
        /// Draws window content
        /// </summary>
        private void DrawMainWindow(int windowID) {
            GUILayout.BeginVertical();
            
            // Header
            GUILayout.BeginHorizontal();
            var filteredCount = _mainUIController.AvailableBookmarks.Count;
            GUILayout.Label(
                ModLocalization.GetString("labelBookmarks", filteredCount, _mainUIController.AvailableBookmarks.Count),
                _uiStyles.LabelStyle,
                GUILayout.ExpandWidth(true)
            );
            
            // Add bookmark button (for current active vessel)
            if (FlightGlobals.ActiveVessel != null) {
                if (GUILayout.Button(ModLocalization.GetString("buttonAdd"), _uiStyles.ButtonStyle, GUILayout.Width(80))) {
                
                    uint vesselPersistentID = FlightGlobals.ActiveVessel.persistentId;
                    
                    if (BookmarkManager.Instance.HasBookmark(BookmarkType.Vessel, vesselPersistentID)) {
                        ScreenMessages.PostScreenMessage(
                            ModLocalization.GetString("messageBookmarkAlreadyExists"),
                            2f,
                            ScreenMessageStyle.UPPER_CENTER
                        );
                    } else {
                        VesselBookmark bookmark = new VesselBookmark(vesselPersistentID);
                        if (BookmarkManager.Instance.AddBookmark(bookmark)) {
                            ScreenMessages.PostScreenMessage(
                                ModLocalization.GetString("messageBookmarkAdded"),
                                2f,
                                ScreenMessageStyle.UPPER_CENTER
                            );
                        }
                    }
                }
            }
            
            if (GUILayout.Button(ModLocalization.GetString("buttonRefresh"), _uiStyles.ButtonStyle, GUILayout.Width(80))) {
                BookmarkManager.Instance.RefreshBookmarks();
            }
            if (GUILayout.Button(ModLocalization.GetString("buttonClose"), _uiStyles.ButtonStyle, GUILayout.Width(80))) {
                _mainUIController.MainWindowsVisible = false;
                _editCommentUI.Controller.CancelCommentEdition();
                if (_toolbarButton != null) {
                    _toolbarButton.SetFalse();
                }
            }
            GUILayout.EndHorizontal();
            Rect line1Rect = GUILayoutUtility.GetLastRect();
            
            GUILayout.Space(5);
            
            // Filters section
            DrawFilters();
            
            GUILayout.Space(5);
            
            // Bookmarks list
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true);
            
            if (_mainUIController.AvailableBookmarks.Count == 0) {
                GUILayout.Label(ModLocalization.GetString("labelNoBookmarks"), _uiStyles.LabelStyle);
            } else {
                for(int i = 0; i < _mainUIController.AvailableBookmarks.Count; i++) {
                    Bookmark bookmark = _mainUIController.AvailableBookmarks[i];
                    this._bookmarkUI.OnGUI(bookmark, i);
                }
            }
            
            GUILayout.EndScrollView();
            
            GUILayout.EndVertical();
            
            // Display tooltip if any
            if (!string.IsNullOrEmpty(GUI.tooltip)) {
                Vector2 mousePos = Event.current.mousePosition;
                Vector2 tooltipSize = GUI.skin.box.CalcSize(new GUIContent(GUI.tooltip));
                Rect tooltipRect = new Rect(mousePos.x + 10, mousePos.y + 10, tooltipSize.x + 10, tooltipSize.y + 5);
                
                // Ensure tooltip stays on screen
                if (tooltipRect.xMax > Screen.width) {
                    tooltipRect.x = mousePos.x - tooltipRect.width - 10;
                }
                if (tooltipRect.yMax > Screen.height) {
                    tooltipRect.y = mousePos.y - tooltipRect.height - 10;
                }
                
                GUI.Box(tooltipRect, GUI.tooltip, _uiStyles.TooltipStyle);
            }
            
            // Allow window dragging
            GUI.DragWindow();
        }
        
        /// <summary>
        /// Draws filter controls
        /// </summary>
        private void DrawFilters() {
            GUILayout.BeginVertical("box");
            
            GUILayout.BeginHorizontal();
            
            // Body filter
            GUILayout.Label(ModLocalization.GetString("labelBody"), _uiStyles.LabelStyle, GUILayout.Width(50));
            
            // Create dropdown options for body
            string currentBodyName= _mainUIController.GetSelectedBody();
            if (GUILayout.Button(currentBodyName, _uiStyles.ButtonStyle, GUILayout.Width(120))) {
                _mainUIController.SelectNextBody();
            }
            
            GUILayout.Space(10);
            
             // Vessel Type filter
            GUILayout.Label(ModLocalization.GetString("labelType"), _uiStyles.LabelStyle, GUILayout.Width(50));
            
            // Create dropdown options for vessel type
            string currentVesselTypeName = _mainUIController.GetSelectedVesselType();
            if (GUILayout.Button(currentVesselTypeName, _uiStyles.ButtonStyle, GUILayout.Width(100))) {
                _mainUIController.SelectNextVesselType();
            }

            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button(ModLocalization.GetString("buttonClear"), _uiStyles.ButtonStyle, GUILayout.Width(60))) {
                _mainUIController.ClearFilters();
            }
            
            GUILayout.EndHorizontal();
            
            GUILayout.EndVertical();
        }
    }
}
