using System;
using System.Net;
using System.Web;

using Exortech.NetReflector;

using ThoughtWorks.CruiseControl.Core;
using ThoughtWorks.CruiseControl.Core.Util;
using ThoughtWorks.CruiseControl.Remote;

namespace ccnet.TwitterPublisher.plugin
{
    [ReflectorType("twitter")]
    public class TwitterPublisher : ITask
    {
		private const string UPDATE_STATUS_URL = "http://twitter.com/statuses/update.xml?status={0}";

		private string _user;
        private string _password;
        private string _proxyHost;
        private int _proxyPort = -1;
        private string _proxyBypassList;
        private bool _proxyBypassOnLocal;
        private string _proxyUsername;
        private string _proxyPassword;

		#region Public properties

        [ReflectorProperty("user", Required = true)]
        public string User
        {
            get { return _user; }
            set { _user = value; }
        }

        [ReflectorProperty("password", Required = true)]
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        [ReflectorProperty("proxyHost", Required = false)]
        public string ProxyHost
        {
            get { return _proxyHost; }
            set { _proxyHost = value; }
        }

        [ReflectorProperty("proxyPort", Required = false)]
        public int ProxyPort
        {
            get { return _proxyPort; }
            set { _proxyPort = value; }
        }

        [ReflectorProperty("proxyBypassList", Required = false)]
        public string ProxyBypassList
        {
            get { return _proxyBypassList; }
            set { _proxyBypassList = value; }
        }

        [ReflectorProperty("proxyBypassOnLocal", Required = false)]
        public bool ProxyBypassOnLocal
        {
            get { return _proxyBypassOnLocal; }
            set { _proxyBypassOnLocal = value; }
        }

        [ReflectorProperty("proxyUsername", Required = false)]
        public string ProxyUsername
        {
            get { return _proxyUsername; }
            set { _proxyUsername = value; }
        }

        [ReflectorProperty("proxyPassword", Required = false)]
        public string ProxyPassword
        {
            get { return _proxyPassword; }
            set { _proxyPassword = value; }
        }

		#endregion

		public void Run(IIntegrationResult result)
        {
            if (result.Status == IntegrationStatus.Unknown)
                return;

            string status = CreateStatus(result);
			try
			{
				string url = String.Format(UPDATE_STATUS_URL, HttpUtility.UrlEncode(status));

				HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
				IWebProxy proxy = GetWebProxy();
				if (proxy != null)
					webRequest.Proxy = proxy;
				webRequest.Method = "POST";
				webRequest.ServicePoint.Expect100Continue = false;
				webRequest.KeepAlive = false;
				webRequest.ContentType = "application/xml";
				webRequest.Accept = "application/xml";
				webRequest.Credentials = new NetworkCredential(_user, _password);

				webRequest.GetResponse();

				Log.Info("Integration results published on twitter");
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
        }

        private IWebProxy GetWebProxy()
        {
			if (!String.IsNullOrEmpty(_proxyHost) && _proxyPort > 0)
            {
                WebProxy proxy = new WebProxy(_proxyHost, _proxyPort);

                if (!String.IsNullOrEmpty(_proxyBypassList))
                    proxy.BypassList = _proxyBypassList.Split(',');
                proxy.BypassProxyOnLocal = _proxyBypassOnLocal;

				if (!String.IsNullOrEmpty(_proxyUsername) && !String.IsNullOrEmpty(_proxyPassword))
                    proxy.Credentials = new NetworkCredential(_proxyUsername, _proxyPassword);

                return proxy;
            }

        	return null;
        }

        private static string CreateStatus(IIntegrationResult result)
        {
            if (result.Status == IntegrationStatus.Success)
            {
                if (result.LastIntegrationStatus != result.Status)
                {
					return String.Format("{0} Build Fixed: Build {1}. See {2}", result.ProjectName, result.Label, result.ProjectUrl);
                }
                else
                {
                    return String.Format("{0} Build Successful: Build {1}. See {2}", result.ProjectName, result.Label, result.ProjectUrl);
                }
            }
            else
            {
				return String.Format("{0} Build Failed. See {1}", result.ProjectName, result.ProjectUrl);
            }
        }
    }
}
