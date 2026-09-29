using MajdataPlay.IO;
using System;

#nullable enable
namespace MajdataPlay
{
    internal static class CreditManager
    {
        internal const int UNLIMITED_PLAYS = int.MaxValue;

        static int _credits;
        static int _playsRemaining;
        static bool _isSessionActive;
        static bool _isReturningFromSession;
        static bool _wasFreePlay;

        internal static event EventHandler? StateChanged;

        internal static int Credits => _credits;

        internal static int PlaysRemaining => _playsRemaining;

        internal static bool IsSessionActive => _isSessionActive;

        internal static bool IsFreePlay => MajEnv.Settings.Arcade.FreePlay;

        internal static int PlaysPerCredit => Math.Max(1, MajEnv.Settings.Arcade.PlaysPerCredit);

        internal static bool IsUnlimitedSessionActive => _isSessionActive && _playsRemaining >= UNLIMITED_PLAYS;

        internal static int CurrentTrack
        {
            get
            {
                if (!_isSessionActive || _playsRemaining >= UNLIMITED_PLAYS)
                {
                    return 0;
                }
                return PlaysPerCredit - _playsRemaining + 1;
            }
        }

        internal static bool IsReturningFromSession => _isReturningFromSession;

        internal static bool CanStartTrack => IsFreePlay || _isSessionActive;

        /// <summary>
        /// Consumes the track the player just finished or abandoned.
        /// Returns true when the session continues and the player may return to song select,
        /// false when the session is over and the player must be returned to the title screen.
        /// </summary>
        internal static bool ConsumeTrack()
        {
            if (IsFreePlay)
            {
                return true;
            }
            if (!_isSessionActive)
            {
                return false;
            }
            if (_playsRemaining >= UNLIMITED_PLAYS)
            {
                EndSession();
                return false;
            }
            _playsRemaining--;
            if (_playsRemaining <= 0)
            {
                EndSession();
                return false;
            }
            RaiseStateChanged();
            return true;
        }

        internal static void Init()
        {
            _credits = 0;
            _playsRemaining = 0;
            _isSessionActive = false;
            _isReturningFromSession = false;
            _wasFreePlay = IsFreePlay;
        }

        internal static void OnPreUpdate()
        {
            var freePlay = IsFreePlay;
            if (freePlay && !_wasFreePlay)
            {
                _credits = 0;
                _playsRemaining = 0;
                _isSessionActive = false;
                RaiseStateChanged();
            }
            _wasFreePlay = freePlay;

            if (!freePlay && InputManager.IsButtonClickedInThisFrame(ButtonZone.Service))
            {
                InsertCredit();
            }
        }

        internal static void InsertCredit()
        {
            if (IsFreePlay)
            {
                return;
            }
            _credits++;
            RaiseStateChanged();
        }

        internal static bool TryRedeemSession(bool isUnlimitedSession = false)
        {
            if (IsFreePlay)
            {
                BeginSession(isUnlimitedSession);
                return true;
            }
            if (_credits <= 0)
            {
                return false;
            }
            _credits--;
            BeginSession(isUnlimitedSession);
            return true;
        }

        internal static void EndSession()
        {
            _isSessionActive = false;
            _playsRemaining = 0;
            RaiseStateChanged();
        }

        internal static void MarkReturningFromSession() => _isReturningFromSession = true;

        internal static void ConsumeReturningFromSession() => _isReturningFromSession = false;

        static void BeginSession(bool isUnlimitedSession)
        {
            _isSessionActive = true;
            _playsRemaining = isUnlimitedSession ? UNLIMITED_PLAYS : PlaysPerCredit;
            RaiseStateChanged();
        }

        static void RaiseStateChanged() => StateChanged?.Invoke(null, EventArgs.Empty);
    }
}
