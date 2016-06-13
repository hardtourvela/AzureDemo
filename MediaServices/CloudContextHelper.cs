using Microsoft.WindowsAzure.MediaServices.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaServices
{
    static class CloudContextHelper
    {
        public static readonly string MSAccount = ConfigurationManager.AppSettings["msAccount"];
        public static readonly string MSPrimaryKey = ConfigurationManager.AppSettings["msPrimaryKey"];
        public static readonly string MSSecondaryKey = ConfigurationManager.AppSettings["msSecondaryKey"];
        public static readonly string STAccount = ConfigurationManager.AppSettings["stAccount"];
        public static readonly string STKey1 = ConfigurationManager.AppSettings["stKey1"];
        public static readonly string STKey2 = ConfigurationManager.AppSettings["stKey2"];

        private static readonly string cookie = "cookie.txt";

        public static CloudMediaContext GetContext()
        {
            string accessToken;
            DateTime tokenExpiration;
            CloudMediaContext _context;

            GetTokenInfo(out accessToken, out tokenExpiration);

            MediaServicesCredentials credentials = new MediaServicesCredentials(MSAccount, MSPrimaryKey);

            if (string.IsNullOrEmpty(credentials.AccessToken))
                credentials.RefreshToken();

            if (!string.IsNullOrEmpty(accessToken) && !tokenExpiration.Equals(DateTime.MaxValue))
            {
                credentials.AccessToken = accessToken;
                credentials.TokenExpiration = tokenExpiration;
            }

            _context = new CloudMediaContext(credentials);

            if (_context.Credentials.TokenExpiration != tokenExpiration)
                SaveTokenInfo(_context.Credentials.AccessToken, _context.Credentials.TokenExpiration);

            return _context;
        }

        private static void SaveTokenInfo(string accessToken, DateTime tokenExpiration)
        {
            string tokenInfoFormat = "{0},{1}";
            File.WriteAllText(cookie, string.Format(tokenInfoFormat, accessToken, tokenExpiration));
        }

        private static void GetTokenInfo(out string accessToken, out DateTime tokenExpiration)
        {
            accessToken = null;
            tokenExpiration = DateTime.MaxValue;

            if (!File.Exists(cookie))
                return;

            string rawTokenInfo = File.ReadAllText(cookie).Trim();

            if (string.IsNullOrEmpty(rawTokenInfo))
                return;

            string[] tokenInfo = rawTokenInfo.Split(',');
            accessToken = tokenInfo[0];
            tokenExpiration = Convert.ToDateTime(tokenInfo[1]);
        }
    }
}
