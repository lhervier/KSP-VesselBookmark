using KSP.UI.Screens;
using System.Collections.Generic;
using UnityEngine;

namespace com.github.lhervier.ksp {
    
    /// <summary>
    /// User interface for managing bookmarks
    /// </summary>
    [KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
    public class VesselBookmarkUI : MonoBehaviour {
        
        private Rect _windowRect = new Rect(100, 100, 500, 600);
        private bool _visible = false;
        private Vector2 _scrollPosition = Vector2.zero;
        private Dictionary<VesselBookmark, string> _commentEdits = new Dictionary<VesselBookmark, string>();
        private ApplicationLauncherButton _toolbarButton;
        private int _windowID;
        
        private void Awake() {
            _windowID = UnityEngine.Random.Range(1000, 2000);
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
                        ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW,
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
            if (!_visible) return;
            
            // Only show window in FLIGHT scene where navigation works
            if (HighLogic.LoadedScene != GameScenes.FLIGHT) {
                _visible = false;
                if (_toolbarButton != null) {
                    _toolbarButton.SetFalse();
                }
                return;
            }
            
            // Window style
            GUI.skin = HighLogic.Skin;
            
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
        
        /// <summary>
        /// Draws window content
        /// </summary>
        private void DrawWindow(int windowID) {
            GUILayout.BeginVertical();
            
            // Header
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Bookmarks: {VesselBookmarkManager.Instance.Bookmarks.Count}", GUILayout.ExpandWidth(true));
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
            
            GUILayout.Space(10);
            
            // Bookmarks list
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
            
            if (VesselBookmarkManager.Instance.Bookmarks.Count == 0) {
                GUILayout.Label("No bookmarks. Right-click on a command module to add one.");
            } else {
                foreach (VesselBookmark bookmark in VesselBookmarkManager.Instance.Bookmarks) {
                    DrawBookmarkItem(bookmark);
                }
            }
            
            GUILayout.EndScrollView();
            
            GUILayout.EndVertical();
            
            // Allow window dragging
            GUI.DragWindow();
        }
        
        /// <summary>
        /// Draws a bookmark item
        /// </summary>
        private void DrawBookmarkItem(VesselBookmark bookmark) {
            GUILayout.BeginVertical("box");
            
            // Command module name and situation
            Vessel vessel = VesselBookmarkManager.Instance.GetVesselForBookmark(bookmark);
            string commandModuleName = !string.IsNullOrEmpty(bookmark.CommandModuleName) 
                ? bookmark.CommandModuleName 
                : "Module not found";
            string situation = vessel != null ? VesselSituationDetector.GetSituation(vessel) : "Vessel not found";
            
            GUILayout.BeginHorizontal();
            GUILayout.Label($"<b>{commandModuleName}</b>", GUILayout.Width(200));
            GUILayout.Label(situation, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
            
            GUILayout.Space(5);
            
            // Editable comment
            GUILayout.BeginHorizontal();
            GUILayout.Label("Comment:", GUILayout.Width(80));
            
            if (!_commentEdits.ContainsKey(bookmark)) {
                _commentEdits[bookmark] = bookmark.Comment;
            }
            
            _commentEdits[bookmark] = GUILayout.TextField(_commentEdits[bookmark], GUILayout.ExpandWidth(true));
            
            if (GUILayout.Button("Save", GUILayout.Width(60))) {
                bookmark.Comment = _commentEdits[bookmark];
            }
            GUILayout.EndHorizontal();
            
            GUILayout.Space(5);
            
            // Action buttons
            GUILayout.BeginHorizontal();
            
            if (vessel != null) {
                if (GUILayout.Button("Go to", GUILayout.Width(100))) {
                    if (VesselNavigator.NavigateToVessel(vessel)) {
                        _visible = false; // Close window after navigation
                        if (_toolbarButton != null) {
                            _toolbarButton.SetFalse();
                        }
                    }
                    // If navigation fails, error is already logged in NavigateToVessel
                }
            } else {
                GUILayout.Label("Vessel unavailable", GUILayout.Width(150));
            }
            
            if (GUILayout.Button("Remove", GUILayout.Width(100))) {
                VesselBookmarkManager.Instance.RemoveBookmark(bookmark.CommandModuleFlightID);
                _commentEdits.Remove(bookmark);
            }
            
            GUILayout.EndHorizontal();
            
            GUILayout.EndVertical();
            GUILayout.Space(5);
        }
    }
}
