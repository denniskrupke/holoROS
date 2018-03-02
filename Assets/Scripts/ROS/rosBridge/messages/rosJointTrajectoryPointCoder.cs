#if WINDOWS_UWP
using RosMessages;
using RosArrayService;
using RosDurationCoder;
using System;
using Windows.Data.Json;
using System.Threading;
using System.Threading.Tasks;
using RosStampCoder;


namespace RosJointTrajectoryPointCoder
{
    public class RosJointTrajectoryPointCoder_
    {
       // JointTrajectoryPoint _point;

        public JointTrajectoryPoint deserializeSimple(JsonValue jpoint)
        {

            // baue den Point
            JointTrajectoryPoint point = new JointTrajectoryPoint();
            RosArrayServiceObject ob = new RosArrayServiceObject();

            //Mache den Header wieder zu einem JsonObject, damit Key Value wieder funktioniert
            string geschachteltesHeaderJson = jpoint.ToString();
            JsonObject jsonPoint = JsonObject.Parse(geschachteltesHeaderJson);


            /* Parallelisierung
            Task[] tasks = new Task[4]
            {
                 Task.Factory.StartNew(() => bauePositionsArray(jpositions)),
                 Task.Factory.StartNew(() => baueVelocitiesArray(jvelocities)),
                 Task.Factory.StartNew(() => baueAccelerationsArray(jaccelerations)), 
                 Task.Factory.StartNew(() => baueEffortArray(jeffort))
            };
            */
            JsonArray jpositions = jsonPoint["positions"].GetArray();
            point.positions = ob.doubleArrayAusJsonArray(jpositions);

            JsonArray jvelocities = jsonPoint["velocities"].GetArray();
            point.velocities = ob.doubleArrayAusJsonArray(jvelocities);

            JsonArray jaccelerations = jsonPoint["accelerations"].GetArray();
            point.accelerations = ob.doubleArrayAusJsonArray(jaccelerations);

            JsonArray jeffort = jsonPoint["effort"].GetArray();
            point.effort = ob.doubleArrayAusJsonArray(jeffort);


            JsonObject jtime_from_start = jsonPoint["time_from_start"].GetObject(); 
            point.time_from_start = RosDurationCoder_.deserializeSingleDuration(jtime_from_start);

            //Task.WaitAll(tasks);
            return point;
        }
        private void bauePositionsArray(JsonArray array)
        {
           // _point.positions = RosArrayService_.doubleArrayAusJSonArray(array);
        }
        private void baueVelocitiesArray(JsonArray array)
        {
          //  _point.velocities = RosArrayService_.doubleArrayAusJSonArray(array);
        }
        private void baueAccelerationsArray(JsonArray array)
        {
           // _point.accelerations = RosArrayService_.doubleArrayAusJSonArray(array);
        }
        private void baueEffortArray(JsonArray array)
        {
          //  _point.effort = RosArrayService_.doubleArrayAusJSonArray(array);
        }

        public static string serializeSimple(JointTrajectoryPoint point)
        {
            // Baue PositionsArray
            double[] jtpositions = point.positions;
            string positionsString = RosArrayService_.baueDoubleWertArrayUm(jtpositions, "\"positions\": ", false);

            // Baue Accelerationsarray
            double[] jtaccelerations = point.accelerations;
            string accelerationsString = RosArrayService_.baueDoubleWertArrayUm(jtaccelerations, "\"accelerations\": ", false);

            // Baue Velocitiesarray
            double[] jtvelocities = point.velocities;
            string velocitiesString = RosArrayService_.baueDoubleWertArrayUm(jtvelocities, "\"velocities\": ", false);

            // Baue Effortsarray
            double[] jteffort = point.effort;
            string effortString = RosArrayService_.baueDoubleWertArrayUm(jteffort, "\"effort\": ", false);

            string durationString = "\"time_from_start\": " + RosDurationCoder_.serializeSingleDuration(point.time_from_start);

            return baueJointTrajectoryString(positionsString,velocitiesString,accelerationsString,effortString, durationString);
        }

        private static string baueJointTrajectoryString(string positions, string velocities, string accelerations, string effort, string time_from_start)
        {
            return "{"+ positions +accelerations + velocities + effort + time_from_start + "}";
        }
    }
}

#endif