using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using tesseract;

namespace OCR
{
    public enum TesseractEngineMode : int
    {
        /// <summary>
        /// Run Tesseract only - fastest
        /// </summary>
        TESSERACT_ONLY = 0,

        /// <summary>
        /// Run Cube only - better accuracy, but slower
        /// </summary>
        CUBE_ONLY = 1,

        /// <summary>
        /// Run both and combine results - best accuracy
        /// </summary>
        TESSERACT_CUBE_COMBINED = 2,

        /// <summary>
        /// Specify this mode when calling init_*(),
        /// to indicate that any of the above modes
        /// should be automatically inferred from the
        /// variables in the language-specific config,
        /// command-line configs, or if not specified
        /// in any of the above should be set to the
        /// default OEM_TESSERACT_ONLY.
        /// </summary>
        DEFAULT = 3
    }

    public enum TesseractPageSegMode : int
    {
        /// <summary>
        /// Fully automatic page segmentation
        /// </summary>
        PSM_AUTO = 0,

        /// <summary>
        /// Assume a single column of text of variable sizes
        /// </summary>
        PSM_SINGLE_COLUMN = 1,

        /// <summary>
        /// Assume a single uniform block of text (Default)
        /// </summary>
        PSM_SINGLE_BLOCK = 2,

        /// <summary>
        /// Treat the image as a single text line
        /// </summary>
        PSM_SINGLE_LINE = 3,

        /// <summary>
        /// Treat the image as a single word
        /// </summary>
        PSM_SINGLE_WORD = 4,

        /// <summary>
        /// Treat the image as a single character
        /// </summary>
        PSM_SINGLE_CHAR = 5
    }
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }
        Bitmap m_image;
        private TesseractProcessor m_tesseract = null;
        List<Word> m_words;
        
        private void btnOCR_Click(object sender, EventArgs e)
        {
            if (m_image != null && !string.IsNullOrEmpty(txtLang.Text))
            {

                
                m_tesseract = new TesseractProcessor();
                bool succeed = m_tesseract.Init(txtPath.Text + "\\", txtLang.Text, (int)TesseractEngineMode.DEFAULT);
                if (!succeed)
                {
                    MessageBox.Show("Tesseract initialization failed. The application will exit.");
                    Application.Exit();
                }
                m_tesseract.DoMonitor = true;
                m_tesseract.SetVariable("tessedit_pageseg_mode", ((int)TesseractPageSegMode.PSM_SINGLE_BLOCK).ToString());
                m_tesseract.Clear();
                m_tesseract.ClearAdaptiveClassifier();
                textBoxResult.Text = FixThaiCodePage(m_tesseract.Apply(m_image));
                m_words = m_tesseract.RetriveResultDetail();
                panel1.Refresh();
            }
        }
        public string FixThaiCodePage(string str)
        {
            byte[] raw = Encoding.Default.GetBytes(str);
            string res = Encoding.GetEncoding("utf-8").GetString(raw);
            return res;
        }
        private void btnSetPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.SelectedPath = txtPath.Text;
            if (fbd.ShowDialog() == DialogResult.OK)
                txtPath.Text = fbd.SelectedPath;
        }

        private void btnSelectImage_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = System.Environment.CurrentDirectory;
            openFileDialog1.RestoreDirectory = false;
            openFileDialog1.Filter = "Common Images|*.tif;*.tiff;*.bmp;*.jpg;*.jpeg;*.png;*.gif";
            openFileDialog1.FilterIndex = 2;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                m_image = new Bitmap(openFileDialog1.FileName);
                m_image.SetResolution(96, 96);
                m_words = null;
                panel1.AutoScrollMinSize = m_image.Size;
                panel1.Refresh();
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            if (m_image != null)
                e.Graphics.DrawImage(m_image, panel1.AutoScrollPosition.X, panel1.AutoScrollPosition.Y);
            if (m_words != null && chk1.Checked==true)
            {
                foreach (Word word in m_words)
                {
                    Pen pen = null;
                    pen = new Pen(Color.FromArgb(255, 128, (int)word.Confidence));
                    e.Graphics.DrawRectangle(pen, word.Left + panel1.AutoScrollPosition.X, word.Top + panel1.AutoScrollPosition.Y, word.Right - word.Left, word.Bottom - word.Top);
                    foreach (tesseract.Character c in word.CharList)
                    {
                        e.Graphics.DrawRectangle(Pens.BlueViolet, c.Left + panel1.AutoScrollPosition.X, c.Top + panel1.AutoScrollPosition.Y, c.Right - c.Left, c.Bottom - c.Top);
                    }
                }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.TessdataPath = txtPath.Text;
            Properties.Settings.Default.Lang = txtLang.Text;
            Properties.Settings.Default.Save();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            txtPath.Text = Properties.Settings.Default.TessdataPath;
            txtLang.Text = Properties.Settings.Default.Lang;
          
        }

        private void chk1_CheckedChanged(object sender, EventArgs e)
        {
            panel1.Refresh();
        }
    }

}
