using Syncfusion.Licensing;
using System;
using System.Configuration;
using System.Windows.Forms;
using Microsoft.Win32;

namespace TestFat
{
    static class Program
    {
        // Sets FEATURE_BROWSER_EMULATION so WebBrowser control uses IE11 rendering.
        // Required for Leaflet.js to work — without this the control defaults to IE7 mode.
        static void SetBrowserEmulationMode()
        {
            try
            {
                string exeName = System.IO.Path.GetFileName(Application.ExecutablePath);
                using (var key = Registry.CurrentUser.OpenSubKey(
                    @"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", true)
                    ?? Registry.CurrentUser.CreateSubKey(
                    @"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION"))
                {
                    key.SetValue(exeName, 11001, RegistryValueKind.DWord); // IE11 edge mode
                }
            }
            catch { /* non-fatal: map will still load, just may look different */ }
        }

        [STAThread]
        static void Main()
        {
            string syncfusionLicense = ConfigurationManager.AppSettings["syncfusionLicense"];
            // Register Syncfusion license key
            SyncfusionLicenseProvider.RegisterLicense(syncfusionLicense);

            SetBrowserEmulationMode();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var main = new Form1("Admin");
            main.StartPosition = FormStartPosition.CenterScreen;
            Application.Run(main);



        }
    }
}