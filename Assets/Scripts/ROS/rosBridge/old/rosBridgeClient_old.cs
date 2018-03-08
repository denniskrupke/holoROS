using UnityEngine;

using RosJSON_old;
using RosMessages_old;

using System.Threading;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine.UI;


#if WINDOWS_UWP    
using System.Threading.Tasks;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using System.Collections.Concurrent;
using Windows.Data.Json;

using RosMessages;
using RosJointStateCoder;
using RosJointTrajectoryCoder;

#else
using Newtonsoft.Json;
#endif




namespace RosBridge_old
{

    class RosBridgeClient_old {        
        public static readonly string ROSBRIDGE_IP = "134.100.13.158";//"134.100.13.223";//"134.100.13.202"; //Real UR-5
        public static readonly string ROSBRIDGE_PORT = "9090";        


#if WINDOWS_UWP        
#else
        //private static WebSocket rosBridgeWebSocket = null;
#endif

        private static UTF8Encoding encoder = new UTF8Encoding();
        //private int millisSinceLastArmUpdate = Environment.TickCount;
        private static bool verbose;

        private CompressedImage_old latestImage;
        private JointState_old latestJointState;
        //private PlanningStatus_old latestPlanningStatus;

        private Vector3 latestHandPosition = new Vector3(0.0f,0.0f,0.0f);
        private Vector3 latestHandPointingDirection = new Vector3(0.0f, 1.0f, 0.0f);
        private Quaternion latestHandRotation = new Quaternion();
        private long jointCount = 0;

#if WINDOWS_UWP
        private Task asyncWorkerCommunication;
		private Task asyncWorkerMessageProcessing;
		private Task asyncWorkerCommandProcessing;
        private static MessageWebSocket webSock = null;
        private Uri serverUri = null;
        private JsonObject jsonObject;

        //CancellationTokenSource tokenSource;        
        //CancellationToken token;      
        //ConcurrentBag<Task> tasks;   


#else
        private Thread asyncWorkerCommunication;
        private Thread asyncWorkerMessageProcessing;
        private Thread asyncWorkerCommandProcessing;
#endif

        private Queue<string> rosMessageStrings;
        private Queue<RosMessage_old> rosCommandQueue;
        private bool processMessageQueue = true;
        private bool processCommandQueue = true;
        private readonly object syncObjMessageQueue = new object();
        private readonly object syncObjCommandQueue = new object();

//        private RosPublish rosPublishIncoming;
        private RosMessageConverter rosMessageConverter;

        // parameters for queue processińg frequency
//        private static readonly int minSleep = 1;
//        private static readonly int maxSleep = 200;
        private static readonly int initialSleep = 20;
        private int inputSleep = initialSleep;
        private int outputSleep = initialSleep;

        private static bool imageStreaming;
        private static bool jointStates;
        private static bool testLatency;
        private static bool handTrackingAprilTags;

        //private StreamWriter streamWriter;
        private List<string> latencyData;

        private Text debugHUDText;
        private Text statusHUDText;
        public int messageCount = 0;

        private string incomingMessageString = "";

        private int latestPlanningStatus;
        private int previousPlanningStatus;


        public int LatestPlanningStatus
        {
            get 
            {
                return latestPlanningStatus;
            }

            set
            {
                latestPlanningStatus = value;
            }
        }

        public int PreviousPlanningStatus
        {
            get 
            {
                return previousPlanningStatus;
            }

            set
            {
                previousPlanningStatus = value;
            }
        }

        public string IncomingMessageString
        {
            get
            {
                return incomingMessageString;
            }           
        }

        public Queue<string> RosMessageStrings
        {
            get
            {
                return rosMessageStrings;
            }
        }

        public RosBridgeClient_old(bool verbose, bool imageStreaming, bool jointStates, bool testLatency, Text debugText, bool handTrackingAprilTags, Text statusText) {
            RosBridgeClient_old.verbose = verbose;
            RosBridgeClient_old.imageStreaming = imageStreaming;
            RosBridgeClient_old.jointStates = jointStates;
            RosBridgeClient_old.testLatency = testLatency;
            RosBridgeClient_old.handTrackingAprilTags = handTrackingAprilTags;

            latestImage = new CompressedImage_old();                // Storage for latest incoming image message
            latestJointState = new JointState_old();               // Storage for latest incoming jointState	

            rosMessageStrings = new Queue<string>();			// Incoming message queue
            rosCommandQueue = new Queue<RosMessage_old>();          // Outgoing message queue

            rosMessageConverter = new RosMessageConverter();    // Deserializer of incoming ROSmessages

            //streamWriter = new StreamWriter("latencyData.txt");
            latencyData = new List<string>();
            this.debugHUDText = debugText;
            this.statusHUDText = statusText;

#if WINDOWS_UWP
            serverUri = new Uri("ws://" + ROSBRIDGE_IP + ":" + ROSBRIDGE_PORT);            
#endif
        }

        // Starts all Tasks/Threads
        public void Start() {
            // communication thread
            //MaybeLog("Starting communication thread...");            
            //if(verbose) this.debugHUDText.text = "\n Starting communication thread..." + this.debugHUDText.text;

#if WINDOWS_UWP
            /*
            tokenSource = new CancellationTokenSource();
            token = tokenSource.Token;           
            tasks = new ConcurrentBag<Task>();            
            asyncWorkerCommunication = Task.Factory.StartNew(() => Communicate(1,token));
            */
            asyncWorkerCommunication = Task.Factory.StartNew(() => Communicate());
            /*
            tasks.Add(asyncWorkerCommunication);
            MaybeLog("Task {0} executing " + asyncWorkerCommunication.Id);
            */
            //if (verbose) this.debugHUDText.text = "\n ...done" + this.debugHUDText.text;
#else
            asyncWorkerCommunication = new Thread(new ThreadStart(Communicate));
            asyncWorkerCommunication.Start();
#endif

            // thread for processing incoming messages
            //MaybeLog("Starting message processing thread...");           
            //if (verbose) this.debugHUDText.text = "\n Starting message processing thread..." + this.debugHUDText.text;

#if WINDOWS_UWP
            asyncWorkerMessageProcessing = Task.Factory.StartNew(() => ProcessRosMessageQueue());
            /*
             asyncWorkerMessageProcessing = Task.Factory.StartNew(() => ProcessRosMessageQueue(2,token));
             tasks.Add(asyncWorkerMessageProcessing);
             MaybeLog("Task {1} executing " + asyncWorkerMessageProcessing.Id);
            */
            //if (verbose) this.debugHUDText.text = "\n ...done" + this.debugHUDText.text;
#else
            asyncWorkerMessageProcessing = new Thread(new ThreadStart(ProcessRosMessageQueue));
            asyncWorkerMessageProcessing.Start();
#endif

            // thread for sending messages to the remote robot side
            //MaybeLog("Starting command processing thread...");            
            //if (verbose) this.debugHUDText.text = "\n Starting command processing thread..." + this.debugHUDText.text;
#if WINDOWS_UWP
            asyncWorkerCommandProcessing = Task.Factory.StartNew(() => ProcessRosCommandQueue());
            /*
             asyncWorkerCommandProcessing = Task.Factory.StartNew(() => ProcessRosCommandQueue(3,token));
             tasks.Add(asyncWorkerCommandProcessing);
             MaybeLog("Task {2} executing " + asyncWorkerCommandProcessing.Id);
            */
            //if (verbose) this.debugHUDText.text = "\n ...done" + this.debugHUDText.text;
#else
            asyncWorkerCommandProcessing = new Thread(new ThreadStart(ProcessRosCommandQueue));
            asyncWorkerCommandProcessing.Start();
#endif
        }

        // TODO: Stops all Tasks
        public void Stop() {
// #if WINDOWS_UWP
//             tokenSource.Cancel();
//             MaybeLog("Task cancellation requested.");

//             try {
//                 Task.WaitAll(tasks.ToArray());
//             }
//             catch (AggregateException e) {
//                 MaybeLog("AggregateException thrown with the following inner exceptions:");
//                 // Display information about each exception. 
//                 foreach (var v in e.InnerExceptions) {
//                   if (v is TaskCanceledException)
//                      MaybeLog("   TaskCanceledException: Task {0} " + 
//                                        ((TaskCanceledException) v).Task.Id);
//                   else
//                      MaybeLog("   Exception: {0} " + v.GetType().Name);
//                 }            
//             }
//             finally
//             {
//                tokenSource.Dispose();
//             }

//             // Display status of all tasks. 
//             foreach (var task in tasks)
//                 MaybeLog("Task {0} status is now {1} " + task.Id +" "+ task.Status);
// #else
//             //TODO implemetation for normal windows usage
// #endif
        }

#if WINDOWS_UWP
        // connects with the ROSbridge server
        public async void Connect()
        {
            MaybeLog ("Connect...");
            //this.debugHUDText.text += "\n";
            //this.debugHUDText.text += "Connecting...";
            webSock = new MessageWebSocket();
            webSock.Control.MessageType = SocketMessageType.Utf8;

            //Add the MessageReceived event handler.
            if (verbose) this.debugHUDText.text = "\n Register Incoming Message Handler..." + this.debugHUDText.text;
            webSock.MessageReceived += WebSock_MessageReceived;            

            //Add the Closed event handler.
            webSock.Closed += WebSock_Closed;

            try
            {
                //Connect to the server.
                await webSock.ConnectAsync(serverUri);               
            }
            catch (Exception ex)
            {
                this.debugHUDText.text = ex.Message;               
            }

        }

        //The MessageReceived event handler. DON'T ACCESS CANVAS!!!!
        private void WebSock_MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
        {           
            DataReader messageReader = args.GetDataReader();
            messageReader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
            messageReader.ByteOrder = ByteOrder.LittleEndian;
            incomingMessageString = messageReader.ReadString(messageReader.UnconsumedBufferLength);
            ReceiveAndEnqueue(incomingMessageString);                                
        }

        //The Closed event handler
        private void WebSock_Closed(IWebSocket sender, WebSocketClosedEventArgs args)
        {
            //Add code here to do something when the connection is closed locally or by the server
        }
#else
        // connects with the ROSbridge server
        private void Connect(string uri) {
            /*      
                  try
                  {
                      rosBridgeWebSocket = new WebSocket(uri);
                      rosBridgeWebSocket.OnMessage += ReceiveAndEnqueue;

                      int count = 1;
                      do{
                          MaybeLog("try connecting "+count++);
                          rosBridgeWebSocket.Connect();
                          Thread.Sleep(1000);
                      }
                      while(!rosBridgeWebSocket.IsConnected);        
                  }
                  catch (Exception ex)
                  {
                      Debug.Log("Exception: {Connect}"+ex.ToString());
                  }
                  finally{}
             */
        }
#endif

        // closes the websocket connection to the ROSbridge server
        public void Disconnect() {
#if WINDOWS_UWP
            try 
	    	{
                webSock.Close(0,"");
            }
            catch (System.Exception e) 
	    	{
				Debug.Log("Exception: {Disconnect}"+e.ToString());
	    	} 
	    	finally {}
#endif
        }

        // checks for valid Websocket connection
        public bool IsConnected() {
            bool connected = false;
#if WINDOWS_UWP
			connected = (webSock != null);
#endif
            return connected;
        }

#if WINDOWS_UWP
       // works 
        private async void Send(RosMessage_old message){
			MaybeLog("Send ROSmessage...");                
			DataWriter messageWriter = new DataWriter(webSock.OutputStream);                
            string messageString = CreateRosMessageString(message);                
            messageWriter.WriteString(messageString);                
            await messageWriter.StoreAsync();                
            await messageWriter.FlushAsync();                
            messageWriter.DetachStream();                
        }
#else
        private void Send(RosMessage_old message) {
            MaybeLog("Send ROSmessage...");            
            byte[] buffer = encoder.GetBytes(CreateRosMessageString(message));
            /*
			rosBridgeWebSocket.Send(buffer);
            */
        }
#endif

        private void ReceiveAndEnqueue(string message) {            
            MaybeLog("Receive...");
            if (!string.IsNullOrEmpty(message)) {          
                lock (syncObjMessageQueue) {                                        
                    this.rosMessageStrings.Enqueue(message);                    
                    MaybeLog("numberOfMessagesInQueue: " + rosMessageStrings.Count());
                }
            }
        }

        public void EnqueRosCommand(RosMessage_old message) {            
            //if (verbose) this.debugHUDText.text = "\n Try to get token..." + this.debugHUDText.text;
           // lock (syncObjCommandQueue) {
                //if (verbose) this.debugHUDText.text = "\n success" + this.debugHUDText.text;
                this.rosCommandQueue.Enqueue(message);
                //if (verbose) this.debugHUDText.text = "\n Enqueued message succesfully" + this.debugHUDText.text;
                //MaybeLog("" + rosCommandQueue.Count());
         //   }
        }


#if WINDOWS_UWP
                //private void Communicate(int taskNum, CancellationToken ct){ 
                public void Communicate(){                                        
                   // if (ct.IsCancellationRequested) {
                    //    MaybeLog("Task {"+ (taskNum-1) +"} cancelled");
                     //   ct.ThrowIfCancellationRequested();
                    //}

                    if (!IsConnected())
                    {
                        MaybeLog("Try to connect with ROSbridge..."); 
                        Connect();                        
                        //Task.Delay(2000).Wait();                                            
                    }                                        
                    else {
                        MaybeLog ("...connect done!");                        
                    }                    
                }

#else
        // starts the communication: connects and subscribes to topics
        public void Communicate()
        {            
            if (!IsConnected())
            {
                MaybeLog("Try to connect with ROSbridge...");
                Connect("ws://" + ROSBRIDGE_IP + ":" + ROSBRIDGE_PORT);
                Thread.Sleep(2000);
            }
            MaybeLog("...connect done!");

            if (IsConnected())
            {
                if (RosBridgeClient_old.imageStreaming)
                {
                    MaybeLog("Subscribing to /camera/rgb/image_rect_color/compressed");
                    Send(new RosSubscribe_old("/camera/rgb/image_rect_color/compressed", "sensor_msgs/CompressedImage"));
                }
                if (RosBridgeClient_old.jointStates)
                {
                    MaybeLog("Subscribing to /joint_states");
                    Send(new RosSubscribe_old("/robot/joint_states", "sensor_msgs/JointState"));                    
                }                
            }
            else
            {
                Communicate();
            }
        }
#endif

        
    public void SubscribeToTopics(){
#if WINDOWS_UWP        
        if(RosBridgeClient_old.imageStreaming) {
            MaybeLog ("Subscribing to /camera/rgb/image_rect_color/compressed");            
            Send(new RosSubscribe("/camera/rgb/image_rect_color/compressed", "sensor_msgs/CompressedImage"));
        }
        if (RosBridgeClient_old.jointStates) {
            // Send(new RosSubscribe_old("/preview_publisher", "sensor_msgs/JointState", 100)); //the minimum amount of time (in ms) that must elapse between messages being sent. Defaults to 0
            // Send(new RosSubscribe_old("/robot/joint_states", "sensor_msgs/JointState", 100)); //the minimum amount of time (in ms) that must elapse between messages being sent. Defaults to 0
            Send(new RosSubscribe_old("/hololens/planned_joint_states", "sensor_msgs/JointState", 100)); //the minimum amount of time (in ms) that must elapse between messages being sent. Defaults to 0                        
        }
        // if (RosBridgeClient_old.handTrackingAprilTags) {        
            // Send(new RosSubscribe_old("/handDirectionPointer", "std_msgs/Float32MultiArray"));            
        // }            
            Send(new RosSubscribe_old("/hololens/state", "std_msgs/Int32", 1)); //the minimum amount of time (in ms) that must elapse between messages being sent. Defaults to 0        
#endif
        }



        // #if WINDOWS_UWP
        //         private void ProcessRosCommandQueue(int taskNum, CancellationToken ct)
        //         {
        //             while (processCommandQueue)
        //             {
        //                 if (this.rosCommandQueue.Count > 0)
        //                 {
        //                     lock (syncObjCommandQueue)
        //                     {
        //                         Send(rosCommandQueue.Dequeue());
        //                     }
        //                 }
        //                 System.Threading.Tasks.Task.Delay(30).Wait();
        //                 if (ct.IsCancellationRequested) {
        //                     MaybeLog("Task {"+ (taskNum-1) +"} cancelled");
        //                     ct.ThrowIfCancellationRequested();
        //                 }
        //             }
        //         }
        // #else
        public void ProcessRosCommandQueue()
        {
            while (processCommandQueue)
            {
                if (this.rosCommandQueue.Count > 0)
                {                   
                    lock (syncObjCommandQueue)
                    {
                        Send(rosCommandQueue.Dequeue());
                    }
                }
#if WINDOWS_UWP
               // System.Threading.Tasks.Task.Delay(30).Wait();
#endif
                /*
#if WINDOWS_UWP
                                System.Threading.Tasks.Task.Delay(30).Wait();
#else
                                Thread.Sleep(30);
#endif
                */
            }
        }
// #endif



// #if WINDOWS_UWP
//         private void ProcessRosMessageQueue(int taskNum, CancellationToken ct)
//         {
//             while (true)
//             {
//                 if (this.rosMessageStrings.Count() > 0)
//                 {                    
//                     lock (syncObjMessageQueue)
//                     {                     
//                         DeserializeJSONstring(rosMessageStrings.Dequeue());
//                     }
//                 }               
//                 System.Threading.Tasks.Task.Delay(30).Wait();
//                 if (ct.IsCancellationRequested) {
//                     MaybeLog("Task {"+ (taskNum-1) +"} cancelled");
//                     ct.ThrowIfCancellationRequested();
//                 }
//             }
//         }
// #else
        private void ProcessRosMessageQueue()
        {
           while (processMessageQueue)                
            {         
                MaybeLog ("ProcessRosMessageQueue...");                
                if (this.rosMessageStrings.Count() > 0)
                 {                    
                    MaybeLog ("Try to dequeue...");
                    //lock (syncObjMessageQueue) // TODO check if the lock is needed
                    //{                    
                    this.DeserializeJSONstringHard(rosMessageStrings.Dequeue());                    
#if WINDOWS_UWP
                        //                        TODO: mertke
                        //this.latestJointState = (JointState_old)this.DeserializeJSONstring(rosMessageStrings.Dequeue());
#endif
                    //}
                }

#if WINDOWS_UWP
                    //System.Threading.Tasks.Task.Delay(2).Wait();
#else
                Thread.Sleep(30);
#endif
            }
        }


#if WINDOWS_UWP
        // serializes a RosMessage to a string
        // TODO: generic
        public string CreateRosMessageString(RosMessage_old msg){
            string messageString = "";
            if (msg.op == "subscribe") {
                Maybelog("CreateRosMessageStrings...");
                messageString = "{ \"op\": \"";
                messageString += msg.op;
                messageString += "\", ";
                messageString += "\"topic\": \"";
                messageString += msg.topic;
                messageString += "\", ";
                messageString += "\"throttle_rate\": ";
                messageString += ((RosSubscribe_old)msg).throttle_rate;
                messageString += "}";
            }
            else if (msg.op == "publish") {
                if(msg.topic == "/hololens/plan_pick"){
                    
                    messageString = "{ \"op\": \"";
                        messageString += msg.op;
                        messageString += "\", ";
                        messageString += "\"topic\": \"";
                        messageString += msg.topic;
                        messageString += "\", ";
                        messageString += "\"msg\": ";
                        messageString += "{";
                        RosMessages_old.geometry_msgs.PointStamped_old ps = (RosMessages_old.geometry_msgs.PointStamped_old)((RosPublish_old)msg).msg;
                            
                            messageString += "\"header\": ";
                            messageString += "{";                        
                                messageString += "\"stamp\": ";
                                messageString += "{";
                                    messageString += "\"secs\": ";
                    messageString += 0;
                                    messageString += " ,";
                                    messageString += "\"nsecs\": ";
                    messageString += 0;
                                messageString += "}";
                                messageString += " ,";
                                messageString += "\"frame_id\": ";
                                messageString += "\"" + ps.header.frame_id + "\"";
                                messageString += " ,";
                                messageString += "\"seq\": ";
                                messageString += ps.header.seq;                        
                            messageString += "}";
                        messageString += " ,";

                        messageString += "\"point\": ";
                            messageString += "{";
                                messageString += "\"x\": ";
                                messageString += ps.point.x;
                                messageString += " ,";
                                messageString += "\"y\": ";
                                messageString += ps.point.y;
                                messageString += " ,";
                                messageString += "\"z\": ";
                                messageString += ps.point.z;                                
                            messageString += "}";
                        messageString += "}";                                                                                     
                    messageString += "}";
                }
                else if (msg.topic == "/hololens/plan_place")
                {                    
                    messageString = "{ \"op\": \"";
                    messageString += msg.op;
                    messageString += "\", ";
                    messageString += "\"topic\": \"";
                    messageString += msg.topic;
                    messageString += "\", ";
                    messageString += "\"msg\": ";
                    messageString += "{";
                    RosMessages_old.geometry_msgs.PointStamped_old ps = (RosMessages_old.geometry_msgs.PointStamped_old)((RosPublish_old)msg).msg;
                    
                    messageString += "\"header\": ";
                    messageString += "{";
                    messageString += "\"stamp\": ";
                    messageString += "{";
                    messageString += "\"secs\": ";
                    messageString += 0;
                    messageString += " ,";
                    messageString += "\"nsecs\": ";
                    messageString += 0;
                    messageString += "}";                   
                    messageString += " ,";
                    messageString += "\"seq\": ";
                    messageString += ps.header.seq;
                    messageString += "}";
                    messageString += " ,";

                    messageString += "\"point\": ";
                    messageString += "{";
                    messageString += "\"x\": ";
                    messageString += ps.point.x;
                    messageString += " ,";
                    messageString += "\"y\": ";
                    messageString += ps.point.y;
                    messageString += " ,";
                    messageString += "\"z\": ";
                    messageString += ps.point.z;
                    messageString += "}";
                    messageString += "}";
                    messageString += "}";
                }
                else if(msg.topic == "/hololens/execute_pick"){                    
                    messageString = "{ \"op\": \"";
                        messageString += msg.op;
                        messageString += "\", ";
                        messageString += "\"topic\": \"";
                        messageString += msg.topic;
                        messageString += "\", ";
                        messageString += "\"msg\": ";
                        messageString += "{";
                        messageString += "}";
                    messageString += "}";
                }
                else if (msg.topic == "/hololens/execute_place")
                {                    
                    messageString = "{ \"op\": \"";
                    messageString += msg.op;
                    messageString += "\", ";
                    messageString += "\"topic\": \"";
                    messageString += msg.topic;
                    messageString += "\", ";
                    messageString += "\"msg\": ";
                    messageString += "{";
                    messageString += "}";
                    messageString += "}";
                }
                else if(msg.topic == "/hololens_open_gripper"){                    
                    messageString = "{ \"op\": \"";
                        messageString += msg.op;
                        messageString += "\", ";
                        messageString += "\"topic\": \"";
                        messageString += msg.topic;
                        messageString += "\", ";
                        messageString += "\"msg\": ";
                        messageString += "{";
                        messageString += "}";
                    messageString += "}";
                }                
            }

            return messageString;
       }
#else
        // serializes a RosMessage to a string
        public string CreateRosMessageString(RosMessage_old msg)
        {           
            return JsonConvert.SerializeObject(msg);
        }
#endif
        

       //--------------------------------------------------------------------------------------------------------------
       // TODO: extending this for more messages
       public void DeserializeJSONstringHard(string message){
            //this.statusHUDText.text = "" + jointCount++;
            //if (verbose) this.debugHUDText.text = "\n processing string..." + this.debugHUDText.text;
            //MaybeLog ("Try to deserialize: " + message);

#if WINDOWS_UWP

            // if (message.Contains("joint_states") || message.Contains("preview_publisher")) {
            if (message.Contains("hololens/planned_joint_states")) {                
                jsonObject = JsonObject.Parse(message);
            
                JsonArray jnarray = jsonObject["msg"].GetObject()["name"].GetArray();                
                JsonArray jparray = jsonObject["msg"].GetObject()["position"].GetArray();

                string[] names = new string[jnarray.Count];                
                double[] positions = new double[jparray.Count];

                for (int i = 0; i < jnarray.Count; i++)
                {
                    names[i] = jnarray[i].GetString();
                }
                for (int i = 0; i < jparray.Count; i++)
                {
                    positions[i] = jparray[i].GetNumber();
                }                

                latestJointState.name = names;
                latestJointState.position = positions;
            }
            else if (message.Contains("hololens/state")){
                jsonObject = JsonObject.Parse(message);

                previousPlanningStatus = latestPlanningStatus;
                latestPlanningStatus = (int) jsonObject["msg"].GetObject()["data"].GetNumber();

                //TODO show planning results in a temporary message
            }
            /* TODO
            else if (message.Contains("planned_successful")) {
                bool success = jsonObject["msg"].GetObject()["data"].GetBoolean();
                jsonObject = JsonObject.Parse(message);
               // this.statusHUDText.text = success ? "Success" : "Fail";                
            }*/
            // else if (message.Contains("handDirectionPointer")) {
            //     jsonObject = JsonObject.Parse(message);
            //     JsonArray jdarray = jsonObject["msg"].GetObject()["data"].GetArray();
            //     this.latestHandPosition.x = (float)jdarray[0].GetNumber();
            //     this.latestHandPosition.y = (float)jdarray[1].GetNumber();
            //     this.latestHandPosition.z = (float)jdarray[2].GetNumber();
            //     //this.debugHUDText.text = "\n marker position" + this.latestHandPosition.x + " " + this.latestHandPosition.y + " " + this.latestHandPosition.z + " " + this.debugHUDText.text;
            //     this.latestHandPointingDirection.x = (float)jdarray[3].GetNumber();
            //     this.latestHandPointingDirection.y = (float)jdarray[4].GetNumber();
            //     this.latestHandPointingDirection.z = (float)jdarray[5].GetNumber();

            //     /*
            //     this.latestHandRotation.x = (float)jdarray[6].GetNumber();
            //     this.latestHandRotation.y = (float)jdarray[7].GetNumber();
            //     this.latestHandRotation.z = (float)jdarray[8].GetNumber();
            //     this.latestHandRotation.w = (float)jdarray[9].GetNumber();
            //     */

            //     //Vector3 position = new Vector3((float)jdarray[0].GetNumber(), (float)jdarray[1].GetNumber(), (float)jdarray[2].GetNumber());
            //     //Vector3 direction = new Vector3((float)jdarray[3].GetNumber(), (float)jdarray[4].GetNumber(), (float)jdarray[5].GetNumber());

            // }
#endif
        }

#if WINDOWS_UWP
        // hier wird einfach hart-gecoded eine JointState-Nachricht deserialisiert
        public RosMessage DeserializeJSONstring(string message)
        {

            JsonObject jsonObject = JsonObject.Parse(message);
            string jtopic = jsonObject["topic"].GetString();
            rosComplexCoder coder = null;
            switch (jtopic)
            {
                case "/joint_states":
                    coder = new RosJointStateCoder_();
                    return coder.startDeserializing(jsonObject);

                case "/preview_trajectory":
                    coder = new RosJointTrajectoryCoder_();
                    return coder.startDeserializing(jsonObject);

                default:
                    return null;
            }
        }

        
        // die andere Richtung wäre auch schön, damit man auch Nachrichten an ROS senden kann
        public string SerializeROSmessage(RosMessage message)
        {
            //todo

            if (message.GetType() == typeof(RosPublish))
            {
                rosComplexCoder coder = null;
                switch (message.topic)
                {
                    case "\"/joint_states\"":
                        coder = new RosJointStateCoder_();
                        return coder.startSerializing(message);

                    case "\"/preview_trajectory\"":
                        coder = new RosJointTrajectoryCoder_();
                        return coder.startSerializing(message);

                    default:
                        return null;
                }
            }
            else
            {
                return null;
            }
        }
#endif

        public CompressedImage_old GetLatestImage(){
			return latestImage;
		}

		public JointState_old GetLatestJoinState (){
			return latestJointState;
		}

        public Vector3 GetLatestHandPosition()
        {
            return this.latestHandPosition;
        }

        public Vector3 GetLatestPointingDirection()
        {
            return this.latestHandPointingDirection;
        }

        public Quaternion GetLatestHandRotation()
        {
            return this.latestHandRotation;
        }

        // optional log function, logs only if 'verbose' is set to true
        public void MaybeLog(string logstring)
        {
            if (verbose)
            {
                #if WINDOWS_UWP
                this.debugHUDText.text = "\n" + logstring + this.debugHUDText.text;
                #else
                Debug.Log(logstring);
                #endif
            }
        }

    }
}
