using System;
using UnityEngine;

namespace com.github.lhervier.ksp {
    
    /// <summary>
    /// Représente un bookmark vers un module de commande d'un vaisseau
    /// </summary>
    public class VesselBookmark {
        
        /// <summary>
        /// Identifiant unique du module de commande (utilise Part.flightID)
        /// </summary>
        public uint CommandModuleFlightID { get; set; }
        
        /// <summary>
        /// Commentaire modifiable par l'utilisateur
        /// </summary>
        public string Comment { get; set; }
        
        /// <summary>
        /// Nom du module de commande (mis à jour dynamiquement)
        /// </summary>
        public string CommandModuleName { get; set; }
        
        /// <summary>
        /// Date de création du bookmark
        /// </summary>
        public double CreationTime { get; set; }
        
        public VesselBookmark() {
            Comment = "";
            CommandModuleName = "";
            CreationTime = Planetarium.GetUniversalTime();
        }
        
        public VesselBookmark(uint flightID, string comment = "") {
            CommandModuleFlightID = flightID;
            Comment = comment ?? "";
            CommandModuleName = "";
            CreationTime = Planetarium.GetUniversalTime();
        }
        
        /// <summary>
        /// Sauvegarde le bookmark dans un ConfigNode
        /// </summary>
        public void Save(ConfigNode node) {
            node.AddValue("commandModuleFlightID", CommandModuleFlightID);
            node.AddValue("comment", Comment);
            node.AddValue("commandModuleName", CommandModuleName);
            // Compatibilité avec les anciennes sauvegardes
            if (!string.IsNullOrEmpty(CommandModuleName)) {
                node.AddValue("vesselName", CommandModuleName);
            }
            node.AddValue("creationTime", CreationTime);
        }
        
        /// <summary>
        /// Charge le bookmark depuis un ConfigNode
        /// </summary>
        public void Load(ConfigNode node) {
            if (node.HasValue("commandModuleFlightID")) {
                uint.TryParse(node.GetValue("commandModuleFlightID"), out uint flightID);
                CommandModuleFlightID = flightID;
            }
            
            Comment = node.GetValue("comment") ?? "";
            
            // Charger le nom du module de commande (nouveau format)
            CommandModuleName = node.GetValue("commandModuleName") ?? "";
            
            // Compatibilité avec les anciennes sauvegardes qui utilisaient "vesselName"
            if (string.IsNullOrEmpty(CommandModuleName) && node.HasValue("vesselName")) {
                CommandModuleName = node.GetValue("vesselName") ?? "";
            }
            
            if (node.HasValue("creationTime")) {
                double.TryParse(node.GetValue("creationTime"), out double time);
                CreationTime = time;
            } else {
                CreationTime = Planetarium.GetUniversalTime();
            }
        }
    }
}
