using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using ASCOM.Utilities;
using ASCOM.IP9212;

namespace ASCOM.IP9212_v2
{
    [ComVisible(false)]					// Form not registered for COM!
    public partial class SetupDialogForm : Form
    {

        public SetupDialogForm()
        {
            InitializeComponent();

            // Initialise current values of user settings from the ASCOM Profile 
            chkTrace.Checked = Switch.traceState;
        }

        private void SetupDialogForm_Load(object sender, EventArgs e)
        {
            PopulateGrid();
        }
        
        private void cmdOK_Click(object sender, EventArgs e) // OK button event handler
        {
            Switch.traceState = chkTrace.Checked;

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

    }
}