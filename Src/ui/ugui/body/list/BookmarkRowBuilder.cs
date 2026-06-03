using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using com.github.lhervier.ksp.bookmarksmod.bookmarks;
using com.github.lhervier.ksp.bookmarksmod.ui.styles;
using com.github.lhervier.ksp.bookmarksmod.ui.ugui.sprites;
using com.github.lhervier.ksp.bookmarksmod;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.body.list
{
    /// <summary>
    /// Une ligne de bookmark : ligne 1 (icône type + alarme + titre + pastille d'état + boutons ▲▼✕),
    /// ligne 2 (situation + nom de vaisseau), ligne 3 (commentaire). Le fond, le liseré gauche, la
    /// couleur du titre, la pastille et la visibilité des boutons dépendent de l'état (sélection,
    /// survol, vaisseau actif, cible) et sont réévalués par Refresh().
    /// </summary>
    public class BookmarkRowBuilder
    {
        private const string MoveUpGlyph = "▲";
        private const string MoveDownGlyph = "▼";
        private const string RemoveGlyph = "✕";

        private readonly BookmarksViewModel _viewModel;
        private readonly ButtonBuilder _buttonBuilder;

        public BookmarkRowBuilder(BookmarksViewModel viewModel)
        {
            this._viewModel = viewModel;
            this._buttonBuilder = new ButtonBuilder(viewModel);
        }

        public BookmarkRowController Create(Bookmark bookmark, bool isFirst, bool isLast)
        {
            var rowGo = new GameObject("Row", typeof(RectTransform));
            BookmarkRowController controller = rowGo.AddComponent<BookmarkRowController>();
            controller.Initialize(_viewModel);

            // Fond (teinté selon l'état)
            var bg = rowGo.AddComponent<Image>();
            bg.sprite = Sprites.Fill;
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
            accentBar.sprite = Sprites.Fill;
            accentBar.type = Image.Type.Simple;
            accentBar.color = VesselBookmarkPalette.AccentColor;
            accentBar.raycastTarget = false;
            accentBar.enabled = false;

            bool vesselExists = bookmark.Vessel != null;

            // ---- Ligne 1 : icône type + alarme + titre + pastille + boutons ----
            var line1 = NewHLine(rowGo.transform, "Line1", 0);
            var line1Layout = line1.GetComponent<HorizontalLayoutGroup>();
            line1Layout.spacing = VesselBookmarkPalette.RowSpacing;
            line1Layout.childAlignment = TextAnchor.MiddleLeft;

            BuildTypeIcon(line1.transform, bookmark.BookmarkVesselType);
            if (bookmark.HasAlarm)
            {
                BuildAlarmIcon(line1.transform);
            }

            // Titre
            var nameGo = new GameObject("Name", typeof(RectTransform));
            nameGo.transform.SetParent(line1.transform, false);
            var nameLe = nameGo.AddComponent<LayoutElement>();
            nameLe.flexibleWidth = 1f;
            var name = nameGo.AddComponent<Text>();
            name.text = BuildTitle(bookmark, vesselExists);
            name.font = HighLogic.UISkin.font;
            name.fontSize = VesselBookmarkPalette.NameFontSize;
            name.fontStyle = vesselExists ? FontStyle.Normal : FontStyle.Italic;
            name.color = VesselBookmarkPalette.NameColor;
            name.alignment = TextAnchor.MiddleLeft;
            name.horizontalOverflow = HorizontalWrapMode.Wrap;
            name.verticalOverflow = VerticalWrapMode.Overflow;
            name.raycastTarget = false;

            // Pastille d'état (Actif / Cible / Disparu) — créée, affichée selon l'état dans Refresh()
            Image chipImage;
            Text chipText;
            GameObject chip = BuildChip(line1.transform, out chipImage, out chipText);

            // Boutons ▲ ▼ ✕ (révélés au survol / sélection)
            CanvasGroup rowButtonsGroup = BuildRowButtons(line1.transform, bookmark, isFirst, isLast);

            // ---- Ligne 2 : situation (+ nom de vaisseau) ----
            var line2 = NewHLine(rowGo.transform, "Line2", Mathf.RoundToInt(VesselBookmarkPalette.TypeIconSize + VesselBookmarkPalette.RowSpacing));
            line2.GetComponent<HorizontalLayoutGroup>().spacing = VesselBookmarkPalette.DefaultSpacing;

            var situationGo = new GameObject("Situation", typeof(RectTransform));
            situationGo.transform.SetParent(line2.transform, false);
            var situation = situationGo.AddComponent<Text>();
            situation.text = bookmark.VesselSituationLabel;
            situation.font = HighLogic.UISkin.font;
            situation.fontSize = VesselBookmarkPalette.SituationFontSize;
            situation.color = VesselBookmarkPalette.SituationColor;
            situation.alignment = TextAnchor.MiddleLeft;
            situation.horizontalOverflow = HorizontalWrapMode.Overflow;
            situation.verticalOverflow = VerticalWrapMode.Overflow;
            situation.raycastTarget = false;

            if (bookmark is CommandModuleBookmark cmb && cmb.VesselName != cmb.CommandModuleName)
            {
                var vnameGo = new GameObject("VesselName", typeof(RectTransform));
                vnameGo.transform.SetParent(line2.transform, false);
                var vname = vnameGo.AddComponent<Text>();
                vname.text = "(" + bookmark.VesselName + ")";
                vname.font = HighLogic.UISkin.font;
                vname.fontSize = VesselBookmarkPalette.SituationFontSize;
                vname.color = VesselBookmarkPalette.VesselNameColor;
                vname.alignment = TextAnchor.MiddleLeft;
                vname.horizontalOverflow = HorizontalWrapMode.Overflow;
                vname.verticalOverflow = VerticalWrapMode.Overflow;
                vname.raycastTarget = false;
            }

            // ---- Ligne 3 : commentaire (si présent) ----
            if (!string.IsNullOrEmpty(bookmark.Comment))
            {
                BuildComment(rowGo.transform, bookmark.Comment);
            }

            // Survol + clic sur la ligne (les boutons enfants consomment leurs propres clics)
            AddTrigger(rowGo, EventTriggerType.PointerEnter, _ => _viewModel.HoveredBookmark = bookmark);
            AddTrigger(rowGo, EventTriggerType.PointerExit, _ => {
                if (_viewModel.HoveredBookmark == bookmark) _viewModel.HoveredBookmark = null;
            });
            AddTrigger(rowGo, EventTriggerType.PointerClick, _ => _viewModel.SelectedBookmark = bookmark);

            controller.Bind(bookmark, bg, accentBar, name, chip, chipImage, chipText, rowButtonsGroup, vesselExists);
            return controller;
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
            box.sprite = Sprites.Border(
                VesselBookmarkPalette.TypeIconBgColor,
                VesselBookmarkPalette.TypeIconBorderColor,
                VesselBookmarkPalette.TypeIconBorderThickness);
            box.type = Image.Type.Sliced;
            box.color = Color.white;
            box.raycastTarget = false;

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
                img.sprite = Sprites.Fill;
                img.color = VesselBookmarkPalette.WarmColor;
            }
            img.type = Image.Type.Simple;
            img.raycastTarget = false;
        }

        private GameObject BuildChip(Transform parent, out Image chipImage, out Text chipText)
        {
            var chipGo = new GameObject("Chip", typeof(RectTransform));
            chipGo.transform.SetParent(parent, false);

            chipImage = chipGo.AddComponent<Image>();
            chipImage.sprite = Sprites.Border(VesselBookmarkPalette.AccentBgColor, VesselBookmarkPalette.AccentBorderColor, VesselBookmarkPalette.ChipBorderThickness);
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
            chipText = labelGo.AddComponent<Text>();
            chipText.font = HighLogic.UISkin.font;
            chipText.fontSize = VesselBookmarkPalette.ChipFontSize;
            chipText.color = VesselBookmarkPalette.AccentColor;
            chipText.alignment = TextAnchor.MiddleCenter;
            chipText.horizontalOverflow = HorizontalWrapMode.Overflow;
            chipText.verticalOverflow = VerticalWrapMode.Overflow;
            chipText.raycastTarget = false;

            chipGo.SetActive(false);
            return chipGo;
        }

        private CanvasGroup BuildRowButtons(Transform parent, Bookmark bookmark, bool isFirst, bool isLast)
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

            ButtonController up = _buttonBuilder.Create(
                "MoveUp", MoveUpGlyph, () => _viewModel.MoveUp(bookmark), !isFirst,
                VesselBookmarkPalette.RowButtonBgColor, VesselBookmarkPalette.RowButtonHoverColor,
                VesselBookmarkPalette.RowButtonSize, VesselBookmarkPalette.RowButtonFontSize);
            up.transform.SetParent(groupGo.transform, false);

            ButtonController down = _buttonBuilder.Create(
                "MoveDown", MoveDownGlyph, () => _viewModel.MoveDown(bookmark), !isLast,
                VesselBookmarkPalette.RowButtonBgColor, VesselBookmarkPalette.RowButtonHoverColor,
                VesselBookmarkPalette.RowButtonSize, VesselBookmarkPalette.RowButtonFontSize);
            down.transform.SetParent(groupGo.transform, false);

            ButtonController remove = _buttonBuilder.Create(
                "Remove", RemoveGlyph, () => _viewModel.RequestRemoval(bookmark), true,
                VesselBookmarkPalette.RowButtonBgColor, VesselBookmarkPalette.RowButtonDangerHoverColor,
                VesselBookmarkPalette.RowButtonSize, VesselBookmarkPalette.RowButtonFontSize);
            remove.transform.SetParent(groupGo.transform, false);

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
            boxBg.sprite = Sprites.Fill;
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
            bar.sprite = Sprites.Fill;
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
            var text = textGo.AddComponent<Text>();
            text.text = comment;
            text.font = HighLogic.UISkin.font;
            text.fontSize = VesselBookmarkPalette.CommentFontSize;
            text.color = VesselBookmarkPalette.CommentTextColor;
            text.alignment = TextAnchor.UpperLeft;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
        }

        private static void AddTrigger(GameObject go, EventTriggerType type, UnityEngine.Events.UnityAction<BaseEventData> cb)
        {
            var trigger = go.GetComponent<EventTrigger>();
            if (trigger == null) trigger = go.AddComponent<EventTrigger>();
            var entry = new EventTrigger.Entry { eventID = type };
            entry.callback.AddListener(cb);
            trigger.triggers.Add(entry);
        }

        private static string BuildTitle(Bookmark bookmark, bool vesselExists)
        {
            string title = bookmark.BookmarkTitle;
            if (!vesselExists)
            {
                string key = bookmark.BookmarkType == BookmarkType.Vessel ? "labelVesselNotFound" : "labelCommandModuleNotFound";
                title += " (" + ModLocalization.GetString(key) + ")";
            }
            return title;
        }

        // ================================================================

        public class BookmarkRowController : BaseController
        {
            private Bookmark _bookmark;
            private Image _bg;
            private Image _accentBar;
            private Text _name;
            private GameObject _chip;
            private Image _chipImage;
            private Text _chipText;
            private CanvasGroup _rowButtons;
            private bool _vesselExists;

            public Bookmark Bookmark => _bookmark;

            public void Bind(
                Bookmark bookmark, Image bg, Image accentBar, Text name,
                GameObject chip, Image chipImage, Text chipText,
                CanvasGroup rowButtons, bool vesselExists)
            {
                this._bookmark = bookmark;
                this._bg = bg;
                this._accentBar = accentBar;
                this._name = name;
                this._chip = chip;
                this._chipImage = chipImage;
                this._chipText = chipText;
                this._rowButtons = rowButtons;
                this._vesselExists = vesselExists;
            }

            public void Start()
            {
                Refresh();
            }

            /// <summary>Réévalue le rendu de la ligne à partir de l'état courant du ViewModel.</summary>
            public void Refresh()
            {
                bool selected = ViewModel.IsSelected(_bookmark);
                bool hovered = ViewModel.IsHovered(_bookmark);
                bool active = _vesselExists && ViewModel.IsCurrentVessel(_bookmark);
                bool target = _vesselExists && ViewModel.IsTarget(_bookmark);

                // Fond
                if (active) _bg.color = VesselBookmarkPalette.RowActiveBgColor;
                else if (selected) _bg.color = VesselBookmarkPalette.RowSelectedBgColor;
                else if (hovered) _bg.color = VesselBookmarkPalette.RowHoverColor;
                else _bg.color = Color.clear;

                // Liseré gauche
                _accentBar.enabled = active || selected;
                _accentBar.color = active ? VesselBookmarkPalette.AccentColor : VesselBookmarkPalette.AccentBorderColor;

                // Couleur du titre
                if (!_vesselExists) _name.color = VesselBookmarkPalette.NameMissingColor;
                else if (active) _name.color = VesselBookmarkPalette.NameActiveColor;
                else if (target) _name.color = VesselBookmarkPalette.NameTargetColor;
                else _name.color = VesselBookmarkPalette.NameColor;

                // Pastille d'état
                if (!_vesselExists) SetChip(true, "chipMissing",
                    VesselBookmarkPalette.ChipMissingTextColor, VesselBookmarkPalette.ChipMissingBgColor, VesselBookmarkPalette.ChipMissingBorderColor);
                else if (active) SetChip(true, "chipActive",
                    VesselBookmarkPalette.AccentColor, VesselBookmarkPalette.AccentBgColor, VesselBookmarkPalette.AccentBorderColor);
                else if (target) SetChip(true, "chipTarget",
                    VesselBookmarkPalette.AccentColor, VesselBookmarkPalette.AccentBgColor, VesselBookmarkPalette.AccentBorderColor);
                else _chip.SetActive(false);

                // Boutons d'ordre/suppression
                bool showButtons = hovered || selected || active;
                _rowButtons.alpha = showButtons ? 1f : 0f;
                _rowButtons.blocksRaycasts = showButtons;
                _rowButtons.interactable = showButtons;
            }

            private void SetChip(bool show, string locKey, Color text, Color bg, Color border)
            {
                if (!show) { _chip.SetActive(false); return; }
                _chip.SetActive(true);
                _chipText.text = ModLocalization.GetString(locKey);
                _chipText.color = text;
                _chipImage.sprite = Sprites.Border(bg, border, VesselBookmarkPalette.ChipBorderThickness);
            }
        }
    }
}
