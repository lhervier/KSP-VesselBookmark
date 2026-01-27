using KSP.Localization;
using KSP.UI.Screens;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using com.github.lhervier.ksp.bookmarksmod.bookmarks;
using com.github.lhervier.ksp.bookmarksmod.util;

namespace com.github.lhervier.ksp.bookmarksmod.ui {

    public class BookmarksListUI {
        private Rect _mainWindowRect = new Rect(100, 100, 500, 600);
        private int _mainWindowID;
        
        private Vector2 _scrollPosition = Vector2.zero;

        private UIStyles _uiStyles;

        public BookmarksListUIController Controller { get; private set; }
        private EditCommentUIController _editCommentUIController;
        private BookmarkUI _bookmarkUI;

        public EventVoid OnClosed = new EventVoid("BookmarksListUI.OnClosed");

        public BookmarksListUI() {
            _mainWindowID = UnityEngine.Random.Range(1000, 2000);
            Controller = new BookmarksListUIController();
        }

        public void Initialize(
            UIStyles uiStyles, 
            EditCommentUIController editCommentUIController, 
            BookmarkUI bookmarkUI) {
            _uiStyles = uiStyles;
            _editCommentUIController = editCommentUIController;
            _bookmarkUI = bookmarkUI;
        }

        public void OnGUI() {
            if (Controller.MainWindowsVisible) {
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
        }

        /// <summary>
        /// Draws window content
        /// </summary>
        private void DrawMainWindow(int windowID) {
            GUILayout.BeginVertical();
            
            // Header
            GUILayout.BeginHorizontal();
            var filteredCount = Controller.AvailableBookmarks.Count;
            GUILayout.Label(
                ModLocalization.GetString("labelBookmarks", filteredCount, Controller.AvailableBookmarks.Count),
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
                Controller.MainWindowsVisible = false;
                _editCommentUIController.CancelCommentEdition();
                this.OnClosed.Fire();
            }
            GUILayout.EndHorizontal();
            Rect line1Rect = GUILayoutUtility.GetLastRect();
            
            GUILayout.Space(5);
            
            // Filters section
            DrawFilters();
            
            GUILayout.Space(5);
            
            // Bookmarks list
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true);
            
            if (Controller.AvailableBookmarks.Count == 0) {
                GUILayout.Label(ModLocalization.GetString("labelNoBookmarks"), _uiStyles.LabelStyle);
            } else {
                for(int i = 0; i < Controller.AvailableBookmarks.Count; i++) {
                    Bookmark bookmark = Controller.AvailableBookmarks[i];
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
            string currentBodyName= Controller.GetSelectedBody();
            if (GUILayout.Button(currentBodyName, _uiStyles.ButtonStyle, GUILayout.Width(120))) {
                Controller.SelectNextBody();
            }
            
            GUILayout.Space(10);
            
             // Vessel Type filter
            GUILayout.Label(ModLocalization.GetString("labelType"), _uiStyles.LabelStyle, GUILayout.Width(50));
            
            // Create dropdown options for vessel type
            string currentVesselTypeName = Controller.GetSelectedVesselType();
            if (GUILayout.Button(currentVesselTypeName, _uiStyles.ButtonStyle, GUILayout.Width(100))) {
                Controller.SelectNextVesselType();
            }

            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button(ModLocalization.GetString("buttonClear"), _uiStyles.ButtonStyle, GUILayout.Width(60))) {
                Controller.ClearFilters();
            }
            
            GUILayout.EndHorizontal();
            
            GUILayout.EndVertical();
        }
    }
}