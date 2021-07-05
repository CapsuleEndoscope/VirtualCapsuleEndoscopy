using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IronManager : MonoBehaviour
{

    public Transform SpawnPoint;
    public Vector3 MinSize;
    public Vector3 Size;
    public int TargetCount;
    public bool SpawnFilings = false;
    public GameObject IronFiling;

    void OnValidate()
    {
        if (SpawnFilings)
        {
            SpawnFilings = false;
            if (SpawnPoint == null)
                return;

            Spawn();
        }
    }



    public Vector3 GetTargetPoint()
    {
        var x = Random.Range(MinSize.x * 0.5f, Size.x * 0.5f);
        var y = Random.Range(MinSize.y * 0.5f, Size.y * 0.5f);
        var z = Random.Range(MinSize.z * 0.5f, Size.z * 0.5f);

        x *= Random.Range(0, 2) == 0 ? -1 : 1;
        y *= Random.Range(0, 2) == 0 ? -1 : 1;
        z *= Random.Range(0, 2) == 0 ? -1 : 1;

        //if(Mathf.Abs(x) < MinSize.x || Mathf.Abs(y) < MinSize.y || Mathf.Abs(z) < MinSize.z)
        //{
        //    return GetTargetPoint();
        //}

        var pt = SpawnPoint.transform.TransformPoint(new Vector3(x, y, z));
        return pt;
    }

    public List<Vector3> GetTargetPoints(int count)
    {
        List<Vector3> pts = new List<Vector3>();
        var z = 0.0f;//SpawnPoint.transform.position.y;

        var rows = (int)Mathf.Sqrt(count);
        var cols = rows;

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                var x = i * (Size.x / cols) - (Size.x / 2.0f);
                var y = j * (Size.y / cols) - (Size.y / 2.0f);

                var pt = SpawnPoint.transform.TransformPoint(new Vector3(x, y, z));

                if (Mathf.Abs(x) < MinSize.x/2.0f && Mathf.Abs(y) < MinSize.y/2.0f)// && Mathf.Abs(pt.z) < MinSize.z)
                {
                    continue;
                }
                
                pts.Add(pt);
            }
            //return pt;
        }
        return pts;
    }

    void Spawn()
    {
#if false
        for (int i = 0; i < TargetCount; i++)
        {
            var pos = GetTargetPoint();
            Instantiate(IronFiling, pos, Quaternion.identity, transform);
        }
#else
        var pts = GetTargetPoints(TargetCount);
        var parent = new GameObject();
        parent.name = "Iron Filings";
        parent.transform.parent = transform;
        for (int i = 0; i < pts.Count; i++)
        {
            var pos = pts[i];
            Instantiate(IronFiling, pos, Quaternion.identity, parent.transform);
        }
#endif
        }
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    Color SizeColor = new Color(0.0f, 0.0f, 1.0f, 0.15f);
    Color MinSizeColor = new Color(0.0f, 0.0f, 1.0f, 0.25f);
    void OnDrawGizmos()
    {
        Gizmos.matrix = SpawnPoint.transform.localToWorldMatrix;
        Gizmos.color = MinSizeColor;
        Gizmos.DrawCube(Vector3.zero, MinSize);
        Gizmos.color = SizeColor;
        Gizmos.DrawCube(Vector3.zero, Size);
    }
}
