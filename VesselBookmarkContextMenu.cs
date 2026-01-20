using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.github.lhervier.ksp {
    
    /// <summary>
    /// Ajoute des actions au menu contextuel des modules de commande
    /// Utilise une approche simple avec un module personnalisé qui s'ajoute dynamiquement
    /// </summary>
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class VesselBookmarkContextMenu : MonoBehaviour {
        
        private HashSet<Part> _processedParts = new HashSet<Part>();
        
        private void Start() {
            // Vérifier périodiquement les nouvelles parties
            InvokeRepeating("ProcessParts", 1f, 2f);
        }
        
        private void OnDestroy() {
            CancelInvoke("ProcessParts");
        }
        
        /// <summary>
        /// Traite les parties pour ajouter le module de bookmark si nécessaire
        /// </summary>
        private void ProcessParts() {
            if (FlightGlobals.ActiveVessel == null) return;
            
            foreach (Part part in FlightGlobals.ActiveVessel.parts) {
                if (part == null || _processedParts.Contains(part)) continue;
                
                // Vérifier si c'est un module de commande
                ModuleCommand commandModule = part.FindModuleImplementing<ModuleCommand>();
                if (commandModule == null) continue;
                
                // Vérifier si le module bookmark n'existe pas déjà
                VesselBookmarkPartModule bookmarkModule = part.GetComponent<VesselBookmarkPartModule>();
                if (bookmarkModule == null) {
                    bookmarkModule = part.gameObject.AddComponent<VesselBookmarkPartModule>();
                }
                
                _processedParts.Add(part);
            }
        }
    }
    
    /// <summary>
    /// Module personnalisé qui ajoute l'action de bookmark aux modules de commande
    /// </summary>
    public class VesselBookmarkPartModule : PartModule {
        
        [KSPEvent(guiActive = true, guiActiveUnfocused = true, guiName = "Toggle Bookmark", active = true)]
        public void ToggleBookmarkEvent() {
            if (part == null) return;
            
            bool hasBookmark = VesselBookmarkManager.Instance.HasBookmark(part);
            
            if (hasBookmark) {
                if (VesselBookmarkManager.Instance.RemoveBookmark(part.flightID)) {
                    ScreenMessages.PostScreenMessage("Bookmark supprimé", 2f, ScreenMessageStyle.UPPER_CENTER);
                }
            } else {
                if (VesselBookmarkManager.Instance.AddBookmark(part)) {
                    ScreenMessages.PostScreenMessage("Bookmark ajouté", 2f, ScreenMessageStyle.UPPER_CENTER);
                }
            }
            
            UpdateEventName();
        }
        
        public override void OnStart(StartState state) {
            base.OnStart(state);
            UpdateEventName();
        }
        
        public override void OnUpdate() {
            base.OnUpdate();
            UpdateEventName();
        }
        
        private void UpdateEventName() {
            if (part == null || Events == null) return;
            
            BaseEvent bookmarkEvent = Events["ToggleBookmarkEvent"];
            if (bookmarkEvent != null) {
                bool hasBookmark = VesselBookmarkManager.Instance.HasBookmark(part);
                bookmarkEvent.guiName = hasBookmark ? "Remove from Bookmarks" : "Add to Bookmarks";
            }
        }
    }
}
