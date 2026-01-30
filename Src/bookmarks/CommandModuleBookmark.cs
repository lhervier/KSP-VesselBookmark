using System;
using UnityEngine;
using com.github.lhervier.ksp;

namespace com.github.lhervier.ksp.bookmarksmod.bookmarks {
    public class CommandModuleBookmark : Bookmark {
        
        /// <summary>
        /// Unique identifier of the command module (uses Part.flightID)
        /// </summary>
        public uint CommandModuleFlightID { get; set; } = 0;
        
        /// <summary>
        /// Command module name (updated dynamically)
        /// </summary>
        public string CommandModuleName { get; set; } = "";

        /// <summary>
        /// Command module type
        /// </summary>
        public VesselType CommandModuleType { get; set; } = VesselType.Unknown;
        
        /// <summary>
        /// Get the display name of the command module bookmark
        /// </summary>
        /// <returns>The display name of the command module bookmark</returns>
        public override string GetBookmarkTitle() {
            return CommandModuleName;
        }

        /// <summary>
        /// Get the display type of the command module bookmark
        /// </summary>
        /// <returns>The display type of the command module bookmark</returns>
        public override VesselType GetBookmarkDisplayType() {
            return CommandModuleType;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bookmarkID">The ID of the bookmark</param>
        public CommandModuleBookmark(uint bookmarkID) : base(BookmarkType.CommandModule, bookmarkID) {
        }
    }
}