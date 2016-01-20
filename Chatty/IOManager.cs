using Chatty.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatty {
    public static class IOManager {
        private static string _dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Chatty";

        public static User GetUserInfo(int id) {
            string info = ReadFile(_dir + GetFileName(id));
            if(info != null && info.Length > 0)
                return JsonConvert.DeserializeObject<User>(info);

            return null;
        }

        public static void WriteUserInfo(User user, int id) {
            WriteFile(_dir, GetFileName(id), JsonConvert.SerializeObject(user));
        }

        private static void CreateFile(string dir, string file) {
            if (!Directory.Exists(dir)) 
                Directory.CreateDirectory(dir);

            File.Create(_dir + file);
        }

        private static string ReadFile(string path) {
            if (File.Exists(path)) {
                string lines = "";
                using (TextReader tr = new StreamReader(path)) {
                    lines += tr.ReadLine();
                }
                return lines;
            }
            return null;
        }

        private static void WriteFile(string dir, string file, string value) {
            if (!File.Exists(dir + file))
                CreateFile(_dir, file);

            if (value != null) {
                using (StreamWriter sw = new StreamWriter(dir + file)) {
                    sw.WriteLine(value);
                }
            }
        }

        private static string GetFileName(int id) => string.Format("/Chatty-{0}.json", id);
    }
}