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
        /// Constructor
        /// </summary>
        public CommandModuleBookmark() : base() {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="commandModuleFlightID">The flight ID of the command module</param>
        public CommandModuleBookmark(uint commandModuleFlightID) : base() {
            if( commandModuleFlightID == 0 ) {
                throw new Exception("commandModuleFlightID cannot be 0");
            }
            CommandModuleFlightID = commandModuleFlightID;
            CommandModuleName = "";
            CommandModuleType = VesselType.Unknown;
        }

        /// <summary>
        /// Get the unique identifier of the bookmark
        /// </summary>
        /// <returns>The unique identifier of the bookmark</returns>
        public override uint GetBookmarkID() {
            return CommandModuleFlightID;
        }

        /// <summary>
        /// Returns the type of the bookmark
        /// </summary>
        /// <returns>The type of the bookmark</returns>
        public override BookmarkType GetBookmarkType() {
            return BookmarkType.CommandModule;
        }

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
        /// Should draw the part of the command module bookmark
        /// </summary>
        /// <returns>True if the part of the command module bookmark should be drawn, false otherwise</returns>
        public override bool ShouldDrawPartOf() {
            return VesselName != CommandModuleName;
        }
    }
}