using System.Collections.Generic;
using System;

namespace com.github.lhervier.ksp.bookmarksmod.util {
    public static class ConfigNodeUtils {
        
        private static readonly ModLogger LOGGER = new ModLogger("ConfigNodeUtils");

        /// <summary>
        /// Get an integer value from a config node
        /// </summary>
        /// <param name="node">Config node to get the value from</param>
        /// <param name="valueName">Name of the value to get</param>
        /// <param name="raiseException">True if an exception should be thrown if the value is not found or not a valid integer, false otherwise</param>
        /// <param name="defaultValue">Default value to return if the value is not found or not a valid integer</param>
        /// <returns>The value of the integer</returns>
        public static int GetIntNodeValue(ConfigNode node, string valueName, bool raiseException, int defaultValue = 0) {
            if( !node.HasValue(valueName) ) {
                LOGGER.LogWarning($"{valueName} not found in the bookmark node");
                if( raiseException ) {
                    throw new Exception($"{valueName} not found in the bookmark node");
                }
                return defaultValue;
            }

            if( !int.TryParse(node.GetValue(valueName), out int value) ) {
                LOGGER.LogWarning($"{valueName} is not a valid integer");
                if( raiseException ) {
                    throw new Exception($"{valueName} is not a valid integer");  
                }
                return defaultValue;
            }
            return value;
        }

        /// <summary>
        /// Get an integer value from a config node
        /// </summary>
        /// <param name="node">Config node to get the value from</param>
        /// <param name="valueName">Name of the value to get</param>
        /// <param name="raiseException">True if an exception should be thrown if the value is not found or not a valid integer, false otherwise</param>
        /// <param name="defaultValue">Default value to return if the value is not found or not a valid integer</param>
        /// <returns>The value of the integer</returns>
        public static uint GetUintNodeValue(ConfigNode node, string valueName, bool raiseException, uint defaultValue = 0) {
            if( !node.HasValue(valueName) ) {
                LOGGER.LogWarning($"{valueName} not found in the bookmark node");
                if( raiseException ) {
                    throw new Exception($"{valueName} not found in the bookmark node");
                }
                return defaultValue;
            }

            if( !uint.TryParse(node.GetValue(valueName), out uint value) ) {
                LOGGER.LogWarning($"{valueName} is not a valid unsigned integer");
                if( raiseException ) {
                    throw new Exception($"{valueName} is not a valid unsigned integer");
                }
                return defaultValue;
            }
            return value;
        }

        /// <summary>
        /// Get an integer value from a config node
        /// </summary>
        /// <param name="node">Config node to get the value from</param>
        /// <param name="valueName">Name of the value to get</param>
        /// <param name="raiseException">True if an exception should be thrown if the value is not found or not a valid integer, false otherwise</param>
        /// <param name="defaultValue">Default value to return if the value is not found or not a valid integer</param>
        /// <returns>The value of the integer</returns>
        public static double GetDoubleNodeValue(ConfigNode node, string valueName, bool raiseException, double defaultValue = 0) {
            if( !node.HasValue(valueName) ) {
                LOGGER.LogWarning($"{valueName} not found in the bookmark node");
                if( raiseException ) {
                    throw new Exception($"{valueName} not found in the bookmark node");
                }
                return defaultValue;
            }

            if( !double.TryParse(node.GetValue(valueName), out double value) ) {
                LOGGER.LogWarning($"{valueName} is not a valid double");
                if( raiseException ) {
                    throw new Exception($"{valueName} is not a valid double");
                }
                return defaultValue;
            }
            return value;
        }
    }
}