#if WINDOWS_UWP
using Windows.Data.Json;
using RosMessages;
using RosArrayService;
using RosHeaderCoder;
using RosJointTrajectoryPointCoder;
using System;

namespace RosJointTrajectoryCoder
{

    class RosJointTrajectoryCoder_ : rosComplexCoder
    {
        private JointTrajectory latestJointTrajectory;

        public RosJointTrajectoryCoder_()
        {
            latestJointTrajectory = new JointTrajectory();
        }

        // hier wird einfach hart-gecoded eine JointState-Nachricht deserialisiert
        public RosPublish startDeserializing(JsonObject jsonObject)
        {
            return deserializeJointTrajectory(jsonObject);
        }

        // die andere Richtung wäre auch schön, damit man auch Nachrichten an ROS senden kann
        public string startSerializing(RosMessage message)
        {
            //todo
            JointTrajectory jt = new JointTrajectory();

            RosPublish publishMessage = (RosPublish)message;

            MessageData msg = publishMessage.msg;
            if (msg.GetType() == typeof(JointTrajectory))
            {
                jt = (JointTrajectory)msg;
            }

            return serializeJointTrajectory(jt);

        }

        public JointTrajectory getJointTrajectory()
        {
            return latestJointTrajectory;
        }

        private bool isJointTrajectory(string message)
        {
            return message.Equals("/joint_trajectories");
        }

        private RosPublish deserializeJointTrajectory(JsonObject jsonObject)
        {

            JsonArray jjoint_names = jsonObject["msg"].GetObject()["joint_names"].GetArray();


            // baue den Header
            Header header = RosHeaderCoder_.deserializeSingleHeader((JsonValue)jsonObject["msg"].GetObject()["header"]);

            // Lasse die JointTrajectoryPoints vom jeweiligen Coder deserialisieren

            JsonArray jjointtrajectorypoints = jsonObject["msg"].GetObject()["points"].GetArray();

            //TODO JointTrajectoryPoints deserialisieren
            JointTrajectoryPoint[] pts = new JointTrajectoryPoint[jjointtrajectorypoints.Count];
            for (int i = 0; i < jjointtrajectorypoints.Count; i++)
            {
                pts[i] = RosJointTrajectoryPointCoder_.deserializeSingleJointTrajectoryPoint((JsonValue)jjointtrajectorypoints[i]);
            }


            JointTrajectory messageData = new JointTrajectory();
            messageData.header = header;
            // Nutze den ArrayService, um die JSONArrays in "normale" Arrays umzuwandeln
            messageData.joint_names = RosArrayService_.stringArrayAusJSonArray(jjoint_names);
            messageData.points = pts;


            return new RosPublish("\"/preview_trajectory\"", messageData);

        }

        private string serializeJointTrajectory(JointTrajectory jt)
        {
            //serialisiere Topic
            string topic = "\"topic\": \"/preview_trajectory\", "; // platzhalter, topic vom RosMessage- objekt später holen

            // serialisiere header
            Header jtheader = jt.header;
            string headerString = RosHeaderCoder_.serializeSingleHeader(jtheader);

            //serialisiere joint_names
            string[] jtjoint_names = jt.joint_names;
            string joint_names_string = RosArrayService_.baueStringWertArrayUm(jtjoint_names, "\"joint_names\": ", false);

            //serialisiere JoinTrajectoryPoints
            JointTrajectoryPoint[] jtps = jt.points;
            string points_string = "";
            for(int i = 0; i<jtps.Length-2; ++i)
            {
                points_string = points_string + RosJointTrajectoryPointCoder_.serializeSingleJointTrajectoryPoint(jtps[i]) + ", ";
            }
            points_string = points_string + RosJointTrajectoryPointCoder_.serializeSingleJointTrajectoryPoint(jtps[jtps.Length-1]);
            points_string = "\"points: \"[" + points_string + "]";


            // Einzelteile zusammenkleben und formatiert zurückgeben
            return "\"{" + topic + "\"msg\": {" + headerString + joint_names_string + points_string + "}, \"op\": \"publish\"}\"";
        }
    }
}
#endif