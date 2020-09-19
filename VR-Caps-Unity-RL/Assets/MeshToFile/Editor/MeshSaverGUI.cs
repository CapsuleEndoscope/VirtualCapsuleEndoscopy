using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Pinwheel.MeshToFile
{
    public class MeshSaverGUI : EditorWindow
    {
        private MeshFilter target;
        private string meshName;
        private string path;
        private MeshSaver.FileType fileType;
        private bool showAd;

        private readonly string[] EDITOR_PREF_KEYS_PATH = new string[2] { "meshsaver", "path" };
        private readonly string[] EDITOR_PREF_KEYS_SHOW_AD = new string[2] { "meshsaver", "showad" };
        private readonly Vector2 SIZE_LARGE = new Vector2(500, 575);
        private readonly Vector2 SIZE_SMALL = new Vector2(500, 200);

        [MenuItem("Window/Mesh To File")]
        public static void ShowWindow()
        {
            MeshSaverGUI window = GetWindow<MeshSaverGUI>();
            window.titleContent = new GUIContent("Mesh Saver");
            window.Show();
        }

        private void OnEnable()
        {
            path = EditorPrefs.GetString(EditorCommon.GetProjectRelatedEditorPrefsKey(EDITOR_PREF_KEYS_PATH), "Assets/");
            showAd = EditorPrefs.GetBool(EditorCommon.GetProjectRelatedEditorPrefsKey(EDITOR_PREF_KEYS_SHOW_AD), true);
            if (showAd)
            {
                Rect r = position;
                r.size = SIZE_LARGE;
                position = r;
            }
        }

        private void OnDisable()
        {
            EditorPrefs.SetString(EditorCommon.GetProjectRelatedEditorPrefsKey(EDITOR_PREF_KEYS_PATH), path);
            EditorPrefs.SetBool(EditorCommon.GetProjectRelatedEditorPrefsKey(EDITOR_PREF_KEYS_SHOW_AD), showAd);
        }

        private void OnGUI()
        {
            target = EditorGUILayout.ObjectField("Target", target, typeof(MeshFilter), true) as MeshFilter;
            meshName = EditorGUILayout.TextField("Mesh name", meshName);
            fileType = (MeshSaver.FileType)EditorGUILayout.EnumPopup("File type", fileType);
            EditorCommon.BrowseFolder("Path", ref path);
            GUI.enabled =
                target != null &&
                target.sharedMesh != null &&
                !string.IsNullOrEmpty(meshName) &&
                !string.IsNullOrEmpty(path);
            if (EditorCommon.RightAnchoredButton("Save"))
            {
                Material mat = null;
                MeshRenderer mr = target.GetComponent<MeshRenderer>();
                if (mr != null)
                    mat = mr.sharedMaterial;
                MeshSaver.Save(target.sharedMesh, mat, path, meshName, fileType);
            }
            GUI.enabled = true;

            EditorCommon.Separator();
            showAd = EditorGUILayout.Foldout(showAd, "Introducing Polaris Ecosystem - The complete toolset for immersive Low Poly levels.");

            if (showAd)
            {
                if (GUILayout.Button("Polaris - Low Poly Terrain"))
                {
                    Application.OpenURL("https://assetstore.unity.com/packages/tools/terrain/low-poly-terrain-polaris-2020-170400?aid=1100l3QbW&pubref=mesh-to-file");
                }
                if (GUILayout.Button("Poseidon - Low Poly Water"))
                {
                    Application.OpenURL("https://assetstore.unity.com/packages/vfx/shaders/substances/poseidon-low-poly-water-system-builtin-lwrp-153826?aid=1100l3QbW&pubref=mesh-to-file");
                }
                if (GUILayout.Button("Jupiter - Procedural Sky"))
                {
                    Application.OpenURL("https://assetstore.unity.com/packages/2d/textures-materials/sky/procedural-sky-builtin-lwrp-urp-jupiter-159992?aid=1100l3QbW&pubref=mesh-to-file");
                }

                Texture2D bg = Resources.Load<Texture2D>("Background");
                if (bg != null)
                {
                    Rect bgRect = GUILayoutUtility.GetAspectRect(bg.width * 1.0f / bg.height);
                    GUI.DrawTexture(bgRect, bg);
                }
            }
        }
    }
}