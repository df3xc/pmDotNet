using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;

using System.Runtime.InteropServices;


namespace enerGenieDotNet
{
    /// <summary>
    /// dotNet control for enerGenie
    /// </summary>

    public class dotNet
    {

        private IntPtr[] udevices = new IntPtr[10]; // list of devices as unmanged pointer
        public List<deviceClass> deviceList = new List<deviceClass>(); // list of device as object

        enum DeviceType { DT_UNKNOWN = 0, DT_PMS_USB = 1, DT_PWM_USB = 2, DT_PWML_USB = 3, DT_PMS_LAN = 4, DT_PWM_LAN = 5, DT_PMS_WLAN = 6/*does not exist in mass production*/, DT_PMS2_LAN = 7, DT_PMS2_WLAN = 8 };

        const int POWERALOG_SAMPLE_PER_PAGE_COUNT = 120;
        const int POWERALOG_PAGE_COUNT = 512;

        //convert c++ CTime to c# DateTime
        static DateTime CTimeToDate(Int64 CTime)
        {
            TimeSpan span = TimeSpan.FromTicks(CTime * TimeSpan.TicksPerSecond);
            DateTime t = new DateTime(1970, 1, 1).Add(span);
            return TimeZone.CurrentTimeZone.ToLocalTime(t);
        }

        //convert c# DateTime to c++  CTime
        static Int64 DateToCTime(DateTime Date)
        {
            DateTime t = TimeZone.CurrentTimeZone.ToUniversalTime(Date);
            TimeSpan span = t.Subtract(new DateTime(1970, 1, 1));
            return (span.Ticks / TimeSpan.TicksPerSecond);

        }

        [StructLayout(LayoutKind.Explicit, Size = 20, CharSet = CharSet.Ansi)]
        public struct POWERACTIVELOG
        {
            [FieldOffset(0)]
            public double PWRActiv_d;
            [FieldOffset(8)]
            public Int64 t_devicetime; //needs to be converted to Datetime
            [FieldOffset(16)]
            public UInt32 dwDeviceID;

        }

        [StructLayout(LayoutKind.Explicit, Size = 136, CharSet = CharSet.Ansi)]
        public struct DATA_BATCH_TO_SHOW_HID
        {
            [FieldOffset(0)]
            public double Irms_d;
            [FieldOffset(8)]
            public double Vrms_d;
            [FieldOffset(16)]
            public double PWRActiv_d;
            [FieldOffset(24)]
            public double PWRFULL_d;
            [FieldOffset(32)]
            public double EnergyActiveDay_d; //signed?
            [FieldOffset(40)]
            public double EnergyFullDay_d; //signed?
            [FieldOffset(48)]
            public double EnergyActiveNight_d; //signed?
            [FieldOffset(56)]
            public double EnergyFullNight_d; //signed?
            [FieldOffset(64)]
            public Int64 tStartup; //needs to be converted to Datetime
            [FieldOffset(72)]
            public Int64 t_devicetime; //needs to be converted to Datetime
            [FieldOffset(80)]
            public double Q_d;
            [FieldOffset(88)]
            public double Cosf_d;
            [FieldOffset(96)]
            public double R_d;
            [FieldOffset(104)]
            public double X_d;
            [FieldOffset(112)]
            public double Z_d;
            [FieldOffset(120)]
            public double Freq_d;
            [FieldOffset(128)]
            public UInt32 dwDeviceID;
            [FieldOffset(132)]
            public int Seq1;
        };


        [StructLayout(LayoutKind.Explicit, Size = 16, CharSet = CharSet.Ansi)]
        public struct ENTRY_T
        {
            [FieldOffset(0)]
            public Int64 tTime; //needs to be converted to Datetime
            [FieldOffset(8)]
            public int bSwitchState;
            [FieldOffset(12)]
            public int bPeriodic;


            public ENTRY_T(DateTime t, bool bSwitch, bool p)
            {
                tTime = DateToCTime(t);
                bSwitchState = Convert.ToInt32(bSwitch);
                bPeriodic = Convert.ToInt32(p);
            }

            public DateTime GetTime()
            {
                return CTimeToDate(tTime);
            }

        };

        public struct SCHEDULE
        {
            public int nLoopTime;
            public int nCurrentEntry;
            public int nTimeLeft;
            public int nEntryCount;
            [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 16/*max schedule size*/)]
            public ENTRY_T[] entries;

        };


        /// <summary>
        /// device definition
        /// </summary>

        public struct DEVICE
        {
            public IntPtr m_device;//SISPM* or SISMP_HID*
            public int m_DeviceID;
            public int m_DeviceType;

            DEVICE(IntPtr pDevice, int dwDeviceID, int dt)
            {
                m_device = pDevice;
                m_DeviceID = dwDeviceID;
                m_DeviceType = dt;
            }

        };

        static char[] StringToCharArray(string str, int length)
        {
            return (str.PadRight(length, '\0')).ToCharArray();
        }

        public struct LAN_DEVICE
        {
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 21)]
            public String strHostname;
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 21)]
            public String strPort;
            [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 21)]
            public String strPassword;
            public bool bEnabled;
            public bool bSeconds;


            public LAN_DEVICE(string host, string Password, string Port, bool e, bool s)
            {
                strHostname = host;//StringToCharArray(host, 256);
                strPort = Port;//StringToCharArray(Port, 256);
                strPassword = Password;//StringToCharArray(Password, 256);
                bEnabled = e;
                bSeconds = s;
            }

            public int LengthOfArrays()
            {
                return strHostname.Length + strPassword.Length + strPort.Length;
            }
        };


        [DllImport("PMDLL.dll")]
        private static extern int GetDeviceList(IntPtr[] devices, IntPtr devcount, IntPtr[] landevs, int landevcount, int interface_type_to_search/*0-usb & lan, 1- usb only, 2- lan only*/);

        [DllImport("PMDLL.dll")]
        private static extern int GetPWMDataBatch(IntPtr data, IntPtr device);

        [DllImport("PMDLL.dll")]
        private static extern int GetSocketState(IntPtr device, int socket, IntPtr state, IntPtr voltage);

        [DllImport("PMDLL.dll")]
        private static extern int SetSocketState(IntPtr device, int socket, bool state);

        [DllImport("PMDLL.dll")]
        private static extern int GetInternalId(IntPtr Id, IntPtr device);

        [DllImport("PMDLL.dll")]
        private static extern int GetSocketSchedule(IntPtr device, int socket, IntPtr schedule);

        [DllImport("PMDLL.dll")]
        private static extern int SetSocketSchedule(IntPtr device, int socket, IntPtr schedule);

        [DllImport("PMDLL.dll")]
        private static extern int CloseDevice(IntPtr device);

        [DllImport("PMDLL.dll")]
        private static extern int CheckDeviceConnectionStatus(IntPtr device);

        [DllImport("PMDLL.dll")]
        private static extern int GetPWMLHardwareLog(IntPtr device, IntPtr[] samples, IntPtr count);

        public DEVICE Device;

        public dotNet()
        {
        }



        /// <summary>
        /// retrieve device list, use first in the list for further communication
        /// </summary>
        /// <returns></returns>

        public void GetDevList()
        {
            int devcount = 10; //size of array

            deviceList.Clear();

            //pointer to memory cell storing actual device array size returned from dll proc
            IntPtr ptcount = Marshal.AllocHGlobal(Marshal.SizeOf(devcount));
            Marshal.WriteInt32(ptcount, devcount);

            int bres = GetDeviceList(udevices, ptcount, null, 0, 1);

            //found devices count
            devcount = Marshal.ReadInt32(ptcount);

            //if all ok, and devices count > 0, use first device in the array for further communication
            if (bres > 0 && devcount > 0)
            {
                Device = new DEVICE();
                Device = (DEVICE)Marshal.PtrToStructure(udevices[0], typeof(DEVICE));

                // add device to the device list
                deviceClass d = new deviceClass();
                d.device = Device;
                d.devicePtr = udevices[0];
                deviceList.Add(d);

                switch ((DeviceType)Device.m_DeviceType)
                {
                    case DeviceType.DT_PWM_USB:
                    case DeviceType.DT_PWML_USB:
                    case DeviceType.DT_PWM_LAN:
                        Console.WriteLine(String.Format("Total devices found {0}. Communicating with PWM device\n", devcount));
                        break;
                    case DeviceType.DT_PMS_USB:
                    case DeviceType.DT_PMS_LAN:
                    case DeviceType.DT_PMS_WLAN:
                    case DeviceType.DT_PMS2_LAN:
                    case DeviceType.DT_PMS2_WLAN:
                        Console.WriteLine(String.Format("Total devices found {0}. Communicating with PMS device\n", devcount));
                        break;
                    default:
                        Console.WriteLine(String.Format("Total devices found {0}. Unknown device found\n", devcount));
                        break;
                }

            }
            else
            {
                Console.WriteLine(String.Format("No device found\n"));
            }

        }

 
        /// <summary>
        /// get PWM data
        /// </summary>

        public void GetPWMData(deviceClass device)
        {
            //return if no device
            if (device.devicePtr == IntPtr.Zero)
            {
                Console.WriteLine("No device found. Use \"Get device list\" button to find device.\n");
                Exception ex = new Exception("No device found. Use \"Get device list\" button to find device.");
                throw (ex);
            }

            //check device type
            switch ((DeviceType)Device.m_DeviceType)
            {
                case DeviceType.DT_PWM_USB:
                case DeviceType.DT_PWML_USB:
                case DeviceType.DT_PWM_LAN:
                    //allocate momory for data_batch structure received from device
                    DATA_BATCH_TO_SHOW_HID data = new DATA_BATCH_TO_SHOW_HID();
                    //allocated unmanaged memory pointer for the databatch structure
                    IntPtr pPointer = Marshal.AllocHGlobal(Marshal.SizeOf(data));
                    //call dll proc
                    GetPWMDataBatch(pPointer, device.devicePtr);
                    //copy data from unmanaged memory to data structure
                    data = (DATA_BATCH_TO_SHOW_HID)Marshal.PtrToStructure(pPointer, typeof(DATA_BATCH_TO_SHOW_HID));
                    //report values received from device
                    Console.WriteLine(String.Format("Device ID={0}, Vrms={1}V, Irms={2}A, Power(Active)={3}W\n", data.dwDeviceID, data.Vrms_d, data.Irms_d, data.PWRActiv_d));

                    break;
                default:
                    Exception ex = new Exception(" unkown device type ");
                    throw (ex);
            }
        }


        /// <summary>
        /// Get PMS socket state
        /// </summary>
        /// <param name="socket"></param>

        public Boolean getSwitchState(deviceClass device, int socket)
        {
            if (device.devicePtr == IntPtr.Zero)
            {
                Console.WriteLine("No device found. Use \"Get device list\" button to find device.\n");
                Exception ex = new Exception("No device found. Use \"Get device list\" button to find device.");
                throw (ex);
            }

            bool state = false; //socket state
            bool voltage = false; //socket voltage
            UInt32 Id = 0; //device id
            int bres = 0;
            //allocate unmanaged memory pointers for dll proc out params
            IntPtr ptstate = Marshal.AllocHGlobal(Marshal.SizeOf(state));
            IntPtr ptvoltage = Marshal.AllocHGlobal(Marshal.SizeOf(voltage));
            IntPtr ptId = Marshal.AllocHGlobal(Marshal.SizeOf(Id));

            //check device type
            switch ((DeviceType)device.device.m_DeviceType)
            {
                case DeviceType.DT_PMS_USB:
                case DeviceType.DT_PMS_LAN:
                case DeviceType.DT_PMS_WLAN:
                case DeviceType.DT_PMS2_LAN:
                case DeviceType.DT_PMS2_WLAN:
                    //retreive device id
                    bres = GetInternalId(ptId, device.devicePtr);
                    if (bres > 0) bres = GetSocketState(device.devicePtr, socket, ptstate, ptvoltage);
                    break;
                default:
                    Exception ex = new Exception(" unkown device type ");
                    throw (ex);
            }

            //if all ok, report result
            if (bres > 0)
            {
                Id = (UInt32)Marshal.ReadInt32(ptId);
                state = Convert.ToBoolean(Marshal.ReadByte(ptstate));
                voltage = Convert.ToBoolean(Marshal.ReadByte(ptvoltage));

                Console.WriteLine(String.Format("Device ID={0} Socket {1} is {2}, voltage {3}\n", Id, socket, state ? "ON" : "OFF", voltage ? "presents" : "does not present\n"));
                
            }
            else
            {
                Exception ex = new Exception("Dll function returned error ");
                throw (ex);
            }
            return (voltage);
        }


        /// <summary>
        /// toggle socket state
        /// </summary>
        /// <param name="socket"></param>

        public void toggleSwitchState(deviceClass device,int socket)
        {
            if (device.devicePtr == IntPtr.Zero)
            {
                Console.WriteLine("No device found. Use \"Get device list\" button to find device.\n");
                Exception ex = new Exception("No device found. Use \"Get device list\" button to find device.");
                throw (ex);
            }

            bool state = false; //socket state
            bool voltage = false; //socket voltage
            UInt32 Id = 0;
            int bres = 0;

            //allocate unmanaged memory pointers for dll proc out params
            IntPtr ptstate = Marshal.AllocHGlobal(Marshal.SizeOf(state));
            IntPtr ptvoltage = Marshal.AllocHGlobal(Marshal.SizeOf(voltage));
            IntPtr ptId = Marshal.AllocHGlobal(Marshal.SizeOf(Id));

            //check device type
            switch ((DeviceType)device.device.m_DeviceType)
            {
                case DeviceType.DT_PMS_USB:
                case DeviceType.DT_PMS_LAN:
                case DeviceType.DT_PMS_WLAN:
                case DeviceType.DT_PMS2_LAN:
                case DeviceType.DT_PMS2_WLAN:
                    //retrieve device id - call dll proc
                    bres = GetInternalId(ptId,device.devicePtr);
                    //get current socket state - call dll proc
                    if (bres > 0) bres = GetSocketState(device.devicePtr, socket, ptstate, ptvoltage);
                    if (bres > 0)
                    {
                        state = !Convert.ToBoolean(Marshal.ReadByte(ptstate));
                        //change socket state to opposite
                        bres = SetSocketState(device.devicePtr, socket, state);
                    }
                    break;
                default:
                    Exception ex = new Exception(" unkown device type ");
                    throw (ex);

            }

            //if all ok report result
            if (bres > 0)
            {
                Id = (UInt32)Marshal.ReadInt32(ptId);
                Console.WriteLine(String.Format("Device ID={0} Socket {1} was switched {2}\n", Id, socket, state ? "ON" : "OFF"));
            }
            else
            {
                Exception ex = new Exception("Dll function returned error ");
                throw (ex);
            }

        }


        /// <summary>
        /// set socket state
        /// </summary>
        /// <param name="socket"></param>

        public void setSwitchState(deviceClass device, int socket, Boolean state)
        {
            if (device.devicePtr == IntPtr.Zero)
            {
                Console.WriteLine("No device found. Use \"Get device list\" button to find device.\n");
                Exception ex = new Exception("No device found. Use \"Get device list\" button to find device.");
                throw (ex);
            }

            bool voltage = false; //socket voltage
            UInt32 Id = 0;
            int bres = 0;

            //allocate unmanaged memory pointers for dll proc out params
            IntPtr ptstate = Marshal.AllocHGlobal(Marshal.SizeOf(state));
            IntPtr ptvoltage = Marshal.AllocHGlobal(Marshal.SizeOf(voltage));
            IntPtr ptId = Marshal.AllocHGlobal(Marshal.SizeOf(Id));

            //check device type
            switch ((DeviceType)device.device.m_DeviceType)
            {
                case DeviceType.DT_PMS_USB:
                case DeviceType.DT_PMS_LAN:
                case DeviceType.DT_PMS_WLAN:
                case DeviceType.DT_PMS2_LAN:
                case DeviceType.DT_PMS2_WLAN:
                    //retrieve device id - call dll proc
                    bres = GetInternalId(ptId, device.devicePtr);
                    //get current socket state - call dll proc
                    if (bres > 0) bres = GetSocketState(device.devicePtr, socket, ptstate, ptvoltage);
                    if (bres > 0)
                    {
                        bres = SetSocketState(device.devicePtr, socket, state);
                    }
                    break;
                default:
                    Exception ex = new Exception(" unkown device type ");
                    throw (ex);

            }

            //if all ok report result
            if (bres > 0)
            {
                Id = (UInt32)Marshal.ReadInt32(ptId);
                Console.WriteLine(String.Format("Device ID={0} Socket {1} was switched {2}\n", Id, socket, state ? "ON" : "OFF"));
            }
            else
            {
                Exception ex = new Exception("Dll function returned error ");
                throw (ex);
            }

        }

        /// <summary>
        /// Get PMS socket(first socket) schedule
        /// </summary>


        public void GetPMSSchedule(deviceClass device)
        {
            if (device.devicePtr == IntPtr.Zero)
            {
                Console.WriteLine("No device found. Use \"Get device list\" button to find device.\n");
                return;
            }

            int socket = 0; //socket number from 0 to 3
            int bres = 0;

            String sCurEntry = String.Empty;

            //check device type
            switch ((DeviceType)Device.m_DeviceType)
            {

                case DeviceType.DT_PMS_USB:
                case DeviceType.DT_PMS_LAN:
                case DeviceType.DT_PMS_WLAN:
                case DeviceType.DT_PMS2_LAN:
                case DeviceType.DT_PMS2_WLAN:
                    //allocate memory for schedule structure received from device
                    SCHEDULE schedule = new SCHEDULE();
                    schedule.entries = new ENTRY_T[16/*16 is MAX entries count*/];

                    int s = Marshal.SizeOf(schedule);

                    //allocated unmanaged memory pointer for the databatch structure
                    IntPtr pPointer = Marshal.AllocHGlobal(Marshal.SizeOf(schedule));

                    //call dll proc
                    bres = GetSocketSchedule(device.devicePtr, socket, pPointer);
                    //if all ok
                    if (bres > 0)
                    {
                        schedule = (SCHEDULE)Marshal.PtrToStructure(pPointer, typeof(SCHEDULE));
                        string sch = "";
                        for (int i = 0; i < schedule.nEntryCount; i++)
                        {
                            sch += String.Format("#{0}: switch={1}, time={2:d/M/yyyy HH:mm:ss}\n", i + 1, schedule.entries[i].bSwitchState, CTimeToDate(schedule.entries[i].tTime));
                        }

                        switch (schedule.nCurrentEntry)
                        {
                            case 0: sCurEntry = "1st entry"; break;
                            case 1: sCurEntry = "2nd entry"; break;
                            case 2: sCurEntry = "3rd entry"; break;
                            case 3: sCurEntry = "4th entry"; break;
                            case 4: sCurEntry = "5th entry"; break;
                            case 5: sCurEntry = "6th entry"; break;
                            case 6: sCurEntry = "7th entry"; break;
                            case 7: sCurEntry = "8th entry"; break;
                            case 8: sCurEntry = "9th entry"; break;
                            case 9: sCurEntry = "10th entry"; break;
                            case 10: sCurEntry = "11th entry"; break;
                            case 11: sCurEntry = "12th entry"; break;
                            case 12: sCurEntry = "13th entry"; break;
                            case 13: sCurEntry = "14th entry"; break;
                            case 14: sCurEntry = "15th entry"; break;
                            case 15: sCurEntry = "16th entry"; break;
                            case -1: sCurEntry = "not set"; break;
                            default: sCurEntry = "error"; break;
                        }

                        //report actual schedule  size and current entry
                        Console.WriteLine(String.Format("Socket {0} has {1} schedule entries, current entry is {2}, \n{3}, schedule has {4}{5} loop\n", socket, schedule.nEntryCount, sCurEntry, sch, (schedule.nLoopTime == -1) ? "no" : schedule.nLoopTime.ToString(), schedule.nLoopTime == -1 ? "" : ((Device.m_DeviceType == 7 || Device.m_DeviceType == 8) ? "seconds" : "minutes")));
                    }
                    else
                    {
                        Exception ex = new Exception("Dll function returned error ");
                        throw (ex);
                    }

                    break;
                default:
                    Console.WriteLine("No PMS Device is in use\n");
                    return; //return if wrong type


            }
        }



        /// <summary>
        /// Set PMS socket (first socket) schedule 
        /// schedule consists of two entries
        /// one switches socket on, another switches it off in 1 minute
        /// schedule is looped for 5 minutes
        /// </summary>

        public void SetPMSSchedule(deviceClass device)
        {

            if (device.devicePtr == IntPtr.Zero)
            {
                Exception ex = new Exception(" No device found. Use \"Get device list\" button to find device. ");
                throw (ex);
            }

            int socket = 0; //socket number from 0 to 3, use 0 for first socket
            int bres = 0;

            //check device type
            switch ((DeviceType)Device.m_DeviceType)
            {

                case DeviceType.DT_PMS_USB:
                case DeviceType.DT_PMS_LAN:
                case DeviceType.DT_PMS_WLAN:
                case DeviceType.DT_PMS2_LAN:
                case DeviceType.DT_PMS2_WLAN:
                    //allocate memory for schedule structure
                    SCHEDULE schedule = new SCHEDULE();
                    schedule.entries = new ENTRY_T[16];

                    //create schedule of two entries
                    schedule.entries[0] = new ENTRY_T(DateTime.Now.AddMinutes(2), true, false);
                    schedule.entries[1] = new ENTRY_T(DateTime.Now.AddMinutes(3), false, false);
                    schedule.nCurrentEntry = 0;
                    if (Device.m_DeviceType == 7 || Device.m_DeviceType == 8)//loop time in seconds
                    {
                        schedule.nLoopTime = 5 * 60; //5 minutes// set "-1" for no loop;
                    }
                    else //loop time in minutes
                    {
                        schedule.nLoopTime = 5; //5 minutes// set "-1" for no loop;
                    }
                    schedule.nEntryCount = 2;//two enties
                    schedule.nTimeLeft = -1;//this is filled by device

                    int s = Marshal.SizeOf(schedule);
                    //allocated unmanaged memory pointer for the databatch structure
                    IntPtr pPointer = Marshal.AllocHGlobal(Marshal.SizeOf(schedule));
                    //put schedule to unmanaged memory
                    Marshal.StructureToPtr(schedule, pPointer, true);
                    bres = SetSocketSchedule(device.devicePtr, socket, pPointer);
                    //if all ok
                    if (bres > 0)
                    {
                        Console.WriteLine(String.Format("Schedule set for socket {0}\n", socket));
                    }
                    else
                    {
                        Exception ex = new Exception("Dll function returned error ");
                        throw (ex);
                    }

                    break;
                default:
                    Console.WriteLine("No PMS Device is in use\n");
                    return; //return if wrong type
            }
        }



        /// <summary>
        /// Close device, must be called each time communication is finished or device is not responding (lost) 
        /// </summary>
        public void CloseDevice(deviceClass device)
        {
            if (device.devicePtr == IntPtr.Zero)
            {
                Exception ex = new Exception(" No device found. Use \"Get device list\" button to find device. ");
                throw (ex);
            }

            int bres = CloseDevice(device.devicePtr);

            if (bres > 0)
            {
                device.devicePtr = IntPtr.Zero;
                Console.WriteLine("Device was closed, do not use the pointer\n");
            }
            else
            {
                Exception ex = new Exception("Dll function returned error ");
                throw (ex);
            }

        }



        /// <summary>
        /// Check if device is responding
        /// </summary>

        public void CheckConnection(deviceClass device)
        {
            if (device.devicePtr == IntPtr.Zero)
            {
                Exception ex = new Exception(" No device found. Use \"Get device list\" button to find device. ");
                throw (ex);
            }

            int bres = CheckDeviceConnectionStatus(device.devicePtr);

            if (bres > 0)
            {
                Console.WriteLine("Device connected\n");
            }
            else
            {
                Console.WriteLine("Device is not connected\n");
            }
        }

        /// <summary>
        /// Get PWML hardware log
        /// </summary>

        public void GetPWMLHardwareLog(deviceClass device)
        {
            //return if no device
            if (device.devicePtr == IntPtr.Zero)
            {
                Exception ex = new Exception(" No device found. Use \"Get device list\" button to find device. ");
                throw (ex);
            }

            switch ((DeviceType)Device.m_DeviceType)
            {
                case DeviceType.DT_PWML_USB:
                    //ok, continue procedure
                    break;
                default:
                    Console.WriteLine("No PWML device is in use\n");
                    return;

            }

            //log array containing POWERALOG_SAMPLE_PER_PAGE_COUNT*POWERALOG_PAGE_COUNT 
            int count = POWERALOG_SAMPLE_PER_PAGE_COUNT * POWERALOG_PAGE_COUNT;
            IntPtr[] samples = new IntPtr[count];

            //pointer to memory cell storing actual device array size returned from dll proc
            IntPtr ptcount = Marshal.AllocHGlobal(Marshal.SizeOf(count));
            Marshal.WriteInt32(ptcount, count);
            //call dll proc
            int bres = GetPWMLHardwareLog(device.devicePtr, samples, ptcount);

            if (bres > 0)
            {
                count = Marshal.ReadInt32(ptcount);
                if (count > 0)
                {

                    POWERACTIVELOG log = new POWERACTIVELOG();
                    log = (POWERACTIVELOG)Marshal.PtrToStructure(samples[0], typeof(POWERACTIVELOG));
                    //report first P value in the log
                    Console.WriteLine(String.Format("First log value P={0}W\n", log.PWRActiv_d));
                }
                else
                {
                    Console.WriteLine("Device log is empty\n");
                }
            }
            else
            {
                Console.WriteLine("Dll function returned with error\n");
            }
        }

    }

    /// <summary>
    /// the EnerGenie Device
    /// </summary>

    public class deviceClass
    {
        /// <summary>
        /// device information
        /// </summary>
        public dotNet.DEVICE device;
        /// <summary>
        /// Pointer to device
        /// </summary>
        public IntPtr devicePtr;
 
        public deviceClass()
        {

        }
    }

}
