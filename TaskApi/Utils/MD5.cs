using System.Text;

namespace TaskApi.Utils
{
    public static class MD5
    {
        private static System.Security.Cryptography.MD5 md5;
        public static string GetMD5(string str)
        {
            md5 = System.Security.Cryptography.MD5.Create();
            ASCIIEncoding encoding = new ASCIIEncoding();
            byte[] stream = null;
            StringBuilder sb = new StringBuilder();
            stream = md5.ComputeHash(encoding.GetBytes(str));
            for (int i = 0; i < stream.Length; i++) sb.AppendFormat("{0:x2}", stream[i]);
            return sb.ToString();
        }
    }
}