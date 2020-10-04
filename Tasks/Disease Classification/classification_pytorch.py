#  # Developer: Vajira Thambawita
#  # Last modified date: 18/07/2018
#  # ##################################

#  # Description ##################
#  # pythroch resnet18 training






###########################################

from __future__ import print_function, division

import datetime

# #start = datetime.datetime.now()
import argparse
import torch
import torch.nn as nn
import torch.nn.functional as F
import torch.optim as optim
from torch.optim import lr_scheduler
from torchvision import datasets, models, transforms, utils
import pickle
from pandas_ml import ConfusionMatrix
import matplotlib as mpl
import matplotlib.pyplot as plt
import time
import os
import copy
import sys
import yaml
import pandas as pd
import numpy as np

import sklearn.metrics as mtc
from sklearn.metrics import confusion_matrix
from sklearn.metrics import classification_report
import sklearn.preprocessing
import itertools
from multiprocessing import Process, freeze_support
from torch.utils.tensorboard import SummaryWriter

from tqdm import tqdm
from torchsummary import summary
from torch.autograd import Variable

from dataset.Dataloader_with_path import ImageFolderWithPaths as dataset


import scikitplot as skplt
import matplotlib.pyplot as plt
#======================================
# Get and set all input parameters
#======================================
parser = argparse.ArgumentParser()

# Hardware
parser.add_argument("--device", default="gpu", help="Device to run the code")
parser.add_argument("--device_id", type=int, default=0, help="")


parser.add_argument("--py_file",default=os.path.abspath(__file__)) # store current python file


# Directories
parser.add_argument("--data_root", 
                default="/work/vajira/DATA/hyper_kvasir/data_new/splits",
                help="Video data root with three subfolders (fold 1,2 and 3)")

parser.add_argument("--out_dir", 
                default="/work/vajira/DATA/hyper_kvasir/output",
                help="Main output dierectory")

parser.add_argument("--tensorboard_dir", 
                default="/work/vajira/DATA/hyper_kvasir/tensorboard",
                help="Folder to save output of tensorboard")

# Hyper parameters
parser.add_argument("--bs", type=int, default=32, help="Mini batch size")
parser.add_argument("--lr", type=float, default=0.001, help="Learning rate for training")
parser.add_argument("--num_workers", type=int, default=32, help="Number of workers in dataloader")
parser.add_argument("--weight_decay", type=float, default=1e-5, help="weight decay of the optimizer")
parser.add_argument("--momentum", type=float, default=0.9, help="Momentum of SGD function")
parser.add_argument("--lr_sch_factor", type=float, default=0.1, help="Factor to reduce lr in the scheduler")
parser.add_argument("--lr_sch_patience", type=int, default=10, help="Num of epochs to be patience for updating lr")


# Action handling 
parser.add_argument("--num_epochs", type=int, default=0, help="Numbe of epochs to train")
# parser.add_argument("--start_epoch", type=int, default=0, help="Start epoch in retraining")
parser.add_argument("--action", type=str, help="Select an action to run", choices=["train", "retrain", "test", "check", "prepare"])
parser.add_argument("--checkpoint_interval", type=int, default=25, help="Interval to save checkpoint models")
parser.add_argument("--val_fold", type=str, default="split_0", help="Select the validation fold")
parser.add_argument("--all_folds", default=["split_0", "split_1"], help="list of all folds available in data folder")
opt = parser.parse_args()

#==========================================
# Device handling
#==========================================
torch.cuda.set_device(opt.device_id)
device = torch.device("cuda" if torch.cuda.is_available() else "cpu")

#===========================================
# Folder handling
#===========================================

#make output folder if not exist
os.makedirs(opt.out_dir, exist_ok=True)


# make subfolder in the output folder 
py_file_name = opt.py_file.split("/")[-1] # Get python file name (soruce code name)
checkpoint_dir = os.path.join(opt.out_dir, py_file_name + "/checkpoints")
os.makedirs(checkpoint_dir, exist_ok=True)

# make tensorboard subdirectory for the experiment
tensorboard_exp_dir = os.path.join(opt.tensorboard_dir, py_file_name)
os.makedirs( tensorboard_exp_dir, exist_ok=True)



#==========================================
# Tensorboard
#==========================================
# Initialize summary writer
writer = SummaryWriter(tensorboard_exp_dir)


###########################################################
###########################################################
###########################################




#==========================================
# Prepare Data
#==========================================
def prepare_data():

    data_transforms = {
        'train': transforms.Compose([
            transforms.Resize(256),
            transforms.CenterCrop(256),
            transforms.Resize(224),
            transforms.RandomHorizontalFlip(),
            transforms.RandomVerticalFlip(),
            transforms.RandomRotation(90),
            transforms.ToTensor(),
            transforms.Normalize([0.5, 0.5, 0.5], [0.5, 0.5, 0.5])
        ]),
        'validation': transforms.Compose([
            transforms.Resize(256),
            transforms.CenterCrop(256),
            transforms.Resize(224),
            transforms.ToTensor(),
            transforms.Normalize([0.5, 0.5, 0.5], [0.5, 0.5, 0.5])
        ]),
    }


    # Use selected fold for validation
#    train_folds = list(set(opt.all_folds) - set([opt.val_fold]))
    train_folds = opt.all_folds
    validation_fold = opt.val_fold
    

    # Train datasets
#    image_datasets_train_all = {x: dataset(os.path.join(opt.data_root, x),
#                                                data_transforms["train"])
#                        for x in train_folds}

    # create one dataset from all datasets of training
#    dataset_train = torch.utils.data.ConcatDataset([image_datasets_train_all[i] for i in train_folds])

    dataset_train = dataset(os.path.join(opt.data_root, train_folds),
                                         data_transforms["train"])
    # Validation datasets
    dataset_val = dataset(os.path.join(opt.data_root, validation_fold),
                                                data_transforms["validation"])
                                                


    dataloader_train = torch.utils.data.DataLoader(dataset_train, batch_size=opt.bs,
                                                    shuffle=True, num_workers=opt.num_workers)


    dataloader_val = torch.utils.data.DataLoader(dataset_val, batch_size=opt.bs,
                                                    shuffle=False, num_workers=opt.num_workers)
                    
    train_size = len(dataset_train)
    val_size = len(dataset_val)

    print("train dataset size =", train_size)
    print("validation dataset size=", val_size)

   
    return {"train":dataloader_train, "val":dataloader_val, "dataset_size":{"train": train_size, "val":val_size} }





#########################################################################
#  Printing images just for testing
#########################################################################
'''
def imshow(img):
    img = img / 2 + 0.5     # unnormalize
    npimg = img.numpy()
    plt.imshow(np.transpose(npimg, (1, 2, 0)))
dataiter = iter(dataloaders['train'])
sample_images, sample_labels = dataiter.next()
npimg = sample_images[0].numpy()
npimg = np.transpose(npimg,(1,2,0))
plt.imshow(npimg[:,:, 0])
plt.show()
print(npimg[:, :, 0])
#imshow(utils.make_grid(sample_images))
input()
exit()
'''

#==========================================================
# Train model
#===========================================================

def train_model(model, optimizer, criterion, dataloaders: dict, scheduler, best_acc=0.0, start_epoch = 0):

    best_model_wts = copy.deepcopy(model.state_dict())


    for epoch in tqdm(range(start_epoch , start_epoch + opt.num_epochs )):

        for phase in ["train", "val"]:

            if phase == "train":
                model.train()
                dataloader = dataloaders["train"]
            else:
                model.eval()
                dataloader = dataloaders["val"]
            
            
            running_loss = 0.0
            running_corrects = 0


            for i, data in tqdm(enumerate(dataloader, 0)):

                inputs, labels, paths = data
                inputs = inputs.to(device)
                labels = labels.to(device)

                # zero the parameter gradients
                optimizer.zero_grad()

                # forward
                # track history if only in train
                with torch.set_grad_enabled(phase == 'train'):
                    outputs = model(inputs)
                    _, preds = torch.max(outputs, 1)
                    loss = criterion(outputs, labels)
                    #  print("outputs=", outputs) # only for testing - vajira
                    #  print("labels = ", labels) # only for testing - vajira

                    # backward + optimize only if in training phase
                    if phase == 'train':
                        loss.backward()
                        optimizer.step()

                # statistics
                running_loss += loss.item() * inputs.size(0)
                running_corrects += torch.sum(preds == labels.data)

            epoch_loss = running_loss / dataloaders["dataset_size"][phase]
            epoch_acc = running_corrects.double() / dataloaders["dataset_size"][phase]

            # update tensorboard writer
            writer.add_scalars("Loss", {phase:epoch_loss}, epoch)
            writer.add_scalars("Accuracy" , {phase:epoch_acc}, epoch)

            print(phase)
             # update the lr based on the epoch loss
            if phase == "val": 
                print("epoch_acc= " , epoch_acc)
                print("best_acc= " , best_acc)
                # keep best model weights
                if epoch_acc > best_acc:
                    best_acc = epoch_acc
                    best_model_wts = copy.deepcopy(model.state_dict())
                    best_epoch =epoch
                    best_epoch_loss = epoch_loss
                    best_epoch_acc = epoch_acc
                    print("Found a better model")

                # Get current lr
                lr = optimizer.param_groups[0]['lr']
                #print("lr=", lr)
                writer.add_scalar("LR", lr, epoch)
                scheduler.step(epoch_loss) 
            


            # Print output
            print('Epoch:\t  %d |Phase: \t %s | Loss:\t\t %.4f | Acc:\t %.4f '
                      % (epoch, phase, epoch_loss, epoch_acc))
    
    save_model(best_model_wts, best_epoch, best_epoch_loss, best_epoch_acc)

            
#===============================================
# Prepare models
#===============================================

def prepare_model():
    model = models.resnet152(pretrained=False)
    num_ftrs = model.fc.in_features
    model.fc = nn.Linear(num_ftrs, 3)
    model = model.to(device)
    
    return model



#====================================
# Run training process
#====================================
def run_train(retrain=False):
    model = prepare_model()
    
    dataloaders = prepare_data()

    # optimizer = optim.Adam(model.parameters(), lr=opt.lr , weight_decay=opt.weight_decay)
    optimizer = optim.SGD(model.parameters(), lr=opt.lr, momentum=0.9)
    # optimizer = optim.SGD(model.parameters(), lr=opt.lr )

    # criterion =  nn.MSELoss() # backprop loss calculation
    criterion = nn.CrossEntropyLoss() # weight=weights
    # criterion_validation = nn.L1Loss() # Absolute error for real loss calculations

    # LR shceduler
    scheduler = lr_scheduler.ReduceLROnPlateau(optimizer, mode="min", factor=opt.lr_sch_factor, patience=opt.lr_sch_patience, verbose=True)

    # call main train loop

    if retrain:
        # train from a checkpoint
        checkpoint_path = input("Please enter the checkpoint path:")
        checkpoint = torch.load(checkpoint_path)
        model.load_state_dict(checkpoint["model_state_dict"])
        start_epoch = checkpoint["epoch"]
        loss = checkpoint["loss"]
        acc = checkpoint["acc"]
        #train_model(model,optimizer,criterion, dataloaders, scheduler, best_acc=acc, start_epoch=start_epoch)
        train_model(model,optimizer,criterion, dataloaders, scheduler, best_acc=0, start_epoch=0)

    else:
        train_model(model,optimizer,criterion, dataloaders, scheduler, best_acc=0.0, start_epoch=0)


#=====================================
# Save models
#=====================================
def save_model(model_weights,  best_epoch,  best_epoch_loss, best_epoch_acc):
   
    check_point_name = py_file_name + "_epoch:{}.pt".format(best_epoch) # get code file name and make a name
    check_point_path = os.path.join(checkpoint_dir, check_point_name)
    # save torch model
    torch.save({
        "epoch": best_epoch,
        "model_state_dict": model_weights,
        # "optimizer_state_dict": optimizer.state_dict(),
        # "train_loss": train_loss,
        "loss": best_epoch_loss,
        "acc": best_epoch_acc,
    }, check_point_path)





#=====================================
# Check model
#=====================================
def check_model_graph():
    model = prepare_model()

    summary(model, (3, 224, 224)) # this run on GPU
    model = model.to('cpu')
    #dataloaders = prepare_data()
    #sample = next(iter(dataloaders["train"]))

    #inputs = sample["features"]
   # inputs = inputs.to(device, torch.float)
    #print(inputs.shape)
    print(model)
    dummy_input = Variable(torch.rand(13, 3, 224, 224))
    
    writer.add_graph(model, dummy_input) # this need the model on CPU

#===============================================
#  Model testing method
#===============================================

def test_model():
    
    test_model_checkpoint = input("Please enter the path of test model:")
    checkpoint = torch.load(test_model_checkpoint)

    model = prepare_model()
    model.load_state_dict(checkpoint["model_state_dict"])
    model.eval()

    dataloaders = prepare_data()
    test_dataloader = dataloaders["val"]

    # TO collect data
    correct = 0
    total = 0
    all_labels_d = torch.tensor([], dtype=torch.long).to(device)
    all_predictions_d = torch.tensor([], dtype=torch.long).to(device)
    all_predictions_probabilities_d = torch.tensor([], dtype=torch.float).to(device)



    with torch.no_grad():
        for i, data in tqdm(enumerate(test_dataloader, 0)):

            inputs, labels, paths = data
            # print(labels)
            inputs = inputs.to(device)
            labels = labels.to(device)


            outputs = model(inputs)
            outputs = F.softmax(outputs, 1)
            predicted_probability, predicted = torch.max(outputs.data, 1)
            print("predicted_probability.shape= " ,predicted_probability.shape)
            print("outputs.data.shape= ", outputs.data.shape)
            print("predicted.shape= ",predicted.shape)
            print("predicted = ", predicted)
            total += labels.size(0)
            correct += (predicted == labels).sum()
            all_labels_d = torch.cat((all_labels_d, labels), 0)
            all_predictions_d = torch.cat((all_predictions_d, predicted), 0)
            all_predictions_probabilities_d = torch.cat((all_predictions_probabilities_d, predicted_probability), 0)
           # all_predictions_probabilities_d = torch.cat((all_predictions_probabilities_d, outputs.data), 0) 
           #all_timePerFrame_host = all_timePerFrame_host + [time_per_image]
            # print("testing")

    print('copying some data back to cpu for generating confusion matrix...')
    y_true = all_labels_d.cpu()
    y_predicted = all_predictions_d.cpu()  # to('cpu')
    testset_predicted_probabilites = all_predictions_probabilities_d.cpu()  # to('cpu')

    #y_true = # ground truth labels
    #y_probas = # predicted probabilities generated by sklearn classifier
    print("testset_predicted_probabilites= ", testset_predicted_probabilites.shape)
    print("testset_predicted_probabilites= ",testset_predicted_probabilites)
    print("y_true= ", y_true.shape)
#    y_true.reshape(len(y_true),1)
#    testset_predicted_probabilites.reshape(len(testset_predicted_probabilites),1)

    #return y_predicted, testset_predicted_probabilites, all_timePerFrame_host


    cm = confusion_matrix(y_true, y_predicted)  # confusion matrix



    print('Accuracy of the network on the %d test images: %f %%' % (total, (
            100.0 * correct / total)))

    print(cm)

    print("taking class names to plot CM")

    class_names = test_dataloader.dataset.classes #test_datasets.classes  # taking class names for plotting confusion matrix

    print("Generating confution matrix")

    plot_confusion_matrix(cm, classes=class_names, title='my confusion matrix')

    

    ##################################################################
    # classification report
    #################################################################
    print(classification_report(y_true, y_predicted, target_names=class_names))

    ##################################################################
    # Standard metrics for medico Task
    #################################################################
    print("Printing standard metric for medico task")

    print("Accuracy =",mtc.accuracy_score(y_true, y_predicted))
    print("Precision score =", mtc.precision_score(y_true,y_predicted, average="weighted"))
    print("Recall score =", mtc.recall_score(y_true, y_predicted, average="weighted"))
    print("F1 score =", mtc.f1_score(y_true, y_predicted, average="weighted"))
    print("Specificity =")
    print("MCC =", mtc.matthews_corrcoef(y_true, y_predicted))

    ##################################################################
    # Standard metrics for medico Task
    #################################################################
    print("Printing standard metric for medico task")


    print("1. Recall score (REC) =", mtc.recall_score(y_true, y_predicted, average="weighted"))
    print("2. Precision score (PREC) =",
            mtc.precision_score(y_true, y_predicted, average="weighted"))
    print("3. Specificity (SPEC) =")
    # print("4. Accuracy (ACC) =", mtc.accuracy_score(y_true, y_predicted, weights))
    print("5. Matthews correlation coefficient(MCC) =", mtc.matthews_corrcoef(y_true, y_predicted))

    print("6. F1 score (F1) =", mtc.f1_score(y_true, y_predicted, average="weighted"))

    
    print('Finished.. ')

    #====================================================================
    # Writing to a file
    #=====================================================================
    
    np.set_printoptions(linewidth=np.inf)
    with open("%s/%s_evaluation.csv" % (opt.out_dir, py_file_name), "w") as f:

        f.write(np.array2string(mtc.confusion_matrix(y_true, y_predicted), separator=", "))

        f.write("\n\n\n\n")
        f.write("--- Macro Averaged Resutls ---\n")
        f.write("Precision: %s\n" % mtc.precision_score(y_true, y_predicted, average="macro"))
        f.write("Recall: %s\n" % mtc.recall_score(y_true, y_predicted, average="macro"))
        f.write("F1-Score: %s\n\n" % mtc.f1_score(y_true, y_predicted, average="macro"))


        f.write("--- Micro Averaged Resutls ---\n")
        f.write("Precision: %s\n" % mtc.precision_score(y_true, y_predicted, average="micro"))
        f.write("Recall: %s\n" % mtc.recall_score(y_true, y_predicted, average="micro"))
        f.write("F1-Score: %s\n\n" % mtc.f1_score(y_true, y_predicted, average="micro"))

        f.write("--- Other Resutls ---\n")
        f.write("MCC: %s\n" % mtc.matthews_corrcoef(y_true, y_predicted))

    f.close()
    print("Report generated")

    #==========================================================================




#==============================================
# Prepare submission file with probabilities
#===============================================
def prepare_prediction_file():

    if opt.bs != 1:
        print("Please run with bs = 1")
        exit()


    test_model_checkpoint = input("Please enter the path of test model:")
    checkpoint = torch.load(test_model_checkpoint)

    model = prepare_model()
    model.load_state_dict(checkpoint["model_state_dict"])
    model.eval()

    dataloaders = prepare_data()
    test_dataloader = dataloaders["val"]

    class_names = test_dataloader.dataset.classes

    df = pd.DataFrame(columns=["filename", "predicted-label", "actual-label"] + class_names)

    print(df.head())
   #  exit()

    with torch.no_grad():
        for i, data in tqdm(enumerate(test_dataloader, 0)):
            
            inputs, labels, paths = data
                

            df_temp = pd.DataFrame(columns=["filename", "predicted-label", "actual-label"] + class_names)


            #print("paths:", paths)
            filename = [list(paths)[0].split("/")[-1]]
            #print("filenames:", filename)
            
            df_temp["filename"] = filename

           

            inputs = inputs.to(device)
            labels = labels.to(device)


            outputs = model(inputs)
            outputs = F.softmax(outputs, 1)
            predicted_probability, predicted = torch.max(outputs.data, 1)
            
            df_temp["predicted-label"] = class_names[predicted.item()]
            df_temp["actual-label"] = class_names[labels.item()]
            

            # print("actual label:", labels.item())
            #print("predicted label:", predicted.item())
            # print("probabilities :", outputs.cpu())

            probabilities = outputs.cpu().squeeze()
            probabilities = probabilities.tolist()
            probabilities = np.around(probabilities, decimals=3)
            #print(probabilities)

            df_temp[class_names] = probabilities

            #record = record + [class_names[labels.item()]] + [class_names[predicted.item()]] 

            #print(record)
            #print(df_temp.head())
            df = df.append(df_temp)
           # break

        print(df.head())
        print("length of DF:", len(df))
        prob_file_name = "%s/%s_probabilities.csv" % (opt.out_dir, py_file_name)
        df.to_csv(prob_file_name, index=False)


##########################################################
# Prepare submission file:
##########################################################

def prepare_submission_file(image_names, predicted_labels, max_probability, time_per_image, submit_dir, data_classes):

    predicted_label_names = []

    for i in predicted_labels:
        predicted_label_names = predicted_label_names + [data_classes[i]]

    #  print(predicted_label_names)

    submission_dataframe = pd.DataFrame(np.column_stack([image_names,
                                                            predicted_label_names,
                                                            max_probability,
                                                            time_per_image]),
                                    columns=['images', 'labels', 'PROB', 'time'])
    #print("image names:{0}".format(image_names))

    submission_dataframe.to_csv(os.path.join(submit_dir, "method_3_test_output"), index=False)

    print(submission_dataframe)
    print("successfully created submission file")
###########################################################

###########################################################
#  Ploting history and save plots to plots directory
###########################################################



############################################################
# Plot confusion matrix - method
############################################################
def plot_confusion_matrix(cm, classes,
                            normalize=False,
                            title='Confusion matrix',
                            cmap=plt.cm.Blues,
                            plt_size=[20,18]):
    """
    This function prints and plots the confusion matrix.
    Normalization can be applied by setting `normalize=True`.
    """
    plt.rcParams['figure.figsize'] = plt_size
    if normalize:
        cm = cm.astype('float') / cm.sum(axis=1)[:, np.newaxis]
        print("Normalized confusion matrix")
    else:
        print('Confusion matrix, without normalization')

    print(cm)

    plt.imshow(cm, interpolation='nearest', cmap=cmap)
   # plt.title(title,fontsize=40)
    cb = plt.colorbar()
    cb.ax.tick_params(labelsize=36)
    tick_marks = np.arange(len(classes))
    plt.xticks(tick_marks, classes, rotation=90,fontsize=28,weight='bold')
    plt.yticks(tick_marks, classes,fontsize=28,weight='bold')

    fmt = '.2f' if normalize else 'd'
    thresh = cm.max() / 2.
    for i, j in itertools.product(range(cm.shape[0]), range(cm.shape[1])):
        plt.text(j, i, format(cm[i, j], fmt),fontsize=30,weight='bold',
                    horizontalalignment="center",
                    color="white" if cm[i, j] > thresh else "black")

    #plt.tight_layout()
    plt.ylabel("True label", fontsize=40, weight='bold')
    plt.xlabel('Predicted label', fontsize=40, weight='bold')
    plt.savefig("saved_plot_confusion.eps",format='eps')
    plt.savefig("saved_plot_confusion.png")
    figure = plt.gcf()
    writer.add_figure("Confusion Matrix", figure)
    print("Finished confusion matrix drawing...")




if __name__ == '__main__':
    print("Started data preparation")
    data_loaders = prepare_data()
    print(vars(opt))
    print("Data is ready")

    # Train or retrain or inference
    if opt.action == "train":
        print("Training process is strted..!")
        run_train()
       # pass
    elif opt.action == "retrain":
        print("Retrainning process is strted..!")
        run_train(retrain=True)
       # pass
    elif opt.action == "test":
        print("Inference process is strted..!")
        test_model()
    elif opt.action == "check":
        check_model_graph()
        print("Check pass")
    elif opt.action == "prepare":
        prepare_prediction_file()
        print("Probability file prepared..!")

    # Finish tensorboard writer
    writer.close()
