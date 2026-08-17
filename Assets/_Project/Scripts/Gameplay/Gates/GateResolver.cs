using System;
using System.Collections.Generic;
using SeaLion.Core.Battle;
using SeaLion.Core.Definitions;
using SeaLion.Core.Events;

namespace SeaLion.Gameplay.Gates
{
    public readonly struct GateResolution
    {
        public readonly int Before, After, Displayed, Converted, Remainder;
        public readonly StableId ConversionId;
        public readonly bool Applied, Compressed;

        public GateResolution(int before, int after, int displayed, int converted, int remainder,
            StableId conversionId, bool applied)
        {
            Before = before; After = after; Displayed = displayed; Converted = converted;
            Remainder = remainder; ConversionId = conversionId; Applied = applied;
            Compressed = displayed != after;
        }
    }

    /// <summary>Authoritative, deterministic gate arithmetic and member commitment tracking.</summary>
    public sealed class GateResolver
    {
        private readonly int presentationCap;
        private readonly BattleSession session;
        private readonly HashSet<string> processed = new HashSet<string>(StringComparer.Ordinal);

        public GateResolver(int presentationCap, BattleSession session = null)
        {
            if (presentationCap < 1) throw new ArgumentOutOfRangeException(nameof(presentationCap));
            this.presentationCap = presentationCap;
            this.session = session;
        }

        public GateResolution Resolve(GateDefinition gate, int before, StableId memberId)
        {
            if (gate == null) throw new ArgumentNullException(nameof(gate));
            return Resolve(gate.Id, gate.Outcome, gate.Value, gate.ConversionId, before, memberId);
        }

        public GateResolution Resolve(StableId gateId, GateOutcome outcome, float value,
            StableId conversionId, int before, StableId memberId)
        {
            Validate(gateId, outcome, value, conversionId, before, memberId);
            var key = gateId.Value + "\n" + memberId.Value;
            if (!processed.Add(key)) return Result(before, before, 0, 0, conversionId, false);

            var converted = 0;
            var remainder = 0;
            int after;
            checked
            {
                switch (outcome)
                {
                    case GateOutcome.Add: after = before + Whole(value); break;
                    case GateOutcome.Multiply:
                        after = (int)Math.Round(before * (double)value, MidpointRounding.AwayFromZero); break;
                    case GateOutcome.Convert:
                        var ratio = Whole(value); converted = before / ratio; remainder = before % ratio;
                        after = converted; break;
                    case GateOutcome.Damage: after = Math.Max(0, before - Whole(value)); break;
                    case GateOutcome.Reward: after = before; break;
                    default: throw new ArgumentOutOfRangeException(nameof(outcome));
                }
            }

            var result = Result(before, after, converted, remainder, conversionId, true);
            Publish(gateId, outcome, value, result);
            return result;
        }

        private GateResolution Result(int before, int after, int converted, int remainder,
            StableId conversionId, bool applied)
        {
            return new GateResolution(before, after, Math.Min(after, presentationCap), converted,
                remainder, conversionId, applied);
        }

        private void Publish(StableId gateId, GateOutcome outcome, float value, GateResolution result)
        {
            if (session == null) return;
            session.TryPublishGameplayEvent(BattleEventType.GateResolved,
                new BattleEventPayload(session.SessionId, gateId, result.ConversionId,
                    Allegiance.Friendly, result.Before, result.After, Whole(value), outcome, default));
        }

        private static int Whole(float value) =>
            (int)Math.Round(value, MidpointRounding.AwayFromZero);

        private static void Validate(StableId gateId, GateOutcome outcome, float value,
            StableId conversionId, int before, StableId memberId)
        {
            if (gateId.IsEmpty) throw new ArgumentException("Gate ID is required.", nameof(gateId));
            if (memberId.IsEmpty) throw new ArgumentException("Member ID is required.", nameof(memberId));
            if (before < 0) throw new ArgumentOutOfRangeException(nameof(before));
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
                throw new ArgumentOutOfRangeException(nameof(value));
            if (outcome == GateOutcome.Convert && (conversionId.IsEmpty || Whole(value) < 1))
                throw new ArgumentException("Convert requires a target and a positive whole ratio.");
        }
    }
}
