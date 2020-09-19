using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Index on (Vertex -> Connected vertices)
internal class VertexVerticesIndex
{
    // Vertex representation
    internal class Vertex
    {
        // Connected vertices
        public List<int> Vertices = new List<int>();
    }

    // Index
    List<Vertex> Vertices = new List<Vertex>();

    // Get or set index capacity
    public int Capacity
    {
        get
        {
            return Vertices.Count;
        }
        set
        {
            Vertices.Clear();

            for (int i = 0; i < value; i++)
                Vertices.Add(new Vertex());
        }
    }

    // Register new connected vertex
    public void Add(int vertex, int connected)
    {
        Vertices[vertex].Vertices.Add(connected);
    }

    // Consolidate index removing duplicates
    public void ConsolidateIndex()
    {
        Vertices.ForEach(t => t.Vertices = t.Vertices.Distinct().ToList());
    }

    // List connected vertices with a vertex
    public void ListConnectedVertices(int vertex, List<int> vertices)
    {
        vertices.AddRange(Vertices[vertex].Vertices);
    }

    // List connected vertices with a vertex
    public List<int> ListConnectedVertices(int vertex)
    {
        List<int> vertices = new List<int>();
        ListConnectedVertices(vertex, vertices);
        return vertices;
    }
}

// Class to hold original mesh data, shared among meshes from the same source
internal class MeshCache
{
    // Static cache of meshes data
    static List<MeshCache> Cache = new List<MeshCache>();

    // Get or create mesh cache for given mesh
    static public MeshCache GetMeshCache(Mesh sharedMesh)
    {
        // Linq search
        MeshCache meshCache = Cache.Where(c => c.Mesh == sharedMesh).FirstOrDefault();
        if (meshCache == null)
        {
            // Create a new cache for this mesh and store
            meshCache = new MeshCache(sharedMesh);
            Cache.Add(meshCache);
        }

        return meshCache;
    }

    // Source mesh of cached data
    public Mesh Mesh;

    // Copy of mesh vertices array
    public Vector3[] Vertices;

    // Copy of mesh triangles array
    public int[] Triangles;

    // Cache for mesh normals
    public Vector3[] Normals;

    // Mesh Size
    public Vector3 MeshSize;

    // Normalized size factor
    public Vector3 SizeFactor;

    // Mesh bounds
    public Bounds Bounds;

    // Vertex -> Vertices index (vertices and the connected vertices)
    // Used for normals re-computation
    public VertexVerticesIndex VertexVerticesIndex = new VertexVerticesIndex();

    // Constructor
    MeshCache(Mesh mesh)
    {
        Mesh = mesh;

        // Cache mesh data
        Vertices = mesh.vertices;
        Triangles = mesh.triangles;
        Normals = mesh.normals;

        // Assemble Vertex -> Vertices index (vertex and the connected vertices)
        VertexVerticesIndex.Capacity = Vertices.Length;
        for (int v = 0; v < Vertices.Length; v++)
        {
            for (int t = 0; t < Triangles.Length / 3; t++)
            {
                int i = t * 3;
                if ((Triangles[i++] == v) || (Triangles[i++] == v) || (Triangles[i++] == v)) // weird but works =)
                {
                    i = t * 3;
                    int tv = Triangles[i++];
                    if (tv != v)
                        VertexVerticesIndex.Add(v, tv);
                    tv = Triangles[i++];
                    if (tv != v)
                        VertexVerticesIndex.Add(v, tv);
                    tv = Triangles[i++];
                    if (tv != v)
                        VertexVerticesIndex.Add(v, tv);
                }
            }
        }
        VertexVerticesIndex.ConsolidateIndex();

        // Store mesh bounds
        Bounds = mesh.bounds;

        // Mesh size for deformation constraints
        MeshSize = mesh.bounds.size;
        MeshSize.x = Mathf.Max(MeshSize.x, 0.1f);
        MeshSize.y = Mathf.Max(MeshSize.y, 0.1f);
        MeshSize.z = Mathf.Max(MeshSize.z, 0.1f);

        // Calculate size factor
        float max = Mathf.Max(MeshSize.x, MeshSize.y, MeshSize.z);
        SizeFactor = new Vector3(1 / (MeshSize.x / max), 1 / (MeshSize.y / max), 1 / (MeshSize.z / max));
    }

    // Find all connected vertices 
    public List<int> FindConnectedVertices(List<int> vertices)
    {
        // Create temp lists
        List<int> connectedVertices = new List<int>();

        // List connected vertices
        foreach (int vertex in vertices)
            VertexVerticesIndex.ListConnectedVertices(vertex, connectedVertices);

        // Store result
        vertices = connectedVertices.ToList();
        connectedVertices.Clear();

        // Expand one more pass
        foreach (int vertex in vertices)
            VertexVerticesIndex.ListConnectedVertices(vertex, connectedVertices);

        return connectedVertices;
    }
}

// Mesh deformation by impact behaviour
public class ImpactDeformable : MonoBehaviour
{
    // Amount of impact resistance of the mesh, default 1
    public float Hardness = 1;

    // Max deformation radius (0 means no limit)
    public float MaxDeformationRadius = 0;

    // Maximum movement allowed for a vertex from its original position in object space (0 means no limit) 
    public float MaxVertexMov = 0;

    // Apply random factor in deformation surface, default false
    public float RandomFactorDeformation = 0;

    // Scale vector applied to deformation force
    // Impact Deformable was built respecting PhysX/Unity scale of 1 = 1 meter
    // If this object is out of scale (cars of 30 meters for example), this can be used to scale deformations proportionally
    // Also, can be used to perform some tricks. (1, 0, 1) for example would disable Y component of deformations while (3, 1, 1) would increase X deformation 3 times
    public Vector3 DeformationsScale = Vector3.one;

    // Controls whether Impact Deformable will request mesh collider update after deformations. Warning: High CPU Usage
    public bool DeformMeshCollider = true;

    // Limit deformation size to fit in mesh bounds
    public bool LimitDeformationToMeshBounds = true;

    // Controls if Impact Deformable will recalculate normal on deformed meshes
    public bool RecalculateNormals = true;

    // Linked master if in a compound collider, used in Unity editor only
    [HideInInspector]
    public ImpactDeformable Master;

    // If there is a master all settings will be inherited by default.
    // This flag disables the above behaviour
    [HideInInspector]
    public bool OverrideMaster;

    // Linked mesh filter with the mesh to deform
    [HideInInspector]
    public MeshFilter MeshFilter;

    public bool isit = true;


    // Associated mesh filter
    MeshFilter deformedMeshFilter;
    // DeformedMesh
    Mesh deformedMesh;
    // Cache extracted from mesh
    MeshCache meshCache;
    // Deformed vertices
    Vector3[] deformedVertices;
    // Deformed normals
    Vector3[] deformedNormals;
    // Mesh collider if any
    MeshCollider meshCollider;
    // List of vertex affected by deformation
    List<int> deformedVerticesIndex = new List<int>();


    // Applying changes in mesh flag to avoid more than one update / frame
    bool applyingChanges = false;

    void Awake()
    {
        // Find if this instance have a master in a compound collider scheme
        ImpactDeformable master = FindMaster();
        if (master != null)
        {
            // Load data from master if not override flag
            if (!OverrideMaster)
            {
                RecalculateNormals = master.RecalculateNormals;
                Hardness = master.Hardness;
                MaxVertexMov = master.MaxVertexMov;
                RandomFactorDeformation = master.RandomFactorDeformation;
                DeformationsScale = master.DeformationsScale;
                DeformMeshCollider = master.DeformMeshCollider;
            }
        }

        // Search for a mesh filter
        if (MeshFilter == null)
            MeshFilter = GetComponent<MeshFilter>();

        UpdateMeshFilter();

        // Search for a mesh collider
        meshCollider = GetComponent<MeshCollider>();
        InvokeRepeating("checker", 0, 0.1f);


    }

    // Find if this instance have a master in a compound collider scheme	
    public ImpactDeformable FindMaster()
    {
        // If there is a rigidbody here then it is a physical object on its own
        if (GetComponent<Rigidbody>() != null)
            return null;

        // A master must have impact deformable and a rigidbody
        Transform parent = transform.parent;
        while (parent != null)
        {
            ImpactDeformable master = parent.GetComponent<ImpactDeformable>();
            Rigidbody masterBody = parent.GetComponent<Rigidbody>();

            if ((master) && (masterBody))
                return master;

            parent = parent.parent;
        }

        return null;
    }

    // Update mesh filter and mesh data if changed
    bool UpdateMeshFilter()
    {
        // Check change
        if (deformedMeshFilter == MeshFilter)
            return deformedMesh != null;

        deformedMeshFilter = MeshFilter;

        if (deformedMeshFilter == null)
        {
            meshCache = null;
            deformedMesh = null;
            deformedVertices = null;
            deformedNormals = null;

            return false;
        }
        else
        {
            // Collect mesh data and cache
            meshCache = MeshCache.GetMeshCache(deformedMeshFilter.sharedMesh);
            deformedMesh = deformedMeshFilter.mesh;
            deformedVertices = deformedMesh.vertices;
            deformedNormals = deformedMesh.normals;

            return true;
        }
    }

    void OnCollisionEnter(Collision col)
    {
        isit = false;
        ProcessCollision(col);
        Debug.Log("STARTED");
    }

    void OnCollisionStay(Collision col)
    {
        ProcessCollision(col);
        Debug.Log("IN STAY");
    }

    void OnCollisionExit(Collision col)
    {
        isit = true;
        Debug.Log("NOT TOUCHING");
    }


    // Process a collision message from Unity
    void ProcessCollision(Collision col)
    {
        // For each contact point
        for (int c = 0; c < col.contacts.Length; c++)
            // Check if there are a impact deformable at source collider
            // The reason behind this is that Unity never sends OnCollisionX events to children at a compound collider structure
            (col.contacts[c].thisCollider.GetComponent<ImpactDeformable>() ?? this)
                // Process collision point
                .ProcessContactPoint(col.contacts[c].point, col.relativeVelocity, col.contacts[c].normal);
    }

    // Process collision point for deformation
    void ProcessContactPoint(Vector3 point, Vector3 relativeVelocity, Vector3 normal)
    {
        // Check mesh filter
        if (!UpdateMeshFilter())
            return;

        // Ignore weak collisions
        if (relativeVelocity.sqrMagnitude / Mathf.Max(Hardness, 0.01f) < 0.25)
            return;

        // Check dot product between normal and collision velocity (a way to estimate collision force)
        float dot = Vector3.Dot(relativeVelocity.normalized, normal);
        if (dot <= 0)
            return;

        // Calculate collision "force" vector
        // 0.02 constant is to model force:deformation ratio in 10 m/s = 0.2 meters of deformation (on hardness level 1)
        Vector3 impactForce = dot * relativeVelocity.magnitude * normal * 0.02f;

        // Perform deformation
        Deform(point, impactForce);
    }

    // Process mesh deformation at point by force
    public void Deform(Vector3 point, Vector3 force)
    {
        // Check mesh filter
        if (!UpdateMeshFilter())
            return;
        Repair(0.06f);


        // Transform impact point to mesh space
        point = MeshFilter.transform.InverseTransformPoint(point);

        // Check min hardness value
        if (Hardness < 0.01f)
            Hardness = 0.01f;

        // Check RandomFactorDeformation bounds
        RandomFactorDeformation = Mathf.Clamp01(RandomFactorDeformation);

        // Transform force vector to mesh space and apply hardness
        force = MeshFilter.transform.InverseTransformDirection(force) * (1 / Hardness);

        // Apply scale compensation
        if (MeshFilter.transform.localScale != Vector3.one)
        {
            Vector3 scale = MeshFilter.transform.localScale;
            force.Scale(new Vector3(1 / scale.x, 1 / scale.y, 1 / scale.z));
        }

        // Extract force magnitude and limit with max deformation radius (if any)
        float forceMagnitude = force.magnitude;
        if (MaxDeformationRadius > 0)
            forceMagnitude = Mathf.Min(forceMagnitude, MaxDeformationRadius);

        // Ignore weak impacts
        if (forceMagnitude < 0.025f)
            return;

        // For sqrMagnitude avoiding slow sqrt
        forceMagnitude *= forceMagnitude;

        // Clear list of affected vertex on deformation
        deformedVerticesIndex.Clear();

        int count = deformedVertices.Length - 1;
        int v = 0;

        bool deformed = false;

        // Get local deformation scale (mesh space)
        Vector3 localDeformationsScale = MeshFilter.transform.InverseTransformVector(DeformationsScale);

        // Deformation loop
        while (count > 0)
        {
            v = count;

            // Get vertex
            Vector3 p = deformedVertices[v];

            // Vertex distance from impact point
            float d = (p - point).sqrMagnitude;

            // If inside blast area
            if (d <= forceMagnitude)
            {
                deformed = true;

                // Calculate deformation
                Vector3 deformation = force * (1 - (d / forceMagnitude));

                // Apply random factor (if any)
                if (RandomFactorDeformation > 0)
                    deformation = (deformation * (1 - RandomFactorDeformation)) + (deformation * UnityEngine.Random.value * RandomFactorDeformation);

                // Apply deformation scale (if any)
                if (DeformationsScale != Vector3.one)
                    deformation.Scale(localDeformationsScale);

                // Apply vertex deformation by it's distance from impact + random noise (if any)                
                deformedVertices[v] += deformation;

                // Limit deformation to mesh bounds (if configured)
                if (LimitDeformationToMeshBounds)
                    if (!meshCache.Bounds.Contains(deformedVertices[v]))
                        deformedVertices[v] = meshCache.Bounds.ClosestPoint(deformedVertices[v]);

                // Apply vertex movement limit, if any
                if (MaxVertexMov > 0)
                {
                    Vector3 n = deformedVertices[v] - meshCache.Vertices[v];
                    if (n.sqrMagnitude > MaxVertexMov * MaxVertexMov)
                        deformedVertices[v] = meshCache.Vertices[v] + (n * (MaxVertexMov / n.magnitude));
                }

                // Mark vertex's triangles as affected by deformation (if custom normal recalculation selected)
                deformedVerticesIndex.Add(v);
            }
            count--;
        }
        ImmediateApplyChanges();

    }


    // Apply deformation data to mesh
    void ImmediateApplyChanges(bool applyDeformedNormals = false)
    {
        // Apply deformed vertex data into mesh
        deformedMesh.MarkDynamic();

        deformedMesh.vertices = deformedVertices;

        // Update mesh bounds
        deformedMesh.RecalculateBounds();

        // Deform mesh collider if selected
        if (DeformMeshCollider)
        {
            if (meshCollider != null)
            {
                meshCollider.sharedMesh = null;
                meshCollider.sharedMesh = deformedMesh;
                meshCollider.sharedMesh.UploadMeshData(false);
            }
        }

        // Recalculate normals RecalculateNormals
        if (RecalculateNormals)
        {
            if (!applyDeformedNormals)
                RecalcNormalsForDeformedVertices();
            else
                deformedMesh.normals = deformedNormals;
        }

        // Request upload mesh data
        deformedMesh.UploadMeshData(false);
    }

    // Deformed vertices normal recalculation
    void RecalcNormalsForDeformedVertices()
    {
        List<int> affectedVertexList = meshCache.FindConnectedVertices(deformedVerticesIndex);

        deformedMesh.RecalculateNormals();
        Vector3[] recalculatedNormals = deformedMesh.normals;

        foreach (int vertex in affectedVertexList)
            deformedNormals[vertex] = recalculatedNormals[vertex];

        deformedMesh.normals = deformedNormals;
        deformedVerticesIndex.Clear();
    }

    public void checker()
    {
        if (isit)
        {
            Repair(0.7f);
        }
        
    }
    // Repair mesh to original state
    public void Repair(float percentual, Vector3? point = null, float? radius = null)
    {
        // Check mesh filter
        if (!UpdateMeshFilter())
            return;

        bool repaired = false;

        // Clear list of affected vertex on deformation

        // Transform world point to mesh local
        Vector3 p = Vector3.zero;
        float r = 0;
        if ((point != null) && (radius != null))
        {
            r = (float)radius;
            r *= r;
            p = MeshFilter.transform.InverseTransformPoint((Vector3)point);
        }

        // Repair process on each vertex
        for (int v = 0; v < deformedVertices.Length; v++)
        {
            // Return normal in direction of cache state
            deformedNormals[v] = Vector3.Lerp(deformedNormals[v], meshCache.Normals[v], percentual);

            // How much vertex is deformed
            Vector3 d = deformedVertices[v] - meshCache.Vertices[v];

            if (d != Vector3.zero)
            {
                // If we have a repair radius, ignore vertices out of it
                if (r > 0)
                {
                    if ((deformedVertices[v] - p).sqrMagnitude >= r)
                        continue;
                }

                // Repair vertex 
                deformedVertices[v] = Vector3.Lerp(deformedVertices[v], meshCache.Vertices[v], percentual);
                repaired = true;
            }
        }

        // Reflect repair on mesh
        if (repaired)
            ImmediateApplyChanges(true);
    }
}