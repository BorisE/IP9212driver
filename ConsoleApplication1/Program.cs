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

        public static Semaphore IP9212Semaphore;

        
        static void Main(string[] args)
        {
            tl = new TraceLogger("", "test_console");
            tl.Enabled = true; //now we can set trace state, specified by user

            IP9212Semaphore = new Semaphore(1, 2, "ip9212");


            tl.LogMessage("Main", "Started");
            Task T = checkLink_async();
            tl.LogMessage("Main", "Return from async");
            tl.LogMessage("Main", "Making something...");
            
            Thread.Sleep(500);

            tl.LogMessage("Main", "Started again");
            Task T2 = checkLink_async();
            tl.LogMessage("Main", "Return from async");

            tl.LogMessage("Main", "Finished...");
            Console.ReadLine();
        }

        class WebClientTO : WebClient
        {
            /// <summary>
            /// Time in milliseconds
            /// </summary>
            public int Timeout { get; set; }

            public WebClientTO(int timeout)
            {
                this.Timeout = timeout;
            }

            protected override WebRequest GetWebRequest(Uri address)
            {
                WebRequest request = base.GetWebRequest(address);
                if (request != null)
                {
                    request.Timeout = this.Timeout;
                }
                return request;
            }
        }

        /// <summary>
        /// Check the availability of IP server by starting async read from input sensors. Result handeled to checkLink_DownloadCompleted()
        /// </summary>
        /// <returns>Nothing</returns> 
        public static async Task checkLink_async()
        {
            string siteipURL = "http://192.168.1.99/ip9212/getio.php";
            Uri uri_siteipURL = new Uri(siteipURL);

            // Send http query
            WebClientTO client = new WebClientTO(1000);
            Task<byte[]> WebTask = new TaskCompletionSource<byte[]>().Task;
            try
            {
                IP9212Semaphore.WaitOne(); // lock working with IP9212
                
                client.Timeout = 1000;
                client.DownloadDataCompleted += new DownloadDataCompletedEventHandler(checkLink_DownloadCompleted_test);
                WebTask = client.DownloadDataTaskAsync(uri_siteipURL);
                tl.LogMessage("CheckLink_async", "http request was sent");

                await WebTask;

                IP9212Semaphore.Release();//unlock ip9212 device for others
                tl.LogMessage("CheckLink_async", "http request was downloaded");

            }
            catch (WebException e)
            {
                IP9212Semaphore.Release();//unlock ip9212 device for others
                tl.LogMessage("CheckLink_async", "error:" + e.Status + " | " + ((HttpWebResponse)e.Response).StatusCode);
                Console.WriteLine(e.Status + " | " + ((HttpWebResponse)e.Response).StatusCode);
            }

            //Parse result
            if (!WebTask.IsCompleted)
            {
                //Task isn't completed
                tl.LogMessage("CheckLink_async", "Strange error: " + WebTask.Status);
                return;
            }
            else
            {
                //Task completed
                if (WebTask.Result != null && WebTask.Result.Length > 0)
                {
                    string downloadedData = Encoding.Default.GetString(WebTask.Result);
                    if (downloadedData.IndexOf("P5") >= 0)
                    {
                        tl.LogMessage("CheckLink_async", "ok");
                    }
                    else
                    {
                        tl.LogMessage("CheckLink_async", "string not found");
                    }
                }
                else
                {
                    tl.LogMessage("CheckLink_async", "bad result");
                }
            }
        }

        private static void checkLink_DownloadCompleted_test(Object sender, DownloadDataCompletedEventArgs e)
        {
            tl.LogMessage("checkLink_DownloadCompleted_test", "passed");
            IP9212Semaphore.Release();//unlock ip9212 device for others

            if (e.Error != null)
            {
                tl.LogMessage("checkLink_DownloadCompleted", "error: " + e.Error.Message);
                Console.WriteLine(e.Error.Message);
                return;
            }

            if (e.Result != null && e.Result.Length > 0)
            {
                string downloadedData = Encoding.Default.GetString(e.Result);
                if (downloadedData.IndexOf("P5") >= 0)
                {
                    tl.LogMessage("checkLink_DownloadCompleted", "ok");
                }
                else
                {
                    tl.LogMessage("checkLink_DownloadCompleted", "string not found");
                }
            }
            else
            {
                tl.LogMessage("checkLink_DownloadCompleted", "bad result");
            }
            return;
        }
   }
}
