using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Pinwheel.MeshToFile;
using System.Text;
using System;
using System.IO;

namespace Pinwheel.MeshToFile
{
    public partial class MeshToFbxAsciiSaver : IMeshSaver
    {
        private Mesh mesh;
        private Material material;
        private bool multipleMesh;
        private Mesh[] meshes;
        private string multipleMeshFileName;

        private const string FBX_HEADER_VERSION = "1003";
        private const string FBX_VERSION = "6100";
        private const string DEFAULT_MESH_NAME = "Default";
        private const string MAIN_TEXTURE_NAME_SUFFIX = "_MainTex";

        public void Save(Mesh m, Material mat, string path)
        {
            mesh = m;
            material = mat;
            StringBuilder sb = new StringBuilder()
                .Append(CreateHeader()).Append(Token.EOL)
                .Append(CreateObjectsDefinition()).Append(Token.EOL)
                .Append(CreateObjects()).Append(Token.EOL)
                .Append(CreateObjectConnections()).Append(Token.EOL);
            string fileContent = sb.ToString();
            try
            {
                //copy texture
                if (material != null && material.mainTexture != null)
                {
                    string srcTexturePath = AssetDatabase.GetAssetPath(material.mainTexture);
                    string desTextureName = string.Format("{0}{1}{2}", mesh.name, MAIN_TEXTURE_NAME_SUFFIX, Path.GetExtension(srcTexturePath));
                    string desTexturePath = Path.Combine(path, desTextureName);
                    File.Copy(srcTexturePath, desTexturePath, true);
                }

                string filePath = Path.Combine(path, GetFbxFileName());
                FbxFormatter formatter = new FbxFormatter();
                formatter.BracketStyle = FbxFormatter.OpenCurlyBracketStyle.Inline;
                string[] formattedContent = formatter.Format(fileContent);
                File.WriteAllLines(filePath, formattedContent);
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
            StringBuilder sb = new StringBuilder()
                .Append(CreateHeader()).Append(Token.EOL)
                .Append(CreateObjectsDefinition()).Append(Token.EOL)
                .Append(CreateObjects()).Append(Token.EOL)
                .Append(CreateObjectConnections()).Append(Token.EOL);
            string fileContent = sb.ToString();
            try
            {
                ////copy texture
                //if (material != null && material.mainTexture != null)
                //{
                //    string srcTexturePath = AssetDatabase.GetAssetPath(material.mainTexture);
                //    string desTextureName = string.Format("{0}{1}{2}", mesh.name, MAIN_TEXTURE_NAME_SUFFIX, Path.GetExtension(srcTexturePath));
                //    string desTexturePath = Path.Combine(path, desTextureName);
                //    File.Copy(srcTexturePath, desTexturePath, true);
                //}

                string filePath = Path.Combine(path, GetFbxFileName());
                FbxFormatter formatter = new FbxFormatter();
                formatter.BracketStyle = FbxFormatter.OpenCurlyBracketStyle.Inline;
                string[] formattedContent = formatter.Format(fileContent);
                File.WriteAllLines(filePath, formattedContent);
                return filePath;
            }
            catch (System.IO.IOException e)
            {
                Debug.Log(e.ToString());
            }
            return null;
        }

        private string GetFbxFileName()
        {
            if (!multipleMesh)
                return string.Format("{0}{1}", mesh != null ? mesh.name : DEFAULT_MESH_NAME, ".fbx");
            else
                return string.Format("{0}{1}", mesh != null ? multipleMeshFileName : DEFAULT_MESH_NAME, ".fbx");
        }

        private string CreateHeader()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("; FBX 6.1.0 project file").Append(Token.EOL); //file magic string
            //sb.Append("; ").Append("Low Poly Terrain Pack").Append(Token.EOL);
            sb.Append("; http://pinwheel.studio").Append(Token.EOL);
            sb.Append("; ----------").Append(Token.EOL);

            FbxNode headerExtensionNode = new FbxNode("FBXHeaderExtension", "")
                .AddProperty("FBXHeaderVersion", FBX_HEADER_VERSION)
                .AddProperty("FBXVersion", FBX_VERSION)
                .AddProperty("Creator", string.Format("\"{0}\"", "Pinwheel Studio"));

            DateTime creationTime = DateTime.Now;
            FbxNode creationTimeNode = new FbxNode("CreationTimeStamp", "")
                .AddProperty("Version", "1000")
                .AddProperty("Year", creationTime.Year)
                .AddProperty("Month", creationTime.Month)
                .AddProperty("Day", creationTime.Day)
                .AddProperty("Hour", creationTime.Hour)
                .AddProperty("Minute", creationTime.Minute)
                .AddProperty("Second", creationTime.Second)
                .AddProperty("Millisecond", creationTime.Millisecond);

            FbxNode otherFlagNode = new FbxNode("OtherFlags", "")
                .AddProperty("FlagPLE", 0);

            headerExtensionNode.AddSubNode(creationTimeNode);
            headerExtensionNode.AddSubNode(otherFlagNode);

            sb.Append(headerExtensionNode.ToString());
            return sb.ToString();
        }

        private string CreateObjectsDefinition()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("; Object definition").Append(Token.EOL);
            sb.Append("; ----------").Append(Token.EOL);

            FbxNode typeModelNode = new FbxNode("ObjectType", "\"Model\"")
                .AddProperty("Count", 1);
            FbxNode typeGeometryNode = new FbxNode("ObjectType", "\"Geometry\"")
                .AddProperty("Count", 1);
            FbxNode typeMaterialNode = new FbxNode("ObjectType", "\"Material\"")
                .AddProperty("Count", 1);
            FbxNode typeTextureNode = new FbxNode("ObjectType", "\"Texture\"")
                .AddProperty("Count", 1);
            FbxNode typeGlobalSettingsNode = new FbxNode("ObjectType", "\"GlobalSettings\"")
                .AddProperty("Count", 1);

            FbxNode definitionNode = new FbxNode("Definitions", "")
                .AddProperty("Version", 100)
                .AddProperty("Count", 3)
                .AddSubNode(typeModelNode)
                .AddSubNode(typeGeometryNode)
                .AddSubNode(typeMaterialNode)
                .AddSubNode(typeTextureNode)
                .AddSubNode(typeGlobalSettingsNode);

            sb.Append(definitionNode.ToString());
            return sb.ToString();
        }

        private string CreateObjects()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("; Objects information").Append(Token.EOL);
            sb.Append("; ----------").Append(Token.EOL);

            FbxNode objectsNode = new FbxNode("Objects", "");
            if (!multipleMesh)
            {
                objectsNode.AddSubNode(CreateModelNode());
            }
            else
            {
                for (int i = 0; i < meshes.Length; ++i)
                {
                    mesh = meshes[i];
                    objectsNode.AddSubNode(CreateModelNode());
                }
            }

            if (material != null)
                objectsNode.AddSubNode(CreateMaterialDefinitionNode());

            if (material != null && material.mainTexture != null)
                objectsNode.AddSubNode(CreateTextureDefinitionNode());

            sb.Append(objectsNode.ToString());
            return sb.ToString();
        }

        private FbxNode CreateModelNode()
        {
            FbxNode modelNode = new FbxNode("Model", string.Format("\"Model::{0}\", \"Mesh\"", mesh.name))
                .AddProperty("Version", 232)
                .AddProperty("MultiLayer", 0)
                .AddProperty("Shading", "Y")
                .AddProperty("Culling", "\"Culling Off\"")
                .AddProperty("Vertices", ListVertices())
                .AddProperty("PolygonVertexIndex", ListTriangles())
                .AddProperty("GeometryVersion", 124)
                .AddSubNode(CreateNormalsNode())
                .AddSubNode(CreateUvNode())
                .AddSubNode(CreateColorsNode())
                .AddSubNode(CreateMaterialNode());

            if (material != null && material.mainTexture != null)
                modelNode.AddSubNode(CreateTextureNode());

            modelNode.AddSubNode(CreateLayerNode());

            return modelNode;
        }

        private FbxNode CreateNormalsNode()
        {
            FbxNode normalsNode = new FbxNode("LayerElementNormal", 0)
                .AddProperty("Version", 101)
                .AddProperty("Name", "\"\"")
                .AddProperty("MappingInformationType", "\"ByVertice\"")
                .AddProperty("ReferenceInformationType", "\"Direct\"")
                .AddProperty("Normals", ListNormals());
            return normalsNode;
        }

        private FbxNode CreateUvNode()
        {
            FbxNode normalsNode = new FbxNode("LayerElementUV", 0)
                .AddProperty("Version", 101)
                .AddProperty("Name", "\"UVMap\"")
                .AddProperty("MappingInformationType", "\"ByVertice\"")
                .AddProperty("ReferenceInformationType", "\"Direct\"")
                .AddProperty("UV", ListUVs());
            return normalsNode;
        }

        private FbxNode CreateMaterialNode()
        {
            FbxNode materialNode = new FbxNode("LayerElementMaterial", 0)
                .AddProperty("Version", 101)
                .AddProperty("Name", "\"\"")
                .AddProperty("MappingInformationType", "\"AllSame\"")
                .AddProperty("ReferenceInformationType", "\"IndexToDirect\"")
                .AddProperty("Materials", 0);
            return materialNode;
        }

        private FbxNode CreateColorsNode()
        {
            FbxNode colorsNode = new FbxNode("LayerElementColor", 0)
                .AddProperty("Version", 101)
                .AddProperty("Name", "\"Color\"")
                .AddProperty("MappingInformationType", "\"ByVertice\"")
                .AddProperty("ReferenceInformationType", "\"Direct\"")
                .AddProperty("Colors", ListColors());
            return colorsNode;
        }

        private FbxNode CreateTextureNode()
        {
            FbxNode textureNode = new FbxNode("LayerElementTexture", 0)
                .AddProperty("Version", 101)
                .AddProperty("Name", "\"\"")
                .AddProperty("MappingInformationType", "\"AllSame\"")
                .AddProperty("ReferenceInformationType", "\"IndexToDirect\"");
            return textureNode;
        }

        private FbxNode CreateLayerNode()
        {
            FbxNode normalLayerElementNode = new FbxNode("LayerElement", "")
                .AddProperty("Type", "\"LayerElementNormal\"")
                .AddProperty("TypedIndex", 0);
            FbxNode uvLayerElementNode = new FbxNode("LayerElement", "")
                .AddProperty("Type", "\"LayerElementUV\"")
                .AddProperty("TypedIndex", 0);
            FbxNode colorLayerElementNode = new FbxNode("LayerElement", "")
                .AddProperty("Type", "\"LayerElementColor\"")
                .AddProperty("TypedIndex", 0);
            FbxNode materialLayerElementNode = new FbxNode("LayerElement", "")
                .AddProperty("Type", "\"LayerElementMaterial\"")
                .AddProperty("TypedIndex", 0);
            FbxNode textureLayerElementNode = new FbxNode("LayerElement", "")
                .AddProperty("Type", "\"LayerElementTexture\"")
                .AddProperty("TypedIndex", 0);

            FbxNode layerNode = new FbxNode("Layer", 0)
                .AddProperty("Version", 100)
                .AddSubNode(normalLayerElementNode)
                .AddSubNode(uvLayerElementNode)
                .AddSubNode(colorLayerElementNode)
                .AddSubNode(materialLayerElementNode)
                .AddSubNode(textureLayerElementNode);

            return layerNode;
        }

        private FbxNode CreateGlobalSettingsNode()
        {
            FbxNode globalSettingsNode = new FbxNode("GlobalSettings", "")
                .AddProperty("Version", 1000);
            FbxNode globalSettingProperties60Node = new FbxNode("Properties60", "")
                .AddProperty("Property", "\"UpAxis\", \"int\", \"\", 1")
                .AddProperty("Property", "\"UpAxisSign\", \"int\", \"\", 1")
                .AddProperty("Property", "\"FrontAxis\", \"int\", \"\", 2")
                .AddProperty("Property", "\"FrontAxisSign\", \"int\", \"\", 1")
                .AddProperty("Property", "\"CoordAxis\", \"int\", \"\", 0")
                .AddProperty("Property", "\"CoordAxisSign\", \"int\", \"\", 1")
                .AddProperty("Property", "\"UnitScaleFactor\", \"double\", \"\", 1");
            globalSettingsNode.AddSubNode(globalSettingProperties60Node);
            return globalSettingsNode;
        }

        private string ListVertices()
        {
            Vector3[] vertices = mesh.vertices;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < vertices.Length; ++i)
            {
                if (sb.Length != 0)
                    sb.Append(",");
                Vector3 v = vertices[i];
                sb.AppendFormat("{0},{1},{2} ", -v.x, v.y, v.z);
            }

            return sb.ToString();
        }

        private string ListTriangles()
        {
            int[] triangles = mesh.triangles;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < triangles.Length - 2; i += 3)
            {
                if (sb.Length != 0)
                    sb.Append(",");
                int t0 = triangles[i + 2];
                int t1 = triangles[i + 1];
                int t2 = triangles[i + 0];
                sb.AppendFormat("{0},{1},{2}", t0, t1, t2 ^ (-1));
            }

            return sb.ToString();
        }

        private string ListNormals()
        {
            Vector3[] normals = mesh.normals;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < normals.Length; ++i)
            {
                if (sb.Length != 0)
                    sb.Append(",");
                Vector3 n = normals[i];
                sb.AppendFormat("{0},{1},{2}", n.x, n.y, n.z);
            }
            return sb.ToString();
        }

        private string ListUVs()
        {
            Vector2[] uvs = mesh.uv;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < uvs.Length; ++i)
            {
                if (sb.Length != 0)
                    sb.Append(",");
                Vector2 uv = uvs[i];
                sb.AppendFormat("{0},{1}", uv.x, uv.y);
            }
            return sb.ToString();
        }

        private string ListColors()
        {
            Color[] colors = mesh.colors;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < colors.Length; ++i)
            {
                if (sb.Length != 0)
                    sb.Append(",");
                Color c = colors[i];
                sb.AppendFormat("{0},{1},{2},{3}", c.r, c.g, c.b, c.a);
            }
            return sb.ToString();
        }

        private FbxNode CreateMaterialDefinitionNode()
        {
            Color mainColor = material.color;
            FbxNode materialProperty60Node = new FbxNode("Properties60", "")
                .AddProperty("Property", string.Format("\"Diffuse\", \"ColorRGB\", \"\", {0}, {1}, {2}", 1, 1, 1))
                .AddProperty("Property", string.Format("\"Opacity\", \"double\", \"\", {0}", mainColor.a))
                .AddProperty("Property", string.Format("\"DiffuseColor\", \"ColorRGB\", \"\", {0}, {1}, {2}", mainColor.r, mainColor.g, mainColor.b));

            FbxNode materialNode = new FbxNode("Material", string.Format("\"Material::{0}\", \"\"", mesh.name))
                .AddProperty("Version", 102)
                .AddProperty("ShadingModel", "\"lambert\"")
                .AddProperty("MultiLayer", 0)
                .AddSubNode(materialProperty60Node);
            return materialNode;
        }

        private FbxNode CreateTextureDefinitionNode()
        {
            FbxNode textureNode = new FbxNode("Texture", string.Format("\"Texture::{0}{1}\"", mesh.name, MAIN_TEXTURE_NAME_SUFFIX))
                .AddProperty("Type", "\"TextureVideoClip\"")
                .AddProperty("Version", 202)
                .AddProperty("TextureName", string.Format("\"Texture::{0}{1}\"", mesh.name, MAIN_TEXTURE_NAME_SUFFIX))
                .AddProperty("RelativeFilename", string.Format("\"{0}\"", GetDesTextureRelativeFileName()));
            return textureNode;
        }

        private string GetDesTextureRelativeFileName()
        {
            if (material == null || material.mainTexture == null)
                return null;
            string srcTexturePath = AssetDatabase.GetAssetPath(material.mainTexture);
            string desTextureRelativePath = string.Format("/{0}{1}{2}", mesh.name, MAIN_TEXTURE_NAME_SUFFIX, Path.GetExtension(srcTexturePath));
            return desTextureRelativePath;
        }

        private string CreateObjectConnections()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("; Object connections").Append(Token.EOL);
            sb.Append("; ----------").Append(Token.EOL);

            if (!multipleMesh)
            {
                string meshToSceneConnectionString = string.Format("\"OO\", \"Model::{0}\", \"Model::Scene\"", mesh.name);
                string materialToMeshConnectionString = string.Format("\"OO\", \"Material::{0}\", \"Model::{0}\"", mesh.name);
                string textureToMaterialConnectionString = string.Format("\"OP\", \"Texture::{0}{1}\", \"Material::{0}\", \"DiffuseColor\"", mesh.name, MAIN_TEXTURE_NAME_SUFFIX);
                FbxNode connectionsNode = new FbxNode("Connections", "")
                    .AddProperty("Connect", meshToSceneConnectionString)
                    .AddProperty("Connect", materialToMeshConnectionString)
                    .AddProperty("Connect", textureToMaterialConnectionString);

                sb.Append(connectionsNode.ToString());
            }
            else
            {
                FbxNode connectionsNode = new FbxNode("Connections", "");
                for (int i = 0; i < meshes.Length; ++i)
                {
                    mesh = meshes[i];
                    string meshToSceneConnectionString = string.Format("\"OO\", \"Model::{0}\", \"Model::Scene\"", mesh.name);
                    string materialToMeshConnectionString = string.Format("\"OO\", \"Material::{0}\", \"Model::{0}\"", mesh.name);
                    connectionsNode
                        .AddProperty("Connect", meshToSceneConnectionString)
                        .AddProperty("Connect", materialToMeshConnectionString);
                }
                sb.Append(connectionsNode.ToString());
            }
            return sb.ToString();
        }
    }
}
