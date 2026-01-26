using UnityEngine;

namespace com.github.lhervier.ksp {
	
	public class ModLogger {
		private static readonly bool DEBUG = true;
        private static readonly string MOD_NAME = "VesselBookmarkMod";
        private static void LogInternal(string level, string message) {
            Debug.Log($"[{MOD_NAME}][{level}] {message}");
        }

        public static void LogInfo(string message) {
            LogInternal("INFO", message);
        }

        public static void LogDebug(string message) {
            if( !DEBUG ) {
                return;
            }
            LogInternal("DEBUG", message);
        }

        public static void LogWarning(string message) {
            LogInternal("WARNING", message);
        }

        public static void LogError(string message) {
            LogInternal("ERROR", message);
        }
	}
}