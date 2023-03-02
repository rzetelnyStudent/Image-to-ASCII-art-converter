using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Jitbit.Utils;
using CsConverter;

namespace ImageToASCIIconverter
{
    public partial class Form1 : Form
    {
        [DllImport(@"C:\Users\alert\OneDrive\Dokumenty\STUDIA\JA\JA projekt\ImageToASCIIconverter\ImageToASCIIconverter\x64\Release\JaAsm.dll")]
        private static extern unsafe int convertLineAsm(int lineNr, byte* imageInBytesPtr, char* textLinePtr, int asciiImWidth);

        //[DllImport(@"C:\Users\alert\OneDrive\Dokumenty\STUDIA\JA\JA projekt\ImageToASCIIconverter\CsConverter\bin\Debug\netcoreapp3.1\CsConverter.dll")]
        //private static extern unsafe void convertLineCs(int lineNr, byte* imageInBytesPtr, char* textLinePtr, int asciiImWidth);

        private enum Language
        {
            CS,
            ASM
        };

        private const int ASCII_IM_W = 256;     // chars per line at the output text. Have to be divisible by 16 because of asm
        private const int THREADS_COUNT_START = 8;     // initial threads count

        private Semaphore semaphore = new Semaphore(THREADS_COUNT_START, THREADS_COUNT_START);
        private Language language;
 
        private char[] asciiChars = { '#', '#', '@', '%', '=', '+', '*', ':', '-', '.', ' ' };
        private string output;
        private byte[] imageInBytes;
        private string[] textLines;
        private char[] asciiOutput;
        private Bitmap image;

        public Form1()
        {
            InitializeComponent();
            trackBar.Value = THREADS_COUNT_START;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }


        /* wrapper function for converting line of image to ascii text output.
         * param lineNr - line no (from 0) to be converted
         */
        unsafe void convertLine(int lineNr)
        {
            fixed (byte* imageInBytesPtr = imageInBytes)     // pointer to image
            {
                fixed (char* textLinePtr = asciiOutput)      // pointer to text output
                {
                    semaphore.WaitOne();      // take one seat
                    if (language == Language.CS)
                    {
                        convertLineCs(lineNr, imageInBytesPtr, textLinePtr, ASCII_IM_W);
                    }
                    else      // Language == ASM
                    {
                        try
                        {
                            int? a = convertLineAsm(lineNr, imageInBytesPtr, textLinePtr, ASCII_IM_W);
                        }
                        catch (System.NullReferenceException e)      // asm procedure throws NullReferenceException, idk why
                        {
                            int i = 1;                       
                        }
                    }                        
                    semaphore.Release();      // release one seat
                }
            }
        }

        /* Convert to ascii button listener
         */
        private void btnConvertToAscii_Click(object sender, EventArgs e)
        {
            btnConvertToAscii.Enabled = false;
            if (image == null)      // image not loaded
            {
                MessageBox.Show("No file chosen!", "File error");
            }
            else
            {
                convertImToAscii();
            }
            btnConvertToAscii.Enabled = true;
        }


        /* Function for converting image to ascii text.
         * Returns conversion time in 100ns ticks.
         */
        private long convertImToAscii()
        {

            // ------------------------------- PREPROCESSING --------------------------------------

            if (radioButton1.Checked)      // Read radioButtons
                language = Language.CS;
            else
                language = Language.ASM;

            // Resize loaded image to width of ASCII_IM_W (256). Required to maintain constant ascii text output line width
            Bitmap resizedImage = GetReSizedImage(this.image, ASCII_IM_W);     

            textLines = new string[resizedImage.Height];
            asciiOutput = new char[resizedImage.Height * resizedImage.Width];

            for (int i = 0; i < asciiOutput.Length; i++)     // initialize ascii ouput with aaaaa...
            {
                asciiOutput[i] = 'a';
            }

            imageInBytes = convertImageToArray(resizedImage);     // converrt image bitmap to RGBRGB... byte array

            Thread[] t = new Thread[resizedImage.Height];     

            Stopwatch mywatch = new Stopwatch();

            // ------------------------------- CONVERSION --------------------------------------

            // In my conversion as many threads as image width are started.
            // It is the semaphore that limits how many threads can do conversion concurrently
            mywatch.Start();     // start timer
            for (int i = 0; i < resizedImage.Height; i++)     // iterate over image line by line
            {
                int temp = i;     // capture value (cannot pass loop counter variable value to thread)
                t[i] = new Thread(() =>
                {
                    convertLine(temp);     // assign function to thread 
                });
                t[i].Start();     // start thread
            }

            for (int i = 0; i < resizedImage.Height; i++)    // wait until threads finish
            {
                t[i].Join();
            }
            mywatch.Stop();     // stop timer

            // ------------------------------- POSTPROCESSING --------------------------------------

            // display conversion time in format: mins:secs:ms:us
            TimeSpan ts = mywatch.Elapsed;
            long microSeconds = ts.Ticks / (TimeSpan.TicksPerMillisecond / 1000) - ts.Milliseconds * 1000;
            string elapsedTime = String.Format("{0:00}:{1:00}.{2:000}.{3:000}", ts.Minutes, ts.Seconds, ts.Milliseconds, microSeconds);
            label4.Text = elapsedTime + " sec";
            long timeSpanTicks = ts.Ticks;
            mywatch.Reset();

            // convert char array to a single string:
            StringBuilder singleString = new StringBuilder();
            for (int i = 0; i < resizedImage.Height; i++)
            {
                singleString.Append(new string(asciiOutput, i * ASCII_IM_W, ASCII_IM_W));       // collect threaded conversion results
                singleString.Append("<BR>");     // insert html new line separators
            }
            output = singleString.ToString();
            browserMain.DocumentText = "<pre>" + "<Font size=0>" + output + "</Font></pre>";     // Note that html have to be displayed with font that has all chars taking the same space (pre)
            return timeSpanTicks;
        }
        

        /* Converts image to RGBRGB... byte array
         * param image - input bitmap (resized in my case)
         */
        private byte[] convertImageToArray(Bitmap image)
        {
            byte[] imageInBytes = new byte[image.Width * image.Height * 3];
            for (int h = 0; h < image.Height; h++)
            {
                for (int w = 0; w < image.Width; w++)
                {
                    Color color = image.GetPixel(w, h);
                    imageInBytes[(h * ASCII_IM_W + w) * 3] = color.R;
                    imageInBytes[(h * ASCII_IM_W + w) * 3 + 1] = color.G;
                    imageInBytes[(h * ASCII_IM_W + w) * 3 + 2] = color.B;
                }
            }
            return imageInBytes;
        }

        /* Resizes input image to specified width. Height is calculated proportionally.
         * param inputBitmap - bitmap to be converted
         * param asciiWidth - returned image width
         * returns resized bitmap 
         */
        private Bitmap GetReSizedImage(Bitmap inputBitmap, int asciiWidth)
        {
            int asciiHeight = 0;
            //Calculate the new Height of the image from its width
            asciiHeight = (int)Math.Ceiling((double)inputBitmap.Height * asciiWidth / inputBitmap.Width / 2);   // /2 to prevent image vertical strechting. Each char is about 2 times taller than wider
            Bitmap result = new Bitmap(asciiWidth, asciiHeight);
            Graphics g = Graphics.FromImage((Image)result);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(inputBitmap, 0, 0, asciiWidth, asciiHeight);
            g.Dispose();
            return result;
        }

        /* Listener method for selecting input image to be converted.
         * */
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            DialogResult diag = openFileDialog1.ShowDialog();
            if (diag == DialogResult.OK)
            {
                txtPath.Text = openFileDialog1.FileName;
            }
            try
            {
                this.image = new Bitmap(txtPath.Text, true);     //Load the Image from the specified path
                pictureBox1.Image = image;
            }
            catch (System.ArgumentException)      // opened file is not an image
            {
                MessageBox.Show("File format not supported!", "File error");
            }

        }

        /* Listener method for saving ascii text as .html or .txt
         */
        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (output == null)
            {
                MessageBox.Show("Nothing to save!", "File error");
            }
            else 
            { 
                saveFileDialog1.Filter = "Text File (*.txt)|.txt|HTML (*.htm)|.htm";
                DialogResult diag = saveFileDialog1.ShowDialog();
                if (diag == DialogResult.OK)
                {
                    if (saveFileDialog1.FilterIndex == 1)
                    {
                        //If the format to be saved is HTML
                        //Replace all HTML spaces to standard spaces
                        //and all linebreaks to CarriageReturn, LineFeed
                        output = output.Replace("&nbsp;", " ").Replace("<BR>", "\r\n");
                    }
                    else
                    {
                        //use <pre></pre> tag to preserve formatting when viewing it in browser
                        output = "<pre>" + output + "</pre>";
                    }
                    StreamWriter sw = new StreamWriter(saveFileDialog1.FileName);
                    sw.Write(output);
                    sw.Flush();
                    sw.Close();
                }
            }
        }

        // when ASM radioBtn is checked, uncheck CS button
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                radioButton1.Checked = false;
            }
        }


        unsafe void convertLineCs(int lineNr, byte* imageInBytesPtr, char* textLinePtr, int asciiImWidth)
        {

            int start_pos = lineNr * asciiImWidth * 3;     // calculate starting position in image RGBRGB... array
            int start_pos_ascii = lineNr * asciiImWidth;      // calculate starting position in output chars array      
            for (int w = 0; w < asciiImWidth * 3; w += 3)
            {
                // load pixel from RGBRGBRGB... array
                Color pixelColor = Color.FromArgb(*(imageInBytesPtr + start_pos + w), *(imageInBytesPtr + start_pos + w + 1), *(imageInBytesPtr + start_pos + w + 2));

                //Average out the RGB components to find the Gray Color
                int avg = ((int)pixelColor.R + (int)pixelColor.G + (int)pixelColor.B) / 3;

                int index = (avg * 10) / 255;       // convert to array with ascii chars index

                *(textLinePtr + start_pos_ascii + (w / 3)) = asciiChars[index];     // save char to output array
            }
        }


        private void browserMain_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void trackBar_Scroll_1(object sender, EventArgs e)
        {
            semaphore = new Semaphore(trackBar.Value, trackBar.Value);
            label2.Text = Convert.ToString(trackBar.Value);
        }

        // when CS radioBtn is checked, uncheck ASM button
        private void radioButton1_CheckedChanged_1(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                radioButton2.Checked = false;
            }
        }


        /* Method for generating CSV report with conversion times vs number of threads
         */
        private void CSVReportGenerate(object sender, EventArgs e)
        {
            CSV_report.Enabled = false;
            btnConvertToAscii.Enabled = false;
            if (image == null)
            {
                MessageBox.Show("No file chosen!", "File error");
            }
            else
            {
                var myExport = new CsvExport();

                for (int i = 1; i < 65; i++)
                {
                    semaphore = new Semaphore(i, i);
                    long avg = 0;
                    for (int j = 0; j < 5; j++)
                    {
                        avg += convertImToAscii();
                    }
                    avg = avg / 5;
                    myExport.AddRow();
                    myExport["Threads count"] = i.ToString();
                    myExport["Time in ticks"] = avg.ToString();
                }

                string path = @"c:\temp\MyTest.csv";
                myExport.ExportToFile(path);
            }

            CSV_report.Enabled = true;
            btnConvertToAscii.Enabled = true;
        }
    }
}