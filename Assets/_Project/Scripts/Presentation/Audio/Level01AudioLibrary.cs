using System;
using UnityEngine;

namespace SeaLion.Presentation.Audio
{
    public enum Level01AudioCue
    {
        BroadsideCannon,
        GateEnergyLoop,
        GateMultiplyX4,
        GateDamage,
        LandingShallowWater,
        CrewLoss,
        GuardianArmorHit,
        GuardianDefeat,
        RewardCorsair,
        FailureMedieval,
        SeaAmbience,
        WindAmbience,
        TraversalMusic,
        GuardianBattleMusic
    }

    [CreateAssetMenu(fileName = "Level01AudioLibrary", menuName = "Sea Lion/Audio/Level 01 Library")]
    public sealed class Level01AudioLibrary : ScriptableObject
    {
        [Header("One shots")]
        [SerializeField] private AudioClip broadsideCannon;
        [SerializeField] private AudioClip gateMultiplyX4;
        [SerializeField] private AudioClip gateDamage;
        [SerializeField] private AudioClip landingShallowWater;
        [SerializeField] private AudioClip crewLoss;
        [SerializeField] private AudioClip guardianArmorHit;
        [SerializeField] private AudioClip guardianDefeat;
        [SerializeField] private AudioClip rewardCorsair;
        [SerializeField] private AudioClip failureMedieval;

        [Header("Loops")]
        [SerializeField] private AudioClip gateEnergyLoop;
        [SerializeField] private AudioClip seaAmbience;
        [SerializeField] private AudioClip windAmbience;
        [SerializeField] private AudioClip traversalMusic;
        [SerializeField] private AudioClip guardianBattleMusic;

        public AudioClip BroadsideCannon => broadsideCannon;
        public AudioClip GateEnergyLoop => gateEnergyLoop;
        public AudioClip GateMultiplyX4 => gateMultiplyX4;
        public AudioClip GateDamage => gateDamage;
        public AudioClip LandingShallowWater => landingShallowWater;
        public AudioClip CrewLoss => crewLoss;
        public AudioClip GuardianArmorHit => guardianArmorHit;
        public AudioClip GuardianDefeat => guardianDefeat;
        public AudioClip RewardCorsair => rewardCorsair;
        public AudioClip FailureMedieval => failureMedieval;
        public AudioClip SeaAmbience => seaAmbience;
        public AudioClip WindAmbience => windAmbience;
        public AudioClip TraversalMusic => traversalMusic;
        public AudioClip GuardianBattleMusic => guardianBattleMusic;

        public AudioClip ClipFor(Level01AudioCue cue)
        {
            switch (cue)
            {
                case Level01AudioCue.BroadsideCannon: return broadsideCannon;
                case Level01AudioCue.GateEnergyLoop: return gateEnergyLoop;
                case Level01AudioCue.GateMultiplyX4: return gateMultiplyX4;
                case Level01AudioCue.GateDamage: return gateDamage;
                case Level01AudioCue.LandingShallowWater: return landingShallowWater;
                case Level01AudioCue.CrewLoss: return crewLoss;
                case Level01AudioCue.GuardianArmorHit: return guardianArmorHit;
                case Level01AudioCue.GuardianDefeat: return guardianDefeat;
                case Level01AudioCue.RewardCorsair: return rewardCorsair;
                case Level01AudioCue.FailureMedieval: return failureMedieval;
                case Level01AudioCue.SeaAmbience: return seaAmbience;
                case Level01AudioCue.WindAmbience: return windAmbience;
                case Level01AudioCue.TraversalMusic: return traversalMusic;
                case Level01AudioCue.GuardianBattleMusic: return guardianBattleMusic;
                default: throw new ArgumentOutOfRangeException(nameof(cue), cue, null);
            }
        }

        public bool AllClipsAssigned(out Level01AudioCue missingCue)
        {
            foreach (Level01AudioCue cue in Enum.GetValues(typeof(Level01AudioCue)))
            {
                if (ClipFor(cue) != null) continue;
                missingCue = cue;
                return false;
            }
            missingCue = default(Level01AudioCue);
            return true;
        }

        public static bool IsLoopingCue(Level01AudioCue cue)
        {
            return cue == Level01AudioCue.GateEnergyLoop ||
                cue == Level01AudioCue.SeaAmbience ||
                cue == Level01AudioCue.WindAmbience ||
                cue == Level01AudioCue.TraversalMusic ||
                cue == Level01AudioCue.GuardianBattleMusic;
        }
    }
}
