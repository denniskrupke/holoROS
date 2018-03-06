using System.Collections.Generic;

namespace Config
{

/*
    Diese Klasse beinhaltet einige Einstellungen, vor allem die Zuweisung der Topics auf die Coderklassen.        
*/
class parserConfig
{
    private static List<string> _jointStateTopics = new List<string>{ "/ur5_joint_states", "\"/ur5_joint_states\"" };

    private static List<string> _jointTrajectoryTopics = new List<string>{ "/preview_trajectory", "\"/preview_trajectory\"" };

    public static List<string> getJointStateTopics()
    {
        return _jointStateTopics;
    }

    public static List<string> getJointTrajectoryTopics()
    {
        return _jointTrajectoryTopics;
    }
}
}