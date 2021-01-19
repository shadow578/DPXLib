using DPXLib.Model.Common;
using DPXLib.Model.JobInstances;
using DPXLib.Model.License;
using DPXLib.Model.Login;
using DPXLib.Model.Nodes;
using DPXLib.Util;
using Newtonsoft.Json;
using Refit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace DPXLib
{
    /// <summary>
    /// Client to communicate with a DPX Master server above version 4.6.0 (using new REST api)
    /// </summary>
    public class DPXClient
    {
        /// <summary>
        /// a event invoked when a api error occurs (unauthentificated)
        /// 
        /// P1 (ApiException): the exeption thrown by the api
        /// R  (bool)        : should the call be retired? if false, the call is aborted and the exeption is thrown
        /// </summary>
        public event Func<ApiException, bool> DPXApiError;

        /// <summary>
        /// The dpx host this clients queries
        /// </summary>
        public string DPXHost { get; set; }

        /// <summary>
        /// the username of the user logged in currently
        /// </summary>
        public string LoggedInUser { get; set; }

        /// <summary>
        /// Dpx api instance using ReFit
        /// </summary>
        readonly DPXApi dpx;

        /// <summary>
        /// current auth token, from the last successfull login.
        /// raw auth token!
        /// </summary>
        string rawToken;

        /// <summary>
        /// current bearer token string, from the last successfull login
        /// </summary>
        string Token
        {
            get
            {
                return "Bearer " + rawToken;
            }
        }

        /// <summary>
        /// initialize the dpx client
        /// </summary>
        /// <param name="host">the hostname of the dpx master server (ex. http://dpx-master.local)</param>
        /// <param name="logRequests">if true, requests and responses are logged using <see cref="HttpLoggingHandler"/></param>
        public DPXClient(string host, bool logRequests = false)
        {
            //set json settings
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings()
            {
                //Date&Time Settings: Use ISO Date format without milliseconds, always convert times to UTC
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                //DateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm.fff':'ss'Z'"
            };

            //init dpx api
            if (logRequests)
            {
                //init logger to write to file
                HttpLoggingHandler logger = new HttpLoggingHandler()
                {
                    LogWriter = File.CreateText(DateTime.Now.ToString("yyyy.MM.dd-HH.mm.ss.FF") + "_dpx-communication.log")
                };

                //init ReFit to use logger
                HttpClient httpClient = new HttpClient(logger)
                {
                    BaseAddress = new Uri(host),
                };
                dpx = RestService.For<DPXApi>(httpClient);
            }
            else
            {
                dpx = RestService.For<DPXApi>(host);
            }

            //set host
            DPXHost = host;
        }

        /// <summary>
        /// Log into the dpx server
        /// Call this BEFORE using any other function
        /// </summary>
        /// <param name="username">the username to use for login</param>
        /// <param name="password">the (cleartext) password to use for login</param>
        /// <returns>was login successfull?</returns>
        public async Task<bool> LoginAsync(string username, string password)
        {
            //check state first
            ThrowIfInvalidState(false);

            return await TryAndRetry(async () =>
            {
                //call api
                LoginResponse response = await dpx.Login(new LoginRequest()
                {
                    Username = username,
                    Password = password
                });

                //check response is ok
                if (response == null || string.IsNullOrWhiteSpace(response.Token))
                    return false;

                //login ok
                LoggedInUser = username;
                rawToken = response.Token;
                return true;
            });
        }

        /// <summary>
        /// Get information about the master server license
        /// </summary>
        /// <returns>the license information</returns>
        public async Task<LicenseResponse> GetLicenseInfoAsync()
        {
            //check state is valid and authentificated
            ThrowIfInvalidState();

            return await TryAndRetry(async () =>
            {
                //send request
                LicenseResponse response = await dpx.GetLicense(Token);
                response.SourceClient = this;
                return response;
            });
        }

        /// <summary>
        /// Get a list of job instances that match the given filters
        /// </summary>
        /// <param name="filters">the filters to use.</param>
        /// <returns>the list of found job instances</returns>
        public async Task<JobInstance[]> GetJobInstancesAsync(params FilterItem[] filters)
        {
            //check state
            ThrowIfInvalidState();

            return await TryAndRetry(async () =>
            {
                //send request
                JobInstance[] jobs = await dpx.GetJobInstances(Token, JSONSerialize(filters));

                //set client reference in each job
                foreach (JobInstance job in jobs)
                    job.SourceClient = this;

                return jobs;
            });
        }

        /// <summary>
        /// Get a specific job instance by its instance ID
        /// </summary>
        /// <param name="jobInstanceID">the job instance to get</param>
        /// <returns>the job with the given instance id. This may be null if the job does not exist</returns>
        public async Task<JobInstance> GetJobInstanceAsync(long jobInstanceID)
        {
            //check state
            ThrowIfInvalidState();

            return await TryAndRetry(async () =>
            {
                //send request
                JobInstance job = await dpx.GetJobInstance(Token, jobInstanceID);

                //set client reference in job
                job.SourceClient = this;

                return job;
            });
        }

        /// <summary>
        /// get logs of the job instance with the given id
        /// </summary>
        /// <param name="jobInstanceID">the job instance to get logs of</param>
        /// <param name="startIndex">the index of the first log entry to get</param>
        /// <param name="count">how many entries to get</param>
        /// <param name="filters">filters to apply to the logs. WARNING: this is more inofficial functionality</param>
        /// <returns>the list of log entries found</returns>
        public async Task<InstanceLogEntry[]> GetJobInstanceLogsAsync(long jobInstanceID, long startIndex = 0, long count = 500, params FilterItem[] filters)
        {
            //check state
            ThrowIfInvalidState();

            return await TryAndRetry(async () =>
            {
                //get logs
                InstanceLogEntry[] logs = await dpx.GetLogEntries(Token, jobInstanceID, startIndex, count,
                                                                    filters.Length == 0 ? null : JSONSerialize(filters));
                foreach (InstanceLogEntry log in logs)
                    log.SourceClient = this;

                return logs;
            });
        }

        /// <summary>
        /// Get all logs of the job instance with the given id
        /// </summary>
        /// <param name="jobInstanceID">the job instance to get logs of</param>
        /// <param name="batchSize">how many logs to load at once</param>
        /// <param name="timeout">timeout to get job logs, in milliseconds. if the timeout is <= 0, no timeout is used</param>
        /// <param name="filters">filters to apply to the logs. WARNING: this is more inofficial functionality</param>
        /// <returns>the list of all log entries found</returns>
        public async Task<InstanceLogEntry[]> GetAllJobInstanceLogsAsync(long jobInstanceID, long batchSize = 500, long timeout = -1, params FilterItem[] filters)
        {
            //check state
            ThrowIfInvalidState();

            //prepare stopwatch for timeout
            Stopwatch timeoutWatch = new Stopwatch();
            timeoutWatch.Start();

            //get all logs, 500 at a time
            List<InstanceLogEntry> logs = new List<InstanceLogEntry>();
            InstanceLogEntry[] currentBatch;
            do
            {
                //get job batch
                currentBatch = await GetJobInstanceLogsAsync(jobInstanceID, logs.Count, batchSize, filters);
                logs.AddRange(currentBatch);

                //check timeout
                if (timeout > 0 && timeoutWatch.ElapsedMilliseconds >= timeout)
                    break;
            } while (currentBatch.Length >= batchSize);
            timeoutWatch.Stop();
            return logs.ToArray();
        }

        /// <summary>
        /// Get the job status info from a status url
        /// </summary>
        /// <param name="statusURL">the status url</param>
        /// <returns>the status info</returns>
        public async Task<JobStatusInfo> GetStatusInfoAsync(string statusURL)
        {
            //check state
            ThrowIfInvalidState();

            //parse status string from url (last path segment)
            if (!Uri.TryCreate(statusURL, UriKind.Absolute, out Uri result))
                return null;

            string statusSegment = result.Segments.Last();
            return await TryAndRetry(async () =>
            {
                //invoke api
                return await dpx.GetStatusInfo(statusSegment);
            });
        }

        /// <summary>
        /// get node group information for a named node group
        /// </summary>
        /// <param name="nodeGroupName">the name of the node group to get info of</param>
        /// <returns>info about the node group</returns>
        public async Task<NodeGroup> GetNodeGroupAsync(string nodeGroupName)
        {
            //check state
            ThrowIfInvalidState();

            return await TryAndRetry(async () =>
            {
                //query node group
                NodeGroup group = await dpx.GetNodeGroup(Token, nodeGroupName);

                //set client reference in group
                if (group != null)
                    group.SourceClient = this;

                return group;
            });
        }

        /// <summary>
        /// Get a list of all node groups
        /// </summary>
        /// <returns>a list of all node groups</returns>
        public async Task<NodeGroup[]> GetNodeGroupsAsync()
        {
            //check state
            ThrowIfInvalidState();

            return await TryAndRetry(async () =>
            {
                //query node groups
                NodeGroup[] groups = await dpx.GetNodeGroups(Token);

                //set client reference in all groups
                foreach (NodeGroup group in groups)
                    group.SourceClient = this;

                return groups;
            });
        }

        /// <summary>
        /// get all nodes matching the criteria
        /// if no criteria are given, a list of all nodes is returned
        /// </summary>
        /// <param name="nodeGroup">the node group nodes must be in (optional)</param>
        /// <param name="nodeType">the node type the nodes must have (optional)</param>
        /// <returns>a list of nodes matching the criteria</returns>
        public async Task<Node[]> GetNodesAsync(string nodeGroup = null, string nodeType = null)
        {
            //check state
            ThrowIfInvalidState();

            return await TryAndRetry(async () =>
            {
                //get nodes
                Node[] nodes = await dpx.GetNodes(Token, nodeGroup, nodeType);

                //set client reference in all nodes
                foreach (Node node in nodes)
                    node.SourceClient = this;

                return nodes;
            });
        }

        /// <summary>
        /// throw a exception if the dpx api instance or auth token are not in a valid state
        /// </summary>
        /// <param name="needAuth">if false, state of auth token is not checked</param>
        void ThrowIfInvalidState(bool needAuth = true)
        {
            //check dpx api
            if (dpx == null)
                throw new InvalidOperationException("dpx api was not initialized!");

            //check access token
            if (needAuth && string.IsNullOrWhiteSpace(rawToken))
                throw new InvalidOperationException("auth token is not set! use DPXClient.Login first.");
        }

        /// <summary>
        /// Serialize a object into a json string using <see cref="JsonConvert"/>
        /// </summary>
        /// <typeparam name="T">the type to convert</typeparam>
        /// <param name="obj">the object to convert</param>
        /// <returns>the json string</returns>
        string JSONSerialize<T>(T obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        /// <summary>
        /// try a api call, catch ApiExceptions and use DPXApiError event to handle them
        /// </summary>
        /// <typeparam name="T">the return type of the function</typeparam>
        /// <param name="func">the function to try</param>
        /// <returns>the return value of the function call</returns>
        async Task<T> TryAndRetry<T>(Func<Task<T>> func)
        {
            while (true)
            {
                try
                {
                    return await func.Invoke();
                }
                catch (ApiException e)
                {
                    if (DPXApiError == null
                        || !DPXApiError.Invoke(e))
                        throw e; // re- throw the exeption and dont retry if handler failed
                    else
                        continue; //retry call
                }
            }
        }
    }
}
