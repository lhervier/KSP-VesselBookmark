using System.Collections.Generic;
using System.Linq;

namespace com.github.lhervier.ksp.bookmarksmod.util {

    /// <summary>
    /// Utility class to order the celestial bodies of the current system for the body filter:
    /// stars outward, each body immediately followed by the bodies orbiting it (moons after their
    /// planet, sub-moons after their moon), every level sorted by distance to its parent.
    /// </summary>
    public static class CelestialBodySorter {

        /// <summary>
        /// Get the names of every celestial body of the current system, ordered star-outward with
        /// each body followed by the ones orbiting it. Stars themselves are not listed (only what
        /// orbits them). Example for the stock system: Moho, Eve, Gilly, Kerbin, Mun, Minmus, Duna, Ike, ...
        /// </summary>
        /// <returns>List of celestial body names in display order</returns>
        public static List<string> GetSortedBodyNames() {
            List<string> sortedBodies = new List<string>();
            if (FlightGlobals.Bodies == null) {
                return sortedBodies;
            }

            // Start from every star (usually just Kerbol) and walk its orbit tree. isStar and
            // orbitingBodies are the native KSP hierarchy, so no manual parent/child matching.
            foreach (CelestialBody star in FlightGlobals.Bodies.Where(b => b.isStar)) {
                AppendOrbitingBodies(star, sortedBodies);
            }
            return sortedBodies;
        }

        /// <summary>
        /// Append the names of the bodies orbiting <paramref name="parent"/> (recursively) to
        /// <paramref name="result"/>, each body added just before the ones orbiting it.
        /// </summary>
        /// <param name="parent">The body whose orbit tree is appended</param>
        /// <param name="result">The list being built, mutated in place</param>
        private static void AppendOrbitingBodies(CelestialBody parent, List<string> result) {
            // Depth-first so a child sits right under its parent; siblings ordered by distance.
            foreach (CelestialBody child in parent.orbitingBodies.OrderBy(b => b.orbit?.semiMajorAxis ?? double.MaxValue)) {
                result.Add(child.bodyName);
                AppendOrbitingBodies(child, result);
            }
        }

        /// <summary>
        /// Nesting depth of the given body relative to its star: 0 for a planet (orbits a star),
        /// 1 for a moon, 2 for a sub-moon, and so on. Unknown names (e.g. filter tokens) return 0.
        /// </summary>
        /// <param name="bodyName">The name of the body to measure</param>
        /// <returns>The number of hops above the planet level</returns>
        public static int GetIndentLevel(string bodyName) {
            CelestialBody body = FlightGlobals.GetBodyByName(bodyName);
            if (body == null) {
                return 0;
            }

            // Count reference hops up to the star. reference != body guards a star referencing itself.
            int level = 0;
            CelestialBody reference = body.referenceBody;
            while (reference != null && !reference.isStar && reference != body) {
                level++;
                body = reference;
                reference = reference.referenceBody;
            }
            return level;
        }
    }
}
