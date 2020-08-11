# Overview

We introduce a virtual active capsule endoscopy platform developed in Unity that provides a simulation environment to develop and test algorithms. Also, we propose a sim2real method which makes use of cycle-consistent image domain style transfer and feature domain adaptation techniques to adapt representations at both the pixel-level and feature-level to solve real medical data tasks. Using that pipeline, we perform various evaluations for common robotics and computer vision tasks of active capsule endoscopy such as abnormality detection, classification, depth estimation, SLAM (Simultaneous Localization and Mapping), autonomous navigation, learning control of endoscopic capsule robot with magnetic field inside GI-tract organs, super-resolution, etc. The demonstration of our virtual environment is available on [YouTube](https://www.youtube.com/watch?v=UQ2u3CIUciA).

Our main contributions are as follows:
  - We propose synthetic data generating tool for creating fully labeled data.
  - Using our simulation environment, we provide a platform for testing numerous highly realistic scenarios.
  
In addition, VR-Caps offers numerous advantages over physical testing in the context of both the active and passive capsule endoscopy such as:
  - VR-Caps enables accelerated both jointly and independent design, testing and optimization process for software and hardware components
  - The marginal cost of synthetic data is low in terms of both time, money, effort and supervision requirements
  - System properties and parameter values can be easily altered to assess sensitivity and robustness
  - VR-Caps carries no risks to live animals or humans
  - VR-Caps can offer reproducibility, which is valuable in the scientific pursuit of new algorithms
  - The prevalence of rare diseases can be exaggerated in VR-Caps to provide data that may be infeasible or impossible to obtain from human study participants


### Summary of Our Work

#### Our simulation environment: VR-Caps

A physician is performing magnetically actuated active capsule endoscopy on the patient. The Franka Emika 7 DOF robotic arm is placed next to the patient holding a permanent magnet to control capsule endoscope swallowed by the patient. On the right side of the figure, our realistic 3D colon, small intestine and stomach models are shown. Models are generated based on real patient's CT (computer tomography) and texture is given based on real endoscopic images. Frames taken via capsule camera are given in RGB format and estimated depth maps are given correspondingly. In the last two rows, from a patient who has polyps in his colon, the result of segmentation algorithm is shown.

<p align="center">
<img src='img/Fig1.png' width=512/> 
</p>

#### 3D organ construction and texture assignment process

Using an open-source 3D medical image reconstruction software (i.e., InVesalius), 3D organ models were created from CT scans. The reconstructed 3D model was then imported into Blender for further processing. The imported model consists of bones, fat, skin, and other artifacts that are removed so that only the geometries of the colon, small intestines and stomach remain. As a next step, textures are created using the Kvasir dataset. In order to create the main mucosa texture from the Kvasir dataset, various endoscopy images are stitched together and applied on the model inner surface to generate clear, non-blurry and continuous mucosa walls.

<p align="center">
<img src='img/Fig2.1.png' width=512/> 
</p>

## Evaluated Tasks 

#### 1. Area Coverage

For that purpose, we propose a Deep Reinforcement Learning (DRL) based active control method that has a goal of learning a maximum coverage policy for human colon monitoring within a minimal operation time.

#### 2. Pose and Depth Estimation

To illustrate the effectiveness of VR-Caps environment in terms of neural network training for pose and depth estimation, we trained a state-of-the-art method, SC-SfMLearner algorithm, using synthetic data with pose and depth ground truths acquired from VR-Caps environment.

#### 3. 3D Reconstruction

In this work, we propose and evaluate a hybrid 3D reconstruction technique including steps Otsu threshold-based reflection detection, OPENCV inpainting-based reflection suppression, feature matching and tracking based image stitching and non-lambertion surface reconstruction. To exemplify the effectiveness of Unity data, we compare the results of reconstructions both on real and synthetic data. 

#### 4. Disease Classification

We mimic the 3 diseases (i.e., Polyps, Haemorrhage and Ulcerative Collitis) in our simulation environment. Hemorrage and Ulcerative Collitis are created based on the real endoscopy images from Kvasir dataset mimicking the abnormal mucosa texture. As polyps are not only distintive in texture but also in topology, we use CT scans from patients who have polyps and use this 3D morphological information to reconstruct 3D organs inside our environment.  instances with different severities ranging from grade 1 to grade 4, three different grades of ulcerative colitis, and different polyps instances with various shapes and sizes.

<p align="center">
<img src='img/DISEASES.png' width=512/> 
</p>

#### 5. Super Resolution

We benchmarked the effectivity of the Unity environment using Deep Super-Resolution for Capsule Endoscopy (EndoL2H) network based on the dilemma of high camera resolution coming with increasing the size of the optics and the sensor array.
