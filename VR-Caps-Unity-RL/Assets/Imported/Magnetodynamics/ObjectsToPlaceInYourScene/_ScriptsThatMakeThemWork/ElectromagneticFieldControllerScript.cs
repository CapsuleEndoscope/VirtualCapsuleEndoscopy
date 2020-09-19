using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class ElectromagneticFieldControllerScript : MonoBehaviour {
    // This keeps track of the world's magnetic (and electric) fields and makes sure that we don't let a magnetic dipole exert a force on itself (which would be infinite in classical physics).
    // Also don't allow two dipoles attached to the same rigidbody to exert forces on each other (we can assume that the rigidbody supplies the necessary internal stresses to keep itself from collapsing or flying apart).

    public Vector3 ambientElectricField;
    public Vector3 ambientMagneticField;

    public float colliderPoof = 1.05f;
    public float dampenLanding = 0.5f;
    public int dampenLandingSteps = 10;
    public int collisionLayer = 9;
    
    public float maxMagnetizedForceOverMass = 20.0f;

    // Keep track of external sources of electric and magnetic fields with lists of functions (cannot be deleted).
    private List<Func<Vector3,Vector3> > electricFieldFunctions = new List<Func<Vector3,Vector3> >();
    private List<Func<Vector3,Vector3> > magneticFieldFunctions = new List<Func<Vector3,Vector3> >();
    private List<Func<Vector3,Vector3> > magneticFieldDxFunctions = new List<Func<Vector3,Vector3> >();
    private List<Func<Vector3,Vector3> > magneticFieldDyFunctions = new List<Func<Vector3,Vector3> >();
    private List<Func<Vector3,Vector3> > magneticFieldDzFunctions = new List<Func<Vector3,Vector3> >();

    private List<Func<Vector3,int,Vector3> > superconductorMagneticFieldFunctions = new List<Func<Vector3,int,Vector3> >();
    private List<Func<Vector3,int,Vector3> > superconductorMagneticFieldDxFunctions = new List<Func<Vector3,int,Vector3> >();
    private List<Func<Vector3,int,Vector3> > superconductorMagneticFieldDyFunctions = new List<Func<Vector3,int,Vector3> >();
    private List<Func<Vector3,int,Vector3> > superconductorMagneticFieldDzFunctions = new List<Func<Vector3,int,Vector3> >();
    
    // Keep track of the dipoles' rigidbodies with a list (cannot be deleted) and the dipoles/charges themselves with dictionary (you can delete MagneticDipoles and StaticCharges: they'll be dropped from the list automatically).
    private List<Rigidbody> rigidbodies = new List<Rigidbody>();
    private int staticChargeCount = 0;
    private Dictionary<int,Dictionary<int,Vector3> > staticChargePosition = new Dictionary<int,Dictionary<int,Vector3> >();
    private Dictionary<int,Dictionary<int,float> > staticChargeStrength = new Dictionary<int,Dictionary<int,float> >();
    private int magneticDipoleCount = 0;
    private Dictionary<int,Dictionary<int,Vector3> > magneticDipolePosition = new Dictionary<int,Dictionary<int,Vector3> >();
    private Dictionary<int,Dictionary<int,Vector3> > magneticDipoleMoment = new Dictionary<int,Dictionary<int,Vector3> >();
    private Dictionary<int,bool> magneticDipoleTorqueOnly = new Dictionary<int,bool>();
    private Dictionary<int,GameObject> magneticDipoles = new Dictionary<int,GameObject>();
    
    private Dictionary<int,Dictionary<int,int> > collisionState = new Dictionary<int,Dictionary<int,int> >();

    private bool IsFinite(Vector3 v) {
        return (!float.IsNaN(v.x)  &&  !float.IsNaN(v.y)  &&  !float.IsNaN(v.z)  &&  v.x != Mathf.Infinity  &&  v.y != Mathf.Infinity  &&  v.z != Mathf.Infinity);
    }
    
    private bool IsFinite(float v) {
        return (!float.IsNaN(v)  &&  v != Mathf.Infinity);
    }

    public void RegisterElectricField(Func<Vector3,Vector3> electricFieldFunction) {
        electricFieldFunctions.Add(electricFieldFunction);
    }
    
    public void RegisterMagneticField(Func<Vector3,Vector3> magneticFieldFunction) {
        magneticFieldFunctions.Add(magneticFieldFunction);
    }
    
    public void RegisterMagneticFieldDerivatives(Func<Vector3,Vector3> magneticFieldDxFunction, Func<Vector3,Vector3> magneticFieldDyFunction, Func<Vector3,Vector3> magneticFieldDzFunction) {
        magneticFieldDxFunctions.Add(magneticFieldDxFunction);
        magneticFieldDyFunctions.Add(magneticFieldDyFunction);
        magneticFieldDzFunctions.Add(magneticFieldDzFunction);
    }

    public void RegisterSuperconductorMagneticField(Func<Vector3,int,Vector3> magneticFieldFunction) {
        superconductorMagneticFieldFunctions.Add(magneticFieldFunction);
    }
    
    public void RegisterSuperconductorMagneticFieldDerivatives(Func<Vector3,int,Vector3> magneticFieldDxFunction, Func<Vector3,int,Vector3> magneticFieldDyFunction, Func<Vector3,int,Vector3> magneticFieldDzFunction) {
        superconductorMagneticFieldDxFunctions.Add(magneticFieldDxFunction);
        superconductorMagneticFieldDyFunctions.Add(magneticFieldDyFunction);
        superconductorMagneticFieldDzFunctions.Add(magneticFieldDzFunction);
    }

    public void AssignStaticChargeId(GameObject charge, Rigidbody parentRigidbody, Collider parentCollider, float poof, out int rigidbodyId, out int staticChargeId) {
        if (poof < 0.0f) { poof = colliderPoof; }
    
        if (rigidbodies.Contains(parentRigidbody)) {
            rigidbodyId = 0;
            while (rigidbodies[rigidbodyId] != parentRigidbody) { rigidbodyId++; }
        }
        else {
            rigidbodyId = rigidbodies.Count;
            rigidbodies.Add(parentRigidbody);
            AddRigidBody(charge, parentRigidbody, parentCollider, rigidbodyId, poof);
        }
        staticChargeId = (staticChargeCount++);
    }

    public void AssignMagneticDipoleId(GameObject dipole, Rigidbody parentRigidbody, Collider parentCollider, float poof, bool applyTorqueOnly, out int rigidbodyId, out int magneticDipoleId) {
        if (poof < 0.0f) { poof = colliderPoof; }

        if (rigidbodies.Contains(parentRigidbody)) {
            rigidbodyId = 0;
            while (rigidbodies[rigidbodyId] != parentRigidbody) { rigidbodyId++; }
        }
        else {
            rigidbodyId = rigidbodies.Count;
            rigidbodies.Add(parentRigidbody);
            AddRigidBody(dipole, parentRigidbody, parentCollider, rigidbodyId, poof);
        }
        magneticDipoleId = (magneticDipoleCount++);
        
        magneticDipoleTorqueOnly[magneticDipoleId] = applyTorqueOnly;
        magneticDipoles[magneticDipoleId] = dipole;
    }
    
    void AddRigidBody(GameObject obj, Rigidbody parentRigidbody, Collider parentCollider, int rigidbodyId, float poof) {
        staticChargePosition[rigidbodyId] = new Dictionary<int,Vector3>();
        staticChargeStrength[rigidbodyId] = new Dictionary<int,float>();
        magneticDipolePosition[rigidbodyId] = new Dictionary<int,Vector3>();
        magneticDipoleMoment[rigidbodyId] = new Dictionary<int,Vector3>();

        // When two magnetized rigidbodies collide, we have to turn off the magnetic interaction, lest the singularities get too close and the bodies experience near-infinite forces.  This creates trigger-colliders so that the dipoles can inform us if they're touching.  These colliders are slightly larger (e.g. 5% larger) than the rigidbody's collider, so that we can shut off the magnetic force *before* the contact forces take over.
        obj.layer = collisionLayer;
        if (parentCollider.gameObject.GetComponent<Collider>() is BoxCollider) {
            BoxCollider triggerCollider = obj.AddComponent<BoxCollider>();
            triggerCollider.center = obj.transform.InverseTransformPoint(obj.transform.parent.TransformPoint(((BoxCollider)(parentCollider)).center));
            triggerCollider.size = obj.transform.InverseTransformDirection(obj.transform.parent.TransformDirection(((BoxCollider)(parentCollider)).size)) * poof;
            triggerCollider.isTrigger = true;
        }
        else if (parentCollider is SphereCollider) {
            SphereCollider triggerCollider = obj.AddComponent<SphereCollider>();
            triggerCollider.center = obj.transform.InverseTransformPoint(obj.transform.parent.TransformPoint(((SphereCollider)(parentCollider)).center));
            Vector3 scaling = obj.transform.InverseTransformPoint(obj.transform.parent.TransformPoint(Vector3.one));
            triggerCollider.radius = ((SphereCollider)(parentCollider)).radius * Mathf.Max(scaling.x, scaling.y, scaling.z) * poof;
            triggerCollider.isTrigger = true;
        }
        else if (parentCollider is CapsuleCollider) {
            CapsuleCollider triggerCollider = obj.AddComponent<CapsuleCollider>();
            triggerCollider.center = obj.transform.InverseTransformPoint(obj.transform.parent.TransformPoint(((CapsuleCollider)(parentCollider)).center));
            triggerCollider.direction = ((CapsuleCollider)(parentCollider)).direction;

            Vector3 linear;
            Vector3 round1;
            Vector3 round2;
            if (triggerCollider.direction == 0) {
                linear = Vector3.right;
                round1 = Vector3.up;
                round2 = Vector3.forward;
            }
            else if (triggerCollider.direction == 1) {
                round1 = Vector3.right;
                linear = Vector3.up;
                round2 = Vector3.forward;
            }
            else {
                round1 = Vector3.right;
                round2 = Vector3.up;
                linear = Vector3.forward;
            }
            linear = obj.transform.InverseTransformDirection(obj.transform.parent.TransformDirection(linear));
            round1 = obj.transform.InverseTransformDirection(obj.transform.parent.TransformDirection(round1));
            round2 = obj.transform.InverseTransformDirection(obj.transform.parent.TransformDirection(round2));

            triggerCollider.height = ((CapsuleCollider)(parentCollider)).height * linear.magnitude * poof;
            triggerCollider.radius = ((CapsuleCollider)(parentCollider)).radius * Mathf.Max(round1.magnitude, round2.magnitude) * poof;
            triggerCollider.isTrigger = true;
        }
        else if (parentCollider is MeshCollider) {
            MeshCollider triggerCollider = obj.AddComponent<MeshCollider>();
            
            Mesh oldMesh = ((MeshCollider)(parentCollider)).sharedMesh;
            Vector3[] oldVertices = oldMesh.vertices;
            Vector2[] oldUV = oldMesh.uv;
            int[] oldTriangles = oldMesh.triangles;
                        
            Mesh mesh = new Mesh();
            Vector3[] newVertices = new Vector3[oldVertices.Length];
            Vector2[] newUV = new Vector2[oldUV.Length];
            int[] newTriangles = new int[oldTriangles.Length];
            
            for (int i = 0;  i < oldVertices.Length;  i++) {
                newVertices[i] = obj.transform.InverseTransformPoint(obj.transform.parent.TransformPoint(oldVertices[i]));
            }
            for (int i = 0;  i < oldUV.Length;  i++) {
                newUV[i] = oldUV[i];
            }
            for (int i = 0;  i < oldTriangles.Length;  i++) {
                newTriangles[i] = oldTriangles[i];
            }
            mesh.vertices = newVertices;
            mesh.uv = newUV;
            mesh.triangles = newTriangles;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
    
            // now "poof" it out like the other collider types
            float size = Mathf.Max(mesh.bounds.size.x, mesh.bounds.size.y, mesh.bounds.size.z);
            Vector3[] vertices = mesh.vertices;
            Vector3[] normals = mesh.normals;
            for (int i = 0;  i < newVertices.Length;  i++) {
                vertices[i] += normals[i] * (poof - 1.0f) * size;
            }
            mesh.vertices = vertices;
            triggerCollider.sharedMesh = mesh;
            
            triggerCollider.convex = ((MeshCollider)(parentCollider)).convex;
            triggerCollider.smoothSphereCollisions = ((MeshCollider)(parentCollider)).smoothSphereCollisions;
            triggerCollider.isTrigger = true;
        }
        else {
            throw new System.Exception("StaticCharges and MagneticDipoles can only be embedded in objects with a BoxCollider, SphereCollider, CapsuleCollider, or a MeshCollider.");
        }
        
        collisionState[rigidbodyId] = new Dictionary<int,int>();
        for (int j = 0;  j < rigidbodyId;  j++) {
            collisionState[rigidbodyId][j] = 0;
        }
    }

    public void RemoveStaticCharge(int rigidbodyId, int staticChargeId) {
        if (staticChargePosition.ContainsKey(rigidbodyId)) {
            staticChargePosition[rigidbodyId].Remove(staticChargeId);
            staticChargeStrength[rigidbodyId].Remove(staticChargeId);
        }
    }

    public void RemoveMagneticDipole(int rigidbodyId, int magneticDipoleId) {
        if (magneticDipolePosition.ContainsKey(rigidbodyId)) {
            magneticDipolePosition[rigidbodyId].Remove(magneticDipoleId);
            magneticDipoleMoment[rigidbodyId].Remove(magneticDipoleId);
        }
        magneticDipoleTorqueOnly.Remove(magneticDipoleId);
        magneticDipoles.Remove(magneticDipoleId);
    }

    // The following functions collect information about static charges, magnetic dipoles, and the collision status of rigidbodies.

    public void UpdateStaticCharge(int rigidbodyId, int staticChargeId, Vector3 position, float strength) {
        if (staticChargePosition.ContainsKey(rigidbodyId)) {
            staticChargePosition[rigidbodyId][staticChargeId] = position;
            staticChargeStrength[rigidbodyId][staticChargeId] = strength;
        }
    }
    
    public void UpdateMagneticDipole(int rigidbodyId, int magneticDipoleId, Vector3 position, Vector3 moment) {
        if (magneticDipolePosition.ContainsKey(rigidbodyId)) {
            magneticDipolePosition[rigidbodyId][magneticDipoleId] = position;
            magneticDipoleMoment[rigidbodyId][magneticDipoleId] = moment;
        }
    }
    
    public void UpdateCollisionState(int one, int two, int state) {
        if (one > two) {
            collisionState[one][two] = state;
            if (state == 0) {
                Joint.Destroy(rigidbodies[one].gameObject.GetComponent<Joint>());
            }
        }
        else {
            collisionState[two][one] = state;
            if (state == 0) {
                Joint.Destroy(rigidbodies[two].gameObject.GetComponent<Joint>());
            }
        }
    }
    
    public bool Colliding(int one, int two) {
        if (one > two) {
            return collisionState[one][two] > 0;
        }
        else {
            return collisionState[two][one] > 0;
        }
    }
    
    // FixedUpdate applies all forces among static charges and external electric fields, and among magnetic dipoles and external magnetic fields.
    void FixedUpdate() {
        int rigidbodyId = 0;
        foreach (Rigidbody rigidbody in rigidbodies) {
            if (!rigidbody) {
                staticChargePosition.Remove(rigidbodyId);
                staticChargeStrength.Remove(rigidbodyId);
                magneticDipolePosition.Remove(rigidbodyId);
                magneticDipoleMoment.Remove(rigidbodyId);
            }
            else {
                if (staticChargePosition.ContainsKey(rigidbodyId)) {
                    foreach (int staticChargeId in staticChargePosition[rigidbodyId].Keys) {
                        Vector3 pos = staticChargePosition[rigidbodyId][staticChargeId];
                        float strength = staticChargeStrength[rigidbodyId][staticChargeId];
                        Vector3 force = ForceOnStaticCharge(rigidbodyId, pos, strength);
                        
                        if (IsFinite(force)) {
                            rigidbody.AddForceAtPosition(force, pos, ForceMode.Impulse);
                        }
                    }
                }
            
                if (magneticDipolePosition.ContainsKey(rigidbodyId)) {
                    foreach (int magneticDipoleId in magneticDipolePosition[rigidbodyId].Keys) {
                        Vector3 pos = magneticDipolePosition[rigidbodyId][magneticDipoleId];
                        Vector3 dip = magneticDipoleMoment[rigidbodyId][magneticDipoleId];
                        
                        // if the position is constrained (as in a compass needle), don't do the extra work of calculating the force
                        if (!magneticDipoleTorqueOnly[magneticDipoleId]) {
                            Vector3 force = ForceOnMagneticDipole(rigidbodyId, pos, dip);
                            if (IsFinite(force)) {
                                rigidbody.AddForceAtPosition(force, pos, ForceMode.Impulse);
                            }
                        }
                        
                        // magnetic dipoles always feel torques (if you wanted a forceless, torqueless source of magnetic field, create and register an external field object)
                        Vector3 torque = TorqueOnMagneticDipole(rigidbodyId, pos, dip);
                        if (IsFinite(torque)) {
                            rigidbody.AddTorque(torque, ForceMode.Impulse);
                        }
                    }
                }
            }
            
            rigidbodyId++;
        }
        
        for (int i = 0;  i < rigidbodies.Count;  i++) {
            for (int j = 0;  j < i;  j++) {
                if (collisionState[i][j] > 0) {
                    // when two magnets collide, they can often experience very large forces, which cause the the colliders to bounce unrealistically; this softens that effect
                    if (collisionState[i][j] < dampenLandingSteps) {
                        rigidbodies[i].velocity *= dampenLanding;
                        rigidbodies[j].velocity *= dampenLanding;
                        collisionState[i][j] += 1;
                    }
                }
            }
        }
    }
    
    // Internal functions called by FixedUpdate.
    
    Vector3 ForceOnStaticCharge(int rigidbodyId, Vector3 pos, float strength) {
        return strength * ElectricField(pos, rigidbodyId);
    }
    
    Vector3 ForceOnMagneticDipole(int rigidbodyId, Vector3 pos, Vector3 dip) {
        Vector3 curlB;
        Vector3 ddxB;
        Vector3 ddyB;
        Vector3 ddzB;
        MagneticFieldDerivatives(pos, out curlB, out ddxB, out ddyB, out ddzB, rigidbodyId);
        return Vector3.Cross(dip, curlB) + dip.x*ddxB + dip.y*ddyB + dip.z*ddzB;
    }

    Vector3 TorqueOnMagneticDipole(int rigidbodyId, Vector3 pos, Vector3 dip) {
        return Vector3.Cross(dip, MagneticField(pos, rigidbodyId));
    }
    
    // Public functions for anyone to calculate the electric or magnetic force at a point.  The "rigidbodyId" option allows the caller to ignore all charges/dipoles from a given rigidbody.
    
    public Vector3 ElectricField(Vector3 pos, int rigidbodyId = -1, int excludeConductors = 0) {
        // I absorbed the 1/(4pi epsilon0) factor into the units of electric charge strength.
        Vector3 E = ambientElectricField;

        foreach (Func<Vector3,Vector3> fieldFunction in electricFieldFunctions) {
            E += fieldFunction(pos);
        }
        
        foreach (int rId in staticChargePosition.Keys) {
            if (rId != rigidbodyId  &&  (rigidbodyId < 0  ||  !Colliding(rId, rigidbodyId))) {
                foreach (int sId in staticChargePosition[rId].Keys) {
                    Vector3 opos = staticChargePosition[rId][sId];
                    float ostr = staticChargeStrength[rId][sId];

                    Vector3 r = (pos - opos);
                    E += ostr * r.normalized/r.sqrMagnitude / (4.0f*Mathf.PI*8.8541878176e-12f);
                }
            }
        }

        return E;
    }
    
    public Vector3 MagneticField(Vector3 pos, int rigidbodyId = -1, int excludeSuperconductors = 0) {
        Vector3 B = ambientMagneticField;

        foreach (Func<Vector3,Vector3> fieldFunction in magneticFieldFunctions) {
            B += fieldFunction(pos);
        }

        foreach (int rId in magneticDipolePosition.Keys) {
            if (rId != rigidbodyId) {
                foreach (int mId in magneticDipolePosition[rId].Keys) {
                    Vector3 opos = magneticDipolePosition[rId][mId];
                    Vector3 odip = magneticDipoleMoment[rId][mId];

                    Vector3 r = (pos - opos);
                    Vector3 rhat = r.normalized;

                    B += (3.0f * ((float)(Vector3.Dot(odip, rhat))) * rhat - odip) / Mathf.Pow(r.magnitude, 3) * 1e-7f;
                }
            }
        }
        
        if (excludeSuperconductors < superconductorMagneticFieldFunctions.Count) {
            B += superconductorMagneticFieldFunctions[excludeSuperconductors](pos, excludeSuperconductors);
        }
        
        return B;
    }
    
    public void MagneticFieldDerivatives(Vector3 pos, out Vector3 curlB, out Vector3 ddxB, out Vector3 ddyB, out Vector3 ddzB, int rigidbodyId = -1, int excludeSuperconductors = 0) {
        curlB = Vector3.zero;
        ddxB = Vector3.zero;
        ddyB = Vector3.zero;
        ddzB = Vector3.zero;

        for (int i = 0;  i < magneticFieldDxFunctions.Count;  i++) {
            Vector3 ddxBi = magneticFieldDxFunctions[i](pos);
            Vector3 ddyBi = magneticFieldDyFunctions[i](pos);
            Vector3 ddzBi = magneticFieldDzFunctions[i](pos);
            ddxB += ddxBi;
            ddyB += ddyBi;
            ddzB += ddzBi;
            curlB += new Vector3(ddyBi.z - ddzBi.y, ddzBi.x - ddxBi.z, ddxBi.y - ddyBi.x);
        }

        foreach (int rId in magneticDipolePosition.Keys) {
            if (rId != rigidbodyId  &&  (rigidbodyId < 0  ||  !Colliding(rId, rigidbodyId))) {
                foreach (int mId in magneticDipolePosition[rId].Keys) {
                    Vector3 opos = magneticDipolePosition[rId][mId];
                    Vector3 odip = magneticDipoleMoment[rId][mId];
                    
                    Vector3 r = (pos - opos);
                    float mdotr = Vector3.Dot(odip, r);
                    float overr2 = 1.0f / r.sqrMagnitude;
                    float overr5 = Mathf.Pow(r.magnitude, -5);
                    
                    Vector3 ddxBi = 3.0f * (odip.x*r + mdotr*Vector3.right + r.x*odip - 5.0f*mdotr*r.x*overr2*r) * overr5;
                    Vector3 ddyBi = 3.0f * (odip.y*r + mdotr*Vector3.up + r.y*odip - 5.0f*mdotr*r.y*overr2*r) * overr5;
                    Vector3 ddzBi = 3.0f * (odip.z*r + mdotr*Vector3.forward + r.z*odip - 5.0f*mdotr*r.z*overr2*r) * overr5;

                    ddxB += ddxBi * 1e-7f;
                    ddyB += ddyBi * 1e-7f;
                    ddzB += ddzBi * 1e-7f;
                    curlB += new Vector3(ddyBi.z - ddzBi.y, ddzBi.x - ddxBi.z, ddxBi.y - ddyBi.x) * 1e-7f;
                }
            }
        }
        
        if (excludeSuperconductors < superconductorMagneticFieldDxFunctions.Count) {
            Vector3 ddxBi = superconductorMagneticFieldDxFunctions[excludeSuperconductors](pos, excludeSuperconductors);
            Vector3 ddyBi = superconductorMagneticFieldDyFunctions[excludeSuperconductors](pos, excludeSuperconductors);
            Vector3 ddzBi = superconductorMagneticFieldDzFunctions[excludeSuperconductors](pos, excludeSuperconductors);
            ddxB += ddxBi;
            ddyB += ddyBi;
            ddzB += ddzBi;
            curlB += new Vector3(ddyBi.z - ddzBi.y, ddzBi.x - ddxBi.z, ddxBi.y - ddyBi.x);
        }
    }

    // MagnetizableMaterials call this function in their FixedUpdate() to get forces applied to them.  (No torques because their induced magnetism is collinear with the direction of the applied field.)

    public void ApplyForcesOnMagnetizable(bool applyExternalForce, bool applyDipoleForce, float strength, GameObject magnetizable, ref Dictionary<int,SpringJoint> sjToDipole, ref Dictionary<int,SpringJoint> sjFromDipole) {

        if (applyExternalForce) {
            // Apply forces due to external fields
            Vector3 Bext = ambientMagneticField;
            Vector3 curlBext = Vector3.zero;
            Vector3 ddxBext = Vector3.zero;
            Vector3 ddyBext = Vector3.zero;
            Vector3 ddzBext = Vector3.zero;
            
            // Get a random point within the collider
            // (This algorithm finds a point in a 3D shape that's filled in toward the center.  It works for boxes, spheres, and capsules.)
            // TODO: special cases for boxes, spheres, and capsules to avoid Elimination Method random number generation and raycasts when the collider has a simple shape.
            Vector3 minBounds = magnetizable.GetComponent<Collider>().bounds.min;
            Vector3 maxBounds = magnetizable.GetComponent<Collider>().bounds.max;        
            float big = 2.0f * (maxBounds - minBounds).magnitude;
            Vector3 randomPos = Vector3.zero;
            Vector3 outside;
            Ray ray = new Ray(Vector3.zero, Vector3.right);
            RaycastHit hitInfo;
            bool done = false;
            while (!done) {
                randomPos.x = UnityEngine.Random.Range(minBounds.x, maxBounds.x);
                randomPos.y = UnityEngine.Random.Range(minBounds.y, maxBounds.y);
                randomPos.z = UnityEngine.Random.Range(minBounds.z, maxBounds.z);
                
                // there's a one-in-a-zillion chance of this happening, but I don't want to see error messages
                if (randomPos == magnetizable.transform.position) continue;
                
                ray.origin = randomPos;
                ray.direction = (randomPos - magnetizable.transform.position).normalized;
                outside = ray.GetPoint(big);
                
                ray.origin = outside;
                ray.direction = (randomPos - outside).normalized;
                
                if (magnetizable.GetComponent<Collider>().Raycast(ray, out hitInfo, (randomPos - outside).magnitude)) {
                    done = true;
                }
            }
    
            foreach (Func<Vector3,Vector3> fieldFunction in magneticFieldFunctions) {
                Bext += fieldFunction(randomPos);
            }
            
            for (int i = 0;  i < magneticFieldDxFunctions.Count;  i++) {        
                Vector3 ddxBi = magneticFieldDxFunctions[i](randomPos);
                Vector3 ddyBi = magneticFieldDyFunctions[i](randomPos);
                Vector3 ddzBi = magneticFieldDzFunctions[i](randomPos);
                ddxBext += ddxBi;
                ddyBext += ddyBi;
                ddzBext += ddzBi;
                curlBext += new Vector3(ddyBi.z - ddzBi.y, ddzBi.x - ddxBi.z, ddxBi.y - ddyBi.x);
            }
        
            Vector3 forceext = strength * (Vector3.Cross(Bext, curlBext) + Bext.x*ddxBext + Bext.y*ddyBext + Bext.z*ddzBext) / (4e-7f * Mathf.PI);

            if (IsFinite(forceext)) {
                magnetizable.GetComponent<Rigidbody>().AddForce(forceext);
            }
            // TODO: AddForceAtPosition?  Would the randomness of randomPos lead to erratic torques?
            // (I'm relying on the fact that this is not ForceMode.Impulse to smooth over the randomness of choice of position within the body.)
        }

        if (applyDipoleForce) {
            // Apply forces due to dipoles
            foreach (int rId in magneticDipolePosition.Keys) {
                foreach (int mId in magneticDipolePosition[rId].Keys) {
                    if (!sjToDipole.ContainsKey(mId)) {
                        AddMagnetizableMaterialSprings(mId, magnetizable, ref sjToDipole, ref sjFromDipole);
                    }
                    
                    if (!magneticDipoles[mId]) {
                        // This dipole has been deleted; remove all springs and stop being attracted to it.
                        sjToDipole.Remove(mId);
                        sjFromDipole.Remove(mId);
                    }
                    else {
                        float depth = 0.01f * Mathf.Min(magnetizable.GetComponent<Collider>().bounds.size.x, magnetizable.GetComponent<Collider>().bounds.size.y, magnetizable.GetComponent<Collider>().bounds.size.z);
                        
                        // We can't integrate over the whole magnetizable volume, so just use the closest point.  That's reasonable because the force falls off so steeply (1/r^7).
                        Vector3 pos = ClosestPointOnMagnetizable(mId, magnetizable, depth);
        
                        Vector3 opos = magneticDipolePosition[rId][mId];
                        Vector3 odip = magneticDipoleMoment[rId][mId];
                        Vector3 r = (pos - opos);
                        Vector3 rhat = r.normalized;
                        float mdotr = Vector3.Dot(odip, r);
                        float overr2 = 1.0f / r.sqrMagnitude;
                        float overr5 = Mathf.Pow(r.magnitude, -5);
                        
                        Vector3 B = (3.0f * ((float)(Vector3.Dot(odip, rhat))) * rhat - odip) / Mathf.Pow(r.magnitude, 3);
        
                        Vector3 ddxB = 3.0f * (odip.x*r + mdotr*Vector3.right + r.x*odip - 5.0f*mdotr*r.x*overr2*r) * overr5;
                        Vector3 ddyB = 3.0f * (odip.y*r + mdotr*Vector3.up + r.y*odip - 5.0f*mdotr*r.y*overr2*r) * overr5;
                        Vector3 ddzB = 3.0f * (odip.z*r + mdotr*Vector3.forward + r.z*odip - 5.0f*mdotr*r.z*overr2*r) * overr5;                
                    
                        Vector3 curlB = new Vector3(ddyB.z - ddzB.y, ddzB.x - ddxB.z, ddxB.y - ddyB.x);
        
                        // Like the force between two dipoles except that the magnetizable's dipole is on the surface and proportional to the local magnetic field
                        Vector3 force = strength * (Vector3.Cross(B, curlB) + B.x*ddxB + B.y*ddyB + B.z*ddzB) / (4e-7f * Mathf.PI);
    
                        float m1 = magneticDipoles[mId].transform.parent.GetComponent<Rigidbody>().mass;
                        float m2 = magnetizable.GetComponent<Rigidbody>().mass;    
                        float forceMag = force.magnitude;
                        if (forceMag/Mathf.Min(m1, m2) > maxMagnetizedForceOverMass) {
                            forceMag = maxMagnetizedForceOverMass * Mathf.Min(m1, m2);
                        }
                        float rMag = r.magnitude;

                        // Instead of applying the force directly (causes huge problems when things get too close to each other), we pass it to the physics engine's SpringJoint, since that handles collisions well.
                        // But remember that a spring constant is force divided by displacement, so we'll have to divide out that extra factor of r.  (Each spring provides half the force, so that Newton's third law holds in this crazy mixed-up world.)
                        float springConstant = forceMag / rMag / 2.0f;
                        if (float.IsNaN(springConstant)  ||  springConstant == Mathf.Infinity) {
                            springConstant = 0.0f;
                        }
                        sjToDipole[mId].spring = springConstant;
                        sjFromDipole[mId].spring = springConstant;
                        
                        // In principle, we should apply the force at the closest point on the surface, but this leads to more Newton's third law violations
                        // sjFromDipole[mId].gameObject.transform.position = pos;
                    }
                }
            }
        }
    }
    
    void AddMagnetizableMaterialSprings(int magneticDipoleId, GameObject magnetizable, ref Dictionary<int,SpringJoint> sjToDipole, ref Dictionary<int,SpringJoint> sjFromDipole) {
        GameObject dipole = magneticDipoles[magneticDipoleId];
        Rigidbody dipoleRigidbody = dipole.transform.parent.GetComponent<Rigidbody>();

        // We need springs in both directions because the physics engine's implementation of springs doesn't obey Newton's third law otherwise.  (I know... I know...)
        GameObject toDipole = new GameObject("magnetizable-to-dipole-" + magneticDipoleId.ToString());
        toDipole.transform.parent = dipole.transform;
        
        GameObject fromDipole = new GameObject("magnetizable-from-dipole-" + magneticDipoleId.ToString());
        fromDipole.transform.parent = magnetizable.transform;
        
        // We can only set a spring's target elongation through the positions of its endpoints at the time of creation.  I want the target elongation to be zero.
        toDipole.transform.position = magnetizable.transform.position;
        SpringJoint sjTo = toDipole.AddComponent<SpringJoint>();
        sjTo.connectedBody = magnetizable.GetComponent<Rigidbody>();
        sjTo.maxDistance = 0.01f * Mathf.Min(magnetizable.GetComponent<Collider>().bounds.size.x, magnetizable.GetComponent<Collider>().bounds.size.y, magnetizable.GetComponent<Collider>().bounds.size.z);
        sjTo.spring = 0.0f;
        toDipole.GetComponent<Rigidbody>().mass = 0.01f * magnetizable.GetComponent<Rigidbody>().mass;
        toDipole.GetComponent<Rigidbody>().useGravity = false;
        toDipole.GetComponent<Rigidbody>().isKinematic = true;
        
        fromDipole.transform.position = dipole.transform.position;
        SpringJoint sjFrom = fromDipole.AddComponent<SpringJoint>();
        sjFrom.connectedBody = dipoleRigidbody;
        sjFrom.maxDistance = sjTo.maxDistance;
        sjFrom.spring = 0.0f;
        fromDipole.GetComponent<Rigidbody>().mass = 0.01f * dipoleRigidbody.mass;
        fromDipole.GetComponent<Rigidbody>().useGravity = false;
        fromDipole.GetComponent<Rigidbody>().isKinematic = true;
        
        toDipole.transform.localPosition = Vector3.zero;
        toDipole.transform.localEulerAngles = Vector3.zero;
        fromDipole.transform.localPosition = Vector3.zero;
        fromDipole.transform.localEulerAngles = Vector3.zero;

        sjToDipole[magneticDipoleId] = sjTo;
        sjFromDipole[magneticDipoleId] = sjFrom;
    }

    Vector3 ClosestPointOnMagnetizable(int magneticDipoleId, GameObject magnetizable, float depth) {
        Vector3 dipolePosition = magneticDipoles[magneticDipoleId].transform.position;
        Vector3 notActuallyOnSurface = magnetizable.GetComponent<Collider>().ClosestPointOnBounds(dipolePosition);
        if (notActuallyOnSurface != dipolePosition) {
            Ray ray = new Ray(dipolePosition, (notActuallyOnSurface - dipolePosition).normalized);
            RaycastHit hitInfo;
            if (magnetizable.GetComponent<Collider>().Raycast(ray, out hitInfo, Mathf.Infinity)) {            
                // Instead of "return hitInfo.point;", return a point just slightly within the magnetizable
                return ray.GetPoint(hitInfo.distance + depth);
            }
        }
        return magnetizable.transform.position;
    }

}
