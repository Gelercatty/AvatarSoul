# api
from flask import Flask , jsonify , request
from flask_socketio import SocketIO,emit
# motionGPT

from mGPT.config import parse_args
from pathlib import Path
import pytorch_lightning as pl
import torch
import time
import numpy as np
import os
from mGPT.data.build_data import build_data
from mGPT.models.build_model import build_model
from scipy.spatial.transform import Rotation as RRR
import imageio
import mGPT.render.matplot.plot_3d_global as plot_3d
import moviepy.editor as mp
import asyncio

from scipy.spatial.transform import Rotation as RRR
from mGPT.render.pyrender.hybrik_loc2rot import HybrIKJointsToRotmat

from api_tools import plot_joint_axes

app = Flask(__name__)
socketio = SocketIO(app)

# set the model to run on the GPU
os.environ['DISPLAY'] = ':0.0'
os.environ['PYOPENGL_PLATFORM'] = 'egl'

cfg = parse_args(phase="webui")
cfg.FOLDER = 'web_api'
# out_dir: web_api/cache
output_dir = Path(cfg.FOLDER)
output_dir.mkdir(parents=True, exist_ok=True)
pl.seed_everything(cfg.SEED_VALUE)

device = torch.device("cuda")

datamodule = build_data(cfg, phase="test")
model = build_model(cfg, datamodule)

state_dict = torch.load(cfg.TEST.CHECKPOINTS, map_location='cpu')["state_dict"]
model.load_state_dict(state_dict)
model.to(device)


def render_motion(data, method='fast'):
    fname = time.strftime("%Y-%m-%d-%H_%M_%S", time.localtime(
        time.time())) + str(np.random.randint(10000, 99999))
    video_fname = fname + '.mp4'
    feats_fname = fname+'_feats' + '.npy'
    output_mp4_path = os.path.join(output_dir, video_fname)
    

    if method == 'fast':
        output_gif_path = output_mp4_path[:-4] + '.gif'
        if len(data.shape) == 3:
            data = data[None]
        if isinstance(data, torch.Tensor):
            data = data.cpu().numpy()
        print(data.shape)

        pose_vis = plot_3d.draw_to_batch(data, [''], [output_gif_path])
        del pose_vis

    return output_mp4_path, video_fname, feats_fname

# # 切换左右手坐标系
# def convert_rotation_matrices(rot_matrices, axis='z'):
#     """
#     Convert rotation matrices from left-hand to right-hand coordinate system or vice versa by mirroring across a specified axis.
    
#     Parameters:
#         rot_matrices (numpy.ndarray): An array of shape [frames, nodes, 3, 3] containing rotation matrices.
#         axis (str): Axis to mirror across ('x', 'y', or 'z'). Default is 'z'.
    
#     Returns:
#         numpy.ndarray: Converted rotation matrices with the same shape as input.
#     """
#     print('changing coordinate system')
#     # Create a reflection matrix for the specified axis
#     if axis == 'x':
#         reflect = np.diag([-1, 1, 1])
#     elif axis == 'y':
#         reflect = np.diag([1, -1, 1])
#     elif axis == 'z':
#         reflect = np.diag([1, 1, -1])
#     else:
#         raise ValueError("Invalid axis, choose 'x', 'y', or 'z'")

#     # Apply reflection matrix to all rotation matrices
#     converted_matrices = np.matmul(reflect, np.matmul(rot_matrices, reflect))
    
#     return converted_matrices


def convert_rotation_matrices(rot_matrices, axis='z', nodes=None):
    """
    Convert rotation matrices from left-hand to right-hand coordinate system or vice versa by mirroring across a specified axis
    for specified nodes or for all nodes if none are specified.
    
    Parameters:
        rot_matrices (numpy.ndarray): An array of shape [frames, nodes, 3, 3] containing rotation matrices.
        axis (str): Axis to mirror across ('x', 'y', or 'z'). Default is 'z'.
        nodes (list of int, optional): List of node indices to mirror. If None, all nodes will be mirrored.
    
    Returns:
        numpy.ndarray: Converted rotation matrices with the same shape as input.
    """
    print('Changing coordinate system')
    # Create a reflection matrix for the specified axis
    if axis == 'x':
        reflect = np.diag([-1, 1, 1])
    elif axis == 'y':
        reflect = np.diag([1, -1, 1])
    elif axis == 'z':
        reflect = np.diag([1, 1, -1])
    else:
        raise ValueError("Invalid axis, choose 'x', 'y', or 'z'")

    # Apply reflection matrix to specified nodes or all nodes
    if nodes is not None:
        # Initialize output with original matrices
        converted_matrices = np.array(rot_matrices, copy=True)
        for node in nodes:
            converted_matrices[:, node] = np.matmul(reflect, np.matmul(rot_matrices[:, node], reflect))
    else:
        # Mirror all nodes
        converted_matrices = np.matmul(reflect, np.matmul(rot_matrices, reflect))
    
    return converted_matrices


def print_rotation_matrix(data,frame,index):
    # data: [frame,22,3,3]
    # index: 0-21
    print(data[frame,index])



def rotate_node(action_data, axis, degrees, node_indices):
    """
    Rotate specified nodes in action data by a given degree around a specified axis using rotation vector.

    Parameters:
    - action_data (numpy.ndarray): The action data array of shape [frame, 22, 3, 3]
    - axis (str): The axis to rotate around ('x', 'y', or 'z')
    - degrees (float): The amount of rotation in degrees
    - node_indices (int or tuple): Indices of nodes to rotate

    Returns:
    - None; the action_data is modified in place
    """
    # Define axis indices
    axis_indices = {'x': 0, 'y': 1, 'z': 2}
    if axis not in axis_indices:
        raise ValueError("Axis must be 'x', 'y', or 'z'")
    
    # Create a rotation vector
    rotvec = np.zeros(3)
    rotvec[axis_indices[axis]] = np.deg2rad(degrees)
    
    # Create a rotation object from the rotation vector
    rotation = RRR.from_rotvec(rotvec)
    # Convert the rotation object to a 3x3 rotation matrix
    rot_matrix = rotation.as_matrix()
    
    # If node_indices is a single integer, make it a tuple to simplify processing
    if isinstance(node_indices, int):
        node_indices = (node_indices,)
    
    # Apply the rotation matrix to the specified nodes
    for frame in range(action_data.shape[0]):
        for node_index in node_indices:
            action_data[frame, node_index] = np.dot(action_data[frame, node_index], rot_matrix)
    

def text2motion(text):
    
    # instruction = ''
    # text = instruction + text
    batch = {
        "length":[0],
        "text":[text],
    }
    outputs = model(batch,task='lot')
    out_lot = outputs['out_lot']

    print(out_lot)
    joints = out_lot['joints'][0].cpu().numpy()
    return joints

def get_motion(text):
    joints = text2motion(text)

    # for test
    # joints = np.load(r'D:\Geler也要发论文\llm\motionGPT\motionGPT\MotionGPT\expereiment_token\2024-02-13-13_34_5068067_joints.npy')
    info_dict= joints2motionInfo(joints)

    socketio.emit('re_joints',info_dict)
    print('get_motion: emited')


def joints2motionInfo(joints):
    # render_motion(joints)
    data_ = joints - joints[0,0]# 0,0是第一个关节的位置. 为了方便计算,将第一个关节的位置设为(0,0,0)
    # joints to euler angles
    pose_genertaor = HybrIKJointsToRotmat()
    # 生成pose
    pose = pose_genertaor(data_)
    
    # 添加节点
    pose_add = np.concatenate([
        pose,
        np.stack([np.stack([np.eye(3)] * pose.shape[0], 0)] * 2, 1)
    ], 1)
    # 切换左右手坐标系
    print_rotation_matrix(pose_add,0,0)
    pose_c = convert_rotation_matrices(pose_add, 'x')
    pose_c = convert_rotation_matrices(pose_c, 'z',[7,8,10,11,16,17,18,19,20,21,12,15])
    pose_c = convert_rotation_matrices(pose_c, 'y',[14,13])

    print_rotation_matrix(pose_c,0,0)
    r = RRR.from_rotvec(np.array([np.pi/2, 0.0, 0.0])) 
    pose_c[:, 0] = np.matmul(r.as_matrix().reshape(1, 3, 3), pose_c[:, 0])
    print_rotation_matrix(pose_c,0,0)
    # 下肢
    pose_c = convert_rotation_matrices(pose_c, 'x',[1,2,4,5,7,8,10,11,16,17,])
    rotate_node(pose_c,'x',180,(1,2))

    # 上肢
    rotate_node(pose_c,'y',-90,(14))
    rotate_node(pose_c,'x',-65,(14))
    rotate_node(pose_c,'z',30,(14))

    rotate_node(pose_c,'x',-90,(17))

    # rotate_node(pose_c,'y',-90,(19))

    rotate_node(pose_c,'y',90,(13))
    rotate_node(pose_c,'x',-65,(13))
    rotate_node(pose_c,'z',-30,(13))

    rotate_node(pose_c,'x',-90,(16))

    # pose_c = convert_rotation_matrices(pose_c, 'y',[19,18])
    pose_c = convert_rotation_matrices(pose_c, 'x',[19,18])
    rotate_node(pose_c,'y',90,(19))
    rotate_node(pose_c,'y',-90,(18))
    pose_c = convert_rotation_matrices(pose_c, 'z',[9]) 



    aroot = data_[:,0]
    print(aroot)
    aroot[:, 1] = -aroot[:, 1] # y轴取反
    aroot_diff = np.diff(aroot, axis=0)
    aroot_diff = np.concatenate((np.zeros((1,3)), aroot_diff), axis=0)
    # aroot = data_[:, 0:1, :]
    # aroot = np.array([1,2,3,4])
# 计算每一帧的位置变化量
    # aroot_diff = np.diff(aroot, axis=0)
    print(aroot.shape)
    info_dict = {
        "Pose":pose_c.tolist(),
        "Aroot":aroot_diff.tolist(),
    }
    return info_dict

@app.route('/status',methods=['GET'])
def get_status():
    return jsonify({'status':'ok',
                    'model':cfg.TEST.CHECKPOINTS,
                    })# 动态查询模型状态

@app.route('/posttest',methods=['POST'])
def post_test():
    data = request.get_json()
    print('data: '+str(data))
    return "ok"



@socketio.on('connect')
def handle_connect():
    print('client connected')
    socketio.emit('connected','connected')

@socketio.on('disconnect')
def handle_disconnect():
    print('client disconnected')

@socketio.on('ask_motion')
def handle_message(data):
    print(data)
    # string to json
    data = eval(data)
    text = data["text"]
    socketio.start_background_task(get_motion,text)



@socketio.on('test')
def handle_test(data):
    print('received message: '+data)


if __name__ == '__main__':
    # app.debug = True
    socketio.run(app,host='127.0.0.1',port=5000)

