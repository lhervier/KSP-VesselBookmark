using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.github.lhervier.ksp {
    
    /// <summary>
    /// Gestionnaire central des bookmarks
    /// </summary>
    public class VesselBookmarkManager {
        
        private static VesselBookmarkManager _instance;
        public static VesselBookmarkManager Instance {
            get {
                if (_instance == null) {
                    _instance = new VesselBookmarkManager();
                }
                return _instance;
            }
        }
        
        private List<VesselBookmark> _bookmarks = new List<VesselBookmark>();
        private const string SAVE_NODE_NAME = "VESSEL_BOOKMARKS";
        
        private VesselBookmarkManager() {
            // S'abonner aux événements de sauvegarde/chargement
            GameEvents.onGameStateLoad.Add(OnGameStateLoad);
            GameEvents.onGameStateSave.Add(OnGameStateSave);
        }
        
        /// <summary>
        /// Liste de tous les bookmarks
        /// </summary>
        public IReadOnlyList<VesselBookmark> Bookmarks => _bookmarks.AsReadOnly();
        
        /// <summary>
        /// Charger les bookmarks depuis la sauvegarde
        /// </summary>
        private void OnGameStateLoad(ConfigNode node) {
            _bookmarks.Clear();
            
            if (node.HasNode(SAVE_NODE_NAME)) {
                ConfigNode bookmarksNode = node.GetNode(SAVE_NODE_NAME);
                ConfigNode[] bookmarkNodes = bookmarksNode.GetNodes("BOOKMARK");
                
                foreach (ConfigNode bookmarkNode in bookmarkNodes) {
                    VesselBookmark bookmark = new VesselBookmark();
                    try {
                        bookmark.Load(bookmarkNode);
                        _bookmarks.Add(bookmark);
                    } catch (Exception e) {
                        Debug.LogError($"[VesselBookmarkMod] Erreur lors du chargement d'un bookmark: {e.Message}");
                    }
                }
            }
            
            // Mettre à jour les noms des modules de commande
            UpdateCommandModuleNames();
            
            Debug.Log($"[VesselBookmarkMod] {_bookmarks.Count} bookmark(s) chargé(s)");
        }
        
        /// <summary>
        /// Sauvegarder les bookmarks dans la sauvegarde
        /// </summary>
        private void OnGameStateSave(ConfigNode node) {
            // Supprimer l'ancien nœud s'il existe
            if (node.HasNode(SAVE_NODE_NAME)) {
                node.RemoveNode(SAVE_NODE_NAME);
            }
            
            ConfigNode bookmarksNode = node.AddNode(SAVE_NODE_NAME);
            
            foreach (VesselBookmark bookmark in _bookmarks) {
                ConfigNode bookmarkNode = bookmarksNode.AddNode("BOOKMARK");
                bookmark.Save(bookmarkNode);
            }
            
            Debug.Log($"[VesselBookmarkMod] {_bookmarks.Count} bookmark(s) sauvegardé(s)");
        }
        
        /// <summary>
        /// Ajouter un bookmark pour un module de commande
        /// </summary>
        public bool AddBookmark(Part commandModulePart) {
            if (commandModulePart == null) {
                Debug.LogError("[VesselBookmarkMod] Tentative d'ajout d'un bookmark avec une partie null");
                return false;
            }
            
            uint flightID = commandModulePart.flightID;
            
            // Vérifier si le bookmark existe déjà
            if (_bookmarks.Any(b => b.CommandModuleFlightID == flightID)) {
                Debug.LogWarning($"[VesselBookmarkMod] Un bookmark existe déjà pour le flightID {flightID}");
                return false;
            }
            
            VesselBookmark bookmark = new VesselBookmark(flightID);
            
            // Mettre à jour le nom du module de commande
            bookmark.CommandModuleName = GetCommandModuleName(commandModulePart);
            
            _bookmarks.Add(bookmark);
            Debug.Log($"[VesselBookmarkMod] Bookmark ajouté pour le flightID {flightID}");
            return true;
        }
        
        /// <summary>
        /// Supprimer un bookmark
        /// </summary>
        public bool RemoveBookmark(uint commandModuleFlightID) {
            VesselBookmark bookmark = _bookmarks.FirstOrDefault(b => b.CommandModuleFlightID == commandModuleFlightID);
            if (bookmark != null) {
                _bookmarks.Remove(bookmark);
                Debug.Log($"[VesselBookmarkMod] Bookmark supprimé pour le flightID {commandModuleFlightID}");
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Vérifier si un bookmark existe pour un module de commande
        /// </summary>
        public bool HasBookmark(Part commandModulePart) {
            if (commandModulePart == null) return false;
            return _bookmarks.Any(b => b.CommandModuleFlightID == commandModulePart.flightID);
        }
        
        /// <summary>
        /// Obtenir le vaisseau actuel pour un bookmark (gère les vaisseaux amarrés)
        /// </summary>
        public Vessel GetVesselForBookmark(VesselBookmark bookmark) {
            if (bookmark == null) return null;
            
            try {
                // Chercher dans tous les vaisseaux chargés
                foreach (Vessel vessel in FlightGlobals.Vessels) {
                    if (vessel == null || vessel.parts == null) continue;
                    
                    try {
                        // Chercher le module de commande avec le flightID correspondant
                        foreach (Part part in vessel.parts) {
                            if (part == null) continue;
                            
                            try {
                                if (part.flightID == bookmark.CommandModuleFlightID) {
                                    // Trouvé ! Retourner le vaisseau racine (gère les amarrés)
                                    return FindRootVessel(part);
                                }
                            } catch (System.Exception e) {
                                Debug.LogWarning($"[VesselBookmarkMod] Erreur lors de la vérification de la partie {part.name}: {e.Message}");
                                continue;
                            }
                        }
                    } catch (System.Exception e) {
                        Debug.LogWarning($"[VesselBookmarkMod] Erreur lors de la vérification du vaisseau {vessel.vesselName}: {e.Message}");
                        continue;
                    }
                }
            } catch (System.Exception e) {
                Debug.LogError($"[VesselBookmarkMod] Erreur lors de la recherche du vaisseau pour le bookmark: {e.Message}");
            }
            
            return null;
        }
        
        /// <summary>
        /// Trouver le vaisseau racine à partir d'une partie (gère les vaisseaux amarrés)
        /// </summary>
        public Vessel FindRootVessel(Part part) {
            if (part == null) return null;
            
            try {
                // Si la partie a un vaisseau, utiliser rootPart pour trouver le vaisseau racine
                if (part.vessel != null) {
                    // Le rootPart du vaisseau pointe vers la partie racine du vaisseau composite
                    Part rootPart = part.vessel.rootPart;
                    if (rootPart != null && rootPart.vessel != null) {
                        return rootPart.vessel;
                    }
                    return part.vessel;
                }
            } catch (System.Exception e) {
                Debug.LogWarning($"[VesselBookmarkMod] Erreur lors de la recherche du vaisseau racine: {e.Message}");
            }
            
            return null;
        }
        
        /// <summary>
        /// Obtenir le module de commande pour un bookmark
        /// </summary>
        public Part GetCommandModuleForBookmark(VesselBookmark bookmark) {
            if (bookmark == null) return null;
            
            try {
                // Chercher dans tous les vaisseaux chargés
                foreach (Vessel vessel in FlightGlobals.Vessels) {
                    if (vessel == null || vessel.parts == null) continue;
                    
                    try {
                        // Chercher le module de commande avec le flightID correspondant
                        foreach (Part part in vessel.parts) {
                            if (part == null) continue;
                            
                            try {
                                if (part.flightID == bookmark.CommandModuleFlightID) {
                                    // Vérifier que c'est bien un module de commande
                                    ModuleCommand commandModule = part.FindModuleImplementing<ModuleCommand>();
                                    if (commandModule != null) {
                                        return part;
                                    }
                                }
                            } catch (System.Exception e) {
                                Debug.LogWarning($"[VesselBookmarkMod] Erreur lors de la vérification de la partie {part.name}: {e.Message}");
                                continue;
                            }
                        }
                    } catch (System.Exception e) {
                        Debug.LogWarning($"[VesselBookmarkMod] Erreur lors de la vérification du vaisseau: {e.Message}");
                        continue;
                    }
                }
            } catch (System.Exception e) {
                Debug.LogError($"[VesselBookmarkMod] Erreur lors de la recherche du module de commande pour le bookmark: {e.Message}");
            }
            
            return null;
        }
        
        /// <summary>
        /// Obtenir le nom d'un module de commande
        /// </summary>
        private string GetCommandModuleName(Part commandModulePart) {
            if (commandModulePart == null) return "Module introuvable";
            
            try {
                // Utiliser le nom de la partie (partInfo.title)
                if (commandModulePart.partInfo != null && !string.IsNullOrEmpty(commandModulePart.partInfo.title)) {
                    return commandModulePart.partInfo.title;
                }
                
                // Sinon utiliser le nom de la partie
                if (!string.IsNullOrEmpty(commandModulePart.partName)) {
                    return commandModulePart.partName;
                }
                
                return "Module de commande";
            } catch (System.Exception e) {
                Debug.LogWarning($"[VesselBookmarkMod] Erreur lors de la récupération du nom du module de commande: {e.Message}");
                return "Module de commande";
            }
        }
        
        /// <summary>
        /// Mettre à jour les noms des modules de commande pour tous les bookmarks
        /// </summary>
        public void UpdateCommandModuleNames() {
            try {
                foreach (VesselBookmark bookmark in _bookmarks) {
                    if (bookmark == null) continue;
                    
                    try {
                        Part commandModulePart = GetCommandModuleForBookmark(bookmark);
                        if (commandModulePart != null) {
                            bookmark.CommandModuleName = GetCommandModuleName(commandModulePart);
                        } else {
                            // Le module de commande n'est pas chargé ou n'existe plus
                            if (string.IsNullOrEmpty(bookmark.CommandModuleName)) {
                                bookmark.CommandModuleName = "Module introuvable";
                            }
                        }
                    } catch (System.Exception e) {
                        Debug.LogWarning($"[VesselBookmarkMod] Erreur lors de la mise à jour du nom pour un bookmark: {e.Message}");
                        if (string.IsNullOrEmpty(bookmark.CommandModuleName)) {
                            bookmark.CommandModuleName = "Erreur";
                        }
                    }
                }
            } catch (System.Exception e) {
                Debug.LogError($"[VesselBookmarkMod] Erreur lors de la mise à jour des noms de modules de commande: {e.Message}");
            }
        }
        
        /// <summary>
        /// Mettre à jour les noms des vaisseaux pour tous les bookmarks (déprécié, utilise UpdateCommandModuleNames)
        /// </summary>
        [System.Obsolete("Utilisez UpdateCommandModuleNames() à la place")]
        public void UpdateVesselNames() {
            UpdateCommandModuleNames();
        }
        
        /// <summary>
        /// Nettoyer les bookmarks vers des vaisseaux qui n'existent plus
        /// </summary>
        public void CleanupInvalidBookmarks() {
            List<VesselBookmark> toRemove = new List<VesselBookmark>();
            
            try {
                foreach (VesselBookmark bookmark in _bookmarks) {
                    if (bookmark == null) {
                        toRemove.Add(bookmark);
                        continue;
                    }
                    
                    try {
                        Vessel vessel = GetVesselForBookmark(bookmark);
                        if (vessel == null) {
                            // Vérifier si le vaisseau existe dans les vaisseaux non chargés
                            // (dans le Tracking Station par exemple)
                            bool found = false;
                            try {
                                foreach (Vessel v in FlightGlobals.VesselsUnloaded) {
                                    if (v == null || v.parts == null) continue;
                                    
                                    try {
                                        foreach (Part p in v.parts) {
                                            if (p != null && p.flightID == bookmark.CommandModuleFlightID) {
                                                found = true;
                                                break;
                                            }
                                        }
                                    } catch (System.Exception e) {
                                        Debug.LogWarning($"[VesselBookmarkMod] Erreur lors de la vérification des parties du vaisseau non chargé: {e.Message}");
                                        continue;
                                    }
                                    
                                    if (found) break;
                                }
                            } catch (System.Exception e) {
                                Debug.LogWarning($"[VesselBookmarkMod] Erreur lors de la vérification des vaisseaux non chargés: {e.Message}");
                            }
                            
                            if (!found) {
                                toRemove.Add(bookmark);
                            }
                        }
                    } catch (System.Exception e) {
                        Debug.LogWarning($"[VesselBookmarkMod] Erreur lors de la vérification d'un bookmark: {e.Message}");
                        // Ne pas supprimer en cas d'erreur, on réessayera plus tard
                    }
                }
                
                foreach (VesselBookmark bookmark in toRemove) {
                    _bookmarks.Remove(bookmark);
                    Debug.Log($"[VesselBookmarkMod] Bookmark nettoyé (vaisseau introuvable): {bookmark?.CommandModuleFlightID ?? 0}");
                }
            } catch (System.Exception e) {
                Debug.LogError($"[VesselBookmarkMod] Erreur lors du nettoyage des bookmarks: {e.Message}");
            }
        }
    }
}
