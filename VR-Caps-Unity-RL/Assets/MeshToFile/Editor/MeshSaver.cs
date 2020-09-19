using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Pinwheel.MeshToFile
{
    public static class MeshSaver
    {
        public enum FileType
        {
            Obj, Fbx,
        }

        public static void Save(Mesh mesh, Material mat, string path, string name, FileType fileType)
        {
            if (mesh == null || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(path))
                throw new System.Exception("Invalid mesh infomation");
            Mesh clonedMesh = Object.Instantiate(mesh);
            clonedMesh.name = name;
            IMeshSaver saver = GetSaver(fileType);
            saver.Save(clonedMesh, mat, path);
            AssetDatabase.Refresh();
            Object.DestroyImmediate(clonedMesh);
        }

        public static string SaveFbxMultipleMesh(Mesh[] meshes, string path, string fileName)
        {
            MeshToFbxAsciiSaver saver = new MeshToFbxAsciiSaver();
            return saver.Save(meshes, path, fileName);
        }

        public static string SaveObjMultipleMesh(Mesh[] meshes, string path, string fileName)
        {
            MeshToObjSaver saver = new MeshToObjSaver();
            return saver.Save(meshes, path, fileName);
        }

        public static IMeshSaver GetSaver(FileType type)
        {
            if (type == FileType.Obj)
                return new MeshToObjSaver();
            else if (type == FileType.Fbx)
                return new MeshToFbxAsciiSaver();
            else
                return null;
        }
    }
}
