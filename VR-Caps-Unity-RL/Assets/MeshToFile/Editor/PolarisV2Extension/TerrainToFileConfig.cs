#if UNITY_EDITOR && GRIFFIN
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pinwheel.MeshToFile;
using Pinwheel.Griffin;

namespace Pinwheel.MeshToFile.GriffinExtension
{
    //[CreateAssetMenu(menuName = "Mesh To File/Terrain To File Config")]
    public class TerrainToFileConfig : ScriptableObject
    {
        private static TerrainToFileConfig instance;
        public static TerrainToFileConfig Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<TerrainToFileConfig>("TerrainToFileConfig");
                    if (instance == null)
                    {
                        instance = ScriptableObject.CreateInstance<TerrainToFileConfig>();
                    }
                }
                return instance;
            }
        }

        [SerializeField]
        private GStylizedTerrain terrain;
        public GStylizedTerrain Terrain
        {
            get
            {
                return terrain;
            }
            set
            {
                terrain = value;
            }
        }

        [SerializeField]
        private int lod;
        public int LOD
        {
            get
            {
                return lod;
            }
            set
            {
                lod = value;
            }
        }

        [SerializeField]
        private MeshSaver.FileType fileType;
        public MeshSaver.FileType FileType
        {
            get
            {
                return fileType;
            }
            set
            {
                fileType = value;
            }
        }

        [SerializeField]
        private string directory;
        public string Directory
        {
            get
            {
                return directory;
            }
            set
            {
                directory = value;
            }
        }
    }
}
#endif
