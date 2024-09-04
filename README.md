# ranger_unity
Importing ranger mini model into Unity.
This is tested in **Ubuntu 22.04LTS**

## Unity Package Dependencies
- URDF Importer
- 
## Ranger Model
```
git clone https://github.com/agilexrobotics/ugv_gazebo_sim.git -b humble
```
After cloning the repository, follow the steps below.
1. In your Unity Project, make a folder path 'Assets > URDF > ranger'
2. In your cloned github repository
   '''
   cd ~/ranger/ranger_mini/urdf
   xacro ranger_mini_gazebo.xacro > ranger_mini_gazebo.urdf
   '''
3. Copy 'ranger_mini_gazebo.urdf' in to Unity Project 'Assets > URDF > ranger'
4. Right click urdf file and select 'import urdf~'
