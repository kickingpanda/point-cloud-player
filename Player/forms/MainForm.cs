using Player.Models;
using Player.Utils;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Player
{
    public partial class MainForm : Form
    {
        private Frame _frame = new Frame();
        bool isRunning = false;
        OpenGLWindow GLWind;


        public static MainForm Instance;
        public MainForm()
        {
            Instance = this;
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
        }

        private void OpenGlWindowInNewThread()
        {
            //Opens the live view window if it is not open yet.
            Task.Run(() =>
            {
                if (GLWind == null)
                {
                    isRunning = true;
                    GLWind = new OpenGLWindow();

                    //The variables below are shared between this class and the OpenGLWindow.
                    lock (_frame.Vertices)
                    {
                        GLWind.frame.Vertices = _frame.Vertices;
                        GLWind.frame.Colors = _frame.Colors;
                        GLWind.cameraPoses = _frame.CameraPoses;
                    }

                    GLWind.Run(30.0f);
                }
            });
        }

        private void playButton_Click(object sender, EventArgs e)
        {
            isRunning = !isRunning;

            if (isRunning)
            {
                playButton.Text = @"Stop!";
                isRunning = true;
                LoadDataToFrame();
                OpenGlWindowInNewThread();
            }
            else
            {
                playButton.Text = @"Play!";
                isRunning = false;
                CancelUpdateWorkers();
            }
        }

        public void CancelUpdateWorkers()
        {
            if (GLWind != null)
            {
                GLWind = null;
            }
        }

        private string _verticeFileName = "0_Vertices";
        private string _colorFileName = "0_Colors";

        private void LoadDataToFrame()
        {
            _frame.SocketCount = 3;
            var vertexFile = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName, "Player", "data", _verticeFileName);
            var colorFile = Path.Combine(Directory.GetParent(Environment.CurrentDirectory).Parent.FullName, "Player", "data", _colorFileName);
            var newCs = LoadSaveFrame.LoadColorsFromBinFile(colorFile);
            var newVs = LoadSaveFrame.LoadVerticesFromBinFile(vertexFile);
            _frame.Colors = newCs;
            _frame.Vertices = newVs;

            var cameraAffine = new AffineTransform[] { };
            _frame.CameraPoses = cameraAffine;
        }
    }
}
