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

        /// <summary>
        /// Read all users from the User json file.
        /// </summary>
        /// <returns></returns>
        public static List<User> GetUsers() {
            List<User> users = ReadFile(_fullPath);
            if(users != null && users.Count > 0)
                return users;

            return null;
        }

        /// <summary>
        /// Write a new profile to the User json file.
        /// </summary>
        /// <param name="user"></param>
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

        /// <summary>
        /// Creates the file (and directory) if it does not exist.
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="file"></param>
        private static void CreateFile(string dir, string file) {
            if (!Directory.Exists(dir)) 
                Directory.CreateDirectory(dir);

            File.Create(_dir + file);
        }

        /// <summary>
        /// Reads the file and deserialize it.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Write a serializable object to the given file.
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="file"></param>
        /// <param name="value"></param>
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