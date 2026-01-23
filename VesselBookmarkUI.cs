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
        private Dictionary<VesselType, Texture2D> _vesselTypeIcons = new Dictionary<VesselType, Texture2D>();
        private BookmarkButton _removeButton;
        private BookmarkButton _moveUpButton;
        private BookmarkButton _moveDownButton;
        private BookmarkButton _goToButton;
        private BookmarkButton _editButton;
        private BookmarkButton _emptyButton;
        
        // Hover state
        private uint _hoveredBookmarkFlightID = 0;
        
        private void Awake() {
            _mainWindowID = UnityEngine.Random.Range(1000, 2000);
            _editWindowID = UnityEngine.Random.Range(2000, 3000);
            GameEvents.onGUIApplicationLauncherReady.Add(OnLauncherReady);
            
            _vesselTypeIcons[VesselType.Base] = GameDatabase.Instance.GetTexture("VesselBookmarkMod/vessel_types/base", false);
            _vesselTypeIcons[VesselType.Debris] = GameDatabase.Instance.GetTexture("VesselBookmarkMod/vessel_types/debris", false);
            _vesselTypeIcons[VesselType.Lander] = GameDatabase.Instance.GetTexture("VesselBookmarkMod/vessel_types/lander", false);
            _vesselTypeIcons[VesselType.Plane] = GameDatabase.Instance.GetTexture("VesselBookmarkMod/vessel_types/plane", false);
            _vesselTypeIcons[VesselType.Probe] = GameDatabase.Instance.GetTexture("VesselBookmarkMod/vessel_types/probe", false);
            _vesselTypeIcons[VesselType.Relay] = GameDatabase.Instance.GetTexture("VesselBookmarkMod/vessel_types/relay", false);
            _vesselTypeIcons[VesselType.Rover] = GameDatabase.Instance.GetTexture("VesselBookmarkMod/vessel_types/rover", false);
            _vesselTypeIcons[VesselType.Ship] = GameDatabase.Instance.GetTexture("VesselBookmarkMod/vessel_types/ship", false);
            _vesselTypeIcons[VesselType.Station] = GameDatabase.Instance.GetTexture("VesselBookmarkMod/vessel_types/station", false);
            
            // Initialize empty button
            _emptyButton = new BookmarkButton(
                "VesselBookmarkMod/buttons/empty",
                null, 
                20, 
                20
            );

            // Initialize remove button
            _removeButton = new BookmarkButton(
                "VesselBookmarkMod/buttons/remove",
                "Remove bookmark", 
                20, 
                20
            );
            
            // Initialize move up button
            _moveUpButton = new BookmarkButton(
                "VesselBookmarkMod/buttons/up",
                "Move up", 
                20, 
                20
            );
            
            // Initialize move down button
            _moveDownButton = new BookmarkButton(
                "VesselBookmarkMod/buttons/down",
                "Move down", 
                20, 
                20
            );
            
            // Initialize go to button
            _goToButton = new BookmarkButton(
                "VesselBookmarkMod/buttons/switch",
                "Go to vessel", 
                20, 
                20
            );
            
            // Initialize edit button
            _editButton = new BookmarkButton(
                "VesselBookmarkMod/buttons/edit",
                "Edit comment", 
                20, 
                20
            );
        }
        
        private void OnDestroy() {
            GameEvents.onGUIApplicationLauncherReady.Remove(OnLauncherReady);
            OnLauncherUnready();
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
            var filteredCount = VesselBookmarkManager.Instance.GetFilteredBookmarks(
                _selectedBody, 
                _selectedVesselType
            )
            .Count();
            GUILayout.Label(
                $"Bookmarks: {filteredCount}/{VesselBookmarkManager.Instance.Bookmarks.Count}", 
                GUILayout.ExpandWidth(true)
            );
            if (GUILayout.Button("Refresh", GUILayout.Width(80))) {
                VesselBookmarkManager.Instance.RefreshBookmarks();
            }
            if (GUILayout.Button("Close", GUILayout.Width(80))) {
                _mainWindowsVisible = false;
                _editWindowVisible = false;
                if (_toolbarButton != null) {
                    _toolbarButton.SetFalse();
                }
            }
            GUILayout.EndHorizontal();
            
            GUILayout.Space(5);
            
            // Filters section
            DrawFilters();
            
            GUILayout.Space(5);
            
            // Bookmarks list
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, true);
            
            var filteredBookmarks = VesselBookmarkManager.Instance.GetFilteredBookmarks(
                _selectedBody, 
                _selectedVesselType
            )
            .ToList();
            
            if (filteredBookmarks.Count == 0) {
                GUILayout.Label("No bookmarks match the filters. Right-click on a command module to add one.");
            } else {
                foreach (VesselBookmark bookmark in filteredBookmarks) {
                    DrawBookmarkItem(bookmark, filteredBookmarks);
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
                
                GUI.Box(tooltipRect, GUI.tooltip);
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
            GUILayout.Label("Body:", GUILayout.Width(50));
            
            // Create dropdown options for body
            string[] bodyOptions = new string[VesselBookmarkManager.Instance.AvailableBodies.Count + 1];
            bodyOptions[0] = "All";
            for (int i = 0; i < VesselBookmarkManager.Instance.AvailableBodies.Count; i++) {
                bodyOptions[i + 1] = VesselBookmarkManager.Instance.AvailableBodies[i].bodyName;
            }
            
            string currentBodyName;
            if( _selectedBody != null) {
                currentBodyName = _selectedBody.bodyName;
            } else {
                currentBodyName = "All";
            }
            if (GUILayout.Button(currentBodyName, GUILayout.Width(120))) {
                _selectedBodyIndex = (_selectedBodyIndex + 1) % bodyOptions.Length;
                if (_selectedBodyIndex == 0) {
                    _selectedBody = null;
                } else {
                    _selectedBody = VesselBookmarkManager.Instance.AvailableBodies[_selectedBodyIndex - 1];
                }
            }
            
            GUILayout.Space(10);
            
             // Vessel Type filter
            GUILayout.Label("Type:", GUILayout.Width(50));
            
            // Create dropdown options for vessel type
            string[] vesselTypeOptions = new string[VesselBookmarkManager.Instance.AvailableVesselTypes.Count + 1];
            vesselTypeOptions[0] = "All";
            for (int i = 0; i < VesselBookmarkManager.Instance.AvailableVesselTypes.Count; i++) {
                vesselTypeOptions[i + 1] = VesselBookmarkManager.Instance.AvailableVesselTypes[i].ToString();
            }
            
            string currentVesselTypeName;
            if( _selectedVesselType.HasValue) {
                currentVesselTypeName = GetVesselTypeDisplayName(_selectedVesselType.Value);
            } else {
                currentVesselTypeName = "All";
            }
            if (GUILayout.Button(currentVesselTypeName, GUILayout.Width(100))) {
                _selectedVesselTypeIndex = (_selectedVesselTypeIndex + 1) % vesselTypeOptions.Length;
                if (_selectedVesselTypeIndex == 0) {
                    _selectedVesselType = null;
                } else {
                    _selectedVesselType = VesselBookmarkManager.Instance.AvailableVesselTypes[_selectedVesselTypeIndex - 1];
                }
            }

            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Clear", GUILayout.Width(60))) {
                _selectedBody = null;
                _selectedVesselType = null;
                _selectedBodyIndex = 0;
                _selectedVesselTypeIndex = 0;
            }
            
            GUILayout.EndHorizontal();
            
            GUILayout.EndVertical();
        }
        
        /// <summary>
        /// Draws the edit window
        /// </summary>
        private void DrawEditWindow(int windowID) {
            GUILayout.BeginVertical();
            
            GUILayout.Label("Comment:");
            _editComment = GUILayout.TextArea(
                _editComment, 
                GUILayout.Height(100), 
                GUILayout.ExpandWidth(true)
            );
            
            GUILayout.Space(10);
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save")) {
                if (_editingBookmark != null) {
                    _editingBookmark.Comment = _editComment;
                }
                _editWindowVisible = false;
                _editingBookmark = null;
                _editComment = "";
            }
            if (GUILayout.Button("Cancel")) {
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
        private void DrawBookmarkItem(VesselBookmark bookmark, List<VesselBookmark> filteredList) {
            Vessel vessel = VesselBookmarkManager.Instance.GetVesselForBookmark(bookmark);
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
            
            Texture2D vesselTypeIcon = GetVesselTypeIcon(bookmark.VesselType);
            
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
            
            GUILayout.BeginVertical(boxStyle, GUILayout.Height(60));
            
            // Line 1: Icon, Module name, Type, Comment, Edit button
            GUILayout.BeginHorizontal();
            
            // Vessel type icon and name
            if (vesselTypeIcon != null) {
                GUILayout.Label(vesselTypeIcon, GUILayout.Width(21), GUILayout.Height(20));
            } else {
                GUILayout.Label("", GUILayout.Width(21));
            }
            
            // Command module name (bold)
            GUILayout.Label($"<b>{commandModuleName}</b>", GUILayout.Width(150));
            
            // Comment
            GUILayout.Label(comment, GUILayout.ExpandWidth(true));
            
            // Edit button
            if( isHovered ) {
                _editButton.Draw(() => {
                    _editingBookmark = bookmark;
                    _editComment = bookmark.Comment;
                    _editWindowVisible = true;
                });
            }
            
            GUILayout.EndHorizontal();
            
            // Line 2: Reorder buttons, Go to, Remove
            GUILayout.BeginHorizontal();
            
            // Reorder buttons - check against full list, not filtered list
            var allBookmarks = VesselBookmarkManager.Instance.Bookmarks.OrderBy(b => b.Order).ThenBy(b => b.CreationTime).ToList();
            int currentIndex = allBookmarks.IndexOf(bookmark);
            bool canMoveUp = currentIndex > 0;
            bool canMoveDown = currentIndex < allBookmarks.Count - 1;
            
            if( isHovered ) {
                // Move up button
                if (canMoveUp) {
                    _moveUpButton.Draw(() => {
                        VesselBookmarkManager.Instance.MoveBookmarkUp(bookmark);
                    });
                } else {
                    _moveUpButton.DrawDisabled();
                }
            
                // Move down button
                if (canMoveDown) {
                    _moveDownButton.Draw(() => {
                        VesselBookmarkManager.Instance.MoveBookmarkDown(bookmark);
                    });
                } else {
                    _moveDownButton.DrawDisabled();
                }
            
                GUILayout.Space(5);
            
                // Go to button
                if (vessel != null) {
                    _goToButton.Draw(() => {
                        if (VesselNavigator.NavigateToVessel(vessel)) {
                            _mainWindowsVisible = false;
                            _editWindowVisible = false;
                            if (_toolbarButton != null) {
                                _toolbarButton.SetFalse();
                            }
                        }
                    });
                } else {
                    // Draw disabled go to button when vessel is unavailable
                    _goToButton.DrawDisabled();
                }

                // Remove button
                _removeButton.Draw(
                    () => {
                        VesselBookmarkManager.Instance.RemoveBookmark(bookmark.CommandModuleFlightID);
                    }
                );
            } else {
                _emptyButton.Draw(null);
            }
            
            GUILayout.EndHorizontal();
            
            // Get the rect of the vertical box BEFORE closing it
            Rect bookmarkRect = GUILayoutUtility.GetLastRect();
            
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
        private Texture2D GetVesselTypeIcon(VesselType type) {
            if (_vesselTypeIcons.ContainsKey(type)) {
                return _vesselTypeIcons[type];
            }
            return null;
        }
    }
}
