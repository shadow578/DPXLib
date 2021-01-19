using Newtonsoft.Json;

namespace DPXLib.Model.Login
{
    /// <summary>
    /// Request to log in. Endpoint responds with a <see cref="LoginResponse"/>
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// The username to use for login
        /// </summary>
        [JsonProperty("username")]
        public string Username { get; set; }

        /// <summary>
        /// The passwort to use for login, in cleartext
        /// </summary>
        [JsonProperty("password")]
        public string Password {get; set;}
    }
}
