# Virtual Capsule Endoscopy

If you use this code, please cite:

>    Kagan Incetan,Abdulhamid Obeid, Omer Celik, Kutsev Bengisu Ozyoruk, Helder Araujo, Hunter B. Gilbert, Nicholas J. Durr, Faisal Mahmood, Mehmet Turan. "Virtual Active Capsule Endoscopy.".

## Overview

We introduce a virtual active capsule endoscopy platform developed in Unity that provides a simulation environment to test new devices and algorithms. Also, we propose a sim2real method which makes use of cycle-consistent image domain style transfer and feature domain adaptation techniques to adapt representations at both the pixel-level and feature-level to solve real medical data tasks. Using that pipeline, we perform various evaluations for common robotics and computer vision tasks of active capsule endoscopy such as abnormality detection, classification, depth estimation, SLAM (Simultaneous Localization and Mapping), autonomous navigation, learning control of endoscopic capsule robot with magnetic field inside GI-tract organs, super-resolution, etc.

Our main contributions are as follows:
  - We propose synthetic data generating tool for creating fully labeled data.
  - Using our simulation environment, we provide a platform for testing numerous highly realistic scenarios.

#### Summary of Our Work

**a)** In order to generate a realistic simulation environment, we have used 3D models reconstructed from real patient CT scans. Then, from real endoscopy images, we have created textures and they were applied to the 3D models by using UV mapping method.
**b)** The flow of creating the mucosa textures from real endoscopy images from the Kvasir dataset. The first step includes the removal of reflections since it  will be an effect introduced in the environment due to light and wet surfaces interaction, and removing any details and features to to make the images appear as flat mucosa images, the next step is to select a region that is neither bright nor dark due to lighting effects and extend that region, then increase the resolution of the image to show the details when the texture is projected on the UV unwrapped mesh of the 3D model.
**c)** The veins are added to the texture images by extracting them from real endoscopy images using a Matlab script, then they are distributed on a texture image by a script that applies random distribution to form a veins network.

<p align="center">
<img src='imgs/Trial.png' width=512/> 
</p>

#### Our proposed simulation environment

A physician is performing magnetically actuated active capsule endoscopy on the patient. The Franka Emika 7 DOF robotic arm is placed next to the patient holding a permanent magnet to control capsule endoscope swallowed by the patient. On the right side of the figure, our realistic 3D colon, small intestine and stomach models are shown. Models are generated based on real patient's CT (computer tomography) and texture is given based on real endoscopic images. Frames taken via capsule camera are given in RGB format and estimated depth maps are given correspondingly. In the last two rows, from a patient who has polyps in his colon, the result of segmentation algorithm is shown.

<p align="center">
<img src='imgs/main_figure.png' width=512/> 
</p>

## Getting Started

### 1. Installation

### 2. Prerequisites

### 3. Code Base Structure

## Results

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
