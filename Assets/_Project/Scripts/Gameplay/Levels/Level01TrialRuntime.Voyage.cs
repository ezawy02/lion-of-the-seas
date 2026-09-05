using System;
using System.Collections.Generic;
using SeaLion.Core.Definitions;
using SeaLion.Gameplay.Deployment;
using UnityEngine;

namespace SeaLion.Gameplay.Levels
{
    public sealed partial class Level01TrialRuntime
    {
        private readonly List<VoyageCraft> fleet = new List<VoyageCraft>();
        private float routeProgress;
        private int craftSequence;
        private bool rescueCollected;
        public float RouteProgress => routeProgress;
        public bool RescueCollected => rescueCollected;
        public int SurvivingCraftCount { get { var n = 0; foreach (var c in fleet) if (c.Contribution > 0) n++; return n; } }
        private float RouteSpeed => levelDefinition != null ? levelDefinition.RouteSpeed : .1f;
        private float GateProgress => levelDefinition != null ? levelDefinition.GateProgress : .4f;
        private float RescueProgress => levelDefinition != null ? levelDefinition.RescueProgress : .7f;
        private float GateWidth => levelDefinition != null ? levelDefinition.GateHalfWidth : .72f;
        private float EasyLane => (levelDefinition != null ? levelDefinition.EasyGatePosition : .36f) * 2f - 1f;
        private float RiskyLane => (levelDefinition != null ? levelDefinition.RiskyGatePosition : .64f) * 2f - 1f;
        private float LandingDuration => levelDefinition != null ? levelDefinition.LandingTransferSeconds : 9f;

        private void ResetVoyage()
        {
            fleet.Clear(); craftSequence = 0; routeProgress = 0f; rescueCollected = false;
            var count = Mathf.Min(LandingCraftCount, initialForce);
            for (var i = 0; i < count; i++)
                AddCraft(initialForce / count + (i < initialForce % count ? 1 : 0));
        }

        private VoyageCraft AddCraft(int contribution)
        {
            var craft = new VoyageCraft(craftSequence++, contribution, routeProgress);
            craft.GateProcessed = routeProgress >= GateProgress;
            fleet.Add(craft);
            return craft;
        }

        private void AdvanceVoyage(float step)
        {
            if (!traversalPlayerSteered || !StepCampaignVoyage(step)) return;
            var next = Mathf.Min(1f, routeProgress + RouteSpeed * step);
            // Hold at the commitment boundary until the flagship is inside a readable lane.
            if (LevelNumber != 2 && !gateCommitted && next >= GateProgress &&
                (Mathf.Abs(horizontalChoice) < Mathf.Min(Mathf.Abs(EasyLane), Mathf.Abs(RiskyLane)) * .6f ||
                (Mathf.Abs(horizontalChoice - EasyLane) > GateWidth &&
                Mathf.Abs(horizontalChoice - RiskyLane) > GateWidth))) return;
            traversalActiveElapsed += step;
            if (routeProgress < GateProgress) deployer.Tick(step);
            var selected = SelectedGate();
            foreach (var craft in fleet)
            {
                if (craft.Contribution <= 0) continue;
                var beforeProgress = craft.Progress;
                craft.Progress = next;
                if (!craft.GateProcessed && beforeProgress < GateProgress && next >= GateProgress)
                {
                    var result = gateResolver.Resolve(selected, craft.Contribution,
                        new StableId("craft-" + craft.Sequence));
                    craft.GateProcessed = true;
                    if (!result.Applied) continue;
                    LastGateBefore += result.Before; LastGateAfter += result.After;
                    if (selected.Outcome == GateOutcome.Convert)
                    { powder += result.Converted; craft.Contribution = result.Remainder; }
                    else craft.Contribution = result.After;
                    ChoseEasyGate = selected == easyGate;
                }
            }
            if (!gateCommitted && next >= GateProgress)
            {
                gateCommitted = true;
                loadout.ReportGateResolved();
            }
            if (!rescueApplied && routeProgress < RescueProgress && next >= RescueProgress)
            {
                rescueApplied = true;
                if (LevelNumber == 1 && horizontalChoice < 0f && Mathf.Abs(horizontalChoice - EasyLane) <= GateWidth)
                {
                    rescueCollected = true;
                    AddCraft(rescue.SurvivorCount).Progress = next;
                }
            }
            routeProgress = LevelNumber == 2 && blockadeHealth > 0f ? Mathf.Min(next, BlockadeProgress) : next;
            SyncSeaForce();
            if (seaForce.LogicalCount == 0) { Finish(false, "force-depleted"); return; }
            if (routeProgress >= 1f) SetPhase(Level01TrialPhase.Landing);
        }

        private void SyncSeaForce()
        {
            var total = 0;
            foreach (var craft in fleet) total = checked(total + craft.Contribution);
            ChangeForce(seaForce, total);
        }

        public bool DestroyCraft(int sequence)
        {
            if (!IsRunning || paused || Phase != Level01TrialPhase.Traversal) return false;
            foreach (var craft in fleet)
            {
                if (craft.Sequence != sequence || craft.Contribution <= 0) continue;
                craft.Contribution = 0;
                SyncSeaForce();
                if (seaForce.LogicalCount == 0) Finish(false, "force-depleted");
                return true;
            }
            return false;
        }

        private void ReconcileFleetCount()
        {
            var current = 0;
            foreach (var craft in fleet) current += craft.Contribution;
            var difference = seaForce.LogicalCount - current;
            if (difference > 0) AddCraft(difference);
            else if (difference < 0)
                foreach (var craft in fleet)
                {
                    var removed = Mathf.Min(craft.Contribution, -difference);
                    craft.Contribution -= removed; difference += removed;
                    if (difference == 0) break;
                }
        }

        private void TransferNextCraft()
        {
            if (landingIndex >= fleet.Count) return;
            var craft = fleet[landingIndex];
            if (!landing.TryAccept(craft, landingIndex, craft.Contribution,
                craft.Contribution > 0, craft.Contribution == 0, loadout.Crew.Role)) return;
            craft.Contribution = 0;
            landingIndex++;
            SyncSeaForce();
        }

        private sealed class VoyageCraft : ILandingCraft
        {
            public readonly int Sequence;
            public int Contribution;
            public float Progress;
            public bool GateProcessed;
            public VoyageCraft(int sequence, int contribution, float progress)
            { Sequence = sequence; Contribution = contribution; Progress = progress; GateProcessed = progress >= .4f; }
            public void Activate(Vector3 position, int contribution, int sequence) { Contribution = contribution; }
            public void Deactivate() { }
        }
    }
}
