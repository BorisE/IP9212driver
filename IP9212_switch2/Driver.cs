//tabs=4
// --------------------------------------------------------------------------------
// ASCOM Switch driver for IP9212 ver 2
//
// Description:	After ASCOM released ISwitchV2 specification, switch driver for Aviosys IP9212 was completely rewritten
//              It going to be used in two major projects: 
//              - WinForm application for switching power on/off (Observatory Control)
//              - Universal Dome driver based on ISwitchV2 interface
//
// Implements:	ASCOM Switch interface version: ISwitchV2
// Author:		(XXX) Boris Emchenko <support@astromania.info>
//
// Edit Log:
//
// Date			Who	Vers	Description
// -----------	---	-----	-------------------------------------------------------
// 25-10-2014	XXX 2.0.0a	Initial created from ASCOM driver template. Not working yet
// 24-11-2014	XXX 2.0.1	Beta 1. First working version 
// 25-11-2014	XXX 2.0.2	Beta 2. Change behaviour for NC contacts (ports 1 - 4). Now true means CLOSED contact, false - OPENED contact (earlier for ports 1..4: true - OPENED, false - CLOSED). Switched to Threads
// 26-11-2014	XXX 2.0.3	Beta 3. Caching input/output switches data
// 26-11-2014	XXX 2.0.4	Beta 4. Localization
// --------------------------------------------------------------------------------
//
#define Switch

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;

using ASCOM;
using ASCOM.Astrometry;
using ASCOM.Astrometry.AstroUtils;
using ASCOM.Utilities;
using ASCOM.DeviceInterface;
using System.Globalization;
using System.Collections;

namespace ASCOM.IP9212_v2
{
    //
    // Your driver's DeviceID is ASCOM.IP9212.Switch
    //
    // The Guid attribute sets the CLSID for ASCOM.IP9212.Switch
    // The ClassInterface/None addribute prevents an empty interface called
    // _IP9212 from being created and used as the [default] interface
    //

    /// <summary>
    /// ASCOM Switch Driver for IP9212.
    /// </summary>
    [Guid("d869dd76-2ed8-40a6-9186-bef53eb7d079")]
    [ClassInterface(ClassInterfaceType.None)]
    public class Switch : ISwitchV2
    {
        /// <summary>
        /// ASCOM DeviceID (COM ProgID) for this driver.
        /// The DeviceID is used by ASCOM applications to load the driver at runtime.
        /// </summary>
        internal static string driverID = "ASCOM.IP9212_v2.Switch";

        /// <summary>
        /// Hardware layer class for this Switch
        /// </summary>
        IP9212_switch_hardware_class Hardware;

        /// <summary>
        /// Private variable to hold the connected state
        /// </summary>
        private bool connectedState;
        private const bool CONNECTIONCHECK_FORCED = true; //Force to skip cache and get straight value
        private const bool CONNECTIONCHECK_CACHED = false; //Use cached checking if possible

        /// <summary>
        /// Private variable to hold the trace logger object (creates a diagnostic log file with information that you specify)
        /// </summary>
        private static TraceLogger tl;

        //Settings
        #region Settings variables        
        
        internal static string traceStateProfileName = "Trace Level";
        internal static string traceStateDefault = "true";
        internal static bool traceState = Convert.ToBoolean(traceStateDefault);

        public static string ip_addr, ip_port, ip_login, ip_pass;
        internal static string ip_addr_profilename = "IP address", ip_port_profilename = "Port number", ip_login_profilename = "login", ip_pass_profilename = "password";
        internal static string ip_addr_default = "192.168.1.90", ip_port_default = "80", ip_login_default = "admin", ip_pass_default = "12345678";

        internal static string switch_name_profilename = "switchname";
        internal static string switch_description_profilename = "switchdescription";
        // ARRAY WITH SWITCH NAMES AND DESCRIPTION
        public class switchDataClass
        {
            public string Name = "";
            public string Desc = "";
            public bool? Val = null;
        }
        public static List<switchDataClass> SwitchData = new List<switchDataClass>();

        public static string currentLang;
        internal static string currentLocalizationProfileName = "Current language";
        internal static string currentLangDefault="ru-RU";

        public static int ConnectCheck_Cache_Timeout;
        internal static string ConnectCheck_Cache_Timeout_ProfileName="ConnectCheck_Cache_Timeout";
        internal static int ConnectCheck_Cache_Timeout_def=20;

        public static int OutputRead_Cache_Timeout;
        internal static string OutputRead_Cache_Timeout_ProfileName="OutputRead_Cache_Timeout";
        internal static int OutputRead_Cache_Timeout_def=5;

        public static int InputRead_Cache_Timeout;
        internal static string InputRead_Cache_Timeout_ProfileName="InputRead_Cache_Timeout";
        internal static int InputRead_Cache_Timeout_def=5;

        #endregion Settings variables

        /// <summary>
        /// Driver description that displays in the ASCOM Chooser.
        /// </summary>
        private static string driverDescriptionShort = "Aviosys IP9212 Switch ver2";
        private static string driverDescription = "ASCOM switch driver for Aviosys IP9212 power controller based on ISwitchV2 interface. Written by Boris Emchenko http://astromania.info";

        // NUMBER OF SWITCHES
        private static short numSwitch = 16;


        /// <summary>
        /// Initializes a new instance of the <see cref="IP9212"/> class.
        /// Must be public for COM registration.
        /// </summary>
        public Switch()
        {
            tl = new TraceLogger("", "IP9212_Switch_v2");
            tl.Enabled = traceState; //set the default value for start (override further)
            tl.LogMessage("Switch", "Starting initialisation");

            //init hardware class
            Hardware = new IP9212_switch_hardware_class(traceState);

            // init SwitchData array
            for (int i = 0; i < numSwitch; i++)
            {
                SwitchData.Add(new switchDataClass { Name = "", Desc = "" });
                SwitchData[i].Name = (i < 8 ? "Output " + (i+1) : "Input " + (i-7));
                SwitchData[i].Desc = (i < 8 ? "Output switch " + (i+1) : "Input switch " + (i - 7));
            }

            readSettings(); // Read device configuration from the ASCOM Profile store
            tl.Enabled = traceState; //Now we can set the right setting

            connectedState = false; // Initialise connected to false

            tl.LogMessage("Switch", "Completed initialisation");
        }


        //
        // PUBLIC COM INTERFACE ISwitchV2 IMPLEMENTATION
        //

        #region Common properties and methods.

        /// <summary>
        /// Displays the Setup Dialog form.
        /// If the user clicks the OK button to dismiss the form, then
        /// the new settings are saved, otherwise the old values are reloaded.
        /// THIS IS THE ONLY PLACE WHERE SHOWING USER INTERFACE IS ALLOWED!
        /// </summary>
        public void SetupDialog()
        {
            // consider only showing the setup dialog if not connected
            // or call a different dialog if connected
            if (IsConnected(CONNECTIONCHECK_FORCED))
                System.Windows.Forms.MessageBox.Show("Already connected, just press OK");

            using (SetupDialogForm F = new SetupDialogForm())
            {
                var result = F.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    writeSettings(); // Persist device configuration values to the ASCOM Profile store
                }
            }
        }

        public ArrayList SupportedActions
        {
            get
            {
                tl.LogMessage("SupportedActions Get", "Returning empty arraylist");
                return new ArrayList();
            }
        }

        public string Action(string actionName, string actionParameters)
        {
            throw new ASCOM.ActionNotImplementedException("Action " + actionName + " is not implemented by this driver");
        }

        public void CommandBlind(string command, bool raw)
        {
            CheckConnected("CommandBlind");
            // Call CommandString and return as soon as it finishes
            //this.CommandString(command, raw);
            // or
            throw new ASCOM.MethodNotImplementedException("CommandBlind");
        }

        public bool CommandBool(string command, bool raw)
        {
            CheckConnected("CommandBool");
            //string ret = CommandString(command, raw);
            // TODO decode the return string and return true or false
            // or
            throw new ASCOM.MethodNotImplementedException("CommandBool");
        }

        public string CommandString(string command, bool raw)
        {
            CheckConnected("CommandString");
            // it's a good idea to put all the low level communication with the device here,
            // then all communication calls this function
            // you need something to ensure that only one command is in progress at a time

            throw new ASCOM.MethodNotImplementedException("CommandString");
        }

        public void Dispose()
        {
            // Clean up the objects
            Hardware.Dispose();
            Hardware = null;

            tl.Enabled = false;
            tl.Dispose();
            tl = null;
        }

        public bool Connected
        {
            get
            {
                bool tempIsConnFlag=IsConnected();
                tl.LogMessage("Connected Get", tempIsConnFlag.ToString());
                return tempIsConnFlag;
            }
            set
            {
                tl.LogMessage("Connected Set", value.ToString());

                if (value == IsConnected(CONNECTIONCHECK_FORCED))
                    return;

                if (value)
                {
                    tl.LogMessage("Connected Set", "Connecting to IP9212...");

                    Hardware.Connect();
                    connectedState = Hardware.hardware_connected_flag;

                    if (connectedState == false)
                    {
                        //if driver couldn't connect to ip9212 then raise an exception. 
                        throw new ASCOM.DriverException("Couldn't connect to IP9212 control device on [" + ip_addr + "]");
                    }

                }
                else
                {
                    tl.LogMessage("Connected Set", "Disconnecting from IP9212...");
                    Hardware.Disconnect();
                    connectedState = Hardware.hardware_connected_flag;

                    if (connectedState == true)
                    {
                        //if driver couldn't disconnect to ip9212 then raise an exception. 
                        throw new ASCOM.DriverException("Couldn't disconnect to IP9212 control device on [" + ip_addr + "]");
                    }
                }
            }
        }

        public string Description
        {
            get
            {
                tl.LogMessage("Description Get", driverDescription);
                return driverDescription;
            }
        }

        public string DriverInfo
        {
            get
            {
                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                string driverInfo = driverDescription + ". Version: " + String.Format(CultureInfo.InvariantCulture, "{0}.{1}", version.Major, version.Minor);
                tl.LogMessage("DriverInfo Get", driverInfo);
                return driverInfo;
            }
        }

        public string DriverVersion
        {
            get
            {
                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                string driverVersion = String.Format(CultureInfo.InvariantCulture, "{0}.{1} build {2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
                //driverVersion=version.ToString();
                tl.LogMessage("DriverVersion Get", driverVersion);
                return driverVersion;
            }
        }

        public short InterfaceVersion
        {
            get
            {
                tl.LogMessage("InterfaceVersion Get", "2");
                return Convert.ToInt16("2");
            }
        }

        public string Name
        {
            get
            {
                tl.LogMessage("Name Get", driverDescriptionShort);
                return driverDescriptionShort;
            }
        }

        #endregion

        #region ISwitchV2 Implementation

        /// <summary>
        /// The number of switches managed by this driver
        /// </summary>
        public short MaxSwitch
        {
            get
            {
                tl.LogMessage("MaxSwitch Get", "MaxSwitch = " + numSwitch.ToString());
                return numSwitch;
            }
        }

        /// <summary>
        /// Return the name of switch n
        /// </summary>
        /// <param name="id">The switch number to return</param>
        /// <returns>
        /// The name of the switch
        /// </returns>
        public string GetSwitchName(short id)
        {
            Validate("GetSwitchName", id);

            string tempStr = SwitchData[id].Name;
            tl.LogMessage("GetSwitchName", string.Format("GetSwitchName({0}) = {1}", id,tempStr));
            return tempStr;
        }

        /// <summary>
        /// Sets a switch name to a specified value
        /// </summary>
        /// <param name="id">The number of the switch whose name is to be set</param>
        /// <param name="name">The name of the switch</param>
        public void SetSwitchName(short id, string name)
        {
            Validate("SetSwitchName", id);
            tl.LogMessage("SetSwitchName", string.Format("SetSwitchName({0}): {1}", id, name));
            SwitchData[id].Name = name;
        }

        /// <summary>
        /// Gets the switch description.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public string GetSwitchDescription(short id)
        {
            Validate("GetSwitchDescription", id);

            string tempStr = SwitchData[id].Desc;
            tl.LogMessage("GetSwitchDescription", string.Format("GetSwitchDescription({0}) = {1}", id, tempStr));
            return tempStr;
        }

        /// <summary>
        /// Sets a switch description to a specified value. UNSPECIFIED BY ASCOM ISwitchV2!!!!!
        /// </summary>
        /// <param name="id">The number of the switch whose name is to be set</param>
        /// <param name="desc">The description of the switch</param>
        public void SetSwitchDescription(short id, string desc)
        {
            Validate("SetSwitchDescription", id);
            tl.LogMessage("SetSwitchDescription", string.Format("SetSwitchDescription({0}): {1}", id, desc));
            SwitchData[id].Desc = desc;
        }
        /// <summary>
        /// Reports if the specified switch can be written to.
        /// This is false if the switch cannot be written to, for example a limit switch or a sensor.
        /// The default is true.
        /// </summary>
        /// <param name="id">The number of the switch whose write state is to be returned</param><returns>
        ///   <c>true</c> if the switch can be written to, otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="MethodNotImplementedException">If the method is not implemented</exception>
        /// <exception cref="InvalidValueException">If id is outside the range 0 to MaxSwitch - 1</exception>
        public bool CanWrite(short id)
        {
            Validate("CanWrite", id);

            bool retFlag = false;
            if (id <= 7)
            {
                retFlag = true;
            }

            tl.LogMessage("CanWrite", string.Format("CanWrite({0}) = {1}", id, retFlag));
            return retFlag;
        }

        #region boolean switch members

        /// <summary>
        /// Return the state of switch n
        /// a multi-value switch must throw a not implemented exception
        /// </summary>
        /// <param name="id">The switch number to return</param>
        /// <returns>
        /// True or false
        /// </returns>
        public bool GetSwitch(short id)
        {
            Validate("GetSwitch", id);

            bool retVal = false;
            if (id <= 7)
            {
                //read value for output switch
                retVal = Hardware.getOutputSwitchStatus(id);
                //invert for NO ports (0-3)
                if (id <= 3)
                    retVal = ! retVal;

            }else{
                retVal = Hardware.getInputSwitchStatus(id - 8);
            }

            tl.LogMessage("GetSwitch", string.Format("GetSwitch({0}) = {1}", id, retVal));
            return retVal;
        }

        /// <summary>
        /// Sets a switch to the specified state
        /// If the switch cannot be set then throws a MethodNotImplementedException.
        /// A multi-value switch must throw a not implemented exception
        /// setting it to false will set it to its minimum value.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="state"></param>
        public void SetSwitch(short id, bool state)
        {
            Validate("SetSwitch", id);
            if (!CanWrite(id))
            {
                var str = string.Format("SetSwitch({0}) - Cannot Write", id);
                tl.LogMessage("SetSwitch", str);
                throw new MethodNotImplementedException(str);
            }

            bool state_correct = (id <= 3 ? !state : state);

            bool retVal = Hardware.setOutputStatus(id, state);
            tl.LogMessage("SetSwitch", string.Format("SetSwitch({0}): {1}", id, retVal));
        }

        #endregion

        #region analogue members

        /// <summary>
        /// returns the maximum value for this switch
        /// boolean switches must return 1.0
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public double MaxSwitchValue(short id)
        {
            Validate("MaxSwitchValue", id);
            // boolean switch implementation:
            tl.LogMessage("MaxSwitchValue", string.Format("MaxSwitchValue({0}): 1", id));
            return 1;
        }

        /// <summary>
        /// returns the minimum value for this switch
        /// boolean switches must return 0.0
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public double MinSwitchValue(short id)
        {
            Validate("MinSwitchValue", id);
            // boolean switch implementation:
            tl.LogMessage("MaxSwitchValue", string.Format("MaxSwitchValue({0}): 0", id));
            return 0;
        }

        /// <summary>
        /// returns the step size that this switch supports. This gives the difference between
        /// successive values of the switch.
        /// The number of values is ((MaxSwitchValue - MinSwitchValue) / SwitchStep) + 1
        /// boolean switches must return 1.0, giving two states.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public double SwitchStep(short id)
        {
            Validate("SwitchStep", id);
            // boolean switch implementation:
            tl.LogMessage("SwitchStep", string.Format("SwitchStep({0}): 1", id));
            return 1;
        }

        #region Analogue switches (not used)
        /// <summary>
        /// returns the analogue switch value for switch id
        /// boolean switches must throw a not implemented exception
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public double GetSwitchValue(short id)
        {
            Validate("GetSwitchValue", id);
            tl.LogMessage("GetSwitchValue", string.Format("GetSwitchValue({0}) - not implemented", id));
            throw new ASCOM.MethodNotImplementedException(string.Format("GetSwitchValue({0}) - not implemented", id));
        }

        /// <summary>
        /// set the analogue value for this switch.
        /// If the switch cannot be set then throws a MethodNotImplementedException.
        /// If the value is not between the maximum and minimum then throws an InvalidValueException
        /// boolean switches must throw a not implemented exception.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        public void SetSwitchValue(short id, double value)
        {
            Validate("SetSwitchValue", id, value);

            if (!CanWrite(id))
            {
                tl.LogMessage("SetSwitchValue", string.Format("SetSwitchValue({0}) - Cannot write", id));
                throw new ASCOM.MethodNotImplementedException(string.Format("SetSwitchValue({0}) - Cannot write", id));
            }
            tl.LogMessage("SetSwitchValue", string.Format("SetSwitchValue({0}) = {1} - not implemented", id, value));
            throw new ASCOM.MethodNotImplementedException(string.Format("SetSwitchValue({0}) = {1} - not implemented", id, value));
        }
        #endregion

        #endregion
        #endregion

        #region private methods

        /// <summary>
        /// Checks that the switch id is in range and throws an InvalidValueException if it isn't
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="id">The id.</param>
        private void Validate(string message, short id)
        {
            if (id < 0 || id >= numSwitch)
            {
                tl.LogMessage(message, string.Format("Switch {0} not available, range is 0 to {1}", id, numSwitch - 1));
                throw new ASCOM.InvalidValueException(message, id.ToString(), string.Format("0 to {0}", numSwitch - 1));
            }
        }

        /// <summary>
        /// Checks that the switch id and value are in range and throws an
        /// InvalidValueException if they are not.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="id">The id.</param>
        /// <param name="value">The value.</param>
        private void Validate(string message, short id, double value)
        {
            tl.LogMessage(message, string.Format("Using analogue override for validate switch {0}, value {1}", id, value));
            Validate(message, id);
            var min = MinSwitchValue(id);
            var max = MaxSwitchValue(id);
            if (value < min || value > max)
            {
                tl.LogMessage(message, string.Format("Value {1} for Switch {0} is out of the allowed range {2} to {3}", id, value, min, max));
                throw new ASCOM.InvalidValueException(message, value.ToString(), string.Format("Switch({0}) range {1} to {2}", id, min, max));
            }
        }

        /// <summary>
        /// Checks that the number of states for the switch is correct and throws a methodNotImplemented exception if not.
        /// Boolean switches must have 2 states and multi-value switches more than 2.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="id"></param>
        /// <param name="expectBoolean"></param>
        //private void Validate(string message, short id, bool expectBoolean)
        //{
        //    Validate(message, id);
        //    var ns = (int)(((MaxSwitchValue(id) - MinSwitchValue(id)) / SwitchStep(id)) + 1);
        //    if ((expectBoolean && ns != 2) || (!expectBoolean && ns <= 2))
        //    {
        //        tl.LogMessage(message, string.Format("Switch {0} has the wriong number of states", id, ns));
        //        throw new MethodNotImplementedException(string.Format("{0}({1})", message, id));
        //    }
        //}

        #endregion

        #region Private properties and methods
        // here are some useful properties and methods that can be used as required
        // to help with driver development

        #region ASCOM Registration

        // Register or unregister driver for ASCOM. This is harmless if already
        // registered or unregistered. 
        //
        /// <summary>
        /// Register or unregister the driver with the ASCOM Platform.
        /// This is harmless if the driver is already registered/unregistered.
        /// </summary>
        /// <param name="bRegister">If <c>true</c>, registers the driver, otherwise unregisters it.</param>
        private static void RegUnregASCOM(bool bRegister)
        {
            using (var P = new ASCOM.Utilities.Profile())
            {
                P.DeviceType = "Switch";
                if (bRegister)
                {
                    P.Register(driverID, driverDescriptionShort);
                }
                else
                {
                    P.Unregister(driverID);
                }
            }
        }

        /// <summary>
        /// This function registers the driver with the ASCOM Chooser and
        /// is called automatically whenever this class is registered for COM Interop.
        /// </summary>
        /// <param name="t">Type of the class being registered, not used.</param>
        /// <remarks>
        /// This method typically runs in two distinct situations:
        /// <list type="numbered">
        /// <item>
        /// In Visual Studio, when the project is successfully built.
        /// For this to work correctly, the option <c>Register for COM Interop</c>
        /// must be enabled in the project settings.
        /// </item>
        /// <item>During setup, when the installer registers the assembly for COM Interop.</item>
        /// </list>
        /// This technique should mean that it is never necessary to manually register a driver with ASCOM.
        /// </remarks>
        [ComRegisterFunction]
        public static void RegisterASCOM(Type t)
        {
            RegUnregASCOM(true);
        }

        /// <summary>
        /// This function unregisters the driver from the ASCOM Chooser and
        /// is called automatically whenever this class is unregistered from COM Interop.
        /// </summary>
        /// <param name="t">Type of the class being registered, not used.</param>
        /// <remarks>
        /// This method typically runs in two distinct situations:
        /// <list type="numbered">
        /// <item>
        /// In Visual Studio, when the project is cleaned or prior to rebuilding.
        /// For this to work correctly, the option <c>Register for COM Interop</c>
        /// must be enabled in the project settings.
        /// </item>
        /// <item>During uninstall, when the installer unregisters the assembly from COM Interop.</item>
        /// </list>
        /// This technique should mean that it is never necessary to manually unregister a driver from ASCOM.
        /// </remarks>
        [ComUnregisterFunction]
        public static void UnregisterASCOM(Type t)
        {
            RegUnregASCOM(false);
        }

        #endregion

        /// <summary>
        /// Returns true if there is a valid connection to the driver hardware
        /// </summary>
        private bool IsConnected(bool forcedflag = CONNECTIONCHECK_CACHED)
        {
            tl.LogMessage("IsConnected", "Enter" + (forcedflag == CONNECTIONCHECK_FORCED?" (forced)":" (cached)"));

            // Check that the driver hardware connection exists and is connected to the hardware
            if (!connectedState)
            {
                // if wasn't previously connected then return false
                return connectedState;
            }
            else
            {
                // if was previously connected then check in background is it still alive
                connectedState = Hardware.IsConnected(forcedflag);
            }

            tl.LogMessage("IsConnected", "Exit. Return status = " + connectedState.ToString());
            return connectedState;
        }

        /// <summary>
        /// Use this function to throw an exception if we aren't connected to the hardware
        /// </summary>
        /// <param name="message"></param>
        private void CheckConnected(string message)
        {
            tl.LogMessage("CheckConnected", "[" + message + "]");
            if (!IsConnected())
            {
                throw new ASCOM.NotConnectedException(message);
            }
        }

        /// <summary>
        /// Read the device configuration from the ASCOM Profile store
        /// </summary>
        internal void __ReadProfile()
        {
            using (Profile driverProfile = new Profile())
            {
                driverProfile.DeviceType = "Switch";
                traceState = Convert.ToBoolean(driverProfile.GetValue(driverID, traceStateProfileName, string.Empty, traceStateDefault));
                //comPort = driverProfile.GetValue(driverID, comPortProfileName, string.Empty, comPortDefault);
            }
        }

        /// <summary>
        /// Write the device configuration to the  ASCOM  Profile store
        /// </summary>
        internal void __WriteProfile()
        {
            using (Profile driverProfile = new Profile())
            {
                driverProfile.DeviceType = "Switch";
             
                driverProfile.WriteValue(driverID, traceStateProfileName, traceState.ToString());
                //driverProfile.WriteValue(driverID, comPortProfileName, comPort.ToString());
            }
        }

        
        /// <summary>
        /// Read settings from ASCOM profile storage
        /// </summary>
        internal void readSettings()
        {
            tl.LogMessage("readSettings", "Enter");
            using (ASCOM.Utilities.Profile p = new Profile())
            {
                //System.Collections.ArrayList T = p.RegisteredDeviceTypes;
                p.DeviceType = "Switch";

                //General settings
                try
                {
                    ip_addr = p.GetValue(driverID, ip_addr_profilename, string.Empty, ip_addr_default);
                }
                catch (Exception e)
                {
                    //p.WriteValue(driverID, ip_addr_profilename, ip_addr_default);
                    ip_addr = ip_addr_default;
                    tl.LogMessage("readSettings", "Wrong input string for [ip_addr]: [" + e.Message + "]");
                }
                try
                {
                    ip_port = p.GetValue(driverID, ip_port_profilename, string.Empty, ip_port_default);
                }
                catch (Exception e)
                {
                    //p.WriteValue(driverID, ip_port_profilename, ip_port_default);
                    ip_port = ip_port_default;
                    tl.LogMessage("readSettings", "Wrong input string for [ip_port]: [" + e.Message + "]");
                }
                try
                {
                    ip_login = p.GetValue(driverID, ip_login_profilename, string.Empty, ip_login_default);
                }
                catch (Exception e)
                {
                    //p.WriteValue(driverID, ip_login_profilename, ip_login_default);
                    ip_login = ip_login_default;
                    tl.LogMessage("readSettings", "Wrong input string for [ip_login]: [" + e.Message + "]");
                }

                try
                {
                    ip_pass = p.GetValue(driverID, ip_pass_profilename, string.Empty, ip_pass_default);
                }
                catch (Exception e)
                {
                    //p.WriteValue(driverID, ip_pass_profilename, ip_pass_default);
                    ip_pass = ip_pass_default;
                    tl.LogMessage("readSettings", "Wrong input string for [ip_pass]: [" + e.Message + "]");
                }

                //Set the same settings to hardware layer class
                Hardware.ip_addr = ip_addr;
                Hardware.ip_port = ip_port;
                Hardware.ip_login = ip_login;
                Hardware.ip_pass = ip_pass;

                //Trace settings
                try
                {
                    traceState = Convert.ToBoolean(p.GetValue(driverID, traceStateProfileName, string.Empty, traceStateDefault));
                }
                catch (Exception e)
                {
                    //p.WriteValue(driverID, traceStateProfileName, traceStateDefault.ToString());
                    traceState = Convert.ToBoolean(traceStateDefault);
                    tl.LogMessage("readSettings", "Input string [traceState] is not a boolean value [" + e.Message + "]");
                }


                //Language
                try
                {
                    currentLang = p.GetValue(driverID, currentLocalizationProfileName, string.Empty, currentLangDefault);
                }
                catch (Exception e)
                {
                    currentLang = currentLangDefault;
                    tl.LogMessage("readSettings", "Wrong input string for [currentLang]: [" + e.Message + "]");
                }


                //Cache settings
                //p.WriteValue(driverID, ConnectCheck_Cache_Timeout_ProfileName, ConnectCheck_Cache_Timeout.ToString());
                //p.WriteValue(driverID, OutputRead_Cache_Timeout_ProfileName, OutputRead_Cache_Timeout.ToString());
                //p.WriteValue(driverID, InputRead_Cache_Timeout_ProfileName, InputRead_Cache_Timeout.ToString());


                //Switch data
                for (int i = 0; i < numSwitch/2; i++)
                {
                    //Output port name
                    try
                    {
                        SwitchData[i].Name = p.GetValue(driverID, switch_name_profilename, "Out_" + (i + 1), "Output " + i );
                    }
                    catch (Exception e)
                    {
                        //p.WriteValue(driverID, switch_name_profilename, SwitchData[i].Name, "Out_" + (i + 1));
                        SwitchData[i].Name = "Output " + i;
                        tl.LogMessage("readSettings", "Wrong input string for [Output name " + i + "]: [" + e.Message + "]");
                    }

                    //Output port description
                    try
                    {
                        SwitchData[i].Desc = p.GetValue(driverID, switch_description_profilename, "Out_" + (i + 1), "Output switch " + i);
                    }
                    catch (Exception e)
                    {
                        //p.WriteValue(driverID, switch_description_profilename, SwitchData[i].Desc, "Out_" + (i + 1));
                        SwitchData[i].Desc = "Output switch " + i;
                        tl.LogMessage("readSettings", "Wrong input string for [Output description " + i + "]: [" + e.Message + "]");
                    }

                    //Input port name
                    try
                    {
                        SwitchData[i+8].Name = p.GetValue(driverID, switch_name_profilename, "In_" + (i + 1), "Input " + i);
                    }
                    catch (Exception e)
                    {
                        //p.WriteValue(driverID, switch_name_profilename, SwitchData[i].Name, "In_" + (i + 1));
                        SwitchData[i + 8].Name = "Input " + i;
                        tl.LogMessage("readSettings", "Wrong input string for [Input name " + i + "]: [" + e.Message + "]");
                    }
                    //Input port description
                    try
                    {
                        SwitchData[i+8].Desc = p.GetValue(driverID, switch_description_profilename, "In_" + (i + 1), "Input switch " + i);
                    }
                    catch (Exception e)
                    {
                        //p.WriteValue(driverID, switch_description_profilename, SwitchData[i].Desc, "In_" + (i + 1));
                        SwitchData[i+8].Desc = "Input switch " + i;
                        tl.LogMessage("readSettings", "Wrong input string for [Input description " + i + "]: [" + e.Message + "]");
                    }
                }
            }

            tl.LogMessage("readSettings", "Exit");
        }

        /// <summary>
        /// Write settings to ASCOM profile storage
        /// </summary>
        internal static void writeSettings()
        {
            tl.LogMessage("writeSettings", "Enter");
            using (Profile p = new Profile())
            {
                p.DeviceType = "Switch";

                //General settings
                p.WriteValue(driverID, ip_addr_profilename, ip_addr);
                p.WriteValue(driverID, ip_port_profilename, ip_port);
                p.WriteValue(driverID, ip_login_profilename, ip_login);
                p.WriteValue(driverID, ip_pass_profilename, ip_pass);

                //Trace value
                p.WriteValue(driverID, traceStateProfileName, traceState.ToString());

                //Language
                p.WriteValue(driverID, currentLocalizationProfileName, currentLang);

                //Cache settings
                p.WriteValue(driverID, ConnectCheck_Cache_Timeout_ProfileName, ConnectCheck_Cache_Timeout.ToString());
                p.WriteValue(driverID, OutputRead_Cache_Timeout_ProfileName, OutputRead_Cache_Timeout.ToString());
                p.WriteValue(driverID, InputRead_Cache_Timeout_ProfileName, InputRead_Cache_Timeout.ToString());

                //Switch data
                for (int i = 0; i < numSwitch/2; i++)
                {
                    //Output port
                    p.WriteValue(driverID, switch_name_profilename, SwitchData[i].Name, "Out_"+(i+1));
                    p.WriteValue(driverID, switch_description_profilename, SwitchData[i].Desc, "Out_" + (i + 1));

                    //Input port
                    p.WriteValue(driverID, switch_name_profilename, SwitchData[i+8].Name, "In_" + (i + 1));
                    p.WriteValue(driverID, switch_description_profilename, SwitchData[i+8].Desc, "In_" + (i + 1));
                }

            }
            tl.LogMessage("writeSettings", "Exit");
        }



        #endregion

    }
}
