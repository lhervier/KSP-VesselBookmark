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
        /// Vessel type
        /// </summary>
        public VesselType VesselType { get; set; }
        
        /// <summary>
        /// Vessel position
        /// </summary>
        public string VesselSituation { get; set; }

        /// <summary>
        /// Bookmark creation date
        /// </summary>
        public double CreationTime { get; set; }
        
        /// <summary>
        /// Custom order for sorting bookmarks (lower values appear first)
        /// </summary>
        public int Order { get; set; }
        
        public VesselBookmark() {
            Comment = "";
            CommandModuleName = "";
            VesselType = VesselType.Unknown;
            Order = 0;
        }
        
        public VesselBookmark(uint flightID, string comment = "") {
            CommandModuleFlightID = flightID;
            Comment = comment ?? "";
            CommandModuleName = "";
            VesselType = VesselType.Unknown;
            VesselSituation = "";
            CreationTime = Planetarium.GetUniversalTime();
            Order = 0;
        }
        
        /// <summary>
        /// Saves the bookmark to a ConfigNode
        /// </summary>
        public void Save(ConfigNode node) {
            node.AddValue("commandModuleFlightID", CommandModuleFlightID);
            node.AddValue("comment", Comment);
            node.AddValue("commandModuleName", CommandModuleName);
            node.AddValue("creationTime", CreationTime);
            node.AddValue("vesselType", (int) VesselType);
            node.AddValue("vesselSituation", VesselSituation);
            node.AddValue("order", Order);
        }
        
        /// <summary>
        /// Loads the bookmark from a ConfigNode
        /// </summary>
        public void Load(ConfigNode node) {
            if (node.HasValue("commandModuleFlightID")) {
                uint.TryParse(node.GetValue("commandModuleFlightID"), out uint flightID);
                CommandModuleFlightID = flightID;
            } else {
                throw new Exception("commandModuleFlightID not found in the bookmark node");
            }
            
            Comment = node.GetValue("comment") ?? "";
            
            CommandModuleName = node.GetValue("commandModuleName") ?? "";
            
            if (node.HasValue("vesselType")) {
                int.TryParse(node.GetValue("vesselType"), out int vesselType);
                VesselType = (VesselType) vesselType;
            } else {
                throw new Exception("vesselType not found in the bookmark node");
            }

            VesselSituation = node.GetValue("vesselSituation") ?? "";

            if (node.HasValue("creationTime")) {
                double.TryParse(node.GetValue("creationTime"), out double time);
                CreationTime = time;
            } else {
                throw new Exception("creationTime not found in the bookmark node");
            }
            
            if (node.HasValue("order")) {
                int.TryParse(node.GetValue("order"), out int order);
                Order = order;
            } else {
                throw new Exception("order not found in the bookmark node");
            }
        }
    }
}
