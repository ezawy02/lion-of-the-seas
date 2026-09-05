using System;
using NUnit.Framework;
using SeaLion.Presentation.Audio;
using UnityEditor;
using UnityEngine;

namespace SeaLion.Tests.EditMode.Audio
{
    public sealed class Level01AudioAssetTests
    {
        private const string LibraryPath = "Assets/_Project/Audio/Level01/Level01AudioLibrary.asset";

        [Test]
        public void LibraryContainsEveryLevel01Cue()
        {
            var library = AssetDatabase.LoadAssetAtPath<Level01AudioLibrary>(LibraryPath);
            Assert.That(library, Is.Not.Null, LibraryPath);
            Assert.That(library.AllClipsAssigned(out var missing), Is.True, missing.ToString());
            foreach (Level01AudioCue cue in Enum.GetValues(typeof(Level01AudioCue)))
            {
                var clip = library.ClipFor(cue);
                Assert.That(clip, Is.Not.Null, cue.ToString());
                Assert.That(clip.frequency, Is.EqualTo(48000), cue.ToString());
                Assert.That(clip.channels, Is.EqualTo(2), cue.ToString());
                Assert.That(clip.length, Is.GreaterThan(0.5f), cue.ToString());
            }
        }

        [Test]
        public void LongMusicStreamsWhileShortCuesRemainPreloaded()
        {
            AssertLoadType("L01_MUS_Traversal_Pirate_R1.mp3", AudioClipLoadType.Streaming);
            AssertLoadType("L01_MUS_GuardianBattle_R1.mp3", AudioClipLoadType.Streaming);
            AssertLoadType("L01_SFX_Broadside_Cannon_R3.ogg", AudioClipLoadType.DecompressOnLoad);
            AssertLoadType("L01_AMB_SeaLoop_R1.ogg", AudioClipLoadType.CompressedInMemory);
        }

        [Test]
        public void LoopContractDoesNotMisclassifyResultOrImpactCues()
        {
            Assert.That(Level01AudioLibrary.IsLoopingCue(Level01AudioCue.SeaAmbience), Is.True);
            Assert.That(Level01AudioLibrary.IsLoopingCue(Level01AudioCue.WindAmbience), Is.True);
            Assert.That(Level01AudioLibrary.IsLoopingCue(Level01AudioCue.GateEnergyLoop), Is.True);
            Assert.That(Level01AudioLibrary.IsLoopingCue(Level01AudioCue.TraversalMusic), Is.True);
            Assert.That(Level01AudioLibrary.IsLoopingCue(Level01AudioCue.GuardianBattleMusic), Is.True);
            Assert.That(Level01AudioLibrary.IsLoopingCue(Level01AudioCue.BroadsideCannon), Is.False);
            Assert.That(Level01AudioLibrary.IsLoopingCue(Level01AudioCue.GuardianDefeat), Is.False);
            Assert.That(Level01AudioLibrary.IsLoopingCue(Level01AudioCue.RewardCorsair), Is.False);
        }

        private static void AssertLoadType(string file, AudioClipLoadType expected)
        {
            var path = "Assets/_Project/Audio/Level01/" + file;
            var importer = AssetImporter.GetAtPath(path) as AudioImporter;
            Assert.That(importer, Is.Not.Null, path);
            Assert.That(importer.defaultSampleSettings.loadType, Is.EqualTo(expected), path);
        }
    }
}
