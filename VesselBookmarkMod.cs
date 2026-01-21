using System;
using System.Collections.Generic;
using System.IO;
using Expansions.Missions.Editor;
using UnityEngine;

namespace com.github.lhervier.ksp {
	
	[KSPAddon(KSPAddon.Startup.PSystemSpawn, false)]
    public class VesselBookmarkMod : MonoBehaviour {
        
        protected void Awake() 
        {
            ModLogger.LogInfo("Awaked");
            DontDestroyOnLoad(this);
        }

        public void Start() {
            ModLogger.LogInfo("Plugin started");
            
            // Initialize the bookmark manager
            // The singleton will be created automatically on first access
            var manager = VesselBookmarkManager.Instance;
            ModLogger.LogInfo("VesselBookmarkManager initialized");
        }

        public void OnDestroy() {
            ModLogger.LogInfo("Plugin stopped");
        }
    }
}
