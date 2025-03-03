/********************************************************************************
 *                      Module - USBDevices.cs                                  *
 *                                                                              *
 *  This module contains the implementation of the USBDevices class for         *
 *  enumerating all attached USB devices.                                       *
 *                                                                              *
 *                      ********************                                    *
 *                      * REVISION HISTORY *                                    *
 *                      ********************                                    *
 *                                                                              *
 *         Date         Author          Revisions                               *
 *      ===========     ======          =========                               *
 *      21-Mar-2018     J. Ono          * Creation.                             *
 *                                                                              *
 ********************************************************************************/
using System;
using System.Collections.Generic;
using System.Management; // reference required

namespace USBDeviceEnumerator
{
  /// <summary>
  /// This class provides functions to enumerate all attached USB devices.
  /// </summary>
  public class USBDevices
  {
    /// <summary>
    /// Prints the all attached USB devices to console.
    /// </summary>
    public static void PrintUsbDevices()
    {
      IList<ManagementBaseObject> usbDevices = GetUsbDevices();

      foreach (ManagementBaseObject usbDevice in usbDevices)
      {
        Console.WriteLine("----- DEVICE -----");
        foreach (var property in usbDevice.Properties)
        {
          Console.WriteLine(string.Format("{0}: {1}", property.Name, property.Value));
        }
        Console.WriteLine("------------------");
      }
    }   // End function PrintUsbDevices

    /// <summary>
    /// Gets a list of all attached USB devices.
    /// </summary>
    /// <returns>IList&lt;ManagementBaseObject&gt;.</returns>
    public static IList<ManagementBaseObject> GetUsbDevices()
    {
      IList<string> usbDeviceAddresses = LookUpUsbDeviceAddresses();
      List<ManagementBaseObject> usbDevices = new List<ManagementBaseObject>();
      foreach (string usbDeviceAddress in usbDeviceAddresses)
      {
        // query MI for the PNP device info
        // address must be escaped to be used in the query; luckily, the form we extracted previously is already escaped
        ManagementObjectCollection curMoc = QueryMi("Select * from Win32_PnPEntity where PNPDeviceID = " + usbDeviceAddress);
        foreach (ManagementBaseObject device in curMoc)
        {
          usbDevices.Add(device);
        }
      }
      return usbDevices;
    }   // End function GetUsbDevices

    /// <summary>
    /// Gets a list of USB device addresses.
    /// </summary>
    /// <returns>IList&lt;System.String&gt;.</returns>
    public static IList<string> LookUpUsbDeviceAddresses()
    {
      // this query gets the addressing information for connected USB devices
      ManagementObjectCollection usbDeviceAddressInfo = QueryMi(@"Select * from Win32_USBControllerDevice");

      List<string> usbDeviceAddresses = new List<string>();

      foreach(var device in usbDeviceAddressInfo)
      {
        string curPnpAddress = (string)device.GetPropertyValue("Dependent");
        // split out the address portion of the data; note that this includes escaped backslashes and quotes
        curPnpAddress = curPnpAddress.Split(new String[] { "DeviceID=" }, 2, StringSplitOptions.None)[1];

        usbDeviceAddresses.Add(curPnpAddress);
      }

      return usbDeviceAddresses;
    }   // End function LookUpUsbDeviceAddresses

    /// <summary>
    /// Runs a query against Windows Management Infrastructure (MI) and returns the resulting collection.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <returns>ManagementObjectCollection.</returns>
    public static ManagementObjectCollection QueryMi(string query)
    {
      ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(query);
      ManagementObjectCollection result = managementObjectSearcher.Get();

      managementObjectSearcher.Dispose();
      return result;
    }   // End function QueryMi
  }     // End class USBDevices
}
