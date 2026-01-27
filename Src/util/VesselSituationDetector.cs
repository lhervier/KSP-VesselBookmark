using UnityEngine;

namespace com.github.lhervier.ksp.bookmarksmod.util {
    
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
                    return ModLocalization.GetString("situationUnknown");
                }
                ModLogger.LogDebug($"Getting situation for vessel {vessel.vesselName}");
                
                // Check if vessel is destroyed
                if (vessel.state == Vessel.State.DEAD) {
                    ModLogger.LogDebug($"- Vessel is destroyed");
                    return ModLocalization.GetString("situationDestroyed");
                }
                
                CelestialBody mainBody = vessel.mainBody;
                string bodyName = mainBody != null ? mainBody.bodyName : ModLocalization.GetString("situationUnknown");
                
                ModLogger.LogDebug($"- Main body: {bodyName}");
                ModLogger.LogDebug($"- Situation: {vessel.situation}");
                switch (vessel.situation) {
                    case Vessel.Situations.LANDED:
                        return ModLocalization.GetString("situationLanded", bodyName);
                        
                    case Vessel.Situations.SPLASHED:
                        return ModLocalization.GetString("situationSplashed", bodyName);
                        
                    case Vessel.Situations.PRELAUNCH:
                        return ModLocalization.GetString("situationPrelaunch", bodyName);
                        
                    case Vessel.Situations.SUB_ORBITAL:
                        return ModLocalization.GetString("situationSuborbital", bodyName);
                        
                    case Vessel.Situations.ORBITING:
                        return ModLocalization.GetString("situationOrbiting", bodyName);
                        
                    case Vessel.Situations.ESCAPING:
                        return ModLocalization.GetString("situationEscaping", bodyName);
                        
                    default:
                        return ModLocalization.GetString("situationInFlight", bodyName);
                }
            } catch (System.Exception e) {
                ModLogger.LogError($"Error getting situation for vessel {vessel.vesselName}: {e.Message}");
                return ModLocalization.GetString("situationUnknown");
            }
        }
    }
}
