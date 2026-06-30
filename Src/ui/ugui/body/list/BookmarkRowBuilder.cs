using UnityEngine;
using UnityEngine.UI;
using TMPro;
using com.github.lhervier.ksp.shared.ugui.button;
using com.github.lhervier.ksp.bookmarksmod.bookmarks;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.sprites;
using com.github.lhervier.ksp.shared;
using com.github.lhervier.ksp.shared.ugui;
using com.github.lhervier.ksp.shared.ugui.sprites;
using com.github.lhervier.ksp.shared.ugui.styles;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.body.list
{
    /// <summary>
    /// Une ligne de bookmark : ligne 1 (icône type + alarme + titre + pastille d'état + boutons ▲▼✕),
    /// ligne 2 (situation + nom de vaisseau), ligne 3 (commentaire). Le fond, le liseré gauche, la
    /// couleur du titre, la pastille et la visibilité des boutons dépendent de l'état (sélection,
    /// survol, vaisseau actif, cible) et sont réévalués par Refresh().
    /// </summary>
    public class BookmarkRowBuilder : IUGUIBuilder<BookmarkRowController>
    {
        // Triangles/cross when the font (or a fallback) provides them, plain arrows/x otherwise.
        private static string MoveUpLabel => DefaultPalette.PickGlyph("▲", "↑");
        private static string MoveDownLabel => DefaultPalette.PickGlyph("▼", "↓");
        private static string RemoveLabel => DefaultPalette.PickGlyph("✕", "✗", "×", "x");

        // ===========================================
        // Builder parameters
        // ===========================================

        private BookmarksViewModel _viewModel;
        public BookmarkRowBuilder WithViewModel(BookmarksViewModel viewModel)
        {
            this._viewModel = viewModel;
            return this;
        }

        private Bookmark _bookmark;
        public BookmarkRowBuilder WithBookmark(Bookmark bookmark)
        {
            this._bookmark = bookmark;
            return this;
        }

        private bool _first;
        public BookmarkRowBuilder WithFirstState(bool first)
        {
            this._first = first;
            return this;
        }

        private bool _last;
        public BookmarkRowBuilder WithLastState(bool last)
        {
            this._last = last;
            return this;
        }

        // ==========================================
        // Build
        // ==========================================

        public BookmarkRowController Build()
        {
            var rowGo = new GameObject("Row", typeof(RectTransform));
            
            // Fond (teinté selon l'état)
            var bg = rowGo.AddComponent<Image>();
            bg.sprite = SpritesGlobal.FillSprite;
            bg.type = Image.Type.Simple;
            bg.color = Color.clear;
            bg.raycastTarget = true;   // cible des clics/survol sur les zones vides de la ligne

            // Séparateur 1px en bas via un sprite à bordures horizontales ? On garde simple : pas de
            // séparateur baked ici ; la lisibilité vient du fond + liseré. (Ajustable plus tard.)

            var vlayout = rowGo.AddComponent<VerticalLayoutGroup>();
            vlayout.padding = new RectOffset(
                Mathf.RoundToInt(VesselBookmarkPalette.RowPaddingH),
                Mathf.RoundToInt(VesselBookmarkPalette.RowPaddingH),
                Mathf.RoundToInt(VesselBookmarkPalette.RowPaddingV),
                Mathf.RoundToInt(VesselBookmarkPalette.RowPaddingV));
            vlayout.spacing = 2f;
            vlayout.childAlignment = TextAnchor.UpperLeft;
            vlayout.childControlWidth = true;
            vlayout.childControlHeight = true;
            vlayout.childForceExpandWidth = true;
            vlayout.childForceExpandHeight = false;

            // Liseré gauche (sélection / vaisseau actif) — superposé, hors layout
            var accentGo = new GameObject("AccentBar", typeof(RectTransform));
            accentGo.transform.SetParent(rowGo.transform, false);
            var accentLe = accentGo.AddComponent<LayoutElement>();
            accentLe.ignoreLayout = true;
            var accentRect = accentGo.GetComponent<RectTransform>();
            accentRect.anchorMin = new Vector2(0f, 0f);
            accentRect.anchorMax = new Vector2(0f, 1f);
            accentRect.pivot = new Vector2(0f, 0.5f);
            accentRect.sizeDelta = new Vector2(VesselBookmarkPalette.RowAccentBarThickness, 0f);
            accentRect.anchoredPosition = Vector2.zero;
            var accentBar = accentGo.AddComponent<Image>();
            accentBar.sprite = SpritesGlobal.FillSprite;
            accentBar.type = Image.Type.Simple;
            accentBar.color = DefaultPalette.AccentColor;
            accentBar.raycastTarget = false;
            accentBar.enabled = false;

            bool vesselExists = _bookmark.Vessel != null;

            // ---- Ligne 1 : icône type + alarme + titre + pastille + boutons ----
            var line1 = NewHLine(rowGo.transform, "Line1", 0);
            var line1Layout = line1.GetComponent<HorizontalLayoutGroup>();
            line1Layout.spacing = VesselBookmarkPalette.RowSpacing;
            line1Layout.childAlignment = TextAnchor.MiddleLeft;

            BuildTypeIcon(line1.transform, _bookmark.BookmarkVesselType);
            if (_bookmark.HasAlarm)
            {
                BuildAlarmIcon(line1.transform);
            }

            // Titre
            var nameGo = new GameObject("Name", typeof(RectTransform));
            nameGo.transform.SetParent(line1.transform, false);
            var nameLe = nameGo.AddComponent<LayoutElement>();
            nameLe.flexibleWidth = 1f;
            var name = UGUILabels.AddLabel(nameGo);
            name.text = BuildTitle(_bookmark, vesselExists);
            name.fontSize = VesselBookmarkPalette.NameFontSize;
            name.fontStyle = vesselExists ? FontStyles.Normal : FontStyles.Italic;
            name.color = VesselBookmarkPalette.NameColor;
            name.alignment = TextAlignmentOptions.Left;
            name.enableWordWrapping = true;

            // Pastille d'état (Actif / Cible / Disparu) — créée, affichée selon l'état dans Refresh()
            GameObject chip = BuildChip(line1.transform, out Image chipImage, out TextMeshProUGUI chipText);

            // Boutons ▲ ▼ ✕ (révélés au survol / sélection)
            CanvasGroup rowButtonsGroup = BuildRowButtons(
                line1.transform, 
                _bookmark, 
                _first, 
                _last,
                out ButtonController upButton,
                out ButtonController downButton,
                out ButtonController removeButton
            );

            // ---- Ligne 2 : situation (+ nom de vaisseau) ----
            var line2 = NewHLine(rowGo.transform, "Line2", Mathf.RoundToInt(VesselBookmarkPalette.TypeIconSize + VesselBookmarkPalette.RowSpacing));
            line2.GetComponent<HorizontalLayoutGroup>().spacing = DefaultPalette.Spacing;

            var situationGo = new GameObject("Situation", typeof(RectTransform));
            situationGo.transform.SetParent(line2.transform, false);
            var situation = UGUILabels.AddLabel(situationGo);
            situation.text = _bookmark.VesselSituationLabel;
            situation.fontSize = VesselBookmarkPalette.SituationFontSize;
            situation.color = VesselBookmarkPalette.SituationColor;
            situation.alignment = TextAlignmentOptions.Left;

            if (_bookmark is CommandModuleBookmark cmb && cmb.VesselName != cmb.CommandModuleName)
            {
                var vnameGo = new GameObject("VesselName", typeof(RectTransform));
                vnameGo.transform.SetParent(line2.transform, false);
                var vname = UGUILabels.AddLabel(vnameGo);
                vname.text = "(" + _bookmark.VesselName + ")";
                vname.fontSize = VesselBookmarkPalette.SituationFontSize;
                vname.color = VesselBookmarkPalette.VesselNameColor;
                vname.alignment = TextAlignmentOptions.Left;
            }

            // ---- Ligne 3 : commentaire (si présent) ----
            if (!string.IsNullOrEmpty(_bookmark.Comment))
            {
                BuildComment(rowGo.transform, _bookmark.Comment);
            }

            // Survol + clic sur la ligne (les boutons enfants consomment leurs propres clics).
            // PointerHandler plutôt qu'EventTrigger pour ne pas bloquer la molette (cf. PointerHandler).
            var pointer = rowGo.AddComponent<PointerHandler>();
            
            return rowGo.AddComponent<BookmarkRowController>()
                .WithViewModel(_viewModel)
                .WithBookmark(_bookmark)
                .WithBackground(bg)
                .WithAccentBar(accentBar)
                .WithNameComponent(name)
                .WithChip(chip)
                .WithChipImage(chipImage)
                .WithChipTextComponent(chipText)
                .WithRowButtons(rowButtonsGroup)
                .WithVesselExists(vesselExists)
                .WithPointerHandler(pointer)
                .WithUpButtonController(upButton)
                .WithDownButtonController(downButton)
                .WithRemoveButtonController(removeButton);
        }

        // ----------------------------------------------------------------
        //  Sous-éléments
        // ----------------------------------------------------------------

        private static GameObject NewHLine(Transform parent, string objectName, int padLeft)
        {
            var go = new GameObject(objectName, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(padLeft, 0, 0, 0);
            layout.spacing = 0f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            return go;
        }

        private void BuildTypeIcon(Transform parent, string vesselType)
        {
            var boxGo = new GameObject("TypeIcon", typeof(RectTransform));
            boxGo.transform.SetParent(parent, false);
            var le = boxGo.AddComponent<LayoutElement>();
            le.minWidth = le.preferredWidth = VesselBookmarkPalette.TypeIconSize;
            le.minHeight = le.preferredHeight = VesselBookmarkPalette.TypeIconSize;

            var box = boxGo.AddComponent<Image>();
            box.sprite = SpritesGlobal.Border(
                VesselBookmarkPalette.TypeIconBgColor,
                VesselBookmarkPalette.TypeIconBorderColor,
                VesselBookmarkPalette.TypeIconBorderThickness);
            box.type = Image.Type.Sliced;
            box.color = Color.white;
            box.raycastTarget = true;   // pour recevoir le survol du tooltip

            // Tooltip = type de vaisseau traduit
            if (!string.IsNullOrEmpty(vesselType))
            {
                Tooltips.Attach(boxGo, ModLocalization.GetString("vesselType" + vesselType));
            }

            Sprite icon = Icons.VesselType(vesselType);
            if (icon != null)
            {
                var iconGo = new GameObject("Glyph", typeof(RectTransform));
                iconGo.transform.SetParent(boxGo.transform, false);
                var iconRect = iconGo.GetComponent<RectTransform>();
                iconRect.anchorMin = Vector2.zero;
                iconRect.anchorMax = Vector2.one;
                iconRect.offsetMin = new Vector2(3f, 3f);
                iconRect.offsetMax = new Vector2(-3f, -3f);
                var iconImg = iconGo.AddComponent<Image>();
                iconImg.sprite = icon;
                iconImg.type = Image.Type.Simple;
                iconImg.preserveAspect = true;
                iconImg.color = Color.white;
                iconImg.raycastTarget = false;
            }
        }

        private void BuildAlarmIcon(Transform parent)
        {
            var go = new GameObject("Alarm", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>();
            le.minWidth = le.preferredWidth = VesselBookmarkPalette.AlarmIconSize;
            le.minHeight = le.preferredHeight = VesselBookmarkPalette.AlarmIconSize;
            var img = go.AddComponent<Image>();
            Sprite alarm = Icons.Alarm;
            if (alarm != null)
            {
                img.sprite = alarm;
                img.preserveAspect = true;
                img.color = Color.white;
            }
            else
            {
                img.sprite = SpritesGlobal.FillSprite;
                img.color = DefaultPalette.WarmColor;
            }
            img.type = Image.Type.Simple;
            img.raycastTarget = true;   // pour recevoir le survol du tooltip
            Tooltips.Attach(go, ModLocalization.GetString("VBM_tooltipAlarm"));
        }

        private GameObject BuildChip(Transform parent, out Image chipImage, out TextMeshProUGUI chipText)
        {
            var chipGo = new GameObject("Chip", typeof(RectTransform));
            chipGo.transform.SetParent(parent, false);

            chipImage = chipGo.AddComponent<Image>();
            chipImage.sprite = SpritesGlobal.Border(DefaultPalette.AccentBgColor, DefaultPalette.AccentBorderColor, VesselBookmarkPalette.ChipBorderThickness);
            chipImage.type = Image.Type.Sliced;
            chipImage.color = Color.white;
            chipImage.raycastTarget = false;

            var layout = chipGo.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(
                Mathf.RoundToInt(VesselBookmarkPalette.ChipPaddingH),
                Mathf.RoundToInt(VesselBookmarkPalette.ChipPaddingH),
                1, 1);
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            var labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(chipGo.transform, false);
            chipText = UGUILabels.AddLabel(labelGo);
            chipText.fontSize = VesselBookmarkPalette.ChipFontSize;
            chipText.color = DefaultPalette.AccentColor;
            chipText.alignment = TextAlignmentOptions.Center;

            chipGo.SetActive(false);
            return chipGo;
        }

        private CanvasGroup BuildRowButtons(
            Transform parent, 
            Bookmark bookmark, 
            bool isFirst, 
            bool isLast,
            out ButtonController upButton,
            out ButtonController downButton,
            out ButtonController removeButton
        )
        {
            var groupGo = new GameObject("RowButtons", typeof(RectTransform));
            groupGo.transform.SetParent(parent, false);

            var layout = groupGo.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = VesselBookmarkPalette.RowButtonSpacing;
            layout.childAlignment = TextAnchor.MiddleRight;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            var group = groupGo.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;

            // Row buttons use their own (smaller) size, font and colors, overriding the VBM defaults.
            upButton = new VBMButtonBuilder()
                .WithObjectName("MoveUp")
                .WithLabel(MoveUpLabel)
                .WithInteractableState(!isFirst)
                .WithSize(VesselBookmarkPalette.RowButtonSize)
                .WithFontSize(VesselBookmarkPalette.RowButtonFontSize)
                .WithBackgroundColor(VesselBookmarkPalette.RowButtonBgColor)
                .WithHoverColor(VesselBookmarkPalette.RowButtonHoverColor)
                .Build();
            upButton.transform.SetParent(groupGo.transform, false);
            Tooltips.Attach(upButton.gameObject, ModLocalization.GetString("VBM_tooltipMoveUp"));

            downButton = new VBMButtonBuilder()
                .WithObjectName("MoveDown")
                .WithLabel(MoveDownLabel)
                .WithInteractableState(!isLast)
                .WithSize(VesselBookmarkPalette.RowButtonSize)
                .WithFontSize(VesselBookmarkPalette.RowButtonFontSize)
                .WithBackgroundColor(VesselBookmarkPalette.RowButtonBgColor)
                .WithHoverColor(VesselBookmarkPalette.RowButtonHoverColor)
                .Build();
            downButton.transform.SetParent(groupGo.transform, false);
            Tooltips.Attach(downButton.gameObject, ModLocalization.GetString("VBM_tooltipMoveDown"));

            removeButton = new VBMButtonBuilder()
                .WithObjectName("Remove")
                .WithLabel(RemoveLabel)
                .WithSize(VesselBookmarkPalette.RowButtonSize)
                .WithFontSize(VesselBookmarkPalette.RowButtonFontSize)
                .WithBackgroundColor(VesselBookmarkPalette.RowButtonBgColor)
                .WithHoverColor(VesselBookmarkPalette.RowButtonDangerHoverColor)
                .Build();
            removeButton.transform.SetParent(groupGo.transform, false);
            Tooltips.Attach(removeButton.gameObject, ModLocalization.GetString("VBM_tooltipRemove"));

            return group;
        }

        private void BuildComment(Transform parent, string comment)
        {
            var line3 = NewHLine(parent, "Line3", Mathf.RoundToInt(VesselBookmarkPalette.TypeIconSize + VesselBookmarkPalette.RowSpacing));

            var boxGo = new GameObject("CommentBox", typeof(RectTransform));
            boxGo.transform.SetParent(line3.transform, false);
            var boxLe = boxGo.AddComponent<LayoutElement>();
            boxLe.flexibleWidth = 1f;

            var boxBg = boxGo.AddComponent<Image>();
            boxBg.sprite = SpritesGlobal.FillSprite;
            boxBg.type = Image.Type.Simple;
            boxBg.color = VesselBookmarkPalette.CommentBgColor;
            boxBg.raycastTarget = false;

            // Liseré gauche du commentaire
            var barGo = new GameObject("Bar", typeof(RectTransform));
            barGo.transform.SetParent(boxGo.transform, false);
            var barLe = barGo.AddComponent<LayoutElement>();
            barLe.ignoreLayout = true;
            var barRect = barGo.GetComponent<RectTransform>();
            barRect.anchorMin = new Vector2(0f, 0f);
            barRect.anchorMax = new Vector2(0f, 1f);
            barRect.pivot = new Vector2(0f, 0.5f);
            barRect.sizeDelta = new Vector2(VesselBookmarkPalette.CommentBorderThickness, 0f);
            barRect.anchoredPosition = Vector2.zero;
            var bar = barGo.AddComponent<Image>();
            bar.sprite = SpritesGlobal.FillSprite;
            bar.type = Image.Type.Simple;
            bar.color = VesselBookmarkPalette.CommentBorderColor;
            bar.raycastTarget = false;

            var boxLayout = boxGo.AddComponent<HorizontalLayoutGroup>();
            boxLayout.padding = new RectOffset(
                Mathf.RoundToInt(VesselBookmarkPalette.CommentPaddingH + VesselBookmarkPalette.CommentBorderThickness),
                Mathf.RoundToInt(VesselBookmarkPalette.CommentPaddingH),
                Mathf.RoundToInt(VesselBookmarkPalette.CommentPaddingV),
                Mathf.RoundToInt(VesselBookmarkPalette.CommentPaddingV));
            boxLayout.childControlWidth = true;
            boxLayout.childControlHeight = true;
            boxLayout.childForceExpandWidth = true;
            boxLayout.childForceExpandHeight = false;

            var textGo = new GameObject("Text", typeof(RectTransform));
            textGo.transform.SetParent(boxGo.transform, false);
            var textLe = textGo.AddComponent<LayoutElement>();
            textLe.flexibleWidth = 1f;
            var text = UGUILabels.AddLabel(textGo);
            text.text = comment;
            text.fontSize = VesselBookmarkPalette.CommentFontSize;
            text.color = VesselBookmarkPalette.CommentTextColor;
            text.alignment = TextAlignmentOptions.TopLeft;
            text.enableWordWrapping = true;
        }

        private static string BuildTitle(Bookmark bookmark, bool vesselExists)
        {
            string title = bookmark.BookmarkTitle;
            if (!vesselExists)
            {
                string key = bookmark.BookmarkType == BookmarkType.Vessel ? "VBM_labelVesselNotFound" : "VBM_labelCommandModuleNotFound";
                title += " (" + ModLocalization.GetString(key) + ")";
            }
            return title;
        }
    }
}
