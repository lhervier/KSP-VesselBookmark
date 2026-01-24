using UnityEngine;

namespace com.github.lhervier.ksp {
    
    /// <summary>
    /// Custom module that adds bookmark action to command modules
    /// Injected into command parts via ModuleManager configuration
    /// </summary>
    public class VesselBookmarkPartModule : PartModule {
        
        [KSPEvent(guiActive = true, guiActiveUnfocused = true, guiName = "Toggle Bookmark", active = true)]
        public void ToggleBookmarkEvent() {
            if (part == null) return;
            
            if (VesselBookmarkManager.Instance.HasBookmark(part)) {
                VesselBookmarkUIDialog.ConfirmRemoval(() => {
                    if (VesselBookmarkManager.Instance.RemoveBookmark(part.flightID)) {
                        ScreenMessages.PostScreenMessage("Bookmark removed", 2f, ScreenMessageStyle.UPPER_CENTER);
                    }
                });
            } else if (VesselBookmarkManager.Instance.AddBookmark(part)) {
                ScreenMessages.PostScreenMessage("Bookmark added", 2f, ScreenMessageStyle.UPPER_CENTER);
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
