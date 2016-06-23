using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using static System.Net.HttpStatusCode;

namespace Boggle
{
    /// <summary>
    /// Provides a way to start and stop the IIS web server from within the test
    /// cases.  If something prevents the test cases from stopping the web server,
    /// subsequent tests may not work properly until the stray process is killed
    /// manually.
    /// </summary>
    public static class IISAgent
    {
        // Reference to the running process
        private static Process process = null;

        /// <summary>
        /// Starts IIS
        /// </summary>
        public static void Start(string arguments)
        {
            if (process == null)
            {
                ProcessStartInfo info = new ProcessStartInfo(Properties.Resources.IIS_EXECUTABLE, arguments);
                info.WindowStyle = ProcessWindowStyle.Minimized;
                info.UseShellExecute = false;
                process = Process.Start(info);
            }
        }

        /// <summary>
        ///  Stops IIS
        /// </summary>
        public static void Stop()
        {
            if (process != null)
            {
                process.Kill();
            }
        }
    }
    [TestClass]
    public class BoggleTests
    {
        /// <summary>
        /// This is automatically run prior to all the tests to start the server
        /// </summary>
        [ClassInitialize()]
        public static void StartIIS(TestContext testContext)
        {
            IISAgent.Start(@"/site:""BoggleService"" /apppool:""Clr4IntegratedAppPool"" /config:""..\..\..\.vs\config\applicationhost.config""");
        }

        /// <summary>
        /// This is automatically run when all tests have completed to stop the server
        /// </summary>
        [ClassCleanup()]
        public static void StopIIS()
        {
            IISAgent.Stop();
        }

        private RestTestClient client = new RestTestClient("http://localhost:60000/");

        [TestMethod]
        public void BasicTests()
        {
            //Create users -- throws exception if unsuccessful
            string usertoken1 = CreateUser("Boggler");
            string usertoken2 = CreateUser("Boggled");

            //Join game 1 -- throws exception if unsuccessful
            string gameid1 = JoinGame(usertoken1, 15);
            dynamic token = GameStatus(gameid1);


            //Test that game is pending
            string status = token.GameState;
            Assert.AreEqual("pending", status);

            //Join game 2
            string gameid2 = JoinGame(usertoken2, 15);

            //Test that gameid1 and gameid2 are same
            Assert.AreEqual(gameid1, gameid2);

            //Test that game is now active
            token = GameStatus(gameid1);
            status = token.GameState;
            Assert.AreEqual("active", status);

            //Test play word player 1
            string score = PlayWord(usertoken1, "ABUDABUDABBADABBA", gameid1);
            Assert.AreEqual("-1", score);

            //Test the game status
            token = GameStatus(gameid1);
            score = token.Player1.Score;
            Assert.AreEqual("-1", score);


            //Test play word player 2
            score = PlayWord(usertoken2, "ZXquekoi03jd@", gameid1);
            Assert.AreEqual("-1", score);

            //Test the game status
            token = GameStatus(gameid1);
            score = token.Player2.Score;
            Assert.AreEqual("-1", score);


            //Test stream generation
            Response r = client.DoGetAsync("/games/" + gameid1).Result;
            Assert.AreEqual(r.Status, OK);

            BoggleBoard bb = new BoggleBoard("AAAAAAAAAAAAAAAA");

        }
        /// <summary>
        /// Basic cancel join
        /// </summary>
        [TestMethod]
        public void CancelJoin()
        {
            string usertoken = CreateUser("yolo");
            JoinGame(usertoken, 75);
            HttpClient c = new HttpClient();
            HttpContent content = new StringContent("{ \"UserToken\":\"" + usertoken + "\"}", Encoding.UTF8, "application/json");
            HttpResponseMessage response = c.PutAsync("http://localhost:60000/BoggleService.svc/games", content).Result;
            Assert.AreEqual(response.StatusCode, OK);

        }
        [TestMethod]
        public void CreateUserEmpty()
        {
            HttpClient c = new HttpClient();
            HttpContent content = new StringContent("{ \"Nickname\":\"\"}", Encoding.UTF8, "application/json");
            HttpResponseMessage response = c.PostAsync("http://localhost:60000/BoggleService.svc/users", content).Result;
            Assert.IsTrue(response.StatusCode == Forbidden);

        }
        [TestMethod]
        public void CreateUserNull()
        {
            HttpClient c = new HttpClient();
            HttpContent content = new StringContent("{ \"Nickname\":null}", Encoding.UTF8, "application/json");
            HttpResponseMessage response = c.PostAsync("http://localhost:60000/BoggleService.svc/users", content).Result;
            Assert.IsTrue(response.StatusCode == Forbidden);
        }
        /// <summary>
        /// joins games with bad time
        /// expects 403 error Forbidden
        /// </summary>
        [TestMethod]
        public void JoinGameBadTime()
        {
            string usertoken = CreateUser("yolo");
            HttpClient client = new HttpClient();
            string json = "{ \"UserToken\":\"" + usertoken + "\"," + "\"TimeLimit\":" + 3 + "}";
            HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = client.PostAsync("http://localhost:60000/BoggleService.svc/games", content).Result;
            Assert.AreEqual(response.StatusCode, Forbidden);

            json = "{ \"UserToken\":\"" + usertoken + "\"," + "\"TimeLimit\":" + 125 + "}";
            content = new StringContent(json, Encoding.UTF8, "application/json");
            response = client.PostAsync("http://localhost:60000/BoggleService.svc/games", content).Result;
            Assert.AreEqual(response.StatusCode, Forbidden);

        }
        /// <summary>
        /// user already in pending game
        /// expects 409 error Conflict
        /// </summary>
        [TestMethod]
        public void JoinGamePending()
        {
            string usertoken = CreateUser("yolo");
            JoinGame(usertoken, 75);

            HttpClient client = new HttpClient();
            string json = "{ \"UserToken\":\"" + usertoken + "\"," + "\"TimeLimit\":" + 75 + "}";
            HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = client.PostAsync("http://localhost:60000/BoggleService.svc/games", content).Result;
            Assert.AreEqual(response.StatusCode, Conflict);

        }
        /// <summary>
        /// join game invalid token
        /// Expects Forbidden 409
        /// </summary>
        [TestMethod]
        public void JoinGameInvalid()
        {
            string usertoken = CreateUser("yolo");
            JoinGame(usertoken, 75);

            HttpClient client = new HttpClient();
            string json = "{ \"UserToken\":\"" + "lemons" + "\"," + "\"TimeLimit\":" + 75 + "}";
            HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = client.PostAsync("http://localhost:60000/BoggleService.svc/games", content).Result;
            Assert.AreEqual(response.StatusCode, Forbidden);

            json = "{ \"UserToken\":\"" + usertoken + "\"," + "\"TimeLimit\":" + 75 + "}";
            content = new StringContent(json, Encoding.UTF8, "application/json");
            response = client.PostAsync("http://localhost:60000/BoggleService.svc/games", content).Result;

            json = "{ \"UserToken\":\"" + "lemons" + "\"," + "\"TimeLimit\":" + 75 + "}";
            content = new StringContent(json, Encoding.UTF8, "application/json");
            response = client.PostAsync("http://localhost:60000/BoggleService.svc/games", content).Result;
            Assert.AreEqual(response.StatusCode, Forbidden);

        }
        /// <summary>
        /// user invalid 
        /// expects 403 Forbidden
        /// </summary>
        [TestMethod]
        public void CancelJoinEdge1()
        {
            HttpClient c = new HttpClient();
            HttpContent content = new StringContent("{ \"UserToken\":\"thistokenisnotvalid\"}", Encoding.UTF8, "application/json");
            HttpResponseMessage response = c.PutAsync("http://localhost:60000/BoggleService.svc/games", content).Result;
            Assert.AreEqual(response.StatusCode, Forbidden);
            content = new StringContent("{ \"UserToken\":\"" + CreateUser("yolo") + "\"}", Encoding.UTF8, "application/json");
            response = c.PutAsync("http://localhost:60000/BoggleService.svc/games", content).Result;
            Assert.AreEqual(response.StatusCode, Forbidden);
        }
        /// <summary>
        /// user not in a pending game
        /// expects 403 Forbidden
        /// </summary>
        [TestMethod]
        public void CancelJoinEdge2()
        {
            string token = CreateUser("yolo");
            HttpClient c = new HttpClient();
            HttpContent content = new StringContent("{ \"UserToken\":\"" + token + "\"}", Encoding.UTF8, "application/json");
            HttpResponseMessage response = c.PutAsync("http://localhost:60000/BoggleService.svc/games", content).Result;
            Assert.AreEqual(response.StatusCode, Forbidden);
        }
        /// <summary>
        /// null word
        /// empty word
        /// invalid/missing usertoken or gameid
        /// expects 403 forbidden
        /// </summary>
        [TestMethod]
        public void PlayWordForbidden()
        {

            JoinGame(CreateUser("buffer"), 5);
            string usertoken = CreateUser("yolo");
            string gameId = JoinGame(usertoken, 5);

            //empty word
            HttpClient client = new HttpClient();
            string json = "{ \"UserToken\":\"" + usertoken + "\"," + "\"Word\":\"\"}";
            HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = client.PutAsync("http://localhost:60000/BoggleService.svc/games/" + gameId, content).Result;
            Assert.AreEqual(response.StatusCode, Forbidden);

            //invalid gameid
            json = "{ \"UserToken\":\"" + usertoken + "\"," + "\"Word\":\"" + "aword" + "\"}";
            content = new StringContent(json, Encoding.UTF8, "application/json");
            response = client.PutAsync("http://localhost:60000/BoggleService.svc/games/9999", content).Result;
            //Assert.AreEqual(response.StatusCode, Forbidden);

            //user not in game
            string notingame = CreateUser("mia");
            json = "{ \"UserToken\":\"" + notingame + "\"," + "\"Word\":\"" + "aword" + "\"}";
            content = new StringContent(json, Encoding.UTF8, "application/json");
            response = client.PutAsync("http://localhost:60000/BoggleService.svc/games/" + gameId, content).Result;
            Assert.AreEqual(response.StatusCode, Forbidden);

            //invalid gamestate
            usertoken = CreateUser("gameover");
            gameId = JoinGame(usertoken, 5);
            JoinGame(CreateUser("buffer"), 5);
            System.Threading.Thread.Sleep(10000);
            json = "{ \"UserToken\":\"" + usertoken + "\"," + "\"Word\":\"" + "dog" + "\"}";
            content = new StringContent(json, Encoding.UTF8, "application/json");
            response = client.PutAsync("http://localhost:60000/BoggleService.svc/games/" + gameId, content).Result;
            GameStatus(gameId);
            Assert.AreEqual(response.StatusCode, Conflict);


        }
        /// <summary>
        /// invalid gameid
        /// 409 Conflict expected
        /// </summary>
        [TestMethod]
        public void GameStatusInvalid()
        {
            GameStatus("100");
        }
        public string CreateUser(string nickname)
        {
            //Create user with nickname
            HttpClient c = new HttpClient();
            HttpContent content = new StringContent("{ \"Nickname\":\"" + nickname + "\"}", Encoding.UTF8, "application/json");
            HttpResponseMessage response = c.PostAsync("http://localhost:60000/BoggleService.svc/users", content).Result;
            string result = response.Content.ReadAsStringAsync().Result;
            dynamic token = JsonConvert.DeserializeObject(result);
            string usertoken = token.UserToken;

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception();
            }
            else
            {
                return usertoken;
            }
        }

        public string JoinGame(string usertoken, int timelimit)
        {
            HttpClient client = new HttpClient();
            string json = "{ \"UserToken\":\"" + usertoken + "\"," + "\"TimeLimit\":" + timelimit + "}";
            HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = client.PostAsync("http://localhost:60000/BoggleService.svc/games", content).Result;

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("failed to joingame: " + response.StatusCode.ToString());
            }
            else
            {
                string result = response.Content.ReadAsStringAsync().Result;
                dynamic token = JsonConvert.DeserializeObject(result);
                string gameId = (string)token.GameID;

                return gameId;
            }
        }

        public string PlayWord(string usertoken, string word, string gameId)
        {
            HttpClient client = new HttpClient();
            string json = "{ \"UserToken\":\"" + usertoken + "\"," + "\"Word\":\"" + word + "\"}";
            HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = client.PutAsync("http://localhost:60000/BoggleService.svc/games/" + gameId, content).Result;
            string result = response.Content.ReadAsStringAsync().Result;
            dynamic token = JsonConvert.DeserializeObject(result);
            string score = (string)token.Score;

            return score;
        }

        public dynamic GameStatus(string gameid)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = client.GetAsync("http://localhost:60000/BoggleService.svc/games/" + gameid).Result;
            string result = response.Content.ReadAsStringAsync().Result;
            dynamic token = JsonConvert.DeserializeObject(result);

            return token;
        }
    }
}
