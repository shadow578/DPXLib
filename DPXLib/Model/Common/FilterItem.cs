using DPXLib.Model.Constants;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace DPXLib.Model.Common
{
    /// <summary>
    /// operation of a filter expression
    /// </summary>
    public class FilterItem
    {
        /// <summary>
        /// the value that should be filtered
        /// JSON: var
        /// </summary>
        [JsonProperty("var")]
        public string Target { get; set; }

        /// <summary>
        /// The filter operation
        /// JSON: op
        /// </summary>
        [JsonProperty("op")]
        public string Operation { get; set; }

        /// <summary>
        /// The value to filter for
        /// JSON: val
        /// </summary>
        [JsonProperty("val")]
        public string[] Value { get; set; }

        #region Filter Helpers
        /// <summary>
        /// creates a filter item that sets the report_start_time
        /// </summary>
        /// <param name="time">the time to start the report at</param>
        /// <returns>a filter item</returns>
        public static FilterItem ReportStart(DateTime time)
        {
            return new FilterItem
            {
                Target = "report_start_time",
                Operation = "=",
                Value = new string[]
                {
                    JsonConvert.SerializeObject(time).Trim('\"')
                }
            };
        }

        /// <summary>
        /// creates a filter item that sets the report_end_time
        /// </summary>
        /// <param name="time">the time to end the report at</param>
        /// <returns>a filter item</returns>
        public static FilterItem ReportEnd(DateTime time)
        {
            return new FilterItem
            {
                Target = "report_end_time",
                Operation = "=",
                Value = new string[]
                {
                    JsonConvert.SerializeObject(time).Trim('\"')
                }
            };
        }

        /// <summary>
        /// creates a filter item that filters for jobs with the given name, setting job_name
        /// </summary>
        /// <param name="name">the name to filter</param>
        /// <returns>a filter item</returns>
        public static FilterItem JobNameIs(params string[] name)
        {
            return new FilterItem
            {
                Target = "job_name",
                Operation = name.Length > 1 ? "in" : "=",
                Value = name
            };
        }

        /// <summary>
        /// creates a filter item that filters for jobs that dont have the given name, setting job_name
        /// </summary>
        /// <param name="name">the name to filter</param>
        /// <returns>a filter item</returns>
        public static FilterItem JobNameIsNot(params string[] name)
        {
            return new FilterItem
            {
                Target = "job_name",
                Operation = "!=",
                Value = name
            };
        }

        /// <summary>
        /// cretes a filter item that filters for jobs whose name contain the given string, setting job_name
        /// </summary>
        /// <param name="name">the name to filter</param>
        /// <returns>a filter item</returns>
        public static FilterItem JobNameContains(params string[] name)
        {
            return new FilterItem
            {
                Target = "job_name",
                Operation = "in",
                Value = name
            };
        }

        /// <summary>
        /// creates a filter that filters for jobs with the given job status, setting job_instance_status_name
        /// </summary>
        /// <param name="status">the name to filter</param>
        /// <returns>a filter item</returns>
        public static FilterItem JobStatus(params JobStatus[] status)
        {
            List<string> statusStr = new List<string>();
            foreach (JobStatus s in status)
                if (!statusStr.Contains(s.ToString()))
                    statusStr.Add(s.ToString());

            return new FilterItem
            {
                Target = "job_instance_status_name",
                Operation = statusStr.Count > 1 ? "in" : "=",
                Value = statusStr.ToArray()
            };
        }
        #endregion
    }
}
