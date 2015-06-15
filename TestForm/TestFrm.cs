///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/* TestFrm
 * 
 * Application for testing switch driver
 * 
 * Main logic:
 * 
 * - driver can throw notconnected exception during any operation
 * - when catching notconnected exception, you need to decide: where you should automaticaly reconnect, and where let the user decide
 * - currently impelented: if exception during inital connection - give up, if inside - make one reconnect attempt and if failed - give up
 * 
 */
///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Windows.Forms;
using System.IO;
using System.Linq;

using System.Globalization;
using System.Threading;

using System.Diagnostics;
using System.Reflection;
using System.Deployment.Application;

using System.Runtime.InteropServices;

using ASCOM;

namespace ASCOM.TestForm
{
    public partial class TestFrm : Form
    {
    // P/Invoke constants
    private const int WM_SYSCOMMAND = 0x112;
    private const int MF_STRING = 0x0;
    private const int MF_SEPARATOR = 0x800;

    // P/Invoke declarations
    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool AppendMenu(IntPtr hMenu, int uFlags, int uIDNewItem, string lpNewItem);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool InsertMenu(IntPtr hMenu, int uPosition, int uFlags, int uIDNewItem, string lpNewItem);


    // ID for the About item on the system menu
    private int SYSMENU_ABOUT_ID = 0x1;

        private ASCOM.DriverAccess.Switch driver;

        public TestFrm()
        {
            InitializeComponent();
            Properties.Settings.Default.DriverId = "ASCOM.IP9212_v2.Switch";
            SetUIState_ConnectedDisconnected();

        }
        
        private void Form1_Load(object sender, EventArgs e)
        {
            numericUpDown1.Value = timer1.Interval / 100;
            driver = new ASCOM.DriverAccess.Switch(Properties.Settings.Default.DriverId);
            SetUIState_ConnectedDisconnected();
        }
        
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (IsConnected)
                driver.Connected = false;

            Properties.Settings.Default.Save();
        }

        private void buttonChoose_Click(object sender, EventArgs e)
        {
            string st= ASCOM.DriverAccess.Switch.Choose(Properties.Settings.Default.DriverId);
            Properties.Settings.Default.DriverId = st;
            SetUIState_ConnectedDisconnected();
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (IsConnected)
            {
                driver.Connected = false;
            }
            else
            {
                try
                {
                    Connect();
                }
                catch
                {
                    toolStripStatusLabel1.Text = "Couldn't connect to device" + " at " + DateTime.Now;
                    txtLog.AppendText("Couldn't connect to device" + " at " + DateTime.Now);
                    txtLog.AppendText(Environment.NewLine);
                    return;
                }
                UpdateFields();
            }
            SetUIState_ConnectedDisconnected();
        }


        /// <summary>
        /// Set UI elements according to connected/not connected events
        /// </summary>
        private void SetUIState_ConnectedDisconnected()
        {
            buttonConnect.Enabled = !string.IsNullOrEmpty(Properties.Settings.Default.DriverId);
            buttonChoose.Enabled = !IsConnected;
            buttonConnect.Text = IsConnected ? "Disconnect" : "Connect";

            toolStripStatusLabel1.Text = IsConnected ? "Connected" : "Disconnected";

            txtLog.AppendText(IsConnected ? "Connected" : "Disconnected");
            txtLog.AppendText(Environment.NewLine);

        }

        /// <summary>
        /// Update fields
        /// </summary>
        private void UpdateFields()
        {
            if (IsConnected)
            {
                DriverName.Text = driver.Name;
                DriverInfo.Text = driver.DriverInfo;
                DriverVersion.Text = driver.DriverVersion;
                DriverDescription.Text = driver.Description;

                txtNumberOfSwitches.Text = driver.MaxSwitch.ToString();

                txtIPaddr.Text = driver.Action("IPAddress","");
                txtCacheConnection.Text = driver.Action("GetCacheParameter", "CacheCheckConnection");
                txtCacheSensorData.Text = driver.Action("GetCacheParameter", "CacheSensorState");
                txtTimeout.Text = driver.Action("GetTimeout", "");

                for (int i = 1; i <= driver.MaxSwitch / 2; i++)
                {
                    try
                    {
                        ((TextBox)this.Controls.Find("txtOutput" + i, true)[0]).Text = driver.GetSwitch((short)(i - 1)).ToString();
                        ((TextBox)this.Controls.Find("txtOutName" + i, true)[0]).Text = driver.GetSwitchName((short)(i - 1)).ToString();
                        ((TextBox)this.Controls.Find("txtOutDesc" + i, true)[0]).Text = driver.GetSwitchDescription((short)(i - 1)).ToString();

                        ((TextBox)this.Controls.Find("txtInput" + i, true)[0]).Text = driver.GetSwitch((short)(i + 7)).ToString();
                        ((TextBox)this.Controls.Find("txtInName" + i, true)[0]).Text = driver.GetSwitchName((short)(i + 7)).ToString();
                        ((TextBox)this.Controls.Find("txtInDesc" + i, true)[0]).Text = driver.GetSwitchDescription((short)(i + 7)).ToString();


                        ((CheckBox)this.Controls.Find("chkOut" + i, true)[0]).Text = driver.GetSwitchName((short)(i - 1)).ToString();
                        ((CheckBox)this.Controls.Find("chkOut" + i, true)[0]).Checked = driver.GetSwitch((short)(i - 1));

                        toolTip1.SetToolTip(((CheckBox)this.Controls.Find("chkOut" + i, true)[0]), driver.GetSwitchDescription((short)(i - 1)));

                        ((CheckBox)this.Controls.Find("chkIn" + i, true)[0]).Text = driver.GetSwitchName((short)(i + 7)).ToString();
                        ((CheckBox)this.Controls.Find("chkIn" + i, true)[0]).Checked = driver.GetSwitch((short)(i + 7));

                        toolTip1.SetToolTip(((CheckBox)this.Controls.Find("chkIn" + i, true)[0]), driver.GetSwitchDescription((short)(i + 7)));

                    }
                    catch (ASCOM.NotConnectedException ex)
                    {
                        txtLog.AppendText(Environment.NewLine);
                        txtLog.AppendText("Oops, error. Press ok and we will try to reconnect" + Environment.NewLine + ex.ToString());
                        txtLog.AppendText(Environment.NewLine);
                        //MessageBox.Show("Oops, error. Press ok and we will try to reconnect" + Environment.NewLine + ex.ToString());
                        toolStripStatusLabel1.Text = "try to reconnect" + " at " + DateTime.Now;
                        txtLog.AppendText("try to reconnect" + " at " + DateTime.Now);
                        txtLog.AppendText(Environment.NewLine);
                        try
                        {
                            Connect();
                        }
                        catch
                        {
                            toolStripStatusLabel1.Text = "Couldn't reconnect" + " at " + DateTime.Now;
                            txtLog.AppendText("Couldn't reconnect" + " at " + DateTime.Now);
                            txtLog.AppendText(Environment.NewLine);
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        txtLog.AppendText(Environment.NewLine);
                        txtLog.AppendText("Unknown exception" + Environment.NewLine + ex.ToString());
                        txtLog.AppendText(Environment.NewLine);
                        MessageBox.Show("Unknown exception" + Environment.NewLine + ex.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Connect to driver
        /// </summary>
        private void Connect()
        {
//            try
            {
                driver.Connected = true;
            }
/*            catch (Exception ex)
            {
                txtLog.AppendText(Environment.NewLine);
                txtLog.AppendText("Couldn't connect to device!" + Environment.NewLine + ex.ToString());
                txtLog.AppendText(Environment.NewLine);
                //MessageBox.Show("Couldn't connect to device!" + Environment.NewLine + ex.ToString());
            }
 */
        }

        /// <summary>
        /// Connection status
        /// </summary>
        private bool IsConnected
        {
            get
            {
                return ((this.driver != null) && (driver.Connected == true));
            }
        }


        private void chkOut_Click(object sender, EventArgs e)
        {
            string chkName = ((CheckBox)sender).Name;
            short chkNum = Convert.ToInt16(chkName.Substring(6, 1));  //chkOut1 -> 0
            chkNum--;

            if (driver != null)
                driver.SetSwitch(chkNum, ((CheckBox)sender).Checked);

            txtLog.AppendText("Swtich " + chkNum + ": set state [" + ((CheckBox)sender).Checked+"]" + Environment.NewLine);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //if (!backgroundWorker1.IsBusy) backgroundWorker1.RunWorkerAsync();

            //Logging
            //UpdateLog();

            SetUIState_ConnectedDisconnected();
            UpdateFields();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            timer1.Interval = (int)numericUpDown1.Value*100;
        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            if (!IsConnected)
            {
                Connect();
            }
        }

        //need to be called manually
        private void backgroundWorker1_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            UpdateFields();
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            SetUIState_ConnectedDisconnected();
            UpdateFields();
        }

        private void UpdateLog()
        {
            try
            {
                string LogAppend = "";
                string DataFilePath = @"c:\Users\admin\Documents\ASCOM\Logs " + DateTime.Now.ToString("yyyy-MM-dd"); //2015 - 05 - 31;
                //string RGCFileName = "ASCOM.IP9212_Switch_v2." +  +".txt"; //ASCOM.IP9212_Switch_v2.1250.096980.txt

                FileInfo result = null;
                var directory = new DirectoryInfo(DataFilePath);
                var list = directory.GetFiles("ASCOM.IP9212_Switch_v2.*.txt");
                if (list.Count() > 0)
                    result = list.OrderByDescending(f => f.LastWriteTime).First();


                using (StreamReader sr = new StreamReader(result.FullName))
                {
                    LogAppend = sr.ReadToEnd();
                }

                txtLog.AppendText(LogAppend);
            }
            catch { }
        }

        private void txtLog_TextChanged(object sender, EventArgs e)
        {
            txtLog.SelectionStart = txtLog.Text.Length;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            // Get a handle to a copy of this form's system (window) menu
            IntPtr hSysMenu = GetSystemMenu(this.Handle, false);

            // Add a separator
            AppendMenu(hSysMenu, MF_SEPARATOR, 0, string.Empty);

            // Add the About menu item
            AppendMenu(hSysMenu, MF_STRING, SYSMENU_ABOUT_ID, "&About…");
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            // Test if the About item was selected from the system menu
            if ((m.Msg == WM_SYSCOMMAND) && ((int)m.WParam == SYSMENU_ABOUT_ID))
            {
                //Assembly Version
                Version AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
                string AssemblyVersionSt = AssemblyVersion.Major.ToString() + "." + AssemblyVersion.Minor.ToString() + "." + AssemblyVersion.Build.ToString() + " rev " + AssemblyVersion.Revision.ToString();

                //File Version
                string FileVersionSt = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion;

                string St = "Application fro testing ASCOM switch driver for Aviosys IP9212 power server";
                St += Environment.NewLine + "Assembly version: " + AssemblyVersionSt;
                St += Environment.NewLine + "File version: " + FileVersionSt;

                MessageBox.Show(St,"About");
            }

        }


    
    }
}
