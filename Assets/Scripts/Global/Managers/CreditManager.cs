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

        internal static event EventHandler? StateChanged;

        internal static int Credits => _credits;

        internal static int PlaysRemaining => _playsRemaining;

        internal static bool IsSessionActive => _isSessionActive;

        internal static bool IsFreePlay => MajEnv.Settings.Arcade.FreePlay;

        internal static int PlaysPerCredit => Math.Max(1, MajEnv.Settings.Arcade.PlaysPerCredit);

        internal static bool IsUnlimitedSessionActive => _isSessionActive && _playsRemaining >= UNLIMITED_PLAYS;

        internal static bool IsReturningFromSession => _isReturningFromSession;

        internal static bool CanEnterGame => IsFreePlay || _isSessionActive || _credits > 0;

        internal static bool IsSessionExhausted => !IsFreePlay && !_isSessionActive;

        internal static void Init()
        {
            _credits = 0;
            _playsRemaining = 0;
            _isSessionActive = false;
            _isReturningFromSession = false;
        }

        internal static void OnPreUpdate()
        {
            if (InputManager.IsButtonClickedInThisFrame(ButtonZone.Service))
            {
                InsertCredit();
            }
        }

        internal static void InsertCredit()
        {
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

        internal static bool TryConsumePlay()
        {
            if (IsFreePlay)
            {
                return true;
            }
            if (!_isSessionActive || _playsRemaining <= 0)
            {
                EndSession();
                return false;
            }
            _playsRemaining--;
            if (_playsRemaining <= 0)
            {
                EndSession();
                return true;
            }
            RaiseStateChanged();
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
