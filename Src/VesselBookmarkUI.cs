using KSP.UI.Screens;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.github.lhervier.ksp {
    
    /// <summary>
    /// User interface for managing bookmarks
    /// </summary>
    [KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
    public class VesselBookmarkUI : MonoBehaviour {
        
        private static readonly int BUTTON_HEIGHT = 20;
        private static readonly int BUTTON_WIDTH = 20;

        private ApplicationLauncherButton _toolbarButton;
        private Rect _mainWindowRect = new Rect(100, 100, 500, 600);
        private bool _mainWindowsVisible = false;
        private int _mainWindowID;
        
        private Rect _editWindowRect = new Rect(200, 200, 400, 200);
        private bool _editWindowVisible = false;
        private int _editWindowID;
        
        private Vector2 _scrollPosition = Vector2.zero;
        
        // Filters
        private CelestialBody _selectedBody = null;
        private int _selectedBodyIndex = 0;
        private VesselType? _selectedVesselType = null;
        private int _selectedVesselTypeIndex = 0;
        
        // Edit window
        private VesselBookmark _editingBookmark = null;
        private string _editComment = "";

        // Icon cache
        private Dictionary<VesselType, VesselBookmarkButton> _vesselTypeButtons = new Dictionary<VesselType, VesselBookmarkButton>();
        private VesselBookmarkButton _removeButton;
        private VesselBookmarkButton _moveUpButton;
        private VesselBookmarkButton _moveDownButton;
        private VesselBookmarkButton _goToButton;
        private VesselBookmarkButton _editButton;
        
        // Hover state
        private uint _hoveredBookmarkFlightID = 0;

        // UI styles with white text
        private GUIStyle _labelStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _textAreaStyle;
        private GUIStyle _tooltipStyle;

        // Bookmarks list to display in the UI (cached for performance)
        private List<VesselBookmark> _availableBookmarks = new List<VesselBookmark>();
        private List<CelestialBody> _availableBodies = new List<CelestialBody>();
        private List<VesselType> _availableVesselTypes = new List<VesselType>();

        private void Awake() {
            _mainWindowID = UnityEngine.Random.Range(1000, 2000);
            _editWindowID = UnityEngine.Random.Range(2000, 3000);
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
                "Remove bookmark", 
                BUTTON_WIDTH, 
                BUTTON_HEIGHT
            );
            
            // Initialize move up button
            _moveUpButton = new VesselBookmarkButton(
                "VesselBookmarkMod/buttons/up",
                "Move up", 
                BUTTON_WIDTH, 
                BUTTON_HEIGHT
            );
            
            // Initialize move down button
            _moveDownButton = new VesselBookmarkButton(
                "VesselBookmarkMod/buttons/down",
                "Move down", 
                BUTTON_WIDTH, 
                BUTTON_HEIGHT
            );
            
            // Initialize go to button
            _goToButton = new VesselBookmarkButton(
                "VesselBookmarkMod/buttons/switch",
                "Go to vessel", 
                BUTTON_WIDTH, 
                BUTTON_HEIGHT
            );
            
            // Initialize edit button
            _editButton = new VesselBookmarkButton(
                "VesselBookmarkMod/buttons/edit",
                "Edit comment", 
                BUTTON_WIDTH, 
                BUTTON_HEIGHT
            );

            VesselBookmarkManager.Instance.OnBookmarksUpdated.Add(OnBookmarksUpdated);
        }

        private void OnBookmarksUpdated() {
            ModLogger.LogDebug($"OnBookmarksUpdated");
            
            _availableBookmarks.Clear();
            _availableBodies.Clear();
            _availableVesselTypes.Clear();

            foreach (VesselBookmark bookmark in VesselBookmarkManager.Instance.Bookmarks) {
                Vessel vessel = VesselBookmarkManager.Instance.GetVessel(bookmark);
                if( vessel != null && !_availableBodies.Contains(vessel.mainBody) ) {
                    _availableBodies.Add(vessel.mainBody);
                }

                if( !_availableVesselTypes.Contains(bookmark.VesselType) ) {
                    _availableVesselTypes.Add(bookmark.VesselType);
                }

                if( !_availableBookmarks.Contains(bookmark) ) {
                    bool addBookmark;

                    if( _selectedBody == null && _selectedVesselType == null ) {
                        addBookmark = true;
                    } else if( _selectedBody == null ) {
                        addBookmark = bookmark.VesselType == _selectedVesselType;
                    } else if( _selectedVesselType == null ) {
                        addBookmark = vessel.mainBody == _selectedBody;
                    } else {
                        addBookmark = vessel.mainBody == _selectedBody && bookmark.VesselType == _selectedVesselType;
                    }
                    if( addBookmark ) {
                        _availableBookmarks.Add(bookmark);
                    }
                }
            }
        }

        private void EnsureWhiteTextStyles() {
            if (_labelStyle != null) {
                return;
            }

            _labelStyle = new GUIStyle(GUI.skin.label) { richText = true };
            _buttonStyle = new GUIStyle(GUI.skin.button);
            _textAreaStyle = new GUIStyle(GUI.skin.textArea);
            _tooltipStyle = new GUIStyle(GUI.skin.box);

            ApplyWhiteText(_labelStyle);
            ApplyWhiteText(_buttonStyle);
            ApplyWhiteText(_textAreaStyle);
            ApplyWhiteText(_tooltipStyle);
        }

        private void ApplyWhiteText(GUIStyle style) {
            if (style == null) {
                return;
            }

            style.normal.textColor = Color.white;
            style.hover.textColor = Color.white;
            style.active.textColor = Color.white;
            style.focused.textColor = Color.white;
            style.onNormal.textColor = Color.white;
            style.onHover.textColor = Color.white;
            style.onActive.textColor = Color.white;
            style.onFocused.textColor = Color.white;
        }
        
        private void OnDestroy() {
            GameEvents.onGUIApplicationLauncherReady.Remove(OnLauncherReady);
            OnLauncherUnready();
            VesselBookmarkManager.Instance.OnBookmarksUpdated.Remove(OnBookmarksUpdated);
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
            _mainWindowsVisible = true;
            _editWindowVisible = false;
            VesselBookmarkManager.Instance.RefreshBookmarks();
        }
        
        /// <summary>
        /// Toolbar button click handler (deactivation)
        /// </summary>
        private void OnToggleOff() {
            _mainWindowsVisible = false;
            _editWindowVisible = false;
        }
        
        private void OnGUI() {
            // Window style
            GUI.skin = HighLogic.Skin;
            EnsureWhiteTextStyles();
            
            if (_mainWindowsVisible) {
                _mainWindowRect = GUILayout.Window(
                    _mainWindowID,
                    _mainWindowRect,
                    DrawMainWindow,
                    "Vessel Bookmarks",
                    GUILayout.MinWidth(500),
                    GUILayout.MinHeight(400)
                );
                
                // Prevent window from going off screen
                _mainWindowRect.x = Mathf.Clamp(_mainWindowRect.x, 0, Screen.width - _mainWindowRect.width);
                _mainWindowRect.y = Mathf.Clamp(_mainWindowRect.y, 0, Screen.height - _mainWindowRect.height);
            }
            
            if (_editWindowVisible) {
                _editWindowRect = GUILayout.Window(
                    _editWindowID,
                    _editWindowRect,
                    DrawEditWindow,
                    "Edit Bookmark Comment",
                    GUILayout.MinWidth(400),
                    GUILayout.MinHeight(200)
                );
                
                // Prevent window from going off screen
                _editWindowRect.x = Mathf.Clamp(_editWindowRect.x, 0, Screen.width - _editWindowRect.width);
                _editWindowRect.y = Mathf.Clamp(_editWindowRect.y, 0, Screen.height - _editWindowRect.height);
            }
        }
        
        /// <summary>
        /// Draws window content
        /// </summary>
        private void DrawMainWindow(int windowID) {
            GUILayout.BeginVertical();
            
            // Header
            GUILayout.BeginHorizontal();
            var filteredCount = _availableBookmarks.Count;
            GUILayout.Label(
                $"Bookmarks: {filteredCount}/{_availableBookmarks.Count}",
                _labelStyle,
                GUILayout.ExpandWidth(true)
            );
            if (GUILayout.Button("Refresh", _buttonStyle, GUILayout.Width(80))) {
                VesselBookmarkManager.Instance.RefreshBookmarks();
            }
            if (GUILayout.Button("Close", _buttonStyle, GUILayout.Width(80))) {
                _mainWindowsVisible = false;
                _editWindowVisible = false;
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
            
            if (_availableBookmarks.Count == 0) {
                GUILayout.Label("No bookmarks match the filters. Right-click on a command module to add one.", _labelStyle);
            } else {
                foreach (VesselBookmark bookmark in _availableBookmarks) {
                    DrawBookmarkItem(bookmark);
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
                
                GUI.Box(tooltipRect, GUI.tooltip, _tooltipStyle);
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
            GUILayout.Label("Body:", _labelStyle, GUILayout.Width(50));
            
            // Create dropdown options for body
            string[] bodyOptions = new string[_availableBodies.Count + 1];
            bodyOptions[0] = "All";
            for (int i = 0; i < _availableBodies.Count; i++) {
                bodyOptions[i + 1] = _availableBodies[i].bodyName;
            }
            
            string currentBodyName;
            if( _selectedBody != null) {
                currentBodyName = _selectedBody.bodyName;
            } else {
                currentBodyName = "All";
            }
            if (GUILayout.Button(currentBodyName, _buttonStyle, GUILayout.Width(120))) {
                _selectedBodyIndex = (_selectedBodyIndex + 1) % bodyOptions.Length;
                if (_selectedBodyIndex == 0) {
                    _selectedBody = null;
                } else {
                    _selectedBody = _availableBodies[_selectedBodyIndex - 1];
                }
                this.OnBookmarksUpdated();
            }
            
            GUILayout.Space(10);
            
             // Vessel Type filter
            GUILayout.Label("Type:", _labelStyle, GUILayout.Width(50));
            
            // Create dropdown options for vessel type
            string[] vesselTypeOptions = new string[_availableVesselTypes.Count + 1];
            vesselTypeOptions[0] = "All";
            for (int i = 0; i < _availableVesselTypes.Count; i++) {
                vesselTypeOptions[i + 1] = _availableVesselTypes[i].ToString();
            }
            
            string currentVesselTypeName;
            if( _selectedVesselType.HasValue) {
                currentVesselTypeName = GetVesselTypeDisplayName(_selectedVesselType.Value);
            } else {
                currentVesselTypeName = "All";
            }
            if (GUILayout.Button(currentVesselTypeName, _buttonStyle, GUILayout.Width(100))) {
                _selectedVesselTypeIndex = (_selectedVesselTypeIndex + 1) % vesselTypeOptions.Length;
                if (_selectedVesselTypeIndex == 0) {
                    _selectedVesselType = null;
                } else {
                    _selectedVesselType = _availableVesselTypes[_selectedVesselTypeIndex - 1];
                }
                this.OnBookmarksUpdated();
            }

            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Clear", _buttonStyle, GUILayout.Width(60))) {
                _selectedBody = null;
                _selectedVesselType = null;
                _selectedBodyIndex = 0;
                _selectedVesselTypeIndex = 0;
                this.OnBookmarksUpdated();
            }
            
            GUILayout.EndHorizontal();
            
            GUILayout.EndVertical();
        }
        
        /// <summary>
        /// Draws the edit window
        /// </summary>
        private void DrawEditWindow(int windowID) {
            GUILayout.BeginVertical();
            
            GUILayout.Label("Comment:", _labelStyle);
            _editComment = GUILayout.TextArea(
                _editComment, 
                _textAreaStyle,
                GUILayout.Height(100), 
                GUILayout.ExpandWidth(true)
            );
            
            GUILayout.Space(10);
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save", _buttonStyle)) {
                if (_editingBookmark != null) {
                    _editingBookmark.Comment = _editComment;
                    this.OnBookmarksUpdated();
                }
                _editWindowVisible = false;
                _editingBookmark = null;
                _editComment = "";
            }
            if (GUILayout.Button("Cancel", _buttonStyle)) {
                _editWindowVisible = false;
                _editingBookmark = null;
                _editComment = "";
            }
            GUILayout.EndHorizontal();
            
            GUILayout.EndVertical();
            
            // Allow window dragging
            GUI.DragWindow();
        }
        
        /// <summary>
        /// Draws a bookmark item
        /// </summary>
        private void DrawBookmarkItem(VesselBookmark bookmark) {
            string commandModuleName;
            if( !string.IsNullOrEmpty(bookmark.CommandModuleName) ) {
                commandModuleName = bookmark.CommandModuleName;
            } else {
                commandModuleName = "Module not found";
            }

            string comment;
            if( !string.IsNullOrEmpty(bookmark.Comment) ) {
                comment = bookmark.Comment;
            } else {
                comment = "No comment";
            }
            
            bool isHovered = _hoveredBookmarkFlightID == bookmark.CommandModuleFlightID;

            // Create custom box style with hover background if this is the hovered bookmark
            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            if (isHovered) {
                // Create hover background texture
                Texture2D hoverBg = new Texture2D(1, 1);
                hoverBg.SetPixel(0, 0, new Color(0.3f, 0.3f, 0.3f, 1f));
                hoverBg.Apply();
                boxStyle.normal.background = hoverBg;
            }
            
            GUILayout.BeginVertical(boxStyle);
            
            // Line 1: Icon, Module name, and action buttons (aligned right)
            GUILayout.BeginHorizontal();
            
            // Vessel type icon
            VesselBookmarkButton vesselTypeButton = GetVesselTypeButton(bookmark.VesselType);
            vesselTypeButton.Draw(
                () => vesselTypeButton != null,
                null
            );
            
            // Command module name (bold)
            GUILayout.Label($"<b>{commandModuleName}</b>", _labelStyle, GUILayout.Width(150));
            
            GUILayout.FlexibleSpace();
            
            int currentIndex = _availableBookmarks.IndexOf(bookmark);
            bool canMoveUp = currentIndex > 0;
            bool canMoveDown = currentIndex < _availableBookmarks.Count - 1;
                
            // Small spacing before buttons
            GUILayout.Space(5);
            
            // Edit button
            _editButton.Draw(
                () => isHovered,
                () => {
                    _editingBookmark = bookmark;
                    _editComment = bookmark.Comment;
                    _editWindowVisible = true;
                }
            );
            
            GUILayout.Space(3);

            // Go to button
            _goToButton.Draw(
                () => isHovered,
                () => {
                    Vessel vessel = VesselBookmarkManager.Instance.GetVessel(bookmark);
                    if (VesselNavigator.NavigateToVessel(vessel)) {
                        _mainWindowsVisible = false;
                        _editWindowVisible = false;
                        if (_toolbarButton != null) {
                            _toolbarButton.SetFalse();
                        }
                    }
                }
            );
                
            GUILayout.Space(3);

            // Move up button
            _moveUpButton.Draw(
                () => isHovered,
                () => {
                    if( canMoveUp ) {
                        VesselBookmark previousBookmark = _availableBookmarks[currentIndex - 1];
                        VesselBookmarkManager.Instance.SwapBookmarks(
                            bookmark.CommandModuleFlightID, 
                            previousBookmark.CommandModuleFlightID
                        );
                    }
                }
            );
        
            // Move down button
            _moveDownButton.Draw(
                () => isHovered,
                () => {
                    if( canMoveDown ) {
                        VesselBookmark nextBookmark = _availableBookmarks[currentIndex + 1];
                        VesselBookmarkManager.Instance.SwapBookmarks(
                            bookmark.CommandModuleFlightID, 
                            nextBookmark.CommandModuleFlightID
                        );
                    }
                }
            );
            
            GUILayout.Space(3);
            
            // Remove button
            _removeButton.Draw(
                () => isHovered,
                () => {
                    VesselBookmarkUIDialog.ConfirmRemoval(() => {
                        VesselBookmarkManager.Instance.RemoveBookmark(bookmark.CommandModuleFlightID);
                    });
                }
            );
            
            GUILayout.EndHorizontal();
            Rect line1Rect = GUILayoutUtility.GetLastRect();

            // Line 2: Comment
            GUILayout.BeginHorizontal();
            GUILayout.Space(BUTTON_WIDTH + 4);
            GUILayout.Label(comment, _labelStyle, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
            Rect line2Rect = GUILayoutUtility.GetLastRect();
            
            // Get the rect of the bookmark area BEFORE closing it
            Rect bookmarkRect = Rect.MinMaxRect(
                Mathf.Min(line1Rect.xMin, line2Rect.xMin),
                Mathf.Min(line1Rect.yMin, line2Rect.yMin),
                Mathf.Max(line1Rect.xMax, line2Rect.xMax),
                Mathf.Max(line1Rect.yMax, line2Rect.yMax)
            );
            
            GUILayout.EndVertical();

            // Detect hover and update the hovered bookmark ID
            if (bookmarkRect.Contains(Event.current.mousePosition)) {
                _hoveredBookmarkFlightID = bookmark.CommandModuleFlightID;
            }
        }
        
        /// <summary>
        /// Gets display name for vessel type
        /// </summary>
        private string GetVesselTypeDisplayName(VesselType type) {
            switch (type) {
                case VesselType.Base: return "Base";
                case VesselType.Debris: return "Debris";
                case VesselType.DroppedPart: return "Debris";
                case VesselType.Lander: return "Lander";
                case VesselType.Plane: return "Plane";
                case VesselType.Probe: return "Probe";
                case VesselType.Relay: return "Relay";
                case VesselType.Rover: return "Rover";
                case VesselType.Ship: return "Ship";
                case VesselType.Station: return "Station";
                
                default: return "Other";
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
