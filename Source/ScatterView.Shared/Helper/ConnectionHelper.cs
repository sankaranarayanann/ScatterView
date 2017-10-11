using Android.App;
using Android.Content;
using Android.Net;

namespace ScatterView.Shared.Helper
{
    /// <summary>
    /// Helper class for network connections
    /// </summary>
    public class ConnectionHelper
    {
        #region Variables
        static ConnectivityManager connectivityManager;
        #endregion

        #region Static ctor
        /// <summary>
        /// Constructor will initialize connectivity manager
        /// </summary>
        static ConnectionHelper()
        {
            connectivityManager = (ConnectivityManager)Application.Context.GetSystemService(Context.ConnectivityService);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Property will returns is it connected to network
        /// </summary>
        public static bool IsNetworkConnected
        {
            get { return connectivityManager != null && connectivityManager.ActiveNetworkInfo != null && connectivityManager.ActiveNetworkInfo.IsConnected; }
        }
        #endregion
    }
}
