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
        /// Get the unique identifier of the bookmark
        /// </summary>
        /// <returns>The unique identifier of the bookmark</returns>
        public abstract uint GetBookmarkID();

        /// <summary>
        /// Type of the bookmark
        /// </summary>
        public abstract BookmarkType GetBookmarkType();

        // =============================================================
        //      Associated vessel data
        // =============================================================

        /// <summary>
        /// Persistent identifier of the vessel
        /// Only used for vessel bookmarks
        /// </summary>
        public uint VesselPersistentID { get; set; } = 0;

        // =========== Cached vessel data ===========

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
        
        // ==================
        //  Transient fields
        // ==================
        
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
        public Bookmark() {
        }

        /// <summary>
        /// Draw the title of the bookmark
        /// </summary>
        public abstract string GetBookmarkTitle();

        /// <summary>
        /// Draw the type of the bookmark
        /// </summary>
        public abstract VesselType GetBookmarkDisplayType();
        
        /// <summary>
        /// Should draw the part of the bookmark
        /// </summary>
        /// <returns>True if the part of the bookmark should be drawn, false otherwise</returns>
        public abstract bool ShouldDrawPartOf();
    }
}
