using ClickThroughFix;
using KSP.Localization;
using KSP.UI.Screens;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using com.github.lhervier.ksp.bookmarksmod.bookmarks;
using com.github.lhervier.ksp.bookmarksmod.util;
using com.github.lhervier.ksp.bookmarksmod;

namespace com.github.lhervier.ksp.bookmarksmod.ui {

    public class BookmarksListUI {
        public const string SCROLL_LOCK_ID = "VesselBookmarkMod_ScrollBlock";

        /// <summary>Last known window rect (screen space, top-left origin). Used so Update() can set scroll lock before camera reads input.</summary>
        private Rect _mainWindowRect = new Rect(100, 100, 500, 600);
        public Rect MainWindowRect => _mainWindowRect;

        private int _mainWindowID;
        
        private Vector2 _scrollPosition = Vector2.zero;

        private UIStyles _uiStyles;

        private static readonly object _bodyCaller = new object();
        private static readonly object _vesselTypeCaller = new object();

        public BookmarksListUIController Controller { get; private set; }
        private EditCommentUIController _editCommentUIController;
        private BookmarkUI _bookmarkUI;
        private VesselBookmarkButton _clearFiltersButton;
        private VesselBookmarkButton _addButton;
        private VesselBookmarkButton _refreshButton;
        private VesselBookmarkButton _closeButton;

        public EventVoid OnClosed = new EventVoid("BookmarksListUI.OnClosed");

        public BookmarksListUI() {
            _mainWindowID = UnityEngine.Random.Range(1000, 2000);
            Controller = new BookmarksListUIController();
            _clearFiltersButton = VesselBookmarkButton.Builder()
                .WithLabel(ModLocalization.GetString("buttonClear"))
                .WithTooltip(ModLocalization.GetString("buttonClear"))
                .WithIconSize(20, 20)
                .Build();
            _addButton = VesselBookmarkButton.Builder()
                .WithLabel(ModLocalization.GetString("buttonAdd"))
                .WithTooltip(ModLocalization.GetString("buttonAdd"))
                .WithIconSize(20, 20)
                .Build();
            _refreshButton = VesselBookmarkButton.Builder()
                .WithLabel(ModLocalization.GetString("buttonRefresh"))
                .WithTooltip(ModLocalization.GetString("buttonRefresh"))
                .WithIconSize(20, 20)
                .Build();
            _closeButton = VesselBookmarkButton.Builder()
                .WithLabel(ModLocalization.GetString("buttonClose"))
                .WithTooltip(ModLocalization.GetString("buttonClose"))
                .WithIconSize(20, 20)
                .Build();
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
                Controller.ProcessSearchDebounce();
                _mainWindowRect = ClickThruBlocker.GUILayoutWindow(
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

                ComboBox.DrawGUI(_uiStyles.ComboPopupStyle, _uiStyles.ComboGridStyle, _uiStyles.ComboGridSelectedStyle);

                // Consume scroll event for IMGUI (scroll list + prevent some handlers from zooming)
                Vector2 mousePos = Event.current.mousePosition;
                if (Event.current.type == EventType.ScrollWheel && _mainWindowRect.Contains(mousePos)) {
                    Event.current.Use();
                }
            } else {
                InputLockManager.RemoveControlLock(SCROLL_LOCK_ID);
                return;
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
            System.Action addAction = Controller.CanAddVesselBookmark() ? (System.Action)Controller.AddVesselBookmark : null;
            _addButton.Draw(() => true, addAction);

            _refreshButton.Draw(() => true, () => BookmarkManager.RefreshBookmarks());

            _closeButton.Draw(() => true, () => {
                _editCommentUIController.CancelCommentEdition();
                Controller.MainWindowsVisible = false;
                this.OnClosed.Fire();
            });

            GUILayout.EndHorizontal();
            Rect line1Rect = GUILayoutUtility.GetLastRect();
            
            GUILayout.Space(5);
            
            // Filters section
            DrawFilters();
            
            GUILayout.Space(5);

            GUILayout.Space(5);
            
            if( Controller.AvailableBookmarks.Count == 0 ) {
                GUILayout.Label(ModLocalization.GetString("labelNoBookmarks"), _uiStyles.LabelStyle);
            } else {
                _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true);
                
                GUILayout.Label(ModLocalization.GetString("labelAddCommandModuleBookmark"), _uiStyles.LabelStyle);
                if( Controller.AvailableBookmarks.ContainsKey(BookmarkType.CommandModule) ) {
                    for(int i = 0; i < Controller.AvailableBookmarks[BookmarkType.CommandModule].Count; i++) {
                        Bookmark bookmark = Controller.AvailableBookmarks[BookmarkType.CommandModule][i];
                        this._bookmarkUI.OnGUI(bookmark, i);
                    }
                }

                GUILayout.Label(ModLocalization.GetString("labelAddVesselBookmark"), _uiStyles.LabelStyle);
                if( Controller.AvailableBookmarks.ContainsKey(BookmarkType.Vessel) ) {
                    for(int i = 0; i < Controller.AvailableBookmarks[BookmarkType.Vessel].Count; i++) {
                        Bookmark bookmark = Controller.AvailableBookmarks[BookmarkType.Vessel][i];
                        this._bookmarkUI.OnGUI(bookmark, i);
                    }
                }

                GUILayout.EndScrollView();
            }
            
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
            Controller.SelectedBody = ComboBox.Box(
                Controller.SelectedBody, 
                Controller.AvailableBodies, 
                _bodyCaller, 
                _uiStyles.ButtonStyle, 
                false
            );
            
            GUILayout.Space(10);
            
             // Vessel Type filter
            GUILayout.Label(ModLocalization.GetString("labelType"), _uiStyles.LabelStyle, GUILayout.Width(50));
            
            // Create dropdown options for vessel type
            Controller.SelectedVesselType = ComboBox.Box(
                Controller.SelectedVesselType, 
                Controller.AvailableVesselTypes, 
                _vesselTypeCaller, 
                _uiStyles.ButtonStyle, 
                false
            );

            GUILayout.Space(10);

            // Filter: bookmarks with comment — libellé puis case, les deux cliquables
            string filterWithCommentLabel = ModLocalization.GetString("labelFilterWithComment");
            GUILayout.Label(filterWithCommentLabel, _uiStyles.LabelStyle);
            Rect labelRect = GUILayoutUtility.GetLastRect();
            if (Event.current.type == EventType.MouseDown && labelRect.Contains(Event.current.mousePosition)) {
                Controller.FilterHasComment = !Controller.FilterHasComment;
                Event.current.Use();
            }
            
            Controller.FilterHasComment = GUILayout.Toggle(Controller.FilterHasComment, "", _uiStyles.ToggleStyle);
            
            GUILayout.FlexibleSpace();
            
            _clearFiltersButton.Draw(() => true, Controller.ClearFilters);
            
            GUILayout.EndHorizontal();
            
            // Zone de saisie de texte
            GUILayout.BeginHorizontal();
            GUILayout.Space(50);
            Controller.SearchText = GUILayout.TextField(Controller.SearchText, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
            
            GUILayout.EndVertical();
        }
    }
}