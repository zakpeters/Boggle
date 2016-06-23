//Zachary Peters and Tyler Gardner -- Last updated 3/24/2016

using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BoggleClient
{
    /// <summary>
    /// Start menu for a boggle board client.
    /// </summary>
    public partial class StartForm : Form
    {
        /// <summary>
        /// Creates a new form.
        /// </summary>
        public StartForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The client used to send requests to the server
        /// </summary>
        private HttpClient client;

        /// <summary>
        /// Timer for updating client
        /// </summary>
        private Timer timer;

        /// <summary>
        /// A unique identifying token
        /// </summary>
        private string usertoken;

        /// <summary>
        /// A unique game identifier
        /// </summary>                
        private string gameId;

        /// <summary>
        /// The board where the game is played
        /// </summary>
        private BoggleBoard board;


        private bool Cancel { get; set; }


        /// <summary>
        /// Creates a new user
        /// </summary>
        private void CreateUser()
        {
            HttpContent content = new StringContent("{ \"Nickname\":\"" + UserText.Text + "\"}", Encoding.UTF8, "application/json");
            HttpResponseMessage response = client.PostAsync("/BoggleService.svc/users", content).Result;
            if (response.IsSuccessStatusCode)
            {
                string result = response.Content.ReadAsStringAsync().Result;
                dynamic token = JsonConvert.DeserializeObject(result);
                usertoken = token.UserToken;
            }
            else
            {
                MessageBox.Show("Please enter a valid UserName");
            }


        }

        /// <summary>
        /// Creates a new game.
        /// </summary>
        private void CreateGame()
        {
            if (Cancel)
            {
                return;
            }

            int time;
            //no time error
            if (!int.TryParse(TimeText.Text, out time))
            {
                MessageBox.Show("Please enter a valid Game Duration.");
                return;
            }

            string json = "{ \"UserToken\":\"" + usertoken + "\"," + "\"TimeLimit\":" + time + "}";
            HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = client.PostAsync("/BoggleService.svc/games", content).Result;

            if (response.IsSuccessStatusCode)
            {
                string responsecode = response.StatusCode.ToString();
                if (!responsecode.Equals("Conflict"))
                {
                    string result = response.Content.ReadAsStringAsync().Result;

                    dynamic token = JsonConvert.DeserializeObject(result);
                    gameId = (string)token.GameID;
                    timer = new Timer();
                    timer.Interval = 1000;
                    timer.Tick += new EventHandler(StartGame);
                    timer.Start();
                }
                else
                {
                    MessageBox.Show("User is already in waiting for pending game");
                }
            }
            //invalid time error
            else if (time > 120 || time < 5)
            {
                MessageBox.Show("Please enter a valid Game Duration.");
            }
        }
        /// <summary>
        /// attempts to start game
        /// </summary>
        private void StartGame(object sender, EventArgs e)
        {
            HttpResponseMessage response = client.GetAsync("/BoggleService.svc/games/" + gameId).Result;
            string result = response.Content.ReadAsStringAsync().Result;
            if (result.Contains("active"))
            {
                timer.Stop();
                board = new BoggleBoard(client, gameId, usertoken, this);
                board.Show();
            }


        }

        /// <summary>
        /// Fires when go button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GoButton_Click(object sender, EventArgs e)
        {
            GoButton.Enabled = false;
            Cancel = false;
            CancelButton.Enabled = true;
            client = new HttpClient();
            if (ServerText.Text == "")
            {
                MessageBox.Show("You are required to enter a servername.");
                GoButton.Enabled = true;
                return;
            }
            client.BaseAddress = new Uri(ServerText.Text);

            if (usertoken == null)
            {
                CreateUser();
            }


            //create game if usertoken is returned
            if (usertoken != null | !Cancel)
            {
                CreateGame();

            }
        }


        /// <summary>
        /// Fires when cancel button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void CancelButton_Click(object sender, EventArgs e)
        {
            Cancel = true;
            CancelButton.Enabled = false;
            GoButton.Enabled = true;
            if (timer != null)
            {
                timer.Stop();
            }
            if (client != null && usertoken != null)
            {
                string json = "{ \"UserToken\":\"" + usertoken + "}";
                HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
                client.PutAsync("/BoggleService.svc/games/", content);

            }

            if (board != null)
            {
                board.Hide();
                board = null;
            }

            usertoken = null;
            this.Show();

        }
        /// <summary>
        /// Sets Text Entry to Basic values
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DefaultButton_Click(object sender, EventArgs e)
        {
            ServerText.Text = "http://localhost:50000";
            TimeText.Text = "120";
            UserText.Text = "USER";
        }

        private void StartForm_Load(object sender, EventArgs e)
        {
            //Not implemented
        }


        /// <summary>
        /// Displays the help menu message box when help is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HelpMenu_Click(object sender, EventArgs e)
        {
            StringBuilder helpmessage = new StringBuilder();
            helpmessage.AppendLine("Enter a server name, a username, and a time between 5 and 120.");
            helpmessage.AppendLine("When ready, click go. To cancel request, click cancel.");
            helpmessage.AppendLine("See website: http://www.fun-with-words.com/play_boggle.html for boggle rules and scoreing details");
            String help = helpmessage.ToString();
            MessageBox.Show(help);
        }
    }
}
