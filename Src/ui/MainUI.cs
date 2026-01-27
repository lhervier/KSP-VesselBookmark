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
        
        private static readonly int BUTTON_HEIGHT = 20;
        private static readonly int BUTTON_WIDTH = 20;

        private ApplicationLauncherButton _toolbarButton;
        private Rect _mainWindowRect = new Rect(100, 100, 500, 600);
        private int _mainWindowID;
        
        private Vector2 _scrollPosition = Vector2.zero;
        
        // Icon cache
        private Dictionary<VesselType, VesselBookmarkButton> _vesselTypeButtons = new Dictionary<VesselType, VesselBookmarkButton>();
        private VesselBookmarkButton _removeButton;
        private VesselBookmarkButton _moveUpButton;
        private VesselBookmarkButton _moveDownButton;
        private VesselBookmarkButton _goToButton;
        private VesselBookmarkButton _editButton;
        
        // Textures for active vessel highlighting
        private Texture2D _activeVesselBackground;
        private Texture2D _activeVesselBorder;

        private UIStyles _uiStyles;

        private MainUIController _mainUIController;
        private EditCommentUI _editCommentUI;

        private void Awake() {
            _mainWindowID = UnityEngine.Random.Range(1000, 2000);
            GameEvents.onGUIApplicationLauncherReady.Add(OnLauncherReady);
            
            // Initialize vessel type buttons
            _vesselTypeButtons[VesselType.Base] = new VesselBookmarkButton("VesselBookmarkMod/vessel_types/base", null, BUTTON_WIDTH, BUTTON_HEIGHT);
            _vesselTypeButtons[VesselType.Debris] = new VesselBookmarkButton("VesselBookmarkMod/vessel_types/debris", null, BUTTON_WIDTH, BUTTON_HEIGHT);
            _vesselTypeButtons[VesselType.Lander] = new VesselBookmarkButton("VesselBookmarkMod/vessel_types/lander", null, BUTTON_WIDTH, BUTTON_HEIGHT);
            _vesselTypeButtons[VesselType.Plane] = new VesselBookmarkButton("VesselBookmarkMod/vessel_types/plane", null, BUTTON_WIDTH, BUTTON_HEIGHT);
            _vesselTypeButtons[VesselType.Probe] = new VesselBookmarkButton("VesselBookmarkMod/vessel_types/probe", null, BUTTON_WIDTH, BUTTON_HEIGHT);
            _vesselTypeButtons[VesselType.Relay] = new VesselBookmarkButton("VesselBookmarkMod/vessel_types/relay", null, BUTTON_WIDTH, BUTTON_HEIGHT);
            _vesselTypeButtons[VesselType.Rover] = new VesselBookmarkButton("VesselBookmarkMod/vessel_types/rover", null, BUTTON_WIDTH, BUTTON_HEIGHT);
            _vesselTypeButtons[VesselType.Ship] = new VesselBookmarkButton("VesselBookmarkMod/vessel_types/ship", null, BUTTON_WIDTH, BUTTON_HEIGHT);
            _vesselTypeButtons[VesselType.Station] = new VesselBookmarkButton("VesselBookmarkMod/vessel_types/station", null, BUTTON_WIDTH, BUTTON_HEIGHT);
            
            // Initialize remove button
            _removeButton = new VesselBookmarkButton(
                "VesselBookmarkMod/buttons/remove",
                ModLocalization.GetString("tooltipRemove"), 
                BUTTON_WIDTH, 
                BUTTON_HEIGHT
            );
            
            // Initialize move up button
            _moveUpButton = new VesselBookmarkButton(
                "VesselBookmarkMod/buttons/up",
                ModLocalization.GetString("tooltipMoveUp"), 
                BUTTON_WIDTH, 
                BUTTON_HEIGHT
            );
            
            // Initialize move down button
            _moveDownButton = new VesselBookmarkButton(
                "VesselBookmarkMod/buttons/down",
                ModLocalization.GetString("tooltipMoveDown"), 
                BUTTON_WIDTH, 
                BUTTON_HEIGHT
            );
            
            // Initialize go to button
            _goToButton = new VesselBookmarkButton(
                "VesselBookmarkMod/buttons/switch",
                ModLocalization.GetString("tooltipGoTo"), 
                BUTTON_WIDTH, 
                BUTTON_HEIGHT
            );
            
            // Initialize edit button
            _editButton = new VesselBookmarkButton(
                "VesselBookmarkMod/buttons/edit",
                ModLocalization.GetString("tooltipEdit"), 
                BUTTON_WIDTH, 
                BUTTON_HEIGHT
            );

            // Initialize textures for active vessel highlighting
            InitializeActiveVesselTextures();

            BookmarkManager.Instance.OnBookmarksUpdated.Add(OnBookmarksUpdated);
        }
        
        /// <summary>
        /// Initializes textures for highlighting active vessel bookmarks
        /// </summary>
        private void InitializeActiveVesselTextures() {
            // Background texture (slightly tinted blue-green)
            _activeVesselBackground = new Texture2D(1, 1);
            _activeVesselBackground.SetPixel(0, 0, new Color(0.15f, 0.25f, 0.3f, 0.6f)); // Bleu-vert foncé avec transparence
            _activeVesselBackground.Apply();
            
            // Border texture (bright blue-green)
            _activeVesselBorder = new Texture2D(1, 1);
            _activeVesselBorder.SetPixel(0, 0, new Color(0.2f, 0.6f, 0.8f, 1f)); // Bleu-vert clair
            _activeVesselBorder.Apply();
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
            
            // Clean up textures
            if (_activeVesselBackground != null) {
                UnityEngine.Object.Destroy(_activeVesselBackground);
            }
            if (_activeVesselBorder != null) {
                UnityEngine.Object.Destroy(_activeVesselBorder);
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
                    DrawBookmarkItem(bookmark, i);
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
        
        /// <summary>
        /// Draws a bookmark item
        /// </summary>
        private void DrawBookmarkItem(Bookmark bookmark, int currentIndex) {
            string bookmarkName;
            if( !string.IsNullOrEmpty(bookmark.GetBookmarkDisplayName())) {
                bookmarkName = bookmark.GetBookmarkDisplayName();
            } else {
                bookmarkName = ModLocalization.GetString("labelModuleNotFound");
            }

            string comment = bookmark.Comment;
            
            bool isHovered = _mainUIController.HoveredBookmarkID == bookmark.GetBookmarkID();
            
            // Check if this bookmark corresponds to the active vessel
            bool isActiveVessel = false;
            if (FlightGlobals.ActiveVessel != null) {
                isActiveVessel = bookmark.VesselPersistentID == FlightGlobals.ActiveVessel.persistentId;
            }

            // Create custom box style with appropriate background
            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            
            if (isActiveVessel) {
                // Active vessel: use colored background
                boxStyle.normal.background = _activeVesselBackground;
                // Add border by setting border values
                boxStyle.border = new RectOffset(2, 2, 2, 2);
            } else if (isHovered) {
                // Hovered bookmark: use hover background
                Texture2D hoverBg = new Texture2D(1, 1);
                hoverBg.SetPixel(0, 0, new Color(0.3f, 0.3f, 0.3f, 1f));
                hoverBg.Apply();
                boxStyle.normal.background = hoverBg;
            }
            
            GUILayout.BeginVertical(boxStyle);
            
            // Line 1: Icon, Module name, and action buttons (aligned right)
            GUILayout.BeginHorizontal();
            
            // Vessel type icon
            VesselBookmarkButton vesselTypeButton = GetVesselTypeButton(bookmark.GetBookmarkDisplayType());
            vesselTypeButton.Draw(
                () => vesselTypeButton != null,
                null
            );
            
            // Bookmark name
            GUILayout.Label($"<b>{bookmarkName} ({bookmark.GetBookmarkType()})</b>", _uiStyles.LabelStyle, GUILayout.Width(150));
            GUILayout.FlexibleSpace();
            
            bool canMoveUp = currentIndex > 0;
            bool canMoveDown = currentIndex < _mainUIController.AvailableBookmarks.Count - 1;
                
            // Small spacing before buttons
            GUILayout.Space(5);
            
            // Edit button
            _editButton.Draw(
                () => isHovered,
                () => {
                    _editCommentUI.Controller.EditComment(bookmark);
                }
            );
            
            GUILayout.Space(3);

            // Go to button (disabled if this is the active vessel)
            System.Action goToAction = null;
            if (!isActiveVessel) {
                goToAction = () => {
                    Vessel vessel = bookmark.GetVessel();
                    if( vessel == null ) {
                        ModLogger.LogWarning($"Bookmark {bookmark.GetBookmarkID()}: Vessel not found");
                        return;
                    }
                    if (VesselNavigator.NavigateToVessel(vessel)) {
                        _mainUIController.MainWindowsVisible = false;
                        _editCommentUI.Controller.CancelCommentEdition();
                        if (_toolbarButton != null) {
                            _toolbarButton.SetFalse();
                        }
                    }
                };
            }
            _goToButton.Draw(() => isHovered, goToAction);
                
            GUILayout.Space(3);

            // Move up button
            System.Action moveUpAction = null;
            if( canMoveUp ) {
                moveUpAction = () => {
                    Bookmark previousBookmark = _mainUIController.AvailableBookmarks[currentIndex - 1];
                    BookmarkManager.Instance.SwapBookmarks(
                        bookmark, 
                        previousBookmark
                    );
                };
            }
            _moveUpButton.Draw(
                () => isHovered,
                moveUpAction
            );
        
            // Move down button
            System.Action moveDownAction = null;
            if( canMoveDown ) {
                moveDownAction = () => {
                    Bookmark nextBookmark = _mainUIController.AvailableBookmarks[currentIndex + 1];
                    BookmarkManager.Instance.SwapBookmarks(
                        bookmark, 
                        nextBookmark
                    );
                };
            }
            _moveDownButton.Draw(
                () => isHovered,
                moveDownAction
            );
            
            GUILayout.Space(3);
            
            // Remove button
            _removeButton.Draw(
                () => isHovered,
                () => {
                    // Close main window temporarily to ensure confirmation dialog appears on top
                    bool wasMainWindowVisible = _mainUIController.MainWindowsVisible;
                    _mainUIController.MainWindowsVisible = false;
                    
                    VesselBookmarkUIDialog.ConfirmRemoval(
                        () => {
                            BookmarkManager.Instance.RemoveBookmark(bookmark);
                            _mainUIController.MainWindowsVisible = wasMainWindowVisible;
                        },
                        () => {
                            _mainUIController.MainWindowsVisible = wasMainWindowVisible;
                        }
                    );
                }
            );
            
            GUILayout.EndHorizontal();
            Rect line1Rect = GUILayoutUtility.GetLastRect();

            // Line 2: Situation and vessel name (if different from command module name)
            GUILayout.BeginHorizontal();
            GUILayout.Space(BUTTON_WIDTH + 4);
            
            // Vessel situation
            if (!string.IsNullOrEmpty(bookmark.VesselSituation)) {
                GUILayout.Label(bookmark.VesselSituation, _uiStyles.LabelStyle, GUILayout.Width(150));
            } else {
                GUILayout.Label(ModLocalization.GetString("labelUnknownSituation"), _uiStyles.LabelStyle, GUILayout.Width(150));
            }

            // Bookmark is prt of 
            if( bookmark.ShouldDrawPartOf() ) {
                GUILayout.Label(ModLocalization.GetString("labelPartOf", bookmark.VesselName), _uiStyles.LabelStyle);
            }
            
            GUILayout.EndHorizontal();
            Rect line2Rect = GUILayoutUtility.GetLastRect();

            // Line 3: Comment
            if( !string.IsNullOrEmpty(comment) ) {
            GUILayout.BeginHorizontal();
                GUILayout.Space(BUTTON_WIDTH + 4);
                GUILayout.Label(comment, _uiStyles.LabelStyle, GUILayout.ExpandWidth(true));
                GUILayout.EndHorizontal();
            }
            Rect line3Rect = GUILayoutUtility.GetLastRect();
            
            // Get the rect of the bookmark area BEFORE closing it
            Rect bookmarkRect = Rect.MinMaxRect(
                Mathf.Min(Mathf.Min(line1Rect.xMin, line2Rect.xMin), line3Rect.xMin),
                Mathf.Min(Mathf.Min(line1Rect.yMin, line2Rect.yMin), line3Rect.yMin),
                Mathf.Max(Mathf.Max(line1Rect.xMax, line2Rect.xMax), line3Rect.xMax),
                Mathf.Max(Mathf.Max(line1Rect.yMax, line2Rect.yMax), line3Rect.yMax)
            );
            
            GUILayout.EndVertical();
            
            // Draw border for active vessel bookmark
            if (isActiveVessel && Event.current.type == EventType.Repaint) {
                // Draw border using lines
                int borderWidth = 2;
                
                // Top border
                GUI.DrawTexture(new Rect(bookmarkRect.x, bookmarkRect.y, bookmarkRect.width, borderWidth), _activeVesselBorder);
                // Bottom border
                GUI.DrawTexture(new Rect(bookmarkRect.x, bookmarkRect.yMax - borderWidth, bookmarkRect.width, borderWidth), _activeVesselBorder);
                // Left border
                GUI.DrawTexture(new Rect(bookmarkRect.x, bookmarkRect.y, borderWidth, bookmarkRect.height), _activeVesselBorder);
                // Right border
                GUI.DrawTexture(new Rect(bookmarkRect.xMax - borderWidth, bookmarkRect.y, borderWidth, bookmarkRect.height), _activeVesselBorder);
            }

            // Detect hover and update the hovered bookmark ID
            if (bookmarkRect.Contains(Event.current.mousePosition)) {
                _mainUIController.HoveredBookmarkID = bookmark.GetBookmarkID();
            }
        }
        
        /// <summary>
        /// Gets display name for vessel type
        /// </summary>
        private string GetVesselTypeDisplayName(VesselType type) {
            switch (type) {
                case VesselType.Base: return ModLocalization.GetString("vesselTypeBase");
                case VesselType.Debris: return ModLocalization.GetString("vesselTypeDebris");
                case VesselType.DroppedPart: return ModLocalization.GetString("vesselTypeDebris");
                case VesselType.Lander: return ModLocalization.GetString("vesselTypeLander");
                case VesselType.Plane: return ModLocalization.GetString("vesselTypePlane");
                case VesselType.Probe: return ModLocalization.GetString("vesselTypeProbe");
                case VesselType.Relay: return ModLocalization.GetString("vesselTypeRelay");
                case VesselType.Rover: return ModLocalization.GetString("vesselTypeRover");
                case VesselType.Ship: return ModLocalization.GetString("vesselTypeShip");
                case VesselType.Station: return ModLocalization.GetString("vesselTypeStation");
                
                default: return ModLocalization.GetString("vesselTypeOther");
            }
        }
        
        /// <summary>
        /// Gets icon texture for vessel type
        /// </summary>
        private VesselBookmarkButton GetVesselTypeButton(VesselType type) {
            if (_vesselTypeButtons.ContainsKey(type)) {
                return _vesselTypeButtons[type];
            }
            return null;
        }
    }
}
