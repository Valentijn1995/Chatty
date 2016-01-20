using Chatty.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Chatty
{
    public static class ProfileManager {
        private static string _dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Chatty";
        private static string _file = "/Chatty.json";
        private static string _fullPath = _dir + _file;

        public static List<User> GetUsers() {
            List<User> users = ReadFile(_fullPath);
            if(users != null && users.Count > 0)
                return users;

            return null;
        }

        public static void WriteUserInfo(User user) {
            List<User> currentUsers = ReadFile(_fullPath);
            if(currentUsers != null) {
                currentUsers.Add(user);
                WriteFile(_dir, _file, currentUsers);
            }
            else {
                WriteFile(_dir, _file, new List<User> { user });
            }
        }

        private static void CreateFile(string dir, string file) {
            if (!Directory.Exists(dir)) 
                Directory.CreateDirectory(dir);

            File.Create(_dir + file);
        }

        private static List<User> ReadFile(string path) {
            if (File.Exists(path)) {
                List<User> users = null;
                using(StreamReader r = new StreamReader(path)) {
                    string json = r.ReadToEnd();
                    try {
                        users = JsonConvert.DeserializeObject<List<User>>(json);
                    }
                    catch { }
                }
                return users;
            }
            return null;
        }

        private static void WriteFile(string dir, string file, object value) {
            if (!File.Exists(dir + file))
                CreateFile(_dir, file);

            if(value != null) {
                using(StreamWriter sw = new StreamWriter(dir + file, false)) {
                    string json = JsonConvert.SerializeObject(value);
                    sw.WriteLine(json);
                }
            }
        }
    }
}