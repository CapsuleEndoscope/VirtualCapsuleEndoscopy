
### 1. Reproduce Results

First, please make sure that you have the unity version installed higher than 2019.3.2f1. Then install ML-Agents from the [here](https://github.com/Unity-Technologies/ml-agents). We highly encourage to check basic tutorials to get started with the ML-Agents. Please give the correct path of the "com.unity.ml-agents" on 5th line in the [manifest.json](Packages/manifest.json) file. Then, download the project folder and open it on Unity Hub. The scene with a capsule agent, a stomach 3D model and the script(CoverageAgent.cs) will be opened. You can see that the trained model (CoverageBrain.nn) is already attached as a component to the capsule. So, you can just run the play mode for the coverage task. 

<p align="center">
  <img src="../img/capsulecoverage.gif" width=500//>

### 2. Train a new model
In order to train a new model, please follow the steps:

#### 2.1 Training Configuration 
</p>
<img align="right" src="../img/capsuleagent.png" width="200">
