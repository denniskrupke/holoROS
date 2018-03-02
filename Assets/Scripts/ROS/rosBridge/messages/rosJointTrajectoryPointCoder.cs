#if WINDOWS_UWP
using RosMessages;
using RosArrayService;
using RosDurationCoder;
using System;
using Windows.Data.Json;
using RosStampCoder;


namespace RosJointTrajectoryPointCoder
{
    public static class RosJointTrajectoryPointCoder_
    {

        public static JointTrajectoryPoint deserializeSingleJointTrajectoryPoint(JsonValue jpoint)
        {
            // baue den Point
            JointTrajectoryPoint point = new JointTrajectoryPoint();

            //Mache den Header wieder zu einem JsonObject, damit Key Value wieder funktioniert
            string geschachteltesHeaderJson = jpoint.ToString();
            JsonObject jsonPoint = JsonObject.Parse(geschachteltesHeaderJson);

           
            JsonArray jpositions = jsonPoint["positions"].GetArray();


            JsonArray jvelocities = jsonPoint["velocities"].GetArray();


            JsonArray jaccelerations = jsonPoint["accelerations"].GetArray();


            JsonArray jeffort = jsonPoint["effort"].GetArray();


            JsonObject jtime_from_start = jsonPoint["time_from_start"].GetObject(); 

            point.positions = RosArrayService_.doubleArrayAusJSonArray(jpositions);
            point.velocities = RosArrayService_.doubleArrayAusJSonArray(jvelocities);
            point.accelerations = RosArrayService_.doubleArrayAusJSonArray(jaccelerations);
            point.effort = RosArrayService_.doubleArrayAusJSonArray(jeffort);
            point.time_from_start = RosDurationCoder_.deserializeSingleDuration(jtime_from_start);

            return point;
        }

        public static string serializeSingleJointTrajectoryPoint(JointTrajectoryPoint point)
        {
            // TODO

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