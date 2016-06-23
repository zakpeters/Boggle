//Written by Zachary Peters u0743528 and Tyler Gardner u0372543 April 2016

using CustomNetworking;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;


namespace Boggle
{
    /// <summary>
    /// Runs a boggle service web server.
    /// </summary>
    public class Server
    {
        /// <summary>
        /// Service to be used for server
        /// </summary>
        private static BoggleService boggleservice;

        public static void Main()
        {
            new Server();
            Console.Read();
        }

        /// <summary>
        /// Listens on TCP
        /// </summary>
        private TcpListener server;

        //matches and groups a string of 1 or more ints
        private Regex gameidnum = new Regex(@"([0-9]+)");


        public Server()
        {
            boggleservice = new BoggleService();
            server = new TcpListener(IPAddress.Any, 60000);
            server.Start();
            server.BeginAcceptSocket(ConnectionRequested, null);
        }

        private void ConnectionRequested(IAsyncResult ar)
        {
            Socket s = server.EndAcceptSocket(ar);
            server.BeginAcceptSocket(ConnectionRequested, null);
            new HttpRequest(new StringSocket(s, new UTF8Encoding()), boggleservice);
        }
    }

    /// <summary>
    /// Performs socket and http functions
    /// </summary>
    class HttpRequest
    {
        private StringSocket ss;
        private int lineCount;
        private int contentLength;
        //public string httpstatus { get; set; }
        private BoggleService boggleservice;
        private string[] newpayload;



        public HttpRequest(StringSocket stringSocket, BoggleService boggleservice)
        {
            this.ss = stringSocket;
            this.boggleservice = boggleservice;
            ss.BeginReceive(LineReceived, null);
            //initialize httpstatus to 400 Bad Request -- change in instance that request is valid/successful
            //httpstatus = "400 Bad Request";
            newpayload = new string[3];
        }

        /// <summary>
        /// Receives the line on the socket
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        /// <param name="payload"></param>
        private void LineReceived(string s, Exception e, object payload)
        {
            string request;
            lineCount++;
            Console.WriteLine(s);


            if (s != null)
            {
                //will store the switch case string
                if (lineCount == 1)
                {
                    Regex r = new Regex(@"^(\S+)\s+(\S+)");
                    Match m = r.Match(s);

                    //matches and groups a string of 1 or more ints
                    Regex gameidnum = new Regex(@"(\/[0-9]+)");


                    Console.WriteLine("Method: " + m.Groups[1].Value);
                    Console.WriteLine("URL: " + m.Groups[2].Value);

                    //Breakdown future action according to method and URL val
                    string method = m.Groups[1].Value;
                    string url = m.Groups[2].Value;


                    //set the request string for switch statement
                    if (method == "POST" && url.Contains("/BoggleService.svc/users"))
                    {
                        request = "createuser";
                        newpayload[0] = request;
                        newpayload[1] = "";

                    }
                    else if (method == "POST" && url.Contains("/BoggleService.svc/games"))
                    {
                        request = "joingame";
                        newpayload[0] = request;
                        newpayload[1] = url;

                    }
                    else if (method == "PUT" && url == ("/BoggleService.svc/games"))
                    {
                        request = "cancelgame";
                        newpayload[0] = request;
                    }
                    else if (method == "PUT" && url.Contains("/BoggleService.svc/games/"))
                    {
                        request = "playword";
                        newpayload[0] = request;
                        newpayload[1] = url;

                        //Match the gameid?
                        m = gameidnum.Match(url);
                        string gameid = m.Groups[0].ToString();
                        if (gameid.Length > 1)
                        {
                            gameid = gameid.Substring(1);
                        }
                        
                        newpayload[2] = gameid;
                    }
                    else if (method == "GET" && url.Contains("/BoggleService.svc/games/"))

                    {
                        request = "gamestatus";
                        newpayload[0] = request;
                        newpayload[1] = url;
                        //Match the gameid?
                        m = gameidnum.Match(url);
                        string gameid = m.Groups[0].ToString();
                        if (gameid.Length > 1)
                        {
                            gameid = gameid.Substring(1);
                        }
                        newpayload[2] = gameid;
                        ss.BeginReceive(ContentReceived, newpayload, 0);
                    }
                    else
                    {

                    }
                }
                if (s.StartsWith("Content-Length:"))
                {
                    contentLength = Int32.Parse(s.Substring(16).Trim());
                }
                if (s == "\r")
                {
                    //sends the json content. sends request string as payload.
                    ss.BeginReceive(ContentReceived, newpayload, contentLength);
                }
                else
                {
                    ss.BeginReceive(LineReceived, null);
                }
            }
        }


        /// <summary>
        /// Processes content as it is received
        /// </summary>
        private void ContentReceived(string s, Exception e, object payload)
        {
            
            string httpstatus;

            Console.WriteLine("s: " + s);
            Console.WriteLine("payload: " + ((string[])payload)[0]);
            Console.WriteLine("payload: " + ((string[])payload)[1]);            
            if (s != null)
            {
                //where to store a serialized object for beginsend
                string result = "";
                
                //string representing the request type
                string request = ((string[])payload)[0]; //(will this throw a null exception??)

                //string containing the url
                string url = ((string[])payload)[1]; //(will this throw a null exception??)
                
                //switch statement based upon result of request to server passed through "payload" param (ie request)
                switch (request)
                {
                    case "createuser":
                        //deserialize json parameters in s to pass to appropriate method in BS
                        UserInfo userinfo = JsonConvert.DeserializeObject<UserInfo>(s);

                        //Check that object has required fields else bad request
                            //TODO
                        
                        //store the return object of the method
                        userinfo = boggleservice.CreateUser(userinfo);                     
                        httpstatus = userinfo.HttpStatus;

                        //remove httpstatus
                        userinfo.HttpStatus = null;

                        //serialize the return object
                        result = JsonConvert.SerializeObject(userinfo, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        Console.WriteLine(result);
                        //get the http status?                       
                        break;
                    case "joingame":
                        //deserialize json parameters in s to pass to appropriate method in BS
                        userinfo = JsonConvert.DeserializeObject<UserInfo>(s);

                        //Check that object has required fields else bad request
                            //TODO

                        //store the return object of the method
                        userinfo = boggleservice.JoinGame(userinfo);
                        httpstatus = userinfo.HttpStatus;
                        //remove httpstatus
                        userinfo.HttpStatus = null;
                        //serialize the return object
                        result = JsonConvert.SerializeObject(userinfo, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        Console.WriteLine(result);
                        //get the http status?                       
                        break;
                    case "cancelgame":
                        //deserialize json parameters in s to pass to appropriate method in BS
                        userinfo = JsonConvert.DeserializeObject<UserInfo>(s);

                        //Check that object has required fields else bad request
                            //TODO

                        //store the return object of the method
                        userinfo = boggleservice.CancelJoinRequest(userinfo);
                        httpstatus = userinfo.HttpStatus;
                        //remove httpstatus
                        userinfo.HttpStatus = null;
                        //serialize the return object
                        result = JsonConvert.SerializeObject(userinfo, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        //get the http status?                       
                        break;
                    case "playword":
                        //deserialize json parameters in s to pass to appropriate method in BS
                        userinfo = JsonConvert.DeserializeObject<UserInfo>(s);

                        //Check that object has required fields else bad request
                            //TODO

                        //extract gameid from URL
                        string gameid = ((string[])payload)[2];                   
                        //store the return object of the method
                        userinfo = boggleservice.PlayWord(gameid, userinfo);
                        httpstatus = userinfo.HttpStatus;
                        //remove httpstatus
                        userinfo.HttpStatus = null;

                        //serialize the return object
                        result = JsonConvert.SerializeObject(userinfo, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        //get the http status?                       
                        break;
                    case "gamestatus":
                        //extract data from the url
                        string brief = "";

                        if (url.Contains("yes"))
                        {
                            brief = "yes";
                        }

                        //save gameid
                        gameid = ((string[])payload)[2];

                        //serialize the return object
                        GameStatus gamestatus = boggleservice.GameStatus(gameid, brief);
                        httpstatus = gamestatus.HttpStatus;
                        //remove httpstatus
                        gamestatus.HttpStatus = null;

                        result = JsonConvert.SerializeObject(gamestatus, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
                        //get the http status?
                        break;
                        default:
                        httpstatus = "400 Bad Request";
                        break;

                }
                Console.WriteLine(httpstatus);
                ss.BeginSend("HTTP/1.1 " + httpstatus + "\r\n", Ignore, null);
                ss.BeginSend("Content-Type: application/json\r\n", Ignore, null);
                ss.BeginSend("Content-Length: " + result.Length + "\r\n", Ignore, null);
                ss.BeginSend("\r\n", Ignore, null);
                ss.BeginSend(result, (ex, py) => { ss.Shutdown(); }, null);
            }
        }

        /// <summary>
        /// Empty method
        /// </summary>
        /// <param name="e"></param>
        /// <param name="payload"></param>
        private void Ignore(Exception e, object payload)
        {
            //do nothing
        }
    }
}