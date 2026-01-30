using System;

namespace com.github.lhervier.ksp.bookmarksmod.bookmarks {

    /// <summary>
    /// Represents a bookmark for a vessel
    /// </summary>
    public class VesselBookmark : Bookmark {
        
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
        /// <param name="bookmarkID">The ID of the bookmark</param>
        public VesselBookmark(uint bookmarkID) : base(BookmarkType.Vessel, bookmarkID) {
        }
    }
}