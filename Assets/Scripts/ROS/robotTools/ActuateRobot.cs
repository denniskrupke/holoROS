using BioIK;
using System;
using UnityEngine;
using UnityEngine.UI;

public class ActuateRobot : MonoBehaviour
{
    private KinematicJoint[] joints;
    private string[] names;
    private double[] angles;
    private double[] lastAngles;
    public Text hudText = null;
    //public ros2unityManager rosBridgeManager = null;

    public string[] Names
    {
        get
        {
            return names;
        }

        set
        {
            names = value;
        }
    }

    public double[] Angles
    {
        get
        {
            return angles;
        }

        set
        {
            angles = value;
        }
    }

    // Use this for initialization
    void Start()
    {
        joints = gameObject.GetComponentsInChildren<KinematicJoint>();
        lastAngles = angles;
        //this.hudText.text = "\n started actuate robot script " + this.hudText.text;
    }

    // Update is called once per frame
    void Update()
    {
        //hudText.text = "" + joints.Length;
        //if (names != null && angles != null)
        {
            //  lastAngles = angles;
            foreach (KinematicJoint joint in joints)
            {
                //hudText.text += " " + joint.gameObject.name;
                //if (!joint.gameObject.name.Contains("finger")){
                int index = Array.IndexOf(names, joint.gameObject.name);
                if (index >= 0)
                {
                    float angle = (float)angles[Array.IndexOf(names, joint.gameObject.name)];
                    joint.GetXMotion().SetTargetValue(angle);
                }
            }
        }
        /* if (UpdateNeccesssary()) {
             lastAngles = angles;
             if (names != null && angles != null) {
                 foreach (KinematicJoint joint in joints) {
                     joint.GetXMotion().SetTargetValue((float)angles[Array.IndexOf(names, joint.gameObject.name)]);
                 }
             }
         } 
         */
    }

    void FixedUpdate()
    {
        /*
        if (names != null && angles != null) {
            foreach (KinematicJoint joint in joints) {
                joint.GetXMotion().SetTargetValue((float)angles[Array.IndexOf(names, joint.gameObject.name)]);
            }
        }
        */
    }

    bool UpdateNeccesssary()
    {
        //return true;
        //this.hudText.text = "\n checking" + this.hudText.text;
        int count = 0;
        this.hudText.text = "\n " + (Math.Abs(angles[0] - lastAngles[0])) + this.hudText.text;
        foreach (double val in angles)
        {
            //this.hudText.text = "\n val: " + val + this.hudText.text;
            if (Math.Abs(val - lastAngles[count++]) > 0.01) return true;

        }
        //for (int i=0; i<angles.Length; i++) {
        //   this.hudText.text = "\n " + (angles[i]- lastAngles[i]) + this.hudText.text;
        /*
        if (angles[i] != lastAngles[i]) {
            return true;

        }
        */
        //}
        return false;
    }

}
