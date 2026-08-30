using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class ArtRenderPipelineSetup
{
    const string Root = "Assets/_Project/Settings/Rendering/";
    const string RendererPath = Root + "SeaLion_UniversalRenderer.asset";
    const string PipelinePath = Root + "SeaLion_URP_Primary.asset";

    [MenuItem("Lion of the Seas/Configure Premium URP Rendering")]
    public static void ConfigurePremiumRendering()
    {
        Directory.CreateDirectory(Root);
        var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererPath);
        if (renderer == null)
        {
            renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
            renderer.name = "SeaLion_UniversalRenderer";
            AssetDatabase.CreateAsset(renderer, RendererPath);
        }

        var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(PipelinePath);
        if (pipeline == null)
        {
            pipeline = UniversalRenderPipelineAsset.Create(renderer);
            pipeline.name = "SeaLion_URP_Primary";
            AssetDatabase.CreateAsset(pipeline, PipelinePath);
        }

        pipeline.supportsCameraDepthTexture = true;
        pipeline.supportsCameraOpaqueTexture = true;
        pipeline.supportsHDR = true;
        pipeline.hdrColorBufferPrecision = HDRColorBufferPrecision._32Bits;
        pipeline.msaaSampleCount = 4;
        pipeline.renderScale = 1f;
        pipeline.mainLightShadowmapResolution = 2048;
        pipeline.shadowDistance = 55f;
        pipeline.shadowCascadeCount = 2;
        pipeline.cascade2Split = 0.35f;
        pipeline.supportsDynamicBatching = true;

        GraphicsSettings.defaultRenderPipeline = pipeline;
        QualitySettings.renderPipeline = pipeline;
        QualitySettings.antiAliasing = 4;
        QualitySettings.lodBias = 1f;
        EditorUtility.SetDirty(renderer);
        EditorUtility.SetDirty(pipeline);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
