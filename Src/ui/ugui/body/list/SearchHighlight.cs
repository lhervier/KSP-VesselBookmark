using System;
using System.Text;
using UnityEngine;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.body.list
{
    /// <summary>
    /// Marks the occurrences of the search text inside a bookmark row's texts, so a row tells at a
    /// glance why it survived the filtering.
    /// </summary>
    public static class SearchHighlight
    {
        // TMP's <mark> only paints a background: the text keeps its own color, so a highlighted title
        // stays "active vessel white" or "target green". Built once, from the palette.
        private static string _openTag;
        private static string OpenTag
        {
            get
            {
                if (_openTag == null)
                {
                    _openTag = "<mark=#" + ColorUtility.ToHtmlStringRGBA(VesselBookmarkPalette.SearchHighlightColor) + ">";
                }
                return _openTag;
            }
        }
        private const string CloseTag = "</mark>";

        /// <summary>
        /// Returns the text with every occurrence of the search string wrapped in a TMP highlight tag.
        /// Returns it unchanged when either is empty.
        /// </summary>
        /// <param name="text">The text to mark up</param>
        /// <param name="search">The searched string (matched case-insensitively)</param>
        public static string Apply(string text, string search)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(search))
            {
                return text;
            }

            // Ordinal comparison: it guarantees the match has the same length as the search string,
            // which is what lets us copy the ORIGINAL casing back between the tags.
            int from = 0;
            int found = text.IndexOf(search, StringComparison.OrdinalIgnoreCase);
            if (found < 0)
            {
                return text;
            }

            var builder = new StringBuilder(text.Length + 32);
            while (found >= 0)
            {
                builder.Append(text, from, found - from);
                builder.Append(OpenTag).Append(text, found, search.Length).Append(CloseTag);
                from = found + search.Length;
                found = from < text.Length ? text.IndexOf(search, from, StringComparison.OrdinalIgnoreCase) : -1;
            }
            builder.Append(text, from, text.Length - from);
            return builder.ToString();
        }
    }
}
