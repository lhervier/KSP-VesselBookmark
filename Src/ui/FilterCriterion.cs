namespace com.github.lhervier.ksp.bookmarksmod.ui
{
    /// <summary>
    /// Identifies one of the search criteria. Used to designate a criterion across the UI (the filter
    /// bar removes one by id) without exposing which ViewModel property backs it.
    /// </summary>
    public enum FilterCriterionId
    {
        Text,
        Body,
        VesselType,
        Situation,
        HasComment,
    }

    /// <summary>
    /// An active search criterion, ready to be displayed: its identifier plus the localized key/value
    /// pair naming it (e.g. "Corps" / "Mun"). Produced by the ViewModel, consumed by the filter bar and
    /// by the "no match" empty state.
    /// </summary>
    public class FilterCriterion
    {
        public FilterCriterionId Id { get; }

        /// <summary>Localized name of the criterion (e.g. "Corps").</summary>
        public string Key { get; }

        /// <summary>Localized value it is currently set to (e.g. "Mun").</summary>
        public string Value { get; }

        public FilterCriterion(FilterCriterionId id, string key, string value)
        {
            this.Id = id;
            this.Key = key;
            this.Value = value;
        }
    }
}
