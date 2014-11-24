using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Globalization;

using ASCOM;
using ASCOM.Utilities;
using ASCOM.DeviceInterface;

namespace ASCOM.IP9212_v2
{
    /// <summary>
    /// Class for working with ip9212 device
    /// </summary>
    public class IP9212_switch_hardware_class
    {
        internal bool debugFlag = true;

        public string ip_addr, ip_port, ip_login, ip_pass;

        /// <summary>
        /// input sensors state
        /// </summary>
        private int[] input_state_arr = new int[1] { -1 };
        // [0] - overall read status
        // [1..8] - status of # input

        /// <summary>
        /// input sensors state
        /// </summary>
        private int[] output_state_arr = new int[1] { -1 };
        // [0] - overall all output switch status
        // [1..8] - status of # output

        /// <summary>
        /// connected?
        /// </summary>
        public bool hardware_connected_flag = false;

        /// <summary>
        /// Private variable to hold the trace logger object (creates a diagnostic log file with information that you specify)
        /// </summary>
        public static TraceLogger tl, tlsem;

        /// <summary>
        /// Semaphor for blocking concurrent requests
        /// </summary>
        public static Semaphore IP9212Semaphore;

        /// <summary>
        /// error message (on hardware level) - don't forget, that there is another one on driver level
        /// all this done for saving error message text during exception and display it to user (MaximDL tested)
        /// </summary>
        public string ASCOM_ERROR_MESSAGE = "";

        //Caching connection check
        public static DateTime EXPIRED_CACHE = new DateTime(2010, 05, 12, 13, 15, 00); //CONSTANT FOR MARKING AN OLD TIME
        private DateTime lastConnectedCheck = EXPIRED_CACHE; //when was the last hardware checking provided for connect state
        int CONNECTED_CHECK_INTERVAL = 20; //how often to held hardware checking (in seconds)

        /// <summary>
        /// Constructor of IP9212_switch_class
        /// </summary>
        public IP9212_switch_hardware_class(bool traceState)
        {
            tl = new TraceLogger("", "IP9212_Switch_v2_Hardware");
            tl.Enabled = traceState; //now we can set trace state, specified by user

            tlsem = new TraceLogger("", "IP9212_Switch_hardware_semaphore");
            tlsem.Enabled = true;


            tl.LogMessage("Switch_constructor", "Starting initialisation");

            hardware_connected_flag = false;

            IP9212Semaphore = new Semaphore(1, 2, "ip9212");

            tl.LogMessage("Switch_constructor", "Exit");
        }

        // if we need to ESTABLISH CONNECTION
        public void Connect()
        {
            tl.LogMessage("Switch_Connect", "Enter");

            //reset cache
            lastConnectedCheck = EXPIRED_CACHE;

            //if current state of connection coincidies with new state then do nothing
            if (hardware_connected_flag)
            {
                tl.LogMessage("Switch_Connect", "Exit because of no state change");
                return;
            }

            // check (forced) if there is connection with hardware
            if (IsConnected(true))
            {
                tl.LogMessage("Switch_Connect", "Connected");
                return;
            }
            else
            {
                tl.LogMessage("Switch_Connect", "Couldn't connect to IP9212 control device on [" + ip_addr + "]");
                ASCOM_ERROR_MESSAGE = "Couldn't connect to IP9212 control device on [" + ip_addr + "]";
                //throw new ASCOM.DriverException(ASCOM_ERROR_MESSAGE);

            }
            tl.LogMessage("Switch_Connect", "Exit");
        }

        // if we need to ESTABLISH CONNECTION
        public void Disconnect()
        {
            tl.LogMessage("Switch_Disconnect", "Enter");

            //reset cache
            lastConnectedCheck = EXPIRED_CACHE;

            //if current state of connection coincidies with new state then do nothing
            if (!hardware_connected_flag)
            {
                tl.LogMessage("Switch_Disconnect", "Exit because of no state change");
                return;
            }
            // if we need to DICONNECT - do nothing except changing _Hardware_Connected 
            hardware_connected_flag = false;
            tl.LogMessage("Switch_Disconnect", "Exit");
        }

        /// <summary>
        /// Check if device is available
        /// </summary>
        /// <param name="forcedflag">[bool] if function need to force noncached checking of device availability</param>
        /// <returns>true is available, false otherwise</returns>
        public bool IsConnected(bool forcedflag = false)
        {
            tl.LogMessage("Switch_IsConnected", "Enter (forced flag=" + forcedflag.ToString()+")");

            //Check - if forced mode? (=no cache, no async)
            if (forcedflag)
            {
                hardware_connected_flag = false;
                checkLink_forced();
                return hardware_connected_flag;
            }

            //Usual mode
            //Measure how much time have passed since last HARDWARE measure
            TimeSpan passed = DateTime.Now - lastConnectedCheck;
            if (passed.TotalSeconds > CONNECTED_CHECK_INTERVAL)
            {
                // check that the driver hardware connection exists and is connected to the hardware
                tl.LogMessage("Switch_IsConnected", String.Format("Using cached value but starting background read [in cache was: {0}s]...",passed.TotalSeconds));
                // reset cache
                lastConnectedCheck = DateTime.Now;
                
                //check data
                SynchronizationContext.SetSynchronizationContext(null); //EXPERTS SAYS THAT THIS COULD BE A PROBLEM...
                Task T = checkLink_async();

                tl.LogMessage("Switch_IsConnected", "Going further...");
                //Thread.Sleep(500);
            }
            else
            {
                // use previos value
                tl.LogMessage("Switch_IsConnected", "Using cached value [in cache:" + passed.TotalSeconds + "s]");
            }

            tl.LogMessage("Switch_IsConnected", "Exit. Return value: " + hardware_connected_flag);
            return hardware_connected_flag;
        }


        /// <summary>
        /// Check the availability of IP server by starting async read from input sensors. 
        /// </summary>
        internal async Task checkLink_async()
        {
            tl.LogMessage("CheckLink_async", "Enter");

            //Check - address was specified?
            if (string.IsNullOrEmpty(ip_addr))
            {
                hardware_connected_flag = false;
                tl.LogMessage("CheckLink_async", "ERROR (ip_addr wasn't set)!");
                // report a problem with the port name
                //throw new ASCOM.DriverException("checkLink_async error");
                return;
            }

            string siteipURL;
            siteipURL = "http://" + ip_login + ":" + ip_pass + "@" + ip_addr + ":" + ip_port + "/set.cmd?cmd=getio";
            //FOR DEBUGGING
            if (debugFlag)
            {
                siteipURL = "http://localhost/ip9212/getio.php";
            }
            Uri uri_siteipURL = new Uri(siteipURL);
            tl.LogMessage("CheckLink_async", "download url:" + siteipURL);

            // Send http query
            WebClient client = new WebClient();
            Task<byte[]> WebTask = new TaskCompletionSource<byte[]>().Task;
            try
            {
                //tl.LogMessage("Semaphore", "WaitOne");
                tlsem.LogMessage("checkLink_async", "WaitOne");
                IP9212Semaphore.WaitOne(); // lock working with IP9212

                client.DownloadDataCompleted += new DownloadDataCompletedEventHandler(checkLink_DownloadCompleted);

                WebTask = client.DownloadDataTaskAsync(uri_siteipURL);
                tl.LogMessage("CheckLink_async", "http request was sent");
                
                //Wait download complete
                await WebTask;

                //Return here after download complete
                try
                {
                    //tl.LogMessage("Semaphore", "Release");
                    IP9212Semaphore.Release();//unlock ip9212 device for others
                    tlsem.LogMessage("checkLink_async", "Release");
                }
                catch
                {
                    // Object was disposed before download complete, so we should release all and exit
                    return;
                }
                tl.LogMessage("checkLink_DownloadCompleted", "http request was processed");
            }
            catch (WebException e)
            {
                //tl.LogMessage("Semaphore", "Release");
                IP9212Semaphore.Release();//unlock ip9212 device for others
                tlsem.LogMessage("checkLink_async", "Release on exception");
            
                hardware_connected_flag = false;
                
                tl.LogMessage("CheckLink_async", "error:" + e.Status);
                tl.LogMessage("CheckLink_async", "exit on web error");

                return;
            }                
             
            //Parse result
            if (!WebTask.IsCompleted)
            {
                //Task isn't completed
                hardware_connected_flag = false;
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
                        hardware_connected_flag = true;
                        tl.LogMessage("CheckLink_async", "ok");
                    }
                    else
                    {
                        hardware_connected_flag = false;
                        tl.LogMessage("CheckLink_async", "string not found");
                    }
                }
                else
                {
                    tl.LogMessage("CheckLink_async", "bad result");
                    hardware_connected_flag = false;
                }
            }
        }

        /// <summary>
        /// Event hadler for async download. 
        /// Theoreticaly not needed with async/await but test shows that it needed for error handling!
        /// For now left also data parsing (not needed event theroretically)
        /// </summary>
        internal void checkLink_DownloadCompleted(Object sender, DownloadDataCompletedEventArgs e)
        {
            try
            {
                tl.LogMessage("checkLink_DownloadCompleted", "Download complete");
            }
            catch { 
            // Object was disposed before download complete, so we should release all and exit
                return;
            }
            if (e.Error != null)
            {
                IP9212Semaphore.Release();//unlock ip9212 device for others
                //tl.LogMessage("Semaphore", "Release");
                tlsem.LogMessage("checkLink_DownloadCompleted", "Release");

                hardware_connected_flag = false;
                tl.LogMessage("checkLink_DownloadCompleted", "error: " + e.Error.Message);
                return;
            }

            if (e.Result != null && e.Result.Length > 0)
            {
                string downloadedData = Encoding.Default.GetString(e.Result);
                if (downloadedData.IndexOf("P5") >= 0)
                {
                    hardware_connected_flag = true;
                    tl.LogMessage("checkLink_DownloadCompleted", "ok");
                }
                else
                {
                    hardware_connected_flag = false;
                    tl.LogMessage("checkLink_DownloadCompleted", "string not found");
                }
            }
            else
            {
                tl.LogMessage("checkLink_DownloadCompleted", "bad result");
                hardware_connected_flag = false;
            }
            return;
        }

        /// <summary>
        /// Check the availability of IP server by straight read (NON ASYNC manner)
        /// </summary>  
        /// <returns>Aviability of IP server </returns> 
        public bool checkLink_forced()
        {
            tl.LogMessage("checkLink_forced", "Enter");

            //Check - address was specified?
            if (string.IsNullOrEmpty(ip_addr))
            {
                hardware_connected_flag = false;
                tl.LogMessage("checkLink_forced", "ERROR (ip_addr wasn't set)!");
                // report a problem with the port name
                //throw new ASCOM.DriverException("checkLink_async error");
                return hardware_connected_flag;
            }

            string siteipURL;
            siteipURL = "http://" + ip_login + ":" + ip_pass + "@" + ip_addr + ":" + ip_port + "/set.cmd?cmd=getio";
            //FOR DEBUGGING
            if (debugFlag)
            {
                siteipURL = "http://localhost/ip9212/getio.php";
            }

            Uri uri_siteipURL = new Uri(siteipURL);
            tl.LogMessage("checkLink_forced", "Download url:" + siteipURL);

            // Send http query
            ////tl.LogMessage("Semaphore", "waitone");
            tlsem.LogMessage("checkLink_forced", "WaitOne");
            IP9212Semaphore.WaitOne(); // lock working with IP9212

            string s = "";
            WebClient client = new WebClient();
            try
            {
                Stream data = client.OpenRead(uri_siteipURL);
                StreamReader reader = new StreamReader(data);
                s = reader.ReadToEnd();
                data.Close();
                reader.Close();

                tl.LogMessage("checkLink_forced", "Download str:" + s);

                int ns = IP9212Semaphore.Release();//unlock ip9212 device for others
                tlsem.LogMessage("checkLink_forced", "Release. Left count " + ns);

                if (s.IndexOf("P5") >= 0)
                {
                    hardware_connected_flag = true;
                    tl.LogMessage("checkLink_forced", "Downloaded data is ok");
                }
                else
                {
                    hardware_connected_flag = false;
                    tl.LogMessage("checkLink_forced", "Downloaded data error - string not found");
                }
            }
            catch (WebException e)
            {
                IP9212Semaphore.Release();//unlock ip9212 device for others
                tlsem.LogMessage("checkLink_forced", "Release on webexception");

                hardware_connected_flag = false;
                tl.LogMessage("checkLink_forced", "Error" + e.Message);
                //throw new ASCOM.NotConnectedException("Couldn't reach network server");
                tl.LogMessage("checkLink_forced", "Exit by web error");
            }
            tl.LogMessage("checkLink_forced", "Exit, ret value " + hardware_connected_flag.ToString());
            return hardware_connected_flag;
        }


        /// <summary>
        /// Get input sensor status
        /// </summary>
        /// <returns>Returns bool TRUE or FALSE</returns> 
        public bool getInputSwitchStatus(int SwitchId)
        {
            tl.LogMessage("getSwitchInputStatus", "Enter");

            input_state_arr = getInputStatus();
            bool curSwitchState = (input_state_arr[SwitchId + 1] == 1);

            tl.LogMessage("getSwitchInputStatus", "getSwitchInputStatus(" + SwitchId + "):" + curSwitchState);

            tl.LogMessage("getSwitchInputStatus", "Exit");
            return curSwitchState;
        }

        /// <summary>
        /// Get output sensor status
        /// </summary>
        /// <returns>Returns bool TRUE or FALSE</returns> 
        public bool getOutputSwitchStatus(int SwitchId)
        {
            tl.LogMessage("getOutputSwitchStatus", "Enter");

            output_state_arr = getOutputStatus();
            bool curSwitchState = (output_state_arr[SwitchId + 1] == 1);

            tl.LogMessage("getOutputSwitchStatus", "getOutputSwitchStatus(" + SwitchId + "):" + curSwitchState);

            tl.LogMessage("getOutputSwitchStatus", "Exit");
            return curSwitchState;
        }


        /// <summary>
        /// Get input sensor status
        /// </summary>
        /// <returns>Returns int array [0..8] with status flags of each input sensor. arr[0] is for read status (-1 for error, 1 for good read, 0 for smth else)</returns> 
        public int[] getInputStatus()
        {
            tl.LogMessage("getInputStatus", "Enter");

            if (string.IsNullOrEmpty(ip_addr))
            {
                input_state_arr[0] = -1;
                tl.LogMessage("getInputStatus", "ERROR (ip_addr wasn't set)!");
                // report a problem with the port name
                ASCOM_ERROR_MESSAGE = "getInputStatus(): no IP address was specified";
                throw new ASCOM.ValueNotSetException(ASCOM_ERROR_MESSAGE);
                //return input_state_arr;
            }

            string siteipURL;
            siteipURL = "http://" + ip_login + ":" + ip_pass + "@" + ip_addr + ":" + ip_port + "/set.cmd?cmd=getio";
            //FOR DEBUGGING
            if (debugFlag)
            {
                siteipURL = "http://localhost/ip9212/getio.php";
            }
            tl.LogMessage("getInputStatus", "Download url:" + siteipURL);

            // Send http query
            tlsem.LogMessage("getInputStatus", "WaitOne");
            IP9212Semaphore.WaitOne(); // lock working with IP9212
            string s = "";
            WebClient client = new WebClient();
            try
            {
                Stream data = client.OpenRead(siteipURL);
                StreamReader reader = new StreamReader(data);
                s = reader.ReadToEnd();
                data.Close();
                reader.Close();

                tl.LogMessage("getInputStatus", "Download str:" + s);

                IP9212Semaphore.Release();//unlock ip9212 device for others
                tlsem.LogMessage("getInputStatus", "Release");
                //wait
                //Thread.Sleep(1000);
            }
            catch (WebException e)
            {
                IP9212Semaphore.Release();//unlock ip9212 device for others
                tlsem.LogMessage("getInputStatus", "Release on webexception");

                input_state_arr[0] = -1;

                tl.LogMessage("getInputStatus", "Error:" + e.Message);

                ASCOM_ERROR_MESSAGE = "getInputStatus(): couldn't reach network server";
                throw new ASCOM.NotConnectedException(ASCOM_ERROR_MESSAGE);
                //Trace("> IP9212_harware.getInputStatus(): exit by web error ");
                //return input_state_arr;
            }

            // Parse data
            try
            {
                // Parse result string
                string[] stringSeparators = new string[] { "P5" };
                string[] iprawdata_arr = s.Split(stringSeparators, StringSplitOptions.None);

                Array.Resize(ref input_state_arr, iprawdata_arr.Length);

                //Parse an array
                for (var i = 1; i < iprawdata_arr.Length; i++)
                {
                    //Убираем запятую
                    if (iprawdata_arr[i].Length > 3)
                    {
                        iprawdata_arr[i] = iprawdata_arr[i].Substring(0, 3);
                    }
                    //Trace(iprawdata_arr[i]);

                    //Разбиваем на пары "номер порта"="значение"
                    char[] delimiterChars = { '=' };
                    string[] data_arr = iprawdata_arr[i].Split(delimiterChars);
                    //st = st + " |" + i + ' ' + data_arr[1];
                    if (data_arr.Length > 1)
                    {
                        input_state_arr[i] = Convert.ToInt16(data_arr[1]);
                        Trace(input_state_arr[i]);
                    }
                    else
                    {
                        input_state_arr[i] = -1;
                    }
                }
                input_state_arr[0] = 1;
                tl.LogMessage("getInputStatus", "Data was read");
            }
            catch
            {
                tl.LogMessage("getInputStatus", "ERROR (Exception)!");
                input_state_arr[0] = -1;
                tl.LogMessage("getInputStatus", "Exit by parse error");
                return input_state_arr;
            }
            tl.LogMessage("getInputStatus", "Exit");
            return input_state_arr;
        }


        /// <summary>
        /// Get output relay status
        /// </summary>
        /// <returns>Returns int array [0..8] with status flags of each realya status. arr[0] is for read status (-1 for error, 1 for good read, 0 for smth else)</returns> 
        public int[] getOutputStatus()
        {
            tl.LogMessage("getOutputStatus", "Enter");

            // get the ip9212 settings from the profile
            //readSettings();

            //return data
            int[] ipdata = new int[1] { 0 };

            if (string.IsNullOrEmpty(ip_addr))
            {
                ipdata[0] = -1;
                tl.LogMessage("getOutputStatus", "ERROR (ip_addr wasn't set)!");
                // report a problem with the port name
                ASCOM_ERROR_MESSAGE = "getOutputStatus(): no IP address was specified";
                throw new ASCOM.ValueNotSetException(ASCOM_ERROR_MESSAGE);
                //return input_state_arr;

            }
            string siteipURL;
            siteipURL = "http://" + ip_login + ":" + ip_pass + "@" + ip_addr + ":" + ip_port + "/set.cmd?cmd=getpower";

            //FOR DEBUGGING
            if (debugFlag)
            {
                siteipURL = "http://localhost/ip9212/getpower.php";
            }
            tl.LogMessage("getOutputStatus", "Download url:" + siteipURL);


            // Send http query
            tlsem.LogMessage("getOutputStatus", "WaitOne");
            IP9212Semaphore.WaitOne(); // lock working with IP9212

            string s = "";
            WebClient client = new WebClient();
            try
            {
                Stream data = client.OpenRead(siteipURL);
                StreamReader reader = new StreamReader(data);
                s = reader.ReadToEnd();
                data.Close();
                reader.Close();

                tl.LogMessage("getOutputStatus", "Download str:" + s);

                IP9212Semaphore.Release();//unlock ip9212 device for others
                tlsem.LogMessage("getOutputStatus", "Release");
                //wait
                //Thread.Sleep(1000);

            }
            catch (WebException e)
            {
                IP9212Semaphore.Release();//unlock ip9212 device for others
                tlsem.LogMessage("getOutputStatus", "Release on WebException");

                ipdata[0] = -1;
                tl.LogMessage("getOutputStatus", "Error:" + e.Message);
                ASCOM_ERROR_MESSAGE = "getInputStatus(): Couldn't reach network server";
                //throw new ASCOM.NotConnectedException(ASCOM_ERROR_MESSAGE);
                Trace("> IP9212_harware.getOutputStatus(): exit by web error");
                tl.LogMessage("getOutputStatus", "Exit by web error");
                return ipdata;
            }

            // Parse data
            try
            {
                string[] stringSeparators = new string[] { "P6" };
                string[] iprawdata_arr = s.Split(stringSeparators, StringSplitOptions.None);

                Array.Resize(ref ipdata, iprawdata_arr.Length);

                //Parse an array
                for (var i = 1; i < iprawdata_arr.Length; i++)
                {
                    //Убираем запятую
                    if (iprawdata_arr[i].Length > 3)
                    {
                        iprawdata_arr[i] = iprawdata_arr[i].Substring(0, 3);
                    }
                    //Console.WriteLine(iprawdata_arr[i]);

                    //Разбиваем на пары "номер порта"="значение"
                    char[] delimiterChars = { '=' };
                    string[] data_arr = iprawdata_arr[i].Split(delimiterChars);
                    //st = st + " |" + i + ' ' + data_arr[1];
                    if (data_arr.Length > 1)
                    {
                        ipdata[i] = Convert.ToInt16(data_arr[1]);
                        Trace(ipdata[i]);
                    }
                    else
                    {
                        ipdata[i] = -1;
                    }
                }
                ipdata[0] = 1;
                tl.LogMessage("getOutputStatus", "Data was read");
            }
            catch
            {
                ipdata[0] = -1;
                tl.LogMessage("getOutputStatus", "ERROR (Exception)!");
                tl.LogMessage("getOutputStatus", "exit by parse error");
                return ipdata;
            }
            return ipdata;
        }


        /// <summary>
        /// Chage output relay state
        /// </summary>
        /// <param name="PortNumber">Relay port number, int [1..9]</param>
        /// <param name="bPortValue">Port value flase = 0, true = 1</param>
        /// <returns>Returns true in case of success</returns> 
        public bool setOutputStatus(int PortNumber, bool bPortValue)
        {
            //convert port value to int
            int intPortValue = (bPortValue ? 1 : 0);

            tl.LogMessage("setOutputStatus", "Enter (" + PortNumber + "," + intPortValue + ")");

            // get the ip9212 settings from the profile
            //readSettings();

            //return data
            bool ret = false;


            if (string.IsNullOrEmpty(ip_addr))
            {
                tl.LogMessage("setOutputStatus", "ERROR (ip_addr wasn't set)!");
                // report a problem with the port name
                ASCOM_ERROR_MESSAGE = "setOutputStatus(): no IP address was specified";
                throw new ASCOM.ValueNotSetException(ASCOM_ERROR_MESSAGE);
                //return ret;
            }
            string siteipURL = "http://" + ip_login + ":" + ip_pass + "@" + ip_addr + ":" + ip_port + "/set.cmd?cmd=setpower+P6" + PortNumber + "=" + intPortValue;
            //FOR DEBUGGING
            if (debugFlag)
            {
                siteipURL = "http://localhost/ip9212/set.php?cmd=setpower+P6" + PortNumber + "=" + intPortValue;
            }
            tl.LogMessage("setOutputStatus", "Download url:" + siteipURL);
            // Send http query
            tlsem.LogMessage("setOutputStatus", "WaitOne"); 
            IP9212Semaphore.WaitOne(); // lock working with IP9212
            string s = "";
            WebClient client = new WebClient();
            try
            {
                Stream data = client.OpenRead(siteipURL);
                StreamReader reader = new StreamReader(data);
                s = reader.ReadToEnd();
                data.Close();
                reader.Close();

                tl.LogMessage("setOutputStatus", "Download str:" + s);

                //wait
                //Thread.Sleep(1000);
                IP9212Semaphore.Release();//unlock ip9212 device for others
                tlsem.LogMessage("setOutputStatus", "Release");

                ret = true;
            }
            catch (WebException e)
            {
                IP9212Semaphore.Release();//unlock ip9212 device for others
                tlsem.LogMessage("setOutputStatus", "Release on WebException");

                ret = false;

                tl.LogMessage("setOutputStatus", "Error:" + e.Message);
                ASCOM_ERROR_MESSAGE = "setOutputStatus(" + PortNumber + "," + intPortValue + "): Couldn't reach network server";
                //throw new ASCOM.NotConnectedException(ASCOM_ERROR_MESSAGE);
                tl.LogMessage("setOutputStatus", "Exit by web error");
                return ret;
                // report a problem with the port name (never get there)
            }
            // Parse data
            // not implemented yet

            return ret;
        }

   
        /// <summary>
        /// Tracing (logging) - 3 overloaded method
        /// </summary>
        public void Trace(string st)
        {
            Console.WriteLine(st);
            try
            {
                using (StreamWriter outfile = File.AppendText("d:/ascom_ip9212_logfile.log"))
                {
                    outfile.WriteLine("{0} {1}: {2}", DateTime.Now.ToLongDateString(), DateTime.Now.ToLongTimeString(), st);
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("Write trace file error! [" + e.Message + "]");
            }
        }

        public void Trace(int st)
        {
            Console.WriteLine(st);
        }

        public void Trace(string st, int[] arr_int)
        {
            string st_out = st;
            foreach (int el in arr_int)
            {
                st_out = st_out + el + " ";
            }

            Console.WriteLine(st_out);

            try
            {
                using (StreamWriter outfile = File.AppendText("d:/ascom_ip9212_logfile.log"))
                {
                    outfile.WriteLine("{0} {1}: {2}", DateTime.Now.ToLongDateString(), DateTime.Now.ToLongTimeString(), st_out);
                }

            }
            catch (IOException e)
            {
                Console.WriteLine("Write trace file error! [" + e.Message + "]");
            }
        }


        /// <summary>
        /// Standart dispose method
        /// </summary>
        public void Dispose()
        {
            tl.Dispose();
            tl = null;

            IP9212Semaphore.Dispose();
            IP9212Semaphore = null;
        }

    }
}
