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
using System;
using System.CodeDom;
using com.github.lhervier.ksp.shared.ugui.styles;

namespace com.github.lhervier.ksp.bookmarksmod.ui.ugui.body.list
{
    public class BookmarkRowController : MonoBehaviour
    {
        // ============================================
        // Life Cycle
        // ============================================

        private BookmarksViewModel _viewModel;
        public BookmarkRowController WithViewModel(BookmarksViewModel viewModel)
        {
            this._viewModel = viewModel;
            return this;
        }

        private Bookmark _bookmark;
        public BookmarkRowController WithBookmark(Bookmark bookmark)
        {
            this._bookmark = bookmark;
            return this;
        }

        private Image _bg;
        public BookmarkRowController WithBackground(Image bg)
        {
            this._bg = bg;
            return this;
        }

        private Image _accentBar;
        public BookmarkRowController WithAccentBar(Image accentBar)
        {
            this._accentBar = accentBar;
            return this;
        }

        private TextMeshProUGUI _name;
        public BookmarkRowController WithNameComponent(TextMeshProUGUI name)
        {
            this._name = name;
            return this;
        }

        private GameObject _chip;
        public BookmarkRowController WithChip(GameObject chip)
        {
            this._chip = chip;
            return this;
        }

        private Image _chipImage;
        public BookmarkRowController WithChipImage(Image chipImage)
        {
            this._chipImage = chipImage;
            return this;
        }

        private TextMeshProUGUI _chipText;
        public BookmarkRowController WithChipTextComponent(TextMeshProUGUI chipText)
        {
            this._chipText = chipText;
            return this;
        }

        private CanvasGroup _rowButtons;
        public BookmarkRowController WithRowButtons(CanvasGroup rowButtons)
        {
            this._rowButtons = rowButtons;
            return this;
        }

        private bool _vesselExists;
        public BookmarkRowController WithVesselExists(bool vesselExists)
        {
            this._vesselExists = vesselExists;
            return this;
        }

        private PointerHandler _pointerHandler;
        public BookmarkRowController WithPointerHandler(PointerHandler pointerHandler)
        {
            this._pointerHandler = pointerHandler;
            return this;
        }

        private ButtonController _upButtonController;
        public BookmarkRowController WithUpButtonController(ButtonController upButtonController)
        {
            _upButtonController = upButtonController;
            return this;
        }

        private ButtonController _downButtonController;
        public BookmarkRowController WithDownButtonController(ButtonController downButtonController)
        {
            _downButtonController = downButtonController;
            return this;
        }

        private ButtonController _removeButtonController;
        public BookmarkRowController WithRemoveButtonController(ButtonController removeButtonController)
        {
            _removeButtonController = removeButtonController;
            return this;
        }

        public void Start()
        {
            Refresh();

            if( _pointerHandler != null )
            {
                _pointerHandler.OnEnter = RowEntered;
                _pointerHandler.OnExit = RowExited;
                _pointerHandler.OnClick = RowClicked;
            }

            if( _upButtonController != null )
            {
                _upButtonController.OnClick.Add(OnUpButton);
            }
            if( _downButtonController != null )
            {
                _downButtonController.OnClick.Add(OnDownButton);
            }
            if( _removeButtonController != null )
            {
                _removeButtonController.OnClick.Add(OnRemoveButton);
            }
        }

        public void OnDestroy()
        {
            if( _upButtonController != null )
            {
                _upButtonController.OnClick.Remove(OnUpButton);
            }
            if( _downButtonController != null )
            {
                _downButtonController.OnClick.Remove(OnDownButton);
            }
            if( _removeButtonController != null )
            {
                _removeButtonController.OnClick.Remove(OnRemoveButton);
            }
            if( _pointerHandler != null )
            {
                _pointerHandler.OnEnter = null;
                _pointerHandler.OnExit = null;
                _pointerHandler.OnClick = null;
            }
        }

        
        // =============================================
        // Methods bound to events
        // =============================================

        private void RowEntered()
        {
            _viewModel.HoveredBookmark = _bookmark;
        }

        private void RowExited()
        {
            if (_viewModel.HoveredBookmark == _bookmark) 
            {
                _viewModel.HoveredBookmark = null;
            }
        }

        private void RowClicked()
        {
            _viewModel.SelectedBookmark = _bookmark;
        }

        private void OnUpButton()
        {
            _viewModel.MoveUp(_bookmark);
        }

        private void OnDownButton()
        {
            _viewModel.MoveDown(_bookmark);
        }

        private void OnRemoveButton()
        {
            _viewModel.RequestRemoval(_bookmark);
        }

        // =====================================
        // Public API
        // =====================================

        public Bookmark GetBookmark()
        {
            return this._bookmark;
        }

        /// <summary>Réévalue le rendu de la ligne à partir de l'état courant du ViewModel.</summary>
        public void Refresh()
        {
            bool selected = _viewModel.IsSelected(_bookmark);
            bool hovered = _viewModel.IsHovered(_bookmark);
            bool active = _vesselExists && _viewModel.IsCurrentVessel(_bookmark);
            bool target = _vesselExists && _viewModel.IsTarget(_bookmark);

            // Fond
            if (active) _bg.color = VesselBookmarkPalette.RowActiveBgColor;
            else if (selected) _bg.color = VesselBookmarkPalette.RowSelectedBgColor;
            else if (hovered) _bg.color = VesselBookmarkPalette.RowHoverColor;
            else _bg.color = Color.clear;

            // Liseré gauche
            _accentBar.enabled = active || selected;
            _accentBar.color = active ? DefaultPalette.AccentColor : DefaultPalette.AccentBorderColor;

            // Couleur du titre
            if (!_vesselExists) _name.color = VesselBookmarkPalette.NameMissingColor;
            else if (active) _name.color = VesselBookmarkPalette.NameActiveColor;
            else if (target) _name.color = VesselBookmarkPalette.NameTargetColor;
            else _name.color = VesselBookmarkPalette.NameColor;

            // Pastille d'état
            if (!_vesselExists) SetChip(true, "VBM_chipMissing",
                VesselBookmarkPalette.ChipMissingTextColor, VesselBookmarkPalette.ChipMissingBgColor, VesselBookmarkPalette.ChipMissingBorderColor);
            else if (active) SetChip(true, "VBM_chipActive",
                DefaultPalette.AccentColor, DefaultPalette.AccentBgColor, DefaultPalette.AccentBorderColor);
            else if (target) SetChip(true, "VBM_chipTarget",
                DefaultPalette.AccentColor, DefaultPalette.AccentBgColor, DefaultPalette.AccentBorderColor);
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
            _chipImage.sprite = SpritesGlobal.Border(bg, border, VesselBookmarkPalette.ChipBorderThickness);
        }
    }
}
