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
using Config;

#else
using Newtonsoft.Json;
#endif




namespace RosBridge_old
{
    /* TODO:
		- automatic reconnect
		- latest frame buffers (robot state, image, point cloud, ...)
	*/
    class RosBridgeClient_old {
        //public static readonly string ROSBRIDGE_IP = "134.100.13.189";//"134.100.13.202";//203";
        public static readonly string ROSBRIDGE_IP = "134.100.13.223";//"134.100.13.158";//"134.100.13.223";//"134.100.13.202"; //Real UR-5
        //public static readonly string ROSBRIDGE_IP = "134.100.13.121";//"134.100.13.202";// Virtual Robot Model
        //public static readonly string ROSBRIDGE_PORT = "8080";
        public static readonly string ROSBRIDGE_PORT = "9090";

        //public string SomeString { get; private set; }


#if WINDOWS_UWP        
        private JointState latestJointState;
#else
        //private static WebSocket rosBridgeWebSocket = null;
#endif

        private static UTF8Encoding encoder = new UTF8Encoding();
        //private int millisSinceLastArmUpdate = Environment.TickCount;
        private static bool verbose;

        private CompressedImage_old latestImage;
        
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
            //latestJointState = new JointState_old();               // Storage for latest incoming jointState	
#if WINDOWS_UWP
            latestJointState = new JointState();               // Storage for latest incoming jointState	
#endif
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
           //this.messageCount++;
           DataReader messageReader = args.GetDataReader();
           messageReader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
           messageReader.ByteOrder = ByteOrder.LittleEndian;
           incomingMessageString = messageReader.ReadString(messageReader.UnconsumedBufferLength);
            //  this.debugHUDText.text += "\n";
            //this.debugHUDText.text = "\n Received..." + messageString + this.debugHUDText.text;
            //Debug.Log("ReceivedString://"+messageString+"//");
            ReceiveAndEnqueue(incomingMessageString);
            //Add code here to do something with the string that is received.
                    
        }

        //The Closed event handler
        private void WebSock_Closed(IWebSocket sender, WebSocketClosedEventArgs args)
        {
            //Add code here to do something when the connection is closed locally or by the server
        }
/*
        //Send a message to the server.
        private async Task WebSock_SendMessage(MessageWebSocket webSock, string message)
        {
            //this.debugHUDText.text += "\n";
            //this.debugHUDText.text += "Sending..."+message;
            DataWriter messageWriter = new DataWriter(webSock.OutputStream);
            //messageWriter.WriteString(message);
            await messageWriter.StoreAsync();
        }
*/
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
                if (verbose) this.debugHUDText.text = "\n Send ROSmessage..." + this.debugHUDText.text;            		         
			DataWriter messageWriter = new DataWriter(webSock.OutputStream);
                if (verbose) this.debugHUDText.text = "\n Created dataWriter" + this.debugHUDText.text;
            string messageString = CreateRosMessageString(message);
                if (verbose) this.debugHUDText.text = "\n Message: " + messageString + " " + this.debugHUDText.text;
            messageWriter.WriteString(messageString);
                if (verbose) this.debugHUDText.text = "\n Message written" + this.debugHUDText.text;
                if (verbose) this.debugHUDText.text = "\n storeAsync" + this.debugHUDText.text;
            await messageWriter.StoreAsync();
                if (verbose) this.debugHUDText.text = "\n flushAsync" + this.debugHUDText.text;
            await messageWriter.FlushAsync();
                if (verbose) this.debugHUDText.text = "\n detach stream" + this.debugHUDText.text;
            messageWriter.DetachStream();
                if (verbose) this.debugHUDText.text = "\n done" + this.debugHUDText.text;                     
        }
#else
        private void Send(RosMessage_old message) {
            MaybeLog("Send ROSmessage...");
            //Debug.Log (CreateRosMessageString (message));
            byte[] buffer = encoder.GetBytes(CreateRosMessageString(message));
            /*
			rosBridgeWebSocket.Send(buffer);
            */
        }
#endif

        private void ReceiveAndEnqueue(string message) {            
            //MaybeLog("Receive...");
            if (!string.IsNullOrEmpty(message)) {          
                lock (syncObjMessageQueue) {
                    //this.statusHUDText.text = "A:";
                    //messageCount++;
                    this.rosMessageStrings.Enqueue(message);
                    //this.messageCount++;
                    //this.debugHUDText.text = "\n" + message + this.debugHUDText.text;
                    //MaybeLog("numberOfMessagesInQueue: " + rosMessageStrings.Count());
                }
            }
        }

        public void EnqueRosCommand(RosMessage_old message) {
            //this.statusHUDText.text = "" + "enqueue";
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
                    //if (verbose) this.debugHUDText.text = "\n COMMUNICATE" + this.debugHUDText.text; 
                    //MaybeLog ("--- inside communicate ---");

                   // if (ct.IsCancellationRequested) {
                    //    MaybeLog("Task {"+ (taskNum-1) +"} cancelled");
                     //   ct.ThrowIfCancellationRequested();
                    //}

                    if (!IsConnected())
                    {
                        MaybeLog("Try to connect with ROSbridge..."); 
                        Connect();
                        if (verbose) this.debugHUDText.text = "\n is not connected" + this.debugHUDText.text;
                        //Task.Delay(2000).Wait();                                            
                    }                                        
                    else {
                        MaybeLog ("...connect done!");
                        if (verbose) this.debugHUDText.text = "\n is connected" + this.debugHUDText.text;                        
                    }                    
                }

#else
        // starts the communication: connects and subscribes to topics
        public void Communicate()
        {

            MaybeLog("--- inside communicate ---");
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
        //this.debugHUDText.text = "\n Subscribing..." + this.debugHUDText.text;
        if(RosBridgeClient_old.imageStreaming) {
            MaybeLog ("Subscribing to /camera/rgb/image_rect_color/compressed");
            if (verbose) this.debugHUDText.text = "\n Subscribing to /camera/rgb/image_rect_color/compressed" + this.debugHUDText.text;            
            //Send(new RosSubscribe("/camera/rgb/image_rect_color/compressed", "sensor_msgs/CompressedImage"));
        }
        //if (RosBridgeClient_old.jointStates) {
            // MaybeLog ("Subscribing to /joint_states");
            // if (verbose) this.debugHUDText.text = "\n Subscribing to /joint_states" + this.debugHUDText.text;
            //RosSubscribe rosSub = new RosSubscribe ("/joint_states", "sensor_msgs/JointState");
            //if (verbose) this.debugHUDText.text = "\n Created subscribe message" + this.debugHUDText.text;
            //this.EnqueRosCommand(rosSub);
            //Send(new RosSubscribe("/joint_states", "sensor_msgs/JointState", 200));
            //Send(new RosSubscribe("/joint_states", "sensor_msgs/JointState", 100)); //the minimum amount of time (in ms) that must elapse between messages being sent. Defaults to 0
            //Send(new RosSubscribe_old("/preview_publisher", "sensor_msgs/JointState", 100)); //the minimum amount of time (in ms) that must elapse between messages being sent. Defaults to 0
            // Send(new RosSubscribe_old("/robot/joint_states", "sensor_msgs/JointState", 100)); //the minimum amount of time (in ms) that must elapse between messages being sent. Defaults to 0
            //Send(new RosSubscribe_old("/planned_joint_states", "sensor_msgs/JointState", 100)); //the minimum amount of time (in ms) that must elapse between messages being sent. Defaults to 0
            Send(new RosSubscribe_old("/ur5_joint_states", "sensor_msgs/JointState", 100)); //the minimum amount of time (in ms) that must elapse between messages being sent. Defaults to 0
            // if (verbose) this.debugHUDText.text = "\n Send subscribe message" + this.debugHUDText.text;
            //this.debugHUDText.text = "\n Enqueued subscribe message" + this.debugHUDText.text;                
        //}
        // if (RosBridgeClient_old.handTrackingAprilTags) {
            // MaybeLog("Subscribing to /handDirectionPointer");
            // if (verbose) this.debugHUDText.text = "\n Subscribing to /handDirectionPointer" + this.debugHUDText.text;
            // Send(new RosSubscribe_old("/handDirectionPointer", "std_msgs/Float32MultiArray"));
            // if (verbose) this.debugHUDText.text = "\n Send subscribe message" + this.debugHUDText.text;
        // }
           // Send(new RosSubscribe_old("/planned_successful", "std_msgs/Bool", 100)); //the minimum amount of time (in ms) that must elapse between messages being sent. Defaults to 0        

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
                //this.messageCount++;
                //MaybeLog ("ProcessRosMessageQueue...");                
                if (this.rosMessageStrings.Count() > 0)
                 {                    
                    //MaybeLog ("Try to dequeue...");
                    //lock (syncObjMessageQueue)
                    {
                        //MaybeLog ("Dequeue...");                    

                        //this.DeserializeJSONstringHard(rosMessageStrings.Dequeue());
                        //this.messageCount += 1;
#if WINDOWS_UWP
                        //                        TODO: mertke
                        
                        RosMessage msg = DeserializeJSONstring(rosMessageStrings.Dequeue());
                        
                        if (msg != null){
                            this.messageCount++;
                            //this.latestJointState
                            var message = msg as RosPublish;
                            this.latestJointState = (JointState)message.msg;                        
                        }

#endif
                    }
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
                if (verbose) this.debugHUDText.text = "\n CreateRosMessageStrings..." + this.debugHUDText.text;                
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
                if(msg.topic == "/hololens_plan_pick"){
                    //TODO
                    messageString = "{ \"op\": \"";
                        messageString += msg.op;
                        messageString += "\", ";
                        messageString += "\"topic\": \"";
                        messageString += msg.topic;
                        messageString += "\", ";
                        messageString += "\"msg\": ";
                        messageString += "{";
                        RosMessages_old.geometry_msgs.PointStamped_old ps = (RosMessages_old.geometry_msgs.PointStamped_old)((RosPublish_old)msg).msg;
                            //Header_old header = ps.header;
                            messageString += "\"header\": ";
                            messageString += "{";                        
                                messageString += "\"stamp\": ";
                                messageString += "{";
                                    messageString += "\"secs\": ";
                    messageString += 0;// ps.header.stamp.secs;
                                    messageString += " ,";
                                    messageString += "\"nsecs\": ";
                    messageString += 0;// ps.header.stamp.nsecs;
                                messageString += "}";
                                messageString += " ,";
                                messageString += "\"frame_id\": ";
                                messageString += "\"" + ps.header.frame_id + "\"";
                                messageString += " ,";
                                messageString += "\"seq\": ";
                                messageString += ps.header.seq;                        
                            messageString += "}";
                        messageString += " ,";
                    //RosMessages_old.geometry_msgs.Point_old point = ps.point;
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
                else if (msg.topic == "/hololens_plan_place")
                {
                    //TODO
                    messageString = "{ \"op\": \"";
                    messageString += msg.op;
                    messageString += "\", ";
                    messageString += "\"topic\": \"";
                    messageString += msg.topic;
                    messageString += "\", ";
                    messageString += "\"msg\": ";
                    messageString += "{";
                    RosMessages_old.geometry_msgs.PointStamped_old ps = (RosMessages_old.geometry_msgs.PointStamped_old)((RosPublish_old)msg).msg;
                    //Header_old header = ps.header;
                    messageString += "\"header\": ";
                    messageString += "{";
                    messageString += "\"stamp\": ";
                    messageString += "{";
                    messageString += "\"secs\": ";
                    messageString += 0;// ps.header.stamp.secs;
                    messageString += " ,";
                    messageString += "\"nsecs\": ";
                    messageString += 0;// ps.header.stamp.nsecs;
                    messageString += "}";                   
                    messageString += " ,";
                    messageString += "\"seq\": ";
                    messageString += ps.header.seq;
                    messageString += "}";
                    messageString += " ,";
                    //RosMessages_old.geometry_msgs.Point_old point = ps.point;
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
                else if(msg.topic == "/hololens_execute_pick"){
                    //TODO
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
                else if (msg.topic == "/hololens_execute_place")
                {
                    //TODO
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
                    //TODO
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
                // if (verbose) this.debugHUDText.text = "\n CreateRosMessageStrings..." + this.debugHUDText.text;                
                // messageString = "{ \"op\": \"";
                // messageString += msg.op;
                // messageString += "\", ";
                // messageString += "\"topic\": \"";
                // messageString += msg.topic;
                // messageString += "\", ";
                // messageString += "\"msg\": ";
                // messageString += "{";
                // messageString += "\"data\": ";
                // messageString += "[";
                // messageString += ((Float32MultiArray_old)((RosPublish_old)msg).msg).data[0];
                // messageString += " ,";
                // messageString += ((Float32MultiArray_old)((RosPublish_old)msg).msg).data[1];
                // messageString += " ,";
                // messageString += ((Float32MultiArray_old)((RosPublish_old)msg).msg).data[2];                
                // messageString += "]";
                // messageString += "}";
                // messageString += "}";
            }

            return messageString;//JsonConvert.SerializeObject(msg);
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
            if (message.Contains("planned_joint_states")) {
                
                jsonObject = JsonObject.Parse(message);

               

                //this.messageCount = (int)jsonObject["msg"].GetObject()["header"].GetObject()["seq"].GetNumber();
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

                //if (!names.Contains("s_model_palm_finger_1_joint")) return;
                //else this.messageCount++;

                latestJointState.name = names;
                latestJointState.position = positions;
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
        // Schnittstelle des Parsers. Hier wird die Deserialisierung aufgerufen.
    public RosMessage DeserializeJSONstring(string message)
    {
        JsonObject jsonObject = JsonObject.Parse(message);
        string jtopic = jsonObject["topic"].GetString();
        rosComplexCoder coder = null;
        if (parserConfig.getJointStateTopics().Contains(jtopic))
        {
            coder = new RosJointStateCoderParallel();
            return coder.startDeserializing(jsonObject);
        }
        else if (parserConfig.getJointTrajectoryTopics().Contains(jtopic))
        {
            coder = new RosJointTrajectoryCoder_();
            return coder.startDeserializing(jsonObject);
        }
        else
        {
            return null;
        }
    }

    // Schnittstelle des Parsers. Hier wird die Serialisierung aufgerufen.
    public string SerializeROSmessage(RosMessage message)
    {

        if (message.GetType() == typeof(RosPublish))
        {
            rosComplexCoder coder = null;
            if (parserConfig.getJointStateTopics().Contains(message.topic))
            {
                coder = new RosJointStateCoderSeriell();
                return coder.startSerializing(message);
            }
            else if (parserConfig.getJointTrajectoryTopics().Contains(message.topic))
            {
                coder = new RosJointTrajectoryCoder_();
                return coder.startSerializing(message);
            }
            else
            {
                return null;
            }
        }
        return null;
    }
#endif

        public CompressedImage_old GetLatestImage(){
			return latestImage;
		}
#if WINDOWS_UWP
		public JointState GetLatestJoinState (){
			return latestJointState;
		}
#endif

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
                Debug.Log(logstring);
            }
        }

    }
}
