using UnityEngine;

namespace com.github.lhervier.ksp {
    
    /// <summary>
    /// Gère la navigation vers les vaisseaux
    /// </summary>
    public static class VesselNavigator {
        
        /// <summary>
        /// Navigue vers un vaisseau (change le vaisseau actif)
        /// Gère les vaisseaux chargés et non chargés, ainsi que la vue carte
        /// Fonctionne depuis le Tracking Station, MapView et en vol
        /// </summary>
        public static bool NavigateToVessel(Vessel vessel) {
            if (vessel == null) {
                Debug.LogError("[VesselBookmarkMod] Tentative de navigation vers un vaisseau null");
                return false;
            }
            
            // Vérifier si on est déjà sur ce vaisseau (seulement si on est en vol)
            if (HighLogic.LoadedScene == GameScenes.FLIGHT && FlightGlobals.ActiveVessel == vessel) {
                Debug.Log($"[VesselBookmarkMod] Déjà sur le vaisseau {vessel.vesselName}");
                return true;
            }
            
            // Changer le vaisseau actif
            // FlightGlobals.SetActiveVessel() gère automatiquement :
            // - Le chargement de la scène de vol depuis le Tracking Station
            // - Le rechargement de la scène si le vaisseau n'est pas chargé
            // - Le centrage de la vue en mode carte
            // - La mise à jour de la caméra
            // Comme le fait le menu "changer" de KSP
            try {
                FlightGlobals.SetActiveVessel(vessel);
                Debug.Log($"[VesselBookmarkMod] Navigation vers {vessel.vesselName} (scène: {HighLogic.LoadedScene}, chargé: {vessel.loaded})");
                return true;
            } catch (System.Exception e) {
                Debug.LogError($"[VesselBookmarkMod] Erreur lors de la navigation vers {vessel.vesselName}: {e.Message}");
                return false;
            }
        }
        
    }
}
