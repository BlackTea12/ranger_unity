using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ControlMode { InPhase=0, OppositPhase, PivotTurn };
public class ArticulationFourWheelController : MonoBehaviour
{
    [Header("[Wheel Gameobjects]")]
    public ArticulationBody flWheel; 
    public ArticulationBody  frWheel;
    public ArticulationBody  rlWheel;
    public ArticulationBody  rrWheel;

    [Header("[Steering Gameobjects]")]
    public ArticulationBody flSteer;
    public ArticulationBody  frSteer;
    public ArticulationBody  rlSteer;
    public ArticulationBody  rrSteer;

    [Header("[Driving Options]")]
    public ControlMode drivingMode = ControlMode.OppositPhase;
    
    [Header("[Maximum Value]")]
    [Tooltip("[m/s]")]
    public float linearVelocityX;
    [Tooltip("[m/s]")]
    public float linearVelocityY;
    [Tooltip("[deg/s]")]
    public float angularVelocity;

    /// <ranger unique value>
    private readonly float wheel_base = 0.364f, wheel_radius = 0.1f, wheel_steering_y_offset = 0.05f; //wheel_seperation = 0.494f
    private readonly float steering_track = 0.394f; // (=wheel_seperation - 2f*wheel_steering_y_offset)
    
    [Header("[Observations]")]
    public readonly float[] vel = { 0.0f, 0.0f, 0.0f, 0.0f };
    public readonly float[] pose = { 0.0f, 0.0f, 0.0f, 0.0f };

    void Start() {}

    void FixedUpdate() 
    {
        float limit_vx = linearVelocityX;   // (m/s)
        float limit_vy = linearVelocityY;   // (m/s)
        float limit_w = angularVelocity*Mathf.PI/180.0f; // (rad/s)
        float[] result = UserControl(limit_vx, limit_w);
        SetRobotVelocity(result[0], result[1]);
    }

    public float[] UserControl(float max_vx, float max_w)
    {
        float inputX = 0.0f, inputY=0.0f;
        float[] result = {0.0f, 0.0f}; // linear speed [m/s], angular speed [rad/s]
        if (Input.GetKey(KeyCode.W)) {
            inputX = 1.0f;
        } else if (Input.GetKey(KeyCode.S)) {
            inputX = -1.0f;
        }

        if (Input.GetKey(KeyCode.D)) {
            inputY = 1.0f;
        } else if (Input.GetKey(KeyCode.A)) {
            inputY = -1.0f;
        }

        result[0] = max_vx*inputX;
        result[1] = max_w*inputY;
        return result;
    }

    public void SetRobotVelocity(float targetLinearSpeed, float targetAngularSpeed)
    {
        switch (drivingMode)
        {
            case ControlMode.InPhase:
            {
                float linearVel = new Vector2(targetLinearSpeed, linearVelocityY).sqrMagnitude;
                float sign = Mathf.Sign(targetLinearSpeed);
                float angular = 0f;
                
                if (targetLinearSpeed != 0.0f)
                {
                    angular = linearVelocityY / targetLinearSpeed;
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
                // compute speed
                float vel_steerring_offset = (targetAngularSpeed * wheel_steering_y_offset)/wheel_radius;
                float sign = Mathf.Sign(targetLinearSpeed);
                float denom = targetAngularSpeed*wheel_base/2.0f, numer = targetAngularSpeed*steering_track/2.0f;
                float left_ele = new Vector2(targetLinearSpeed - numer, denom).sqrMagnitude / wheel_radius;
                float right_ele = new Vector2(targetLinearSpeed + numer, denom).sqrMagnitude / wheel_radius;
                vel[0] = sign*left_ele - vel_steerring_offset;
                vel[1] = sign*right_ele + vel_steerring_offset;
                vel[2] = vel[0];
                vel[3] = vel[1];

                // compute steering
                if (Mathf.Abs(targetAngularSpeed) > 0.001f) {
                    float twice_speed = 2.0f*targetLinearSpeed, track_angular = targetAngularSpeed*steering_track;
                    if (Mathf.Abs(twice_speed) > Mathf.Abs(track_angular)) {
                        pose[0] = Mathf.Atan(targetAngularSpeed*wheel_base/(twice_speed - track_angular));
                        pose[1] = Mathf.Atan(targetAngularSpeed*wheel_base/(twice_speed + track_angular));
                    } else {
                        pose[0] = Mathf.Sign(targetAngularSpeed)*Mathf.PI/2.0f;
                        pose[1] = pose[0];
                    }
                    pose[2] = -pose[0];
                    pose[3] = -pose[1];
                } else {
                    pose[0] = pose[1] = pose[2] = pose [3] = 0.0f;
                }
            }
            break;
            
            case ControlMode.PivotTurn:
            {
                pose[0] = -Mathf.Atan(wheel_base/steering_track);
                pose[1] = Mathf.Atan(wheel_base/steering_track);
                pose[2] = Mathf.Atan(wheel_base/steering_track);
                pose[3] = -Mathf.Atan(wheel_base/steering_track);
                
                vel[0] = -targetAngularSpeed;
                vel[1] = targetAngularSpeed;
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
        
        // set values
        Debug.Log($"vel: {vel[0]}, {vel[1]}, {vel[2]}, {vel[3]}");
        Debug.Log($"pose: {pose[0]}, {pose[1]}, {pose[2]}, {pose[3]}");
        SetWheelVelocity(flWheel, vel[0]);
        SetWheelVelocity(frWheel, vel[1]);
        SetWheelVelocity(rlWheel, vel[2]);
        SetWheelVelocity(rrWheel, vel[3]);
        SetWheelPosition(flSteer, pose[0]);
        SetWheelPosition(frSteer, pose[1]);
        SetWheelPosition(rlSteer, pose[2]);
        SetWheelPosition(rrSteer, pose[3]);

    }
    /// <param name="jointVelocity">deg/s</param>
    private void SetWheelVelocity(ArticulationBody wheel, float jointVelocity)
    {
        ArticulationDrive drive = wheel.xDrive;
        drive.targetVelocity = jointVelocity * Mathf.Rad2Deg;
        wheel.xDrive = drive;
    }

    /// <param name="jointAngle">deg</param>
    private void SetWheelPosition(ArticulationBody wheel, float jointAngle)
    {
        ArticulationDrive drive = wheel.xDrive;
        drive.target = jointAngle * Mathf.Rad2Deg;
        wheel.xDrive = drive;
    }
    private void StopWheel(ArticulationBody wheel)
    {
        ArticulationDrive drive = wheel.xDrive;
        drive.target = wheel.jointPosition[0] * Mathf.Rad2Deg;
        wheel.xDrive = drive;
    }
}
