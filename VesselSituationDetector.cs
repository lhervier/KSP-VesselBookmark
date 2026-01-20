using UnityEngine;

namespace com.github.lhervier.ksp {
    
    /// <summary>
    /// Détecte et décrit la situation d'un vaisseau
    /// </summary>
    public static class VesselSituationDetector {
        
        /// <summary>
        /// Obtient une description textuelle de la situation d'un vaisseau
        /// </summary>
        public static string GetSituation(Vessel vessel) {
            if (vessel == null) {
                return "Vaisseau introuvable";
            }
            
            // Vérifier si le vaisseau est détruit
            if (vessel.state == Vessel.State.DEAD) {
                return "Détruit";
            }
            
            // Vérifier si le vaisseau n'est pas chargé
            if (!vessel.loaded) {
                return GetSituationForUnloadedVessel(vessel);
            }
            
            CelestialBody mainBody = vessel.mainBody;
            string bodyName = mainBody != null ? mainBody.displayName : "Inconnu";
            
            switch (vessel.situation) {
                case Vessel.Situations.LANDED:
                    return $"Posé sur {bodyName}";
                    
                case Vessel.Situations.SPLASHED:
                    return $"Dans l'océan de {bodyName}";
                    
                case Vessel.Situations.PRELAUNCH:
                    return $"Sur le pas de tir ({bodyName})";
                    
                case Vessel.Situations.SUB_ORBITAL:
                    return $"Suborbital ({bodyName})";
                    
                case Vessel.Situations.ORBITING:
                    return $"En orbite autour de {bodyName}";
                    
                case Vessel.Situations.ESCAPING:
                    return $"Trajectoire de libération ({bodyName})";
                    
                default:
                    return $"En vol ({bodyName})";
            }
        }
        
        /// <summary>
        /// Obtient la situation pour un vaisseau non chargé
        /// </summary>
        private static string GetSituationForUnloadedVessel(Vessel vessel) {
            if (vessel.protoVessel == null) {
                return "Vaisseau introuvable";
            }
            
            CelestialBody mainBody = vessel.mainBody;
            string bodyName = mainBody != null ? mainBody.displayName : "Inconnu";
            
            switch (vessel.situation) {
                case Vessel.Situations.LANDED:
                    return $"Posé sur {bodyName}";
                    
                case Vessel.Situations.SPLASHED:
                    return $"Dans l'océan de {bodyName}";
                    
                case Vessel.Situations.PRELAUNCH:
                    return $"Sur le pas de tir ({bodyName})";
                    
                case Vessel.Situations.SUB_ORBITAL:
                    return $"Suborbital ({bodyName})";
                    
                case Vessel.Situations.ORBITING:
                    return $"En orbite autour de {bodyName}";
                    
                case Vessel.Situations.ESCAPING:
                    return $"Trajectoire de libération ({bodyName})";
                    
                default:
                    return $"En vol ({bodyName})";
            }
        }
    }
}
