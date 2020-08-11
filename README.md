# VR-Caps
A Virtual Environment for Active Capsule Endoscopy

<p align="center">
<img src='img/Fig1.png' width=512/> 
</p>

## Overview

We introduce a virtual active capsule endoscopy environment developed in Unity that provides a simulation platform to generate synthetic data as well as a test bed to develop and test algorithms. Using that envronment, we perform various evaluations for common robotics and computer vision tasks of active capsule endoscopy such as classification, pose and depth estimation, area coverage, autonomous navigation, learning control of endoscopic capsule robot with magnetic field inside GI-tract organs, super-resolution, etc. The demonstration of our virtual environment is available on [YouTube](https://www.youtube.com/watch?v=UQ2u3CIUciA).

Our main contributions are as follows:
  - We propose synthetic data generating tool for creating fully labeled data.
  - Using our simulation environment, we provide a platform for testing numerous highly realistic scenarios.
  
See [Summary of our work](Summary.md) for details.



## Getting Started

### 1. Installation

The VR-Caps contains several components:
  - Unity
  - MagnetoDynamics
  - SOFA
  
Consequently, to install and use the VR-Caps you will need to:

  - [Download](https://unity3d.com/get-unity/download) and Install Unity(2018.4 or later)
  - [Install](https://www.python.org/downloads/) Python(3.6.1 or higher)
  - [Install](https://github.com/Unity-Technologies/ml-agents) ML-Agents(Release 1 or higher)
  - [Install](http://infinytech3d.com/SofaUnity/sofaUnity.php) SofaAPAPI-Unity3D

#### Clone the VR-Caps Repository

Now that you have installed Unity and Python, you can now clone this repository

```sh
git clone https://github.com/CapsuleEndoscope/VirtualCapsuleEndoscopy.git
```

### 2. Creating a New Environment

After installing all dependiences, you will need open Unity and simply create a new Unity project.
Then, you need to import the following assets into it:

1. GI Organs
2. Franka Emika Robot Arm
3. Materials


## Tasks 

#### 1. Area Coverage

We use Unity's ML-Agents Toolkit for a Deep Reinforcement Learning (DRL) based active control method that has a goal of learning a maximum coverage policy for human colon monitoring within a minimal operation time.

- [Designing a Learning Environment](Learning-Environment-Design.md)
- [Training ML-Agents](Training-ML-Agents.md)

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


## Results

Visual demonstration of all tasks done on this work and their results are as follows: For more details, please visit the article.

<p align="center">
<img src='img/main_figure.png' width=800/> 
</p>

## Frequently Asked Questions

## Limitations

## Reference

If you find our work useful in your research or if you use parts of this code please consider citing our paper:

```
@article{...,
    title={VR-Caps: A Virtual Environment for Active Capsule Endoscopy},
    author={Kagan Incetan and Abdulhamid Obeid and Omer Celik and Guliz Irem Gokceler and Yasin Almalioglu and Hunter B. Gilbert and Nicholas J. Durr and Faisal Mahmood and Kutsev Bengisu Ozyoruk and Mehmet Turan},
    journal={...},
    year={2020}
}
```



<!-- 

## Installation & Set-up

- [Installation](#1-installation)


## Getting Started

- [Creating An Environment](Creating-An-Environment.md)


## Help

- [Frequently Asked Questions](#frequently-asked-questions)
- [Limitations](#limitations)

-->
