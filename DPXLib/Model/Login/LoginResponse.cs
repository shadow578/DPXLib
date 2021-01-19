using Newtonsoft.Json;

namespace DPXLib.Model.Login
{
    /// <summary>
    /// response to a <see cref="LoginRequest"/>, containing the auth token to use for further requests
    /// </summary>
    public class LoginResponse
    {
        /// <summary>
        /// Refresh token to refresh the access token without using a username/password
        /// Currently seems to not be implemented :(
        /// </summary>
        [JsonProperty("refreshToken")]
        public string RefreshToken { get; set; }

        /// <summary>
        /// Access token for further requests
        /// </summary>
        [JsonProperty("token")]
        public string Token { get; set; }
    }
}
