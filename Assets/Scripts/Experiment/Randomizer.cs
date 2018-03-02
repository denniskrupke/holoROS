using UnityEngine;
using UnityEngine.UI;


public class TrialMemory_Cylinder{
	public int count = 0; //how often was it already used	
	public bool[] angles; //indicate which angle was already used
}

public class Trial{
	public int method; // which control method
	public int cylinder; // index of the cylinder in the object array
	public int angle; // value of the angle
}


public class Randomizer : MonoBehaviour{
	public int[] methods; // e.g. index of different input methods: gaze and pointing
	public int[] cylinders; // contains all cylinder names 0,1,2,3,4,5
	public int[] angles; // contains all angles 0,30,120

	private int[] currentMethods;
	private int[] currentCylinders;
	private int[] currentAngles;

    private int currentMethodIndex = 0;
	private int currentCylinderIndex = -1;
    private int currentCylinderIndex_old;
    private int currentAngleIndex;

	private TrialMemory_Cylinder[] memory;
	private bool done = false;

    public Text debugHUD = null;

    public bool IsDone() { return this.done; }
    public bool secondRoundRunning = false;

    void Start () {
		currentMethods = (int[]) methods.Clone();
		currentCylinders = (int[]) cylinders.Clone();
		currentAngles = (int[]) angles.Clone();		

		// init memory
		InitMemory();
	}

    int FindLessUsedCylinder(int referenceIndex)
    {       
        for(int i=0; i<currentCylinders.Length; i++)
        {
            if(memory[currentCylinders[referenceIndex]].count > memory[currentCylinders[i]].count) // wenn der aktuelle Zylinder häufiger dran war als ein anderer verfügbarer
            {                
                return i;
            }
        }
        return referenceIndex;
    }

    private bool ContainsMoreThanOneCylinder(int old)
    {
        bool sth = false;        

        for(int i=0; i<currentCylinders.Length; i++)
        {            
            if (currentCylinders[old] != i) sth = true;
        }

        return sth;
    }


    public Trial NextTrial() {
        Trial trial = new Trial();

        trial.method = currentMethods[currentMethodIndex];

        // determine available cylinder        
        currentCylinderIndex_old = currentCylinderIndex;        

        currentCylinderIndex = Random.Range(0, currentCylinders.Length - 1); //which cylinder by index    
        /*if (currentCylinders.Length > 1)
        {
            while (currentCylinderIndex_old == currentCylinderIndex)
            {
                currentCylinderIndex = Random.Range(0, currentCylinders.Length - 1); //which cylinder by index      
            }
        }

        currentCylinderIndex = Random.Range(0, currentCylinders.Length - 1); //which cylinder by index      
        */

        //currentCylinderIndex = FindLessUsedCylinder(currentCylinderIndex); // this is for balancing but doesn't work
        /*
        bool search = true;
        while ((currentCylinderIndex_old == currentCylinderIndex) && search)
            {
            if (!ContainsOnlyOneCylinder(currentCylinderIndex_old))
            {
                currentCylinderIndex = Random.Range(0, currentCylinders.Length - 1); //which cylinder by index        
            }
            else
            {
                currentCylinderIndex = Random.Range(0, currentCylinders.Length - 1); //which cylinder by index        
                //currentCylinderIndex = FindLessUsedCylinder(currentCylinderIndex); // this is for balancing but doesn't work
                search = false;
            }
        }
          */
        //}       

        memory[currentCylinders[currentCylinderIndex]].count++; //remember its usage        
        trial.cylinder = currentCylinders[currentCylinderIndex];// activate this cylinder for the current trial
                


        //load remaining set of angles        
        TrialMemory_Cylinder tmc = memory[currentCylinders[currentCylinderIndex]]; // current memory object
        int countUnusedAngles = 0;  //init
        for (int i=0; i<tmc.angles.Length; i++)
        {
            if (!tmc.angles[i]) countUnusedAngles++;//count the unused
        }
       
        //debugHUD.text = "\n " + "cylinder " + currentCylinders[currentCylinderIndex] + "has " + countUnusedAngles + "unused angles" + debugHUD.text;
        int[] selectionAngles = new int[countUnusedAngles]; // new array for indices with number of unused angles

        
        int count = 0;  //init
        for (int i = 0; i < tmc.angles.Length; i++)
        {
            if (!tmc.angles[i])
            {
                selectionAngles[count] = i; //copy unused indices
                count++;
            }
        }      
        

        // determine available angle
        currentAngleIndex = Random.Range(0, selectionAngles.Length-1); //which angle by index

        memory[currentCylinders[currentCylinderIndex]].angles[selectionAngles[currentAngleIndex]] = true; // remember the use of the angle		
        trial.angle = angles[selectionAngles[currentAngleIndex]]; //activate selected angle for current trial        
        trial.angle = angles[Random.Range(0, angles.Length - 1)];

        
        // if all angles are used, remove cylinder from choice for the next round
        if (!AngleIsLeft()) {
        //    debugHUD.text = "\n " + "no angle is left for cylinder:" + currentCylinders[currentCylinderIndex] + debugHUD.text;
            RemoveCylinderFromChoice(currentCylinderIndex);
        }
        
                
        return trial;
	}

    private bool AngleIsLeft()
    {
        bool result = false;
        bool[] angleArray = memory[currentCylinders[currentCylinderIndex]].angles;
        for (int i = 0; i < angleArray.Length; i++)
        {
            if (!angleArray[i])
            {
                result = true; 
            }
        }
        return result;
    }

    private void RemoveCylinderFromChoice(int index){
		if(currentCylinders.Length == 1) {this.done=true;}//done...it was the last cylinder
		else {
            int[] oldCylinders = (int[])currentCylinders.Clone();   //remember remaining cylinders
            currentCylinders = new int[currentCylinders.Length-1]; //to remove cylinder create new smaller array
			int offset = 0;
			for(int i=0; i<currentCylinders.Length; i++){//iterate
				if(index==i){//take next if the one to remove is reached
					offset++; 
				}
				currentCylinders[i] = oldCylinders[i+offset];//copy value
			}
		}
	}	

	private void InitMemory(){
        memory = new TrialMemory_Cylinder[cylinders.Length];
		for(int i=0; i<memory.Length; i++){
			memory[i] = new TrialMemory_Cylinder();
			memory[i].count = 0;
			memory[i].angles = new bool[angles.Length];
			for(int k=0; k<angles.Length; k++){
				memory[i].angles[k] = false;
			}
		}
	}

    // TODO
	public void NextMethod(){
        this.done = false;
		InitMemory();
        currentMethodIndex = 1; 
		currentCylinders = (int[])cylinders.Clone();
		currentAngles = (int[])angles.Clone();
        this.secondRoundRunning = true;
	}

}