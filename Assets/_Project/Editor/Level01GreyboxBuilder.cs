using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Level01GreyboxBuilder
{
    const string ScenePath = "Assets/_Project/Scenes/Level_01_HundredSails.unity";
    static Material Mat(string name, Color color) { var m = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard")); m.name = "GREYBOX_MAT_" + name; m.color = color; return m; }
    static GameObject Block(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat)
    { var g = GameObject.CreatePrimitive(PrimitiveType.Cube); g.name = name; g.transform.SetParent(parent); g.transform.localPosition = pos; g.transform.localScale = scale; g.GetComponent<Renderer>().sharedMaterial = mat; return g; }
    static GameObject Marker(Transform parent, string name, Vector3 pos, Material mat)
    { var g = GameObject.CreatePrimitive(PrimitiveType.Cylinder); g.name = name; g.transform.SetParent(parent); g.transform.localPosition = pos; g.transform.localScale = new Vector3(1.8f, .35f, 1.8f); g.GetComponent<Renderer>().sharedMaterial = mat; return g; }
    public static void Build()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var root = new GameObject("LEVEL01_GREYBOX__HundredSails").transform;
        var sea = Mat("Sea", new Color(.035f, .28f, .34f)); var friendly = Mat("Friendly", new Color(.05f, .72f, .72f)); var choice = Mat("Choice", new Color(.36f, .22f, .75f)); var hostile = Mat("Hostile", new Color(.62f, .08f, .07f)); var sand = Mat("Beach", new Color(.76f, .58f, .32f));
        Block(root, "GREYBOX_ENV__SeaLane", new Vector3(0, -1, 38), new Vector3(30, 1, 100), sea);
        Block(root, "GREYBOX_ENV__BeachLanding", new Vector3(0, 0, 82), new Vector3(30, .4f, 14), sand);
        Block(root, "GREYBOX_ENV__HarborWall", new Vector3(0, 3, 106), new Vector3(30, 6, 2), hostile);
        Marker(root, "ANCHOR_01_FlagshipLane_Start", new Vector3(0, .5f, 5), friendly);
        Marker(root, "ANCHOR_02_GateChoice_Easy_x4", new Vector3(-7, .5f, 32), choice);
        Marker(root, "ANCHOR_03_GateChoice_Risky_Damage1", new Vector3(7, .5f, 32), choice);
        Block(root, "GREYBOX_GATE__Easy_BreezeOfTheHundredSails", new Vector3(-7, 2, 32), new Vector3(5, 4, 1), choice);
        Block(root, "GREYBOX_GATE__Risky_SharpCrosswind", new Vector3(7, 2, 32), new Vector3(5, 4, 1), choice);
        Marker(root, "ANCHOR_04_PrisonerRescue_Sailmakers", new Vector3(0, .5f, 50), friendly);
        Block(root, "GREYBOX_RESCUE__CaptiveSailmakers_12", new Vector3(0, 1, 50), new Vector3(6, 2, 2), friendly);
        Marker(root, "ANCHOR_05_BeachLanding_Transfer", new Vector3(0, .5f, 78), sand);
        Block(root, "GREYBOX_FIELD__DefenderField", new Vector3(0, 1, 91), new Vector3(22, 2, 12), hostile);
        Marker(root, "ANCHOR_06_HarborGuardian_Entry", new Vector3(0, 2, 104), hostile);
        Block(root, "GREYBOX_BOSS__HarborGuardian", new Vector3(0, 7, 103), new Vector3(8, 10, 3), hostile);
        var cam = new GameObject("PORTRAIT_CAMERA__Level01Opening"); cam.transform.position = new Vector3(0, 54, -62); cam.transform.rotation = Quaternion.Euler(38, 0, 0); var camera = cam.AddComponent<Camera>(); camera.fieldOfView = 42; camera.aspect = 9f / 16f; camera.tag = "MainCamera";
        var light = new GameObject("KEY_LIGHT__Level01Greybox"); light.transform.position = new Vector3(-12, 22, -10); light.transform.rotation = Quaternion.Euler(45, -25, 0); var l = light.AddComponent<Light>(); l.type = LightType.Directional; l.intensity = 1.2f; l.color = new Color(1f, .85f, .65f);
        EditorSceneManager.SaveScene(scene, ScenePath); AssetDatabase.Refresh();
    }
}
