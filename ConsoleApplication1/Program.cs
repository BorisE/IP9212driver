using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;

using ASCOM;
using ASCOM.Utilities;
using ASCOM.DeviceInterface;


namespace ConsoleApplication1
{
    class Program
    {

        public static TraceLogger tl, tlsem;
        
        static void Main(string[] args)
        {
            tl = new TraceLogger("", "test_console");
            tl.Enabled = true; //now we can set trace state, specified by user

            tl.LogMessage("Main", "Started");
            Task T = checkLink_async();
            tl.LogMessage("Main", "Return from async");
            tl.LogMessage("Main", "Making something...");
            Thread.Sleep(10);
            tl.LogMessage("Main", "Finished...");

            Console.ReadLine();
        }



        /// <summary>
        /// Check the availability of IP server by starting async read from input sensors. Result handeled to checkLink_DownloadCompleted()
        /// </summary>
        /// <returns>Nothing</returns> 
        public static async Task checkLink_async()
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
    }
}
