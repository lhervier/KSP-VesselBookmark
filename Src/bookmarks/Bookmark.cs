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
        
        // =====================================================================

        /// <summary>
        /// Find the vessel for the bookmark
        /// </summary>
        /// <returns>The vessel for the bookmark</returns>
        private Vessel FindVessel() {
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

        private bool CheckHasAlarm() {
            try {
                if( Vessel == null ) {
                    ModLogger.LogWarning($"Bookmark {GetBookmarkID()}: Vessel not found");
                    return false;
                }

                DictionaryValueList<uint, AlarmTypeBase> alarms = AlarmClockScenario.Instance.alarms;
                foreach (AlarmTypeBase alarm in alarms.Values) {
                    if( alarm.Vessel == null ) {
                        continue;
                    }
                    if( alarm.Vessel.persistentId == Vessel.persistentId ) {
                        return true;
                    }
                }
                return false;
            } catch (Exception e) {
                ModLogger.LogError($"Error checking if bookmark {GetBookmarkID()} has an alarm: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets a textual description of vessel situation
        /// </summary>
        /// <returns>The label for the situation</returns>
        public string GetSituationLabel() {
            try {
                if( VesselBody == null ) {
                    ModLogger.LogError($"Getting situation: Body is null");
                    return ModLocalization.GetString("situationUnknown");
                }
                ModLogger.LogDebug($"Getting situation labeled as {VesselSituation} for vessel on body {VesselBody.bodyName}");
                
                switch (VesselSituation) {
                    case Vessel.Situations.LANDED:
                        return ModLocalization.GetString("situationLanded", VesselBody.bodyName);
                        
                    case Vessel.Situations.SPLASHED:
                        return ModLocalization.GetString("situationSplashed", VesselBody.bodyName);
                        
                    case Vessel.Situations.PRELAUNCH:
                        return ModLocalization.GetString("situationPrelaunch", VesselBody.bodyName);
                        
                    case Vessel.Situations.SUB_ORBITAL:
                        return ModLocalization.GetString("situationSuborbital", VesselBody.bodyName);
                        
                    case Vessel.Situations.ORBITING:
                        return ModLocalization.GetString("situationOrbiting", VesselBody.bodyName);
                        
                    case Vessel.Situations.ESCAPING:
                        return ModLocalization.GetString("situationEscaping", VesselBody.bodyName);
                        
                    default:
                        return ModLocalization.GetString("situationInFlight", VesselBody.bodyName);
                }
            } catch (System.Exception e) {
                ModLogger.LogError($"Error getting situation labeled as {VesselSituation} for vessel on body {VesselBody.bodyName}: {e.Message}");
                return ModLocalization.GetString("situationUnknown");
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

                Vessel = this.FindVessel();
                if( Vessel == null ) {
                    ModLogger.LogWarning($"Bookmark {GetBookmarkID()}: Vessel not found");
                    return false;
                }
                
                VesselName = Vessel.vesselName;
                VesselType = Vessel.vesselType;
                
                VesselSituation = Vessel.situation;
                VesselBody = Vessel.mainBody;
                VesselSituationLabel = GetSituationLabel();
                
                HasAlarm = CheckHasAlarm();

                if( sendEvent ) {
                    BookmarkManager.Instance.OnBookmarksUpdated.Fire();
                }

                return true;
            } catch (Exception e) {
                ModLogger.LogError($"Error refreshing bookmark for bookmark {GetBookmarkID()}: {e.Message}");
                return false;
            }
        }
    }
}
