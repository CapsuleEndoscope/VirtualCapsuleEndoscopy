using UnityEngine;
using System.Collections;

public class SuperconductorSmallSphereScript : SuperconductorScript {
    // cache derivatives so that you don't have to compute them three times (for dx, dy, dz)
    private Vector3 lastpos = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
    private Vector3 lastselfpos = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
    private Vector3 ddx_B = Vector3.zero;
    private Vector3 ddy_B = Vector3.zero;
    private Vector3 ddz_B = Vector3.zero;

    private float radius = 0.5f;

	void Start() {
	    if (!electromagneticFieldController) {
	        electromagneticFieldController = GameObject.Find("ElectromagneticFieldController");
	        if (!electromagneticFieldController) {
	            throw new System.Exception("Could not find ElectromagneticFieldController");
	        }
	    }
	    controller = electromagneticFieldController.GetComponent<ElectromagneticFieldControllerScript>();
	    
	    controller.RegisterSuperconductorMagneticField(MagneticField);    
	    controller.RegisterSuperconductorMagneticFieldDerivatives(MagneticFieldBx, MagneticFieldBy, MagneticFieldBz);
    }
    
    Vector3 MagneticField(Vector3 pos, int excludeSuperconductors) {
        Vector3 H0 = controller.MagneticField(transform.position, -1, excludeSuperconductors + 1);   
        H0 = transform.InverseTransformDirection(H0);
        Vector3 Hnorm = H0.normalized;
        
        // Vector3 rvec = pos - transform.position;
        Vector3 rvec = transform.InverseTransformPoint(pos);
        
        float rmag = rvec.magnitude;
        if (rmag <= radius) { return -H0; }
        
        Vector3 rhat = rvec.normalized;
        float cosTheta = Vector3.Dot(Hnorm, rhat);
        float sinTheta = Mathf.Sqrt(1.0f - cosTheta*cosTheta);  // theta is only defined from 0 to pi
        Vector3 thetahat = rhat * cosTheta/sinTheta - Hnorm/sinTheta;
        
        return transform.TransformDirection(-H0.magnitude * Mathf.Pow(radius/rmag, 3) * (2.0f * cosTheta * rhat + sinTheta * thetahat));
    }

    Vector3 MagneticFieldBx(Vector3 pos, int excludeSuperconductors) {
        derivatives(pos, excludeSuperconductors);
        return ddx_B;
    }
    
    Vector3 MagneticFieldBy(Vector3 pos, int excludeSuperconductors) {
        derivatives(pos, excludeSuperconductors);
        return ddy_B;
    }
    
    Vector3 MagneticFieldBz(Vector3 pos, int excludeSuperconductors) {
        derivatives(pos, excludeSuperconductors);
        return ddz_B;
    }

    void derivatives(Vector3 pos, int excludeSuperconductors) {
        if (pos == lastpos  &&  transform.position == lastselfpos) { return; }
        lastpos = pos;
        lastselfpos = transform.position;

        Vector3 H0 = controller.MagneticField(transform.position, -1, excludeSuperconductors + 1);
        H0 = transform.InverseTransformDirection(H0);
        Vector3 Hnorm = H0.normalized;
        float Hmag = H0.magnitude;
        
        // Vector3 rvec = pos - transform.position;
        Vector3 rvec = transform.InverseTransformPoint(pos);

        float rmag = rvec.magnitude;
        if (rmag <= radius) {
            ddx_B = Vector3.zero;
            ddy_B = Vector3.zero;
            ddz_B = Vector3.zero;
        }
        else {
            Vector3 rhat = rvec.normalized;
            float cosTheta = Vector3.Dot(Hnorm, rhat);
            float sinTheta = Mathf.Sqrt(1.0f - cosTheta*cosTheta);  // theta is only defined from 0 to pi            
            Vector3 thetahat = rhat * cosTheta/sinTheta - Hnorm/sinTheta;

            float prefactor = -Hmag * Mathf.Pow(radius/rmag, 3);
            float ddx_prefactor = 3.0f * prefactor * rvec.x / rmag/rmag;
            float ddy_prefactor = 3.0f * prefactor * rvec.y / rmag/rmag;
            float ddz_prefactor = 3.0f * prefactor * rvec.z / rmag/rmag;
            
            Vector3 ddx_rhat = Vector3.right/rmag - rvec * rvec.x / rmag/rmag/rmag;
            Vector3 ddy_rhat = Vector3.up/rmag - rvec * rvec.y / rmag/rmag/rmag;
            Vector3 ddz_rhat = Vector3.forward/rmag - rvec * rvec.z / rmag/rmag/rmag;
            
            float ddx_cosTheta = Vector3.Dot(ddx_rhat, Hnorm);
            float ddy_cosTheta = Vector3.Dot(ddy_rhat, Hnorm);
            float ddz_cosTheta = Vector3.Dot(ddz_rhat, Hnorm);
                        
            float ddx_sinTheta = -cosTheta/sinTheta * ddx_cosTheta;
            float ddy_sinTheta = -cosTheta/sinTheta * ddy_cosTheta;
            float ddz_sinTheta = -cosTheta/sinTheta * ddz_cosTheta;

            float ddx_oneOverSinTheta = -ddx_sinTheta / sinTheta / sinTheta;
            float ddy_oneOverSinTheta = -ddy_sinTheta / sinTheta / sinTheta;
            float ddz_oneOverSinTheta = -ddz_sinTheta / sinTheta / sinTheta;

            Vector3 ddx_thetahat = ddx_rhat*cosTheta/sinTheta + rhat*ddx_cosTheta/sinTheta + rhat*cosTheta*ddx_oneOverSinTheta - Hnorm*ddx_oneOverSinTheta;
            Vector3 ddy_thetahat = ddy_rhat*cosTheta/sinTheta + rhat*ddy_cosTheta/sinTheta + rhat*cosTheta*ddy_oneOverSinTheta - Hnorm*ddy_oneOverSinTheta;
            Vector3 ddz_thetahat = ddz_rhat*cosTheta/sinTheta + rhat*ddz_cosTheta/sinTheta + rhat*cosTheta*ddz_oneOverSinTheta - Hnorm*ddz_oneOverSinTheta;

            Vector3 firstTerm = 2.0f * cosTheta * rhat;
            Vector3 ddx_firstTerm = 2.0f * (ddx_cosTheta * rhat + cosTheta * ddx_rhat);
            Vector3 ddy_firstTerm = 2.0f * (ddy_cosTheta * rhat + cosTheta * ddy_rhat);
            Vector3 ddz_firstTerm = 2.0f * (ddz_cosTheta * rhat + cosTheta * ddz_rhat);

            Vector3 secondTerm = sinTheta * thetahat;
            Vector3 ddx_secondTerm = ddx_sinTheta * thetahat + sinTheta * ddx_thetahat;
            Vector3 ddy_secondTerm = ddy_sinTheta * thetahat + sinTheta * ddy_thetahat;
            Vector3 ddz_secondTerm = ddz_sinTheta * thetahat + sinTheta * ddz_thetahat;

            // when we put this all together, we must realize that these are derivatives of a local vector in local coordinates
            Vector3 ddxloc_Bloc = ddx_prefactor * (firstTerm + secondTerm) + prefactor * (ddx_firstTerm + ddx_secondTerm);
            Vector3 ddyloc_Bloc = ddy_prefactor * (firstTerm + secondTerm) + prefactor * (ddy_firstTerm + ddy_secondTerm);
            Vector3 ddzloc_Bloc = ddz_prefactor * (firstTerm + secondTerm) + prefactor * (ddz_firstTerm + ddz_secondTerm);
            
            // and then convert them to global derivatives of a global vector (copied from ExternalFieldSolenoidScript, which has been verified)
            Vector3 right = transform.InverseTransformDirection(Vector3.right);
            Vector3 up = transform.InverseTransformDirection(Vector3.up);
            Vector3 forward = transform.InverseTransformDirection(Vector3.forward);
            float dxloc_dxglob = right.x;
            float dyloc_dxglob = right.y;
            float dzloc_dxglob = right.z;
            float dxloc_dyglob = up.x;
            float dyloc_dyglob = up.y;
            float dzloc_dyglob = up.z;
            float dxloc_dzglob = forward.x;
            float dyloc_dzglob = forward.y;
            float dzloc_dzglob = forward.z;
    
            float ddxglob_Bxloc = dxloc_dxglob*ddxloc_Bloc.x + dyloc_dxglob*ddyloc_Bloc.x + dzloc_dxglob*ddzloc_Bloc.x;
            float ddxglob_Byloc = dxloc_dxglob*ddxloc_Bloc.y + dyloc_dxglob*ddyloc_Bloc.y + dzloc_dxglob*ddzloc_Bloc.y;
            float ddxglob_Bzloc = dxloc_dxglob*ddxloc_Bloc.z + dyloc_dxglob*ddyloc_Bloc.z + dzloc_dxglob*ddzloc_Bloc.z;
    
            float ddyglob_Bxloc = dxloc_dyglob*ddxloc_Bloc.x + dyloc_dyglob*ddyloc_Bloc.x + dzloc_dyglob*ddzloc_Bloc.x;
            float ddyglob_Byloc = dxloc_dyglob*ddxloc_Bloc.y + dyloc_dyglob*ddyloc_Bloc.y + dzloc_dyglob*ddzloc_Bloc.y;
            float ddyglob_Bzloc = dxloc_dyglob*ddxloc_Bloc.z + dyloc_dyglob*ddyloc_Bloc.z + dzloc_dyglob*ddzloc_Bloc.z;
    
            float ddzglob_Bxloc = dxloc_dzglob*ddxloc_Bloc.x + dyloc_dzglob*ddyloc_Bloc.x + dzloc_dzglob*ddzloc_Bloc.x;
            float ddzglob_Byloc = dxloc_dzglob*ddxloc_Bloc.y + dyloc_dzglob*ddyloc_Bloc.y + dzloc_dzglob*ddzloc_Bloc.y;
            float ddzglob_Bzloc = dxloc_dzglob*ddxloc_Bloc.z + dyloc_dzglob*ddyloc_Bloc.z + dzloc_dzglob*ddzloc_Bloc.z;
    
            ddx_B = transform.TransformDirection(new Vector3(ddxglob_Bxloc, ddxglob_Byloc, ddxglob_Bzloc));
            ddy_B = transform.TransformDirection(new Vector3(ddyglob_Bxloc, ddyglob_Byloc, ddyglob_Bzloc));
            ddz_B = transform.TransformDirection(new Vector3(ddzglob_Bxloc, ddzglob_Byloc, ddzglob_Bzloc));
        }
    }
}
