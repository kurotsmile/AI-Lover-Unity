﻿using UnityEngine;

[AddComponentMenu("Camera-Control/Mouse Orbit with zoom")]
public class MouseOrbitImproved : MonoBehaviour
{
    public Camera cam;
    public Transform target;
    public float distance = 5.0f;
    public float xSpeed = 120.0f;
    public float ySpeed = 120.0f;

    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;

    public float distanceMin = .5f;
    public float distanceMax = 15f;

    float x = 0.0f;
    float y = 0.0f;

    private Type_Rotate_Character rotate_character=Type_Rotate_Character.statics;
    public bool is_view_portrait = true;

    [Header("GyroControl")]
    private Quaternion localRotation;
    float speed = 1f;

    void Start()
    {
        this.localRotation = this.target.rotation;
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
    }

    void LateUpdate()
    {
        if (this.rotate_character == Type_Rotate_Character.touch)
        {
            if (target)
            {
                x += Input.GetAxis("Mouse X") * xSpeed * distance * 0.02f;
                y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

                y = ClampAngle(y, yMinLimit, yMaxLimit);

                Quaternion rotation = Quaternion.Euler(y, x, 0);

                distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * 6, distanceMin, distanceMax);

                RaycastHit hit;
                if (Physics.Linecast(target.position, transform.position, out hit))
                {
                    distance -= hit.distance;
                }
                Vector3 negDistance = new Vector3(0.0f, 0.00f, -distance);
                Vector3 position = rotation * negDistance + target.position;

                transform.rotation = rotation;
                transform.position = new Vector3(position.x,position.y+1.2f,position.z);
            }
        }

        if (this.rotate_character==Type_Rotate_Character.sensor)
        {
            float curSpeed = Time.deltaTime * speed;
            localRotation.y += Input.acceleration.x * curSpeed;
            if (localRotation.y > -0.4f && localRotation.y < 0.4f) this.target.rotation = localRotation;
        }

    }


    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }

    public void Reset_pos()
    {
        if (this.is_view_portrait)
        {
            this.transform.localPosition = new Vector3(0f, 1.8f, 10f);
            this.transform.localRotation = Quaternion.Euler(2, 180f, 0f);
        }
        else
        {
            this.transform.localPosition = new Vector3(1.2f, 3.04f, 8.8f);
            this.transform.localRotation = Quaternion.Euler(10, 180f, 0f);
        }

        this.target.rotation = Quaternion.Euler(Vector3.zero);
    }

    public void set_mode(Type_Rotate_Character model)
    {
        this.rotate_character = model;
        this.Reset_pos();
    }
}