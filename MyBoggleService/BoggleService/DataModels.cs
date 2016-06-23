//Written by Zachary Peters u0743528 and Tyler Gardner u0372543 April 2016
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Boggle
{

    // DataContract for inputting and returning JSON
    // MakeUser
    // JoinGame
    // PlayWord
    // CancelJoin
    [DataContract]
    public class UserInfo
    {
        [DataMember(EmitDefaultValue = false)]
        public string Nickname { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string UserToken { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int TimeLimit { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string Word { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string GameID { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string Score { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string HttpStatus { get; set; }
    }

    // Datacontract for returning GameStatus
    [DataContract]
    public class GameStatus
    {
        [DataMember(EmitDefaultValue = false)]
        public string GameState { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string Board { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int TimeLimit { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int? TimeLeft { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public PlayerModel Player1 { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public PlayerModel Player2 { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string HttpStatus { get; set; }

    }
    // Sub-Datacontract for Gamestatus
    [DataContract]
    public class PlayerModel
    {
        [DataMember(EmitDefaultValue = false)]
        public string Nickname { get; set; }
        [DataMember]
        public int Score { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public List<WordPlayed> WordsPlayed { get; set; }
    }
    // Sub-DataContract for Player Model
    [DataContract]
    public class WordPlayed
    {
        [DataMember(EmitDefaultValue = false)]
        public string Word { get; set; }
        [DataMember]
        public int Score { get; set; }
    }
    // DataModel for simplified queries to the GamesDB
    public class GamesDB
    {
        public int GameID { get; set; }
        public string Player1 { get; set; }
        public string Player2 { get; set; }
        public string Board { get; set; }
        public int TimeLimit { get; set; }
        public DateTime StartTime { get; set; }
    }
    // DataModel for simplified queries to the WordsDB
    public class WordsDB
    {
        public int ID { get; set; }
        public string Word { get; set; }
        public int GameID { get; set; }
        public string Player { get; set; }
        public int Score { get; set; }
    }


}