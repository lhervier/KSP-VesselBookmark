using KSP.Localization;
using KSP.UI.Screens;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using com.github.lhervier.ksp.bookmarksmod.bookmarks;
using com.github.lhervier.ksp.bookmarksmod.util;
using System;

namespace com.github.lhervier.ksp.bookmarksmod.ui {
    public class BookmarkUIController {
        private static readonly ModLogger LOGGER = new ModLogger("BookmarkUIController");

        /// <summary>
        /// Events
        /// </summary>
        public EventData<Bookmark> OnBookmarkSelected = new EventData<Bookmark>("BookmarkUI.OnBookmarkSelected");
        public EventData<Bookmark> OnBookmarkHovered = new EventData<Bookmark>("BookmarkUI.OnBookmarkHovered");
        public EventData<Bookmark> OnBookmarkRemoved = new EventData<Bookmark>("BookmarkUI.OnBookmarkRemoved");
        public EventData<Bookmark> OnBookmarkEdit = new EventData<Bookmark>("BookmarkUI.OnBookmarkEdit");
        public EventData<Bookmark> OnBookmarkSetTargetAs = new EventData<Bookmark>("BookmarkUI.OnBookmarkSetTargetAs");
        public EventData<Bookmark> OnBookmarkSwitchToVessel = new EventData<Bookmark>("BookmarkUI.OnBookmarkSwitchToVessel");
        public EventData<Bookmark> OnBookmarkMovedUp = new EventData<Bookmark>("BookmarkUI.OnBookmarkMovedUp");
        public EventData<Bookmark> OnBookmarkMovedDown = new EventData<Bookmark>("BookmarkUI.OnBookmarkMovedDown");

        private Bookmark _currentBookmark;

        private bool _isHovered = false;
        private bool _isSelected = false;
        private bool _isFirst = false;
        private bool _isLast = false;

        public void Initialize(
            Bookmark currentBookmark, 
            bool isHovered,
            bool isSelected,
            bool isFirst,
            bool isLast
        ) {
            _currentBookmark = currentBookmark;
            _isHovered = isHovered;
            _isSelected = isSelected;
            _isFirst = isFirst;
            _isLast = isLast;
        }

        public bool IsHovered() {
            return _isHovered;
        }

        public bool IsSelected() {
            return _isSelected;
        }

        public bool IsHoveredOrSelected() {
            return IsHovered() || IsSelected();
        }

        public bool IsActiveVessel() {
            bool isActiveVessel = false;
            if (FlightGlobals.ActiveVessel != null) {
                isActiveVessel = _currentBookmark.VesselPersistentID == FlightGlobals.ActiveVessel.persistentId;
            }
            return isActiveVessel;
        }

        public bool CanMoveUp() {
            return !_isFirst;
        }

        public bool CanMoveDown() {
            return !_isLast;
        }

        public bool CanSwitchToVessel() {
            if( _currentBookmark.Vessel == null ) {
                return false;
            }
            if( IsActiveVessel() ) {
                return false;
            }
            return true;
        }

        public bool IsTarget() {
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
            return targetVessel.persistentId == _currentBookmark.VesselPersistentID;
        }

        public bool CanSetTargetAs() {
            if( FlightGlobals.ActiveVessel == null ) {
                return false;
            }
            if( _currentBookmark.Vessel == null ) {
                return false;
            }
            if( IsActiveVessel() ) {
                return false;
            }
            return true;
        }
    }
}