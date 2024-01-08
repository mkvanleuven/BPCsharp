using Thorlabs.MotionControl.DeviceManagerCLI;
using Thorlabs.MotionControl.Benchtop.PiezoCLI;
using Thorlabs.MotionControl.GenericPiezoCLI.Piezo;
using System.Collections.Generic;
using System.Threading;
using System;

// 71281854

namespace BPC
{
    public class BPC
    {
        public string serialNo;
        public List<PiezoChannel> piezoChannels = new List<PiezoChannel>();

        public BPC(string serialNo, Boolean autoZero = false)
        {
            this.serialNo = serialNo;

            try
            {
                DeviceManagerCLI.BuildDeviceList(); // Get list of all connected devices
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception raised by BuildDeviceList {0}", ex);
                Console.ReadKey();
                return;
            }


            List<string> serialNumbers = DeviceManagerCLI.GetDeviceList();  // Get available benchtop piezo controllers and check for our serial number
                                                                            // You can pass leading digits of the serial number into GetDeviceList() to filter them

            if (!serialNumbers.Contains(serialNo))  // Check for invalid serial numbers
            {
                Console.WriteLine("{0} is not a valid serial number. Detected devices:", serialNo);
                serialNumbers.ForEach(Console.WriteLine);   // Print valid serial numbers and exit
                Console.ReadKey();
                return;
            }

            BenchtopPiezo controller = BenchtopPiezo.CreateBenchtopPiezo(serialNo); // Initialise benchtop piezo controller

            if (controller == null) // Error handling for non-benchtop piezo devices
            {
                Console.WriteLine("{0} is not a benchtop piezo controller.", serialNo);
                Console.ReadKey();
                return;
            }

            try
            {
                Console.WriteLine("Opening controller {0}", serialNo);
                controller.Connect(serialNo);   // Open connnection to controller
            }
            catch (Exception)   // Connection failed
            {
                Console.WriteLine("Could not open controller {0}", serialNo);
                Console.ReadKey();
                return;
            }

            short numChannels = controller.ChannelCount;

            for (short i = numChannels; i > 0; i--) // Looping backwards because the specific controller in our lab has the channels connected backwards (x => 3, y => 2, z => 1)
            {
                PiezoChannel channel = controller.GetChannel(i) as PiezoChannel;
                if (channel == null)    // Check for invalid channels
                {
                    Console.WriteLine("Channel {0} unavailable.", i);
                    Console.ReadKey();
                    return;
                }
                if (!channel.IsSettingsInitialized())   // Check that channel settings are initialised
                {
                    try
                    {
                        channel.WaitForSettingsInitialized(5000);   // If channel settings not initialised, wait up to 5000ms
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Could not initialise settings on channel {0} with exception {1}.", i, ex);
                        Console.ReadKey();
                        return;
                    }

                }

                this.piezoChannels.Add(channel);
                channel.StartPolling(250);
                Thread.Sleep(500);  // Good night
                channel.EnableDevice();
                Thread.Sleep(500);  // Good night
                if (autoZero)
                {
                    ZeroChannel(channel);
                }
            }
        }

        public static void SetLoopMode(PiezoChannel channel, string loopMode)   // Set loop mode of the channel
        {
            loopMode = loopMode.ToLower();
            _ = channel.RequestPositionControlMode();
            // PiezoControlModeTypes positionControlMode = channel.GetPositionControlMode();

            if (loopMode == "open")
            {
                channel.SetPositionControlMode((PiezoControlModeTypes)1);
            }
            if (loopMode == "closed")
            {
                channel.SetPositionControlMode((PiezoControlModeTypes)2);
            }
            else
            {
                Console.WriteLine("{0} is not a valid loop mode. Valid loops modes are \"open\" and \"closed\".", loopMode);
                Console.ReadKey();
                return;
            }
        }

        public static decimal GetChannelMaxVoltage(PiezoChannel channel)    // Get maximum voltage of the channel
        {
            decimal maxVoltage = channel.GetMaxOutputVoltage();
            // Console.WriteLine("Channel {0} maximum voltage is {1}V.", channel, maxVolts); // Uncomment this line to print the max voltage each time this method is called
            return maxVoltage;
        }

        public static decimal GetChannelVoltage(PiezoChannel channel)
        {
            decimal voltage = channel.GetOutputVoltage();
            // Console.WriteLine("Channel {0} voltage is {1}V.", channel, voltage); // Uncomment this line to print the voltage each time this method is called
            return voltage;
        }

        public static void MoveChannelTo(PiezoChannel channel, decimal percentage)  // Move channel to percentage of maximum value
        {
            if (!(0 <= percentage && percentage <= 100))    // Check for valid percentage value
            {
                Console.WriteLine("{0} is not a valid percentage, please choose a value in the range 0 <= percentage <= 100.", percentage);
                Console.ReadKey();
                return;
            }

            decimal maxVoltage = GetChannelMaxVoltage(channel);
            decimal setVoltage = maxVoltage * percentage / 100;

            try
            {
                // Console.WriteLine("Setting output voltage to {0}V.", setVoltage); // Uncomment this line to print the new voltage each time this method is called
                channel.SetOutputVoltage(setVoltage);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not set voltage to {0} with exception {1}.", setVoltage, ex);
                Console.ReadKey();
                return;
            }
        }

        public static void MoveChannelBy(PiezoChannel channel, decimal percentage)  // Move channel by percentage of total range
        {
            decimal maxVoltage = GetChannelMaxVoltage(channel);
            decimal currentVoltage = GetChannelVoltage(channel);
            decimal setVoltage = currentVoltage + maxVoltage * percentage / 100;

            if (!(0 <= setVoltage && setVoltage <= maxVoltage))
            {
                Console.WriteLine("{0}V is out of range. This channel is bound between 0V and {1}V.", setVoltage, maxVoltage);
                Console.ReadKey();
                return;
            }

            try
            {
                // Console.WriteLine("Setting output voltage to {0}V.", setVoltage); // Uncomment this line to print the new voltage each time this method is called
                channel.SetOutputVoltage(setVoltage);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not set voltage to {0} with exception {1}.", setVoltage, ex);
                Console.ReadKey();
                return;
            }
        }

        public static void ZeroChannel(PiezoChannel channel)    // Zero channel
        {
            try
            {
                Console.WriteLine("Zeroing channel {0}", channel);
                channel.SetZero();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to zero channel {0} with exception {1}.", channel, ex);
                Console.ReadKey();
                return;
            }
        }

        ~BPC()
        {
            // Implement destructor to properly close the piezo controller
        }

    }
}