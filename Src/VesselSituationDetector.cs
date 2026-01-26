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
                    return "Unknown";
                }
                ModLogger.LogDebug($"Getting situation for vessel {vessel.vesselName}");
                
                // Check if vessel is destroyed
                if (vessel.state == Vessel.State.DEAD) {
                    ModLogger.LogDebug($"- Vessel is destroyed");
                    return "Destroyed";
                }
                
                CelestialBody mainBody = vessel.mainBody;
                string bodyName = mainBody != null ? mainBody.bodyName : "Unknown";
                
                ModLogger.LogDebug($"- Main body: {bodyName}");
                ModLogger.LogDebug($"- Situation: {vessel.situation}");
                switch (vessel.situation) {
                    case Vessel.Situations.LANDED:
                        return $"Landed on {bodyName}";
                        
                    case Vessel.Situations.SPLASHED:
                        return $"Splashed down in {bodyName}'s ocean";
                        
                    case Vessel.Situations.PRELAUNCH:
                        return $"On launchpad ({bodyName})";
                        
                    case Vessel.Situations.SUB_ORBITAL:
                        return $"Suborbital ({bodyName})";
                        
                    case Vessel.Situations.ORBITING:
                        return $"Orbiting {bodyName}";
                        
                    case Vessel.Situations.ESCAPING:
                        return $"Escape trajectory ({bodyName})";
                        
                    default:
                        return $"In flight ({bodyName})";
                }
            } catch (System.Exception e) {
                ModLogger.LogError($"Error getting situation for vessel {vessel.vesselName}: {e.Message}");
                return "Unknown";
            }
        }
    }
}
