//Created By Shai Raiten
// http://blogs.microsoft.co.il/blogs/shair

using Facebook;
using Facebook.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WPCordovaClassLib.Cordova;
using WPCordovaClassLib.Cordova.Commands;

namespace org.apache.cordova.facebook
{
    public class Connect : BaseCommand
    {
        public Connect()
        {
            Settings = IsolatedStorageSettings.ApplicationSettings;
        }

        private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        internal static string AccessToken = String.Empty;
        internal static string FacebookId = String.Empty;
        public static bool isAuthenticated = false;
        public static FacebookSessionClient FacebookSessionClient;
        private FacebookSession session = null;
        private IsolatedStorageSettings Settings;

        private long CalculateTimeMillis(DateTime target)
        {
            return (long)(target - DateTime.UtcNow).TotalMilliseconds;
        }
        private async Task PerformLogin()
        {
            session = await FacebookSessionClient.LoginAsync();
        }

        public void init(string args)
        {
            var pr = new PluginResult(PluginResult.Status.OK);
            pr.KeepCallback = true;

            try
            {
                if (string.IsNullOrEmpty(args))
                {
                    DispatchCommandResult(new PluginResult(PluginResult.Status.ERROR, "You must supply Facebook Application Key"));
                    return;
                }

                var _args = WPCordovaClassLib.Cordova.JSON.JsonHelper.Deserialize<string[]>(args);
                FacebookSessionClient = new FacebookSessionClient(_args[0]);

                DateTime access_expires;

                Settings.TryGetValue<string>("access_token", out AccessToken);
                Settings.TryGetValue<DateTime>("access_expires", out  access_expires);

                if (AccessToken != null)
                    this.DispatchCommandResult(new PluginResult(PluginResult.Status.OK));
                else
                    DispatchCommandResult(new PluginResult(PluginResult.Status.NO_RESULT));
            }
            catch (Exception ex)
            {
                RemoveLocalData();
                DispatchCommandResult(new PluginResult(PluginResult.Status.ERROR, ex.Message));
            }
        }

        public void showDialog(string args)
        {
            DispatchCommandResult(new PluginResult(PluginResult.Status.OK));
        }

        private void RemoveLocalData()
        {
            if (Settings.Contains("access_token"))
                Settings.Remove("access_token");
            if (Settings.Contains("access_expires"))
                Settings.Remove("access_expires");
            Settings.Save();
        }

        public string getResponse()
        {
            String response;
            if (session != null && !string.IsNullOrEmpty(session.AccessToken))
            {
                var expiresTimeInterval = CalculateTimeMillis(session.Expires) - CalculateTimeMillis(DateTime.Now);
                var expiresIn = (expiresTimeInterval > 0) ? expiresTimeInterval : 0;
                response = "{" +
                "\"status\": \"connected\"," +
                "\"authResponse\": {" +
                  "\"accessToken\": \"" + session.AccessToken + "\"," +
                  "\"expiresIn\": \"" + expiresIn + "\"," +
                  "\"session_key\": true," +
                  "\"sig\": \"...\"," +
                  "\"userId\": \"" + session.FacebookId + "\"" +
                "}" +
              "}";
            }
            else
            {
                response = "{" +
                "\"status\": \"unknown\"" +
              "}";
            }

            return response;
        }

        private string ParseScopes(string scope)
        {
            if (string.IsNullOrEmpty(scope)) return "";

            var scopes = WPCordovaClassLib.Cordova.JSON.JsonHelper.Deserialize<string[]>(scope);
            var sb = new StringBuilder();
            foreach (var val in scopes)
                sb.Append(val + ",");

            return sb.ToString();
        }

        public void login(string scope)
        {
            try
            {
                if (FacebookSessionClient != null && FacebookSessionClient.LoginInProgress)
                {
                    this.DispatchCommandResult(new PluginResult(PluginResult.Status.NO_RESULT));
                    return;
                }

                var scopes = ParseScopes(scope);

                Deployment.Current.Dispatcher.BeginInvoke(async () =>
                {
                    session = await FacebookSessionClient.LoginAsync(scopes);
                    if (string.IsNullOrEmpty(session.AccessToken))
                        this.DispatchCommandResult(new PluginResult(PluginResult.Status.NO_RESULT));
                    else
                    {
                        RemoveLocalData();
                        Settings.Add("access_token", session.AccessToken);
                        Settings.Add("access_expires", session.Expires);
                        Settings.Save();

                        this.DispatchCommandResult(new PluginResult(PluginResult.Status.OK, this.getResponse()));
                    }
                });
            }
            catch (Exception ex)
            {
                DispatchCommandResult(new PluginResult(PluginResult.Status.JSON_EXCEPTION, ex.Message));
            }
        }

        public void logout(string args)
        {
            try
            {
                FacebookSessionClient.Logout();
                FacebookSessionCacheProvider.Current.DeleteSessionData();
                FacebookSessionIsolatedStorageCacheProvider.Current.DeleteSessionData();
                RemoveLocalData();

                this.DispatchCommandResult(new PluginResult(PluginResult.Status.OK, this.getResponse()));
            }
            catch (Exception ex)
            {
                this.DispatchCommandResult(new PluginResult(PluginResult.Status.JSON_EXCEPTION, ex.Message));
            }
        }

        public void getLoginStatus(string args)
        {
            try
            {
                if (session != null)
                    this.DispatchCommandResult(new PluginResult(PluginResult.Status.OK, this.getResponse()));
                else
                    this.DispatchCommandResult(new PluginResult(PluginResult.Status.NO_RESULT));
            }
            catch (Exception ex)
            {
                this.DispatchCommandResult(new PluginResult(PluginResult.Status.JSON_EXCEPTION, ex.Message));
            }
        }
    }
}
