using System;
using System.Diagnostics;
using SeaLion.Core.Battle;
using SeaLion.Core.Events;

namespace SeaLion.Gameplay.Results
{
    /// <summary>
    /// Owns the terminal boundary for one battle session and creates an isolated session on retry.
    /// The controller deliberately does not make presentation completion part of the retry path.
    /// </summary>
    public sealed class BattleResultController : IDisposable
    {
        public const double DefaultRetryBudgetSeconds = 3d;

        private readonly Func<BattleSession> createSession;
        private readonly Action clearRuntime;
        private readonly double retryBudgetSeconds;
        private IDisposable subscription;
        private bool terminalConsumed;
        private bool disposed;
        private BattleResult? terminalResult;

        public BattleSession CurrentSession { get; private set; }
        public BattleResult? TerminalResult => terminalResult;
        public bool HasTerminalResult => terminalConsumed;
        public int TerminalEventCount { get; private set; }
        public int RetryCount { get; private set; }
        public double LastRetryDurationSeconds { get; private set; }
        public bool LastRetryWithinBudget { get; private set; }
        public double RetryBudgetSeconds => retryBudgetSeconds;

        public event Action<BattleResult> TerminalResultReceived;
        public event Action<BattleSession> SessionRetried;

        public BattleResultController(BattleSession session, Func<BattleSession> createSession,
            Action clearRuntime = null, double retryBudgetSeconds = DefaultRetryBudgetSeconds)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            if (createSession == null) throw new ArgumentNullException(nameof(createSession));
            if (double.IsNaN(retryBudgetSeconds) || double.IsInfinity(retryBudgetSeconds) || retryBudgetSeconds <= 0d)
                throw new ArgumentOutOfRangeException(nameof(retryBudgetSeconds));

            CurrentSession = session;
            this.createSession = createSession;
            this.clearRuntime = clearRuntime;
            this.retryBudgetSeconds = retryBudgetSeconds;
            Subscribe(session);
        }

        /// <summary>Requests a retry only after an authoritative terminal result.</summary>
        public bool TryRetry(out BattleSession retriedSession)
        {
            retriedSession = null;
            if (disposed || !terminalConsumed) return false;

            var start = Stopwatch.GetTimestamp();
            var previous = CurrentSession;
            Unsubscribe();
            clearRuntime?.Invoke();

            BattleSession next;
            try { next = createSession(); }
            catch
            {
                CurrentSession = previous;
                Subscribe(previous);
                LastRetryDurationSeconds = ElapsedSeconds(start);
                LastRetryWithinBudget = false;
                return false;
            }

            if (next == null)
            {
                CurrentSession = previous;
                Subscribe(previous);
                LastRetryDurationSeconds = ElapsedSeconds(start);
                LastRetryWithinBudget = false;
                return false;
            }

            CurrentSession = next;
            terminalConsumed = false;
            terminalResult = null;
            TerminalEventCount = 0;
            RetryCount++;
            Subscribe(next);
            LastRetryDurationSeconds = ElapsedSeconds(start);
            LastRetryWithinBudget = LastRetryDurationSeconds < retryBudgetSeconds;
            retriedSession = next;
            SessionRetried?.Invoke(next);
            return LastRetryWithinBudget;
        }

        public bool TryRetry() { return TryRetry(out _); }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            Unsubscribe();
            TerminalResultReceived = null;
            SessionRetried = null;
        }

        private void Subscribe(BattleSession session) { subscription = session.Events.Subscribe(HandleEvent); }

        private void Unsubscribe()
        {
            subscription?.Dispose();
            subscription = null;
        }

        private void HandleEvent(BattleEvent battleEvent)
        {
            if (disposed || terminalConsumed || battleEvent.Type != BattleEventType.BattleEnded ||
                battleEvent.Payload.SessionId != CurrentSession.SessionId) return;

            terminalConsumed = true;
            terminalResult = battleEvent.Payload.Result;
            TerminalEventCount++;
            TerminalResultReceived?.Invoke(terminalResult.Value);
        }

        private static double ElapsedSeconds(long start)
        { return (Stopwatch.GetTimestamp() - start) / (double)Stopwatch.Frequency; }
    }
}
