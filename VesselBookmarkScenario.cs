using System;
using UnityEngine;

namespace com.github.lhervier.ksp {
    
    /// <summary>
    /// ScenarioModule for persisting vessel bookmarks across game sessions
    /// </summary>
    [KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.FLIGHT | GameScenes.TRACKSTATION | GameScenes.SPACECENTER)]
    public class VesselBookmarkScenario : ScenarioModule {
        
        /// <summary>
        /// Called when the scenario is saved
        /// </summary>
        public override void OnSave(ConfigNode node) {
            base.OnSave(node);
            
            try {
                // Remove old node if it exists
                if (node.HasNode(VesselBookmarkManager.SAVE_NODE_NAME)) {
                    node.RemoveNode(VesselBookmarkManager.SAVE_NODE_NAME);
                }
                
                ConfigNode bookmarksNode = node.AddNode(VesselBookmarkManager.SAVE_NODE_NAME);
                
                // Get bookmarks from manager
                var manager = VesselBookmarkManager.Instance;
                foreach (VesselBookmark bookmark in manager.Bookmarks) {
                    ConfigNode bookmarkNode = bookmarksNode.AddNode("BOOKMARK");
                    bookmark.Save(bookmarkNode);
                }
                
                Debug.Log($"[VesselBookmarkMod] {manager.Bookmarks.Count} bookmark(s) saved to scenario");
            } catch (Exception e) {
                Debug.LogError($"[VesselBookmarkMod] Error saving bookmarks to scenario: {e.Message}");
            }
        }
        
        /// <summary>
        /// Called when the scenario is loaded
        /// </summary>
        public override void OnLoad(ConfigNode node) {
            base.OnLoad(node);
            
            try {
                // Get manager and clear existing bookmarks
                var manager = VesselBookmarkManager.Instance;
                manager.ClearBookmarks();
                
                if (node.HasNode(VesselBookmarkManager.SAVE_NODE_NAME)) {
                    ConfigNode bookmarksNode = node.GetNode(VesselBookmarkManager.SAVE_NODE_NAME);
                    ConfigNode[] bookmarkNodes = bookmarksNode.GetNodes("BOOKMARK");
                    
                    foreach (ConfigNode bookmarkNode in bookmarkNodes) {
                        VesselBookmark bookmark = new VesselBookmark();
                        try {
                            bookmark.Load(bookmarkNode);
                            manager.AddBookmarkDirectly(bookmark);
                        } catch (Exception e) {
                            Debug.LogError($"[VesselBookmarkMod] Error loading bookmark from scenario: {e.Message}");
                        }
                    }
                }
                
                // Update command module names
                manager.UpdateCommandModuleNames();
                
                Debug.Log($"[VesselBookmarkMod] {manager.Bookmarks.Count} bookmark(s) loaded from scenario");
            } catch (Exception e) {
                Debug.LogError($"[VesselBookmarkMod] Error loading bookmarks from scenario: {e.Message}");
            }
        }
    }
}
