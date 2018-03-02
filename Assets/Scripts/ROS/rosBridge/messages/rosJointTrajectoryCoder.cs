#if WINDOWS_UWP
using Windows.Data.Json;
using RosMessages;
using RosArrayService;
using RosHeaderCoder;
using RosJointTrajectoryPointCoder;
using System.Threading.Tasks;
using System;

namespace RosJointTrajectoryCoder
{

    class RosJointTrajectoryCoder_ : rosComplexCoder
    {
     //   private JointTrajectoryPoint[] result;

        public RosJointTrajectoryCoder_()
        {
        }

        // Start der Deserialisierung, Schnittstelle
        public RosPublish startDeserializing(JsonObject jsonObject)
        {
            return deserializeJointTrajectory(jsonObject);
        }

        // Start der Serialisierung, Schnittstelle
        public string startSerializing(RosMessage message)
        {
            JointTrajectory jt = new JointTrajectory();

            string topic = message.topic;

            RosPublish publishMessage = (RosPublish)message;

            MessageData msg = publishMessage.msg;
            if (msg.GetType() == typeof(JointTrajectory))
            {
                jt = (JointTrajectory)msg;
            }

            return serializeJointTrajectory(jt, topic);
        }

        private RosPublish deserializeJointTrajectory(JsonObject jsonObject)
        {
            JointTrajectory messageData = new JointTrajectory();
            RosJointTrajectoryPointCoder_ pointCoder = new RosJointTrajectoryPointCoder_();

            // Nutze den ArrayService, um die JSONArrays in "normale" Arrays umzuwandeln
            JsonArray jjoint_names = jsonObject["msg"].GetObject()["joint_names"].GetArray();

            // baue den Header
            Header header = RosHeaderCoder_.deserializeSingleHeader((JsonValue)jsonObject["msg"].GetObject()["header"]);

            JsonArray jjointtrajectorypoints = jsonObject["msg"].GetObject()["points"].GetArray();
            int jtpCount = jjointtrajectorypoints.Count;

            // Lasse die JointTrajectoryPoints vom jeweiligen Coder deserialisieren
            /* Multithreading
        * result = new JointTrajectoryPoint[jtpCount];
       Task[] tasks = new Task[1]
       {
            Task.Factory.StartNew(() => baueJointTrajectoryPointArray(jjointtrajectorypoints, jtpCount, pointCoder))
       };
       */
            JointTrajectoryPoint[] pts = new JointTrajectoryPoint[jtpCount];
            Parallel.For(0, jtpCount, i =>
            {
                pts[i] = pointCoder.deserializeSimple((JsonValue)jjointtrajectorypoints[i]);
            });
            // tasks[0].Wait();

            messageData.joint_names = RosArrayService_.stringArrayAusJsonArray(jjoint_names);
            messageData.header = header;
            messageData.points = pts;

            return new RosPublish("\"/preview_trajectory\"", messageData);
        }
        // Threading
        private void baueJointTrajectoryPointArray(JsonArray array, int count, RosJointTrajectoryPointCoder_ coder)
        {
            Parallel.For(0, count, i =>
            {
 //               result[i] = coder.deserializeSimple((JsonValue)array[i]);
            });
        }

        private string serializeJointTrajectory(JointTrajectory jt, string topic)
        {
            //serialisiere Topic
            //string topic = "\"topic\": \"/preview_trajectory\", "; // platzhalter, topic vom RosMessage- objekt später holen : DONE
            string ausgabeTopic = "\"topic\": " + topic + ", ";

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
                points_string = points_string + RosJointTrajectoryPointCoder_.serializeSimple(jtps[i]) + ", ";
            }
            points_string = points_string + RosJointTrajectoryPointCoder_.serializeSimple(jtps[jtps.Length-1]);
            points_string = "\"points: \"[" + points_string + "]";


            // Einzelteile zusammenkleben und formatiert zurückgeben
            return "\"{" + ausgabeTopic + "\"msg\": {" + headerString + joint_names_string + points_string + "}, \"op\": \"publish\"}\"";
        }
    }
}
#endif