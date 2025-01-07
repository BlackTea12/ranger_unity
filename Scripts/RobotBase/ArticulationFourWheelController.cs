using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ControlMode { InPhase=0, OppositPhase, PivotTurn };
public class ArticulationFourWheelController : MonoBehaviour
{
    public ArticulationBody flWheel, frWheel, rlWheel, rrWheel;
    public ArticulationBody flSteer, frSteer, rlSteer, rrSteer;

    public ControlMode drivingMode = ControlMode.OppositPhase;
    public float linearVelocityX, linearVelocityY, angularVelocity;    // [m/s] and [rad/s]

    /// <ranger unique value>
    private readonly float wheel_base = 0.364f, wheel_radius = 0.1f, wheel_steering_y_offset = 0.05f; //wheel_seperation = 0.494f
    private readonly float steering_track = 0.394f; // (=wheel_seperation - 2f*wheel_steering_y_offset)
    public readonly float[] vel = { 0, 0, 0, 0 };
    public readonly float[] pose = { 0, 0, 0, 0 };

    void Start() {}

    void FixedUpdate() 
    {
        SetRobotVelocity(linearVelocityX, angularVelocity);
    }

    public void SetRobotVelocity(float targetLinearSpeed, float targetAngularSpeed)
    {
        switch (drivingMode)
        {
            case ControlMode.InPhase:
            {
                float linearVel = new Vector2(linearVelocityX, linearVelocityY).sqrMagnitude;
                float sign = Mathf.Sign(linearVelocityX);
                float angular = 0f;
                
                if (linearVelocityX != 0.0f)
                {
                    angular = linearVelocityY / linearVelocityX;
                }
                vel[0] = sign*linearVel;
                vel[1] = sign*linearVel;
                vel[2] = sign*linearVel;
                vel[3] = sign*linearVel;
                pose[0] = Mathf.Atan(angular);
                pose[1] = Mathf.Atan(angular);
                pose[2] = pose[0];
                pose[3] = pose[1];
            }
            break;

            case ControlMode.OppositPhase:
            {
                float vel_steerring_offset = angularVelocity * wheel_steering_y_offset;
                float sign = Mathf.Sign(linearVelocityX);
                float left_ele = new Vector2(linearVelocityX - angularVelocity*steering_track/2, angularVelocity*wheel_base/2).sqrMagnitude;
                float right_ele = new Vector2(linearVelocityX + angularVelocity*steering_track/2, angularVelocity*wheel_base/2).sqrMagnitude;
                vel[0] = sign*left_ele - vel_steerring_offset;
                vel[1] = sign*right_ele + vel_steerring_offset;
                vel[2] = sign*left_ele - vel_steerring_offset;
                vel[3] = sign*right_ele + vel_steerring_offset;
                pose[0] = Mathf.Atan(angularVelocity*wheel_base/(2f*linearVelocityX+angularVelocity*steering_track));
                pose[1] = Mathf.Atan(angularVelocity*wheel_base/(2f*linearVelocityX + angularVelocity*steering_track));
                pose[2] = -pose[0];
                pose[3] = -pose[1];
            }
            break;
            
            case ControlMode.PivotTurn:
            {
                pose[0] = -Mathf.Atan(wheel_base/steering_track);
                pose[1] = Mathf.Atan(wheel_base/steering_track);
                pose[2] = Mathf.Atan(wheel_base/steering_track);
                pose[3] = -Mathf.Atan(wheel_base/steering_track);
                
                vel[0] = -angularVelocity;
                vel[1] = angularVelocity;
                vel[2] = vel[0];
                vel[3] = vel[1];
            }
            break;

            default:
            {
                StopWheel(flWheel);
                StopWheel(frWheel);
                StopWheel(rlWheel);
                StopWheel(rrWheel);
                StopWheel(rrSteer);
                StopWheel(frSteer);
                StopWheel(rlSteer);
                StopWheel(rrSteer);
            }
            break;
        }
        
        SetWheelVelocity(flWheel, vel[0] / wheel_radius * Mathf.Rad2Deg);
        SetWheelVelocity(frWheel, vel[1] / wheel_radius * Mathf.Rad2Deg);
        SetWheelVelocity(rlWheel, vel[2] / wheel_radius * Mathf.Rad2Deg);
        SetWheelVelocity(rrWheel, vel[3] / wheel_radius * Mathf.Rad2Deg);
        SetWheelPosition(flSteer, pose[0] * Mathf.Rad2Deg);
        SetWheelPosition(frSteer, pose[1] * Mathf.Rad2Deg);
        SetWheelPosition(rlSteer, pose[2] * Mathf.Rad2Deg);
        SetWheelPosition(rrSteer, pose[3] * Mathf.Rad2Deg);
        // Stop the wheel if target velocity is 0
        // if (targetLinearSpeed == 0 && targetAngularSpeed == 0)
        // {
        //     StopWheel(leftWheel);
        //     StopWheel(rightWheel);
        // }
        // else
        // {
        //     // Convert from linear x and angular z velocity to wheel speed
        //     vRight = targetAngularSpeed*(wheelTrackLength/2) + targetLinearSpeed;
        //     vLeft = -targetAngularSpeed*(wheelTrackLength/2) + targetLinearSpeed;

        //     SetWheelVelocity(leftWheel, vLeft / wheelRadius * Mathf.Rad2Deg);
        //     SetWheelVelocity(rightWheel, vRight / wheelRadius * Mathf.Rad2Deg);
        // }
    }
    /// <param name="jointVelocity">deg/s</param>
    private void SetWheelVelocity(ArticulationBody wheel, float jointVelocity)
    {
        ArticulationDrive drive = wheel.xDrive;
        drive.target += jointVelocity * Time.fixedDeltaTime;
        wheel.xDrive = drive;
    }

    /// <param name="jointAngle">deg</param>
    private void SetWheelPosition(ArticulationBody wheel, float jointAngle)
    {
        ArticulationDrive drive = wheel.xDrive;
        drive.target = jointAngle;
        wheel.xDrive = drive;
    }
    private void StopWheel(ArticulationBody wheel)
    {
        ArticulationDrive drive = wheel.xDrive;
        drive.target = wheel.jointPosition[0] * Mathf.Rad2Deg;
        wheel.xDrive = drive;
    }
}
