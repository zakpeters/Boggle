using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace Boggle
{
    [ServiceContract]
    public interface IBoggleService
    {
        /// <summary>
        /// Sends back index.html as the response body.
        /// </summary>
        [WebGet(UriTemplate = "/api")]
        Stream API();

        /// <summary>
        /// Creates a user on the service
        /// </summary>
        [WebInvoke(Method = "POST", UriTemplate = "/users")]
        UserInfo CreateUser(UserInfo user);
        
        /// <summary>
        /// Attempts to join a game on the service
        /// </summary>
        [WebInvoke(Method = "POST", UriTemplate = "/games")]
        UserInfo JoinGame(UserInfo user);
        
        /// <summary>
        /// Cancels the a pending join request
        /// </summary>
        [WebInvoke(Method = "PUT", UriTemplate = "/games")]
        UserInfo CancelJoinRequest(UserInfo user);

        /// <summary>
        /// Plays a word
        /// </summary>
        [WebInvoke(Method = "PUT", UriTemplate = "/games/{GameID}")]
        UserInfo PlayWord(string GameID, UserInfo user);
        
        /// <summary>
        /// Gets the games current status
        /// </summary>
        [WebInvoke(Method = "GET", UriTemplate = "/games/{GameID}?Brief={Brief}")]
        GameStatus GameStatus(string GameID, string Brief);     
    }
}
