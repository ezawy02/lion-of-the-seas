using System;
using System.Collections.Generic;
using SeaLion.Gameplay.Levels;
using UnityEngine;

namespace SeaLion.Presentation.Levels
{
    /// <summary>
    /// Runtime-only motion for the approved Level 1 staging. The source FBXs contain no
    /// animation clips, so this review layer adds readable fleet, landing, and battle motion
    /// without modifying the approved art scene or its authored transforms.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Level01TrialMotionPresenter : MonoBehaviour
    {
        private const float CaptainScaleMultiplier = 1.12f;
        private static readonly Vector3 TraversalHeroCenterOffset = new Vector3(3.15f, 0f, 0f);

        private enum MotionKind
        {
            HeroShip,
            SupportShip,
            PatrolShip,
            LandingCraft,
            FriendlyUnit,
            HostileUnit,
            Character,
            Guardian,
            Attachment,
            Wake
        }

        private sealed class MotionTrack
        {
            public Transform Target;
            public Vector3 Position;
            public Quaternion Rotation;
            public Vector3 Scale;
            public MotionKind Kind;
            public float Offset;
            public bool PlayerControlled;
            public RigPose Rig;
            public Transform Anchor;
            public Vector3 AnchorPosition;
            public Quaternion AnchorRotation;
            public float PreviousX;
            public float LateralVelocity;
        }

        private sealed class RigPose
        {
            public Transform Spine, Head, LeftArm, RightArm, LeftLeg, RightLeg, LeftKnee, RightKnee;
            public Quaternion SpineRotation, HeadRotation, LeftArmRotation, RightArmRotation;
            public Quaternion LeftLegRotation, RightLegRotation, LeftKneeRotation, RightKneeRotation;
        }

        private readonly List<MotionTrack> tracks = new List<MotionTrack>(256);
        private Level01TrialRuntime runtime;
        private float lastBossHealth = 1f;
        private float guardianHitPulse;
        private bool ready;

        public int TrackCount => tracks.Count;

        public void Bind(Level01TrialRuntime trialRuntime, GameObject opening, GameObject traversal,
            GameObject landing, GameObject assault, GameObject victory)
        {
            runtime = trialRuntime;
            tracks.Clear();
            TrackOpening(opening);
            TrackTraversal(traversal);
            TrackLanding(landing);
            TrackAssault(assault);
            TrackVictory(victory);
            lastBossHealth = 1f;
            guardianHitPulse = 0f;
            ready = runtime != null && tracks.Count > 0;
        }

        private void LateUpdate()
        {
            if (!ready || runtime == null) return;
            if (runtime.Phase == Level01TrialPhase.Assault && runtime.BossHealth01 < lastBossHealth - .0001f)
                guardianHitPulse = .3f;
            lastBossHealth = runtime.BossHealth01;
            var time = runtime.TotalElapsed;
            for (var index = 0; index < tracks.Count; index++)
            {
                var track = tracks[index];
                if (track.Target == null || !track.Target.gameObject.activeInHierarchy) continue;
                Animate(track, time);
            }
            guardianHitPulse = Mathf.Max(0f, guardianHitPulse - Time.unscaledDeltaTime);
        }

        private void Animate(MotionTrack track, float time)
        {
            var wave = Mathf.Sin(time * 2.15f + track.Offset) +
                Mathf.Sin(time * 3.7f + track.Offset * 1.31f) * .28f;
            var secondary = Mathf.Sin(time * 1.1f + track.Offset * 0.73f);
            var position = track.Position;
            var rotation = track.Rotation;
            var scale = track.Scale;
            switch (track.Kind)
            {
                case MotionKind.HeroShip:
                    if (track.PlayerControlled)
                    {
                        position.x = track.Target.localPosition.x;
                        var deltaTime = Mathf.Max(.001f, Time.unscaledDeltaTime);
                        var velocity = (position.x - track.PreviousX) / deltaTime;
                        var blend = 1f - Mathf.Exp(-5.5f * deltaTime);
                        track.LateralVelocity = Mathf.Lerp(track.LateralVelocity,
                            Mathf.Clamp(velocity, -4f, 4f), blend);
                        track.PreviousX = position.x;
                    }
                    position.y += wave * 0.11f;
                    position.z += TravelDistance(track.Kind, track.Offset);
                    rotation *= Quaternion.Euler(secondary * 0.7f, track.LateralVelocity * .45f + wave * .32f,
                        wave * 1.15f - track.LateralVelocity * 1.9f);
                    break;
                case MotionKind.SupportShip:
                    position.y += wave * 0.075f;
                    position.z += TravelDistance(track.Kind, track.Offset);
                    rotation *= Quaternion.Euler(secondary * 0.55f, 0f, wave * 1.35f);
                    break;
                case MotionKind.PatrolShip:
                    position.x += secondary * 0.55f;
                    position.y += wave * 0.06f;
                    position.z += Mathf.Repeat(runtime.PhaseElapsed * 0.22f + track.Offset, 2.5f);
                    rotation *= Quaternion.Euler(0f, wave * 0.9f, wave * 1.4f);
                    break;
                case MotionKind.LandingCraft:
                    position.y += Mathf.Abs(wave) * 0.09f;
                    position.z += Level01SeaMotion.ForwardDistance(runtime.PhaseElapsed, 9f,
                        17f + track.Offset * .2f);
                    rotation *= Quaternion.Euler(secondary * 0.8f, 0f, wave * 1.8f);
                    break;
                case MotionKind.FriendlyUnit:
                    AnimateUnit(track, time, 1f, ref position, ref rotation);
                    break;
                case MotionKind.HostileUnit:
                    AnimateUnit(track, time, -0.55f, ref position, ref rotation);
                    break;
                case MotionKind.Character:
                    position.y += Mathf.Abs(wave) * 0.018f;
                    rotation *= Quaternion.Euler(secondary * 0.65f, 0f, wave * 0.5f);
                    break;
                case MotionKind.Guardian:
                    position.y += Mathf.Abs(wave) * 0.06f;
                    var hit = guardianHitPulse <= 0f ? 0f :
                        Mathf.Sin(Mathf.Clamp01(guardianHitPulse / .3f) * Mathf.PI);
                    rotation *= Quaternion.Euler(secondary * 1.1f - hit * 7f,
                        wave * 1.5f, -wave * 0.75f + hit * 3f);
                    scale *= 1f + secondary * 0.012f + hit * .055f;
                    break;
                case MotionKind.Attachment:
                    if (track.Anchor != null)
                    {
                        var delta = track.Anchor.localRotation * Quaternion.Inverse(track.AnchorRotation);
                        position = track.Anchor.localPosition + delta * (track.Position - track.AnchorPosition);
                        rotation = delta * track.Rotation;
                    }
                    break;
                case MotionKind.Wake:
                    if (track.Anchor != null)
                    {
                        var anchorDelta = track.Anchor.localPosition - track.AnchorPosition;
                        position.x += anchorDelta.x;
                        position.z += anchorDelta.z;
                    }
                    break;
            }
            track.Target.localPosition = position;
            track.Target.localRotation = rotation;
            track.Target.localScale = scale;
            AnimateRig(track, time);
        }

        private static void AnimateRig(MotionTrack track, float time)
        {
            var rig = track.Rig;
            if (rig == null) return;
            if (track.Kind == MotionKind.FriendlyUnit || track.Kind == MotionKind.HostileUnit)
            {
                var stride = Mathf.Sin(time * 5.2f + track.Offset);
                Set(rig.LeftArm, rig.LeftArmRotation, Quaternion.Euler(-stride * 26f, 0f, 0f));
                Set(rig.RightArm, rig.RightArmRotation, Quaternion.Euler(stride * 26f, 0f, 0f));
                Set(rig.LeftLeg, rig.LeftLegRotation, Quaternion.Euler(stride * 31f, 0f, 0f));
                Set(rig.RightLeg, rig.RightLegRotation, Quaternion.Euler(-stride * 31f, 0f, 0f));
                Set(rig.LeftKnee, rig.LeftKneeRotation, Quaternion.Euler(Mathf.Max(0f, -stride) * 28f, 0f, 0f));
                Set(rig.RightKnee, rig.RightKneeRotation, Quaternion.Euler(Mathf.Max(0f, stride) * 28f, 0f, 0f));
                Set(rig.Spine, rig.SpineRotation, Quaternion.Euler(0f, stride * 2.2f, 0f));
            }
            else if (track.Kind == MotionKind.Guardian)
            {
                var attack = (Mathf.Sin(time * 1.45f + track.Offset) + 1f) * 0.5f;
                var recoil = Mathf.Sin(time * 2.9f + track.Offset) * 4f;
                Set(rig.LeftArm, rig.LeftArmRotation, Quaternion.Euler(-12f - attack * 36f, 0f, -attack * 8f));
                Set(rig.RightArm, rig.RightArmRotation, Quaternion.Euler(-12f - attack * 36f, 0f, attack * 8f));
                Set(rig.Spine, rig.SpineRotation, Quaternion.Euler(recoil, 0f, 0f));
                Set(rig.Head, rig.HeadRotation, Quaternion.Euler(-recoil * 0.35f, Mathf.Sin(time) * 3f, 0f));
            }
            else
            {
                var idle = Mathf.Sin(time * 1.6f + track.Offset);
                Set(rig.Spine, rig.SpineRotation, Quaternion.Euler(idle * 0.8f, 0f, 0f));
                Set(rig.Head, rig.HeadRotation, Quaternion.Euler(0f, idle * 2f, 0f));
                Set(rig.LeftArm, rig.LeftArmRotation, Quaternion.Euler(idle * 2.5f, 0f, 0f));
                Set(rig.RightArm, rig.RightArmRotation, Quaternion.Euler(-idle * 2.5f, 0f, 0f));
            }
        }

        private static void Set(Transform bone, Quaternion original, Quaternion delta)
        {
            if (bone != null) bone.localRotation = original * delta;
        }

        private void AnimateUnit(MotionTrack track, float time, float direction,
            ref Vector3 position, ref Quaternion rotation)
        {
            var march = Mathf.Abs(Mathf.Sin(time * 4.8f + track.Offset));
            position.y += march * 0.055f;
            if (runtime.Phase == Level01TrialPhase.Landing)
                position.z += Mathf.Clamp01((runtime.PhaseElapsed - track.Offset * 0.08f) / 7.5f) * 2.4f;
            else if (runtime.Phase == Level01TrialPhase.Assault || runtime.Phase == Level01TrialPhase.Failure)
                position.z += Mathf.Clamp01(runtime.PhaseElapsed / 12f) * 4.2f * direction;
            rotation *= Quaternion.Euler(march * 1.8f, Mathf.Sin(time * 2.4f + track.Offset) * 0.7f, 0f);
        }

        private float TravelDistance(MotionKind kind, float offset)
        {
            var elapsed = runtime.Phase == Level01TrialPhase.Traversal
                ? runtime.TraversalActiveElapsed
                : runtime.PhaseElapsed;
            if (runtime.Phase == Level01TrialPhase.Opening)
                return Level01SeaMotion.ForwardDistance(elapsed, 3f,
                    kind == MotionKind.HeroShip ? 2.2f : 1.7f);
            if (runtime.Phase == Level01TrialPhase.Traversal)
                return Level01SeaMotion.ForwardDistance(elapsed, 10f,
                    kind == MotionKind.HeroShip ? 9.5f : 11.5f);
            if (runtime.Phase == Level01TrialPhase.Landing)
                return Level01SeaMotion.ForwardDistance(elapsed, 9f, 3.5f);
            return kind == MotionKind.HeroShip ? Mathf.Sin(elapsed * 0.3f + offset) * 0.25f : 0f;
        }

        private void TrackOpening(GameObject root)
        {
            var hero = AddNamed(root, "PLAYER__Flagship", MotionKind.HeroShip);
            AddWake(root, "VFX__FlagshipWake", hero);
            AddAttached(root, "PLAYER__SecondLateenAndHelm", hero);
            AddAttached(root, "CHARACTER__Hayreddin_OnDeck", hero, CaptainScaleMultiplier);
            AddAttached(root, "PROP__FlagshipLionWaveBanner", hero);
            var port = AddNamed(root, "ESCORT__Port", MotionKind.SupportShip);
            var starboard = AddNamed(root, "ESCORT__Starboard", MotionKind.SupportShip);
            AddAttachedPrefix(root, "CREW__OpeningPort_", port);
            AddAttachedPrefix(root, "CREW__OpeningStarboard_", starboard);
            AddPrefix(root, "ENEMY__Patrol_", MotionKind.PatrolShip, false);
        }

        private void TrackTraversal(GameObject root)
        {
            var hero = AddNamed(root, "PLAYER__Flagship", MotionKind.HeroShip, true);
            AddWake(root, "VFX__FlagshipWake", hero);
            AddAttached(root, "PLAYER__SecondLateenAndHelm", hero);
            AddAttached(root, "CHARACTER__Hayreddin_OnDeck", hero, CaptainScaleMultiplier);
            AddAttached(root, "PROP__FlagshipLionWaveBanner", hero);
            OffsetTrackedTarget(hero, TraversalHeroCenterOffset);
            for (var index = 0; index < 5; index++)
            {
                var craft = AddNamed(root, "FRIENDLY__GateCraft_" + index, MotionKind.SupportShip);
                AddAttachedPrefix(root, "CREW__GateCraft_" + index + "_", craft);
            }
            AddNamed(root, "RESCUE__CaptiveSailmakers", MotionKind.SupportShip);
            AddPrefix(root, "ENEMY__Patrol_", MotionKind.PatrolShip, false);
        }

        private void TrackLanding(GameObject root)
        {
            var hero = AddNamed(root, "PLAYER__Flagship", MotionKind.HeroShip);
            AddWake(root, "VFX__FlagshipWake", hero);
            AddAttached(root, "CHARACTER__Hayreddin_OnDeck", hero, CaptainScaleMultiplier);
            AddAttached(root, "PROP__FlagshipLionWaveBanner", hero);
            for (var index = 0; index < 7; index++)
            {
                var craft = AddNamed(root, "CRAFT__LandingFan_" + index, MotionKind.LandingCraft);
                AddAttachedPrefix(root, "CREW__LandingFan_" + index + "_", craft);
            }
        }

        private void TrackAssault(GameObject root)
        {
            var hero = AddNamed(root, "PLAYER__BattleFlagship", MotionKind.HeroShip);
            AddAttached(root, "PLAYER__BattleSecondLateenAndHelm", hero);
            AddAttached(root, "CHARACTER__Hayreddin_Battle", hero, CaptainScaleMultiplier);
            AddAttached(root, "PROP__Friendly_Landing_Banner", hero);
            AddNamed(root, "HOSTILE__EnemyCommander_REVIEW", MotionKind.Character);
            AddNamed(root, "BOSS__HarborGuardian", MotionKind.Guardian);
        }

        private void TrackVictory(GameObject root)
        {
            AddPrefix(root, "FRIENDLY__", MotionKind.Character, false);
            AddPrefix(root, "CHARACTER__", MotionKind.Character, false);
        }

        private void AddChildren(GameObject root, string groupName, MotionKind kind)
        {
            var group = Find(root, groupName);
            if (group == null) return;
            for (var index = 0; index < group.childCount; index++) Add(group.GetChild(index), kind, false);
        }

        private Transform AddNamed(GameObject root, string objectName, MotionKind kind, bool controlled = false)
        {
            var target = Find(root, objectName);
            if (target != null) Add(target, kind, controlled);
            return target;
        }

        private void AddAttached(GameObject root, string objectName, Transform anchor, float scaleMultiplier = 1f)
        {
            var target = Find(root, objectName);
            if (target != null && anchor != null && !target.IsChildOf(anchor))
                AddAttachment(target, anchor, scaleMultiplier);
        }

        private void AddWake(GameObject root, string objectName, Transform anchor)
        {
            var target = Find(root, objectName);
            if (target == null || anchor == null) return;
            tracks.Add(new MotionTrack
            {
                Target = target,
                Position = target.localPosition,
                Rotation = target.localRotation,
                Scale = target.localScale,
                Kind = MotionKind.Wake,
                Offset = tracks.Count * .71f,
                Anchor = anchor,
                AnchorPosition = anchor.localPosition,
                AnchorRotation = anchor.localRotation
            });
        }

        private void AddAttachedPrefix(GameObject root, string prefix, Transform anchor)
        {
            if (root == null || anchor == null) return;
            var values = root.GetComponentsInChildren<Transform>(true);
            for (var index = 0; index < values.Length; index++)
                if (values[index].parent == root.transform && values[index].name.StartsWith(prefix, StringComparison.Ordinal))
                    AddAttachment(values[index], anchor);
        }

        private void AddPrefix(GameObject root, string prefix, MotionKind kind, bool includeNested)
        {
            if (root == null) return;
            var values = root.GetComponentsInChildren<Transform>(true);
            for (var index = 0; index < values.Length; index++)
            {
                var target = values[index];
                if (!target.name.StartsWith(prefix, StringComparison.Ordinal)) continue;
                if (!includeNested && target.parent != root.transform) continue;
                Add(target, kind, false);
            }
        }

        private void Add(Transform target, MotionKind kind, bool controlled)
        {
            tracks.Add(new MotionTrack
            {
                Target = target,
                Position = target.localPosition,
                Rotation = target.localRotation,
                Scale = target.localScale,
                Kind = kind,
                Offset = tracks.Count * 0.71f,
                PlayerControlled = controlled,
                PreviousX = target.localPosition.x,
                Rig = kind == MotionKind.HeroShip || kind == MotionKind.SupportShip ||
                    kind == MotionKind.PatrolShip || kind == MotionKind.LandingCraft ? null : CreateRig(target)
            });
        }

        private void OffsetTrackedTarget(Transform target, Vector3 offset)
        {
            if (target == null) return;
            target.localPosition += offset;
            for (var index = 0; index < tracks.Count; index++)
            {
                var track = tracks[index];
                if (track.Target == target && track.Kind != MotionKind.Attachment)
                    track.Position += offset;
                else if (track.Anchor == target)
                    track.Target.localPosition += offset;
            }
        }

        private void AddAttachment(Transform target, Transform anchor, float scaleMultiplier = 1f)
        {
            tracks.Add(new MotionTrack
            {
                Target = target,
                Position = target.localPosition,
                Rotation = target.localRotation,
                Scale = target.localScale * Mathf.Max(0.01f, scaleMultiplier),
                Kind = MotionKind.Attachment,
                Offset = tracks.Count * 0.71f,
                Anchor = anchor,
                AnchorPosition = anchor.localPosition,
                AnchorRotation = anchor.localRotation,
                Rig = CreateRig(target)
            });
        }

        private static RigPose CreateRig(Transform root)
        {
            var rig = new RigPose
            {
                Spine = Find(root, "spine"),
                Head = Find(root, "head"),
                LeftArm = Find(root, "upper_arm_L"),
                RightArm = Find(root, "upper_arm_R"),
                LeftLeg = Find(root, "upper_leg_L"),
                RightLeg = Find(root, "upper_leg_R"),
                LeftKnee = Find(root, "lower_leg_L"),
                RightKnee = Find(root, "lower_leg_R")
            };
            if (rig.Spine == null && rig.Head == null && rig.LeftArm == null) return null;
            rig.SpineRotation = Rotation(rig.Spine);
            rig.HeadRotation = Rotation(rig.Head);
            rig.LeftArmRotation = Rotation(rig.LeftArm);
            rig.RightArmRotation = Rotation(rig.RightArm);
            rig.LeftLegRotation = Rotation(rig.LeftLeg);
            rig.RightLegRotation = Rotation(rig.RightLeg);
            rig.LeftKneeRotation = Rotation(rig.LeftKnee);
            rig.RightKneeRotation = Rotation(rig.RightKnee);
            return rig;
        }

        private static Quaternion Rotation(Transform value) => value == null ? Quaternion.identity : value.localRotation;

        private static Transform Find(Transform root, string name)
        {
            if (root.name == name) return root;
            for (var index = 0; index < root.childCount; index++)
            {
                var value = Find(root.GetChild(index), name);
                if (value != null) return value;
            }
            return null;
        }

        private static Transform Find(GameObject root, string name)
        {
            if (root == null) return null;
            var values = root.GetComponentsInChildren<Transform>(true);
            for (var index = 0; index < values.Length; index++)
                if (values[index].name == name) return values[index];
            return null;
        }
    }
}
