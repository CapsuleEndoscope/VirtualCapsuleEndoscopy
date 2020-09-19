using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Pinwheel.MeshToFile;
using System.Text;
using System.IO;

namespace Pinwheel.MeshToFile
{
    public class MeshToObjSaver : IMeshSaver
    {
        private const string MTL_FILE_NAME_SUFFIX = "Material";
        private const string DEFAULT_MATERIAL_NAME = "Default";
        private const string DEFAULT_MESH_NAME = "Mesh";
        private Mesh mesh;
        private Material material;
        private bool multipleMesh;
        private Mesh[] meshes;
        private string multipleMeshFileName;

        public void Save(Mesh m, Material mat, string path)
        {
            mesh = m;
            material = mat;

            string objFileContent = GetObjFileContent();
            string mtlFileContent = GetMtlFileContent();
            try
            {
                string objFilePath = Path.Combine(path, GetObjFileName());
                File.WriteAllText(objFilePath, objFileContent);

                if (material != null)
                {
                    string mtlFilePath = Path.Combine(path, GetMtlFileName());
                    File.WriteAllText(mtlFilePath, mtlFileContent);
                }

                if (material != null && material.mainTexture != null)
                {
                    string texturePath = AssetDatabase.GetAssetPath(material.mainTexture);
                    string newTexturePath = Path.Combine(path, GetMainTextureFileName());
                    File.Copy(texturePath, newTexturePath, true);
                }
            }
            catch (System.IO.IOException e)
            {
                Debug.Log(e.ToString());
            }
        }

        public string Save(Mesh[] meshes, string path, string fileName)
        {
            mesh = null;
            material = null;
            multipleMesh = true;
            this.meshes = meshes;
            multipleMeshFileName = fileName;

            string objFileContent = GetObjFileContent();
            try
            {
                string objFilePath = Path.Combine(path, GetObjFileName());
                File.WriteAllText(objFilePath, objFileContent);
                return objFilePath;
            }
            catch (System.IO.IOException e)
            {
                Debug.Log(e.ToString());
            }
            return null;
        }

        public string GetObjFileContent()
        {
            Vector3[] vertices;
            Vector3[] normals;
            Vector2[] uvCoords;
            int[] triangles;
            string meshName;

            if (multipleMesh)
            {
                List<Vector3> vertList = new List<Vector3>();
                List<Vector3> normalList = new List<Vector3>();
                List<Vector2> uvList = new List<Vector2>();
                List<int> trisList = new List<int>();

                for (int i = 0; i < meshes.Length; ++i)
                {
                    Mesh m = meshes[i];
                    int currentTrisIndex = trisList.Count;
                    int[] tris = m.triangles;
                    for (int t = 0; t < tris.Length; ++t)
                    {
                        trisList.Add(tris[t] + currentTrisIndex);
                    }

                    vertList.AddRange(m.vertices);
                    normalList.AddRange(m.normals);
                    uvList.AddRange(m.uv);
                }

                vertices = vertList.ToArray();
                normals = normalList.ToArray();
                uvCoords = uvList.ToArray();
                triangles = trisList.ToArray();
                meshName = multipleMeshFileName;
            }
            else
            {
                vertices = mesh.vertices;
                normals = mesh.normals;
                uvCoords = mesh.uv;
                triangles = mesh.triangles;
                meshName = mesh.name;
            }

            string header = CreateHeader(meshName);
            string verticesList = ListVertices(vertices);
            string textureCoordsList = ListTextureCoords(uvCoords);
            string normalsList = ListNormals(normals);
            string materialRefs = material ? ListMaterialRefs() : string.Empty;
            string facesList = ListFaces(triangles, vertices.Length, normals.Length, uvCoords.Length);

            string s = new StringBuilder()
                .Append(header)
                .Append(verticesList)
                .Append(textureCoordsList)
                .Append(normalsList)
                .Append(materialRefs)
                .Append(facesList)
                .ToString();
            return s;
        }

        private string CreateHeader(string meshName)
        {
            StringBuilder sb = new StringBuilder()
                .Append("# ").Append(VersionInfo.ProductNameAndVersion).Append("\n")
                .Append("# http://pinwheel.studio").Append("\n")
                .AppendFormat("o {0}", meshName).Append("\n");
            return sb.ToString();
        }

        private string ListVertices(Vector3[] vertices)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("# Vertices list").Append("\n");
            for (int i = 0; i < vertices.Length; ++i)
            {
                Vector3 v = vertices[i];
                sb.AppendFormat("v {0} {1} {2}", -v.x, v.y, v.z).Append("\n");
            }
            return sb.ToString();
        }

        private string ListTextureCoords(Vector2[] uvCoords)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("# Texture coordinates list").Append("\n");
            for (int i = 0; i < uvCoords.Length; ++i)
            {
                Vector2 uv = uvCoords[i];
                sb.AppendFormat("vt {0} {1} 0", uv.x, uv.y).Append("\n");
            }
            return sb.ToString();
        }

        private string ListNormals(Vector3[] normals)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("# Normal vectors list").Append("\n");
            for (int i = 0; i < normals.Length; ++i)
            {
                Vector3 n = normals[i];
                sb.AppendFormat("vn {0} {1} {2}", n.x, n.y, n.z).Append("\n");
            }
            return sb.ToString();
        }

        private string ListMaterialRefs()
        {
            StringBuilder sb = new StringBuilder()
                .Append("# Material file references").Append("\n")
                .AppendFormat("mtllib {0}", GetMtlFileName()).Append("\n")
                .AppendFormat("usemtl {0}", DEFAULT_MATERIAL_NAME).Append("\n");
            return sb.ToString();
        }

        private string ListFaces(int[] triangles, int vLength, int nLength, int uvLength)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("# Faces list").Append("\n");
            for (int i = 0; i < triangles.Length - 2; i += 3)
            {
                sb.AppendFormat("f {0}/{1}/{2} {3}/{4}/{5} {6}/{7}/{8}",
                    (triangles[i + 2] + 1).ToString(),
                    vLength == nLength ? (triangles[i + 2] + 1).ToString() : "",
                    vLength == uvLength ? (triangles[i + 2] + 1).ToString() : "",
                    (triangles[i + 1] + 1).ToString(),
                    vLength == nLength ? (triangles[i + 1] + 1).ToString() : "",
                    vLength == uvLength ? (triangles[i + 1] + 1).ToString() : "",
                    (triangles[i + 0] + 1).ToString(),
                    vLength == nLength ? (triangles[i + 0] + 1).ToString() : "",
                    vLength == uvLength ? (triangles[i + 0] + 1).ToString() : "")
                    .Append("\n");
            }
            return sb.ToString();
        }

        private string GetObjFileName()
        {
            if (multipleMesh)
            {
                return string.Format("{0}{1}", !string.IsNullOrEmpty(multipleMeshFileName) ? multipleMeshFileName : DEFAULT_MESH_NAME, ".obj");
            }
            else
            {
                return string.Format("{0}{1}", mesh != null ? mesh.name : DEFAULT_MESH_NAME, ".obj");
            }
        }

        private string GetMtlFileName()
        {
            return string.Format("{0}{1}{2}", mesh != null ? mesh.name : string.Empty, MTL_FILE_NAME_SUFFIX, ".mtl");
        }

        private string GetMainTextureFileName()
        {
            if (material == null || material.mainTexture == null)
                return null;
            string mainTexturePath = AssetDatabase.GetAssetPath(material.mainTexture);
            return Path.GetFileName(mainTexturePath);
        }

        private string GetMtlFileContent()
        {
            if (material == null)
                return GetDefaultMtlFileContent();

            StringBuilder sb = new StringBuilder();
            sb.Append("# Define a new material with default name").Append("\n");
            sb.AppendFormat("newmtl {0}", DEFAULT_MATERIAL_NAME).Append("\n");

            Color mainColor = material.color;
            sb.Append("# Diffuse/Tint color").Append("\n");
            sb.AppendFormat("Kd {0} {1} {2}", mainColor.r, mainColor.g, mainColor.b).Append("\n");
            sb.AppendFormat("d {0}", mainColor.a).Append("\n");
            sb.AppendFormat("Tr {0}", 1 - mainColor.a).Append("\n");

            if (material.mainTexture != null)
            {
                sb.Append("# Diffuse map").Append("\n");
                sb.AppendFormat("map_Kd {0}", GetMainTextureFileName()).Append("\n");
            }

            return sb.ToString();
        }

        private string GetDefaultMtlFileContent()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("# Define a new material with default name").Append("\n");
            sb.AppendFormat("newmtl {0}", DEFAULT_MATERIAL_NAME).Append("\n");

            Color mainColor = Color.white;
            sb.Append("# Diffuse/Tint color").Append("\n");
            sb.AppendFormat("Kd {0} {1} {2}", mainColor.r, mainColor.g, mainColor.b).Append("\n");
            sb.AppendFormat("d {0}", mainColor.a).Append("\n");
            sb.AppendFormat("Tr {0}", 1 - mainColor.a).Append("\n");

            return sb.ToString();
        }
    }
}
