using System;
using Microsoft.Win32;
using System.Text.RegularExpressions;

namespace VulcanClient2
{
    public class ApplicationFinder
    {
        public string GetVersionKey(RegistryKey key, string p_name)
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
        public string GetVersionKeyRegex(RegistryKey key, string regex)
        {
            foreach (String keyName in key.GetSubKeyNames())
            {
                string displayName;
                string displayVersion;
                
                RegistryKey subkey = key.OpenSubKey(keyName);
                displayName = subkey.GetValue("DisplayName") as string;
                displayVersion = subkey.GetValue("DisplayVersion") as string;
                Match match = Regex.Match(displayName, regex);
                if (match.Success) return displayVersion;

                /*if (p_name.Equals(displayName, StringComparison.OrdinalIgnoreCase) == true)
                {
                    return displayVersion;
                }*/
            }
            return "";
        }
        
        
        
        public string GetApplicationVersion(string p_name)
        {
            // search in: CurrentUser
            RegistryKey CurrentUserKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
            string CurrentUser = GetVersionKey(CurrentUserKey, p_name);

            if (CurrentUser != "") return CurrentUser;

            RegistryKey LocalMachine32Key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
            string LocalMachine32 = GetVersionKey(LocalMachine32Key, p_name);
            if (LocalMachine32 != "") return LocalMachine32;
            
            RegistryKey LocalMachine64Key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall");
            string LocalMachine64 = GetVersionKey(LocalMachine64Key, p_name);
            if (LocalMachine64 != "") return LocalMachine64;
            return "";
        }

        public string GetApplicationVersionRegex(string regex)
        {
            RegistryKey CurrentUserKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
            string CurrentUser = GetVersionKeyRegex(CurrentUserKey, regex);

            if (CurrentUser != "") return CurrentUser;

            RegistryKey LocalMachine32Key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
            string LocalMachine32 = GetVersionKeyRegex(LocalMachine32Key, regex);
            if (LocalMachine32 != "") return LocalMachine32;
            
            RegistryKey LocalMachine64Key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall");
            string LocalMachine64 = GetVersionKeyRegex(LocalMachine64Key, regex);
            if (LocalMachine64 != "") return LocalMachine64;
            return "";
        }
        
        public string GetSystemDefaultBrowser()
        {
            string browserName = "Internet Explorer";
            using (RegistryKey userChoiceKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http\UserChoice"))
            {
                if (userChoiceKey != null)
                {
                    object progIdValue = userChoiceKey.GetValue("Progid");
                    if (progIdValue != null)
                    {
                        if(progIdValue.ToString().ToLower().Contains("chrome"))
                            browserName = "Google Chrome";
                        else if(progIdValue.ToString().ToLower().Contains("firefox"))
                            browserName = "Firefox";
                        else if (progIdValue.ToString().ToLower().Contains("opera"))
                            browserName = "Opera";
                        else if (progIdValue.ToString().ToLower().Contains("msedge"))
                            browserName = "Microsoft Edge";
                    }
                }
            }
            return browserName;
        }
    }
}