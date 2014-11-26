using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Globalization;
using System.Threading;

using System.Diagnostics;

using ASCOM.Utilities;
using ASCOM.IP9212;


namespace ASCOM.IP9212_v2
{
    [ComVisible(false)]					// Form not registered for COM!
    public partial class SetupDialogForm : Form
    {

        public SetupDialogForm()
        {

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(Switch.currentLang);
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(Switch.currentLang);            
            
            InitializeComponent();

            // Initialise current values of user settings from the ASCOM Profile 
            chkTrace.Checked = Switch.traceState;
            
            //Write driver version
            Version ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

            string driverVersion = String.Format(CultureInfo.InvariantCulture, "{0}.{1} build {2}.{3}", ver.Major, ver.Minor, ver.Build, ver.Revision);
            //MessageBox.Show("Application " + assemName.Name + ", Version " + ver.ToString());
            string fileVersion = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;

            lblVersion.Text = "Driver: " + driverVersion;
            lblVersion.Text += Environment.NewLine + "File: "+ fileVersion;
            lblVersion.Text += Environment.NewLine + "Compile time: " + RetrieveLinkerTimestamp();

            cmbLang.DataSource = new CultureInfo[]{
                CultureInfo.GetCultureInfo("en-US"),
                CultureInfo.GetCultureInfo("ru-RU")
            };
            cmbLang.DisplayMember = "NativeName";
            cmbLang.ValueMember = "Name";

            cmbLang.SelectedValue = Switch.currentLang;
            txtCacheConnect.Text=Switch.ConnectCheck_Cache_Timeout.ToString();
            txtCacheRead.Text=Switch.InputRead_Cache_Timeout.ToString();

        }

        private void SetupDialogForm_Load(object sender, EventArgs e)
        {
           
            PopulateGrid();
        }
        
        private void cmdOK_Click(object sender, EventArgs e) // OK button event handler
        {
            Switch.traceState = chkTrace.Checked;
            Switch.currentLang = cmbLang.SelectedValue.ToString();
            Switch.ConnectCheck_Cache_Timeout = Convert.ToInt32(txtCacheConnect.Text);
            Switch.InputRead_Cache_Timeout = Convert.ToInt32(txtCacheRead.Text);
            Switch.OutputRead_Cache_Timeout = Switch.InputRead_Cache_Timeout; //set output=input read
            IP9212_switch_hardware_class.CACHE_CONNECTED_CHECK_MAX_INTERVAL = Switch.ConnectCheck_Cache_Timeout;
            IP9212_switch_hardware_class.CACHE_OUTPUT_MAX_INTERVAL = Switch.OutputRead_Cache_Timeout;
            IP9212_switch_hardware_class.CACHE_INPUT_MAX_INTERVAL = Switch.InputRead_Cache_Timeout;

            //Convert data from grid to settings vars
            SaveGrid();
            
            //Save settings into ASCOM Profile
            Switch.writeSettings();
        }

        private void cmdCancel_Click(object sender, EventArgs e) // Cancel button event handler
        {
            Close();
        }

        private void BrowseToAscom(object sender, EventArgs e) // Click on ASCOM logo event handler
        {
            try
            {
                System.Diagnostics.Process.Start("http://ascom-standards.org/");
            }
            catch (System.ComponentModel.Win32Exception noBrowser)
            {
                if (noBrowser.ErrorCode == -2147467259)
                    MessageBox.Show(noBrowser.Message);
            }
            catch (System.Exception other)
            {
                MessageBox.Show(other.Message);
            }
        }

        /// <summary>
        /// Read data from settings vars and populate grid
        /// </summary>
        private void PopulateGrid()
        {
            // Clear grids
            dataGridOutputSwitch.Rows.Clear();
            dataGridInputSwitch.Rows.Clear(); 
            // Populate both grids
            for (int curRowIndex = 0; curRowIndex <= 7; curRowIndex++)
            {
                //Output switches
                if (1 != null)
                {
                    //Add sensor to grid
                    dataGridOutputSwitch.Rows.Add();
                    dataGridOutputSwitch.Rows[curRowIndex].Cells["SwitchId"].Value = (curRowIndex + 1).ToString() + (curRowIndex < 4 ? " NC" : " NO"); ;
                    dataGridOutputSwitch.Rows[curRowIndex].Cells["SwitchName"].Value = Switch.SwitchData[curRowIndex].Name;
                    dataGridOutputSwitch.Rows[curRowIndex].Cells["SwitchDescription"].Value = Switch.SwitchData[curRowIndex].Desc;
                }

                //Input switches
                if (1 != null)
                {
                    //Add sensor to grid
                    dataGridInputSwitch.Rows.Add();
                    dataGridInputSwitch.Rows[curRowIndex].Cells["InputSwitchId"].Value = (curRowIndex + 1).ToString() +(curRowIndex<4?" V":" R");
                    dataGridInputSwitch.Rows[curRowIndex].Cells["InputSwitchName"].Value = Switch.SwitchData[curRowIndex+8].Name;
                    dataGridInputSwitch.Rows[curRowIndex].Cells["InputSwitchDescription"].Value = Switch.SwitchData[curRowIndex+8].Desc;
                }
            }        
        }


        /// <summary>
        /// Save data from grid into settings vars
        /// </summary>
        private void SaveGrid()
        {
            for (int curRowIndex = 0; curRowIndex <= 7; curRowIndex++)
            {
                if (1 != null)
                {
                    //Output switches
                    Switch.SwitchData[curRowIndex].Name = dataGridOutputSwitch.Rows[curRowIndex].Cells["SwitchName"].Value.ToString();
                    Switch.SwitchData[curRowIndex].Desc = dataGridOutputSwitch.Rows[curRowIndex].Cells["SwitchDescription"].Value.ToString();

                    //Input switches
                    Switch.SwitchData[curRowIndex + 8].Name = dataGridInputSwitch.Rows[curRowIndex].Cells["InputSwitchName"].Value.ToString();
                    Switch.SwitchData[curRowIndex + 8].Desc = dataGridInputSwitch.Rows[curRowIndex].Cells["InputSwitchDescription"].Value.ToString();

                }
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://www.aviosys.com/9212delux.html");
            }
            catch (System.ComponentModel.Win32Exception noBrowser)
            {
                if (noBrowser.ErrorCode == -2147467259)
                    MessageBox.Show(noBrowser.Message);
            }
            catch (System.Exception other)
            {
                MessageBox.Show(other.Message);
            }

        }

        private DateTime RetrieveLinkerTimestamp()
        {
            string filePath = System.Reflection.Assembly.GetCallingAssembly().Location;
            const int c_PeHeaderOffset = 60;
            const int c_LinkerTimestampOffset = 8;
            byte[] b = new byte[2048];
            System.IO.Stream s = null;

            try
            {
                s = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                s.Read(b, 0, 2048);
            }
            finally
            {
                if (s != null)
                {
                    s.Close();
                }
            }

            int i = System.BitConverter.ToInt32(b, c_PeHeaderOffset);
            int secondsSince1970 = System.BitConverter.ToInt32(b, i + c_LinkerTimestampOffset);
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0);
            dt = dt.AddSeconds(secondsSince1970);
            dt = dt.AddHours(TimeZone.CurrentTimeZone.GetUtcOffset(dt).Hours);
            return dt;
        }


    }
}