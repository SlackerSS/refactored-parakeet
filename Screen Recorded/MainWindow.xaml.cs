using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using Microsoft.Expression.Encoder;
using Microsoft.Expression.Encoder.Devices;
using Microsoft.Expression.Encoder.ScreenCapture;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using System.Windows.Navigation;

namespace Screen_Recorded
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string outputPath;
        private FileDialog fd;
        private FolderBrowserDialog fbd;
        private ScreenCaptureJob job;
        private OutputFormat of;

        public Collection<EncoderDevice> audioDevices;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void StartRecording_Click(object sender, RoutedEventArgs e)
        {
            StartRecordingScreen();
        }

        void StartRecordingScreen()
        {
            job = new ScreenCaptureJob(); //create new job
            //TODO: Change this to a customizable rectangle for selected area recording
            //System.Drawing.Size WorkingArea = SystemInformation.WorkingArea.Size;
            System.Drawing.Size WorkingArea = SystemInformation.PrimaryMonitorSize;
            Rectangle captureRect = new Rectangle(0, 0, WorkingArea.Width, WorkingArea.Height);
            job.CaptureRectangle = captureRect;

            fbd = new FolderBrowserDialog();
            fbd.RootFolder = System.Environment.SpecialFolder.MyDocuments;
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                outputPath = fbd.SelectedPath;
            } else if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.Abort || fbd.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
            {
                if (!Directory.Exists(@"C\Slackstudios ScreenRecorder\Recordings\"))
                Directory.CreateDirectory(@"C:\SlackStudios ScreenRecorder\Recordings\");

                outputPath = @"C:\SlackStudios ScreenRecorder\Recordings\";
            }
            job.ShowFlashingBoundary = true;
            job.ShowCountdown = true;
            job.CaptureMouseCursor = true;
            job.AddAudioDeviceSource(AudioDevicesList.SelectedItem as EncoderDevice); //used for recording audio
            if (outputPath != null && outputPath != string.Empty)
            {
               // job.OutputPath = outputPath;
                job.OutputPath = outputPath;
            }
            job.Start();
        }

        EncoderDevice AudioDevices()
        {
            EncoderDevice foundDevice = null;
            audioDevices = EncoderDevices.FindDevices(EncoderDeviceType.Audio);
            if (AudioDevicesList.Items != null)
            {
                foreach (EncoderDevice AudioDevice in audioDevices)
                {
                    AudioDevicesList.Items.Add(AudioDevice.Name);
                }
            }
            else
            {
                for (int i = 0; i < AudioDevicesList.Items.Count - 1; i++)
                {
                    AudioDevicesList.Items.RemoveAt(i);
                }
            }
            try
            {
                //TODO: Create list that allows user to choice recording device
                foundDevice = audioDevices.First();
            }
            catch(Exception ex)
            {
                System.Windows.MessageBox.Show("Cannot find preffered audio devvice using " + audioDevices[0].Name + ex.Message);
            }
            return foundDevice;
        }

        private void StopRecording_Click(object sender, RoutedEventArgs e)
        {
            if(job.Status == RecordStatus.Running)
            {
                job.Stop();
            }
        }

        void Encoder()
        {
            using(Job j = new Job())
            {
                string fileName = string.Empty;
                fd = new OpenFileDialog();
                if(fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    fileName = fd.FileName;
                }
                string OutputPath = fileName;
                MediaItem mediaItem = new MediaItem(OutputPath);
                var size = mediaItem.OriginalVideoSize;
                WindowsMediaOutputFormat WMV_Format = new WindowsMediaOutputFormat(); 
                WMV_Format.VideoProfile = new Microsoft.Expression.Encoder.Profiles.AdvancedVC1VideoProfile();
                WMV_Format.AudioProfile = new Microsoft.Expression.Encoder.Profiles.WmaAudioProfile();
                WMV_Format.VideoProfile.AspectRatio = new System.Windows.Size(16, 9);
                WMV_Format.VideoProfile.AutoFit = true;
             //   if(size.Width > 1920 && size.Height > 1080)
               // {
                    WMV_Format.VideoProfile.Size = new System.Drawing.Size(1920, 1080); 
                    WMV_Format.VideoProfile.Bitrate = new Microsoft.Expression.Encoder.Profiles.VariableUnconstrainedBitrate(6000);
                //}
                /*else if (size.Width > 1280 && size.Height > 720)
                {
                    WMV_Format.VideoProfile.Size = new System.Drawing.Size(1280, 720);
                    WMV_Format.VideoProfile.Bitrate = new Microsoft.Expression.Encoder.Profiles.VariableUnconstrainedBitrate(4000);
                }
                else
                {
                    WMV_Format.VideoProfile.Size = new System.Drawing.Size(size.Width, size.Height);
                    WMV_Format.VideoProfile.Bitrate = new Microsoft.Expression.Encoder.Profiles.VariableUnconstrainedBitrate(2000);
                }*/

                mediaItem.VideoResizeMode = VideoResizeMode.Letterbox;
                mediaItem.OutputFormat = WMV_Format;
                j.MediaItems.Add(mediaItem);
                j.CreateSubfolder = false;
                j.OutputDirectory = @"C:\Custom Applications\";
                j.EncodeProgress += new EventHandler<EncodeProgressEventArgs>(j_encodeProgress);
                j.Encode();
            }
        }

        private Task ProcessData(IProgress<ProgressReport> progress)
        {
            throw new NotImplementedException();
        }

        void j_encodeProgress(object sender, EncodeProgressEventArgs e)
        {
            string status = string.Format("{0:f1}%", e.Progress);
            EncoderProgressBar.Value = e.Progress;
            
        }

        private void RecorderWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (job != null)
            {
                if (job.Status == RecordStatus.Running)
                {
                    job.Stop();
                    job.Dispose();
                }
            }
        }

        private void EncodeButton_Click(object sender, RoutedEventArgs e)
        {
            Encoder();
        }

        private void AudioDevicesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void AudioDevicesList_Initialized(object sender, EventArgs e)
        {
            AudioDevices();
        }
    }
}
