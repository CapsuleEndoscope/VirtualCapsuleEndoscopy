# Disease Classification

To show the effectiveness of using synthetic data generated on VR-Caps, we have trained ResNet-152 architecture in 2 different ways:
In the first case we use real data from Kvasir dataset for training and test the model on the real data (different set from Kvasir)
In the second scenario, we first pre-train the same model with the synthetic data and then fine-tune the model with the real data and test in on the test set from Kvasir.

To reproduce our results presented in the paper, we provide the used datasets on the Google drive on this [link](https://drive.google.com/drive/folders/1PJvGr9i3G5oe1t_Qw6mwq2YX3QPmk5-T?usp=sharing)

## Training

### Without Pre-Training

```sh
python classification_pytorch.py --data_root kvasir_data --out_dir nofinetune --tensorboard_dir tensorboard --all_folds training --val_fold validation --action train --num_epochs 10
```
### With Pre-Training
 - Pre-training with the synthetic data:
```sh
python classification_pytorch.py --data_root unity_data --out_dir unity-pretrain --tensorboard_dir tensorboard --all_folds training --val_fold validation --action train --num_epochs 10
```
 - Fine-tuning with the real data:
```sh
python classification_pytorch.py --data_root kvasir_data --out_dir results/unity-pretrain-kvasir-finetune --tensorboard_dir tensorboard --all_folds training --val_fold validation --action retrain --num_epochs 10
```
## Testing
Please input the path of the relevant model for testing.

```sh
python classification_pytorch.py --data_root kvasir_data --out_dir test --tensorboard_dir tensorboard --all_folds training --val_fold validation --action test
```
