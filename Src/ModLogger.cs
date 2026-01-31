using UnityEngine;

namespace com.github.lhervier.ksp.bookmarksmod {
	
	public class ModLogger {
		public static bool DEBUG = true;
        private static readonly string MOD_NAME = "VesselBookmarkMod";
        private static readonly int MAX_LOGGER_NAME_LENGTH = 15;
        private string loggerName = "";

        /// <summary>
        /// Compute the logger name, ensuring it is not too long
        /// </summary>
        /// <param name="loggerName">The name to compute</param>
        /// <returns>The computed logger name</returns>
        public static string ComputeLoggerName(string loggerName) {
            char[] l = new char[MAX_LOGGER_NAME_LENGTH];
            for( int i = 0; i < MAX_LOGGER_NAME_LENGTH; i++ ) {
                l[i] = ' ';
            }

            int loggerNamePos = loggerName.Length - 1;
            int lPos = l.Length - 1;
            while( loggerNamePos >= 0 && lPos >= 0 ) {
                l[lPos] = loggerName[loggerNamePos];
                loggerNamePos--;
                lPos--;
            }

            if( loggerName.Length > MAX_LOGGER_NAME_LENGTH && lPos < 3 ) {
                for( int i = 0; i<3; i++ ) {
                    l[i] = '.';
                }
            }
            return new string(l);
        }

        public ModLogger(string loggerName) {
            this.loggerName = ComputeLoggerName(loggerName);
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