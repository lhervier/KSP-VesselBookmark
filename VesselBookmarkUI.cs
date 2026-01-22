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
        
        private Rect _windowRect = new Rect(100, 100, 500, 600);
        private Rect _editWindowRect = new Rect(200, 200, 400, 200);
        private bool _visible = false;
        private bool _editWindowVisible = false;
        private Vector2 _scrollPosition = Vector2.zero;
        private ApplicationLauncherButton _toolbarButton;
        private int _windowID;
        private int _editWindowID;
        
        // Filters
        private CelestialBody _selectedBody = null;
        private VesselType? _selectedVesselType = null;
        private int _selectedBodyIndex = 0;
        private int _selectedVesselTypeIndex = 0;
        
        // Edit window
        private VesselBookmark _editingBookmark = null;
        private string _editComment = "";
        
        private void Awake() {
            _windowID = UnityEngine.Random.Range(1000, 2000);
            _editWindowID = UnityEngine.Random.Range(2000, 3000);
            GameEvents.onGUIApplicationLauncherReady.Add(OnLauncherReady);
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
            _visible = true;
            VesselBookmarkManager.Instance.RefreshBookmarks();
        }
        
        /// <summary>
        /// Toolbar button click handler (deactivation)
        /// </summary>
        private void OnToggleOff() {
            _visible = false;
        }
        
        private void OnGUI() {
            // Window style
            GUI.skin = HighLogic.Skin;
            
            if (_visible) {
                _windowRect = GUILayout.Window(
                    _windowID,
                    _windowRect,
                    DrawWindow,
                    "Vessel Bookmarks",
                    GUILayout.MinWidth(500),
                    GUILayout.MinHeight(400)
                );
                
                // Prevent window from going off screen
                _windowRect.x = Mathf.Clamp(_windowRect.x, 0, Screen.width - _windowRect.width);
                _windowRect.y = Mathf.Clamp(_windowRect.y, 0, Screen.height - _windowRect.height);
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
        private void DrawWindow(int windowID) {
            GUILayout.BeginVertical();
            
            // Header
            GUILayout.BeginHorizontal();
            var filteredCount = VesselBookmarkManager.Instance.GetFilteredBookmarks(_selectedBody, _selectedVesselType).Count();
            GUILayout.Label($"Bookmarks: {filteredCount}/{VesselBookmarkManager.Instance.Bookmarks.Count}", GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Refresh", GUILayout.Width(80))) {
                VesselBookmarkManager.Instance.RefreshBookmarks();
            }
            if (GUILayout.Button("Close", GUILayout.Width(80))) {
                _visible = false;
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
            
            var filteredBookmarks = VesselBookmarkManager.Instance.GetFilteredBookmarks(_selectedBody, _selectedVesselType).ToList();
            
            if (filteredBookmarks.Count == 0) {
                GUILayout.Label("No bookmarks match the filters. Right-click on a command module to add one.");
            } else {
                foreach (VesselBookmark bookmark in filteredBookmarks) {
                    DrawBookmarkItem(bookmark, filteredBookmarks);
                }
            }
            
            GUILayout.EndScrollView();
            
            GUILayout.EndVertical();
            
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
            
            string currentBodyName = _selectedBody != null ? _selectedBody.bodyName : "All";
            if (GUILayout.Button(currentBodyName, GUILayout.Width(120))) {
                // Simple toggle: cycle through options
                _selectedBodyIndex = (_selectedBodyIndex + 1) % bodyOptions.Length;
                _selectedBody = (_selectedBodyIndex == 0) ? null : VesselBookmarkManager.Instance.AvailableBodies[_selectedBodyIndex - 1];
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
            
            string currentVesselTypeName = _selectedVesselType.HasValue 
                ? GetVesselTypeDisplayName(_selectedVesselType.Value) 
                : "All";
            if (GUILayout.Button(currentVesselTypeName, GUILayout.Width(100))) {
                // Cycle through type options
                _selectedVesselTypeIndex = (_selectedVesselTypeIndex + 1) % vesselTypeOptions.Length;
                _selectedVesselType = (_selectedVesselTypeIndex == 0) ? (VesselType?)null : VesselBookmarkManager.Instance.AvailableVesselTypes[_selectedVesselTypeIndex - 1];
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
            _editComment = GUILayout.TextArea(_editComment, GUILayout.Height(100), GUILayout.ExpandWidth(true));
            
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
            string commandModuleName = !string.IsNullOrEmpty(bookmark.CommandModuleName) 
                ? bookmark.CommandModuleName 
                : "Module not found";
            string comment = !string.IsNullOrEmpty(bookmark.Comment) 
                ? bookmark.Comment 
                : "No comment";
            VesselType vesselType = bookmark.VesselType;
            string vesselTypeIcon = GetVesselTypeIcon(vesselType);
            
            GUILayout.BeginVertical("box", GUILayout.Height(60));
            
            // Line 1: Icon, Module name, Type, Comment, Edit button
            GUILayout.BeginHorizontal();
            
            // Vessel type icon and name
            GUILayout.Label(vesselTypeIcon, GUILayout.Width(20));
            
            // Command module name (bold)
            GUILayout.Label($"<b>{commandModuleName}</b>", GUILayout.Width(150));
            
            // Comment
            GUILayout.Label(comment, GUILayout.ExpandWidth(true));
            
            // Edit button
            if (GUILayout.Button("Edit", GUILayout.Width(50))) {
                _editingBookmark = bookmark;
                _editComment = bookmark.Comment;
                _editWindowVisible = true;
            }
            
            GUILayout.EndHorizontal();
            
            // Line 2: Reorder buttons, Go to, Remove
            GUILayout.BeginHorizontal();
            
            // Reorder buttons - check against full list, not filtered list
            var allBookmarks = VesselBookmarkManager.Instance.Bookmarks.OrderBy(b => b.Order).ThenBy(b => b.CreationTime).ToList();
            int currentIndex = allBookmarks.IndexOf(bookmark);
            bool canMoveUp = currentIndex > 0;
            bool canMoveDown = currentIndex < allBookmarks.Count - 1;
            
            GUI.enabled = canMoveUp;
            if (GUILayout.Button("↑", GUILayout.Width(25))) {
                VesselBookmarkManager.Instance.MoveBookmarkUp(bookmark);
            }
            GUI.enabled = canMoveDown;
            if (GUILayout.Button("↓", GUILayout.Width(25))) {
                VesselBookmarkManager.Instance.MoveBookmarkDown(bookmark);
            }
            GUI.enabled = true;
            
            GUILayout.Space(5);
            
            // Go to button
            if (vessel != null) {
                if (GUILayout.Button("Go to", GUILayout.Width(70))) {
                    if (VesselNavigator.NavigateToVessel(vessel)) {
                        _visible = false;
                        if (_toolbarButton != null) {
                            _toolbarButton.SetFalse();
                        }
                    }
                }
            } else {
                GUILayout.Label("Unavailable", GUILayout.Width(70));
            }
            
            // Remove button
            if (GUILayout.Button("Remove", GUILayout.Width(70))) {
                VesselBookmarkManager.Instance.RemoveBookmark(bookmark.CommandModuleFlightID);
            }
            
            GUILayout.EndHorizontal();
            
            GUILayout.EndVertical();
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
                
                case VesselType.DeployedGroundPart: return "Science";
                case VesselType.DeployedScienceController: return "Science";
                case VesselType.DeployedSciencePart: return "Science";
                case VesselType.EVA: return "Kerbal";
                case VesselType.Flag: return "Flag";
                case VesselType.SpaceObject: return "Object";
                case VesselType.Unknown: return "Unknown";
                
                default: return "Unknown";
            }
        }
        
        /// <summary>
        /// Gets icon symbol for vessel type
        /// </summary>
        private string GetVesselTypeIcon(VesselType type) {
            switch (type) {
                case VesselType.Base: return "🏠";
                case VesselType.Debris: return "💥";
                case VesselType.DroppedPart: return "💥";
                case VesselType.Lander: return "🌙";
                case VesselType.Plane: return "🌙";
                case VesselType.Probe: return "📡";
                case VesselType.Relay: return "📡";
                case VesselType.Rover: return "🚗";
                case VesselType.Ship: return "🚀";
                case VesselType.Station: return "🛰️";
                
                case VesselType.DeployedGroundPart: return "💥";
                case VesselType.DeployedScienceController: return "💥";
                case VesselType.DeployedSciencePart: return "💥";
                case VesselType.EVA: return "👤";
                case VesselType.Flag: return "🚩";
                case VesselType.SpaceObject: return "⭐";
                case VesselType.Unknown: return "🛰️";
                
                default: return "❓";
            }
        }
    }
}
