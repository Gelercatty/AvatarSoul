import numpy as np
import matplotlib.pyplot as plt
from mpl_toolkits.mplot3d import Axes3D
from mGPT.render.pyrender.hybrik_loc2rot import HybrIKJointsToRotmat,SMPL_BODY_BONES

from scipy.spatial.transform import Rotation as RRR



# def plot_joint_axes(joints, pose, frame_index, joint_index):
#     """
#     Plot the rotation axes for a specific joint from a specific frame.
    
#     Parameters:
#         joints (numpy.ndarray): The joint positions of shape [frames, 22, 3]
#         pose (numpy.ndarray): The rotation matrices of shape [frames, 22, 3, 3]
#         frame_index (int): Index of the frame to plot the rotation axes from
#         joint_index (int): Index of the joint to plot the rotation axes
#     """
#     fig = plt.figure()
#     ax = fig.add_subplot(111, projection='3d')

#     # Joint position from the specific frame and joint index
#     joint_pos = joints[frame_index, joint_index]

#     # Rotation matrix for the specific joint from the specific frame
#     rot_mat = pose[frame_index, joint_index]

#     # Axes vectors
#     axes_length = 0.1  # Scale for better visualization
#     x_axis = rot_mat[:, 0] * axes_length
#     y_axis = rot_mat[:, 1] * axes_length
#     z_axis = rot_mat[:, 2] * axes_length

#     # Plotting the joint axes
#     ax.quiver(joint_pos[0], joint_pos[1], joint_pos[2],
#               x_axis[0], x_axis[1], x_axis[2], color='r', label='X-axis')
#     ax.quiver(joint_pos[0], joint_pos[1], joint_pos[2],
#               y_axis[0], y_axis[1], y_axis[2], color='g', label='Y-axis')
#     ax.quiver(joint_pos[0], joint_pos[1], joint_pos[2],
#               z_axis[0], z_axis[1], z_axis[2], color='b', label='Z-axis')

#     # Setting the plot limits
#     ax.set_xlim([joint_pos[0] - 0.2, joint_pos[0] + 0.2])
#     ax.set_ylim([joint_pos[1] - 0.2, joint_pos[1] + 0.2])
#     ax.set_zlim([joint_pos[2] - 0.2, joint_pos[2] + 0.2])

#     # Labels and title
#     ax.set_xlabel('X')
#     ax.set_ylabel('Y')
#     ax.set_zlabel('Z')
#     ax.set_title(f'Rotation Axes for Joint {joint_index} in Frame {frame_index}')
#     ax.legend()

#     plt.show()

# # Example usage
# if __name__ == "__main__":
#     jts2rot_hybrik = HybrIKJointsToRotmat()

#     joints = np.load(r"D:\Geler也要发论文\llm\motionGPT\motionGPT\MotionGPT\expereiment_TaiChiGPT\2024-02-27-02_15_4668067_joints.npy") 
#     print(joints.shape)

#     pose = jts2rot_hybrik(joints)  # Assuming 'joints' is already computed as from previous code
#     print("Shape of pose:", pose.shape)  # Print the shape of the pose array

#     # Plot the joint axes for the first joint in the first frame
#     plot_joint_axes(joints, pose, frame_index=0, joint_index=0)




##############################

import numpy as np
import matplotlib.pyplot as plt
from mpl_toolkits.mplot3d import Axes3D

def plot_joint_axes(joints, pose, frame_index, axes_joints):
    fig = plt.figure()
    ax = fig.add_subplot(111, projection='3d')

    num_joints = joints.shape[1]
    axes_length = 0.1  # Scale for better visualization

    # Plot all joints positions
    for j in range(num_joints):
        joint_pos = joints[frame_index, j]
        ax.scatter(joint_pos[0], joint_pos[1], joint_pos[2], color='grey', alpha=0.5)  # less emphasis on other joints
        # scatter data structure: https://matplotlib.org/stable/api/_as_gen/matplotlib.pyplot.scatter.html
        # (x,y,z)
    # Plot axes for specified joints
    for joint_index in axes_joints:
        joint_pos = joints[frame_index, joint_index]
        rot_mat = pose[frame_index, joint_index]

        x_axis = rot_mat[:, 0] * axes_length
        y_axis = rot_mat[:, 1] * axes_length
        z_axis = rot_mat[:, 2] * axes_length

        ax.quiver(joint_pos[0], joint_pos[1], joint_pos[2],
                  x_axis[0], x_axis[1], x_axis[2], color='r', label='X-axis' if joint_index == axes_joints[0] else "")
        ax.quiver(joint_pos[0], joint_pos[1], joint_pos[2],
                  y_axis[0], y_axis[1], y_axis[2], color='g', label='Y-axis' if joint_index == axes_joints[0] else "")
        ax.quiver(joint_pos[0], joint_pos[1], joint_pos[2],
                  z_axis[0], z_axis[1], z_axis[2], color='b', label='Z-axis' if joint_index == axes_joints[0] else "")

    # Marking the World Coordinate System Origin
    ax.scatter(0, 0, 0, color='k', marker='o', s=100, label='World Origin')

    # Setting the plot limits and labels
    all_joints = joints[frame_index, :, :].reshape(-1, 3)
    buffer = 0.5
    ax.set_xlim([all_joints[:, 0].min() - buffer, all_joints[:, 0].max() + buffer])
    ax.set_ylim([all_joints[:, 1].min() - buffer, all_joints[:, 1].max() + buffer])
    ax.set_zlim([all_joints[:, 2].min() - buffer, all_joints[:, 2].max() + buffer])

    ax.set_xlabel('X')
    ax.set_ylabel('Y')
    ax.set_zlabel('Z')
    ax.set_title(f'Rotation Axes for Selected Joints in Frame {frame_index}')
    ax.legend()

    plt.show()

# Example usage
if __name__ == "__main__":
    jts2rot_hybrik = HybrIKJointsToRotmat()

    joints = np.load(r"D:\Geler也要发论文\llm\motionGPT\motionGPT\MotionGPT\expereiment_TaiChiGPT\2024-02-27-02_15_4668067_joints.npy")
    
    
    pose = jts2rot_hybrik(joints)  # Assuming 'joints' is already computed as from previous code
    # r = RRR.from_rotvec(np.array([np.pi/2, 0.0, 0.0])) 
    # pose[:, 0] = np.matmul(r.as_matrix().reshape(1, 3, 3), pose[:, 0])
    
    frame_index = 0
    axes_joints = [9,13,14,16,17,1,2,4,5,7,8,11,10]  # Joints at which to draw axes
    plot_joint_axes(joints, pose, frame_index, axes_joints)