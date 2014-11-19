using System;
using System.Windows.Forms;

namespace ASCOM.TestForm
{
    public partial class TestFrm : Form
    {

        private ASCOM.DriverAccess.Switch driver;

        public TestFrm()
        {
            InitializeComponent();
            SetUIState();
        }
        
        private void Form1_Load(object sender, EventArgs e)
        {

        }
        
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (IsConnected)
                driver.Connected = false;

            Properties.Settings.Default.Save();
        }

        private void buttonChoose_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.DriverId = ASCOM.DriverAccess.Switch.Choose(Properties.Settings.Default.DriverId);
            SetUIState();
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            if (IsConnected)
            {
                driver.Connected = false;
            }
            else
            {
                driver = new ASCOM.DriverAccess.Switch(Properties.Settings.Default.DriverId);
                driver.Connected = true;
                UpdateFields();
            }
            SetUIState();
        }

        private void SetUIState()
        {
            buttonConnect.Enabled = !string.IsNullOrEmpty(Properties.Settings.Default.DriverId);
            buttonChoose.Enabled = !IsConnected;
            buttonConnect.Text = IsConnected ? "Disconnect" : "Connect";

        }

        private void UpdateFields()
        {
            DriverName.Text = driver.Name;
            DriverInfo.Text = driver.DriverInfo;
            DriverVersion.Text = driver.DriverVersion;
            DriverDescription.Text = driver.Description;


            txtNumberOfSwitches.Text = driver.MaxSwitch.ToString();

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

                    ((CheckBox)this.Controls.Find("chkIn" + i, true)[0]).Text = driver.GetSwitchName((short)(i + 7)).ToString();
                    ((CheckBox)this.Controls.Find("chkIn" + i, true)[0]).Checked = driver.GetSwitch((short)(i + 7));
                }
                catch { }
            }
        }

        private bool IsConnected
        {
            get
            {
                return ((this.driver != null) && (driver.Connected == true));
            }
        }


        private void chkOut_CheckedChanged(object sender, EventArgs e)
        {

            string chkName = ((CheckBox)sender).Name;
            short chkNum = Convert.ToInt16(chkName.Substring(6,1));  //chkOut1
            
            if (driver != null)
                driver.SetSwitch(chkNum, ((CheckBox)sender).Checked);
        }


        private void chkOut_Click(object sender, EventArgs e)
        {


            string chkName = ((CheckBox)sender).Name;
            short chkNum = Convert.ToInt16(chkName.Substring(6, 1));  //chkOut1

            if (driver != null)
                driver.SetSwitch(chkNum, ((CheckBox)sender).Checked);
        }        
    
    }
}
