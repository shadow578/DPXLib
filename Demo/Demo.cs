using DPXLib;
using DPXLib.Extension;
using DPXLib.Model.Common;
using DPXLib.Model.Constants;
using DPXLib.Model.JobInstances;
using DPXLib.Model.License;
using DPXLib.Model.Nodes;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace DPXTool
{
    /// <summary>
    /// contains early usage demos and testts for the DPXClient class.
    /// Just staying here as a reference, may be removed later :P
    /// </summary>
    static class Demos
    {
        /// <summary>
        /// global dpx client for running demos
        /// </summary>
        static DPXClient client;

        public static void Main(string[] args)
        {
            RunDemos().GetAwaiter().GetResult();
        }

        static async Task RunDemos()
        {
            //get info from user
            Console.Write("enter host name (http://dpx-example.local): ");
            string host = Console.ReadLine();
            Console.Write("enter username: ");
            string user = Console.ReadLine();
            Console.Write($"{user}@{host}'s password: ");
            string pw = Console.ReadLine();

            //init and login
            client = new DPXClient(host, true);
            bool loginOk = await client.LoginAsync(user, pw);
            if (loginOk)
                Console.WriteLine("login ok");
            else
            {
                Console.WriteLine("login failed");
                return;
            }

            //demos
            //await DemoLicense();
            //await DemoJobs();
            //await DemoLogs();
            //await DemoVolsers();
            //await DemoNodes();
            //await DemoBackupSize();
            //await DemoLogsCached();
            //await DemoJobTimings();
            await DemoUniqueJob();


            Console.WriteLine("demo end");
            Console.ReadLine();
        }

        /// <summary>
        /// demo for GetLicenseInfoAsync()
        /// </summary>
        static async Task DemoLicense()
        {
            //get license
            LicenseResponse lic = await client.GetLicenseInfoAsync();

            //check if valid response
            if (lic == null)
            {
                Console.WriteLine("Server returned no license info");
                return;
            }

            //print license info
            Console.WriteLine(@$"
Info for {lic.ServerHostName}:
DPX Version: {lic.DPXVersion} ({lic.DPXBuildDate} {lic.DPXBuildTime})
Node:        {lic.ServerNodeName} ({lic.ServerNodeAddress})
Eval:        {(lic.IsEvalLicanse ? "Yes" : "No")}
Expires in:  {lic.ExpiresInDays} days

Licensed:");

            foreach (LicenseCategory cat in lic.LicenseCategories)
            {
                ConsoleWriteLineC(@$"   {cat.Name}:  {cat.Consumed} of {cat.Licensed}", ConsoleColor.Red, cat.IsLicenseViolated);
            }
        }

        /// <summary>
        /// demo for GetJobInstances()
        /// </summary>
        static async Task DemoJobs()
        {
            //get jobs
            JobInstance[] jobs = await client.GetJobInstancesAsync(FilterItem.ReportStart(DateTime.Now.Subtract(TimeSpan.FromDays(2))));

            Console.WriteLine($"Found total of {jobs.Length} jobs in the last 2 Days:");
            foreach (JobInstance job in jobs)
            {
                Console.WriteLine(@$"   {job.DisplayName}");
            }
        }

        /// <summary>
        /// demo for GetJobInstance()
        /// </summary>
        static async Task DemoUniqueJob()
        {
            //get id of job to get
            Console.Write("Enter job instance id: ");
            long jobId = Convert.ToInt64(Console.ReadLine());

            //get job
            JobInstance job = await client.GetJobInstanceAsync(jobId);

            //print job details
            if (job == null)
                Console.WriteLine("Job not found");
            else
                Console.WriteLine($"  {job.DisplayName} with ID {job.ID}");
        }

        /// <summary>
        /// demo for GetJobInstanceLogs()
        /// </summary>
        static async Task DemoLogs()
        {
            //get id of job to get
            Console.Write("Enter job instance id: ");
            long jobId = Convert.ToInt64(Console.ReadLine());

            //Get logs for job 
            InstanceLogEntry[] logs = await client.GetAllJobInstanceLogsAsync(jobId);

            Console.WriteLine($"found total of {logs.Length} logs:");
            Console.WriteLine(InstanceLogEntry.GetHeader());
            foreach (InstanceLogEntry log in logs)
                Console.WriteLine(log);

        }

        /// <summary>
        /// demo for GetJobInstanceLogs() with caching
        /// </summary>
        static async Task DemoLogsCached()
        {
            //get name of job to get
            Console.Write("Enter job name: ");
            string jobName = Console.ReadLine();

            //get jobs, select first
            JobInstance[] jobs = await client.GetJobInstancesAsync(FilterItem.JobNameIs(jobName),
                FilterItem.ReportStart(DateTime.Now.Subtract(TimeSpan.FromDays(2))),
                FilterItem.ReportEnd(DateTime.Now));

            JobInstance job = jobs[0];
            Stopwatch sw = new Stopwatch();
            Console.WriteLine($"running logs demo for job {job.DisplayName} ({job.ID})");

            //Get logs for job the first time
            Console.WriteLine("get all logs (first run; uncached)");
            sw.Restart();
            InstanceLogEntry[] logs = await job.GetLogEntriesAsync(getAllLogs: true);
            sw.Stop();

            Console.WriteLine($"found total of {logs.Length} logs in {sw.ElapsedMilliseconds} ms (1st; not cached)");

            // get logs the second time (cached)
            sw.Restart();
            logs = await job.GetLogEntriesAsync(getAllLogs: true);
            sw.Stop();

            Console.WriteLine($"found total of {logs.Length} logs in {sw.ElapsedMilliseconds} ms (2nd; cached)");
        }

        /// <summary>
        /// demo for GetVolsersUsed()
        /// </summary>
        /// <returns></returns>
        static async Task DemoVolsers()
        {
            //get name of job to get
            Console.Write("Enter job name: ");
            string jobName = Console.ReadLine();

            //get jobs
            JobInstance[] jobs = await client.GetJobInstancesAsync(FilterItem.JobNameIs(jobName),
                FilterItem.ReportStart(DateTime.Now.Subtract(TimeSpan.FromDays(2))),
                FilterItem.ReportEnd(DateTime.Now));

            //list volsers for each found job
            Console.WriteLine($"Found {jobs.Length} jobs");
            foreach (JobInstance job in jobs)
            {
                if (job.RunType != JobRunType.BASE)
                    Console.WriteLine("not base job!");

                Console.WriteLine($"Volsers used by job {job.DisplayName}:");
                string[] volsers = await job.GetVolsersUsed();
                if (volsers == null)
                    Console.WriteLine("  none");
                else
                    foreach (string volser in volsers)
                        Console.WriteLine($"  {volser}");
            }
        }

        /// <summary>
        /// demo for GetBackupSize()
        /// </summary>
        static async Task DemoBackupSize()
        {
            //get name of job to get
            Console.Write("Enter job name: ");
            string jobName = Console.ReadLine();

            //get jobs
            JobInstance[] jobs = await client.GetJobInstancesAsync(FilterItem.JobNameIs(jobName),
                FilterItem.ReportStart(DateTime.Now.Subtract(TimeSpan.FromDays(32))),
                FilterItem.ReportEnd(DateTime.Now));

            //list backup size for each found job
            Console.WriteLine($"Found {jobs.Length} jobs");
            foreach (JobInstance job in jobs)
            {
                Console.WriteLine($"Backup size info of job {job.DisplayName}:");
                JobSizeInfo size = await job.GetBackupSizeAsync();
                if (size == null)
                    Console.WriteLine("  no info available");
                else
                    Console.WriteLine($"  Total Data: {size.TotalDataBackedUp}; Total on Media: {size.TotalDataOnMedia}");
            }

        }

        /// <summary>
        /// demo for GetJobTimings()
        /// </summary>
        static async Task DemoJobTimings()
        {
            const string FMT = @"hh\:mm\:ss";

            //get name of job to get
            Console.Write("Enter job name: ");
            string jobName = Console.ReadLine();

            //get jobs
            JobInstance[] jobs = await client.GetJobInstancesAsync(FilterItem.JobNameIs(jobName),
                FilterItem.ReportStart(DateTime.Now.Subtract(TimeSpan.FromDays(32))),
                FilterItem.ReportEnd(DateTime.Now));

            // query and print times for every job that completed
            foreach (JobInstance job in jobs.Where(j => j.GetStatus() == JobStatus.Completed))
            {
                // query timing infos
                JobTimeInfo timeSpend = await job.GetJobTimingsAsync();

                // print times
                Console.WriteLine($@"
Time Summary of job {job.DisplayName} ({job.ID}):
 Total: {timeSpend.Total.ToString(FMT)}
 Init: {timeSpend.Initializing.ToString(FMT)}
 Waiting: {timeSpend.Initializing.ToString(FMT)}
 Preprocess: {timeSpend.Preprocessing.ToString(FMT)}
 Transfer: {timeSpend.Transferring.ToString(FMT)}
");
            }

        }

        /// <summary>
        /// demo for GetNodes() and GetNodeGroups()
        /// </summary>
        /// <returns></returns>
        static async Task DemoNodes()
        {
            //get all node groups
            NodeGroup[] groups = await client.GetNodeGroupsAsync();

            //print node grups
            Console.WriteLine($"found {groups.Length} node groups:");
            foreach (NodeGroup group in groups)
                Console.WriteLine($" {group.Name} - created by {group.Creator}");

            //get all nodes
            Node[] nodes = await client.GetNodesAsync();

            //print nodes
            Console.WriteLine($"found {nodes.Length} nodes:");
            foreach (Node node in nodes)
                Console.WriteLine($" {node.Name} in group {node.GroupName} running {node.OSDisplayName}");

            //get nodes in group
            Console.Write("enter node group name: ");
            string targetGroup = Console.ReadLine();
            Node[] targetedNodes = await client.GetNodesAsync(nodeGroup: targetGroup);

            //print nodes
            Console.WriteLine($"found {targetedNodes.Length} nodes:");
            foreach (Node node in targetedNodes)
                Console.WriteLine($" {node.Name} in group {node.GroupName} running {node.OSDisplayName}");
        }

        static void ConsoleWriteLineC(string msg, ConsoleColor fgc, bool useFGC)
        {
            ConsoleColor orgColor = Console.ForegroundColor;
            if (useFGC)
                Console.ForegroundColor = fgc;

            Console.WriteLine(msg);

            if (useFGC)
                Console.ForegroundColor = orgColor;
        }
    }
}
