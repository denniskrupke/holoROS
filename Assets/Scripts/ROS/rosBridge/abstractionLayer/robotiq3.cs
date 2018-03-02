using RosMessages_old;
using System;

namespace AbstractionLayer{
	/*
		Abstraction level to generate proper control messages for the Robotiq 3-finger adaptive gripper°
	*/

	public class JointStateGenerator{		
		public static JointState_old emptyJointState(){
            JointState_old state =  new JointState_old();
			state.effort = new double[]{5.34, 1.345, 2.4543, 2.3432, 0.45367};
			state.position = new double[]{5.34, 1.345, 2.4543, 2.3432, 0.45367};
			state.velocity = new double[]{5.34, 1.345, 2.4543, 2.3432, 0.45367};
			state.name = new string[]{ "Heinz", "Peter", "Alfons", "Dieter", "Karl" };

			return state;
		}
	}

	public class HandControlMessageGenerator{

		public static OutputMessageData_old initHand(){
            OutputMessageData_old command = new OutputMessageData_old();
			command.rACT = 1;
			command.rGTO = 1;
			command.rSPA = 255;
			command.rFRA = 150;

			return command;
		}

		public static OutputMessageData_old openHand(){
			return openHand(1.0f);
		}

		public static OutputMessageData_old openHand(float openingState=1.0f){
            OutputMessageData_old command = new OutputMessageData_old();
			command.rACT = 1;
			command.rGTO = 1;
			command.rSPA = 255;
			command.rFRA = 150;
			command.rPRA = (uint) (255*(1.0f-openingState));

			return command;
		}

		public static OutputMessageData_old closeHand(){
			return closeHand(1.0f);
		}

		public static OutputMessageData_old closeHand(float closingState=1.0f){
            OutputMessageData_old command = new OutputMessageData_old();
			command.rACT = 1;
			command.rGTO = 1;
			command.rSPA = 255;
			command.rFRA = 150;
			command.rPRA = (uint) (255*closingState);

			return command;
		}

		
	}

}