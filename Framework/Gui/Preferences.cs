﻿/******************************************************************************
* The MIT License
* Copyright (c) 2008 Rusty Howell, Thomas Wiest
*
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to  permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/

// Authors:
// 		Thomas Wiest  twiest@users.sourceforge.net
//		Rusty Howell  rhowell@users.sourceforge.net

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace UvsChess.Gui
{
    public partial class Preferences : Form
    {
        #region Members

        private static string TIME = "time";
        private static string GRACEPERIOD = "graceperiod";

        private static int time_default = 5000;
        private static int grace_default = 1000;

        static Dictionary<string, string> items = null;
        private static string inifile = AppDomain.CurrentDomain.BaseDirectory + "UvsChess.ini";

        #endregion

        #region Properties

        public static int Time
        {
            get { return Convert.ToInt32(items[TIME]); }
            set { items[TIME] = value.ToString(); }
        }
        public static int GracePeriod
        {
            get { return Convert.ToInt32(items[GRACEPERIOD]); }
            set { items[GRACEPERIOD] = value.ToString(); }
        }
        #endregion

        #region Contructors

        public Preferences()
        {
            InitializeComponent();
        }

        #endregion

        #region Methods

        private void Preferences_Load(object sender, EventArgs e)
        {
            LoadFromFile();

            txtTime.Text = Time.ToString();
            txtGrace.Text = GracePeriod.ToString();
        }
        public static void LoadFromFile()
        {

            items = new Dictionary<string, string>();
            if (!File.Exists(inifile))
            {
                Time = time_default;
                GracePeriod = grace_default;
                return;
            }
            StreamReader infile = new StreamReader(inifile);

            string line = infile.ReadLine();
            while (line != null)
            {
                if (line == string.Empty)
                {
                    line = infile.ReadLine();
                    continue;
                }
                string[] sections = line.Split('=');
                items[sections[0]] = sections[1];
                line = infile.ReadLine();
            }
            infile.Close();
        }
        public static void SavePreferences()
        {
            StreamWriter outfile = new StreamWriter(inifile);
            foreach (string key in items.Keys)
            {
                outfile.WriteLine("{0}={1}", key, items[key]);
            }
            outfile.Close();
        }

        private void Preferences_FormClosing(object sender, FormClosingEventArgs e)
        {
            SavePreferences();
        }

        #endregion
    }
}