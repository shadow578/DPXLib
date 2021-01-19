using Newtonsoft.Json;
using System;

namespace DPXLib.Model.JobInstances
{
    /// <summary>
    /// a entry in the log of a <see cref="JobInstance"/>
    /// </summary>
    public class InstanceLogEntry
    {
        /// <summary>
        /// The client that created this object
        /// </summary>
        [JsonIgnore]
        public DPXClient SourceClient { get; internal set; }

        /// <summary>
        /// Time this log entry was issued
        /// </summary>
        [JsonProperty("time")]
        public DateTime Time { get; set; }

        /// <summary>
        /// module that issued this entry
        /// </summary>
        [JsonProperty("module")]
        public string Module { get; set; }

        /// <summary>
        /// the message of this entry
        /// </summary>
        [JsonProperty("message")]
        public string Message { get; set; }

        /// <summary>
        /// the ip of the node this entry came from
        /// </summary>
        [JsonProperty("source_ip")]
        public string SourceIP { get; set; }

        /// <summary>
        /// log message code, for search in DPX Knowledge Base
        /// </summary>
        [JsonProperty("message_code")]
        public string MessageCode { get; set; }

        /// <summary>
        /// offset, in bytes (?)
        /// </summary>
        [JsonProperty("offset")]
        [Obsolete("Unknown Usage")]
        public long Offset { get; set; }

        /// <summary>
        /// log_size, in bytes (?)
        /// </summary>
        [JsonProperty("log_size")]
        [Obsolete("Unknown Usage")]
        public long LogSize { get; set; }

        /// <summary>
        /// get the header for <see cref="ToString"/>
        /// </summary>
        /// <returns>header string</returns>
        public static string GetHeader()
        {
            return "Source IP\t| Time\t\t| Module\t| Message Code\t| Message";
        }

        /// <summary>
        /// convert the log entry into a string, with equal spacing to <see cref="GetHeader"/> 
        /// </summary>
        /// <returns>string of the log entry</returns>
        public override string ToString()
        {
            return $"{SourceIP}\t| {Time}\t\t| {Module}\t| {MessageCode}\t| {Message}";
        }
    }
}
