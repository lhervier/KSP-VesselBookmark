using System;
using UnityEngine;
using com.github.lhervier.ksp.bookmarksmod;

namespace com.github.lhervier.ksp.bookmarksmod.bookmarks {
    
    /// <summary>
    /// Represents a bookmark to a command module of a vessel
    /// </summary>
    public abstract class Bookmark {

        /// <summary>
        /// Get the unique identifier of the bookmark
        /// </summary>
        /// <returns>The unique identifier of the bookmark</returns>
        public abstract string GetBookmarkID();

        /// <summary>
        /// Type of the bookmark
        /// </summary>
        public abstract BookmarkType GetBookmarkType();

        /// <summary>
        /// User-editable comment
        /// </summary>
        public string Comment { get; set; }
        
        /// <summary>
        /// Persistent identifier of the vessel
        /// Only used for vessel bookmarks
        /// </summary>
        public uint VesselPersistentID { get; set; }

        /// <summary>
        /// Name of the vessel
        /// </summary>
        public string VesselName { get; set; }

        /// <summary>
        /// Type of the vessel
        /// </summary>
        public VesselType VesselType { get; set; }

        /// <summary>
        /// Vessel position
        /// </summary>
        public string VesselSituation { get; set; }

        /// <summary>
        /// Custom order for sorting bookmarks (lower values appear first)
        /// </summary>
        public int Order { get; set; }
        
        /// <summary>
        /// Bookmark creation date
        /// </summary>
        public double CreationTime { get; set; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        protected Bookmark() {
            Comment = "";
            VesselSituation = "";
            VesselPersistentID = 0;
            VesselName = "";
            VesselType = VesselType.Unknown;
            Order = 0;
            CreationTime = Planetarium.GetUniversalTime();
        }

        public Bookmark(ConfigNode node) : this() {
            Load(node);
        }
        
        /// <summary>
        /// Saves the specific data of the bookmark to a ConfigNode
        /// </summary>
        /// <param name="node">The ConfigNode to save the specific data to</param>
        protected abstract void SaveSpecificData(ConfigNode node);

        /// <summary>
        /// Saves the bookmark to a ConfigNode
        /// </summary>
        public void Save(ConfigNode node) {
            node.AddValue("bookmarkType", (int) GetBookmarkType());
            node.AddValue("comment", Comment);
            node.AddValue("creationTime", CreationTime);
            node.AddValue("vesselSituation", VesselSituation);
            node.AddValue("vesselPersistentID", VesselPersistentID);
            node.AddValue("vesselName", VesselName);
            node.AddValue("vesselType", (int) VesselType);
            node.AddValue("order", Order);
            SaveSpecificData(node);
        }
        
        /// <summary>
        /// Loads the specific data of the bookmark from a ConfigNode
        /// </summary>
        /// <param name="node">The ConfigNode to load the specific data from</param>
        protected abstract void LoadSpecificData(ConfigNode node);
        
        /// <summary>
        /// Loads the bookmark from a ConfigNode
        /// </summary>
        /// <param name="node">The ConfigNode to load the bookmark from</param>
        public void Load(ConfigNode node) {
            Comment = node.GetValue("comment") ?? "";
            
            if( node.HasValue("vesselPersistentID") ) {
                uint.TryParse(node.GetValue("vesselPersistentID"), out uint persistentID);
                VesselPersistentID = persistentID;
            } else {
                throw new Exception("vesselPersistentID not found in the bookmark node");
            }

            VesselSituation = node.GetValue("vesselSituation") ?? "";
            VesselName = node.GetValue("vesselName") ?? "";
            
            if (node.HasValue("vesselType")) {
                int.TryParse(node.GetValue("vesselType"), out int vesselType);
                VesselType = (VesselType) vesselType;
            } else {
                throw new Exception("vesselType not found in the bookmark node");
            }
            
            if (node.HasValue("order")) {
                int.TryParse(node.GetValue("order"), out int order);
                Order = order;
            } else {
                throw new Exception("order not found in the bookmark node");
            }

            if (node.HasValue("creationTime")) {
                double.TryParse(node.GetValue("creationTime"), out double time);
                CreationTime = time;
            } else {
                throw new Exception("creationTime not found in the bookmark node");
            }
            
            LoadSpecificData(node);
        }
        
        /// <summary>
        /// Get the vessel for the bookmark
        /// </summary>
        /// <returns>The vessel for the bookmark</returns>
        public Vessel GetVessel() {
            try {
                ModLogger.LogDebug($"Getting vessel for bookmark {GetBookmarkID()}");
                if( VesselPersistentID == 0 ) {
                    ModLogger.LogWarning($"Bookmark {GetBookmarkID()}: Vessel persistent ID is empty");
                    return null;
                }
                foreach (Vessel vessel in FlightGlobals.Vessels) {
                    if (vessel == null || vessel.persistentId != VesselPersistentID) continue;
                    return vessel;
                }
                foreach (Vessel vessel in FlightGlobals.VesselsUnloaded) {
                    if (vessel == null || vessel.persistentId != VesselPersistentID) continue;
                    return vessel;
                }
                return null;
            } catch (Exception e) {
                ModLogger.LogError($"Error getting vessel for bookmark {GetBookmarkID()}: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Refresh the bookmark
        /// </summary>
        /// <returns>True if the bookmark was refreshed, false otherwise</returns>
        protected abstract bool RefreshSpecific();

        /// <summary>
        /// Refresh the bookmark
        /// </summary>
        /// <returns>True if the bookmark was refreshed, false otherwise</returns>
        public bool Refresh(bool sendEvent) {
            try {
                ModLogger.LogDebug($"Refreshing bookmark for bookmark {GetBookmarkID()}");
                if( !this.RefreshSpecific() ) {
                    ModLogger.LogWarning($"Bookmark {GetBookmarkID()}: Failed to refresh specific data");
                    return false;
                }

                Vessel vessel = this.GetVessel();
                if( vessel == null ) {
                    ModLogger.LogWarning($"Bookmark {GetBookmarkID()}: Vessel not found");
                    return false;
                }
                VesselSituation = VesselSituationDetector.GetSituation(vessel);
                VesselName = vessel.vesselName;
                VesselType = vessel.vesselType;

                if( sendEvent ) {
                    VesselBookmarkManager.Instance.OnBookmarksUpdated.Fire();
                }

                return true;
            } catch (Exception e) {
                ModLogger.LogError($"Error refreshing bookmark for bookmark {GetBookmarkID()}: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Draw the title of the bookmark
        /// </summary>
        public abstract string GetBookmarkDisplayName();

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
