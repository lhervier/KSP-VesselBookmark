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
            if (vessel == null) {
                return "Vessel not found";
            }
            
            // Check if vessel is destroyed
            if (vessel.state == Vessel.State.DEAD) {
                return "Destroyed";
            }
            
            // Check if vessel is not loaded
            if (!vessel.loaded) {
                return GetSituationForUnloadedVessel(vessel);
            }
            
            CelestialBody mainBody = vessel.mainBody;
            string bodyName = mainBody != null ? mainBody.displayName : "Unknown";
            
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
        }
        
        /// <summary>
        /// Gets situation for an unloaded vessel
        /// </summary>
        private static string GetSituationForUnloadedVessel(Vessel vessel) {
            if (vessel.protoVessel == null) {
                return "Vessel not found";
            }
            
            CelestialBody mainBody = vessel.mainBody;
            string bodyName = mainBody != null ? mainBody.displayName : "Unknown";
            
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
        }
    }
}
