using System;
using com.github.lhervier.ksp.bookmarksmod.bookmarks;
using com.github.lhervier.ksp.bookmarksmod.ui;
using com.github.lhervier.ksp.shared;

namespace com.github.lhervier.ksp.bookmarksmod {
    
    /// <summary>
    /// Custom module that adds bookmark action to command modules
    /// Injected into command parts via ModuleManager configuration
    /// </summary>
    public class VesselBookmarkPartModule : PartModule {

        private static readonly ModLogger LOGGER = new ModLogger("VesselBookmarkPartModule");

        [KSPEvent(guiActive = true, guiActiveUnfocused = true, guiName = "Toggle Bookmark", active = true)]
        public void ToggleBookmarkEvent() {
            try {
                if (part == null) {
                    LOGGER.LogWarning($"ToggleBookmarkEvent: Part is null");
                    return;
                }
                LOGGER.LogDebug($"ToggleBookmarkEvent: Part flightID {part.flightID}");

                Bookmark bookmark = BookmarksManager.Instance.GetBookmark(BookmarkType.CommandModule, part.flightID);
                
                if (bookmark != null) {
                    LOGGER.LogDebug($"ToggleBookmarkEvent: Removing command module bookmark for part {part.flightID}");
                    string displayName = !string.IsNullOrEmpty(bookmark.BookmarkTitle)
                        ? bookmark.BookmarkTitle
                        : ModLocalization.GetString("VBM_labelModuleNotFound");
                    ConfirmRemovalDialog.ConfirmRemoval(() => {
                        bool removed = BookmarksManager.Instance.RemoveBookmark(bookmark);
                        if (removed) {
                            ScreenMessages.PostScreenMessage(
                                ModLocalization.GetString("messageBookmarkRemoved"), 
                                2f, 
                                ScreenMessageStyle.UPPER_CENTER
                            );
                        }
                    }, bookmarkName: displayName);
                } else {
                    LOGGER.LogDebug($"ToggleBookmarkEvent: Adding command module bookmark for part {part.flightID}");
                    CommandModuleBookmark newCommandModuleBookmark = new CommandModuleBookmark(part.flightID);
                    BookmarkRefreshManager.Instance.RefreshBookmark(newCommandModuleBookmark);
                    bool added = BookmarksManager.Instance.AddBookmark(newCommandModuleBookmark);
                    if (added) {
                        ScreenMessages.PostScreenMessage(
                            ModLocalization.GetString("messageBookmarkAdded"), 
                            2f, 
                            ScreenMessageStyle.UPPER_CENTER
                        );
                    } else {
                        LOGGER.LogDebug($"ToggleBookmarkEvent: Bookmark not added for part {part.flightID}");
                        ScreenMessages.PostScreenMessage(
                            ModLocalization.GetString("messageBookmarkAdded"), 
                            2f, 
                            ScreenMessageStyle.UPPER_CENTER
                        );
                    }
                }
                
                UpdateEventName();
            } catch (Exception e) {
                LOGGER.LogError($"Error in ToggleBookmarkEvent: {e.Message}");
            }
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
            try {
                if (part == null || Events == null) {
                    LOGGER.LogWarning($"UpdateEventName: Part is null or Events is null");
                    return;
                }

                BaseEvent bookmarkEvent = Events["ToggleBookmarkEvent"];
                if (bookmarkEvent != null) {
                    bool hasBookmark = BookmarksManager.Instance.HasBookmark(BookmarkType.CommandModule, part.flightID);
                    bookmarkEvent.guiName = hasBookmark 
                        ? ModLocalization.GetString("contextMenuRemoveFromBookmarks")
                        : ModLocalization.GetString("contextMenuAddToBookmarks");
                }
            } catch (Exception e) {
                LOGGER.LogError($"Error in UpdateEventName: {e.Message}");
            }
        }
    }
}
