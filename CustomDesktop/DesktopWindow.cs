using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CustomDesktop.Properties;
using Qlik.Engine;
using Qlik.Engine.Extensions;
using Qlik.Sense.Client;
using Qlik.Sense.Client.Visualizations;

namespace CustomDesktop
{
    /// <summary>
    /// This example illustrates one way of creating a custom desktop that launches Qlik Sense as a
    /// background process and provides a simple custom GUI for interacting with it. More information
    /// and more detailed explanations of the concepts used in the example can be found on the developer
    /// help site at http://help.qlik.com , Building applications with the .NET SDK -> Download code samples
    /// -> Custom Desktop.
    /// </summary>
    public partial class DesktopWindow : Form
    {
        private readonly IApp _theApp;
        private IApp TheApp { get { return _theApp; } }

        private readonly ILocation _theLocation;
        private ILocation TheLocation { get { return _theLocation; } }

        private static Process TheProcess { get; set; }

        /// <summary>
        /// Prepare desktop by launching Qlik Sense and adding the desktop app to the hub.
        /// </summary>
        public DesktopWindow()
        {
            InitializeComponent();
            _theLocation = LaunchEngine();
            _theApp = PrepareApp(TheLocation);
        }

        /// <summary>
        /// The name of the app used for the desktop application.
        /// </summary>
        public const string CustomDesktopAppName = "CustomDesktopApp";

        /// <summary>
        /// Assume default location for the app directory. Default location is:
        /// C:\Users\&lt;usr&gt;\Documents\Qlik\Sense\Apps\
        /// </summary>
        private static string DesktopAppPath
        {
            get
            {
                var userDocumentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                return Path.Combine(userDocumentsPath, @"Qlik\Sense\Apps\" + CustomDesktopAppName + ".qvf");
            }
        }

        /// <summary>
        /// Assume default location for the Qlik Sense Desktop executable. Default location is:
        /// C:\Users\&lt;usr&gt;\AppData\Local\Programs\Qlik\Sense\QlikSense.exe
        /// </summary>
        private static string PathToQlikSense
        {
            get
            {
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(appDataPath, @"Programs\Qlik\Sense\QlikSense.exe");
            }
        }

        /// <summary>
        /// Launch Qlik Sense and open a connection.
        /// </summary>
        /// <returns></returns>
        private static ILocation LaunchEngine()
        {
            ExitIfEngineIsAlreadyRunning();
            TheProcess = LaunchQlikSense();
            
            var location = Qlik.Engine.Location.FromUri("ws://127.0.0.1:4848");
            location.AsDirectConnectionToPersonalEdition();

            ExitOnConnectionError(location);
            return location;
        }

        private static void ExitIfEngineIsAlreadyRunning()
        {
            if (Process.GetProcessesByName("QlikSense").Any())
            {
                MessageBox.Show("An Engine is already running on this machine.", "Engine Launch Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
        }

        private static Process LaunchQlikSense()
        {
            Process process = null;
            try
            {
                // Launch QlikSense Desktop.
                process = Process.Start(PathToQlikSense);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Engine Launch Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(1);
            }
            return process;
        }

        /// <summary>
        /// Check if connection is alive. Poll three times to allow Qlik Sense time to launch.
        /// If the operation fails, shut down the engine and exit.
        /// </summary>
        /// <param name="location"></param>
        private static void ExitOnConnectionError(ILocation location)
        {
            try
            {
                for (var attemptCount = 0; attemptCount < 3; attemptCount++)
                {
                    if (location.IsAlive())
                        return;
                }
                ConnectionError("Connection requests timed out after three attempts.");
            }
            catch (Exception e)
            {
                ConnectionError(e.Message);
            }
        }

        /// <summary>
        /// Report error message, shut down engine, and exit.
        /// </summary>
        /// <param name="message">The error message to display.</param>
        private static void ConnectionError(string message)
        {
            MessageBox.Show("Connection failure: " + message, "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            if (TheProcess != null && !TheProcess.HasExited)
                TheProcess.Kill();
            Environment.Exit(1);
        }

        /// <summary>
        /// Write app resource and load with correct session to enable synchronization with other clients.
        /// </summary>
        /// <param name="location"></param>
        /// <returns>The custom desktop app.</returns>
        private static IApp PrepareApp(ILocation location)
        {
            File.WriteAllBytes(DesktopAppPath, Resources.CustomDesktopApp);
            var appIdentifier = location.AppWithNameOrDefault(CustomDesktopAppName);
            return location.App(appIdentifier);
        }

        private void UpdateButtonState()
        {
            // Collect forward and back selection counts from the app, and update the
            // enable status of the buttons accordingly.
            var countTasks = new[] {TheApp.ForwardCountAsync(), TheApp.BackCountAsync()};
            Task.WaitAll(countTasks);

            var forwardCnt = countTasks[0].Result;
            var backCnt = countTasks[1].Result;

            SetEnableStatus(buttonClear, forwardCnt != 0 || backCnt != 0);
            SetEnableStatus(buttonBack, backCnt != 0);
            SetEnableStatus(buttonForward, forwardCnt != 0);
        }

        private delegate void SetEnableStatusCallback(Button button, bool b);
        private void SetEnableStatus(Button button, bool b)
        {
            if (button.InvokeRequired)
            {
                var d = new SetEnableStatusCallback(SetEnableStatus);
                Invoke(d, new object[]{button, b});
            }
            else
            {
                button.Enabled = b;
            }
        }

        private void TrackSelectionState(object sender, EventArgs e)
        {
            Task.Factory.StartNew(UpdateButtonState);
        }

        private void DesktopWindow_Load(object sender, EventArgs e)
        {
            // Add change notification handler and evaluate clean state to start change
            // notification flow.
            _theApp.Changed += TrackSelectionState;
            _theApp.GetAppLayout();

            // Add single mashups of the two objects we want to add to the desktop.
            var visualizations = TheApp.GetSheets().First().Children.ToArray();
            var barchart = visualizations.OfType<Barchart>().First();
            var piechart = visualizations.OfType<Piechart>().First();
            webBrowser1.Navigate(barchart.SingleUrl().Fix());
            webBrowser2.Navigate(piechart.SingleUrl().Fix());
        }

        private void DesktopWindow_Closed(object sender, FormClosedEventArgs formClosedEventArgs)
        {
            Hide();
            // Perform clean shutdown of the Qlik Sense process and delete the app resource.
            TheLocation.Hub().ShutdownProcess();
            TheProcess.WaitForExit();
            File.Delete(DesktopAppPath);
        }

        private void ButtonClear_Click(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() => TheApp.ClearAll());
        }

        private void ButtonBack_Click(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() => TheApp.Back());
        }

        private void ButtonForward_Click(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() => TheApp.Forward());
        }

        private void MenuFileExitItem_Clicked(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }

    static class UriExtensions
    {
        /// <summary>
        /// Fix for known limitation in Qlik Sense 1.1.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        static public Uri Fix(this Uri uri)
        {
            return new Uri(uri.AbsoluteUri.Replace("resources/single.html", "single"));
        }
    }
}
