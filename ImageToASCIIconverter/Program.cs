using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ImageToASCIIconverter
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
 
        //static extern int convertLineAsm(int a, int b, int c);
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());


        }
    }
}