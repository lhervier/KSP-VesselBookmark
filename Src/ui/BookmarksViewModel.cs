using KSP.Localization;
using KSP.UI.Screens;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using com.github.lhervier.ksp.bookmarksmod.bookmarks;
using com.github.lhervier.ksp.bookmarksmod.util;
using System;

namespace com.github.lhervier.ksp.bookmarksmod.ui {
    public class BookmarksViewModel : MonoBehaviour
    {
        private static readonly ModLogger LOGGER = new ModLogger("BookmarksViewModel");

        private static readonly float SEARCH_DEBOUNCE_SECONDS = 0.2f;
        private static readonly string ALL_VESSEL_TYPES = "All";
        
        // =================================================================
        // Currently selected and hovered bookmark
        // =================================================================

        /// <summary>
        /// The currently hovered bookmark (null if none).
        /// </summary>
        public Bookmark HoveredBookmark
        {
            get => _hoveredBookmark;
            set
            {
                if( _hoveredBookmark == value ) return;
                _hoveredBookmark = value;
                this.OnHoveredBookmarkChanged.Fire();
            }
        }
        private Bookmark _hoveredBookmark = null;
        public readonly EventVoid OnHoveredBookmarkChanged = new EventVoid("BookmarksViewModel.OnHoveredBookmarkChanged");

        /// <summary>
        /// The currently selected bookmark (stays selected when clicking the row). Null if none.
        /// </summary>
        public Bookmark SelectedBookmark {
            get => _selectedBookmark;
            set
            {
                if( _selectedBookmark == value ) return;
                _selectedBookmark = value;
                OnSelectedBookmarkChanged.Fire();
            }
        }
        private Bookmark _selectedBookmark = null;
        public readonly EventVoid OnSelectedBookmarkChanged = new EventVoid("BookmarksViewModel.OnSelectedBookmarkChanged");
        
        // ======================================================================
        // Lists of values (bookmarks, bodies, vessel types, ...)
        // ======================================================================

        /// <summary>
        /// Bookmarks list to display in the UI (cached for performance)
        /// </summary>
        public IReadOnlyDictionary<BookmarkType, List<Bookmark>> AvailableBookmarks => _availableBookmarks;
        private readonly Dictionary<BookmarkType, List<Bookmark>> _availableBookmarks = new Dictionary<BookmarkType, List<Bookmark>>();
        private bool _preventBookmarksUpdates = false;
        public readonly EventVoid OnAvailableBookmarksChanged = new EventVoid("BookmarksViewModel.OnAvailableBookmarksChanged");

        /// <summary>Nombre de bookmarks affichés (après filtrage), toutes sections confondues.</summary>
        public int AvailableBookmarksCount {
            get {
                int n = 0;
                foreach( var list in _availableBookmarks.Values ) n += list.Count;
                return n;
            }
        }

        /// <summary>Nombre total de bookmarks (avant filtrage), toutes sections confondues.</summary>
        public int TotalBookmarksCount {
            get {
                int n = 0;
                foreach( var instance in BookmarkManager.Instances.Values ) n += instance.Bookmarks.Count;
                return n;
            }
        }
        
        /// <summary>
        /// The list of available bodies
        /// </summary>
        public IReadOnlyList<string> AvailableBodies => _availableBodies;
        private List<string> _availableBodies = new List<string>();
        public readonly EventVoid OnAvailableBodiesChanged = new EventVoid("BookmarksViewModel.OnAvailableBodiesChanged");
        
        /// <summary>
        /// The list of available vessel types
        /// </summary>
        public IReadOnlyList<string> AvailableVesselTypes => _availableVesselTypes;
        private List<string> _availableVesselTypes = new List<string>();
        public readonly EventVoid OnAvailableVesselTypesChanged = new EventVoid("BookmarksViewModel.OnAvailableVesselTypesChanged");
        
        // ===============================================================
        // Search criteria
        // ===============================================================

        /// <summary>
        /// The selected body
        /// </summary>
        public string SelectedBody {
            get => _selectedBody; 
            set {
                if( value == _selectedBody ) return;
                _selectedBody = value;
                OnSelectedBodyChanged.Fire();
            }
        }
        private string _selectedBody = ModLocalization.GetString("labelAll");
        public readonly EventVoid OnSelectedBodyChanged = new EventVoid("BookmarksViewModel.OnSelectedBodyChanged");
        
        /// <summary>
        /// The selected vessel type
        /// </summary>
        public string SelectedVesselType { 
            get => _selectedVesselType; 
            set {
                if( value == _selectedVesselType ) return;
                _selectedVesselType = value;
                OnSelectedVesselTypeChanged.Fire();
            }
        }
        private string _selectedVesselType = ALL_VESSEL_TYPES;
        public readonly EventVoid OnSelectedVesselTypeChanged = new EventVoid("BookmarksViewModel.OnSelectedVesselTypeChanged");

        /// <summary>
        /// Filtre : n'afficher que les bookmarks qui ont un commentaire (pour usage futur).
        /// </summary>
        public bool FilterHasComment { 
            get => _filterHasComment;
            set {
                if( value == _filterHasComment ) return;
                _filterHasComment = value;
                OnFilterHasCommentChanged.Fire();
            }
        }
        private bool _filterHasComment = false;
        public readonly EventVoid OnFilterHasCommentChanged = new EventVoid("BookmarksViewModel.OnFilterHasCommentChanged");
        
        /// <summary>
        /// The text in the search box
        /// </summary>
        public string SearchText {
            get => _searchText;
            set {
                if (value == _searchText) return;
                _searchText = value ?? string.Empty;
                _searchTextChangeTime = Time.realtimeSinceStartup;
            }
        }
        private float _searchTextChangeTime = -1f;
        private string _searchText = string.Empty;
        public readonly EventVoid OnSearchTextChanged = new EventVoid("BookmarksViewModel.OnSearchTextChanged");
        
        // =============================================================
        // Comment edition
        // =============================================================

        /// <summary>
        /// Temporary zone to memorize the selected bookmark comment.
        /// </summary>
        public string Comment
        {
            get => _comment;
            set {
                if( _comment == value ) return;
                _comment = value;
                OnCommentChanged.Fire();
            }
        }
        private string _comment = string.Empty;
        public EventVoid OnCommentChanged = new EventVoid("BookmarksViewModel.OnCommentChanged");

        // =============================================================
        // Presentation state (drives the uGUI layer)
        // =============================================================

        /// <summary>
        /// Whether the main window is visible (drives the PopupDialog spawn/despawn).
        /// </summary>
        public bool WindowVisible
        {
            get => _windowVisible;
            set {
                if( _windowVisible == value ) return;
                _windowVisible = value;
                OnWindowVisibleChanged.Fire();
            }
        }
        private bool _windowVisible = false;
        public readonly EventVoid OnWindowVisibleChanged = new EventVoid("BookmarksViewModel.OnWindowVisibleChanged");

        /// <summary>
        /// Whether the filters menu ("⋯") is open.
        /// </summary>
        public bool FilterMenuOpen
        {
            get => _filterMenuOpen;
            set {
                if( _filterMenuOpen == value ) return;
                _filterMenuOpen = value;
                OnFilterMenuOpenChanged.Fire();
            }
        }
        private bool _filterMenuOpen = false;
        public readonly EventVoid OnFilterMenuOpenChanged = new EventVoid("BookmarksViewModel.OnFilterMenuOpenChanged");

        /// <summary>
        /// Whether the comment edition overlay is open.
        /// </summary>
        public bool EditingComment
        {
            get => _editingComment;
            set {
                if( _editingComment == value ) return;
                _editingComment = value;
                OnEditingCommentChanged.Fire();
            }
        }
        private bool _editingComment = false;
        public readonly EventVoid OnEditingCommentChanged = new EventVoid("BookmarksViewModel.OnEditingCommentChanged");

        /// <summary>
        /// The bookmark whose removal is awaiting confirmation (drives the removal overlay). Null if none.
        /// </summary>
        public Bookmark PendingRemoval
        {
            get => _pendingRemoval;
            set {
                if( _pendingRemoval == value ) return;
                _pendingRemoval = value;
                OnPendingRemovalChanged.Fire();
            }
        }
        private Bookmark _pendingRemoval = null;
        public readonly EventVoid OnPendingRemovalChanged = new EventVoid("BookmarksViewModel.OnPendingRemovalChanged");

        // =============================================================
        // Live game state (active vessel / target highlighting)
        // =============================================================

        /// <summary>
        /// Fired when the active vessel or the current target changes. The bookmark rows re-evaluate
        /// their "active vessel" / "target" highlighting on this event (these depend on live game
        /// state that changes without any bookmark event firing).
        /// </summary>
        public readonly EventVoid OnActiveOrTargetChanged = new EventVoid("BookmarksViewModel.OnActiveOrTargetChanged");

        // =============================================================
        // Lifecycle
        // =============================================================

        /// <summary>
        /// Constructor
        /// </summary>
        public void Start() 
        {
            this.OnSelectedVesselTypeChanged.Add(UpdateBookmarksSelection);
            this.OnSelectedBodyChanged.Add(UpdateBookmarksSelection);
            this.OnSearchTextChanged.Add(UpdateBookmarksSelection);
            this.OnFilterHasCommentChanged.Add(UpdateBookmarksSelection);

            this.OnSelectedBookmarkChanged.Add(_onSelectedBookmarkChanged);

            BookmarkManager.OnBookmarksUpdated.Add(UpdateBookmarksSelection);

            // Live game state: active vessel / target highlighting depends on game state that changes
            // without any bookmark event, so listen to the relevant GameEvents and re-broadcast.
            GameEvents.onVesselChange.Add(_onActiveVesselChanged);
            GameEvents.OnTargetObjectChanged.Add(_onTargetChanged);

            UpdateBookmarksSelection();
        }

        /// <summary>
        /// Called each frame
        /// </summary>
        public void Update()
        {
            _processSearchDebounce();
        }

        /// <summary>
        /// À appeler chaque frame (ex. depuis OnGUI). Déclenche RefreshBookmarks 200 ms après la dernière modification de SearchText.
        /// </summary>
        private void _processSearchDebounce() {
            if (_searchTextChangeTime < 0f) return;
            if (Time.realtimeSinceStartup - _searchTextChangeTime < SEARCH_DEBOUNCE_SECONDS) return;
            _searchTextChangeTime = -1f;
            this.OnSearchTextChanged.Fire();
        }

        public void OnDestroy()
        {
            BookmarkManager.OnBookmarksUpdated.Remove(UpdateBookmarksSelection);

            this.OnFilterHasCommentChanged.Remove(UpdateBookmarksSelection);
            this.OnSearchTextChanged.Remove(UpdateBookmarksSelection);
            this.OnSelectedBodyChanged.Remove(UpdateBookmarksSelection);
            this.OnSelectedVesselTypeChanged.Remove(UpdateBookmarksSelection);

            this.OnSelectedBookmarkChanged.Remove(_onSelectedBookmarkChanged);

            GameEvents.onVesselChange.Remove(_onActiveVesselChanged);
            GameEvents.OnTargetObjectChanged.Remove(_onTargetChanged);
        }

        private void _onSelectedBookmarkChanged()
        {
            Comment = SelectedBookmark?.Comment ?? string.Empty;
        }

        private void _onActiveVesselChanged(Vessel vessel)
        {
            this.OnActiveOrTargetChanged.Fire();
        }

        private void _onTargetChanged(MapObject target)
        {
            this.OnActiveOrTargetChanged.Fire();
        }

        // ======================================================================
        //  Update of the available bookmarks, bodies and vessel types
        // ======================================================================

        /// <summary>
        /// Update the bookmarks selection
        /// </summary>
        public void UpdateBookmarksSelection() {
            try {
                LOGGER.LogDebug($"Updating bookmarks");
                if( _preventBookmarksUpdates ) return;

                this.UpdateAvailableBodies();
                this.UpdateAvailableVesselTypes();
                this.UpdateAvailableBookmarks();
            } catch (Exception e) {
                LOGGER.LogError($"Error updating bookmarks: {e.Message}");
            }
        }

        /// <summary>
        /// Update the available bodies
        /// </summary>
        private void UpdateAvailableBodies() {
            try {
                LOGGER.LogDebug($"Updating available bodies");
                _availableBodies.Clear();
                foreach (Bookmark bookmark in BookmarkManager.GetAllBookmarks()) {
                    Vessel vessel = bookmark.Vessel;
                    if( vessel == null ) {
                        continue;
                    }
                    string vesselBodyName = vessel.mainBody.bodyName;
                    if( !_availableBodies.Contains(vesselBodyName) ) {
                        _availableBodies.Add(vesselBodyName);
                    }
                }
                // Sort bodies by distance to Kerbol with moons interleaved
                _availableBodies = CelestialBodySorter.SortBodyNames(_availableBodies);
                _availableBodies.Insert(0, ModLocalization.GetString("labelAll"));
                
                if( !_availableBodies.Contains(SelectedBody) ) {
                    SelectedBody = this.AvailableBodies[0];
                }
                this.OnAvailableBodiesChanged.Fire();
            } catch (Exception e) {
                LOGGER.LogError($"Error updating available bodies: {e.Message}");
            }
        }

        private void UpdateAvailableVesselTypes() {
            try {
                LOGGER.LogDebug($"Updating available vessel types");
                _availableVesselTypes.Clear();
                foreach (Bookmark bookmark in BookmarkManager.GetAllBookmarks()) {
                    if( !_availableVesselTypes.Contains(bookmark.BookmarkVesselType) ) {
                        _availableVesselTypes.Add(bookmark.BookmarkVesselType);
                    }
                }
                _availableVesselTypes.Sort();
                _availableVesselTypes.Insert(0, ALL_VESSEL_TYPES);

                if( !_availableVesselTypes.Contains(SelectedVesselType) ) {
                    SelectedVesselType = _availableVesselTypes[0];
                }
                this.OnAvailableVesselTypesChanged.Fire();
            } catch (Exception e) {
                LOGGER.LogError($"Error updating available vessel types: {e.Message}");
            }
        }

        /// <summary>
        /// Update the available bookmarks
        /// </summary>
        private void UpdateAvailableBookmarks() {
            try {
                LOGGER.LogDebug($"Updating available bookmarks");
                this._availableBookmarks.Clear();
                
                string all = ModLocalization.GetString("labelAll");
                
                foreach( var instance in BookmarkManager.Instances.Values ) {
                    List<Bookmark> selectionedBookmarks = new List<Bookmark>();
                    foreach( var bookmark in instance.Bookmarks ) {
                        bool addBookmark;
                        if( string.Equals(SelectedBody, all) && string.Equals(SelectedVesselType, ALL_VESSEL_TYPES) ) {
                            addBookmark = true;
                        } else if( string.Equals(SelectedBody, all) ) {
                            addBookmark = string.Equals(
                                bookmark.BookmarkVesselType, 
                                SelectedVesselType
                            );
                        } else if( string.Equals(SelectedVesselType, ALL_VESSEL_TYPES) ) {
                            addBookmark = string.Equals(
                                bookmark.VesselBodyName, 
                                SelectedBody
                            );
                        } else {
                            addBookmark = string.Equals(
                                    bookmark.VesselBodyName, 
                                    SelectedBody
                                ) 
                                && 
                                string.Equals(
                                    bookmark.BookmarkVesselType, 
                                    SelectedVesselType
                                );
                        }

                        if( addBookmark && !string.IsNullOrEmpty(SearchText) ) {
                            string fullSearchText = bookmark.BookmarkTitle + " ";
                            fullSearchText += bookmark.VesselSituationLabel + " ";   // Situation contains celestial body name
                            fullSearchText += bookmark.VesselName + " ";
                            fullSearchText += ModLocalization.GetString("vesselType" + bookmark.BookmarkVesselType) + " ";
                            fullSearchText += bookmark.Comment + " ";
                            if( !fullSearchText.ToLower().Contains(SearchText.ToLower()) ) {
                                addBookmark = false;
                            }
                        }

                        if( addBookmark && FilterHasComment ) {
                            if( string.IsNullOrEmpty(bookmark.Comment) ) {
                                addBookmark = false;
                            }
                        }

                        if( addBookmark ) {
                            selectionedBookmarks.Add(bookmark);
                        }
                    }
                    _availableBookmarks[instance.BookmarkType] = selectionedBookmarks;
                }

                // Clear selection if the selected bookmark is no longer in the filtered list
                if (SelectedBookmark != null && !_availableBookmarks.Values.Any(list => list.Contains(SelectedBookmark))) {
                    SelectedBookmark = null;
                }
                this.OnAvailableBookmarksChanged.Fire();
            } catch (Exception e) {
                LOGGER.LogError($"Error updating available bookmarks: {e.Message}");
            }
        }

        // ======================================================================
        //  Actions
        // ======================================================================

        /// <summary>
        /// Whether to show the add vessel bookmark button
        /// </summary>
        /// <returns>Whether to show the add vessel bookmark button</returns>
        public bool CanAddVesselBookmark() {
            return FlightGlobals.ActiveVessel != null;
        }

        /// <summary>
        /// Add a vessel bookmark
        /// </summary>
        public void AddVesselBookmark() {
            if( !CanAddVesselBookmark() ) {
                LOGGER.LogWarning("Cannot add vessel bookmark: no active vessel");
                return;
            }

            uint vesselPersistentID = FlightGlobals.ActiveVessel.persistentId;
            
            BookmarkManager manager = BookmarkManager.GetInstance(BookmarkType.Vessel);
            if (manager.HasBookmark(vesselPersistentID)) {
                ScreenMessages.PostScreenMessage(
                    ModLocalization.GetString("messageBookmarkAlreadyExists"),
                    2f,
                    ScreenMessageStyle.UPPER_CENTER
                );
                return;
            }

            VesselBookmark bookmark = new VesselBookmark(vesselPersistentID);
            if (BookmarkManager.AddBookmark(bookmark)) {
                ScreenMessages.PostScreenMessage(
                    ModLocalization.GetString("messageBookmarkAdded"),
                    2f,
                    ScreenMessageStyle.UPPER_CENTER
                );
            }
        }

        /// <summary>
        /// Reload the bookmarks
        /// </summary>
        public void RefreshBookmarks() {
            BookmarkManager.RefreshBookmarks();
        }

        // =============================================================================
        //  Filters
        // =============================================================================

        /// <summary>
        /// Clear the filters
        /// </summary>
        public void ClearFilters() {
            this._preventBookmarksUpdates = true;
            SelectedBody = ModLocalization.GetString("labelAll");
            SelectedVesselType = ALL_VESSEL_TYPES;
            SearchText = string.Empty;
            FilterHasComment = false;
            this._preventBookmarksUpdates = false;
            UpdateBookmarksSelection();
        }

        // ======================================================================
        //  Bookmark states
        // ======================================================================

        /// <summary>
        /// Whether the given bookmark is hovered
        /// </summary>
        /// <param name="bookmark">The bookmark to check</param>
        /// <returns>Whether the given bookmark is hovered</returns>
        public bool IsHovered(Bookmark bookmark) {
            return bookmark != null && HoveredBookmark == bookmark;
        }

        /// <summary>
        /// Whether the given bookmark is selected
        /// </summary>
        /// <param name="bookmark">The bookmark to check</param>
        /// <returns>Whether the given bookmark is selected</returns>
        public bool IsSelected(Bookmark bookmark) {
            return bookmark != null && SelectedBookmark == bookmark;
        }

        /// <summary>
        /// Whether the given bookmark corresponds to the current vessel
        /// </summary>
        /// <param name="bookmark">The bookmark to check</param>
        /// <returns>Whether the given bookmark corresponds to the current vessel</returns>
        public bool IsCurrentVessel(Bookmark bookmark)
        {
            bool isActiveVessel = false;
            if (FlightGlobals.ActiveVessel != null) {
                isActiveVessel = bookmark.VesselPersistentID == FlightGlobals.ActiveVessel.persistentId;
            }
            return isActiveVessel;
        }

        /// <summary>
        /// Whether the given bookmark corresponds to the current vessel's target
        /// </summary>
        /// <param name="bookmark">The bookmark to check</param>
        /// <returns>Whether the given bookmark corresponds to the current vessel's target</returns>
        public bool IsTarget(Bookmark bookmark)
        {
            if( FlightGlobals.ActiveVessel == null ) {
                return false;
            }
            ITargetable target = FlightGlobals.ActiveVessel.targetObject;
            if( target == null ) {
                return false;
            }
            Vessel targetVessel = target.GetVessel();
            if( targetVessel == null ) {
                return false;
            }
            return targetVessel.persistentId == bookmark.VesselPersistentID;
        }

        /// <summary>
        /// Whether the selected bookmark can be edited
        /// </summary>
        /// <returns>Whether the selected bookmark can be edited</returns>
        public bool CanEditCurrentVesselComment() {
            return SelectedBookmark != null;
        }

        /// <summary>
        /// Whether "Set target as" is available for the given bookmark (active vessel exists, bookmark has vessel, not active vessel).
        /// </summary>
        public bool CanSetCurrentBookmarkVesselAsTarget() {
            if (SelectedBookmark == null || FlightGlobals.ActiveVessel == null || SelectedBookmark.Vessel == null) {
                return false;
            }
            return SelectedBookmark.VesselPersistentID != FlightGlobals.ActiveVessel.persistentId;
        }

        /// <summary>
        /// Whether "Switch to vessel" is available for the given bookmark (vessel exists, not active vessel).
        /// </summary>
        public bool CanSwitchToCurrentBookmarkVessel() {
            if (SelectedBookmark == null || SelectedBookmark.Vessel == null) {
                return false;
            }
            return SelectedBookmark.VesselPersistentID != FlightGlobals.ActiveVessel?.persistentId;
        }
        
        // ======================================================================
        //  Current selected bookmark actions
        // ======================================================================

        /// <summary>
        /// Set the target as the given bookmark
        /// </summary>
        public void SetCurrentBookmarkVesselAsTarget() {
            if( SelectedBookmark == null ) {
                LOGGER.LogWarning("Cannot set current vessel as target: no selected bookmark");
                return;
            }
            FlightGlobals flightGlobals = FlightGlobals.fetch;
            if( flightGlobals == null ) {
                LOGGER.LogWarning($"Bookmark {SelectedBookmark}: FlightGlobals not found. Cannot set target as.");
                return;
            }
            flightGlobals.SetVesselTarget(SelectedBookmark.Vessel);
        }

        /// <summary>
        /// Switch to the given bookmark
        /// </summary>
        public void SwitchToSelectedVessel() { 
            if( SelectedBookmark == null ) {
                LOGGER.LogWarning("Cannot switch to current vessel: no selected bookmark");
                return;
            }
            Vessel vessel = SelectedBookmark.Vessel;
            if( vessel == null ) {
                LOGGER.LogWarning($"Bookmark {SelectedBookmark}: Vessel not found. Cannot switch to vessel.");
                return;
            }
            VesselNavigator.NavigateToVessel(SelectedBookmark.Vessel);
        }

        /// <summary>
        /// Begin editing the selected bookmark's comment (opens the edition overlay).
        /// The Comment buffer is already kept in sync with the selection (see _onSelectedBookmarkChanged).
        /// </summary>
        public void BeginCommentEdition()
        {
            if( SelectedBookmark == null ) return;
            EditingComment = true;
        }

        /// <summary>
        /// Update the comment of the current bookmark, then close the edition overlay.
        /// </summary>
        public void SaveBookmarkComment()
        {
            EditingComment = false;
            if( SelectedBookmark == null ) return;
            SelectedBookmark.Comment = Comment;
            BookmarkManager.OnBookmarksUpdated.Fire();
        }

        /// <summary>
        /// Discard comment edits (restore the buffer from the bookmark), then close the edition overlay.
        /// </summary>
        public void CancelBookmarkCommentEdition()
        {
            EditingComment = false;
            if( SelectedBookmark == null ) return;
            Comment = SelectedBookmark.Comment;
        }

        // ======================================================================
        //  Bookmark actions
        // ======================================================================

        /// <summary>
        /// Move the given bookmark up
        /// </summary>
        /// <param name="bookmark">The bookmark to move up</param>
        public void MoveUp(Bookmark bookmark) {
            SelectedBookmark = bookmark;
            List<Bookmark> bookmarks = AvailableBookmarks[bookmark.BookmarkType];

            int index = bookmarks.IndexOf(bookmark);
            Bookmark previousBookmark = bookmarks[index - 1];

            while( bookmark.Order > previousBookmark.Order ) {
                BookmarkManager.MoveBookmarkUp(bookmark, false);
            }
            BookmarkManager.OnBookmarksUpdated.Fire();
        }

        /// <summary>
        /// Move the given bookmark down
        /// </summary>
        /// <param name="bookmark">The bookmark to move down</param>
        public void MoveDown(Bookmark bookmark) {
            SelectedBookmark = bookmark;
            List<Bookmark> bookmarks = AvailableBookmarks[bookmark.BookmarkType];
            int index = bookmarks.IndexOf(bookmark);
            Bookmark nextBookmark = bookmarks[index + 1];

            while( bookmark.Order < nextBookmark.Order ) {
                BookmarkManager.MoveBookmarkDown(bookmark, false);
            }
            BookmarkManager.OnBookmarksUpdated.Fire();
        }

        /// <summary>
        /// Remove the given bookmark
        /// </summary>
        /// <param name="bookmark">The bookmark to remove</param>
        public void Remove(Bookmark bookmark) {
            if( this.IsSelected(bookmark) ) {
                SelectedBookmark = null;
            }
            BookmarkManager.RemoveBookmark(bookmark);
        }

        // ======================================================================
        //  Removal confirmation flow (drives the removal overlay)
        // ======================================================================

        /// <summary>
        /// Ask to remove the given bookmark: opens the confirmation overlay.
        /// </summary>
        /// <param name="bookmark">The bookmark to remove</param>
        public void RequestRemoval(Bookmark bookmark) {
            PendingRemoval = bookmark;
        }

        /// <summary>
        /// Confirm the pending removal: actually removes the bookmark and closes the overlay.
        /// </summary>
        public void ConfirmPendingRemoval() {
            Bookmark bookmark = PendingRemoval;
            PendingRemoval = null;
            if( bookmark == null ) return;
            Remove(bookmark);
        }

        /// <summary>
        /// Cancel the pending removal: closes the overlay without removing anything.
        /// </summary>
        public void CancelPendingRemoval() {
            PendingRemoval = null;
        }
    }
}