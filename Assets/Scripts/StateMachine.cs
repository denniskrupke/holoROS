
namespace RobotAR {

    // states are ordered
    public enum State { Pick, Place };

    public static class StateMachine {

        private const State start = State.Pick;
        private static bool changed = false;
        private static State currentState = start;

        public static State GetCurrentState()
        {
            return currentState;
        }

        public static void NextState()
        {
            long nextStateIndex = ((long)currentState + 1) % System.Enum.GetNames(typeof(State)).Length;
            currentState = (State)nextStateIndex;
            changed = true;
        }

        public static bool Changed()
        {
            return changed;
        }

        public static void Reset()
        {
            changed = false;
        }

    }

}
