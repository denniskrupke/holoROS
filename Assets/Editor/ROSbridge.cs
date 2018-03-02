﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

using System.Xml;
using System;


// TODO: everything ;-) the idea is to pass settings from here to the behaviour scripts of the rosbridgeclient
public class ROSbridgeSettings : EditorWindow
{

    [MenuItem("Addons/ROSbridge")]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(ROSbridgeSettings));
    }

    private string ROSBRIDGE_IP = "134.100.13.203";
    private string ROSBRIDGE_PORT = "9090";

    void OnGUI()
    {
        ROSBRIDGE_IP = EditorGUILayout.TextField("IP", ROSBRIDGE_IP);
        ROSBRIDGE_PORT = EditorGUILayout.TextField("Port", ROSBRIDGE_PORT);
        if (GUILayout.Button("Apply"))
        {
            //ExportSettings();
        }
    }

}
