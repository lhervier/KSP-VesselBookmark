using System;
using UnityEngine;
using com.github.lhervier.ksp.bookmarksmod.bookmarks;
using com.github.lhervier.ksp.bookmarksmod.ui;

namespace com.github.lhervier.ksp.bookmarksmod {
    
    /// <summary>
    /// Custom module that adds bookmark action to command modules
    /// Injected into command parts via ModuleManager configuration
    /// </summary>
    public class VesselBookmarkPartModule : PartModule {
        
        [KSPEvent(guiActive = true, guiActiveUnfocused = true, guiName = "Toggle Bookmark", active = true)]
        public void ToggleBookmarkEvent() {
            try {
                if (part == null) {
                    ModLogger.LogWarning($"ToggleBookmarkEvent: Part is null");
                    return;
                }
                ModLogger.LogDebug($"ToggleBookmarkEvent: Part flightID {part.flightID}");

                BookmarkManager manager = BookmarkManager.GetInstance(BookmarkType.CommandModule);
                Bookmark bookmark = manager.GetBookmark(part.flightID);
                
                if (bookmark != null) {
                    ModLogger.LogDebug($"ToggleBookmarkEvent: Removing command module bookmark for part {part.flightID}");
                    string displayName = !string.IsNullOrEmpty(bookmark.BookmarkTitle)
                        ? bookmark.BookmarkTitle
                        : ModLocalization.GetString("labelModuleNotFound");
                    VesselBookmarkUIDialog.ConfirmRemoval(() => {
                        bool removed = BookmarkManager.RemoveBookmark(bookmark);
                        if (removed) {
                            ScreenMessages.PostScreenMessage(
                                ModLocalization.GetString("messageBookmarkRemoved"), 
                                2f, 
                                ScreenMessageStyle.UPPER_CENTER
                            );
                        }
                    }, bookmarkName: displayName);
                } else {
                    ModLogger.LogDebug($"ToggleBookmarkEvent: Adding command module bookmark for part {part.flightID}");
                    CommandModuleBookmark newCommandModuleBookmark = new CommandModuleBookmark(part.flightID);
                    bool added = BookmarkManager.AddBookmark(newCommandModuleBookmark);
                    if (added) {
                        ScreenMessages.PostScreenMessage(
                            ModLocalization.GetString("messageBookmarkAdded"), 
                            2f, 
                            ScreenMessageStyle.UPPER_CENTER
                        );
                    } else {
                        ModLogger.LogDebug($"ToggleBookmarkEvent: Bookmark not added for part {part.flightID}");
                        ScreenMessages.PostScreenMessage(
                            ModLocalization.GetString("messageBookmarkAdded"), 
                            2f, 
                            ScreenMessageStyle.UPPER_CENTER
                        );
                    }
                }
                
                UpdateEventName();
            } catch (Exception e) {
                ModLogger.LogError($"Error in ToggleBookmarkEvent: {e.Message}");
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
                    ModLogger.LogWarning($"UpdateEventName: Part is null or Events is null");
                    return;
                }

                BookmarkManager manager = BookmarkManager.GetInstance(BookmarkType.CommandModule);
                
                BaseEvent bookmarkEvent = Events["ToggleBookmarkEvent"];
                if (bookmarkEvent != null) {
                    bool hasBookmark = manager.HasBookmark(part.flightID);
                    bookmarkEvent.guiName = hasBookmark 
                        ? ModLocalization.GetString("contextMenuRemoveFromBookmarks")
                        : ModLocalization.GetString("contextMenuAddToBookmarks");
                }
            } catch (Exception e) {
                ModLogger.LogError($"Error in UpdateEventName: {e.Message}");
            }
        }
    }
}
