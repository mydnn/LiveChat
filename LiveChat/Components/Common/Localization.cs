using DotNetNuke.Common.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Web;
using System.Xml;

namespace MyDnn.Modules.Support.LiveChat.Components.Common
{
    /// <summary>
    /// 
    /// </summary>
    public class Localization
    {
        /// <summary>
        /// 
        /// </summary>
        public static Localization Instance
        {
            get
            {
                return new Localization();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public Hashtable GetResources(string resource, string culture)
        {
            string currentCulture = (culture.ToLower() == "en-us" ? "" : "." + culture);
            var resourceFilePath = HttpContext.Current.Request.MapPath(string.Format("{0}{1}.resx", resource, currentCulture));
            if (!File.Exists(resourceFilePath))
                resourceFilePath = HttpContext.Current.Request.MapPath(string.Format("{0}.resx", resource));

            var cacheKey = Path.GetFileNameWithoutExtension(resourceFilePath);

            var result = DataCache.GetCache<Hashtable>(cacheKey);
            if (result != null)
                return result;

            result = new Hashtable();

            var d = new XmlDocument();
            bool xmlLoaded = false;
            try
            {
                d.Load(resourceFilePath);
                xmlLoaded = true;
            }
            catch
            {
                xmlLoaded = false;
            }
            if (xmlLoaded)
            {
                XmlNode n = null;
                foreach (XmlNode n_loopVariable in d.SelectNodes("root/data"))
                {
                    try
                    {

                        n = n_loopVariable;
                        if (n.NodeType != XmlNodeType.Comment)
                        {
                            string val = n.SelectSingleNode("value").InnerXml;
                            string name = n.Attributes["name"].Value;
                            if (result[name] == null)
                            {
                                result.Add(name, val);
                            }
                            else
                            {
                                result[name] = val;
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }

            DataCache.SetCache(cacheKey, result);

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="curCulture"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetString(string resource, string curCulture, string key)
        {
            var resources = GetResources(resource, curCulture);

            return resources[key].ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="curCulture"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void UpdateString(string resource, string curCulture, string key, string value)
        {
            string currentCulture = (curCulture.ToLower() == "en-us" ? "" : "." + curCulture);
            var resourceFilePath = HttpContext.Current.Request.MapPath(string.Format("{0}{1}.resx", resource, currentCulture));
            if (!File.Exists(resourceFilePath))
                resourceFilePath = HttpContext.Current.Request.MapPath(string.Format("{0}.resx", resource));

            if (!File.Exists(resourceFilePath))
                return;

            try
            {
                XmlNode node;
                XmlNode nodeData;
                XmlAttribute attr;

                var resDoc = new XmlDocument();
                resDoc.Load(resourceFilePath);

                node = resDoc.SelectSingleNode("//root/data[@name='" + key + "']/value");
                if (node == null)
                {
                    //missing entry
                    nodeData = resDoc.CreateElement("data");
                    attr = resDoc.CreateAttribute("name");
                    attr.Value = key;
                    nodeData.Attributes.Append(attr);
                    resDoc.SelectSingleNode("//root").AppendChild(nodeData);

                    node = nodeData.AppendChild(resDoc.CreateElement("value"));
                }
                node.InnerXml = value;

                resDoc.Save(resourceFilePath);
            }
            catch
            {
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="curCulture"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void UpdateStrings(string resource, string curCulture, Hashtable locales)
        {
            string currentCulture = (curCulture.ToLower() == "en-us" ? "" : "." + curCulture);
            var resourceFilePath = HttpContext.Current.Request.MapPath(string.Format("{0}{1}.resx", resource, currentCulture));
            if (!File.Exists(resourceFilePath))
                resourceFilePath = HttpContext.Current.Request.MapPath(string.Format("{0}.resx", resource));

            if (!File.Exists(resourceFilePath))
                return;

            try
            {
                XmlNode node;
                XmlNode nodeData;
                XmlAttribute attr;

                var resDoc = new XmlDocument();
                resDoc.Load(resourceFilePath);

                foreach (DictionaryEntry item in locales)
                {
                    node = resDoc.SelectSingleNode("//root/data[@name='" + item.Key + "']/value");
                    if (node == null)
                    {
                        //missing entry
                        nodeData = resDoc.CreateElement("data");
                        attr = resDoc.CreateAttribute("name");
                        attr.Value = item.Key.ToString();
                        nodeData.Attributes.Append(attr);
                        resDoc.SelectSingleNode("//root").AppendChild(nodeData);

                        node = nodeData.AppendChild(resDoc.CreateElement("value"));
                    }
                    node.InnerXml = item.Value.ToString();

                }

                resDoc.Save(resourceFilePath);

                var cacheKey = Path.GetFileNameWithoutExtension(resourceFilePath);
                DataCache.ClearCache(cacheKey);
            }
            catch
            {
            }
        }

    }
}