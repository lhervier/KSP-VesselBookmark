using UnityEngine;

namespace com.github.lhervier.ksp.bookmarksmod {
	
	public class ModLogger {
		public static bool DEBUG = true;
        private static readonly string MOD_NAME = "VesselBookmarkMod";
        private string loggerName = "";

        public ModLogger(string loggerName) {
            this.loggerName = loggerName;
        }

        private void LogInternal(string level, string message) {
            Debug.Log($"[{MOD_NAME}][{loggerName}][{level}] {message}");
        }

        public void LogInfo(string message) {
            LogInternal("INFO", message);
        }

        public void LogDebug(string message) {
            if( !DEBUG ) {
                return;
            }
            LogInternal("DEBUG", message);
        }

        public void LogWarning(string message) {
            LogInternal("WARNING", message);
        }

        public void LogError(string message) {
            LogInternal("ERROR", message);
        }
	}
}