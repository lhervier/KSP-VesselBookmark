using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using com.github.lhervier.ksp.bookmarksmod.bookmarks;
using com.github.lhervier.ksp.bookmarksmod.util;
using System;
using com.github.lhervier.ksp.shared;

namespace com.github.lhervier.ksp.bookmarksmod.ui {
    public class BookmarksViewModel : MonoBehaviour
    {
        private static readonly ModLogger LOGGER = new ModLogger("BookmarksViewModel");

        private static readonly float SEARCH_DEBOUNCE_SECONDS = 0.2f;
        private static readonly string ALL_VESSEL_TYPES = "All";

        /// <summary>
        /// Vessel-type filter value grouping every "unknown" type — i.e. a real VesselType that the
        /// mod does not give a dedicated label to. Its label comes from the "vesselTypeOther" key.
        /// </summary>
        private static readonly string OTHER_VESSEL_TYPE = "Other";

        private BookmarksManager _bookmarkManager;
        
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
                return _bookmarkManager.GetBookmarksCount();
            }
        }
        
        /// <summary>
        /// The list of available bodies
        /// </summary>
        public IReadOnlyList<string> AvailableBodies => _availableBodies;
        private List<string> _availableBodies = new List<string>();
        public readonly EventVoid OnAvailableBodiesChanged = new EventVoid("BookmarksViewModel.OnAvailableBodiesChanged");

        /// <summary>
        /// Body-filter value standing for "any body". Kept as an opaque token : the combo turns it
        /// into the localized "Tous" label via its LabelFor.
        /// </summary>
        public const string ALL_BODIES = "All";

        /// <summary>
        /// Body-filter value standing for the current main body. Kept as an opaque token : the combo
        /// turns it into a "Courant (&lt;body&gt;)" label via its LabelFor, and filtering resolves it
        /// to CurrentBodyName.
        /// </summary>
        public const string CURRENT_BODY = "CURRENT";

        /// <summary>
        /// Name of the current main body (the one CURRENT_BODY resolves to), or null when there is no
        /// current body (not in flight) — in which case the CURRENT_BODY entry is absent from the list.
        /// </summary>
        public string CurrentBodyName => _currentBodyName;
        private string _currentBodyName = null;
        
        /// <summary>
        /// The list of available vessel types
        /// </summary>
        public IReadOnlyList<string> AvailableVesselTypes => _availableVesselTypes;
        private List<string> _availableVesselTypes = new List<string>();
        public readonly EventVoid OnAvailableVesselTypesChanged = new EventVoid("BookmarksViewModel.OnAvailableVesselTypesChanged");

        /// <summary>
        /// Situation-filter value standing for "any situation". Kept as an opaque token : the combo
        /// turns it into the localized "Tous" label via its LabelFor.
        /// </summary>
        public const string ALL_SITUATIONS = "All";

        /// <summary>
        /// The list of available vessel situations (raw enum names, e.g. "LANDED"), plus the
        /// ALL_SITUATIONS token in front.
        /// </summary>
        public IReadOnlyList<string> AvailableSituations => _availableSituations;
        private List<string> _availableSituations = new List<string>();
        public readonly EventVoid OnAvailableSituationsChanged = new EventVoid("BookmarksViewModel.OnAvailableSituationsChanged");

        // ===============================================================
        // "Populated" values : the subset of the exhaustive lists that is actually carried by at least
        // one bookmark. The combos list every possible value but grey out the ones absent from these
        // sets (a filter on such a value would return nothing). Recomputed on every bookmarks update.
        // ===============================================================

        private readonly HashSet<string> _populatedBodies = new HashSet<string>();
        private readonly HashSet<string> _populatedVesselTypes = new HashSet<string>();
        private readonly HashSet<string> _populatedSituations = new HashSet<string>();

        /// <summary>
        /// Whether the given body filter value matches at least one bookmark (the "all"/"current"
        /// tokens always do). Drives the greying of the body combo.
        /// </summary>
        public bool IsBodyPopulated(string body) {
            if( body == ALL_BODIES || body == CURRENT_BODY ) return true;
            return _populatedBodies.Contains(body);
        }

        /// <summary>
        /// Whether the given vessel-type filter value matches at least one bookmark (the "all" token
        /// always does). Drives the greying of the type combo.
        /// </summary>
        public bool IsVesselTypePopulated(string vesselType) {
            if( vesselType == ALL_VESSEL_TYPES ) return true;
            return _populatedVesselTypes.Contains(vesselType);
        }

        /// <summary>
        /// Whether the given situation filter value matches at least one bookmark (the "all" token
        /// always does). Drives the greying of the situation combo.
        /// </summary>
        public bool IsSituationPopulated(string situation) {
            if( situation == ALL_SITUATIONS ) return true;
            return _populatedSituations.Contains(situation);
        }

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
        private string _selectedBody = ALL_BODIES;
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
        /// The selected vessel situation (raw enum name, or ALL_SITUATIONS)
        /// </summary>
        public string SelectedSituation {
            get => _selectedSituation;
            set {
                if( value == _selectedSituation ) return;
                _selectedSituation = value;
                OnSelectedSituationChanged.Fire();
            }
        }
        private string _selectedSituation = ALL_SITUATIONS;
        public readonly EventVoid OnSelectedSituationChanged = new EventVoid("BookmarksViewModel.OnSelectedSituationChanged");

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
        /// Whether the comment edition popin is open.
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
        /// The bookmark whose removal is awaiting confirmation (drives the removal popin). Null if none.
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

        public void Initialize(BookmarksManager bookmarkManager)
        {
            this._bookmarkManager = bookmarkManager;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public void Start() 
        {
            this.OnSelectedVesselTypeChanged.Add(UpdateBookmarksSelection);
            this.OnSelectedSituationChanged.Add(UpdateBookmarksSelection);
            this.OnSelectedBodyChanged.Add(UpdateBookmarksSelection);
            this.OnSearchTextChanged.Add(UpdateBookmarksSelection);
            this.OnFilterHasCommentChanged.Add(UpdateBookmarksSelection);

            this.OnSelectedBookmarkChanged.Add(_onSelectedBookmarkChanged);

            _bookmarkManager.OnBookmarksUpdated.Add(UpdateBookmarksSelection);

            // Live game state: active vessel / target highlighting depends on game state that changes
            // without any bookmark event, so listen to the relevant GameEvents and re-broadcast.
            GameEvents.onVesselChange.Add(_onActiveVesselChanged);
            GameEvents.OnTargetObjectChanged.Add(_onTargetChanged);

            // The "current body" filter follows the active vessel's main body, which moves on vessel
            // switch and on SOI crossing — neither raises a bookmark event.
            GameEvents.onVesselSOIChanged.Add(_onVesselSOIChanged);

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
            _bookmarkManager.OnBookmarksUpdated.Remove(UpdateBookmarksSelection);

            this.OnFilterHasCommentChanged.Remove(UpdateBookmarksSelection);
            this.OnSearchTextChanged.Remove(UpdateBookmarksSelection);
            this.OnSelectedBodyChanged.Remove(UpdateBookmarksSelection);
            this.OnSelectedSituationChanged.Remove(UpdateBookmarksSelection);
            this.OnSelectedVesselTypeChanged.Remove(UpdateBookmarksSelection);

            this.OnSelectedBookmarkChanged.Remove(_onSelectedBookmarkChanged);

            GameEvents.onVesselChange.Remove(_onActiveVesselChanged);
            GameEvents.OnTargetObjectChanged.Remove(_onTargetChanged);
            GameEvents.onVesselSOIChanged.Remove(_onVesselSOIChanged);
        }

        private void _onSelectedBookmarkChanged()
        {
            Comment = SelectedBookmark?.Comment ?? string.Empty;
        }

        private void _onActiveVesselChanged(Vessel vessel)
        {
            this.OnActiveOrTargetChanged.Fire();
            if( string.Equals(SelectedBody, CURRENT_BODY) ) {
                UpdateBookmarksSelection();
            }
        }

        private void _onTargetChanged(MapObject target)
        {
            this.OnActiveOrTargetChanged.Fire();
        }

        private void _onVesselSOIChanged(GameEvents.HostedFromToAction<Vessel, CelestialBody> action)
        {
            // Only the active vessel's SOI change moves the current main body.
            if( action.host != FlightGlobals.ActiveVessel ) return;
            if( string.Equals(SelectedBody, CURRENT_BODY) ) {
                UpdateBookmarksSelection();
            }
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
                this.UpdateAvailableSituations();
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

                // "Populated" set : bodies actually carried by at least one bookmark (matched against
                // VesselBodyName, exactly like the body filter does). Drives the greying.
                _populatedBodies.Clear();
                foreach (Bookmark bookmark in _bookmarkManager.GetAllBookmarks()) {
                    if( string.IsNullOrEmpty(bookmark.VesselBodyName) ) {
                        continue;
                    }
                    _populatedBodies.Add(bookmark.VesselBodyName);
                }

                // Exhaustive list : every body of the current system (sorted Kerbol-outward with moons
                // interleaved), so a selected body is always a valid entry — the combo just greys the
                // ones absent from _populatedBodies.
                _availableBodies = CelestialBodySorter.GetSortedBodyNames();
                _availableBodies.Insert(0, ALL_BODIES);

                // Current-body shortcut, just after "All" (the default). Only available in flight
                // (a current main body exists) ; it resolves to that body's name for filtering.
                CelestialBody currentBody = FlightGlobals.ActiveVessel?.mainBody ?? FlightGlobals.currentMainBody;
                if( currentBody != null ) {
                    _currentBodyName = currentBody.bodyName;
                    _availableBodies.Insert(1, CURRENT_BODY);
                } else {
                    _currentBodyName = null;
                }

                // Only CURRENT_BODY can now be absent from the list (real bodies are always present) :
                // fall back to "All" when leaving flight while it was selected.
                if( !_availableBodies.Contains(SelectedBody) ) {
                    SelectedBody = ALL_BODIES;
                }
                this.OnAvailableBodiesChanged.Fire();
            } catch (Exception e) {
                LOGGER.LogError($"Error updating available bodies: {e.Message}");
            }
        }

        private void UpdateAvailableVesselTypes() {
            try {
                LOGGER.LogDebug($"Updating available vessel types");

                // "Populated" set : a known type present on a bookmark adds itself ; any exotic
                // (non-known, non-empty) type adds the OTHER token. Empty/unresolved types are ignored
                // (like the situation filter), so they never populate "Other". Drives the greying.
                _populatedVesselTypes.Clear();
                foreach (Bookmark bookmark in _bookmarkManager.GetAllBookmarks()) {
                    string vesselType = bookmark.BookmarkVesselType;
                    if( string.IsNullOrEmpty(vesselType) ) {
                        continue;
                    }
                    _populatedVesselTypes.Add(IsKnownVesselType(vesselType) ? vesselType : OTHER_VESSEL_TYPE);
                }

                // List : "All" + every known VesselType (enum order) + "Other". BookmarkVesselType is
                // always a VesselType name ; every type without a dedicated label is grouped under
                // "Other" rather than shown as a blank entry. Unused entries are greyed by the combo.
                _availableVesselTypes = new List<string> { ALL_VESSEL_TYPES };
                foreach (VesselType vesselType in Enum.GetValues(typeof(VesselType))) {
                    string name = vesselType.ToString();
                    if( IsKnownVesselType(name) ) {
                        _availableVesselTypes.Add(name);
                    }
                }
                _availableVesselTypes.Add(OTHER_VESSEL_TYPE);

                this.OnAvailableVesselTypesChanged.Fire();
            } catch (Exception e) {
                LOGGER.LogError($"Error updating available vessel types: {e.Message}");
            }
        }

        private void UpdateAvailableSituations() {
            try {
                LOGGER.LogDebug($"Updating available situations");

                // "Populated" set : situations actually carried by at least one bookmark. Drives the greying.
                _populatedSituations.Clear();
                foreach (Bookmark bookmark in _bookmarkManager.GetAllBookmarks()) {
                    if( !string.IsNullOrEmpty(bookmark.VesselSituation) ) {
                        _populatedSituations.Add(bookmark.VesselSituation);
                    }
                }

                // Exhaustive list : every Vessel.Situations value (raw enum name). Enum declaration
                // order (ground → space) ; unused ones are greyed by the combo.
                _availableSituations = new List<string> { ALL_SITUATIONS };
                foreach (Vessel.Situations situation in Enum.GetValues(typeof(Vessel.Situations))) {
                    _availableSituations.Add(situation.ToString());
                }

                this.OnAvailableSituationsChanged.Fire();
            } catch (Exception e) {
                LOGGER.LogError($"Error updating available situations: {e.Message}");
            }
        }

        /// <summary>
        /// Update the available bookmarks
        /// </summary>
        private void UpdateAvailableBookmarks() {
            try {
                LOGGER.LogDebug($"Updating available bookmarks");
                this._availableBookmarks.Clear();

                // Resolve the "current body" shortcut to the actual body name it stands for.
                string filterBody = SelectedBody;
                if( string.Equals(SelectedBody, CURRENT_BODY) ) {
                    filterBody = _currentBodyName;
                }

                foreach( var bookmark in _bookmarkManager.GetAllBookmarks() ) {
                    // Each criterion is an independent AND filter. A criterion left at its "all"
                    // (or empty) value lets every bookmark through, so a bookmark is kept only when
                    // it passes all of them.
                    if( !MatchesBody(bookmark, filterBody) ) continue;
                    if( !MatchesVesselType(bookmark) ) continue;
                    if( !MatchesSituation(bookmark) ) continue;
                    if( !MatchesSearchText(bookmark) ) continue;
                    if( !MatchesHasComment(bookmark) ) continue;

                    if( !_availableBookmarks.TryGetValue(bookmark.BookmarkType, out List<Bookmark> b) )
                    {
                        b = new List<Bookmark>();
                        _availableBookmarks[bookmark.BookmarkType] = b;
                    }
                    b.Add(bookmark);
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

        // ----------------------------------------------------------------------
        //  Per-criterion filters (each returns true when the bookmark passes the
        //  criterion, including when the criterion is inactive / set to "all").
        // ----------------------------------------------------------------------

        /// <summary>
        /// Whether the bookmark passes the body filter.
        /// </summary>
        /// <param name="bookmark">The bookmark to test</param>
        /// <param name="filterBody">The body to match, already resolved from the CURRENT_BODY shortcut</param>
        private bool MatchesBody(Bookmark bookmark, string filterBody) {
            if( string.Equals(filterBody, ALL_BODIES) ) return true;
            return string.Equals(bookmark.VesselBodyName, filterBody);
        }

        /// <summary>
        /// Whether the bookmark passes the vessel-type filter.
        /// </summary>
        /// <param name="bookmark">The bookmark to test</param>
        private bool MatchesVesselType(Bookmark bookmark) {
            if( string.Equals(SelectedVesselType, ALL_VESSEL_TYPES) ) return true;
            string vesselType = bookmark.BookmarkVesselType;
            if( string.Equals(SelectedVesselType, OTHER_VESSEL_TYPE) ) {
                // "Other" groups every real (non-empty) type outside the known set.
                return !string.IsNullOrEmpty(vesselType) && !IsKnownVesselType(vesselType);
            }
            return string.Equals(vesselType, SelectedVesselType);
        }

        /// <summary>
        /// Whether the mod gives the given raw VesselType name a dedicated filter entry. "Known" is
        /// defined as "has a dedicated label" : any type without one is grouped under OTHER_VESSEL_TYPE.
        /// </summary>
        /// <param name="vesselType">The raw VesselType name (e.g. "Ship")</param>
        private static bool IsKnownVesselType(string vesselType) {
            return !string.IsNullOrEmpty(ModLocalization.GetString("vesselType" + vesselType));
        }

        /// <summary>
        /// Whether the bookmark passes the situation filter.
        /// </summary>
        /// <param name="bookmark">The bookmark to test</param>
        private bool MatchesSituation(Bookmark bookmark) {
            if( string.Equals(SelectedSituation, ALL_SITUATIONS) ) return true;
            return string.Equals(bookmark.VesselSituation, SelectedSituation);
        }

        /// <summary>
        /// Whether the bookmark passes the free-text search filter.
        /// </summary>
        /// <param name="bookmark">The bookmark to test</param>
        private bool MatchesSearchText(Bookmark bookmark) {
            if( string.IsNullOrEmpty(SearchText) ) return true;
            string fullSearchText = bookmark.BookmarkTitle + " ";
            fullSearchText += bookmark.VesselSituationLabel + " ";   // Situation contains celestial body name
            fullSearchText += bookmark.VesselName + " ";
            fullSearchText += ModLocalization.GetString("vesselType" + bookmark.BookmarkVesselType) + " ";
            fullSearchText += bookmark.Comment + " ";
            return fullSearchText.ToLower().Contains(SearchText.ToLower());
        }

        /// <summary>
        /// Whether the bookmark passes the "has comment" filter.
        /// </summary>
        /// <param name="bookmark">The bookmark to test</param>
        private bool MatchesHasComment(Bookmark bookmark) {
            if( !FilterHasComment ) return true;
            return !string.IsNullOrEmpty(bookmark.Comment);
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
            
            if (_bookmarkManager.HasBookmark(BookmarkType.Vessel, vesselPersistentID)) {
                ScreenMessages.PostScreenMessage(
                    ModLocalization.GetString("messageBookmarkAlreadyExists"),
                    2f,
                    ScreenMessageStyle.UPPER_CENTER
                );
                return;
            }

            VesselBookmark bookmark = new VesselBookmark(vesselPersistentID);
            if (_bookmarkManager.AddBookmark(bookmark)) {
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
        public void ForceReload() {
            _bookmarkManager.ForceReload();
        }

        // =============================================================================
        //  Filters
        // =============================================================================

        /// <summary>
        /// Whether any filter is currently active (différent de l'état par défaut).
        /// </summary>
        public bool HasActiveFilters {
            get {
                return SelectedBody != ALL_BODIES
                    || SelectedVesselType != ALL_VESSEL_TYPES
                    || SelectedSituation != ALL_SITUATIONS
                    || !string.IsNullOrEmpty(SearchText)
                    || FilterHasComment;
            }
        }

        /// <summary>
        /// Clear the filters
        /// </summary>
        public void ClearFilters() {
            this._preventBookmarksUpdates = true;
            SelectedBody = ALL_BODIES;
            SelectedVesselType = ALL_VESSEL_TYPES;
            SelectedSituation = ALL_SITUATIONS;
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
            if( FlightGlobals.ActiveVessel == null )
            {
                return true;
            }
            if( !FlightGlobals.ActiveVessel.loaded )
            {
                return true;
            }
            return SelectedBookmark.VesselPersistentID != FlightGlobals.ActiveVessel.persistentId;
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
            // SetVesselTarget ne déclenche pas de façon fiable GameEvents.OnTargetObjectChanged ici :
            // on diffuse nous-mêmes pour que les lignes réévaluent le tag "cible" immédiatement.
            this.OnActiveOrTargetChanged.Fire();
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
        /// Begin editing the selected bookmark's comment (opens the edition popin).
        /// The Comment buffer is already kept in sync with the selection (see _onSelectedBookmarkChanged).
        /// </summary>
        public void BeginCommentEdition()
        {
            if( SelectedBookmark == null ) return;
            EditingComment = true;
        }

        /// <summary>
        /// Update the comment of the current bookmark, then close the edition popin.
        /// </summary>
        public void SaveBookmarkComment()
        {
            EditingComment = false;
            if( SelectedBookmark == null ) return;
            SelectedBookmark.Comment = Comment;
            _bookmarkManager.OnBookmarksUpdated.Fire();
        }

        /// <summary>
        /// Discard comment edits (restore the buffer from the bookmark), then close the edition popin.
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
                _bookmarkManager.MoveBookmarkUp(bookmark);
            }
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
                _bookmarkManager.MoveBookmarkDown(bookmark);
            }
        }

        /// <summary>
        /// Remove the given bookmark
        /// </summary>
        /// <param name="bookmark">The bookmark to remove</param>
        public void Remove(Bookmark bookmark) {
            if( this.IsSelected(bookmark) ) {
                SelectedBookmark = null;
            }
            _bookmarkManager.RemoveBookmark(bookmark);
        }

        // ======================================================================
        //  Removal confirmation flow (drives the removal popin)
        // ======================================================================

        /// <summary>
        /// Ask to remove the given bookmark: opens the confirmation popin.
        /// </summary>
        /// <param name="bookmark">The bookmark to remove</param>
        public void RequestRemoval(Bookmark bookmark) {
            PendingRemoval = bookmark;
        }

        /// <summary>
        /// Confirm the pending removal: actually removes the bookmark and closes the popin.
        /// </summary>
        public void ConfirmPendingRemoval() {
            Bookmark bookmark = PendingRemoval;
            PendingRemoval = null;
            if( bookmark == null ) return;
            Remove(bookmark);
        }

        /// <summary>
        /// Cancel the pending removal: closes the popin without removing anything.
        /// </summary>
        public void CancelPendingRemoval() {
            PendingRemoval = null;
        }
    }
}