using System;
using System.IO;
using SeaLion.Presentation.Audio;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static partial class VerticalSliceBlockoutBuilder
{
    const string Level01AudioRoot = "Assets/_Project/Audio/Level01/";
    const string Level01AudioLibraryPath = Level01AudioRoot + "Level01AudioLibrary.asset";
    const string Level01ProductionScene = "Assets/_Project/Scenes/Level_01_HundredSails.unity";

    static readonly AudioAssetBinding[] Level01AudioBindings =
    {
        new AudioAssetBinding("broadsideCannon", "L01_SFX_Broadside_Cannon_R3.ogg", AudioClipLoadType.DecompressOnLoad),
        new AudioAssetBinding("gateEnergyLoop", "L01_SFX_Gate_EnergyLoop_R1.wav", AudioClipLoadType.CompressedInMemory),
        new AudioAssetBinding("gateMultiplyX4", "L01_SFX_Gate_MultiplyX4_R1.ogg", AudioClipLoadType.DecompressOnLoad),
        new AudioAssetBinding("gateDamage", "L01_SFX_Gate_Damage_R1.ogg", AudioClipLoadType.DecompressOnLoad),
        new AudioAssetBinding("landingShallowWater", "L01_SFX_Landing_ShallowWater_R1.ogg", AudioClipLoadType.DecompressOnLoad),
        new AudioAssetBinding("crewLoss", "L01_SFX_Crew_Loss_R1.ogg", AudioClipLoadType.DecompressOnLoad),
        new AudioAssetBinding("guardianArmorHit", "L01_SFX_Guardian_ArmorHit_R1.ogg", AudioClipLoadType.DecompressOnLoad),
        new AudioAssetBinding("guardianDefeat", "L01_SFX_Guardian_Defeat_R1.ogg", AudioClipLoadType.DecompressOnLoad),
        new AudioAssetBinding("rewardCorsair", "L01_SFX_Reward_Corsair_R1.ogg", AudioClipLoadType.DecompressOnLoad),
        new AudioAssetBinding("failureMedieval", "L01_SFX_Failure_Medieval_R1.mp3", AudioClipLoadType.DecompressOnLoad),
        new AudioAssetBinding("seaAmbience", "L01_AMB_SeaLoop_R1.ogg", AudioClipLoadType.CompressedInMemory),
        new AudioAssetBinding("windAmbience", "L01_AMB_WindLoop_R1.wav", AudioClipLoadType.CompressedInMemory),
        new AudioAssetBinding("traversalMusic", "L01_MUS_Traversal_Pirate_R1.mp3", AudioClipLoadType.Streaming),
        new AudioAssetBinding("guardianBattleMusic", "L01_MUS_GuardianBattle_R1.mp3", AudioClipLoadType.Streaming)
    };

    [MenuItem("Lion of the Seas/Audio/Prepare Level 01 Audio Library REVIEW")]
    public static void PrepareLevel01AudioLibraryReview()
    {
        var library = EnsureLevel01AudioLibrary();
        ValidateLevel01AudioLibrary(library);
        AssetDatabase.SaveAssets();
        Debug.Log("Level 01 audio library REVIEW prepared. User listening approval is still required.");
    }

    [MenuItem("Lion of the Seas/Audio/Install Level 01 Audio In Production Scene REVIEW")]
    public static void InstallLevel01AudioInProductionSceneReview()
    {
        var library = EnsureLevel01AudioLibrary();
        EditorSceneManager.OpenScene(Level01ProductionScene, OpenSceneMode.Single);
        var director = UnityEngine.Object.FindFirstObjectByType<Level01AudioDirector>();
        if (director == null)
        {
            var audioObject = new GameObject("AUDIO__Level01Director_REVIEW");
            director = audioObject.AddComponent<Level01AudioDirector>();
        }
        director.Configure(library, true);
        EnsureSingleAudioListener();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), Level01ProductionScene);
        AssetDatabase.SaveAssets();
        Debug.Log("Level 01 production scene audio REVIEW installed. Event hooks remain presentation-driven.");
    }

    static void BuildBenchmarkArtAudioReview(Transform root)
    {
        var library = EnsureLevel01AudioLibrary();
        var audioObject = new GameObject("AUDIO__Level01FullPass_REVIEW");
        audioObject.transform.SetParent(root, false);
        var director = audioObject.AddComponent<Level01AudioDirector>();
        director.Configure(library, true);
        var sequence = audioObject.AddComponent<Level01AudioReviewSequence>();
        sequence.Configure(director, true);
        EnsureSingleAudioListener();
    }

    static void ValidateBenchmarkArtAudioReview()
    {
        var director = UnityEngine.Object.FindFirstObjectByType<Level01AudioDirector>();
        var sequence = UnityEngine.Object.FindFirstObjectByType<Level01AudioReviewSequence>();
        if (director == null || sequence == null)
            throw new MissingReferenceException("Benchmark audio director or review sequence is missing.");
        ValidateLevel01AudioLibrary(director.Library);
        var listeners = UnityEngine.Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        if (listeners.Length != 1)
            throw new InvalidDataException($"Benchmark must have exactly one AudioListener; found {listeners.Length}.");
    }

    static Level01AudioLibrary EnsureLevel01AudioLibrary()
    {
        Directory.CreateDirectory(Level01AudioRoot);
        foreach (var binding in Level01AudioBindings) ConfigureAudioImporter(binding);
        var library = AssetDatabase.LoadAssetAtPath<Level01AudioLibrary>(Level01AudioLibraryPath);
        if (library == null)
        {
            library = ScriptableObject.CreateInstance<Level01AudioLibrary>();
            AssetDatabase.CreateAsset(library, Level01AudioLibraryPath);
        }
        var serialized = new SerializedObject(library);
        foreach (var binding in Level01AudioBindings)
        {
            var property = serialized.FindProperty(binding.Field);
            if (property == null) throw new MissingFieldException(typeof(Level01AudioLibrary).Name, binding.Field);
            property.objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(binding.Path);
        }
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(library);
        return library;
    }

    static void ConfigureAudioImporter(AudioAssetBinding binding)
    {
        if (!File.Exists(binding.Path)) throw new FileNotFoundException("Level 01 audio clip is missing.", binding.Path);
        AssetDatabase.ImportAsset(binding.Path, ImportAssetOptions.ForceSynchronousImport);
        var importer = AssetImporter.GetAtPath(binding.Path) as AudioImporter;
        if (importer == null) throw new InvalidDataException("Unity did not create an AudioImporter for " + binding.Path);
        var settings = importer.defaultSampleSettings;
        settings.loadType = binding.LoadType;
        settings.compressionFormat = binding.Path.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase)
            ? AudioCompressionFormat.MP3
            : AudioCompressionFormat.Vorbis;
        settings.quality = binding.LoadType == AudioClipLoadType.Streaming ? 0.55f : 0.72f;
        settings.preloadAudioData = binding.LoadType != AudioClipLoadType.Streaming;
        importer.defaultSampleSettings = settings;
        importer.forceToMono = false;
        importer.loadInBackground = binding.LoadType == AudioClipLoadType.Streaming;
        importer.SaveAndReimport();
    }

    static void ValidateLevel01AudioLibrary(Level01AudioLibrary library)
    {
        if (library == null) throw new MissingReferenceException("Level 01 audio library is missing.");
        if (!library.AllClipsAssigned(out var missing))
            throw new MissingReferenceException("Level 01 audio library is missing cue: " + missing);
        foreach (Level01AudioCue cue in Enum.GetValues(typeof(Level01AudioCue)))
        {
            var clip = library.ClipFor(cue);
            if (clip.frequency != 48000 || clip.channels != 2 || clip.length <= 0.5f)
                throw new InvalidDataException($"Invalid imported audio cue {cue}: {clip.frequency} Hz, {clip.channels} channels, {clip.length:F2}s.");
        }
    }

    static void EnsureSingleAudioListener()
    {
        var camera = Camera.main;
        if (camera == null) throw new MissingReferenceException("Main camera is required for Level 01 audio.");
        var listeners = UnityEngine.Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        foreach (var listener in listeners)
            if (listener.gameObject != camera.gameObject) UnityEngine.Object.DestroyImmediate(listener);
        if (camera.GetComponent<AudioListener>() == null) camera.gameObject.AddComponent<AudioListener>();
    }

    readonly struct AudioAssetBinding
    {
        public readonly string Field;
        public readonly string Path;
        public readonly AudioClipLoadType LoadType;

        public AudioAssetBinding(string field, string file, AudioClipLoadType loadType)
        {
            Field = field;
            Path = Level01AudioRoot + file;
            LoadType = loadType;
        }
    }
}
