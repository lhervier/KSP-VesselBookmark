using System;

namespace com.github.lhervier.ksp.bookmarksmod.bookmarks {

    /// <summary>
    /// Represents a bookmark for a vessel
    /// </summary>
    public class VesselBookmark : Bookmark {
        
        /// <summary>
        /// Constructor
        /// </summary>
        public VesselBookmark() : base() {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="vesselPersistentID">The persistent ID of the vessel</param>
        public VesselBookmark(uint vesselPersistentID) : base() {
            if( vesselPersistentID == 0 ) {
                throw new Exception("vesselPersistentID cannot be 0");
            }
            VesselPersistentID = vesselPersistentID;
        }

        /// <summary>
        /// Get the unique identifier of the bookmark
        /// </summary>
        /// <returns>The unique identifier of the bookmark</returns>
        public override uint GetBookmarkID() {
            return VesselPersistentID;
        }

        /// <summary>
        /// Returns the type of the bookmark
        /// </summary>
        /// <returns>The type of the bookmark</returns>
        public override BookmarkType GetBookmarkType() {
            return BookmarkType.Vessel;
        }

        /// <summary>
        /// Saves the specific data of the vessel bookmark to a ConfigNode
        /// </summary>
        /// <param name="node">The ConfigNode to save the specific data to</param>
        protected override void SaveSpecificData(ConfigNode node) {
        }

        /// <summary>
        /// Loads the specific data of the vessel bookmark from a ConfigNode
        /// </summary>
        /// <param name="node">The ConfigNode to load the specific data from</param>
        protected override void LoadSpecificData(ConfigNode node) {
        }

        /// <summary>
        /// Refresh the vessel bookmark
        /// </summary>
        /// <returns>True if the vessel bookmark was refreshed, false otherwise</returns>
        protected override bool RefreshSpecific() {
            return true;
        }

        /// <summary>
        /// Get the display name of the vessel bookmark
        /// </summary>
        /// <returns>The display name of the vessel bookmark</returns>
        public override string GetBookmarkTitle() {
            return VesselName;
        }

        /// <summary>
        /// Get the display type of the vessel bookmark
        /// </summary>
        /// <returns>The display type of the vessel bookmark</returns>
        public override VesselType GetBookmarkDisplayType() {
            return VesselType;
        }

        /// <summary>
        /// Should draw the part of the vessel bookmark
        /// </summary>
        /// <returns>True if the part of the vessel bookmark should be drawn, false otherwise</returns>
        public override bool ShouldDrawPartOf() {
            return false;
        }
    }
}