using UnityEngine;

namespace com.github.lhervier.ksp {
    
    /// <summary>
    /// Manages navigation to vessels
    /// </summary>
    public static class VesselNavigator {
        
        /// <summary>
        /// Navigate to a vessel (changes active vessel)
        /// Handles loaded and unloaded vessels, as well as map view
        /// Works from Tracking Station, MapView and in flight
        /// </summary>
        public static bool NavigateToVessel(Vessel vessel) {
            if (vessel == null) {
                Debug.LogError("[VesselBookmarkMod] Attempted to navigate to null vessel");
                return false;
            }
            
            // Check if already on this vessel (only if in flight)
            if (HighLogic.LoadedScene == GameScenes.FLIGHT && FlightGlobals.ActiveVessel == vessel) {
                Debug.Log($"[VesselBookmarkMod] Already on vessel {vessel.vesselName}");
                return true;
            }
            
            // Change active vessel
            // FlightGlobals.SetActiveVessel() automatically handles:
            // - Loading flight scene from Tracking Station
            // - Reloading scene if vessel is not loaded
            // - Centering view in map mode
            // - Updating camera
            // Like KSP's "switch" menu does
            try {
                FlightGlobals.SetActiveVessel(vessel);
                Debug.Log($"[VesselBookmarkMod] Navigating to {vessel.vesselName} (scene: {HighLogic.LoadedScene}, loaded: {vessel.loaded})");
                return true;
            } catch (System.Exception e) {
                Debug.LogError($"[VesselBookmarkMod] Error navigating to {vessel.vesselName}: {e.Message}");
                return false;
            }
        }
        
    }
}
