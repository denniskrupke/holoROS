using System.IO;
using UnityEngine;



public class ExperimentFileWriter : MonoBehaviour
{
    public string filename = "default.csv";
    // Use this for initialization
    void Start()
    {
        /*
        string path = Path.Combine(Application.persistentDataPath, filename);
        using (StreamWriter writer = File.CreateText(path))
        {
            //writer.WriteLine("0");
            //writer.Flush();
            writer.Dispose();
        }
        */
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void WriteTestFile()
    {
        string path = Path.Combine(Application.persistentDataPath, "TestFileMoop.txt");
        using (StreamWriter writer = File.CreateText(path))
        {
            writer.WriteLine("0");
            writer.Flush();
            writer.Dispose();
        }
    }

    public int RetieveLastParticipantID()
    {
        string path = Path.Combine(Application.persistentDataPath, this.filename);

        int lastParticipant = 0;
        string[] lines = File.ReadAllLines(path);
        if (lines.Length > 0)
        {
            string lastLine = lines[lines.Length - 2];                        
            int.TryParse(lastLine.Split(',')[0], out lastParticipant); //first csv element
        }

        return lastParticipant;
    }

    public void AppendLineToFile(string line)
    {
        string path = Path.Combine(Application.persistentDataPath, this.filename);
        //int lastParticipant = RetieveLastParticipantID();

        
        using (StreamWriter writer = File.AppendText(path))
        {            
            writer.WriteLine(line);
            writer.Flush();
            writer.Dispose();
        }        
    }

    public string ExperimentDataFrame2String(ExperimentDataFrame frame)
    {
        string data = "";
        data += frame.id_participant;
        data += ",";
        data += frame.method;
        data += ",";
        data += frame.trial;
        data += ",";
        data += frame.currentAngle;
        data += ",";
        data += frame.cylinderIndex;
        data += ",";
        data += frame.timeStamp_start;
        data += ",";
        data += frame.timeStamp_stop;
        data += ",";
        data += "[";
        data += frame.position_cylinder.x;
        data += ",";
        data += frame.position_cylinder.y;
        data += ",";
        data += frame.position_cylinder.z;
        data += "]";
        data += ",";
        data += "[";
        data += frame.position_target.x;
        data += ",";
        data += frame.position_target.y;
        data += ",";
        data += frame.position_target.z;
        data += "]";
        data += ",";
        data += "[";
        data += frame.position_user.x;
        data += ",";
        data += frame.position_user.y;
        data += ",";
        data += frame.position_user.z;
        data += "]";
        data += "[";
        data += frame.rotation_user.x;
        data += ",";
        data += frame.rotation_user.y;
        data += ",";
        data += frame.rotation_user.z;
        data += ",";
        data += frame.rotation_user.w;
        data += "]";
        data += ",";
        data += frame.distance_euklid_user2cylinder_before;
        data += ",";
        data += frame.distance_euklid_user2cylinder_after;
        data += ",";
        data += frame.distance_euklid_target2selection;
        data += ",";
        data += frame.error;
        data += ",";
        data += frame.time;

        return data;
    }
}
