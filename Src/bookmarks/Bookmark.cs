using System;
using UnityEngine;
using com.github.lhervier.ksp.bookmarksmod;
using com.github.lhervier.ksp.bookmarksmod.util;
using System.Linq;

namespace com.github.lhervier.ksp.bookmarksmod.bookmarks {
    
    /// <summary>
    /// Base class for all bookmarks
    /// </summary>
    public abstract class Bookmark {

        // =============================================================
        //    Bookmark identifier 
        // =============================================================

        /// <summary>
        /// Unique identifier of the bookmark
        /// Value depends on the bookmark type
        /// </summary>
        public uint BookmarkID { get; protected set; } = 0;

        /// <summary>
        /// Type of the bookmark
        /// </summary>
        public BookmarkType BookmarkType { get; set; } = BookmarkType.Unknown;

        // =============================================================
        //      Other common bookmark data
        // =============================================================

        /// <summary>
        /// User-editable comment
        /// </summary>
        public string Comment { get; set; } = "";
        
        /// <summary>
        /// Custom order for sorting bookmarks (lower values appear first)
        /// </summary>
        public int Order { get; set; } = 0;
        
        /// <summary>
        /// Bookmark creation date
        /// </summary>
        public double CreationTime { get; set; } = Planetarium.GetUniversalTime();
        
        // =============================================================
        //      Fields computed from the bookmark data
        //        Not saved, but used for display purposes
        // =============================================================

        /// <summary>
        /// Draw the title of the bookmark
        /// </summary>
        public string BookmarkTitle = "";

        /// <summary>
        /// Vessel type associated to the bookmark
        /// (different from the type of the vessel attached to the bookmark)
        /// </summary>
        public string BookmarkVesselType { get; set; } = "";
        
        // ==============================================================
        //      Associated vessel data
        //        Recomputed on refresh, but stored for display 
        //        and caching purposes
        // ==============================================================

        /// <summary>
        /// The vessel for the bookmark (not saved)
        /// </summary>
        public Vessel Vessel { get; set; } = null;

        /// <summary>
        /// Persistent identifier of the vessel
        /// Only used for vessel bookmarks
        /// </summary>
        public uint VesselPersistentID { get; set; } = 0;

        /// <summary>
        /// Name of the vessel
        /// </summary>
        public string VesselName { get; set; } = "";

        /// <summary>
        /// Type of the vessel
        /// </summary>
        public string VesselType { get; set; } = "";

        /// <summary>
        /// The main body of the vessel
        /// </summary>
        public CelestialBody VesselBody { get; set; } = null;

        /// <summary>
        /// The situation of the vessel
        /// </summary>
        public Vessel.Situations VesselSituation { get; set; } = Vessel.Situations.PRELAUNCH;
        
        /// <summary>
        /// The label of the vessel situation
        /// </summary>
        public string VesselSituationLabel { get; set; } = "";

        /// <summary>
        /// If the bookmark has an alarm
        /// </summary>
        public bool HasAlarm { get; set; } = false;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bookmarkType">The type of the bookmark</param>
        public Bookmark(BookmarkType bookmarkType, uint bookmarkID) {
            BookmarkType = bookmarkType;
            BookmarkID = bookmarkID;
        }

        /// <summary>
        /// Return a string representation of the bookmark
        /// </summary>
        /// <returns>A string representation of the bookmark</returns>
        public override string ToString() {
            return $"{BookmarkID} ({BookmarkType})";
        }
    }
}
