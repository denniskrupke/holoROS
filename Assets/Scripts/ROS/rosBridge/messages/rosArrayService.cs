#if WINDOWS_UWP
using Windows.Data.Json;
// Dieser Namespace ist als ArrayService zu verstehen. Hier sind alle Methoden untergebacht, welche von anderen Klassen verwendet werden, um mit Arrays zu arbeiten.
namespace RosArrayService
{

    class RosArrayService_
    {
        
		
	public static string[] stringArrayAusJSonArray(JsonArray inputArray)
	{
        string[] output = new string[inputArray.Count];
        for (int i = 0; i < inputArray.Count; i++)
        {
            output[i] = inputArray[i].GetString();
        }
        return output;
    }
	public static double[] doubleArrayAusJSonArray(JsonArray inputArray)
	{
    double[] output = new double[inputArray.Count];
        for (int i = 0; i < inputArray.Count; i++)
        {
            output[i] = inputArray[i].GetNumber();
        }
        return output;      
    }

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
}
#endif