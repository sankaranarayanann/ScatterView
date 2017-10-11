using Android.Util;
using System;
using System.Xml;

namespace ScatterView.Shared.Helper
{
    /// <summary>
    /// Helper class to resolve the xml with the url
    /// </summary>
    public class XmlResolveHelper
    {
        /// <summary>
        /// Method will resolve the xml elements with the url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static XmlElement GetXmlElement(string url)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(url);
                XmlElement root = doc.DocumentElement;
                return root;
            }
            catch (Exception e)
            {
#if DEBUG
                Log.Error(AppConstant.AppName, e.Message, e);
#endif
                return null;
            }
        }
    }
}
