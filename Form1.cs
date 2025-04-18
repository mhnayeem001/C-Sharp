using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Guna.UI2.WinForms;
using NAudio.Wave;

namespace MusicPlayer03
{
    public partial class Form1 : Form
    {
        private bool isMuted = false;
        private IWavePlayer wavePlayer;
        private TimeSpan trackDuration;
        private AudioFileReader audioFile;
        private List<string> playlist = new List<string>();
        private int currentTrackIndex = 0;
        private bool userSelected = true;
        private bool isPlaying = false;
        public Form1()
        {

            InitializeComponent();
            wavePlayer = new WaveOutEvent();
            playlistListBox.SelectedIndexChanged += PlaylistListBox_SelectedIndexChanged;
            SetVolume();
            progressBar1.MouseDown += progressBar1_MouseDown;

        }
        private void PlaylistListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (userSelected && playlistListBox.SelectedIndex != -1)
            {
                PlayTrack(playlist[playlistListBox.SelectedIndex]);
                /*if (currentTrackIndex >= 0 && currentTrackIndex < playlist.Count)
                {
                    PlayTrack(playlist[currentTrackIndex]);
                }
                else
                {
                    currentTrackIndex = 0;
                    PlayTrack(playlist[currentTrackIndex]);
                }*/
            }

            userSelected = true;
        }

        private void PlaylistListBox_MouseDown(object sender, MouseEventArgs e)
        {
            userSelected = true;
        }

        private void PlaylistListBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (playlistListBox.SelectedIndex != -1)
            {
                currentTrackIndex = playlistListBox.SelectedIndex;
                PlayTrack(playlist[currentTrackIndex]);
            }
        }
        private void PlayTrack(string musicFilePath)
        {
            if (File.Exists(musicFilePath))
            {
                if (wavePlayer != null && wavePlayer.PlaybackState == PlaybackState.Playing)
                {
                    wavePlayer.Stop();
                    wavePlayer.Dispose(); // Dispose of the previous wavePlayer
                    wavePlayer = new WaveOutEvent(); // Create a new wavePlayer
                }

                if (audioFile != null)
                {
                    audioFile.Dispose(); // Dispose of the previous audioFile
                }

                audioFile = new AudioFileReader(musicFilePath);
                trackDuration = audioFile.TotalTime; // Set the track duration
                wavePlayer.Init(audioFile);
                wavePlayer.Play();
                isPlaying = true;


                // Start a background thread to update the progress bar
                Thread thread = new Thread(() => UpdateProgressBar());
                thread.Start();

                timer1.Interval = 100; // Update progress bar every 100 milliseconds
                timer1.Start();
            }
            else
            {
                MessageBox.Show("The music file does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string[] arr = playlist.Select(Path.GetFileName).ToArray();
            string search = textBox1.Text;

            var items = (from a in arr
                         where a.StartsWith(search, StringComparison.OrdinalIgnoreCase)
                         select a).ToArray();

            playlistListBox.Items.Clear();
            playlistListBox.Items.AddRange(items);
        }

        private void UpdateProgressBar()
        {
            while (wavePlayer != null && wavePlayer.PlaybackState == PlaybackState.Playing)
            {
                double progress = 0;

                // Safely access audioFile.CurrentTime using a lock to avoid race conditions
                lock (audioFile)
                {
                    progress = audioFile.CurrentTime.TotalSeconds / trackDuration.TotalSeconds;
                }

                progressBar1.Invoke((Action)(() => progressBar1.Value = (int)(progress * 100))); // Update progress bar on UI thread
                Thread.Sleep(100); // Wait 100 milliseconds before checking again
            }
        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            if (wavePlayer != null && wavePlayer.PlaybackState == PlaybackState.Playing)
            {
                double progress = audioFile.CurrentTime.TotalSeconds / trackDuration.TotalSeconds;
                progressBar1.Value = (int)(progress * 100);
            }
        }
        private void progressBar1_MouseDown(object sender, MouseEventArgs e)
        {
            if (wavePlayer != null && wavePlayer.PlaybackState == PlaybackState.Playing)
            {
                // Calculate the position based on the mouse click
                double clickPercentage = (double)e.X / progressBar1.Width;
                TimeSpan newPosition = TimeSpan.FromSeconds(clickPercentage * trackDuration.TotalSeconds);

                // Safely set the new position in the audio playback
                lock (audioFile)
                {
                    audioFile.CurrentTime = newPosition;
                }

                // If you want to immediately update the progress bar position
                progressBar1.Value = (int)(clickPercentage * 100);
            }
        }
        private void UpdatePlaylistListBox()
        {
            
            foreach (string song in playlist)
            {
                playlistListBox.Items.Add(Path.GetFileName(song));
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (wavePlayer != null)
            {
                wavePlayer.Stop();
                wavePlayer.Dispose();
            }

            if (audioFile != null)
            {
                audioFile.Dispose();
            }

            base.OnFormClosing(e);
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            
        }

        private void guna2Button3_Click(object sender, EventArgs e)
        {
            
        }

        private HashSet<string> uniquePaths = new HashSet<string>();

        private void guna2Button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "MP3 Files|*.mp3",
                Multiselect = true  // Allow multiple file selection
            };

            try
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (string newSong in openFileDialog.FileNames)
                    {
                        string normalizedPath = Path.GetFullPath(newSong);
                        if (uniquePaths.Add(normalizedPath))
                        {
                            // Only add to the playlist if it's a new unique path
                            playlist.Add(normalizedPath);
                        }
                    }

                    UpdatePlaylistListBox();

                    if (!isPlaying && playlist.Count > 0)
                    {
                        currentTrackIndex = 0;
                        PlayTrack(playlist[currentTrackIndex]);
                    }
                }
            }
            finally
            {
                openFileDialog.Dispose();
            }
        }




        private void guna2CircleButton5_Click(object sender, EventArgs e)
        {

        }

        private void guna2CircleButton5_Click_1(object sender, EventArgs e)
        {

        }
        private void SetVolume()
        {
            //float volume = trackBar1.Value / 100.0f;
            float volume = isMuted ? 0.0f : trackBar1.Value / 100.0f;

            if (wavePlayer != null)
            {
                wavePlayer.Volume = volume;
            }
        }
        private void guna2Panel5_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2CircleButton6_Click(object sender, EventArgs e)
        {

        }

        private void guna2CircleButton8_Click(object sender, EventArgs e)
        {

        }

        private void guna2CircleButton8_Click_1(object sender, EventArgs e)
        {

        }

        private void guna2Panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2Panel7_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2Panel6_Paint(object sender, PaintEventArgs e)
        {

        }

        private void guna2CircleButton3_Click(object sender, EventArgs e)
        {
            if (wavePlayer != null)
            {
                if (isPlaying)
                {
                    wavePlayer.Pause();
                }
                else
                {
                    wavePlayer.Play();
                }
                isPlaying = !isPlaying;
            }
        }

        private void guna2CircleButton2_Click(object sender, EventArgs e)
        {
            if (playlist.Count > 0)
            {
                currentTrackIndex = (currentTrackIndex + 1) % playlist.Count;
                PlayTrack(playlist[currentTrackIndex]);
            }
        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            //float volume = trackBar1.Value / 100.0f; 

            if (!isMuted)
            {
                float volume = trackBar1.Value / 100.0f;

                if (wavePlayer != null)
                {
                    wavePlayer.Volume = volume;
                }
            }
            /*if (wavePlayer != null)
            {
                wavePlayer.Volume = volume;
            }*/
        }

        private void guna2CircleButton4_Click(object sender, EventArgs e)
        {
            if (playlist.Count > 0)
            {
                
                currentTrackIndex = (currentTrackIndex - 1 + playlist.Count) % playlist.Count;

               
                PlayTrack(playlist[currentTrackIndex]);
            }
        }

        private void guna2CircleButton1_Click(object sender, EventArgs e)
        {
            isMuted = !isMuted;
            // guna2CircleButton1.Image = isMuted ? mutedIcon : unmutedIcon;
            trackBar1_Scroll(sender, e);
            SetVolume();
        }

        private void guna2CirclePictureBox4_Click(object sender, EventArgs e)
        {
            Seek(TimeSpan.FromSeconds(-5));
        }

        private void guna2CirclePictureBox3_Click(object sender, EventArgs e)
        {
            Seek(TimeSpan.FromSeconds(5));
        }

        private void Seek(TimeSpan offset)
        {
            if (wavePlayer != null && wavePlayer.PlaybackState == PlaybackState.Playing)
            {
                TimeSpan newPosition;

                // Safely access audioFile.CurrentTime using a lock to avoid race conditions
                lock (audioFile)
                {
                    newPosition = audioFile.CurrentTime + offset;
                    if (newPosition < TimeSpan.Zero)
                    {
                        newPosition = TimeSpan.Zero;
                    }
                    else if (newPosition > trackDuration)
                    {
                        newPosition = trackDuration;
                    }

                    audioFile.CurrentTime = newPosition;
                }

                // Update progress bar position
                progressBar1.Invoke((Action)(() => progressBar1.Value = (int)(newPosition.TotalSeconds / trackDuration.TotalSeconds * 100)));
            }
        }
    }
}
