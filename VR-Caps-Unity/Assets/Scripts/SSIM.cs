// Compute Structural Similarity Index (SSIM) image quality metrics                                                                                 
// See http://www.ece.uwaterloo.ca/~z70wang/research/ssim/                                                                                          
// C# implementation Copyright Chris Lomont 2009-2011                                                                                               
// Send fixes and comments to WWW.LOMONT.ORG                                                                                                        
                                                                                                                                                    
/* History:                                                                                                                                         
 * June 2011                                                                                                                                        
 * 0.91 - Fix to Texture2D creation to prevent locking file handles                                                                                    
 *                                                                                                                                                  
 * Sept 2009                                                                                                                                        
 * 0.9  - Initial Release                                                                                                                           
 *                                                                                                                                                  
 *                                                                                                                                                  
 */                                                                                                                                                 
                                                                                                                                                    
using System;                                                                                                                                       
// using System.Drawing; 
using System.Collections.Generic; 
using UnityEngine;                                                                                                                             
// namespace Lomont                                                                                                                                    
    // {                                                                                                                                               
    class SSIM                                                                                                                                      
        {                                                                                                                                           
        /// <summary>                                                                                                                               
        /// The version of the SSIM code                                                                                                            
        /// </summary>                                                                                                                              
        internal static string Version { get { return "0.91";} }                                                                                    
                                                                                                                                                    
        /// <summary>                                                                                                                               
        /// Compute from two linear ushort grayscale images with given size and bitdepth                                                            
        /// </summary>                                                                                                                              
        /// <param name="img1">Image 1 data</param>                                                                                                 
        /// <param name="img2">Image 2 data</param>                                                                                                 
        /// <param name="w">width</param>                                                                                                           
        /// <param name="h">height</param>                                                                                                          
        /// <param name="depth">Bit depth (1-16)</param>                                                                                            
        /// <returns></returns>                                                                                                                     
        internal double Index(ushort[] img1, ushort[] img2, int w, int h, int depth)                                                                
            {                                                                                                                                       
            L = (1 << depth) - 1;                                                                                                                   
            return ComputeSSIM(ConvertLinear(img1, w, h), ConvertLinear(img2, w, h));                                                               
            }                                                                                                                                       
                                                                                                                                                    
        /// <summary>                                                                                                                               
        /// Take two System.Drawing.Texture2Ds                                                                                                         
        /// </summary>                                                                                                                              
        /// <param name="img1"></param>                                                                                                             
        /// <param name="img2"></param>                                                                                                             
        /// <returns></returns>                                                                                                                     
        internal double Index(Texture2D img1, Texture2D img2)                                                                                             
            {                                                                                                                                       
            L = 255; // todo - this assumes 8 bit, but color conversion later is always 8 bit, so ok?                                               
            return ComputeSSIM(ConvertTexture2D(img1), ConvertTexture2D(img2));                                                                           
            }                                                                                                                                       
                                                                                                                                                    
                                                                                                                                       
                                                                                                                                                    
        #region Implementation                                                                                                                      
                                                                                                                                                    
        #region Locals                                                                                                                              
        // default settings, names from paper                                                                                                       
        internal double K1 = 0.01, K2 = 0.03;                                                                                                       
        internal double L = 255;                                                                                                                    
        readonly Grid window = Gaussian(11, 1.5);                                                                                                   
        #endregion                                                                                                                                  
                                                                                                                                                    
        /// <summary>                                                                                                                               
        /// Compute the SSIM index of two same sized Grids                                                                                          
        /// </summary>                                                                                                                              
        /// <param name="img1">The first Grid</param>                                                                                               
        /// <param name="img2">The second Grid</param>                                                                                              
        /// <returns>SSIM index</returns>                                                                                                           
        double ComputeSSIM(Grid img1, Grid img2)                                                                                                    
            {                                                                                                                                       
            // uses notation from paper                                                                                                             
            // automatic downsampling                                                                                                               
            int f = (int)Math.Max(1, Math.Round(Math.Min(img1.width,img1.height) / 256.0));                                                         
            if (f > 1)                                                                                                                              
                { // downsampling by f                                                                                                              
                  // use a simple low-pass filter and subsample by f                                                                                
                img1 = SubSample(img1, f);                                                                                                          
                img2 = SubSample(img2, f);                                                                                                          
                }                                                                                                                                   
                                                                                                                                                    
            // normalize window - todo - do in window set {}                                                                                        
            double scale = 1.0/window.Total;                                                                                                        
            Grid.Op((i, j) => window[i, j] * scale, window);                                                                                        
                                                                                                                                                    
            // image statistics                                                                                                                     
            var mu1 = Filter(img1, window);                                                                                                         
            var mu2 = Filter(img2, window);                                                                                                         
                                                                                                                                                    
            var mu1mu2 = mu1 * mu2;                                                                                                                 
            var mu1SQ  = mu1 * mu1;                                                                                                                 
            var mu2SQ  = mu2 * mu2;                                                                                                                 
                                                                                                                                                    
            var sigma12  = Filter(img1 * img2, window) - mu1mu2;                                                                                    
            var sigma1SQ = Filter(img1 * img1, window) - mu1SQ;                                                                                     
            var sigma2SQ = Filter(img2 * img2, window) - mu2SQ;                                                                                     
                                                                                                                                                    
            // constants from the paper                                                                                                             
            double C1 = K1 * L; C1 *= C1;                                                                                                           
            double C2 = K2 * L; C2 *= C2;                                                                                                           
                                                                                                                                                    
            Grid ssim_map = null;                                                                                                                   
            if ((C1 > 0) && (C2 > 0))                                                                                                               
                {                                                                                                                                   
                ssim_map = Grid.Op((i, j) =>                                                                                                        
                    (2 * mu1mu2[i, j] + C1) * (2 * sigma12[i, j] + C2) /                                                                            
                    (mu1SQ[i, j] + mu2SQ[i, j] + C1) / (sigma1SQ[i, j] + sigma2SQ[i, j] + C2),                                                      
                    new Grid(mu1mu2.width, mu1mu2.height));                                                                                         
                }                                                                                                                                   
            else                                                                                                                                    
                {                                                                                                                                   
                var num1 = Linear(2, mu1mu2, C1);                                                                                                   
                var num2 = Linear(2, sigma12, C2);                                                                                                  
                var den1 = Linear(1, mu1SQ + mu2SQ, C1);                                                                                            
                var den2 = Linear(1, sigma1SQ + sigma2SQ, C2);                                                                                      
                                                                                                                                                    
                var den = den1 * den2; // total denominator                                                                                         
                ssim_map = new Grid(mu1.width, mu1.height);                                                                                         
                for (int i = 0; i < ssim_map.width; ++i)                                                                                            
                    for (int j = 0; j < ssim_map.height; ++j)                                                                                       
                        {                                                                                                                           
                        ssim_map[i, j] = 1;                                                                                                         
                        if (den[i, j] > 0)                                                                                                          
                            ssim_map[i, j] = num1[i, j] * num2[i, j] / (den1[i, j] * den2[i, j]);                                                   
                        else if ((den1[i, j] != 0) && (den2[i, j] == 0))                                                                            
                            ssim_map[i, j] = num1[i, j] / den1[i, j];                                                                               
                        }                                                                                                                           
                }                                                                                                                                   
                                                                                                                                                    
            // average all values                                                                                                                   
            return ssim_map.Total / (ssim_map.width * ssim_map.height);                                                                             
            } // ComputeSSIM                                                                                                                        
                                                                                                                                                    
                                                                                                                                                    
        #region Grid                                                                                                                                
        /// <summary>                                                                                                                               
        /// Hold a grid of doubles as an array with appropriate operators                                                                           
        /// </summary>                                                                                                                              
        class Grid                                                                                                                                  
            {                                                                                                                                       
            double[,] data;                                                                                                                         
            internal int width, height;                                                                                                             
            internal Grid(int w, int h)                                                                                                             
                {                                                                                                                                   
                data = new double[w, h];                                                                                                            
                width = w;                                                                                                                          
                height = h;                                                                                                                         
                }                                                                                                                                   
                                                                                                                                                    
            /// <summary>                                                                                                                           
            /// Indexer to read the i,j item                                                                                                        
            /// </summary>                                                                                                                          
            /// <param name="i"></param>                                                                                                            
            /// <param name="j"></param>                                                                                                            
            /// <returns></returns>                                                                                                                 
            internal double this[int i, int j]                                                                                                      
                {                                                                                                                                   
                get { return data[i, j]; }                                                                                                          
                set { data[i, j] = value; }                                                                                                         
                }                                                                                                                                   
                                                                                                                                                    
            /// <summary>                                                                                                                           
            /// Get the summed value from the Grid                                                                                                  
            /// </summary>                                                                                                                          
            internal double Total                                                                                                                   
                {                                                                                                                                   
                get                                                                                                                                 
                    {                                                                                                                               
                    double s = 0;                                                                                                                   
                    foreach (var d in data) s += d;                                                                                                 
                    return s;                                                                                                                       
                    }                                                                                                                               
                }                                                                                                                                   
                                                                                                                                                    
            /// <summary>                                                                                                                           
            /// componentwise addition of Grids                                                                                                     
            /// </summary>                                                                                                                          
            /// <param name="a"></param>                                                                                                            
            /// <param name="b"></param>                                                                                                            
            /// <returns></returns>                                                                                                                 
            static public Grid operator+(Grid a, Grid b)                                                                                            
                {                                                                                                                                   
                return Op((i,j)=>a[i,j]+b[i,j],new Grid(a.width,a.height));                                                                         
                }                                                                                                                                   
                                                                                                                                                    
            /// <summary>                                                                                                                           
            /// componentwise subtraction of Grids                                                                                                  
            /// </summary>                                                                                                                          
            /// <param name="a"></param>                                                                                                            
            /// <param name="b"></param>                                                                                                            
            /// <returns></returns>                                                                                                                 
            static public Grid operator-(Grid a, Grid b)                                                                                            
                {                                                                                                                                   
                return Op((i, j) => a[i, j] - b[i, j], new Grid(a.width, a.height));                                                                
                }                                                                                                                                   
                                                                                                                                                    
            /// <summary>                                                                                                                           
            /// componentwise multiplication of Grids                                                                                               
            /// </summary>                                                                                                                          
            /// <param name="a"></param>                                                                                                            
            /// <param name="b"></param>                                                                                                            
            /// <returns></returns>                                                                                                                 
            static public Grid operator*(Grid a, Grid b)                                                                                            
                {                                                                                                                                   
                return Op((i, j) => a[i, j] * b[i, j], new Grid(a.width, a.height));                                                                
                }                                                                                                                                   
                                                                                                                                                    
            /// <summary>                                                                                                                           
            /// componentwise division of Grids                                                                                                     
            /// </summary>                                                                                                                          
            /// <param name="a"></param>                                                                                                            
            /// <param name="b"></param>                                                                                                            
            /// <returns></returns>                                                                                                                 
            static public Grid operator/(Grid a, Grid b)                                                                                            
                {                                                                                                                                   
                return Op((i, j) => a[i, j] / b[i, j], new Grid(a.width, a.height));                                                                
                }                                                                                                                                   
                                                                                                                                                    
            /// <summary>                                                                                                                           
            /// Generic function maps (i,j) onto the given grid                                                                                     
            /// </summary>                                                                                                                          
            /// <param name="f"></param>                                                                                                            
            /// <param name="a"></param>                                                                                                            
            /// <returns></returns>                                                                                                                 
            static internal Grid Op(Func<int,int,double> f,Grid g)                                                                                  
                {                                                                                                                                   
                int w = g.width, h = g.height;                                                                                                      
                for (int i = 0; i < w; ++i)                                                                                                         
                    for (int j = 0; j < h; ++j)                                                                                                     
                        g[i, j] = f(i,j);                                                                                                           
                return g;                                                                                                                           
                }                                                                                                                                   
                                                                                                                                                    
            } // class Grid                                                                                                                         
        #endregion //Grid                                                                                                                           
                                                                                                                                                    
                                                                                                                                                    
        /// <summary>                                                                                                                               
        /// Create a gaussian window of the given size and standard deviation                                                                       
        /// </summary>                                                                                                                              
        /// <param name="size">Odd number</param>                                                                                                   
        /// <param name="sigma">Gaussian std deviation</param>                                                                                      
        /// <returns></returns>                                                                                                                     
        static Grid Gaussian(int size, double sigma)                                                                                                
            {                                                                                                                                       
            var filter = new Grid(size, size);                                                                                                      
            double s2 = sigma * sigma, c = (size-1)/2.0, dx, dy;                                                                                    
                                                                                                                                                    
            Grid.Op((i, j) =>                                                                                                                       
                {                                                                                                                                   
                    dx = i - c;                                                                                                                     
                    dy = j - c;                                                                                                                     
                    return Math.Exp(-(dx * dx + dy * dy) / (2 * s2));                                                                               
                },                                                                                                                                  
                filter);                                                                                                                            
            var scale = 1.0/filter.Total;                                                                                                           
            Grid.Op((i, j) => filter[i, j] * scale, filter);                                                                                        
            return filter;                                                                                                                          
            }                                                                                                                                       
                                                                                                                                                    
        /// <summary>                                                                                                                               
        /// subsample a grid by step size, averaging each box into the result value                                                                 
        /// </summary>                                                                                                                              
        /// <returns></returns>                                                                                                                     
        static Grid SubSample(Grid img, int skip)                                                                                                   
            {                                                                                                                                       
            int w = img.width;                                                                                                                      
            int h = img.height;                                                                                                                     
            double scale = 1.0 / (skip * skip);                                                                                                     
            var ans = new Grid(w / skip, h / skip);                                                                                                 
            for (int i = 0; i < w-skip ; i+=skip)                                                                                                   
                for (int j = 0; j < h-skip ; j+=skip)                                                                                               
                    {                                                                                                                               
                    double sum = 0;                                                                                                                 
                    for (int x = i; x < i + skip; ++x)                                                                                              
                        for (int y = j; y < j+ skip; ++y)                                                                                           
                            sum += img[x, y];                                                                                                       
                    ans[i/skip, j/skip] = sum * scale;                                                                                              
                    }                                                                                                                               
            return ans;                                                                                                                             
            }                                                                                                                                       
                                                                                                                                                    
        /// <summary>                                                                                                                               
        /// Apply filter, return only center part.                                                                                                  
        /// C = Filter(A,B) should be same as matlab filter2( ,'valid')                                                                             
        /// </summary>                                                                                                                              
        /// <returns></returns>                                                                                                                     
        static Grid Filter(Grid a, Grid b)                                                                                                          
            {                                                                                                                                       
#if false                                                                                                                                           
            int ax = a.width, ay = a.height;                                                                                                        
            int bx = b.width, by = b.height;                                                                                                        
            int bcx = (bx + 1) / 2, bcy = (by + 1) / 2; // center position                                                                          
            var c = new Grid(ax - bx + 1, ay - by + 1);                                                                                             
            for (int i = bx - bcx + 1; i < ax - bx; ++i)                                                                                            
                for (int j = by - bcy + 1; j < ay - by; ++j)                                                                                        
                    {                                                                                                                               
                    double sum = 0;                                                                                                                 
                    for (int x = bcx - bx + 1 + i; x < 1 + i + bcx; ++x)                                                                            
                        for (int y = bcy - by + 1 + j; y < 1 + j + bcy; ++y)                                                                        
                            sum += a[x, y] * b[bx - bcx - 1 - i + x, by - bcy - 1 - j + y];                                                         
                    c[i - bcx, j - bcy] = sum;                                                                                                      
                    }                                                                                                                               
            return c;                                                                                                                               
#else                                                                                                                                               
            // todo - check and clean this                                                                                                          
            int ax = a.width, ay = a.height;                                                                                                        
            int bx = b.width, by = b.height;                                                                                                        
            int bcx = (bx + 1) / 2, bcy = (by + 1) / 2; // center position                                                                          
            var c = new Grid(ax - bx + 1, ay - by + 1);                                                                                             
            for (int i = bx - bcx + 1; i < ax - bx; ++i)                                                                                            
                for (int j = by - bcy + 1; j < ay - by; ++j)                                                                                        
                    {                                                                                                                               
                    double sum = 0;                                                                                                                 
                    for (int x = bcx - bx + 1 + i; x < 1 + i + bcx; ++x)                                                                            
                        for (int y = bcy - by + 1 + j; y < 1 + j + bcy; ++y)                                                                        
                            sum += a[x, y] * b[bx - bcx - 1 - i + x, by - bcy - 1 - j + y];                                                         
                    c[i - bcx, j - bcy] = sum;                                                                                                      
                    }                                                                                                                               
            return c;                                                                                                                               
#endif                                                                                                                                              
            }                                                                                                                                       
                                                                                                                                                    
        /// <summary>                                                                                                                               
        /// componentwise s*a[i,j]+c->a[i,j]                                                                                                        
        /// </summary>                                                                                                                              
        /// <param name="s"></param>                                                                                                                
        /// <param name="a"></param>                                                                                                                
        /// <param name="c"></param>                                                                                                                
        /// <returns></returns>                                                                                                                     
        static Grid Linear(double s, Grid a, double c)                                                                                              
            {                                                                                                                                       
            return Grid.Op((i, j) => s * a[i, j] + c, new Grid(a.width,a.height));                                                                  
            }                                                                                                                                       
                                                                                                                                                    
        #region Conversion                                                                                                                          
        /// <summary>                                                                                                                               
        /// convert image from 1D ushort to Grid                                                                                                    
        /// </summary>                                                                                                                              
        /// <param name="img"></param>                                                                                                              
        /// <param name="w"></param>                                                                                                                
        /// <param name="h"></param>                                                                                                                
        /// <returns></returns>                                                                                                                     
        static Grid ConvertLinear(ushort[] img, int w, int h)                                                                                       
            {                                                                                                                                       
            return Grid.Op((i,j)=>img[i+j*w],new Grid(w,h));                                                                                        
            }                                                                                                                                       
                                                                                                                                                    
        /// <summary>                                                                                                                               
        /// Convert a Texture2D to a grayscale Grid                                                                                                    
        /// </summary>                                                                                                                              
        /// <returns></returns>                                                                                                                     
        static Grid ConvertTexture2D(Texture2D bmp)                                                                                                       
            {                                                                                                                                       
            return Grid.Op((i, j) => { Color c = bmp.GetPixel(i, j); return 0.3 * c[0] + 0.59 * c[1] + 0.11 * c[2]; }, new Grid(bmp.width,bmp.height));
            }                                                                                                                                       
        #endregion // Conversion                                                                                                                    
                                                                                                                                                    
        #endregion                                                                                                                                  
        } // class SSIM                                                                                                                             
    // } // namespace Lomont                                                                                                                           
                                                                                                                                                    
// end of file                                                                                                                                      
                            
