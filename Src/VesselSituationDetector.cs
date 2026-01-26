using UnityEngine;

namespace com.github.lhervier.ksp {
    
    /// <summary>
    /// Detects and describes vessel situation
    /// </summary>
    public static class VesselSituationDetector {
        
        /// <summary>
        /// Gets a textual description of vessel situation
        /// </summary>
        public static string GetSituation(Vessel vessel) {
            try {
                if (vessel == null) {
                    ModLogger.LogError($"Getting situation: Vessel is null");
                    return VesselBookmarkLocalization.GetString("situationUnknown");
                }
                ModLogger.LogDebug($"Getting situation for vessel {vessel.vesselName}");
                
                // Check if vessel is destroyed
                if (vessel.state == Vessel.State.DEAD) {
                    ModLogger.LogDebug($"- Vessel is destroyed");
                    return VesselBookmarkLocalization.GetString("situationDestroyed");
                }
                
                CelestialBody mainBody = vessel.mainBody;
                string bodyName = mainBody != null ? mainBody.bodyName : VesselBookmarkLocalization.GetString("situationUnknown");
                
                ModLogger.LogDebug($"- Main body: {bodyName}");
                ModLogger.LogDebug($"- Situation: {vessel.situation}");
                switch (vessel.situation) {
                    case Vessel.Situations.LANDED:
                        return VesselBookmarkLocalization.GetString("situationLanded", bodyName);
                        
                    case Vessel.Situations.SPLASHED:
                        return VesselBookmarkLocalization.GetString("situationSplashed", bodyName);
                        
                    case Vessel.Situations.PRELAUNCH:
                        return VesselBookmarkLocalization.GetString("situationPrelaunch", bodyName);
                        
                    case Vessel.Situations.SUB_ORBITAL:
                        return VesselBookmarkLocalization.GetString("situationSuborbital", bodyName);
                        
                    case Vessel.Situations.ORBITING:
                        return VesselBookmarkLocalization.GetString("situationOrbiting", bodyName);
                        
                    case Vessel.Situations.ESCAPING:
                        return VesselBookmarkLocalization.GetString("situationEscaping", bodyName);
                        
                    default:
                        return VesselBookmarkLocalization.GetString("situationInFlight", bodyName);
                }
            } catch (System.Exception e) {
                ModLogger.LogError($"Error getting situation for vessel {vessel.vesselName}: {e.Message}");
                return VesselBookmarkLocalization.GetString("situationUnknown");
            }
        }
    }
}
