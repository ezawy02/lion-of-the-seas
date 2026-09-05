using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static partial class VerticalSliceBlockoutBuilder
{
    const string FlagshipLod1 =
        ShipRoot + "L01-SHP-004_Hero_Flagship_TripoV31_R2_LOD1_REVIEW.fbx";
    const string FlagshipLod2 =
        ShipRoot + "L01-SHP-004_Hero_Flagship_TripoV31_R2_LOD2_REVIEW.fbx";
    const string GuardianLod1 =
        CharacterRoot + "L01-CHR-004_Harbor_Guardian_Boss_Rigged_Optimized_LOD1_REVIEW.fbx";
    const string GuardianLod2 =
        CharacterRoot + "L01-CHR-004_Harbor_Guardian_Boss_Rigged_Optimized_LOD2_REVIEW.fbx";

    static void ConfigureBenchmarkTierALods()
    {
        AttachLodGroup("PLAYER__BattleFlagship", FlagshipLod1, FlagshipLod2,
            0.30f, 0.14f, 0.025f, "PROP__Friendly_Landing_Banner");
        AttachLodGroup("BOSS__HarborGuardian", GuardianLod1, GuardianLod2,
            0.38f, 0.17f, 0.03f, null);
    }

    static void ValidateBenchmarkTierALods()
    {
        ValidateLodGroup("PLAYER__BattleFlagship");
        ValidateLodGroup("BOSS__HarborGuardian");
    }

    static void AttachLodGroup(string objectName, string lod1Path, string lod2Path,
        float lod0Height, float lod1Height, float cullHeight, string excludedChildName)
    {
        var lod0Root = GameObject.Find(objectName);
        if (lod0Root == null)
            throw new MissingReferenceException("LOD0 object is missing: " + objectName);

        var lod0Renderers = lod0Root.GetComponentsInChildren<Renderer>(true)
            .Where(renderer => !IsUnderNamedChild(renderer.transform, excludedChildName))
            .ToArray();
        var groupRoot = WrapForLodGroup(lod0Root);
        var lod1Root = InstantiateLod(groupRoot.transform, lod1Path, "LOD1__REVIEW_CANDIDATE");
        var lod2Root = InstantiateLod(groupRoot.transform, lod2Path, "LOD2__REVIEW_CANDIDATE");
        var lod1Renderers = lod1Root.GetComponentsInChildren<Renderer>(true);
        var lod2Renderers = lod2Root.GetComponentsInChildren<Renderer>(true);
        CopyMaterials(lod0Renderers, lod1Renderers);
        CopyMaterials(lod0Renderers, lod2Renderers);

        var group = groupRoot.AddComponent<LODGroup>();
        group.fadeMode = LODFadeMode.CrossFade;
        group.animateCrossFading = true;
        group.SetLODs(new[]
        {
            new LOD(lod0Height, lod0Renderers) { fadeTransitionWidth = 0.12f },
            new LOD(lod1Height, lod1Renderers) { fadeTransitionWidth = 0.12f },
            new LOD(cullHeight, lod2Renderers) { fadeTransitionWidth = 0.12f }
        });
        group.RecalculateBounds();
    }

    static GameObject WrapForLodGroup(GameObject lod0Root)
    {
        var source = lod0Root.transform;
        var wrapper = new GameObject(lod0Root.name + "__LOD_GROUP");
        wrapper.transform.SetParent(source.parent, false);
        wrapper.transform.localPosition = source.localPosition;
        wrapper.transform.localRotation = source.localRotation;
        wrapper.transform.localScale = source.localScale;
        source.SetParent(wrapper.transform, false);
        source.localPosition = Vector3.zero;
        source.localRotation = Quaternion.identity;
        source.localScale = Vector3.one;
        return wrapper;
    }

    static GameObject InstantiateLod(Transform parent, string path, string name)
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (asset == null) throw new MissingReferenceException("LOD asset is missing: " + path);
        var instance = PrefabUtility.InstantiatePrefab(asset) as GameObject;
        if (instance == null) throw new InvalidOperationException("Could not instantiate: " + path);
        instance.name = name;
        instance.transform.SetParent(parent, false);
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.transform.localScale = Vector3.one;
        return instance;
    }

    static void CopyMaterials(Renderer[] source, Renderer[] targets)
    {
        if (source.Length == 0 || targets.Length == 0)
            throw new MissingReferenceException("LOD renderer list is empty.");
        for (var index = 0; index < targets.Length; index++)
            targets[index].sharedMaterials = source[Math.Min(index, source.Length - 1)].sharedMaterials;
    }

    static bool IsUnderNamedChild(Transform candidate, string childName)
    {
        if (string.IsNullOrEmpty(childName)) return false;
        for (var current = candidate; current != null; current = current.parent)
            if (current.name == childName) return true;
        return false;
    }

    static void ValidateLodGroup(string objectName)
    {
        var root = GameObject.Find(objectName);
        var group = root == null ? null : root.GetComponentInParent<LODGroup>();
        if (group == null) throw new MissingReferenceException("LODGroup is missing: " + objectName);
        var lods = group.GetLODs();
        if (lods.Length != 3 || lods.Any(lod => lod.renderers == null || lod.renderers.Length == 0))
            throw new InvalidOperationException("LODGroup is incomplete: " + objectName);
        if (!(lods[0].screenRelativeTransitionHeight > lods[1].screenRelativeTransitionHeight &&
              lods[1].screenRelativeTransitionHeight > lods[2].screenRelativeTransitionHeight))
            throw new InvalidOperationException("LOD thresholds are not descending: " + objectName);
    }
}
