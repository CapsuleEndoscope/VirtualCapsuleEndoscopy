#if UNITY_EDITOR && GRIFFIN && GRIFFIN_MESH_TO_FILE
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Pinwheel.MeshToFile;
using Pinwheel.Griffin;
using System.IO;

namespace Pinwheel.MeshToFile.GriffinExtension
{
    public class TerrainToFileGriffinExtension
    {
        private const string GX_TERRAIN_TO_MESH = "http://bit.ly/2WrSctE";

        public static string GetExtensionName()
        {
            return "Terrain To File";
        }

        public static string GetPublisherName()
        {
            return "Pinwheel Studio";
        }

        public static string GetDescription()
        {
            return
                "Export Polaris terrain meshes to 3D file.";
        }

        public static string GetVersion()
        {
            return "v1.0.0";
        }

        public static void OpenSupportLink()
        {
            OpenEmailEditor(
                "customer@pinwheel.studio",
                "Griffin Extension - Terrain To File",
                "YOUR_MESSAGE_HERE");
        }

        public static void OpenEmailEditor(string receiver, string subject, string body)
        {
            string url = string.Format(
                "mailto:{0}" +
                "?subject={1}" +
                "&body={2}",
                receiver,
                subject.Replace(" ", "%20"),
                body.Replace(" ", "%20"));

            Application.OpenURL(url);
        }

        public static void OnGUI()
        {
            TerrainToFileConfig config = TerrainToFileConfig.Instance;
            config.Terrain = EditorGUILayout.ObjectField("Terrain", config.Terrain, typeof(GStylizedTerrain), true) as GStylizedTerrain;
            int maxLod = 0;
            if (config.Terrain != null && config.Terrain.TerrainData != null)
            {
                maxLod = config.Terrain.TerrainData.Geometry.LODCount;
            }
            List<int> lodValues = new List<int>();
            List<string> lodLabels = new List<string>();
            for (int i = 0; i < maxLod; ++i)
            {
                lodValues.Add(i);
                lodLabels.Add(i.ToString());
            }
            config.LOD = EditorGUILayout.IntPopup("LOD", config.LOD, lodLabels.ToArray(), lodValues.ToArray());
            config.FileType = (MeshSaver.FileType)EditorGUILayout.EnumPopup("File Type", config.FileType);
            string dir = config.Directory;
            EditorCommon.BrowseFolder("Directory", ref dir);
            config.Directory = dir;
            EditorUtility.SetDirty(config);

            GUI.enabled =
                config.Terrain != null &&
                config.Terrain.TerrainData != null &&
                !string.IsNullOrEmpty(config.Directory);

            if (GUILayout.Button("Export"))
            {
                GAnalytics.Record(GX_TERRAIN_TO_MESH);
                Export();
            }
            GUI.enabled = true;
        }

        private static void Export()
        {
            EditorUtility.DisplayProgressBar("Exporting", "Exporting terrain to file...", 1f);
            try
            {
                List<Mesh> meshes = new List<Mesh>();
                TerrainToFileConfig config = TerrainToFileConfig.Instance;
                GStylizedTerrain terrain = config.Terrain;
                GTerrainData terrainData = terrain.TerrainData;
                int gridSize = terrainData.Geometry.ChunkGridSize;
                int lod = config.LOD;

                GTerrainChunk[] chunks = terrain.GetComponentsInChildren<GTerrainChunk>();
                for (int i = 0; i < chunks.Length; ++i)
                {
                    Mesh m = chunks[i].MeshFilterComponent.sharedMesh;
                    if (m != null)
                    {
                        Mesh cloneMesh = Object.Instantiate(m);
                        Matrix4x4 chunkToWorld = chunks[i].transform.localToWorldMatrix;
                        Matrix4x4 worldToTerrain = terrain.transform.worldToLocalMatrix;
                        Matrix4x4 matrix = chunkToWorld * worldToTerrain;
                        TransformMesh(cloneMesh, matrix);
                        meshes.Add(cloneMesh);
                    }
                }
                 
                string fileName = string.Format("{0}_{1}_{2}",
                    config.Terrain.name.Replace(" ", "_"),
                    config.Terrain.GetInstanceID().ToString(),
                    "LOD" + config.LOD);

                GUtilities.EnsureDirectoryExists(config.Directory);
                string filePath = null;
                if (config.FileType == MeshSaver.FileType.Obj)
                {
                    filePath = MeshSaver.SaveObjMultipleMesh(meshes.ToArray(), config.Directory, fileName);
                }
                else if (config.FileType == MeshSaver.FileType.Fbx)
                {
                    filePath = MeshSaver.SaveFbxMultipleMesh(meshes.ToArray(), config.Directory, fileName);
                }

                for (int i = 0; i < meshes.Count; ++i)
                {
                    Object.DestroyImmediate(meshes[i]);
                }

                AssetDatabase.Refresh();

                if (!string.IsNullOrEmpty(filePath))
                {
                    string assetPath = filePath;
                    ModelImporter importer = ModelImporter.GetAtPath(assetPath) as ModelImporter;
                    if (importer != null)
                    {
                        importer.useFileScale = false;
                        importer.weldVertices = false;
                        importer.SaveAndReimport();
                    }

                    Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                    if (asset != null)
                    {
                        Selection.activeObject = asset;
                        EditorGUIUtility.PingObject(asset);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
            }

            EditorUtility.ClearProgressBar();
        }

        private static void TransformMesh(Mesh m, Matrix4x4 matrix)
        {
            Vector3[] vertices = m.vertices;
            for (int i = 0; i < vertices.Length; ++i)
            {
                vertices[i] = matrix.MultiplyPoint(vertices[i]);
            }
            m.vertices = vertices;
        }
    }
}
#endif
