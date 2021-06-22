using UnityEngine;
using System.Collections;

public class ExternalFieldSolenoidScript : MonoBehaviour {
    public GameObject electromagneticFieldController = null;
    private ElectromagneticFieldControllerScript controller;

    public float strength = 1.0f;  // solenoidal current in Amps (sum of all windings, if produced by a coil of wire)
    
    // radius (a) and length (L) of the solenoid to match the shape of the cylinder mesh in the scene view
    const float a = 0.5f;
    const float L = 2.5f;
    
    // cache derivatives so that you don't have to compute them three times (for dx, dy, dz)
    private Vector3 lastpos = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
    private Vector3 lastselfpos = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
    private Vector3 lastselfdir = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
    private Vector3 ddx_B = Vector3.zero;
    private Vector3 ddy_B = Vector3.zero;
    private Vector3 ddz_B = Vector3.zero;

	void Start() {
	    if (!electromagneticFieldController) {
	        electromagneticFieldController = GameObject.Find("ElectromagneticFieldController");
	        if (!electromagneticFieldController) {
	            throw new System.Exception("Could not find ElectromagneticFieldController");
	        }
	    }
	    controller = electromagneticFieldController.GetComponent<ElectromagneticFieldControllerScript>();
	    
	    controller.RegisterMagneticField(MagneticField);
	    controller.RegisterMagneticFieldDerivatives(MagneticFieldDx, MagneticFieldDy, MagneticFieldDz);
    }

	Vector3 MagneticField(Vector3 pos) {
        // convert to local cylindrical coordinates (around the cylinder's y axis)
        Vector3 localPos = transform.InverseTransformPoint(pos);
        float rho = Mathf.Sqrt(localPos.x*localPos.x + localPos.z*localPos.z);
        float phi = Mathf.Atan2(localPos.z, localPos.x);
        float z = localPos.y;
        
        float Brho, Bz;
        
        // if there's a domain error, skip this evaluation
        try {
            // general solution of a finite solenoid
            // http://nukephysik101.wordpress.com/2011/07/17/the-magnetic-field-of-a-finite-length-solenoid/
            // re-expressed to minimize error near rho = 0 (by taking a factor of rho out of k2 and cancelling rhos in the denominator)

            float zetaplus = z + L/2.0f;
            float zetaminus = z - L/2.0f;
            float h2 = 4.0f*a*rho/Mathf.Pow(a + rho, 2);
            float k2plus = 4.0f*a/(Mathf.Pow(a + rho, 2) + zetaplus*zetaplus);
            float k2minus = 4.0f*a/(Mathf.Pow(a + rho, 2) + zetaminus*zetaminus);
            float kplus = Mathf.Sqrt(k2plus);
            float kminus = Mathf.Sqrt(k2minus);
            
            if (rho < 1e-5) {
                Brho = 0.0f;
            }
            else {
                float brhoplus = (k2plus*rho - 2.0f)/(2.0f*kplus) * ellipK(k2plus*rho) + ellipE(k2plus*rho)/kplus;
                float brhominus = (k2minus*rho - 2.0f)/(2.0f*kminus) * ellipK(k2minus*rho) + ellipE(k2minus*rho)/kminus;
                Brho = 2e-7f * strength * (1.0f/L) * Mathf.Sqrt(a)/rho * (brhoplus - brhominus);
            }
            float bzplus = zetaplus*kplus*(ellipK(k2plus*rho) + (a - rho)/(a + rho)*ellipPi(h2, k2plus*rho));
            float bzminus = zetaminus*kminus*(ellipK(k2minus*rho) + (a - rho)/(a + rho)*ellipPi(h2, k2minus*rho));
            Bz = 1e-7f * strength * (1.0f/L/2.0f/Mathf.Sqrt(a)) * (bzplus - bzminus);
        }
        catch {
            Brho = 0.0f;
            Bz = 0.0f;
        }
        
//        if (false) {
//            // from a 1960 NASA paper: http://ntrs.nasa.gov/search.jsp?R=19980227402
//            // gives the wrong result (consistently: even for the on-axis approximation)
//            // I must be misinterpreting some of the symbols they're using (didn't follow the references to tables of integrals!)
// 
//            float zetaplus = z + L/2.0f;
//            float zetaminus = z - L/2.0f;
//            float k2plus = 4.0f*a*rho/(Mathf.Pow(zetaplus, 2) + Mathf.Pow(a + rho, 2));
//            float k2minus = 4.0f*a*rho/(Mathf.Pow(zetaminus, 2) + Mathf.Pow(a + rho, 2));
//            float kplus = Mathf.Sqrt(k2plus);
//            float kminus = Mathf.Sqrt(k2minus);
//            float phiplus = Mathf.Atan2(zetaplus, (a - rho));
//            float phiminus = Mathf.Atan2(zetaminus, (a - rho));
//            
//            if (Mathf.Abs(rho/a) < 0.01f) {
//                float brhoplus = a*a*rho/Mathf.Pow(zetaplus*zetaplus + a*a, 1.5f);
//                float bhrominus = a*a*rho/Mathf.Pow(zetaminus*zetaminus + a*a, 1.5f);
//                Brho = 1e-7f*Mathf.PI * strength * (brhoplus - bhrominus);
//                
//                float bzplus = zetaplus/Mathf.Sqrt(zetaplus*zetaplus + a*a);
//                float bzminus = zetaminus/Mathf.Sqrt(zetaminus*zetaminus + a*a);
//                Bz = 2e-7f*Mathf.PI * strength * (bzplus - bzminus);
//            }
//            else {
//                float brhoplus = (2.0f - k2plus)/(2.0f*kplus)*ellipK(k2plus) - ellipE(k2plus)/kplus;
//                float brhominus = (2.0f - k2minus)/(2.0f*kminus)*ellipK(k2minus) - ellipE(k2minus)/kminus;
//                Brho = 4e-7f * strength * Mathf.Sqrt(a/rho) * (brhoplus - brhominus);
//                
//                float bzplus = (zetaplus*kplus)/(Mathf.PI*Mathf.Sqrt(a*rho))*ellipK(k2plus) + Mathf.Sign((a - rho)*zetaplus) * heuman_lambda(phiplus, k2plus);
//                float bzminus = (zetaminus*kminus)/(Mathf.PI*Mathf.Sqrt(a*rho))*ellipK(k2minus) + Mathf.Sign((a - rho)*zetaminus) * heuman_lambda(phiminus, k2minus);
//
//                Debug.Log(string.Format("bzplus {0} bzminus {1}", bzplus, bzminus));
//                
//                Bz = 1e-7f*Mathf.PI * strength * (bzplus - bzminus);
//            }
//        }
                
        // convert to global cartesian coordinates
        Vector3 B = new Vector3(Brho*Mathf.Cos(phi), Bz, Brho*Mathf.Sin(phi));

        return transform.TransformDirection(B);
	}

	Vector3 MagneticFieldDx(Vector3 pos) {
        derivatives(pos);
        return ddx_B;
    }

	Vector3 MagneticFieldDy(Vector3 pos) {
        derivatives(pos);
        return ddy_B;
    }

	Vector3 MagneticFieldDz(Vector3 pos) {
        derivatives(pos);
        return ddz_B;
    }

    void derivatives(Vector3 pos) {
        if (pos == lastpos  &&  transform.position == lastselfpos  &&  transform.eulerAngles == lastselfdir) { return; }
        lastpos = pos;
        lastselfpos = transform.position;
        lastselfdir = transform.eulerAngles;

        // convert to local cylindrical coordinates (around the cylinder's y axis)
        Vector3 localPos = transform.InverseTransformPoint(pos);
        float rho = Mathf.Sqrt(localPos.x*localPos.x + localPos.z*localPos.z);
        float phi = Mathf.Atan2(localPos.z, localPos.x);
        float z = localPos.y;

        try {
            float zetaplus = z + L/2.0f;
            float zetaminus = z - L/2.0f;
            float h2 = 4.0f*a*rho/Mathf.Pow(a + rho, 2);
            float k2plus = 4.0f*a/(Mathf.Pow(a + rho, 2) + zetaplus*zetaplus);
            float k2minus = 4.0f*a/(Mathf.Pow(a + rho, 2) + zetaminus*zetaminus);
            float kplus = Mathf.Sqrt(k2plus);
            float kminus = Mathf.Sqrt(k2minus);
            
            float ddr_h2 = 4.0f*a/Mathf.Pow(a + rho, 2) - 8.0f*a*rho/Mathf.Pow(a + rho, 3);
            float ddr_k2plus = -8.0f*a*(a + rho)/Mathf.Pow(Mathf.Pow(a + rho, 2) + zetaplus*zetaplus, 2);
            float ddr_k2minus = -8.0f*a*(a + rho)/Mathf.Pow(Mathf.Pow(a + rho, 2) + zetaminus*zetaminus, 2);
            float ddz_k2plus = -8.0f*a*zetaplus/Mathf.Pow(Mathf.Pow(a + rho, 2) + zetaplus*zetaplus, 2);
            float ddz_k2minus = -8.0f*a*zetaminus/Mathf.Pow(Mathf.Pow(a + rho, 2) + zetaminus*zetaminus, 2);
            float ddr_kplus = (0.5f/kplus) * ddr_k2plus;
            float ddr_kminus = (0.5f/kminus) * ddr_k2minus;
            float ddz_kplus = (0.5f/kplus) * ddz_k2plus;
            float ddz_kminus = (0.5f/kminus) * ddz_k2minus;
            
            float ellipK_k2plus_rho = ellipK(k2plus*rho);
            float ellipK_k2minus_rho = ellipK(k2minus*rho);
            float ellipE_k2plus_rho = ellipE(k2plus*rho);
            float ellipE_k2minus_rho = ellipE(k2minus*rho);
            float ellipPi_h2_k2plus_rho = ellipPi(h2, k2plus*rho);
            float ellipPi_h2_k2minus_rho = ellipPi(h2, k2minus*rho);
    
            // derivative of K(m) (complete elliptic, first kind, parameterized by m = k^2) is
            // E(m)/(2.0f * m * (1.0f - m)) - K(m)/(2.0f * m)
            float ddr_ellipK_k2plus_rho = (ellipE_k2plus_rho/(2.0f * k2plus*rho * (1.0f - k2plus*rho)) - ellipK_k2plus_rho/(2.0f * k2plus*rho)) * (k2plus + ddr_k2plus*rho);
            float ddr_ellipK_k2minus_rho = (ellipE_k2minus_rho/(2.0f * k2minus*rho * (1.0f - k2minus*rho)) - ellipK_k2minus_rho/(2.0f * k2minus*rho)) * (k2minus + ddr_k2minus*rho);
            float ddz_ellipK_k2plus_rho = (ellipE_k2plus_rho/(2.0f * k2plus*rho * (1.0f - k2plus*rho)) - ellipK_k2plus_rho/(2.0f * k2plus*rho)) * (ddz_k2plus*rho);
            float ddz_ellipK_k2minus_rho = (ellipE_k2minus_rho/(2.0f * k2minus*rho * (1.0f - k2minus*rho)) - ellipK_k2minus_rho/(2.0f * k2minus*rho)) * (ddz_k2minus*rho);
    
            // derivative of E(m) (complete elliptic, second kind, parameterized by m = k^2) is
            // (E(m) - K(m)) / (2.0f * m)
            float ddr_ellipE_k2plus_rho = ((ellipE_k2plus_rho - ellipK_k2plus_rho) / (2.0f * k2plus*rho)) * (k2plus + ddr_k2plus*rho);
            float ddr_ellipE_k2minus_rho = ((ellipE_k2minus_rho - ellipK_k2minus_rho) / (2.0f * k2minus*rho)) * (k2minus + ddr_k2minus*rho);
            float ddz_ellipE_k2plus_rho = ((ellipE_k2plus_rho - ellipK_k2plus_rho) / (2.0f * k2plus*rho)) * (ddz_k2plus*rho);
            float ddz_ellipE_k2minus_rho = ((ellipE_k2minus_rho - ellipK_k2minus_rho) / (2.0f * k2minus*rho)) * (ddz_k2minus*rho);
            
            // derivative of Pi(n, m) (complete elliptic, third kind, parameterized by m = k^2) is
            // d/dn Pi(n, m) = (E(m) + (m - n)/n * K(m) + (n*n - m)/n * Pi(n, m))/(2.0f * (m - n) * (n - 1.0f))
            // d/dm Pi(n, m) = (E(m)/(m - 1.0f) + Pi(n, m))/(2.0f * (n - m))
            float ddr_ellipPi_h2_k2plus_rho = ((ellipE_k2plus_rho + (k2plus*rho - h2)/h2 * ellipK_k2plus_rho + (h2*h2 - k2plus*rho)/h2 * ellipPi_h2_k2plus_rho)/(2.0f * (k2plus*rho - h2) * (h2 - 1.0f))) * (ddr_h2) + ((ellipE_k2plus_rho/(k2plus*rho - 1.0f) + ellipPi_h2_k2plus_rho)/(2.0f * (h2 - k2plus*rho))) * (k2plus + ddr_k2plus*rho);
            float ddr_ellipPi_h2_k2minus_rho = ((ellipE_k2minus_rho + (k2minus*rho - h2)/h2 * ellipK_k2minus_rho + (h2*h2 - k2minus*rho)/h2 * ellipPi_h2_k2minus_rho)/(2.0f * (k2minus*rho - h2) * (h2 - 1.0f))) * (ddr_h2) + ((ellipE_k2minus_rho/(k2minus*rho - 1.0f) + ellipPi_h2_k2minus_rho)/(2.0f * (h2 - k2minus*rho))) * (k2minus + ddr_k2minus*rho);
            float ddz_ellipPi_h2_k2plus_rho = ((ellipE_k2plus_rho/(k2plus*rho - 1.0f) + ellipPi_h2_k2plus_rho)/(2.0f * (h2 - k2plus*rho))) * (ddz_k2plus*rho);
            float ddz_ellipPi_h2_k2minus_rho = ((ellipE_k2minus_rho/(k2minus*rho - 1.0f) + ellipPi_h2_k2minus_rho)/(2.0f * (h2 - k2minus*rho))) * (ddz_k2minus*rho);
            
            float ddr_brhoplus = (ddr_k2plus*rho)/(2.0f*kplus)*ellipK_k2plus_rho + (k2plus)/(2.0f*kplus)*ellipK_k2plus_rho - (k2plus*rho - 2.0f)*2.0f*ddr_kplus/(4.0f*k2plus)*ellipK_k2plus_rho + (k2plus*rho - 2.0f)/(2.0f*kplus)*ddr_ellipK_k2plus_rho + ddr_ellipE_k2plus_rho/kplus - ellipE_k2plus_rho*ddr_kplus/(k2plus);
            float ddr_brhominus = (ddr_k2minus*rho)/(2.0f*kminus)*ellipK_k2minus_rho + (k2minus)/(2.0f*kminus)*ellipK_k2minus_rho - (k2minus*rho - 2.0f)*2.0f*ddr_kminus/(4.0f*k2minus)*ellipK_k2minus_rho + (k2minus*rho - 2.0f)/(2.0f*kminus)*ddr_ellipK_k2minus_rho + ddr_ellipE_k2minus_rho/kminus - ellipE_k2minus_rho*ddr_kminus/(k2minus);
            float ddz_brhoplus = (ddz_k2plus*rho)/(2.0f*kplus)*ellipK_k2plus_rho - (k2plus*rho - 2.0f)*2.0f*ddz_kplus/(4.0f*k2plus)*ellipK_k2plus_rho + (k2plus*rho - 2.0f)/(2.0f*kplus)*ddz_ellipK_k2plus_rho + ddz_ellipE_k2plus_rho/kplus - ellipE_k2plus_rho*ddz_kplus/(k2plus);
            float ddz_brhominus = (ddz_k2minus*rho)/(2.0f*kminus)*ellipK_k2minus_rho - (k2minus*rho - 2.0f)*2.0f*ddz_kminus/(4.0f*k2minus)*ellipK_k2minus_rho + (k2minus*rho - 2.0f)/(2.0f*kminus)*ddz_ellipK_k2minus_rho + ddz_ellipE_k2minus_rho/kminus - ellipE_k2minus_rho*ddz_kminus/(k2minus);
    
            float brhoplus = (k2plus*rho - 2.0f)/(2.0f*kplus) * ellipK_k2plus_rho + ellipE_k2plus_rho/kplus;
            float brhominus = (k2minus*rho - 2.0f)/(2.0f*kminus) * ellipK_k2minus_rho + ellipE_k2minus_rho/kminus;
            float Brho = 2e-7f * strength * (1.0f/L) * Mathf.Sqrt(a)/rho * (brhoplus - brhominus);
            float ddr_Brho = 2e-7f * strength * (1.0f/L) * (-Mathf.Sqrt(a)/rho/rho * (brhoplus - brhominus) + Mathf.Sqrt(a)/rho * (ddr_brhoplus - ddr_brhominus));
            float ddz_Brho = 2e-7f * strength * (1.0f/L) * Mathf.Sqrt(a)/rho * (ddz_brhoplus - ddz_brhominus);
    
            float ddr_bzplus = zetaplus*ddr_kplus*(ellipK_k2plus_rho + (a - rho)/(a + rho)*ellipPi_h2_k2plus_rho) + zetaplus*kplus*(ddr_ellipK_k2plus_rho - 1.0f/(a + rho)*ellipPi_h2_k2plus_rho - (a - rho)/Mathf.Pow(a + rho, 2)*ellipPi_h2_k2plus_rho + (a - rho)/(a + rho)*ddr_ellipPi_h2_k2plus_rho);
            float ddr_bzminus = zetaminus*ddr_kminus*(ellipK_k2minus_rho + (a - rho)/(a + rho)*ellipPi_h2_k2minus_rho) + zetaminus*kminus*(ddr_ellipK_k2minus_rho - 1.0f/(a + rho)*ellipPi_h2_k2minus_rho - (a - rho)/Mathf.Pow(a + rho, 2)*ellipPi_h2_k2minus_rho + (a - rho)/(a + rho)*ddr_ellipPi_h2_k2minus_rho);
            float ddz_bzplus = kplus*(ellipK_k2plus_rho + (a - rho)/(a + rho)*ellipPi_h2_k2plus_rho) + zetaplus*ddz_kplus*(ellipK_k2plus_rho + (a - rho)/(a + rho)*ellipPi_h2_k2plus_rho) + zetaplus*kplus*(ddz_ellipK_k2plus_rho + (a - rho)/(a + rho)*ddz_ellipPi_h2_k2plus_rho);
            float ddz_bzminus = kminus*(ellipK_k2minus_rho + (a - rho)/(a + rho)*ellipPi_h2_k2minus_rho) + zetaminus*ddz_kminus*(ellipK_k2minus_rho + (a - rho)/(a + rho)*ellipPi_h2_k2minus_rho) + zetaminus*kminus*(ddz_ellipK_k2minus_rho + (a - rho)/(a + rho)*ddz_ellipPi_h2_k2minus_rho);
            
            // float bzplus = zetaplus*kplus*(ellipK(k2plus*rho) + (a - rho)/(a + rho)*ellipPi(h2, k2plus*rho));
            // float bzminus = zetaminus*kminus*(ellipK(k2minus*rho) + (a - rho)/(a + rho)*ellipPi(h2, k2minus*rho));
            // float Bz = 1e-7f * strength * (1.0f/L/2.0f/Mathf.Sqrt(a)) * (bzplus - bzminus);
            float ddr_Bz = 1e-7f * strength * (1.0f/L/2.0f/Mathf.Sqrt(a)) * (ddr_bzplus - ddr_bzminus);
            float ddz_Bz = 1e-7f * strength * (1.0f/L/2.0f/Mathf.Sqrt(a)) * (ddz_bzplus - ddz_bzminus);
            
            // convert from local-cylindrical to local-cartesian
            float dr_dx = localPos.x/rho;
            float dr_dz = localPos.z/rho;
            float dz_dy = 1.0f;
            float dphi_dx = -localPos.z/rho;
            float dphi_dz = localPos.x/rho;
            // all the rest are zero
            
            float ddxloc_Bxloc = dr_dx * ddr_Brho*Mathf.Cos(phi) - dphi_dx * Brho*Mathf.Sin(phi);
            float ddxloc_Byloc = dr_dx * ddr_Bz;
            float ddxloc_Bzloc = dr_dx * ddr_Brho*Mathf.Sin(phi) + dphi_dx * Brho*Mathf.Cos(phi);
    
            float ddyloc_Bxloc = dz_dy * ddz_Brho*Mathf.Cos(phi);
            float ddyloc_Byloc = dz_dy * ddz_Bz;
            float ddyloc_Bzloc = dz_dy * ddz_Brho*Mathf.Sin(phi);
    
            float ddzloc_Bxloc = dr_dz * ddr_Brho*Mathf.Cos(phi) - dphi_dz * Brho*Mathf.Sin(phi);
            float ddzloc_Byloc = dr_dz * ddr_Bz;
            float ddzloc_Bzloc = dr_dz * ddr_Brho*Mathf.Sin(phi) + dphi_dz * Brho*Mathf.Cos(phi);
    
            // convert from local-cartesian to global-cartesian
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
    
            float ddxglob_Bxloc = dxloc_dxglob*ddxloc_Bxloc + dyloc_dxglob*ddyloc_Bxloc + dzloc_dxglob*ddzloc_Bxloc;
            float ddxglob_Byloc = dxloc_dxglob*ddxloc_Byloc + dyloc_dxglob*ddyloc_Byloc + dzloc_dxglob*ddzloc_Byloc;
            float ddxglob_Bzloc = dxloc_dxglob*ddxloc_Bzloc + dyloc_dxglob*ddyloc_Bzloc + dzloc_dxglob*ddzloc_Bzloc;
    
            float ddyglob_Bxloc = dxloc_dyglob*ddxloc_Bxloc + dyloc_dyglob*ddyloc_Bxloc + dzloc_dyglob*ddzloc_Bxloc;
            float ddyglob_Byloc = dxloc_dyglob*ddxloc_Byloc + dyloc_dyglob*ddyloc_Byloc + dzloc_dyglob*ddzloc_Byloc;
            float ddyglob_Bzloc = dxloc_dyglob*ddxloc_Bzloc + dyloc_dyglob*ddyloc_Bzloc + dzloc_dyglob*ddzloc_Bzloc;
    
            float ddzglob_Bxloc = dxloc_dzglob*ddxloc_Bxloc + dyloc_dzglob*ddyloc_Bxloc + dzloc_dzglob*ddzloc_Bxloc;
            float ddzglob_Byloc = dxloc_dzglob*ddxloc_Byloc + dyloc_dzglob*ddyloc_Byloc + dzloc_dzglob*ddzloc_Byloc;
            float ddzglob_Bzloc = dxloc_dzglob*ddxloc_Bzloc + dyloc_dzglob*ddyloc_Bzloc + dzloc_dzglob*ddzloc_Bzloc;
    
            ddx_B = transform.TransformDirection(new Vector3(ddxglob_Bxloc, ddxglob_Byloc, ddxglob_Bzloc));
            ddy_B = transform.TransformDirection(new Vector3(ddyglob_Bxloc, ddyglob_Byloc, ddyglob_Bzloc));
            ddz_B = transform.TransformDirection(new Vector3(ddzglob_Bxloc, ddzglob_Byloc, ddzglob_Bzloc));
        }
        catch {
            ddx_B = Vector3.zero;
            ddy_B = Vector3.zero;
            ddz_B = Vector3.zero;
        }
    }

	// Complete elliptic integrals as calculated in http://www.springerlink.com/content/q4122876627860x4/ (1979 paper)
	
    // first kind: K(m) = int((1 - m*sin(th)**2)**(-1/2), th=0 to pi/2)
	float ellipK(float m) {
	    // VERIFIED
        if (m >= 1.0) { throw new System.Exception(); }
        else if (m >= 1.0f - 1.490116119e-08f) {
            float y = 1.0f - m;
            float ta = 1.38629436112f + y*(0.09666344259f + y*0.03590092383f);
            float tb = -Mathf.Log(y) * (0.5f * y*(0.12498593597f + y*0.06880248576f));
            return ta + tb;
        }
        else {
            float y = 1.0f - m;
            return helperRF(0.0f, y, 1.0f);
        }
    }

	// second kind: E(m) = int((1 - m*sin(th)**2)**(1/2), th=0 to pi/2)
	float ellipE(float m) {
	    // VERIFIED
        if (m >= 1.0f) { throw new System.Exception(); }
        else if (m >= 1.0f - 1.490116119e-08f) {
            float y = 1.0f - m;
            float ta = 1.0f + y*(0.44325141463f + y*(0.06260601220f + 0.04757383546f*y));
            float tb = -y*Mathf.Log(y) * (0.24998368310f + y*(0.09200180037f + 0.04069697526f*y));
            return ta + tb;
        }
        else {
            float y = 1.0f - m;
            float rf = helperRF(0.0f, y, 1.0f);
            float rd = helperRD(0.0f, y, 1.0f);
            return rf - m/3.0f * rd;
        }
    }

	// third kind: Pi(n,m) = int((1 - n*sin(th)**2)**(-1) * (1 - m*sin(th)**2)**(-1/2), th=0 to pi/2)
	float ellipPi(float n, float m) {
	    // VERIFIED
	    n = -n;  // use the convention of Abramowitz and Stegun
	    
        if (m >= 1.0f  ||  n >= 1.0f) { throw new System.Exception(); }
        else {
            float rf = helperRF(0.0f, 1.0f - m, 1.0f);
            float rj = helperRJ(0.0f, 1.0f - m, 1.0f, 1.0f + n);
            return -1.0f * rf + n/3.0f * rj + 2.0f * (rf - (n/3.0f) * rj);
        }
    }

	// Incomplete elliptic integral of the first kind
	// F(phi, m) = int((1 - m*sin(th)**2)**(-1/2), th=0 to phi)
    float incomplete_ellipticF(float phi, float m) {
        // VERIFIED
        float nc = Mathf.Floor(phi/Mathf.PI + 0.5f);
        float phi_red = phi - nc * Mathf.PI;
        phi = phi_red;
        
        float sin_phi  = Mathf.Sin(phi);
        float sin2_phi = sin_phi*sin_phi;
        float x = 1.0f - sin2_phi;
        float y = 1.0f - m*sin2_phi;
        float rf = helperRF(x, y, 1.0f);
        float result = sin_phi * rf;
        if (nc == 0.0f) {
            return result;
        } else {
            float rk = ellipK(m);
            result += 2.0f * nc * rk;
        }
        return result;
    }
    
	// Incomplete elliptic integral of the second kind
	// E(phi, m) = int((1 - m*sin(th)**2)**(1/2), th=0 to phi)
	float incomplete_ellipticE(float phi, float m) {
	    // VERIFIED
        float nc = Mathf.Floor(phi/Mathf.PI + 0.5f);
        float phi_red = phi - nc * Mathf.PI;
        phi = phi_red;
        
        float sin_phi  = Mathf.Sin(phi);
        float sin2_phi = sin_phi * sin_phi;
        float x = 1.0f - sin2_phi;
        float y = 1.0f - m*sin2_phi;
        
        if(x < float.Epsilon) {
            float re = ellipE(m);
            if (sin_phi >= 0.0f) {
                return 2.0f*nc*re + re;
            }
            else {
                return 2.0f*nc*re - re;
            }
        }
        else {
            float sin3_phi = sin2_phi * sin_phi;
            float rf = helperRF(x, y, 1.0f);
            float rd = helperRD(x, y, 1.0f);
            float result = sin_phi * rf - m/3.0f * sin3_phi * rd;
            
            if (nc == 0.0f) {
                return result;
            }
            else {
                float re = ellipE(m);
                result += 2.0f * nc * re;
                return result;
            }
        }
    }

    // http://mathworld.wolfram.com/JacobiZetaFunction.html
    float jacobi_zeta(float phi, float m) {
        return incomplete_ellipticE(phi, m) - (ellipE(m)*incomplete_ellipticF(phi, m)/ellipK(m));
    }

    // http://mathworld.wolfram.com/HeumanLambdaFunction.html
    float heuman_lambda(float phi, float m) {
        return (incomplete_ellipticF(phi, 1.0f - m)/ellipK(1.0f - m)) * ((2.0f/Mathf.PI) * ellipK(m) * jacobi_zeta(phi, 1.0f - m));
    }

    // helper functions
    
    float helperRF(float x, float y, float z) {
        // VERIFIED
        float lolim = 5.0f * float.Epsilon;
        float uplim = 0.2f * float.MaxValue;
        float errtol = 0.03f;

        if (x < 0.0f  ||  y < 0.0f  ||  z < 0.0f) { throw new System.Exception(); }
        else if (x+y < lolim  ||  x+z < lolim  ||  y+z < lolim) { throw new System.Exception(); }
        else if (Mathf.Max(x, Mathf.Max(y, z)) < uplim) { 
            float c1 = 1.0f / 24.0f;
            float c2 = 3.0f / 44.0f;
            float c3 = 1.0f / 14.0f;
            float xn = x;
            float yn = y;
            float zn = z;
            float mu, xndev, yndev, zndev, e2, e3, s;
            while (true) {
                float epslon, lamda;
                float xnroot, ynroot, znroot;
                mu = (xn + yn + zn) / 3.0f;
                xndev = 2.0f - (mu + xn) / mu;
                yndev = 2.0f - (mu + yn) / mu;
                zndev = 2.0f - (mu + zn) / mu;
                epslon = Mathf.Max(Mathf.Abs(xndev), Mathf.Max(Mathf.Abs(yndev), Mathf.Abs(zndev)));
                if (epslon < errtol) break;
                xnroot = Mathf.Sqrt(xn);
                ynroot = Mathf.Sqrt(yn);
                znroot = Mathf.Sqrt(zn);
                lamda = xnroot * (ynroot + znroot) + ynroot * znroot;
                xn = (xn + lamda) * 0.25f;
                yn = (yn + lamda) * 0.25f;
                zn = (zn + lamda) * 0.25f;
            }
            e2 = xndev * yndev - zndev * zndev;
            e3 = xndev * yndev * zndev;
            s = 1.0f + (c1 * e2 - 0.1f - c2 * e3) * e2 + c3 * e3;
            return s / Mathf.Sqrt(mu);
            }
        else { throw new System.Exception(); }
    }

    float helperRD(float x, float y, float z) {
        // VERIFIED
        float errtol = 0.03f;
        float lolim = 2.0f/Mathf.Pow(float.MaxValue, 2.0f/3.0f);
        // float uplim = Mathf.Pow(0.1f*errtol/float.Epsilon, 2.0f/3.0f);   // evaluates to Infinity in floating-point precision

        if (Mathf.Min(x,y) < 0.0  ||  Mathf.Min(x+y, z) < lolim) { throw new System.Exception(); }
        else { // else if (Mathf.Max(x, Mathf.Max(y, z)) < uplim) {
            float c1 = 3.0f / 14.0f;
            float c2 = 1.0f /  6.0f;
            float c3 = 9.0f / 22.0f;
            float c4 = 3.0f / 26.0f;
            float xn = x;
            float yn = y;
            float zn = z;
            float sigma  = 0.0f;
            float power4 = 1.0f;
            float ea, eb, ec, ed, ef, s1, s2;
            float mu, xndev, yndev, zndev;
            while (true) {
                float xnroot, ynroot, znroot, lamda;
                float epslon;
                mu = (xn + yn + 3.0f * zn) * 0.2f;
                xndev = (mu - xn) / mu;
                yndev = (mu - yn) / mu;
                zndev = (mu - zn) / mu;
                epslon = Mathf.Max(Mathf.Abs(xndev), Mathf.Max(Mathf.Abs(yndev), Mathf.Abs(zndev)));
                if (epslon < errtol) break;
                xnroot = Mathf.Sqrt(xn);
                ynroot = Mathf.Sqrt(yn);
                znroot = Mathf.Sqrt(zn);
                lamda = xnroot * (ynroot + znroot) + ynroot * znroot;
                sigma += power4 / (znroot * (zn + lamda));
                power4 *= 0.25f;
                xn = (xn + lamda) * 0.25f;
                yn = (yn + lamda) * 0.25f;
                zn = (zn + lamda) * 0.25f;
            }
            ea = xndev * yndev;
            eb = zndev * zndev;
            ec = ea - eb;
            ed = ea - 6.0f * eb;
            ef = ed + ec + ec;
            s1 = ed * (-c1 + 0.25f * c3 * ed - 1.5f * c4 * zndev * ef);
            s2 = zndev * (c2 * ef + zndev * (-c3 * ec + zndev * c4 * ea));
            return 3.0f * sigma + power4 * (1.0f + s1 + s2) / (mu * Mathf.Sqrt(mu));
        }
        // else { throw new System.Exception(); }
    }

    float helperRC(float x, float y) {
        // VERIFIED
        float lolim = 5.0f * float.Epsilon;
        float uplim = 0.2f * float.MaxValue;
        float errtol = 0.03f;

        if (x < 0.0f  ||  y < 0.0f  ||  x + y < lolim) { throw new System.Exception(); }
        else if (Mathf.Max(x, y) < uplim) { 
            float c1 = 1.0f / 7.0f;
            float c2 = 9.0f / 22.0f;
            float xn = x;
            float yn = y;
            float mu, sn, lamda, s;
            while (true) {
                mu = (xn + yn + yn) / 3.0f;
                sn = (yn + mu) / mu - 2.0f;
                if (Mathf.Abs(sn) < errtol) break;
                lamda = 2.0f * Mathf.Sqrt(xn) * Mathf.Sqrt(yn) + yn;
                xn = (xn + lamda) * 0.25f;
                yn = (yn + lamda) * 0.25f;
            }
            s = sn * sn * (0.3f + sn * (c1 + sn * (0.375f + sn * c2)));
            return (1.0f + s) / Mathf.Sqrt(mu);
        }
        else { throw new System.Exception(); }
    }

    float helperRJ(float x, float y, float z, float p) {
        // VERIFIED
        float errtol = 0.03f;
        float lolim = Mathf.Pow(5.0f * float.Epsilon, 1.0f/3.0f);
        float uplim = 0.3f * Mathf.Pow(0.2f * float.MaxValue, 1.0f/3.0f);

        if (x < 0.0f  ||  y < 0.0f  ||  z < 0.0f) { throw new System.Exception(); }
        else if (x + y < lolim  ||  x + z < lolim  ||  y + z < lolim  ||  p < lolim) { throw new System.Exception(); }
        else if (Mathf.Max(Mathf.Max(x, y), Mathf.Max(z, p)) < uplim) {
            float c1 = 3.0f / 14.0f;
            float c2 = 1.0f /  3.0f;
            float c3 = 3.0f / 22.0f;
            float c4 = 3.0f / 26.0f;
            float xn = x;
            float yn = y;
            float zn = z;
            float pn = p;
            float sigma = 0.0f;
            float power4 = 1.0f;
            float mu, xndev, yndev, zndev, pndev;
            float ea, eb, ec, e2, e3, s1, s2, s3;
            while (true) {
                float xnroot, ynroot, znroot;
                float lamda, alfa, beta;
                float epslon;
                mu = (xn + yn + zn + pn + pn) * 0.2f;
                xndev = (mu - xn) / mu;
                yndev = (mu - yn) / mu;
                zndev = (mu - zn) / mu;
                pndev = (mu - pn) / mu;
                epslon = Mathf.Max(Mathf.Max(Mathf.Abs(xndev), Mathf.Abs(yndev)), Mathf.Max(Mathf.Abs(zndev), Mathf.Abs(pndev)));
                if(epslon < errtol) break;
                xnroot = Mathf.Sqrt(xn);
                ynroot = Mathf.Sqrt(yn);
                znroot = Mathf.Sqrt(zn);
                lamda = xnroot * (ynroot + znroot) + ynroot * znroot;
                alfa = pn * (xnroot + ynroot + znroot) + xnroot * ynroot * znroot;
                alfa = alfa * alfa;
                beta = pn * (pn + lamda) * (pn + lamda);
                
                float rc = helperRC(alfa, beta);
                sigma += power4 * rc;
                power4 *= 0.25f;
                xn = (xn + lamda) * 0.25f;
                yn = (yn + lamda) * 0.25f;
                zn = (zn + lamda) * 0.25f;
                pn = (pn + lamda) * 0.25f;
            }
            
            ea = xndev * (yndev + zndev) + yndev * zndev;
            eb = xndev * yndev * zndev;
            ec = pndev * pndev;
            e2 = ea - 3.0f * ec;
            e3 = eb + 2.0f * pndev * (ea - ec);
            s1 = 1.0f + e2 * (-c1 + 0.75f * c3 * e2 - 1.5f * c4 * e3);
            s2 = eb * (0.5f * c2 + pndev * (-c3 - c3 + pndev * c4));
            s3 = pndev * ea * (c2 - pndev * c3) - c2 * pndev * ec;
            return 3.0f * sigma + power4 * (s1 + s2 + s3) / (mu * Mathf.Sqrt(mu));
        }
        else { throw new System.Exception(); }
    }
}
