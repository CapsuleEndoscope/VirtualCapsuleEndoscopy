using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pinwheel.MeshToFile
{
    /// <summary>
    /// Utility class contains product info
    /// </summary>
    public static class VersionInfo
    {
        public static string Code
        {
            get
            {
                return "1.1.0";
            }
        }

        public static string ProductName
        {
            get
            {
                return "Mesh To File";
            }
        }

        public static string ProductNameAndVersion
        {
            get
            {
                return string.Format("{0} v{1}", ProductName, Code);
            }
        }

        public static string ProductNameShort
        {
            get
            {
                return "Mesh To File";
            }
        }

        public static string ProductNameAndVersionShort
        {
            get
            {
                return string.Format("{0} v{1}", ProductNameShort, Code);
            }
        }
    }
}
