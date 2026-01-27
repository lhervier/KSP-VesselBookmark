using System.Collections.Generic;
using System.Linq;

namespace com.github.lhervier.ksp.bookmarksmod.util {
    
    /// <summary>
    /// Utility class to sort celestial bodies by their distance to Kerbol (the Sun),
    /// interleaving moons after their parent planet sorted by distance to the planet.
    /// </summary>
    public static class CelestialBodySorter {
        
        /// <summary>
        /// Get all celestial bodies sorted by distance to Kerbol,
        /// with moons interleaved after their parent planet.
        /// Example output for stock system: Moho, Eve, Gilly, Kerbin, Mun, Minmus, Duna, Ike, ...
        /// </summary>
        /// <returns>List of celestial body names in sorted order</returns>
        public static List<string> GetSortedBodyNames() {
            List<string> sortedBodies = new List<string>();
            
            if (FlightGlobals.Bodies == null || FlightGlobals.Bodies.Count == 0) {
                return sortedBodies;
            }
            
            // Find the Sun (Kerbol) - it's the body that orbits itself or has no reference body
            CelestialBody sun = FlightGlobals.Bodies.FirstOrDefault(
                b => b.referenceBody == null || b.referenceBody == b
            );
            
            if (sun == null) {
                // Fallback: return bodies in default order
                return FlightGlobals.Bodies.Select(b => b.bodyName).ToList();
            }
            
            // Get all planets (bodies that orbit directly around the Sun)
            // Sort them by semi-major axis (distance to the Sun)
            List<CelestialBody> planets = FlightGlobals.Bodies
                .Where(b => b.referenceBody == sun && b != sun)
                .OrderBy(b => b.orbit?.semiMajorAxis ?? double.MaxValue)
                .ToList();
            
            // For each planet, add it followed by its moons (sorted by distance to planet)
            foreach (CelestialBody planet in planets) {
                sortedBodies.Add(planet.bodyName);
                
                // Get moons of this planet, sorted by distance
                List<CelestialBody> moons = FlightGlobals.Bodies
                    .Where(b => b.referenceBody == planet && b != planet)
                    .OrderBy(b => b.orbit?.semiMajorAxis ?? double.MaxValue)
                    .ToList();
                
                foreach (CelestialBody moon in moons) {
                    sortedBodies.Add(moon.bodyName);
                }
            }
            
            return sortedBodies;
        }
        
        /// <summary>
        /// Sort a list of body names according to the celestial body order
        /// (distance to Kerbol with moons interleaved).
        /// Bodies not found in FlightGlobals will be placed at the end.
        /// </summary>
        /// <param name="bodyNames">List of body names to sort</param>
        /// <returns>Sorted list of body names</returns>
        public static List<string> SortBodyNames(IEnumerable<string> bodyNames) {
            List<string> sortedOrder = GetSortedBodyNames();
            
            // Create a dictionary for quick lookup of position
            Dictionary<string, int> orderIndex = new Dictionary<string, int>();
            for (int i = 0; i < sortedOrder.Count; i++) {
                orderIndex[sortedOrder[i]] = i;
            }
            
            // Sort the input body names according to the order
            return bodyNames
                .OrderBy(name => orderIndex.ContainsKey(name) ? orderIndex[name] : int.MaxValue)
                .ToList();
        }
    }
}
