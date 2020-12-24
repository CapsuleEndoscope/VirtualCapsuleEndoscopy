
import numpy as np
import matplotlib.pyplot as plt
from mpl_toolkits import mplot3d
import math

def rotate_origin_only(xy, radians):
    """Only rotate a point around the origin (0, 0)."""
    x, y = xy
    xx = x * math.cos(radians) - y * math.sin(radians)
    yy = x * math.sin(radians) + y * math.cos(radians)

    return xx, yy


gt = open("09_gt.txt",'r')

zline = []
xline = []
yline = []

gt_4x4 = []

for idx, line in enumerate(gt):
    splitted = line.split(" ")
    if idx == 0:
        gt_x_first = float(splitted[3])
        gt_y_first = float(splitted[7])
        gt_z_first = float(splitted[11])

    gt_x = float(splitted[3]) - gt_x_first
    gt_y = float(splitted[7]) - gt_y_first
    gt_z = float(splitted[11]) - gt_z_first
    # print(gt_x)
    xline.append(gt_x*10)
    yline.append(gt_y*10)
    zline.append(gt_z*10)

    gt_4x4.append( [(splitted[0], splitted[1], splitted[2], gt_x*100),
                    (splitted[4], splitted[5], splitted[6], gt_y*100),
                    (splitted[8], splitted[9], splitted[10],gt_z*100),
                    (          0,           0,            0,            1)   ] )


pred = open("09_pred.txt",'r')

pos_pred_cum_x = []
pos_pred_cum_y = []
pos_pred_cum_z = []

zline_est = []
xline_est = []
yline_est = []

pred_4x4 = []

for idx, line in enumerate(pred):
    splitted = line.split(" ")
    if idx == 0:
        pred_x_first = float(splitted[3])
        pred_y_first = float(splitted[7])
        pred_z_first = float(splitted[11])

    xx, zz = rotate_origin_only( (float(splitted[3]),float(splitted[11])), 0*math.pi/180 ) 
    xx, yy = rotate_origin_only( (float(splitted[3]),float(splitted[7])), 0*math.pi/180 ) 
    yy, zz = rotate_origin_only( (float(splitted[7]),float(splitted[11])), 0*math.pi/180 )
    yline_est.append((xx-pred_x_first)*10)
    xline_est.append((yy-pred_y_first)*10)
    zline_est.append((zz-pred_z_first)*10)



    pred_4x4.append( [(splitted[0], splitted[1], splitted[2], (xx-pred_x_first)*10),
                      (splitted[4], splitted[5], splitted[6], (yy-pred_y_first)*10),
                      (splitted[8], splitted[9], splitted[10],(zz-pred_z_first)*10),
                      (          0,           0,            0,            1)   ] )



fig = plt.figure()
ax = plt.axes(projection='3d')


ax.plot3D(xline, yline, zline, 'blue', label='Ground Truth')

# ax.view_init(azim=135, elev=30)
ax.view_init(azim=45, elev=60)
ax.plot3D(xline_est, yline_est, zline_est, 'red', label='Predicted')


ax.set_xlabel('x [cm]', fontsize=16)#, rotation=150)
ax.set_ylabel('y [cm]', fontsize=16)
ax.set_zlabel('z [cm]', fontsize=16)
ax.tick_params(labelsize=12)
ax.legend(fontsize=18)


plt.savefig('posgraph_txt_colon_1.eps', format='eps')
plt.show()

error_ate = np.zeros(len(xline))
errors = []
gt_xyz = np.array([xline,yline,zline])
pred_xyz = np.array([xline_est,yline_est,zline_est])
# print(gt_xyz)
for i in range(len(pred_xyz[0,:])):
    # print(i)
    align_err = gt_xyz[:,i] - pred_xyz[:,i]
    error_ate[i] = (np.sqrt(np.sum(align_err ** 2)))
    # error_ate[i] = error_ate[i]**2
    errors.append(np.sqrt(np.sum(align_err ** 2)))
ate_mean1 = np.sqrt(np.mean(np.asarray(errors) ** 2))
ate_mean2 = error_ate.mean()


gt_4x4 = np.array(gt_4x4)
gt_4x4 = gt_4x4.astype(np.float)
pred_4x4 = np.array(pred_4x4)
pred_4x4 = pred_4x4.astype(np.float)
# print(pred_4x4)
totalDisp = 0
for i in range(len(gt_xyz[0,:])):
    if i == 0:
        continue
    # print(i)
    displacement = gt_xyz[:,i] - gt_xyz[:,i-1]

    totalDisp += np.sqrt(np.sum(displacement ** 2))
print("Trajectory Length [m]: ", totalDisp/100)

dataLen = len(gt_xyz[0,:])
# print(dataLen)

def compute_ATE(gt, pred):
    """Compute RMSE of ATE
    Args:
        gt (4x4 array dict): ground-truth poses
        pred (4x4 array dict): predicted poses
    """
    errors = []
    # idx_0 = list(pred.keys())[0]
    idx_0  = 0
    gt_0 = gt[idx_0]
    pred_0 = pred[idx_0]

    for i in range(dataLen):
        # cur_gt = np.linalg.inv(gt_0) @ gt[i]
        cur_gt = gt[i]
        gt_xyz = cur_gt[:3, 3]
        # cur_pred = np.linalg.inv(pred_0) @ pred[i]
        cur_pred = pred[i]
        pred_xyz = cur_pred[:3, 3]

        align_err = gt_xyz - pred_xyz
        # print('i: ', i)
        # print("gt: ", gt_xyz)
        # print("pred: ", pred_xyz)
        # input("debug")
        errors.append(np.sqrt(np.sum(align_err ** 2)))
    ate_mean = np.sqrt(np.mean(np.asarray(errors) ** 2))
    ate_std = np.sqrt(np.std(np.asarray(errors) ** 2))
    return ate_mean, ate_std

def compute_RPE(gt, pred):
    """Compute RPE
    Args:
        gt (4x4 array dict): ground-truth poses
        pred (4x4 array dict): predicted poses
    Returns:
        rpe_trans
        rpe_rot
    """
    trans_errors = []
    rot_errors = []
    # for i in list(pred.keys())[:-1]:
    for i in range(len(gt_xyz[0,:])-1):
        gt1 = gt[i]
        gt2 = gt[i+1]
        gt_rel = np.linalg.inv(gt1) @ gt2

        pred1 = pred[i]
        pred2 = pred[i+1]
        pred_rel = np.linalg.inv(pred1) @ pred2
        rel_err = np.linalg.inv(gt_rel) @ pred_rel

        trans_errors.append(translation_error(rel_err))
        rot_errors.append(rotation_error(rel_err))
    # rpe_trans = np.sqrt(np.mean(np.asarray(trans_errors) ** 2))
    # rpe_rot = np.sqrt(np.mean(np.asarray(rot_errors) ** 2))
    rpe_trans_mean = np.mean(np.asarray(trans_errors))
    rpe_trans_std = np.std(np.asarray(trans_errors))
    rpe_rot_mean = np.mean(np.asarray(rot_errors))
    rpe_rot_std = np.std(np.asarray(rot_errors))
    return rpe_trans_mean, rpe_trans_std, rpe_rot_mean, rpe_rot_std

def translation_error(pose_error):
    """Compute translation error
    Args:
        pose_error (4x4 array): relative pose error
    Returns:
        trans_error (float): translation error
    """
    dx = pose_error[0, 3]
    dy = pose_error[1, 3]
    dz = pose_error[2, 3]
    trans_error = np.sqrt(dx**2+dy**2+dz**2)
    return trans_error

def rotation_error(pose_error):
    """Compute rotation error
    Args:
        pose_error (4x4 array): relative pose error
    Returns:
        rot_error (float): rotation error
    """
    a = pose_error[0, 0]
    b = pose_error[1, 1]
    c = pose_error[2, 2]
    d = 0.5*(a+b+c-1.0)
    rot_error = np.arccos(max(min(d, 1.0), -1.0))
    return rot_error

print("ATE mean [m]: ", compute_ATE(gt_4x4, pred_4x4)[0]/100)
print("ATE std [m]: ", compute_ATE(gt_4x4, pred_4x4)[1]/100)

print("RPE trans mean [m]: ", compute_RPE(gt_4x4,pred_4x4)[0]/100)
print("RPE trans std [m]: ", compute_RPE(gt_4x4,pred_4x4)[1]/100)
print("RPE rot mean [deg]: ", compute_RPE(gt_4x4,pred_4x4)[2] * 180 / np.pi)
print("RPE rot std [deg]: ", compute_RPE(gt_4x4,pred_4x4)[3] * 180 / np.pi)