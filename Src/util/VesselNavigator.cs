using UnityEngine;

namespace com.github.lhervier.ksp.bookmarksmod.util {
    
    /// <summary>
    /// Manages navigation to vessels
    /// </summary>
    public static class VesselNavigator {

        private static readonly ModLogger LOGGER = new ModLogger("VesselNavigator");

        /// <summary>
        /// Navigate to a vessel (changes active vessel)
        /// Handles loaded and unloaded vessels, as well as map view
        /// Works from Tracking Station, MapView and in flight
        /// </summary>
        public static bool NavigateToVessel(Vessel vessel) {
            if (vessel == null) {
                LOGGER.LogError("Attempted to navigate to null vessel");
                return false;
            }

            LOGGER.LogDebug($"Navigating to vessel {vessel.vesselName}");

            // Check if already on this vessel (only if in flight)
            if (HighLogic.LoadedScene == GameScenes.FLIGHT && FlightGlobals.ActiveVessel == vessel) {
                LOGGER.LogDebug($"Already on vessel {vessel.vesselName}");
                return true;
            }
            
            // If we are ont the flight scene
            if (HighLogic.LoadedSceneIsFlight)
            {
                try {
                    FlightGlobals.SetActiveVessel(vessel);
                    LOGGER.LogDebug($"Navigating to {vessel.vesselName} (scene: {HighLogic.LoadedScene}, loaded: {vessel.loaded})");
                    return true;
                } catch (System.Exception e) {
                    LOGGER.LogError($"Error navigating to {vessel.vesselName}: {e.Message}");
                    return false;
                }
            
            // Otherwise, we need to save and reload the game
            } else {
                // First, save the game, forcing the flight scene
                Game currentGame;
                if (HighLogic.CurrentGame == null)
                {
                    HighLogic.CurrentGame = new Game().Updated(GameScenes.FLIGHT);
                    currentGame = HighLogic.CurrentGame;
                }
                else 
                {
                    currentGame = HighLogic.CurrentGame.Updated(GameScenes.FLIGHT);
                }
                GamePersistence.SaveGame(currentGame, "persistent", HighLogic.SaveFolder, SaveMode.OVERWRITE);

                // Then, load the saved game, and focus on the vessel
                FlightDriver.StartAndFocusVessel("persistent", FlightGlobals.Vessels.IndexOf(vessel));

                return true;
            }
        }
        
    }
}
