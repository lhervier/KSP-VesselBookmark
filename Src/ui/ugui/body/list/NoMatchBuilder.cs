using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui;
using com.github.lhervier.ksp.shared;
using com.github.lhervier.ksp.shared.ugui;
using com.github.lhervier.ksp.shared.ugui.button;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.body.list
{
    /// <summary>
    /// Panneau qui remplace les sections quand le filtrage ne laisse aucun signet : il annonce
    /// l'absence de correspondance, rappelle les critères en clair et offre la réinitialisation. Un
    /// utilisateur tombé sur une liste vide voit ainsi POURQUOI elle l'est, et comment en sortir.
    /// </summary>
    public class NoMatchBuilder : IUGUIBuilder<NoMatchController>
    {
        // Séparateur entre deux critères rappelés (« Mun · Station · « taxi » »).
        private const string CriteriaSeparator = "   ·   ";

        // ================================================
        // Builder parameters
        // ================================================

        private BookmarksViewModel _viewModel;
        public NoMatchBuilder WithViewModel(BookmarksViewModel viewModel)
        {
            this._viewModel = viewModel;
            return this;
        }

        private Transform _parent;
        public NoMatchBuilder WithParent(Transform parent)
        {
            this._parent = parent;
            return this;
        }

        // =======================================
        // Build
        // =======================================

        public NoMatchController Build()
        {
            var go = new GameObject("NoMatch", typeof(RectTransform));
            go.transform.SetParent(_parent, false);

            var layout = go.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(
                Mathf.RoundToInt(VesselBookmarkPalette.EmptyPaddingH),
                Mathf.RoundToInt(VesselBookmarkPalette.EmptyPaddingH),
                Mathf.RoundToInt(VesselBookmarkPalette.EmptyPaddingV),
                Mathf.RoundToInt(VesselBookmarkPalette.EmptyPaddingV));
            layout.spacing = VesselBookmarkPalette.EmptySpacing;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            AddText(go.transform, "Title", ModLocalization.GetString("emptyNoMatchTitle"),
                VesselBookmarkPalette.EmptyTitleFontSize, VesselBookmarkPalette.EmptyTitleColor);

            AddText(go.transform, "Criteria",
                ModLocalization.GetString("emptyNoMatchCriteria", FormatCriteria()),
                VesselBookmarkPalette.EmptyTextFontSize, VesselBookmarkPalette.EmptyTextColor);

            BuildResetButton(go.transform);

            return go.AddComponent<NoMatchController>();
        }

        // ----------------------------------------------------------------
        //  Sous-éléments
        // ----------------------------------------------------------------

        /// <summary>Les valeurs des critères actifs, mises bout à bout pour l'affichage.</summary>
        private string FormatCriteria()
        {
            var builder = new StringBuilder();
            List<FilterCriterion> criteria = _viewModel.ActiveCriteria;
            foreach( FilterCriterion criterion in criteria )
            {
                if( builder.Length > 0 ) builder.Append(CriteriaSeparator);
                builder.Append(criterion.Value);
            }
            return builder.ToString();
        }

        private void BuildResetButton(Transform parent)
        {
            // Centré : le layout étire ses enfants en largeur, on emboîte donc le bouton dans une
            // ligne qui, elle, ne l'étire pas.
            var rowGo = new GameObject("ResetRow", typeof(RectTransform));
            rowGo.transform.SetParent(parent, false);
            var rowLayout = rowGo.AddComponent<HorizontalLayoutGroup>();
            rowLayout.padding = new RectOffset(0, 0, 0, 0);
            rowLayout.spacing = 0f;
            rowLayout.childAlignment = TextAnchor.MiddleCenter;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = false;

            ButtonController reset = new VBMButtonBuilder()
                .WithObjectName("ResetFilters")
                .WithLabel(ModLocalization.GetString("menuResetFilters"))
                .WithSize(VesselBookmarkPalette.CardButtonHeight)
                .WithAutoWidth(VesselBookmarkPalette.CardButtonPaddingH)
                .WithFontSize(VesselBookmarkPalette.CardButtonFontSize)
                .Build();
            reset.transform.SetParent(rowGo.transform, false);
            reset.OnClick.Add(() => _viewModel.ClearFilters());
        }

        private static TextMeshProUGUI AddText(Transform parent, string objectName, string text, int fontSize, Color color)
        {
            var go = new GameObject(objectName, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var label = UGUILabels.AddLabel(go);
            label.text = text;
            label.fontSize = fontSize;
            label.color = color;
            label.alignment = TextAlignmentOptions.Top;
            label.enableWordWrapping = true;
            return label;
        }
    }
}
