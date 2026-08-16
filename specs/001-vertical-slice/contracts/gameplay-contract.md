# Gameplay Contract

This contract fixes the observable behavior between the deterministic battle domain and
its input, presentation, UI, save, and test adapters. Concrete C# interfaces may refine the
names but cannot weaken the invariants.

## Input contract

### Flagship movement

- Input is one normalized horizontal intent in the range `[-1, 1]`.
- The battle domain clamps the flagship to the active level's control bounds.
- Losing touch focus returns intent to zero; it does not teleport or continue stale input.
- Input sampling frequency cannot change gate arithmetic or combat results.

### Captain ability

- Activation is accepted only while the battle is Active or Assault, the ability is ready,
  and no terminal result exists.
- A rejected activation emits a reason for UI feedback and changes no gameplay state.

## Fixed-step battle contract

- Authoritative movement, gates, landings, combat, boss phases, and victory/failure are
  evaluated on a fixed simulation step.
- Presentation may interpolate between steps but cannot write authoritative state.
- A deterministic seed plus definitions and input recording must reproduce gate results,
  reward eligibility, and terminal outcome.
- Ordinary-agent simulation may run in parallel only when write ownership and job ordering
  are explicit.

## Gate contract

For each eligible force member crossing a gate commitment volume:

1. Validate the member has not processed this gate.
2. Resolve the outcome from the immutable gate definition.
3. Apply arithmetic to authoritative logical force state exactly once.
4. Mark the member/gate pair processed.
5. Emit one `GateResolved` event containing the before count, after count, outcome, and ID.

### Required rules

- Multiply uses whole-number output defined by level tuning; rounding mode is explicit.
- Add cannot reduce a force.
- Damage cannot increase a force or reduce the count below zero.
- Convert preserves the declared conversion ratio and reports unconverted remainder.
- Visual agent compression at the presentation cap never changes logical count.

## Landing contract

- A landing zone accepts only surviving eligible landing craft.
- Each craft transfers its declared contribution once, then leaves sea simulation.
- `LandingStarted` fires before the first transfer; `LandingCompleted` fires after all
  accepted craft have transferred or been destroyed.
- The land force's initial role counts equal transferred contribution after conversion.
- Camera and VFX timing cannot delay the authoritative phase transition.

## Combat contract

- Target selection uses allegiance, range, target rules, and a stable tie-break order.
- A hit applies at most once and records source, target, amount, and simulation step.
- Dead ordinary agents cannot attack, process gates, transfer at landing, or receive healing.
- Boss damage cannot skip required phases; excess damage carries only when the boss
  definition explicitly permits it.
- Reduced-effects mode emits fewer presentation effects but preserves all events and values.

## Battle lifecycle events

| Event | Required payload | Emitted when |
|------|------------------|--------------|
| BattleReady | session ID, level ID, loadout | Scene and definitions are valid |
| BattleStarted | session ID, seed | Player gains control |
| ForceChanged | allegiance, before, after, cause ID | Logical force changes |
| GateResolved | gate ID, outcome, before, after | Gate commits successfully |
| LandingStarted | zone ID, incoming force | First craft commits |
| LandingCompleted | zone ID, resulting roles | Transfer finishes |
| BossPhaseChanged | boss ID, old phase, new phase | Threshold resolves |
| AbilityActivated | ability ID, charge, affected targets | Accepted activation resolves |
| BattleEnded | result snapshot | Victory or failure becomes authoritative |
| RewardGranted | transaction ID, reward ID, target | Save commit succeeds |

### Event rules

- Sequence numbers increase strictly within a battle session.
- `BattleEnded` occurs exactly once.
- No gameplay-changing event occurs after `BattleEnded`.
- Subscribers may be absent; gameplay must still complete.
- A presentation subscriber failure is logged and isolated from battle state.

## Retry contract

- Retry disposes or clears old runtime buffers and creates a new session.
- Immutable definitions and pooled presentation objects may be reused.
- No enemy, gate-processed flag, boss phase, queued hit, or ability charge leaks across retry.
- Meaningful player control returns within three seconds on a reference device.

## Persistence contract

- Save data contains stable IDs and player state, never scene object references.
- Writes use a temporary file, validation, and atomic replacement.
- Load validates schema, IDs, ownership, and numeric ranges before use.
- A failed migration preserves the previous valid save and starts a recoverable default
  session only after recording the failure.
- Reward grants are idempotent by transaction and reward ID.

## Independent test fixtures

- Level 1 direct launch with default loadout and fixed seed.
- Gate arithmetic table for Add, Multiply, Convert, and Damage.
- Landing transfer with destroyed and surviving craft.
- Boss phase threshold with excess damage.
- Reduced-effects outcome parity against Primary quality.
- Retry state isolation.
- Interrupted reward persistence and replay.
