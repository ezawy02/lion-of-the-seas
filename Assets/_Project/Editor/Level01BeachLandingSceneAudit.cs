using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class Level01BeachLandingSceneAudit
{
    const string ScenePath = "Assets/_Project/Scenes/Review/Level01_BeachLanding_REVIEW.unity";
    const string ReportPath = "Artifacts/Local/Approval/Level01BeachLanding/asset_visibility_audit.csv";

    [MenuItem("Lion of the Seas/Audit Level 01 Beach Landing REVIEW Assets")]
    public static void Run()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var camera = Camera.main;
        if (camera == null) throw new MissingComponentException("Beach Landing review camera is missing.");

        var seen = new HashSet<int>();
        var rows = new List<string>
        {
            "scene_object,asset_path,world_x,world_y,world_z,size_x,size_y,size_z,min_y,viewport_min_x,viewport_max_x,viewport_min_y,viewport_max_y,visible,screen_area_percent"
        };
        foreach (var value in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            var instanceRoot = PrefabUtility.GetNearestPrefabInstanceRoot(value);
            if (instanceRoot == null || !seen.Add(instanceRoot.GetInstanceID())) continue;
            var assetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(instanceRoot);
            if (!assetPath.Contains("/L01-")) continue;
            var renderers = instanceRoot.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) continue;
            var bounds = renderers[0].bounds;
            for (var index = 1; index < renderers.Length; index++) bounds.Encapsulate(renderers[index].bounds);
            rows.Add(BuildRow(camera, instanceRoot.name, assetPath, bounds));
        }

        rows.Sort(1, rows.Count - 1, Comparer<string>.Default);
        Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
        File.WriteAllLines(ReportPath, rows, new UTF8Encoding(false));
        Debug.Log($"Beach Landing asset visibility audit wrote {rows.Count - 1} instances to {ReportPath}.");
    }

    static string BuildRow(Camera camera, string name, string assetPath, Bounds bounds)
    {
        var minimum = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        var maximum = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
        var inFront = false;
        for (var x = -1; x <= 1; x += 2)
        for (var y = -1; y <= 1; y += 2)
        for (var z = -1; z <= 1; z += 2)
        {
            var corner = bounds.center + Vector3.Scale(bounds.extents, new Vector3(x, y, z));
            var viewport = camera.WorldToViewportPoint(corner);
            if (viewport.z <= 0f) continue;
            inFront = true;
            minimum = Vector2.Min(minimum, viewport);
            maximum = Vector2.Max(maximum, viewport);
        }
        var visible = inFront && maximum.x >= 0f && minimum.x <= 1f && maximum.y >= 0f && minimum.y <= 1f;
        var clippedMin = Vector2.Max(minimum, Vector2.zero);
        var clippedMax = Vector2.Min(maximum, Vector2.one);
        var area = visible ? Mathf.Max(0f, clippedMax.x - clippedMin.x) * Mathf.Max(0f, clippedMax.y - clippedMin.y) * 100f : 0f;
        return string.Join(",", Escape(name), Escape(assetPath), F(bounds.center.x), F(bounds.center.y),
            F(bounds.center.z), F(bounds.size.x), F(bounds.size.y), F(bounds.size.z), F(bounds.min.y),
            F(minimum.x), F(maximum.x), F(minimum.y),
            F(maximum.y), visible ? "yes" : "no", F(area));
    }

    static string Escape(string value) => "\"" + value.Replace("\"", "\"\"") + "\"";
    static string F(float value) => value.ToString("0.###", CultureInfo.InvariantCulture);
}
