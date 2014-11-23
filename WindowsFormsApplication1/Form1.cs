using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Threading;
using System.Globalization;
using System.Diagnostics;

using ASCOM;
using ASCOM.Utilities;
using ASCOM.DeviceInterface;


namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public static TraceLogger tl, tlsem;

        public static Semaphore IP9212Semaphore;

        int CountCalls = 0;
        
        public Form1()
        {
            tl = new TraceLogger("", "test");
            tlsem = new TraceLogger("", "test_semaphore");
            tl.Enabled = true; //now we can set trace state, specified by user
            tlsem.Enabled = true; //now we can set trace state, specified by user

            IP9212Semaphore = new Semaphore(1, 2, "ip9212_test");

            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            //SynchronizationContext.SetSynchronizationContext(null);

            await checkLink_async();
            Thread.CurrentThread.Join(5000);

            await checkLink_async();
            Thread.CurrentThread.Join(5000);

            await checkLink_async();
            Thread.CurrentThread.Join(5000);
        }


        /// <summary>
        /// Check the availability of IP server by starting async read from input sensors. Result handeled to checkLink_DownloadCompleted()
        /// </summary>
        /// <returns>Nothing</returns> 
        public async Task checkLink_async()
        {
            string siteipURL = "http://localhost/ip9212/getio.php";
            Uri uri_siteipURL = new Uri(siteipURL);

            // Send http query
            WebClient client = new WebClient();
            try
            {
                //client.DownloadDataCompleted += new DownloadDataCompletedEventHandler(checkLink_DownloadCompleted_test);
                Task t = client.DownloadDataTaskAsync(uri_siteipURL);
                tl.LogMessage("CheckLink_async", "http request was sent");
                
                await t;
                tl.LogMessage("CheckLink_async", "http request was downloaded");

            }
            catch (WebException e)
            {
                tl.LogMessage("CheckLink_async", "error:" + e.Message);
            }
        }

        private void checkLink_DownloadCompleted_test(Object sender, DownloadDataCompletedEventArgs e)
        {
            tl.LogMessage("checkLink_DownloadCompleted", "http request was processed");
        }
    }
}
