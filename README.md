# Virtual Capsule Endoscopy

If you use this code, please cite:

>    Kagan Incetan,Abdulhamid Obeid, Omer Celik, Kutsev Bengisu Ozyoruk, Helder Araujo, Hunter B. Gilbert, Nicholas J. Durr, Faisal Mahmood, Mehmet Turan. "Virtual Active Capsule Endoscopy.".

## Overview

We introduce a virtual active capsule endoscopy platform developed in Unity that provides a simulation environment to test new devices and algorithms. Also, we propose a sim2real method which makes use of cycle-consistent image domain style transfer and feature domain adaptation techniques to adapt representations at both the pixel-level and feature-level to solve real medical data tasks. Using that pipeline, we perform various evaluations for common robotics and computer vision tasks of active capsule endoscopy such as abnormality detection, classification, depth estimation, SLAM (Simultaneous Localization and Mapping), autonomous navigation, learning control of endoscopic capsule robot with magnetic field inside GI-tract organs, super-resolution, etc.

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

#### 3D organ construction and texture assignment process

**a)** We use 3D models reconstructed from real patient CT scans and create textures based on analysis of real endoscopic videos and assign the generated textures to the 3D models by using UV mapping technique.
**b)** The flow of creating the mucosa textures from real endoscopy images.The flow of creating the mucosa textures from real endoscopy images. We first remove artifacts and reflections on the endoscopy image and then select a uniform region in terms of color and expand it to create the main mucosa texture.
**c)** The pipeline for adding veins to the mucosa texture. We extract veins from real endoscopy images using MATLAB and assign them on a texture image using a Gaussian distribution that forms relevant vein network.

<p align="center">
<img src='img/Fig2.1.png' width=512/> 
</p>

#### Our VR-Caps simulation environment

A physician is performing magnetically actuated active capsule endoscopy on the patient. The Franka Emika 7 DOF robotic arm is placed next to the patient holding a permanent magnet to control capsule endoscope swallowed by the patient. On the right side of the figure, our realistic 3D colon, small intestine and stomach models are shown. Models are generated based on real patient's CT (computer tomography) and texture is given based on real endoscopic images. Frames taken via capsule camera are given in RGB format and estimated depth maps are given correspondingly. In the last two rows, from a patient who has polyps in his colon, the result of segmentation algorithm is shown.

<p align="center">
<img src='img/Fig1.png' width=512/> 
</p>

#### Disease Classification

We mimic the 3 diseases (i.e., Polyps, Haemorrhage and Ulcerative Collitis) in our simulation environment. Hemorrage and Ulcerative Collitis are created based on the real endoscopy images from Kvasir dataset mimicking the abnormal mucosa texture. As polyps are not only distintive in texture but also in topology, we use CT scans from patients who have polyps and use this 3D morphological information to reconstruct 3D organs inside our environment.  instances with different severities ranging from grade 1 to grade 4, three different grades of ulcerative colitis, and different polyps instances with various shapes and sizes.

<p align="center">
<img src='img/DISEASES.png' width=512/> 
</p>

## Getting Started

### 1. Installation

### 2. Prerequisites

### 3. Code Base Structure

## Results

Summary of all tasks done on this work is as follows: For details, please visit the article.

<p align="center">
<img src='img/main_figure.png' width=512/> 
</p>

## Reproducibility

## Acknowledgments

## Reference

If you find our work useful in your research please consider citing our paper:

```
@article{...,
    title={Virtual Active Capsule Endoscopy},
    author={Kagan Incetan and Abdulhamid Obeid and Omer Celik and Helder Araujo and Hunter B. Gilbert and Nicholas J. Durr and Faisal Mahmood and Mehmet Turan},
    journal={...},
    year={2020}
}
```
