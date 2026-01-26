using System;

namespace com.github.lhervier.ksp.bookmarks {
    public class VesselBookmark : Bookmark {
        
        /// <summary>
        /// Compute the unique identifier of the bookmark
        /// </summary>
        /// <param name="vesselPersistentID">The persistent ID of the vessel</param>
        /// <returns>The unique identifier of the bookmark</returns>
        public static string ComputeBookmarkID(uint vesselPersistentID) {
            return $"{BookmarkType.Vessel}:{vesselPersistentID}";
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
        /// Constructor
        /// </summary>
        /// <param name="node">The ConfigNode to load the bookmark from</param>
        public VesselBookmark(ConfigNode node) : base(node) {
        }

        /// <summary>
        /// Get the unique identifier of the bookmark
        /// </summary>
        /// <returns>The unique identifier of the bookmark</returns>
        public override string GetBookmarkID() {
            return ComputeBookmarkID(VesselPersistentID);
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
    }
}