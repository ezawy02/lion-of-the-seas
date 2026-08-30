using UnityEngine;

public static partial class VerticalSliceBlockoutBuilder
{
    static void PlaceTraversalFlagshipGroup(Transform root)
    {
        var position = new Vector3(-3.2f, 0.05f, 13.3f);
        var rotation = new Vector3(-90f, 350f, 0f);
        var flagship = Model(root, "PLAYER__Flagship", Level01ReferenceShip,
            position, Vector3.one * 7.4f, rotation);
        ApprovedOpeningModel(root, "PLAYER__SecondLateenAndHelm", ApprovedOpeningAddon,
            position, Vector3.one * 4f, rotation);
        Model(root, "CHARACTER__Hayreddin_OnDeck", Level01HeroPose,
            new Vector3(-2.85f, 4.1f, 7.15f), Vector3.one * 1.65f, new Vector3(0f, -10f, 0f));
        var banner = Model(root, "PROP__FlagshipLionWaveBanner",
            EnvironmentRoot + "L01-PRP-002_Lion_Wave_Banner_Optimized.fbx",
            new Vector3(-4.35f, 7.3f, 13.3f), Vector3.one * 0.95f, rotation);
        if (flagship != null && banner != null) banner.transform.SetParent(flagship.transform, true);
        CompactCraftWake(root, "VFX__FlagshipWake", new Vector3(-3.2f, 0.048f, 7f), 350f, true);
    }
}
