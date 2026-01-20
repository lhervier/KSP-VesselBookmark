using System;
using System.Collections.Generic;
using System.IO;
using Expansions.Missions.Editor;
using UnityEngine;

namespace com.github.lhervier.ksp {
	
	[KSPAddon(KSPAddon.Startup.PSystemSpawn, false)]
    public class VesselBookmarkMod : MonoBehaviour {
        
        private static bool DEBUG = false;
        private static void LogInternal(string level, string message) {
            Debug.Log($"[VesselBookmarkMod][{level}] {message}");
        }

        private static void LogInfo(string message) {
            LogInternal("INFO", message);
        }

        private static void LogDebug(string message) {
            if( !DEBUG ) {
                return;
            }
            LogInternal("DEBUG", message);
        }

        private static void LogError(string message) {
            LogInternal("ERROR", message);
        }

        protected void Awake() 
        {
            LogInfo("Awaked");
            DontDestroyOnLoad(this);
        }

        public void Start() {
            LogInfo("Plugin started");
        }

        public void OnDestroy() {
            LogInfo("Plugin stopped");
        }
    }
}
