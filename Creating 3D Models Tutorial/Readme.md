 This is a detailed tutorial to show the steps of creating the 3D models used in VR-Caps

1- The "NBIA Software" is downloaded from the Cancer Imaging Archive website that we get the DICOMs from, it is used to download the DICOMs
When you want to download the datasets, the website has a manual on how to download this software and when you are about to download it shows you how

This is the [link](https://wiki.cancerimagingarchive.net/display/NBIA/Downloading+TCIA+Images)

<p align="center">
<img src='images/1_NBIA Data Retriever.PNG' width=512/> 
</p>

2- Install InVesalius software and open it and import the datatset which was downloaded in step 1. Then select the "Supine" set of dicoms and click on "Import"

<p align="center">
<img src='images/2_InVesalius3 (Importing DICOM and selecting Supine set).PNG' width=512/> 
</p>

3- The following screen will show, from the dropdown menu on the left "Set predefined or manual threshold" select "Soft TIssue" which will cause the green selection that is shown, then click on "Create Surface"

<p align="center">
<img src='images/3_Selecting Soft Tissue & click on Create Surface.PNG' width=512/> 
</p>

4- When it is done you get the 3D surface in the right bottom corner, then from the left sidebar click on "Export 3D Surface" and save the 3D model.

<p align="center">
<img src='images/4_3D Created then click on Export 3D surface.PNG' width=512/> 
</p>

5- Open Blender and import it.

<p align="center">
<img src='images/5_Import to Blender.PNG' width=512/> 
</p>

6- Edit the mesh by deleting the vertices that surround the organs.

<p align="center">
<img src='images/6_Remove the unnecessary mesh.PNG' width=512/> 
</p>

7- Create seams, which are like imaginary cuts that allow us to project the mesh on a 2D plane (one vertical seam throughout the length of the organ, and several horizontal cuts to make small segments.)

<p align="center">
<img src='images/7_Marking Seams to unwrap the mesh (UV Unwrapping).PNG' width=512/> 
</p>

8- Then by clicking on UV >> Unwrap, it will unwrap the mesh according to our seams as shown on the left as segments

<p align="center">
<img src='images/8_UV Unwrapping.PNG' width=512/> 
</p>

9- Apply a material to the colon and choose an image (The colon texture Image from the files). 

<p align="center">
<img src='images/9_Add material (Colon Texture Image).PNG' width=512/> 
</p>

10- When it shows the textured organ, if the projection seems to be too big (the veins are too large) you have to scale the UV maps to make it cover a larger surface area from the texture image.

<p align="center">
<img src='images/10_The textured organs (UV unwrapped mesh not resized yet).PNG' width=512/> 
</p>

11- Scale the UV Maps until the textured organ seems to be having normal sized veins.

<p align="center">
<img src='images/11_Resize the mesh.PNG' width=512/> 
</p>

12- You will see the textured organs after scaling the UV maps

<p align="center">
<img src='images/12_Textured organ with resized projection areas of UV map.PNG' width=512/> 
</p>

Now saving this as a .blend file, and just importing it to Unity is sufficient

After importing to Unity,  choose the "Colon material" and it will be projected the exact same way as it was in Blender (based on the UV Maps)
