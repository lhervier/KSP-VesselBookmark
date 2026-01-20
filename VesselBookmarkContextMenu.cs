using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.github.lhervier.ksp {
    
    /// <summary>
    /// Adds actions to command module context menu
    /// Uses a simple approach with a custom module that is added dynamically
    /// </summary>
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class VesselBookmarkContextMenu : MonoBehaviour {
        
        private HashSet<Part> _processedParts = new HashSet<Part>();
        
        private void Start() {
            // Periodically check for new parts
            InvokeRepeating("ProcessParts", 1f, 2f);
        }
        
        private void OnDestroy() {
            CancelInvoke("ProcessParts");
        }
        
        /// <summary>
        /// Processes parts to add bookmark module if needed
        /// </summary>
        private void ProcessParts() {
            if (FlightGlobals.ActiveVessel == null) return;
            
            foreach (Part part in FlightGlobals.ActiveVessel.parts) {
                if (part == null || _processedParts.Contains(part)) continue;
                
                // Check if it's a command module
                ModuleCommand commandModule = part.FindModuleImplementing<ModuleCommand>();
                if (commandModule == null) continue;
                
                // Check if bookmark module doesn't already exist
                VesselBookmarkPartModule bookmarkModule = part.GetComponent<VesselBookmarkPartModule>();
                if (bookmarkModule == null) {
                    bookmarkModule = part.gameObject.AddComponent<VesselBookmarkPartModule>();
                }
                
                _processedParts.Add(part);
            }
        }
    }
    
    /// <summary>
    /// Custom module that adds bookmark action to command modules
    /// </summary>
    public class VesselBookmarkPartModule : PartModule {
        
        [KSPEvent(guiActive = true, guiActiveUnfocused = true, guiName = "Toggle Bookmark", active = true)]
        public void ToggleBookmarkEvent() {
            if (part == null) return;
            
            bool hasBookmark = VesselBookmarkManager.Instance.HasBookmark(part);
            
            if (hasBookmark) {
                if (VesselBookmarkManager.Instance.RemoveBookmark(part.flightID)) {
                    ScreenMessages.PostScreenMessage("Bookmark removed", 2f, ScreenMessageStyle.UPPER_CENTER);
                }
            } else {
                if (VesselBookmarkManager.Instance.AddBookmark(part)) {
                    ScreenMessages.PostScreenMessage("Bookmark added", 2f, ScreenMessageStyle.UPPER_CENTER);
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
