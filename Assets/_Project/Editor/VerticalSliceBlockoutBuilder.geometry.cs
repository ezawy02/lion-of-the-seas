using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public static partial class VerticalSliceBlockoutBuilder
{
    static void LandingBeachSurface(Transform root)
    {
        const int columns = 12;
        const int rows = 5;
        const float width = 70f;
        var vertices = new Vector3[(columns + 1) * (rows + 1)];
        var uv = new Vector2[vertices.Length];
        var triangles = new int[columns * rows * 6];
        for (var row = 0; row <= rows; row++)
        {
            var v = row / (float)rows;
            for (var column = 0; column <= columns; column++)
            {
                var u = column / (float)columns;
                var x = Mathf.Lerp(-width * 0.5f, width * 0.5f, u);
                var waterEdge = 80.0f - x * 0.42f + Mathf.Sin(x * 0.20f) * 2.8f;
                var z = Mathf.Lerp(waterEdge, 116f, v);
                var y = 0.10f + Mathf.Sin(x * 0.31f + z * 0.17f) * 0.035f + v * 0.08f;
                var index = row * (columns + 1) + column;
                vertices[index] = new Vector3(x, y, z);
                uv[index] = new Vector2(u, v);
            }
        }
        for (var row = 0; row < rows; row++)
        for (var column = 0; column < columns; column++)
        {
            var vertex = row * (columns + 1) + column;
            var triangle = (row * columns + column) * 6;
            triangles[triangle] = vertex;
            triangles[triangle + 1] = vertex + columns + 1;
            triangles[triangle + 2] = vertex + 1;
            triangles[triangle + 3] = vertex + 1;
            triangles[triangle + 4] = vertex + columns + 1;
            triangles[triangle + 5] = vertex + columns + 2;
        }
        var mesh = new Mesh { name = "L01_LandingBeach_AuthoredSurface" };
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        var beach = new GameObject("ENV__LandingBeach_AuthoredSurface", typeof(MeshFilter), typeof(MeshRenderer));
        beach.transform.SetParent(root);
        beach.GetComponent<MeshFilter>().sharedMesh = mesh;
        beach.GetComponent<MeshRenderer>().sharedMaterial = NaturalSandMaterial();
        LandingShoreFoam(root);
    }

    static void LandingShoreFoam(Transform root)
    {
        const int segments = 48;
        const float width = 70f;
        var vertices = new Vector3[segments * 2];
        var uv = new Vector2[vertices.Length];
        var colors = new Color[vertices.Length];
        var triangles = new int[(segments - 1) * 6];
        for (var index = 0; index < segments; index++)
        {
            var t = index / (float)(segments - 1);
            var x = Mathf.Lerp(-width * 0.5f, width * 0.5f, t);
            var edge = 80.0f - x * 0.42f + Mathf.Sin(x * 0.20f) * 2.8f;
            var ribbon = 0.55f + Mathf.Sin(x * 0.37f) * 0.16f;
            var vertex = index * 2;
            vertices[vertex] = new Vector3(x, 0.135f, edge - ribbon);
            vertices[vertex + 1] = new Vector3(x, 0.135f, edge + ribbon);
            uv[vertex] = new Vector2(t, 0f);
            uv[vertex + 1] = new Vector2(t, 1f);
            var alpha = 0.72f + Mathf.Sin(index * 1.91f) * 0.18f;
            colors[vertex] = new Color(1f, 1f, 1f, alpha * 0.52f);
            colors[vertex + 1] = new Color(1f, 1f, 1f, alpha);
        }
        for (var index = 0; index < segments - 1; index++)
        {
            var vertex = index * 2;
            var triangle = index * 6;
            triangles[triangle] = vertex;
            triangles[triangle + 1] = vertex + 2;
            triangles[triangle + 2] = vertex + 1;
            triangles[triangle + 3] = vertex + 1;
            triangles[triangle + 4] = vertex + 2;
            triangles[triangle + 5] = vertex + 3;
        }
        var mesh = new Mesh { name = "L01_LandingShoreFoam_Authored" };
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.colors = colors;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        var foam = new GameObject("VFX__LandingShoreFoam_Authored", typeof(MeshFilter), typeof(MeshRenderer));
        foam.transform.SetParent(root);
        foam.GetComponent<MeshFilter>().sharedMesh = mesh;
        foam.GetComponent<MeshRenderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/_Project/Materials/Water/SeaLion_Foam_Primary.mat");
    }

    static Material NaturalSandMaterial()
    {
        var path = MaterialRoot + "L01_NaturalSand.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        var shader = Shader.Find("Sea Lion/Environment/Natural Sand");
        if (shader == null) return material;
        if (material == null)
        {
            material = new Material(shader) { name = "L01_NaturalSand" };
            AssetDatabase.CreateAsset(material, path);
        }
        material.shader = shader;
        material.SetColor("_DryColor", new Color(0.82f, 0.65f, 0.39f, 1f));
        material.SetColor("_WetColor", new Color(0.54f, 0.35f, 0.19f, 1f));
        material.SetFloat("_Variation", 0.14f);
        EditorUtility.SetDirty(material);
        return material;
    }

    static void Cliff(Transform root, Vector3 position, Vector3 scale, Color color) =>
        Primitive(root, "ENV__StraitCliff", PrimitiveType.Cube, position, scale, Mat("StraitStone", color));

    static void Ship(Transform root, string name, Vector3 position, Color hull, Color trim, float size)
    {
        var group = new GameObject(name).transform;
        group.SetParent(root);
        group.position = position;
        Primitive(group, "Hull", PrimitiveType.Capsule, Vector3.zero, new Vector3(size * 2.2f, size, size * 4), Mat(name + "_Hull", hull, 0.1f, 0.4f)).transform.rotation = Quaternion.Euler(90, 0, 0);
        Primitive(group, "Deck", PrimitiveType.Cube, new Vector3(0, size * 0.6f, 0), new Vector3(size * 2.4f, size * 0.3f, size * 4.4f), Mat(name + "_Trim", trim, 0.6f, 0.55f));
        Primitive(group, "Mast", PrimitiveType.Cylinder, new Vector3(0, size * 3.2f, 0), new Vector3(size * 0.15f, size * 3, size * 0.15f), Mat("Wood", new Color(0.25f, 0.11f, 0.04f)));
        var sail = Primitive(group, "Sail", PrimitiveType.Cube, new Vector3(0, size * 4.1f, 0.3f), new Vector3(size * 2.8f, size * 2.6f, size * 0.1f), Mat("IvorySail", Ivory));
        sail.transform.rotation = Quaternion.Euler(0, 0, -8);
    }

    static void Gate(Transform root, string name, Vector3 position, string value, Color color)
    {
        var group = new GameObject(name).transform;
        group.SetParent(root);
        var material = Mat(name + "_Material", color, 0.2f, 0.55f);
        Primitive(group, "Left", PrimitiveType.Cube, position + Vector3.left * 2.4f, new Vector3(0.45f, 4, 0.7f), material);
        Primitive(group, "Right", PrimitiveType.Cube, position + Vector3.right * 2.4f, new Vector3(0.45f, 4, 0.7f), material);
        Primitive(group, "Top", PrimitiveType.Cube, position + Vector3.up * 2, new Vector3(5.2f, 0.45f, 0.7f), material);
        var text = new GameObject("VALUE__" + value).AddComponent<TextMesh>();
        text.transform.SetParent(group);
        text.transform.position = position + new Vector3(0, 0.7f, -0.5f);
        text.transform.rotation = Quaternion.Euler(90, 0, 0);
        text.text = value;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.characterSize = 1.1f;
        text.fontSize = 64;
        text.color = Color.white;
    }

    static void Marker(Transform root, string name, Vector3 position, Color color, Vector3 scale) =>
        Primitive(root, name, PrimitiveType.Cube, position, scale, Mat(name + "_Material", color));

    static void TraversalCraftFormation(Transform root)
    {
        var positions = new[]
        {
            new Vector3(-1.7f, 0.05f, 42f),
            new Vector3(1.4f, 0.05f, 47f),
            new Vector3(-2.6f, 0.05f, 51f),
            new Vector3(2.1f, 0.05f, 56f),
            new Vector3(-0.4f, 0.05f, 61f)
        };
        var headings = new[] { 4f, -5f, 5f, -4f, 1f };
        for (var i = 0; i < positions.Length; i++)
        {
            Model(root, "FRIENDLY__GateCraft_" + i,
                ShipRoot + "L01-SHP-002_Landing_Craft_Optimized.fbx",
                positions[i], Vector3.one * 2.1f, new Vector3(-90, headings[i], 0));
            CraftCrew(root, "CREW__GateCraft_" + i, positions[i] + Vector3.up * 0.2f, headings[i], 3);
            CompactCraftWake(root, "VFX__GateCraftWake_" + i, positions[i], headings[i]);
        }
    }

    static void CraftCrew(Transform root, string name, Vector3 center, float heading, int count)
    {
        for (var index = 0; index < count; index++)
        {
            var column = index % 2;
            var row = index / 2;
            var offset = new Vector3((column - 0.5f) * 0.42f, 0, (row - 0.5f) * 0.55f);
            Model(root, name + "_" + index,
                CharacterRoot + "L01-CHR-002_Friendly_Marine_Rigged_Optimized.fbx",
                center + offset, Vector3.one * 0.36f, new Vector3(0, 180 + heading, 0));
        }
    }

    static void LandingCraftFan(Transform root)
    {
        var positions = new[]
        {
            new Vector3(-6.5f, 0.05f, 48f),
            new Vector3(-1.5f, 0.05f, 51f),
            new Vector3(4.2f, 0.05f, 53f),
            new Vector3(-3.8f, 0.05f, 59f),
            new Vector3(2.2f, 0.05f, 62f),
            new Vector3(7.5f, 0.05f, 66f),
            new Vector3(-0.5f, 0.05f, 70f)
        };
        var headings = new[] { 10f, 5f, -7f, 8f, -4f, -11f, 2f };
        for (var i = 0; i < positions.Length; i++)
        {
            Model(root, "CRAFT__LandingFan_" + i,
                ShipRoot + "L01-SHP-002_Landing_Craft_Optimized.fbx",
                positions[i], Vector3.one * 2.25f, new Vector3(-90, headings[i], 0));
            for (var rider = -1; rider <= 1; rider++)
                Model(root, $"CREW__LandingFan_{i}_{rider}",
                    CharacterRoot + "L01-CHR-002_Friendly_Marine_Rigged_Optimized.fbx",
                    positions[i] + new Vector3(rider * 0.36f, 0.23f, rider == 0 ? 0.15f : -0.25f),
                    Vector3.one * 0.45f, new Vector3(0, 180 + headings[i], 0));
            CompactCraftWake(root, "VFX__LandingCraftWake_" + i, positions[i], headings[i]);
        }
    }

    static void LandingCraftLine(Transform root, Vector3 start, int count, Color color)
    {
        for (var i = 0; i < count; i++)
        {
            var position = start + new Vector3((i - count / 2) * 2.2f, 0, i % 2);
            if (Model(root, "CRAFT__" + i, ShipRoot + "L01-SHP-002_Landing_Craft_Optimized.fbx", position, Vector3.one * 1.4f, new Vector3(-90, 0, 0)) == null)
                Primitive(root, "CRAFT__" + i + "__BLOCKOUT_FALLBACK", PrimitiveType.Capsule, position, new Vector3(0.7f, 0.35f, 1.5f), Mat("LandingCraft", color));
        }
    }

    static void Crowd(Transform root, string name, Vector3 center, int columns, int rows, Color color)
    {
        var group = new GameObject(name).transform;
        group.SetParent(root);
        var material = Mat(name + "_Material", color);
        for (var row = 0; row < rows; row++)
            for (var column = 0; column < columns; column++)
                Primitive(group, $"Unit_{row}_{column}", PrimitiveType.Capsule, center + new Vector3((column - (columns - 1) * 0.5f) * 1.3f, 0, row * 1.35f), new Vector3(0.42f, 0.7f, 0.42f), material);
    }

    static void ModelCrowd(Transform root, string name, Vector3 center, int columns, int rows, string assetPath, float facing)
    {
        var group = new GameObject(name).transform;
        group.SetParent(root);
        for (var row = 0; row < rows; row++)
            for (var column = 0; column < columns; column++)
            {
                var offset = new Vector3((column - (columns - 1) * 0.5f) * 1.3f, 0, row * 1.35f);
                Model(group, $"Unit_{row}_{column}", assetPath, center + offset, Vector3.one * 0.78f, new Vector3(0, facing, 0));
            }
    }

    static void Fortress(Transform root, Vector3 center, float width, float height, Color stone, Color enemy)
    {
        var material = Mat("FortressStone", stone);
        Primitive(root, "FORTRESS__Wall", PrimitiveType.Cube, center + Vector3.up * height * 0.5f, new Vector3(width, height, 3), material);
        for (var x = -1; x <= 1; x += 2)
            Primitive(root, "FORTRESS__Tower", PrimitiveType.Cube, center + new Vector3(x * width * 0.42f, height * 0.65f, 0), new Vector3(6, height * 1.3f, 6), material);
        Marker(root, "FORTRESS__EnemyGate", center + new Vector3(0, height * 0.35f, -1.7f), enemy, new Vector3(7, height * 0.7f, 0.7f));
    }

    static void Boss(Transform root, string name, Vector3 position, Color color, float size)
    {
        var group = new GameObject(name).transform;
        group.SetParent(root);
        Primitive(group, "Body", PrimitiveType.Capsule, position, new Vector3(size, size * 1.5f, size), Mat(name + "_Armor", color, 0.65f, 0.5f));
        Primitive(group, "Shield", PrimitiveType.Cylinder, position + new Vector3(size, 0, -0.2f), new Vector3(size, 0.25f, size), Mat(name + "_Shield", Charcoal, 0.7f, 0.35f)).transform.rotation = Quaternion.Euler(90, 0, 0);
    }

    static void ShoreCannons(Transform root, Vector3 start, int count)
    {
        for (var i = 0; i < count; i++)
            Primitive(root, "HAZARD__ShoreCannon_" + i, PrimitiveType.Cylinder, start + new Vector3(i % 2 * 21, 0, i / 2 * 13), new Vector3(0.8f, 2.2f, 0.8f), Mat("Cannon", Charcoal, 0.75f, 0.35f)).transform.rotation = Quaternion.Euler(90, 0, 0);
    }

    static void MineField(Transform root, Vector3 start)
    {
        for (var i = 0; i < 10; i++)
            Primitive(root, "HAZARD__Mine_" + i, PrimitiveType.Sphere, start + new Vector3((i % 3 - 1) * 2.2f, 0, i / 3 * 3), Vector3.one * 0.7f, Mat("Mine", Charcoal, 0.8f, 0.3f));
    }

    static void Chain(Transform root, Vector3 center, float width)
    {
        for (var i = 0; i < 18; i++)
        {
            var link = Primitive(root, "OBJECTIVE__ChainLink_" + i, PrimitiveType.Capsule, center + Vector3.right * (-width * 0.5f + i * width / 17), new Vector3(0.35f, 0.7f, 0.35f), Mat("Chain", Copper, 0.8f, 0.35f));
            link.transform.rotation = Quaternion.Euler(0, 0, i % 2 == 0 ? 90 : 0);
        }
    }

    static void StormColumns(Transform root)
    {
        for (var i = 0; i < 9; i++)
            InvisibleMarker(root, "STORM__Gust_VFX_MARKER_" + i, new Vector3((i % 3 - 1) * 8, 2, 22 + i * 7));
    }

    static void PowderBoats(Transform root, Vector3 start)
    {
        for (var i = 0; i < 5; i++)
        {
            Primitive(root, "POWDER__Boat_" + i, PrimitiveType.Capsule, start + new Vector3((i - 2) * 2.1f, 0, i % 2), new Vector3(0.7f, 0.35f, 1.5f), Mat("PowderBoat", Gold));
            Primitive(root, "POWDER__Barrel_" + i, PrimitiveType.Cylinder, start + new Vector3((i - 2) * 2.1f, 0.8f, i % 2), new Vector3(0.5f, 0.7f, 0.5f), Mat("PowderBarrel", Copper));
        }
    }

    static void CameraAndLight(Transform root, Vector3 cameraPosition, Vector3 target, bool storm)
    {
        var cameraObject = new GameObject("PORTRAIT_CAMERA__Gameplay");
        cameraObject.transform.SetParent(root);
        cameraObject.transform.position = cameraPosition;
        cameraObject.transform.rotation = Quaternion.LookRotation(target - cameraPosition);
        var camera = cameraObject.AddComponent<Camera>();
        camera.fieldOfView = 42;
        camera.aspect = 9f / 16f;
        camera.clearFlags = storm ? CameraClearFlags.SolidColor : CameraClearFlags.Skybox;
        camera.backgroundColor = storm ? new Color(0.055f, 0.075f, 0.11f) : new Color(0.22f, 0.54f, 0.74f);
        cameraObject.tag = "MainCamera";
        foreach (var canvas in root.GetComponentsInChildren<Canvas>(true))
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera) canvas.worldCamera = camera;

        var lightObject = new GameObject("KEY_LIGHT__Blockout");
        lightObject.transform.SetParent(root);
        lightObject.transform.rotation = Quaternion.Euler(storm ? 28 : 48, storm ? -55 : -28, 0);
        var light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = storm ? 0.78f : 1.15f;
        light.color = storm ? new Color(0.62f, 0.71f, 0.88f) : new Color(1f, 0.93f, 0.82f);
        if (storm)
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.10f, 0.14f, 0.21f);
            RenderSettings.ambientIntensity = 1f;
            RenderSettings.reflectionIntensity = 0.28f;
            RenderSettings.fog = false;
            return;
        }

        RenderSettings.skybox = MediterraneanSky();
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.42f, 0.48f, 0.50f);
        RenderSettings.ambientIntensity = 0.92f;
        RenderSettings.reflectionIntensity = 0.34f;
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.48f, 0.69f, 0.78f);
        RenderSettings.fogDensity = 0.0019f;
    }

    static Material MediterraneanSky()
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(SkyboxMaterialPath);
        var shader = Shader.Find("Sea Lion/Sky/Mediterranean Procedural");
        if (shader == null) return material;
        if (material == null)
        {
            material = new Material(shader) { name = "Level01_MediterraneanSky" };
            AssetDatabase.CreateAsset(material, SkyboxMaterialPath);
        }
        material.shader = shader;
        material.SetColor("_ZenithColor", new Color(0.12f, 0.52f, 0.86f));
        material.SetColor("_HorizonColor", new Color(0.52f, 0.80f, 0.92f));
        material.SetColor("_CloudColor", new Color(0.96f, 0.97f, 0.94f));
        material.SetFloat("_CloudStrength", 0.92f);
        EditorUtility.SetDirty(material);
        return material;
    }
}
