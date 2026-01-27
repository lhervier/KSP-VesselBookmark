using System;
using KSP.Localization;

namespace com.github.lhervier.ksp.bookmarksmod {
    
    /// <summary>
    /// Handles localization for the Vessel Bookmark mod
    /// Uses KSP's standard Localizer system
    /// </summary>
    public static class VesselBookmarkLocalization {
        
        /// <summary>
        /// Gets a localized string with parameters
        /// </summary>
        /// <param name="key">The localization key</param>
        /// <param name="args">Format arguments</param>
        /// <returns>Localized and formatted string</returns>
        public static string GetString(string key, params object[] args) {
            if (string.IsNullOrEmpty(key)) {
                return $"[{key}]";
            }
            try {
                return Localizer.Format($"#LOC_{key}", args);
            } catch (Exception e) {
                ModLogger.LogWarning($"Error formatting localization string '{key}': {e.Message}");
                return $"[{key}]";
            }
        }
    }
}
