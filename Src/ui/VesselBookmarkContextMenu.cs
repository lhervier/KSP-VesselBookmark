using UnityEngine;
using com.github.lhervier.ksp.bookmarks;

namespace com.github.lhervier.ksp {
    
    /// <summary>
    /// Custom module that adds bookmark action to command modules
    /// Injected into command parts via ModuleManager configuration
    /// </summary>
    public class VesselBookmarkPartModule : PartModule {
        
        [KSPEvent(guiActive = true, guiActiveUnfocused = true, guiName = "Toggle Bookmark", active = true)]
        public void ToggleBookmarkEvent() {
            ModLogger.LogDebug($"ToggleBookmarkEvent");
            if (part == null) {
                ModLogger.LogWarning($"- ToggleBookmarkEvent: Part is null");
                return;
            }

            if (VesselBookmarkManager.Instance.HasBookmark(CommandModuleBookmark.ComputeBookmarkID(part.flightID))) {
                ModLogger.LogDebug($"- ToggleBookmarkEvent: Removing bookmark for part {part.flightID}");
                VesselBookmarkUIDialog.ConfirmRemoval(() => {
                    if (VesselBookmarkManager.Instance.RemoveBookmark(CommandModuleBookmark.ComputeBookmarkID(part.flightID))) {
                        ScreenMessages.PostScreenMessage(
                            VesselBookmarkLocalization.GetString("messageBookmarkRemoved"), 
                            2f, 
                            ScreenMessageStyle.UPPER_CENTER
                        );
                    }
                });
            } else if (VesselBookmarkManager.Instance.AddBookmark(new CommandModuleBookmark(part.flightID))) {
                ModLogger.LogDebug($"- ToggleBookmarkEvent: Bookmark added for part {part.flightID}");
                ScreenMessages.PostScreenMessage(
                    VesselBookmarkLocalization.GetString("messageBookmarkAdded"), 
                    2f, 
                    ScreenMessageStyle.UPPER_CENTER
                );
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
            if (part == null || Events == null) {
                ModLogger.LogWarning($"- UpdateEventName: Part is null or Events is null");
                return;
            }
            
            BaseEvent bookmarkEvent = Events["ToggleBookmarkEvent"];
            if (bookmarkEvent != null) {
                bool hasBookmark = VesselBookmarkManager.Instance.HasBookmark(part.flightID);
                bookmarkEvent.guiName = hasBookmark 
                    ? VesselBookmarkLocalization.GetString("contextMenuRemoveFromBookmarks")
                    : VesselBookmarkLocalization.GetString("contextMenuAddToBookmarks");
            }
        }
    }
}
