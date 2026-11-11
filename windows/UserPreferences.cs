using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TelexMini
{
    // User settings only. No keystrokes, text history, or passwords are written to disk.
    internal static class UserPreferences
    {
        static readonly Dictionary<string,string> values = Load();
        internal static string FilePath {
            get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                 "TelexMini", "preferences.ini"); }
        }
        static Dictionary<string,string> Load() {
            var v = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
            try {
                if(File.Exists(FilePath)) foreach(string line in File.ReadAllLines(FilePath,Encoding.UTF8)) {
                    int index=line.IndexOf('=');
                    if(index>0) v[line.Substring(0,index).Trim()] = line.Substring(index+1).Trim();
                }
            } catch(IOException) {} catch(UnauthorizedAccessException) {}
            return v;
        }
        internal static bool Bool(string key,bool fallback) {
            string v;
            return values.TryGetValue(key,out v) ? (v=="true" ? true : v=="false" ? false : fallback) : fallback;
        }
        internal static string String(string key,string fallback) {
            string value;
            return values.TryGetValue(key,out value) ? value : fallback;
        }
        internal static void Set(string key,bool value) { Set(key,value ? "true" : "false"); }
        internal static void Set(string key,string value) {
            values[key] = value ?? "";
            Save();
        }
        internal static void Reload() {
            values.Clear();
            foreach(var entry in Load()) values[entry.Key]=entry.Value;
        }
        static void Save() {
            try {
                string folder=Path.GetDirectoryName(FilePath);
                Directory.CreateDirectory(folder);
                var rows=new List<string>();
                foreach(var item in values) rows.Add(item.Key+"="+item.Value);
                // Atomic replacement when possible and safe fallback on first write.
                string temp=FilePath+".tmp";
                File.WriteAllLines(temp,rows.ToArray(),new UTF8Encoding(false));
                if(File.Exists(FilePath)) File.Replace(temp,FilePath,null);
                else File.Move(temp,FilePath);
            } catch(IOException) {} catch(UnauthorizedAccessException) {} catch(PlatformNotSupportedException) {}
        }
    }
}
