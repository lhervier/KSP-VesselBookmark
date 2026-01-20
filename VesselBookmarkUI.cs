using KSP.UI.Screens;
using System.Collections.Generic;
using UnityEngine;

namespace com.github.lhervier.ksp {
    
    /// <summary>
    /// Interface utilisateur pour gérer les bookmarks
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
        /// Appelé quand la toolbar est prête
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
                        ApplicationLauncher.AppScenes.ALWAYS,
                        GameDatabase.Instance.GetTexture("VesselBookmarkMod/icon", false) ?? Texture2D.whiteTexture
                    );
                } catch (System.Exception e) {
                    Debug.LogError($"[VesselBookmarkMod] Erreur lors de la création du bouton Toolbar: {e.Message}");
                }
            }
        }
        
        /// <summary>
        /// Appelé quand la toolbar n'est plus prête
        /// </summary>
        private void OnLauncherUnready() {
            if (_toolbarButton != null) {
                try {
                    ApplicationLauncher.Instance.RemoveModApplication(_toolbarButton);
                } catch (System.Exception e) {
                    Debug.LogError($"[VesselBookmarkMod] Erreur lors de la suppression du bouton Toolbar: {e.Message}");
                }
                _toolbarButton = null;
            }
        }
        
        /// <summary>
        /// Gestionnaire du clic sur le bouton toolbar (activation)
        /// </summary>
        private void OnToggleOn() {
            _visible = true;
            VesselBookmarkManager.Instance.UpdateCommandModuleNames();
        }
        
        /// <summary>
        /// Gestionnaire du clic sur le bouton toolbar (désactivation)
        /// </summary>
        private void OnToggleOff() {
            _visible = false;
        }
        
        private void OnGUI() {
            if (!_visible) return;
            
            // Style de la fenêtre
            GUI.skin = HighLogic.Skin;
            
            _windowRect = GUILayout.Window(
                _windowID,
                _windowRect,
                DrawWindow,
                "Vessel Bookmarks",
                GUILayout.MinWidth(500),
                GUILayout.MinHeight(400)
            );
            
            // Empêcher la fenêtre de sortir de l'écran
            _windowRect.x = Mathf.Clamp(_windowRect.x, 0, Screen.width - _windowRect.width);
            _windowRect.y = Mathf.Clamp(_windowRect.y, 0, Screen.height - _windowRect.height);
        }
        
        /// <summary>
        /// Dessine le contenu de la fenêtre
        /// </summary>
        private void DrawWindow(int windowID) {
            GUILayout.BeginVertical();
            
            // En-tête
            GUILayout.BeginHorizontal();
            GUILayout.Label($"Bookmarks: {VesselBookmarkManager.Instance.Bookmarks.Count}", GUILayout.ExpandWidth(true));
            if (GUILayout.Button("Refresh", GUILayout.Width(80))) {
                VesselBookmarkManager.Instance.UpdateCommandModuleNames();
                VesselBookmarkManager.Instance.CleanupInvalidBookmarks();
            }
            if (GUILayout.Button("Close", GUILayout.Width(80))) {
                _visible = false;
                if (_toolbarButton != null) {
                    _toolbarButton.SetFalse();
                }
            }
            GUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            
            // Liste des bookmarks
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
            
            if (VesselBookmarkManager.Instance.Bookmarks.Count == 0) {
                GUILayout.Label("Aucun bookmark. Cliquez droit sur un module de commande pour en ajouter un.");
            } else {
                foreach (VesselBookmark bookmark in VesselBookmarkManager.Instance.Bookmarks) {
                    DrawBookmarkItem(bookmark);
                }
            }
            
            GUILayout.EndScrollView();
            
            GUILayout.EndVertical();
            
            // Permettre de déplacer la fenêtre
            GUI.DragWindow();
        }
        
        /// <summary>
        /// Dessine un item de bookmark
        /// </summary>
        private void DrawBookmarkItem(VesselBookmark bookmark) {
            GUILayout.BeginVertical("box");
            
            // Nom du module de commande et situation
            Vessel vessel = VesselBookmarkManager.Instance.GetVesselForBookmark(bookmark);
            string commandModuleName = !string.IsNullOrEmpty(bookmark.CommandModuleName) 
                ? bookmark.CommandModuleName 
                : "Module introuvable";
            string situation = vessel != null ? VesselSituationDetector.GetSituation(vessel) : "Vaisseau introuvable";
            
            GUILayout.BeginHorizontal();
            GUILayout.Label($"<b>{commandModuleName}</b>", GUILayout.Width(200));
            GUILayout.Label(situation, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();
            
            GUILayout.Space(5);
            
            // Commentaire éditable
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
            
            // Boutons d'action
            GUILayout.BeginHorizontal();
            
            if (vessel != null) {
                if (GUILayout.Button("Go to", GUILayout.Width(100))) {
                    if (VesselNavigator.NavigateToVessel(vessel)) {
                        _visible = false; // Fermer la fenêtre après navigation
                        if (_toolbarButton != null) {
                            _toolbarButton.SetFalse();
                        }
                    }
                    // Si la navigation échoue, l'erreur est déjà loggée dans NavigateToVessel
                }
            } else {
                GUILayout.Label("Vaisseau non disponible", GUILayout.Width(150));
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
