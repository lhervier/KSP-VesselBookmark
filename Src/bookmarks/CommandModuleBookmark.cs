using System;
using UnityEngine;
using com.github.lhervier.ksp;

namespace com.github.lhervier.ksp.bookmarksmod.bookmarks {
    public class CommandModuleBookmark : Bookmark {
        
        /// <summary>
        /// Unique identifier of the command module (uses Part.flightID)
        /// </summary>
        public uint CommandModuleFlightID { get; set; } = 0;
        
        // =============================================================
        //      Associated command module data
        //        Recomputed on refresh, but stored for display 
        //        and caching purposes
        // =============================================================

        /// <summary>
        /// Command module name (updated dynamically)
        /// </summary>
        public string CommandModuleName { get; set; } = "";

        /// <summary>
        /// Command module type
        /// </summary>
        public VesselType CommandModuleType { get; set; } = VesselType.Unknown;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="flightID">The flight ID of the command module</param>
        public CommandModuleBookmark(uint flightID) : base(BookmarkType.CommandModule, flightID) {
        }
    }
}