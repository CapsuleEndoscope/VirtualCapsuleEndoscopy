To show the effectiveness of using synthetic data generated on VR-Caps, we have trained ResNet-152 architecture in 2 different ways:
In the first case we use real data from Kvasir dataset for training and test the model on the real data (different set from Kvasir)
In the second scenario, we first pre-train the same model with the synthetic data and then fine-tune the model with the real data and test in on the test set from Kvasir.
