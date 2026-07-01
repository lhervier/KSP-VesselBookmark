namespace com.github.lhervier.ksp.bookmarksmod.util {

    /// <summary>
    /// Turns an internal celestial body name (CelestialBody.bodyName, used everywhere as a stable
    /// identity/filter key) into the game's localized display name for the UI. Never use the result
    /// as a key: it changes with the active language.
    /// </summary>
    public static class CelestialBodyLabels {

        /// <summary>
        /// Localized display name for the body identified by <paramref name="bodyName"/>, or the
        /// name itself when no such body exists in the current system (e.g. filter tokens).
        /// </summary>
        /// <param name="bodyName">The internal name of the body (CelestialBody.bodyName)</param>
        /// <returns>The localized, gender-tag-free display name, or the input name as fallback</returns>
        public static string GetDisplayName(string bodyName) {
            CelestialBody body = FlightGlobals.GetBodyByName(bodyName);
            // displayName carries a Lingoona gender tag ("^N", "^M"…) that must be stripped for display.
            return body != null ? body.displayName.LocalizeRemoveGender() : bodyName;
        }
    }
}
