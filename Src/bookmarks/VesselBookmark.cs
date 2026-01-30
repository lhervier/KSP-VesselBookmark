using System;

namespace com.github.lhervier.ksp.bookmarksmod.bookmarks {

    /// <summary>
    /// Represents a bookmark for a vessel
    /// </summary>
    public class VesselBookmark : Bookmark {
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bookmarkID">The ID of the bookmark</param>
        public VesselBookmark(uint bookmarkID) : base(BookmarkType.Vessel, bookmarkID) {
        }
    }
}