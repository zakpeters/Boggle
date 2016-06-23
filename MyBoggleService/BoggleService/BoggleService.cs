//Written by Zachary Peters u0743528 and Tyler Gardner u0372543 April 2016

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.ServiceModel.Web;

namespace Boggle
{
    /// <summary>
    /// Boggle service using TSQL database
    /// </summary>
    public class BoggleService : IBoggleService
    {
        /// <summary>
        /// A hashset containing words from dictionary.txt
        /// </summary>
        private static HashSet<string> validwords;
        /// <summary>
        /// A T-SQL database for retaining BoggleService's game states
        /// </summary>
        private string BoggleDB;

        /// <summary>
        /// Static constructor is called before class can be referenced and initializes certain vars
        /// </summary>
        public BoggleService()
        {
            //Initializes the dictionary:
            //Get the path of the file relative to the current directory
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..\\dictionary.txt");

            // Read each line of the file into a string array. Each element
            // of the array is one line of the file.
            string[] lines = System.IO.File.ReadAllLines(path);
            validwords = new HashSet<string>();
            foreach (string s in lines)
            {
                validwords.Add(s);
            }

            //Initialie the connection string into the program
            BoggleDB = ConfigurationManager.ConnectionStrings["BoggleDB"].ConnectionString;
        }

        /// <summary>
        /// The most recent call to SetStatus determines the response code used when
        /// an http response is sent.
        /// </summary>
        private static void SetStatus(HttpStatusCode status)
        {
            //old method
            //WebOperationContext.Current.OutgoingResponse.StatusCode = status;
        }

        /// <summary>
        /// Returns a Stream version of index.html.
        /// </summary>
        public Stream API()
        {
            //Initialie the connection string into the program
            //BoggleDB = ConfigurationManager.ConnectionStrings[BoggleDB].ConnectionString;
            //Set up the API
            WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";
            return File.OpenRead(AppDomain.CurrentDomain.BaseDirectory + "index.html");
        }

        /// <summary>
        /// Cancels a join request.
        /// </summary>
        public UserInfo CancelJoinRequest(UserInfo user)
        {
            using (SqlConnection conn = new SqlConnection(BoggleDB))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {

                    //select the most recent game from the database
                    using (SqlCommand command = new SqlCommand("select top 1 * from Games order by GameID DESC", conn, trans))
                    {
                        //initialize the reader to read the row selected
                        using (SqlDataReader reader = command.ExecuteReader())
                        {

                            reader.Read();
                            //check that usertoken matches most recent game and that Player2 hasn't joined
                            if (!(reader["Player1"] is System.DBNull) && (reader["Player2"] is System.DBNull) && (string)reader["Player1"] == user.UserToken)
                            {
                                //set time and user to null
                                using (SqlCommand newcommand = new SqlCommand("update Games set Player1 = NULL  where GameID = @GameID", conn, trans))
                                {

                                    //newcommand.Parameters.AddWithValue("@UserID", null);
                                    newcommand.Parameters.AddWithValue("@GameID", (int)reader["GameID"]);
                                    reader.Close();
                                    newcommand.ExecuteNonQuery();
                                    UserInfo ret = new UserInfo();
                                    ret.HttpStatus = "200 OK";
                                    trans.Commit();
                                    return ret;
                                }
                            }
                            else
                            {

                                UserInfo ret = new UserInfo();
                                ret.HttpStatus = "403 Forbidden";
                                return ret;
                            }
                        }
                    }
                }
            }

        }

        /// <summary>
        /// Returns the status of a game.
        /// </summary>
        public GameStatus GameStatus(string GameID, string Brief)
        {
            
            bool brief = false;
            if (Brief == "yes")
            {
                brief = true;
            }
            int GameId;
            int.TryParse(GameID, out GameId);
            GamesDB game = QueryGame(GameId);
            GameStatus ret = new GameStatus();
            if (game == null)
            {
                ret.HttpStatus = "403 Forbidden";
                return ret;
            }
            if (game.Player2 == null)
            {
                ret.HttpStatus = "200 OK";
                ret.GameState = "pending";
                return ret;
            }
            else if (game.StartTime.AddSeconds(game.TimeLimit) < DateTime.Now)
            {
                ret.GameState = "completed";
                ret.TimeLeft = 0;
            }
            else
            {
                ret.GameState = "active";
                ret.TimeLeft = (game.StartTime.AddSeconds(game.TimeLimit) - DateTime.Now).Seconds + (game.StartTime.AddSeconds(game.TimeLimit) - DateTime.Now).Minutes * 60;
            }

            if (!brief)
            {
                ret.Board = game.Board;
                ret.TimeLimit = game.TimeLimit;
            }

            //Player info
            PlayerModel P1 = new PlayerModel();
            PlayerModel P2 = new PlayerModel();
            ret.Player1 = P1;
            ret.Player2 = P2;
            P1.Score = 0;
            P2.Score = 0;
            if (!brief)
            {
                P1.Nickname = QueryUser(game.Player1);
                P2.Nickname = QueryUser(game.Player2);
            }

            LinkedList<WordsDB> words1 = QueryWord(GameId, game.Player1);
            LinkedList<WordsDB> words2 = QueryWord(GameId, game.Player2);

            //Player1 calculate score and words played
            List<WordPlayed> WordsPlayed = new List<WordPlayed>();
            foreach (WordsDB word in words1)
            {
                WordPlayed curr = new WordPlayed();
                curr.Word = word.Word;
                curr.Score = word.Score;
                P1.Score += curr.Score;
                WordsPlayed.Add(curr);
            }
            if (ret.GameState == "completed" && !brief)
            {
                P1.WordsPlayed = WordsPlayed;
            }
            //Player2 calculate score and words played
            WordsPlayed = new List<WordPlayed>();
            foreach (WordsDB word in words2)
            {
                WordPlayed curr = new WordPlayed();
                curr.Word = word.Word;
                curr.Score = word.Score;
                P2.Score += curr.Score;
                WordsPlayed.Add(curr);
            }
            if (ret.GameState == "completed" && !brief)
            {
                P2.WordsPlayed = WordsPlayed;
            }
            ret.HttpStatus = "200 OK";
            return ret;
        }

        /// <summary>
        /// Method for joining game.
        /// </summary>
        public UserInfo JoinGame(UserInfo user)
        {
            //check time limit
            if (user.TimeLimit < 5 | user.TimeLimit > 120)
            {
                UserInfo ret = new UserInfo();
                ret.HttpStatus = "403 Forbidden";
                return ret;
            }
            //check if user exists
            if (QueryUser(user.UserToken) == null)
            {
                UserInfo ret = new UserInfo();
                ret.HttpStatus = "403 Forbidden";
                return ret;
            }
            using (SqlConnection conn = new SqlConnection(BoggleDB))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    //select the most recent game from the database
                    using (SqlCommand command = new SqlCommand("select top 1 * from Games order by GameID DESC", conn, trans))
                    {
                        //initialize the reader to read the row selected
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            reader.Read();
                            //check if there is a player1 already and that player2 is null
                            if (reader["Player1"] is System.DBNull || !(reader["Player2"] is System.DBNull))
                            {
                                //create new row ie game
                                using (SqlCommand newcommand = new SqlCommand("insert into Games (Player1, TimeLimit) output inserted.GameID values(@UserID, @TimeLimit)", conn, trans))
                                {
                                    newcommand.Parameters.AddWithValue("@UserID", user.UserToken);
                                    newcommand.Parameters.AddWithValue("@TimeLimit", user.TimeLimit);

                                    // We execute the command with the ExecuteScalar method, which will return to
                                    // us the requested auto-generated ItemID.
                                    reader.Close();
                                    string GameID = newcommand.ExecuteScalar().ToString();

                                   
                                    trans.Commit();

                                    UserInfo ret = new UserInfo();
                                    ret.HttpStatus = "202 Accepted";
                                    ret.GameID = GameID;
                                    return ret;
                                }
                            }
                            else
                            {
                                // add user to game when player1 != null
                                using (SqlCommand newcommand = new SqlCommand("update Games set Player2 = @UserID , TimeLimit = @TimeLimit , Board = @Board , StartTime = @StartTime where GameID = @GameID", conn, trans))
                                {
                                    UserInfo ret;
                                    //if player1 already in pending game set status to conflict
                                    if ((string)reader["Player1"] == (user.UserToken))
                                    {
                                        ret = new UserInfo();
                                        ret.HttpStatus = "409 Conflict";
                                        return ret;
                                    }
                                    int gameID = (int)reader["GameID"];
                                    int timelim = ((int)reader["TimeLimit"] + user.TimeLimit) / 2;
                                    newcommand.Parameters.AddWithValue("@UserID", user.UserToken);
                                    newcommand.Parameters.AddWithValue("@TimeLimit", timelim);
                                    newcommand.Parameters.AddWithValue("@GameID", gameID);
                                    newcommand.Parameters.AddWithValue("@Board", new BoggleBoard().ToString());
                                    newcommand.Parameters.AddWithValue("@StartTime", DateTime.Now);

                                    reader.Close();
                                    newcommand.ExecuteNonQuery();
                                   
                                    trans.Commit();

                                     ret = new UserInfo();
                                    ret.HttpStatus = "201 Created";
                                    ret.GameID = gameID + "";
                                    return ret;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Plays a word.
        /// </summary>
        public UserInfo PlayWord(string GameID, UserInfo user)
        {
            int GameInt;
            int.TryParse(GameID, out GameInt);
            //trim word
            if (user.Word != null)
            {
                user.Word = user.Word.Trim();
            }
            GamesDB game = QueryGame(GameInt);

            // forbidden when:
            // usertoken is null
            // word is null or ""
            // game doesn't exist
            // usertoken not associated with the game
            if (game == null || user.Word == null || user.Word.Length == 0 || !(game.Player1 == user.UserToken || game.Player2 == user.UserToken))
            {
                UserInfo ret = new UserInfo();
                ret.HttpStatus = "403 Forbidden";
                return ret;
            }

            // Conflict
            // game state isn't active
            if ((game.Player2 == null) || game.StartTime.AddSeconds(game.TimeLimit) < DateTime.Now)
            {
                UserInfo ret = new UserInfo();
                ret.HttpStatus = "409 Conflict";
                return ret;
            }

            //gets all words played so far by this user
            //returns 0 if word has already been played
            LinkedList<WordsDB> played = QueryWord(GameInt, user.UserToken);
            foreach (WordsDB word in played)
            {
                if (word.Word == user.Word)
                {
                    UserInfo ret = new UserInfo();
                    ret.HttpStatus = "200 OK";
                    ret.Score = "0";
                    return ret;
                }
            }
            //Calculates the words score
            int score = CheckScore(user.Word, new BoggleBoard(game.Board));

            using (SqlConnection conn = new SqlConnection(BoggleDB))
            {
                // Connections must be opened
                conn.Open();
                UserInfo ret = new UserInfo();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    
                    using (SqlCommand command =
                    new SqlCommand("insert into Words (GameID, Player, Score, Word) values(@GameID, @Player, @Score, @Word)",
                                    conn,
                                    trans))
                    {
                        command.Parameters.AddWithValue("@GameID", GameInt);
                        command.Parameters.AddWithValue("@Player", user.UserToken);
                        command.Parameters.AddWithValue("@Score", score);
                        command.Parameters.AddWithValue("@Word", user.Word);

                        //attempt query twice
                        if (command.ExecuteNonQuery() == 1)
                        {
                            
                        }
                        else
                        {
                            
                            ret = new UserInfo();
                            ret.HttpStatus = "409 Conflict";
                            return ret;
                        }
                    }
                    trans.Commit();
                }

                ret.HttpStatus = "200 OK";
                ret.Score = score + "";
                return ret;
            }
        }

        /// <summary>
        /// Creates a new user.
        /// </summary>
        public UserInfo CreateUser(UserInfo user)
        {
            UserInfo ret = new UserInfo();
            // This validates the user.Name property.  It is best to do any work that doesn't
            // involve the database before creating the DB connection or after closing it.
            if (user.Nickname == null || user.Nickname.Trim().Length == 0 || user.Nickname.Trim().Length > 50)
            {               
                ret.HttpStatus = "403 Forbidden";
                return ret;
            }

            using (SqlConnection conn = new SqlConnection(BoggleDB))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    using (SqlCommand command =
                        new SqlCommand("insert into Users (UserID, Nickname) values(@UserID, @Nickname)",
                                        conn,
                                        trans))
                    {
                        // We generate the userID to use.                   
                        string userID = Guid.NewGuid().ToString();

                        command.Parameters.AddWithValue("@UserID", userID);
                        command.Parameters.AddWithValue("@Nickname", user.Nickname.Trim());

                        if (command.ExecuteNonQuery() == 1)
                        {
                        }
                        else
                        {                         
                            ret.HttpStatus = "403 Forbidden";
                            return ret;
                        }

                        trans.Commit();
                        UserInfo newuser = new UserInfo();
                        newuser.UserToken = userID;
                        newuser.HttpStatus = "201 Created";
                        return newuser;
                    }
                }
            }
        }

        /// <summary>
        /// Helper method returns a game object from the T-SQL database from the Games table
        /// </summary>
        private GamesDB QueryGame(int GameID)
        {
            using (SqlConnection conn = new SqlConnection(BoggleDB))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    using (SqlCommand command = new SqlCommand("select * from Games where GameID=@GameID", conn, trans))
                    {
                        command.Parameters.AddWithValue("@GameID", GameID);

                        //initialize the reader to read the row selected
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (!reader.HasRows)
                            {
                                return null;
                            }
                            reader.Read();
                            GamesDB game = new GamesDB();
                            game.GameID = GameID;
                            game.Player1 = (string)reader["Player1"];
                            //if game is pending don't set other values
                            if (!(reader["Player2"] is System.DBNull))
                            {
                                game.Player2 = (string)reader["Player2"];
                                game.Board = (string)reader["Board"];
                                game.TimeLimit = (int)reader["TimeLimit"];
                                game.StartTime = (DateTime)reader["StartTime"];
                            }
                            return game;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Helper Method that returns a list word objects played by a specific player in that game
        /// </summary>
        private LinkedList<WordsDB> QueryWord(int GameID, string Player)
        {
            using (SqlConnection conn = new SqlConnection(BoggleDB))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    using (SqlCommand command = new SqlCommand("select * from Words where GameID=@GameID and Player=@Player", conn, trans))
                    {
                        command.Parameters.AddWithValue("@GameID", GameID);
                        command.Parameters.AddWithValue("@Player", Player);
                        //initialize the reader to read the row selected
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            LinkedList<WordsDB> words = new LinkedList<WordsDB>();

                            while (reader.Read())
                            {
                                WordsDB word = new WordsDB();
                                word.ID = (int)reader["Id"];
                                word.Word = (string)reader["Word"];
                                word.GameID = (int)reader["GameID"];
                                word.Player = (string)reader["Player"];
                                word.Score = (int)reader["Score"];
                                words.AddLast(word);
                            }
                            return words;
                        }
                    }
                }
            }
        }
        /// <summary>
        /// returns users nickname or null if invalid userID
        /// </summary>
        private string QueryUser(string UserID)
        {
            using (SqlConnection conn = new SqlConnection(BoggleDB))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    using (SqlCommand command = new SqlCommand("select * from Users where UserID = @UserID", conn, trans))
                    {
                        command.Parameters.AddWithValue("@UserID", UserID);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return (string)reader["Nickname"];
                            }
                            else
                            {
                                return null;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Helper to check score for a given word
        /// </summary>
        private int CheckScore(string word, BoggleBoard board)
        {
            if (word.Length < 3)
            {
                return 0;
            }

            // Illegal word
            if (!validwords.Contains(word) || !board.CanBeFormed(word))
            {
                return -1;
            }

            else if (word.Length > 7)
            {
                return 11;
            }
            else if (word.Length == 7)
            {
                return 5;
            }
            else if (word.Length == 6)
            {
                return 3;
            }
            else if (word.Length == 5)
            {
                return 2;
            }
            // word length 3 or 4
            else {
                return 1;
            }

        }
    }
}

