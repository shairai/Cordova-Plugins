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
using WPCordovaClassLib.Cordova.JSON;

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

        /**
         * TODO This function isn't implemented completely. View the similar code for the android plugin https://github.com/phonegap/phonegap-facebook-plugin.git.
         */
        public void init(string options)
        {
            try
            {
                if (string.IsNullOrEmpty(options))
                {
                    throw new Exception("Invalid JSON args used. Expected a Facebook Application Key as the first arg.");
                }

                string[] args = JsonHelper.Deserialize<string[]>(options);
                // TODO Validate whether the first element of the array exists or not.
                string facebookAppId = args[0];
                FacebookSessionClient = new FacebookSessionClient(facebookAppId);

                // TODO Validate value of this variable. For now this variable is unused.
                DateTime access_expires;

                Settings.TryGetValue<string>("access_token", out AccessToken);
                Settings.TryGetValue<DateTime>("access_expires", out  access_expires);

                if (AccessToken != null)
                    // TODO Request facebook user profile with this access token. Return the authResponse if the access token is valid.
                    this.DispatchCommandResult(new PluginResult(PluginResult.Status.NO_RESULT));
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

        public void login(string options)
        {
            try
            {
                if (FacebookSessionClient == null)
                {
                    this.DispatchCommandResult(new PluginResult(PluginResult.Status.ERROR, "Must call init before login."));
                    return;
                }

                if (FacebookSessionClient.LoginInProgress)
                {
                    this.DispatchCommandResult(new PluginResult(PluginResult.Status.NO_RESULT));
                    return;
                }

                string[] args = JsonHelper.Deserialize<string[]>(options);
                string scope = String.Join(",", fetchPermissionList(args));

                Deployment.Current.Dispatcher.BeginInvoke(async () =>
                {
                    try
                    {
                        session = await FacebookSessionClient.LoginAsync(scope);
                        if (string.IsNullOrEmpty(session.AccessToken))
                            this.DispatchCommandResult(new PluginResult(PluginResult.Status.ERROR, "Failed to login."));
                        else
                        {
                            RemoveLocalData();
                            Settings.Add("access_token", session.AccessToken);
                            Settings.Add("access_expires", session.Expires);
                            Settings.Save();

                            this.DispatchCommandResult(new PluginResult(PluginResult.Status.OK, this.getResponse()));
                        }
                    }
                    catch (InvalidOperationException e)
                    {
                        this.DispatchCommandResult(new PluginResult(PluginResult.Status.ERROR, "Failed to login."));
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

        public void getLoginStatus(string options)
        {
            try
            {
                string[] args = JsonHelper.Deserialize<string[]>(options);
                string callbackId = args[0];

                if (FacebookSessionClient != null)
                    this.DispatchCommandResult(new PluginResult(PluginResult.Status.OK, this.getResponse()), callbackId);
                else
                    DispatchCommandResult(new PluginResult(PluginResult.Status.ERROR, "Must call init before getLoginStatus."));
            }
            catch (Exception ex)
            {
                this.DispatchCommandResult(new PluginResult(PluginResult.Status.JSON_EXCEPTION, ex.Message));
            }
        }

        private string[] fetchPermissionList(string[] args)
        {
            if (args.Length < 1)
            {
                // there are no permissions
                return new string[0];
            }

            // Let's fetch all except the last element of the array. We don't need the last element of array, because it is a callbackId.
            int arrayLength = args.Length - 1;
            string[] result = new string[arrayLength];
            Array.Copy(args, result, arrayLength);
            return result;
        }

    }
}
