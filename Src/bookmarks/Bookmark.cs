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
        /// Draw the title of the bookmark
        /// </summary>
        public abstract string GetBookmarkTitle();

        /// <summary>
        /// Draw the type of the bookmark
        /// </summary>
        public abstract VesselType GetBookmarkDisplayType();
        
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
        
        // ==============================================================
        //      Associated vessel data
        //        Recomputed on refresh, but stored for display 
        //        and caching purposes
        // ==============================================================

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
        public VesselType VesselType { get; set; } = VesselType.Unknown;

        /// <summary>
        /// The main body of the vessel
        /// </summary>
        public CelestialBody VesselBody { get; set; } = null;

        /// <summary>
        /// The situation of the vessel
        /// </summary>
        public Vessel.Situations VesselSituation { get; set; } = Vessel.Situations.PRELAUNCH;
        
        /// <summary>
        /// If the bookmark has an alarm
        /// </summary>
        public bool HasAlarm { get; set; } = false;

        // ============================================================
        //  Transient fields. Not saved, but used for display purposes
        // ============================================================
        
        /// <summary>
        /// The vessel for the bookmark (not saved)
        /// </summary>
        public Vessel Vessel { get; set; } = null;

        /// <summary>
        /// The label of the vessel situation
        /// </summary>
        public string VesselSituationLabel { get; set; } = "";

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bookmarkType">The type of the bookmark</param>
        public Bookmark(BookmarkType bookmarkType, uint bookmarkID) {
            BookmarkType = bookmarkType;
            BookmarkID = bookmarkID;
        }
    }
}
