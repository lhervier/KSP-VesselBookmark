using System;
using UnityEngine;

namespace com.github.lhervier.ksp {
    
    /// <summary>
    /// Represents a bookmark to a command module of a vessel
    /// </summary>
    public class VesselBookmark {
        
        /// <summary>
        /// Unique identifier of the command module (uses Part.flightID)
        /// </summary>
        public uint CommandModuleFlightID { get; set; }
        
        /// <summary>
        /// User-editable comment
        /// </summary>
        public string Comment { get; set; }
        
        /// <summary>
        /// Command module name (updated dynamically)
        /// </summary>
        public string CommandModuleName { get; set; }
        
        /// <summary>
        /// Bookmark creation date
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
        /// Saves the bookmark to a ConfigNode
        /// </summary>
        public void Save(ConfigNode node) {
            node.AddValue("commandModuleFlightID", CommandModuleFlightID);
            node.AddValue("comment", Comment);
            node.AddValue("commandModuleName", CommandModuleName);
            // Compatibility with old save files
            if (!string.IsNullOrEmpty(CommandModuleName)) {
                node.AddValue("vesselName", CommandModuleName);
            }
            node.AddValue("creationTime", CreationTime);
        }
        
        /// <summary>
        /// Loads the bookmark from a ConfigNode
        /// </summary>
        public void Load(ConfigNode node) {
            if (node.HasValue("commandModuleFlightID")) {
                uint.TryParse(node.GetValue("commandModuleFlightID"), out uint flightID);
                CommandModuleFlightID = flightID;
            }
            
            Comment = node.GetValue("comment") ?? "";
            
            // Load command module name (new format)
            CommandModuleName = node.GetValue("commandModuleName") ?? "";
            
            // Compatibility with old save files that used "vesselName"
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
