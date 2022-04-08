using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Konbini.RfidFridge.Deployment
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Build();
        }

        private void Build()
        {
            // HW Controller
            var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
            var hwControllerProperty = new Dictionary<string, string>
            {
                { "Configuration", "Debug" },
                { "Platform", "AnyCPU" },
            };

            var hwSolutionFile = @"D:\CODE\StandardCloud\V2\Konbi.MachineBrain\Devices\MagicBox\Konbini.RfidFridge\Konbini.RfidFridge.HwController\Konbini.RfidFridge.HwController.csproj";

            //var result = BuildSolution(hwSolutionFile, hwControllerProperty);
            //var a = result.OverallResult;
            //BuildByMsBuild(hwSolutionFile);
            //Test(hwSolutionFile);
            //BuildByMsBuild(hwSolutionFile);
            Publish(@"D:\CODE\StandardCloud\V2\Konbi.MachineBrain\MachineAdmin\aspnet-core\src\KonbiCloud.Web.Host\KonbiCloud.Web.Host.csproj");
        }

        private BuildResult BuildSolution(string solutionFile, Dictionary<string, string> globalProperty)
        {
            string projectFileName = solutionFile;

            ProjectCollection pc = new ProjectCollection();
            BuildRequestData BuidlRequest = new BuildRequestData(projectFileName, globalProperty, null, new string[] { "Build" }, null);
            BuildResult buildResult = BuildManager.DefaultBuildManager.Build(new BuildParameters(pc), BuidlRequest);
            return buildResult;
        }

        public void Test(string file)
        {
            string projectFileName = file;
            BasicLogger Logger = new BasicLogger();
            var projectCollection = new ProjectCollection();

            var buildParamters = new BuildParameters(projectCollection)
            {
                Loggers = new List<ILogger>() { Logger }
            };
            var globalProperty = new Dictionary<String, String>();
            globalProperty.Add("Configuration", "Debug");
            globalProperty.Add("Platform", "AnyCPU");

            BuildManager.DefaultBuildManager.ResetCaches();
            var buildRequest = new BuildRequestData(projectFileName, globalProperty, null, new String[] { "Build" }, null);
            var buildResult = BuildManager.DefaultBuildManager.Build(buildParamters, buildRequest);

            MessageBox.Show(buildResult.OverallResult.ToString());
            if (buildResult.OverallResult == BuildResultCode.Failure)
            {
                MessageBox.Show(Logger.GetLogString());
            }
        }

        public void Publish(string file)
        {
            string projectFileName = file;
            BasicLogger Logger = new BasicLogger();
            var projectCollection = new ProjectCollection();

            var buildParamters = new BuildParameters(projectCollection)
            {
                Loggers = new List<ILogger>() { Logger }
            };
            var globalProperty = new Dictionary<String, String>
            {
                { "Configuration", "Release" },
            };

            BuildManager.DefaultBuildManager.ResetCaches();
            var buildRequest = new BuildRequestData(projectFileName, globalProperty, null, new String[] { "Publish" }, null);
            var buildResult = BuildManager.DefaultBuildManager.Build(buildParamters, buildRequest);

            MessageBox.Show(buildResult.OverallResult.ToString());
            if (buildResult.OverallResult == BuildResultCode.Failure)
            {
                MessageBox.Show(Logger.GetLogString());
            }
        }

        public void BuildByMsBuild(string csProjectFile)
        {
            //C:\\Windows\\Microsoft.NET\\Framework64\\v4.0.30319\\
            var path = @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe";
            var cmd = $"{path} \"{csProjectFile}\" -property:Configuration=Debug";
            System.Diagnostics.ProcessStartInfo psi =
               new System.Diagnostics.ProcessStartInfo(cmd)
               {
                   RedirectStandardOutput = true,
                   //WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                   UseShellExecute = false
               };
            System.Diagnostics.Process proc = System.Diagnostics.Process.Start(psi); ;
            System.IO.StreamReader myOutput = proc.StandardOutput;
            proc.WaitForExit(2000);
            if (proc.HasExited)
            {
                string output = myOutput.ReadToEnd();
                Console.WriteLine(output);
            }
        }
    }


    public class BasicLogger : Logger
    {
        MemoryStream streamMem = new MemoryStream();
        /// <summary>
        /// Initialize is guaranteed to be called by MSBuild at the start of the build
        /// before any events are raised.
        /// </summary>
        public override void Initialize(IEventSource eventSource)
        {

            try
            {
                // Open the file
                this.streamWriter = new StreamWriter(streamMem);
                //this.streamWriter = new StreamWriter(logFile);
            }
            catch (Exception ex)
            {
                if
                (
                    ex is UnauthorizedAccessException
                    || ex is ArgumentNullException
                    || ex is PathTooLongException
                    || ex is DirectoryNotFoundException
                    || ex is NotSupportedException
                    || ex is ArgumentException
                    || ex is SecurityException
                    || ex is IOException
                )
                {
                    throw new LoggerException("Failed to create log file: " + ex.Message);
                }
                else
                {
                    // Unexpected failure
                    throw;
                }
            }

            // For brevity, we'll only register for certain event types. Loggers can also
            // register to handle TargetStarted/Finished and other events.
            eventSource.ProjectStarted += new ProjectStartedEventHandler(eventSource_ProjectStarted);
            eventSource.TaskStarted += new TaskStartedEventHandler(eventSource_TaskStarted);
            eventSource.MessageRaised += new BuildMessageEventHandler(eventSource_MessageRaised);
            eventSource.WarningRaised += new BuildWarningEventHandler(eventSource_WarningRaised);
            eventSource.ErrorRaised += new BuildErrorEventHandler(eventSource_ErrorRaised);
            eventSource.ProjectFinished += new ProjectFinishedEventHandler(eventSource_ProjectFinished);
        }

        void eventSource_ErrorRaised(object sender, BuildErrorEventArgs e)
        {
            // BuildErrorEventArgs adds LineNumber, ColumnNumber, File, amongst other parameters
            string line = String.Format(": ERROR {0}({1},{2}): ", e.File, e.LineNumber, e.ColumnNumber);
            WriteLineWithSenderAndMessage(line, e);
        }

        void eventSource_WarningRaised(object sender, BuildWarningEventArgs e)
        {
            // BuildWarningEventArgs adds LineNumber, ColumnNumber, File, amongst other parameters
            string line = String.Format(": Warning {0}({1},{2}): ", e.File, e.LineNumber, e.ColumnNumber);
            WriteLineWithSenderAndMessage(line, e);
        }

        void eventSource_MessageRaised(object sender, BuildMessageEventArgs e)
        {
            // BuildMessageEventArgs adds Importance to BuildEventArgs
            // Let's take account of the verbosity setting we've been passed in deciding whether to log the message
            if ((e.Importance == MessageImportance.High && IsVerbosityAtLeast(LoggerVerbosity.Minimal))
                || (e.Importance == MessageImportance.Normal && IsVerbosityAtLeast(LoggerVerbosity.Normal))
                || (e.Importance == MessageImportance.Low && IsVerbosityAtLeast(LoggerVerbosity.Detailed))
            )
            {
                WriteLineWithSenderAndMessage(String.Empty, e);
            }
        }

        void eventSource_TaskStarted(object sender, TaskStartedEventArgs e)
        {
            // TaskStartedEventArgs adds ProjectFile, TaskFile, TaskName
            // To keep this log clean, this logger will ignore these events.
        }

        void eventSource_ProjectStarted(object sender, ProjectStartedEventArgs e)
        {
            // ProjectStartedEventArgs adds ProjectFile, TargetNames
            // Just the regular message string is good enough here, so just display that.
            WriteLine(String.Empty, e);
            indent++;
        }

        void eventSource_ProjectFinished(object sender, ProjectFinishedEventArgs e)
        {
            // The regular message string is good enough here too.
            indent--;
            WriteLine(String.Empty, e);
        }

        /// <summary>
        /// Write a line to the log, adding the SenderName and Message
        /// (these parameters are on all MSBuild event argument objects)
        /// </summary>
        private void WriteLineWithSenderAndMessage(string line, BuildEventArgs e)
        {
            if (0 == String.Compare(e.SenderName, "MSBuild", true /*ignore case*/))
            {
                // Well, if the sender name is MSBuild, let's leave it out for prettiness
                WriteLine(line, e);
            }
            else
            {
                WriteLine(e.SenderName + ": " + line, e);
            }
        }

        /// <summary>
        /// Just write a line to the log
        /// </summary>
        private void WriteLine(string line, BuildEventArgs e)
        {
            for (int i = indent; i > 0; i--)
            {
                streamWriter.Write("\t");
            }
            streamWriter.WriteLine(line + e.Message);
        }


        public string GetLogString()
        {
            var sr = new StreamReader(streamMem);
            var myStr = sr.ReadToEnd();
            return myStr;
        }
        /// <summary>
        /// Shutdown() is guaranteed to be called by MSBuild at the end of the build, after all 
        /// events have been raised.
        /// </summary>
        /// 
        /// 
        public override void Shutdown()
        {
            streamWriter.Flush();
            streamMem.Position = 0;
        }
        private StreamWriter streamWriter;
        private int indent;
    }
}
