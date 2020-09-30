using System;
using Microsoft.Win32;

namespace VulcanClient2
{
    public class ApplicationFinder
    {
        public string getVersionKey(RegistryKey key, string p_name)
        {
            foreach (String keyName in key.GetSubKeyNames())
            {
                string displayName;
                string displayVersion;
                
                RegistryKey subkey = key.OpenSubKey(keyName);
                displayName = subkey.GetValue("DisplayName") as string;
                displayVersion = subkey.GetValue("DisplayVersion") as string;
                if (p_name.Equals(displayName, StringComparison.OrdinalIgnoreCase) == true)
                {
                    return displayVersion;
                }
            }
            return "";
        }
        public string getApplicationVersion(string p_name)
        {
            // search in: CurrentUser
            RegistryKey CurrentUserKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
            string CurrentUser = getVersionKey(CurrentUserKey, p_name);

            if (CurrentUser != "") return CurrentUser;

            RegistryKey LocalMachine32Key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
            string LocalMachine32 = getVersionKey(LocalMachine32Key, p_name);
            if (LocalMachine32 != "") return LocalMachine32;
            
            RegistryKey LocalMachine64Key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall");
            string LocalMachine64 = getVersionKey(LocalMachine64Key, p_name);
            if (LocalMachine64 != "") return LocalMachine64;
            return "";
        }
    }
}