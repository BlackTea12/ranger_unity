# Importing ranger mini v2 into Unity package
<div align="right">

  <a href="">![Ubuntu](https://img.shields.io/badge/Ubuntu-22.04-green)</a>
  <a href="">![ROS2](https://img.shields.io/badge/ROS2-humble-blue)</a>
  <a href="">![Unity](https://img.shields.io/badge/Unity-2022.3.41f1-red)</a>

</div>

## Unity Package Dependencies
- URDF Importer

## Ranger Model
```
git clone https://github.com/agilexrobotics/ugv_gazebo_sim.git -b humble
```
After cloning the repository, follow the steps below.
1. In your Unity Project, make a folder path '*Assets* > *URDF* > *ranger*'
2. In your cloned github repository
   ```
      cd ~/ranger/ranger_mini/urdf
      xacro ranger_mini_gazebo.xacro > ranger_mini_gazebo.urdf
   ```
3. Copy '*ranger_mini_gazebo.urdf*' in to Unity Project '*Assets* > *URDF* > *ranger*'
4. Right click urdf file and select '*import* *urdf*~'
