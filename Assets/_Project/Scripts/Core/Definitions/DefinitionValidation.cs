using System;
using System.Collections.Generic;

namespace SeaLion.Core.Definitions
{
    /// <summary>Small, presentation-independent validation helpers for authored definitions.</summary>
    public static class DefinitionValidation
    {
        public static string ValidateId(StableId id, string fieldName = "id")
        {
            if (id.IsEmpty) return fieldName + " is empty.";
            return StableId.IsValid(id.Value) ? string.Empty : fieldName + " has invalid format: " + id.Value;
        }

        public static IReadOnlyList<string> ValidateUniqueIds(IEnumerable<StableId> ids, string fieldName = "id")
        {
            var errors = new List<string>();
            if (ids == null) { errors.Add(fieldName + " collection is null."); return errors; }
            var seen = new HashSet<StableId>();
            foreach (var id in ids)
            {
                var error = ValidateId(id, fieldName);
                if (error.Length > 0) errors.Add(error);
                else if (!seen.Add(id)) errors.Add(fieldName + " is duplicated: " + id.Value);
            }
            return errors;
        }

        public static IReadOnlyList<string> ValidateReferences(
            IEnumerable<StableId> references, IEnumerable<StableId> definedIds, string fieldName = "reference")
        {
            var errors = new List<string>();
            if (references == null) return errors;
            if (definedIds == null) { errors.Add("Defined ID collection is null."); return errors; }
            var definedList = new List<StableId>(definedIds);
            errors.AddRange(ValidateUniqueIds(definedList, "defined id"));
            var defined = new HashSet<StableId>(definedList);
            foreach (var id in references)
            {
                var error = ValidateId(id, fieldName);
                if (error.Length > 0) errors.Add(error);
                else if (!defined.Contains(id)) errors.Add(fieldName + " is unresolved: " + id.Value);
            }
            return errors;
        }

        public static IReadOnlyList<string> ValidatePhaseGraph(IEnumerable<PhaseLink> phaseLinks)
        {
            var errors = new List<string>();
            if (phaseLinks == null) { errors.Add("Phase graph is null."); return errors; }

            var links = new Dictionary<StableId, PhaseLink>();
            foreach (var phase in phaseLinks)
            {
                var idError = ValidateId(phase.Id, "phase id");
                if (idError.Length > 0) { errors.Add(idError); continue; }
                if (links.ContainsKey(phase.Id)) { errors.Add("phase id is duplicated: " + phase.Id); continue; }
                links.Add(phase.Id, phase);
            }

            foreach (var pair in links)
            {
                var phase = pair.Value;
                if (phase.IsTerminal)
                {
                    if (!phase.NextPhaseId.IsEmpty) errors.Add("terminal phase has a next phase: " + phase.Id);
                }
                else if (phase.NextPhaseId.IsEmpty || !links.ContainsKey(phase.NextPhaseId))
                {
                    errors.Add("next phase is unresolved for: " + phase.Id);
                }
            }

            var visitState = new Dictionary<StableId, byte>();
            foreach (var id in links.Keys) DetectCycle(id, links, visitState, errors);
            return errors;
        }

        public static IReadOnlyList<string> ValidateLoadout(
            LoadoutSnapshot loadout,
            IEnumerable<StableId> flagshipIds,
            IEnumerable<StableId> crewRoleIds,
            IEnumerable<StableId> captainAbilityIds,
            IEnumerable<StableId> ownedIds)
        {
            var errors = new List<string>();
            ValidateLoadoutSlot(errors, loadout.FlagshipId, flagshipIds, ownedIds, "flagshipId");
            ValidateLoadoutSlot(errors, loadout.CrewRoleId, crewRoleIds, ownedIds, "crewRoleId");
            ValidateLoadoutSlot(errors, loadout.CaptainAbilityId, captainAbilityIds, ownedIds, "captainAbilityId");
            return errors;
        }

        public static IReadOnlyList<string> ValidateGate(
            GateOutcome outcome, float value, StableId conversionId, GateVisualStyle visualStyle)
        {
            var errors = new List<string>();
            if (float.IsNaN(value) || float.IsInfinity(value)) errors.Add("gate value must be finite.");
            else if ((outcome == GateOutcome.Multiply || outcome == GateOutcome.Add ||
                      outcome == GateOutcome.Damage || outcome == GateOutcome.Reward) && value <= 0f)
                errors.Add(outcome + " value must be positive.");

            if (outcome == GateOutcome.Convert && !IsValid(conversionId))
                errors.Add("conversionId is required for Convert.");
            if (outcome != GateOutcome.Convert && !conversionId.IsEmpty)
                errors.Add("conversionId is only valid for Convert.");
            if ((outcome == GateOutcome.Add || outcome == GateOutcome.Multiply || outcome == GateOutcome.Reward) &&
                visualStyle == GateVisualStyle.Hostile)
                errors.Add("positive gates cannot use hostile visual style.");
            return errors;
        }

        private static void ValidateLoadoutSlot(
            List<string> errors, StableId selectedId, IEnumerable<StableId> definedIds,
            IEnumerable<StableId> ownedIds, string fieldName)
        {
            var idError = ValidateId(selectedId, fieldName);
            if (idError.Length > 0) { errors.Add(idError); return; }
            if (definedIds == null || !new HashSet<StableId>(definedIds).Contains(selectedId))
                errors.Add(fieldName + " is undefined: " + selectedId);
            if (ownedIds == null || !new HashSet<StableId>(ownedIds).Contains(selectedId))
                errors.Add(fieldName + " is not owned: " + selectedId);
        }

        private static void DetectCycle(
            StableId id, IReadOnlyDictionary<StableId, PhaseLink> links,
            IDictionary<StableId, byte> visitState, ICollection<string> errors)
        {
            byte state;
            if (visitState.TryGetValue(id, out state))
            {
                if (state == 1) errors.Add("phase cycle detected at: " + id);
                return;
            }

            visitState[id] = 1;
            PhaseLink phase;
            if (links.TryGetValue(id, out phase) && !phase.IsTerminal && links.ContainsKey(phase.NextPhaseId))
                DetectCycle(phase.NextPhaseId, links, visitState, errors);
            visitState[id] = 2;
        }

        public static bool IsValid(StableId id) => ValidateId(id).Length == 0;
        public static bool AreValid(IEnumerable<StableId> ids) => ValidateUniqueIds(ids).Count == 0;
    }
}
