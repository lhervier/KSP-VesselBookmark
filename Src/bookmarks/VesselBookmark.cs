using System;

namespace com.github.lhervier.ksp.bookmarksmod.bookmarks {

    /// <summary>
    /// Represents a bookmark for a vessel
    /// </summary>
    public class VesselBookmark : Bookmark {
        
        /// <summary>
        /// Get the unique identifier of the bookmark
        /// </summary>
        /// <returns>The unique identifier of the bookmark</returns>
        public override uint GetBookmarkID() {
            return VesselPersistentID;
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
        /// Constructor
        /// </summary>
        public VesselBookmark() : base(BookmarkType.Vessel) {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="vesselPersistentID">The persistent ID of the vessel</param>
        public VesselBookmark(uint vesselPersistentID) : this() {
            VesselPersistentID = vesselPersistentID;
        }
    }
}