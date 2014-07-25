using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using UploadAPIDemo;
using System.ComponentModel;
using System.Timers;

namespace UploadAPIDemoGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static bool selfSigned = true; // Target server is a self-signed server
        private static bool hasBeenInitialized = false;
        private static long DEFAULT_PARTSIZE = 1048576; // Size of each upload in the multipart upload process
        private static System.Timers.Timer timer;

        public MainWindow()
        {
            InitializeComponent();

            if (selfSigned)
            {
                // For self-signed servers
                EnsureCertificateValidation();
            }
        }

        /// <summary>
        /// Opens a file chooser and allows the user to pick which file to upload
        /// </summary>
        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.DefaultExt = ".mp4";
            dlg.Filter = "MP4 File (.mp4)|*.mp4";

            DialogResult result = dlg.ShowDialog();

            if (result.Equals(System.Windows.Forms.DialogResult.OK))
            {
                string filename = dlg.FileName;
                FilePath.Text = filename;
            }
        }

        /// <summary>
        /// Uploads the selected file to designated server and folder on click
        /// </summary>
        private void Upload_Click(object sender, RoutedEventArgs e)
        {
            LockAllFields();
            Status.Content = "Uploading...";

            Common.SetServer(Server.Text);

            timer = new System.Timers.Timer(1500);
            timer.Elapsed += ProcessUpload;
            timer.Start();
        }

        /// <summary>
        /// Disabling all fields
        /// </summary>
        private void LockAllFields()
        {
            Server.IsEnabled = false;
            UserID.IsEnabled = false;
            UserPassword.IsEnabled = false;
            FolderID.IsEnabled = false;
            SessionName.IsEnabled = false;
            Upload.IsEnabled = false;
            Browse.IsEnabled = false;
        }

        /// <summary>
        /// Enabling all fields that are originally enabled
        /// </summary>
        private void FreeAllFields()
        {
            Server.IsEnabled = true;
            UserID.IsEnabled = true;
            UserPassword.IsEnabled = true;
            FolderID.IsEnabled = true;
            SessionName.IsEnabled = true;
            Upload.IsEnabled = true;
            Browse.IsEnabled = true;
        }

        /// <summary>
        /// Calls the upload method and handles any error
        /// </summary>
        private void ProcessUpload(Object source, ElapsedEventArgs e)
        {
            timer.Stop();

            Dispatcher.BeginInvoke(new Action(() =>
            {
                bool hasError = false;

                try
                {
                    UploadAPIWrapper.UploadFile(
                        UserID.Text,
                        UserPassword.Password,
                        FolderID.Text,
                        SessionName.Text,
                        FilePath.Text,
                        DEFAULT_PARTSIZE);
                }
                catch (Exception ex) // error handling and status
                {
                    hasError = true;
                    Status.Content = "Upload Failed: " + ex.Message;
                }

                if (!hasError)
                {
                    Status.Content = "Upload Successful";
                }

                FreeAllFields();
            }));
        }

        //========================= Needed to use self-signed servers

        /// <summary>
        /// Ensures that our custom certificate validation has been applied
        /// </summary>
        public static void EnsureCertificateValidation()
        {
            if (!hasBeenInitialized)
            {
                ServicePointManager.ServerCertificateValidationCallback += new System.Net.Security.RemoteCertificateValidationCallback(CustomCertificateValidation);
                hasBeenInitialized = true;
            }
        }

        /// <summary>
        /// Ensures that server certificate is authenticated
        /// </summary>
        private static bool CustomCertificateValidation(object sender, X509Certificate cert, X509Chain chain, System.Net.Security.SslPolicyErrors error)
        {
            return true;
        }
    }
}
