//Zachary Peters and Tyler Gardner -- Last updated 3/24/2016

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;

namespace BoggleClient
{
    /// <summary>
    /// A board for playing boggle.
    /// </summary>
    public partial class BoggleBoard : Form
    {

        /// <summary>
        /// Creates board and initializes vars
        /// </summary>
        /// <param name="client"></param>
        /// <param name="gameId"></param>
        /// <param name="usertoken"></param>
        /// <param name="form"></param>
        public BoggleBoard(HttpClient client, string gameId, string usertoken, StartForm form)
        {
            InitializeComponent();
            closed = false;
            this.client = client;
            this.gameId = gameId;
            this.user = usertoken;
            this.form = form;

            HttpResponseMessage response = client.GetAsync("/BoggleService.svc/games/" + gameId).Result;
            string result = response.Content.ReadAsStringAsync().Result;
            dynamic token = JsonConvert.DeserializeObject(result);

            Player1.Text = (string)token.Player1.Nickname + " :";
            Player2.Text = (string)token.Player2.Nickname + " :";

            //populate board
            board = (string)token.Board;
            int i = 0;
            foreach (Button s in Buttons.Controls)
            {
                s.Text = board[i++] + "";
                if (s.Text == "Q")
                {
                    s.Text = "QU";
                }
            }
            UpdateBoard();
            time = new Timer();
            time.Interval = 1000;
            time.Tick += new EventHandler(TimerTick);
            time.Start();

            FormClosing += form.CancelButton_Click;
            FormClosing += Closed;
            form.Hide();

        }

        /// <summary>
        /// Client that communicates with server
        /// </summary>
        private HttpClient client;

        /// <summary>
        /// Timer for client
        /// </summary>
        private Timer time;

        /// <summary>
        /// Start form for GUI
        /// </summary>
        private Form form;

        /// <summary>
        /// Unique game identifier
        /// </summary>
        private string gameId;

        /// <summary>
        /// Nick name of user
        /// </summary>
        private string user;

        /// <summary>
        /// board string
        /// </summary>
        private string board;

        /// <summary>
        /// True when board is closed. False otherwise.
        /// </summary>
        private Boolean closed;

        /// <summary>
        /// Handler for timer
        /// </summary>
        private void TimerTick(object sender, EventArgs e)
        {
            UpdateBoard();
        }


        /// <summary>
        /// Sets the closed var when IsClosing event fires
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Closed(object sender, EventArgs e)
        {
            closed = true;
        }
        /// <summary>
        /// updates scores and time ends game if completed
        /// </summary>
        private void UpdateBoard()
        {
            HttpResponseMessage response = client.GetAsync("/BoggleService.svc/games/" + gameId).Result;
            string result = response.Content.ReadAsStringAsync().Result;
            dynamic token = JsonConvert.DeserializeObject(result);

            Score1.Text = (string)token.Player1.Score;
            Score2.Text = (string)token.Player2.Score;
            TimeLeft.Text = ((int)token.TimeLeft) / 60 + ":" + ((int)token.TimeLeft) % 60 / 10 + "" + ((int)token.TimeLeft) % 60 % 10;

            if (token.GameState == "completed")
            {
                time.Stop();
                EndGame(token);
            }
        }

        /// <summary>
        /// Plays a word
        /// </summary>
        /// <param name="word"></param>
        private void PlayWord(string word)
        {
            string json = "{ \"UserToken\":\"" + user + "\"," + "\"Word\":\"" + word + "\"}";
            HttpContent content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = client.PutAsync("/BoggleService.svc/games/" + gameId, content).Result;
            TextEntry.Text = "";
        }


        /// <summary>
        /// End game message box
        /// </summary>
        /// <param name="token"></param>
        private void EndGame(dynamic token)
        {
            if (closed)
            {
                return;
            }
            //Block all buttons
            foreach (Button s in Buttons.Controls)
            {
                s.Enabled = false;
            }


            TextEntry.Enabled = false;
            EnterButton.Enabled = false;
            CheatButton.Enabled = false;

            //Get scores and determine winner
            int s1, s2;
            int.TryParse(Score1.Text, out s1);
            int.TryParse(Score2.Text, out s2);
            string winner = Player1.Text;
            if (s1 < s2)
            {
                winner = Player2.Text;
            }
            //message construction
            string message = "GAME OVER! " + winner + " is the winner!\n";
            message += "\n=======PLAYER1=======\n";
            message += "\n\t  " + Player1.Text + "  :  " + Score1.Text;
            message += "\n========WORDS========\n";
            foreach (dynamic pair in token.Player1.WordsPlayed)
            {
                message += (string)pair.Word + " : ";
                message += (string)pair.Score + "\n";
            }
            message += "\n=======PLAYER2=======\n";
            message += "\n\t  " + Player2.Text + "  :  " + Score2.Text;
            message += "\n========WORDS========\n";
            foreach (dynamic pair in token.Player2.WordsPlayed)
            {
                message += (string)pair.Word + " : ";
                message += (string)pair.Score + "\n";
            }
            //display scores and words
            MessageBox.Show(message);
        }


        /// <summary>
        /// When enter button is clicked this fires.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EnterButton_Click(object sender, EventArgs e)
        {
            PlayWord(TextEntry.Text);
        }

        private void Letter_Click(object sender, EventArgs e)
        {
            TextEntry.Text += ((Button)sender).Text;
            EnterButton.Select();
        }

        private void CheatButton_Click(object sender, EventArgs e)
        {
            CheatButton.Enabled = false;
            Cheat();
        }

        /// <summary>
        /// ONLY FOR CHEATERS
        /// </summary>
        private void Cheat()
        {

            HashSet<string> dict = new HashSet<string>();
            using (StreamReader sr = new StreamReader("cheat.txt"))
            {

                String file = sr.ReadToEnd();
                string[] list = file.Split();
                foreach (string word in list)
                {
                    dict.Add(word.ToUpper());
                }
            }
            HashSet<string> words = new HashSet<string>();
            string s = "";
            bool[] visited = new bool[16];
            for (int i = 0; i < 16; i++)
            {
                DFS(i, visited, s, words);
            }
            foreach (string word in words)
            {
                if (dict.Contains(word))
                {
                    PlayWord(word);
                }
            }
        }
        /// <summary>
        /// recursive DFS for cheat button
        /// </summary>
        private void DFS(int i, bool[] visited, string s, HashSet<string> words)
        {
            if (i < 0 || i > 15 || visited[i]) { return; }
            visited[i] = true;
            s += board[i];
            words.Add(s);

            if (i % 4 != 0)
            {
                DFS(i - 1, visited, s, words);
                DFS(i - 5, visited, s, words);
                DFS(i + 3, visited, s, words);

            }
            if (i % 4 == 0)
            {
                DFS(i - 3, visited, s, words);
                DFS(i + 1, visited, s, words);
                DFS(i + 5, visited, s, words);
            }
            DFS(i - 4, visited, s, words);
            DFS(i + 4, visited, s, words);
            visited[i] = false;
        }


        /// <summary>
        /// Not implemented
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BoggleBoard_Load(object sender, EventArgs e)
        {
            //not implemented.
        }
    }
}
