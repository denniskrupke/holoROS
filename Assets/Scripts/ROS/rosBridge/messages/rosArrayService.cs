#if WINDOWS_UWP
using Windows.Data.Json;
using System.Threading.Tasks;

// Dieser Namespace ist als ArrayService zu verstehen. Hier sind alle Methoden untergebacht, welche von anderen Klassen verwendet werden, um mit Arrays zu arbeiten.
namespace RosArrayService
{
    class RosArrayService_
    {
        // Für Deserialisierung
        public static string[] stringArrayAusJsonArray(JsonArray inputArray)
	    {
            string[] output = new string[inputArray.Count];
            for (int i = 0; i < inputArray.Count; i++)
            {
                output[i] = inputArray[i].GetString();
            }
        return output;
        }

	    public static double[] doubleArrayAusJsonArray(JsonArray inputArray)
	    {
            double[] output = new double[inputArray.Count];
            for (int i = 0; i < inputArray.Count; i++)
            {
                output[i] = inputArray[i].GetNumber();
            }
        return output;      
    }

    //Für Serialisierung
	public static string baueDoubleWertArrayUm(double[] werte, string name, bool isLast)
	{
            // Sonderfall: Array ist leer
            if (werte.GetLength(0) == 0)
            {
                return name + "[], ";
            }

		    string values = "";
            for(int i = 0; i<werte.GetLength(0)-1;i++)
            {
                values = values + werte[i] + ", ";
            }
            values = values + werte[werte.GetLength(0)-1];
            if (isLast)
            {
                return name + "[" + values + "]";
            }
            else
            {
                return name + "[" + values + "], ";
            }
	}

        public static string baueStringWertArrayUm(string[] werte, string name, bool isLast)
        {
            // Sonderfall: Array ist leer
            if (werte.GetLength(0) == 0)
            {
                return name + "[], ";
            }

            string values = "";
            for (int i = 0; i < werte.GetLength(0) - 1; i++)
            {
                values = values + "\"" + werte[i] + "\"" + ", ";
            }
            values = values + "\""+ werte[werte.GetLength(0) - 1] + "\"";
            if (isLast)
            {
                return name + "[" + values + "]";
            }
            else
            {
                return name + "[" + values + "], ";
            }
        }
    }

    public class RosArrayServiceObject
    {
        public RosArrayServiceObject()
        {
        }

        public string[] stringArrayAusJsonArray(JsonArray inputArray)
        {
            string[] output = new string[inputArray.Count];
            for (int i = 0; i < inputArray.Count; i++)
            {
                output[i] = inputArray[i].GetString();
            }
            return output;
        }

        public double[] doubleArrayAusJsonArray(JsonArray inputArray)
        {
            double[] output = new double[inputArray.Count];

            for(int i = 0; i < inputArray.Count; i++)
            {
                output[i] = inputArray[i].GetNumber();
            }
            return output;
        }

        public string baueDoubleWertArrayUm(double[] werte, string name, bool isLast)
        {
            // Sonderfall: Array ist leer
            if (werte.GetLength(0) == 0)
            {
                return name + "[], ";
            }

            string values = "";
            for (int i = 0; i < werte.GetLength(0) - 1; i++)
            {
                values = values + werte[i] + ", ";
            }
            values = values + werte[werte.GetLength(0) - 1];
            if (isLast)
            {
                return name + "[" + values + "]";
            }
            else
            {
                return name + "[" + values + "], ";
            }
        }

        public string baueStringWertArrayUm(string[] werte, string name, bool isLast)
        {
            // Sonderfall: Array ist leer
            if (werte.GetLength(0) == 0)
            {
                return name + "[], ";
            }

            string values = "";
            for (int i = 0; i < werte.GetLength(0) - 1; i++)
            {
                values = values + "\"" + werte[i] + "\"" + ", ";
            }
            values = values + "\"" + werte[werte.GetLength(0) - 1] + "\"";
            if (isLast)
            {
                return name + "[" + values + "]";
            }
            else
            {
                return name + "[" + values + "], ";
            }
        }
    }
}
#endif