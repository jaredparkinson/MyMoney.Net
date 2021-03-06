﻿namespace Walkabout.Ofx
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Xml.Linq;
    using Walkabout.Data;

    /// <summary>
    /// This class represents a field that has date/time of last change information so we
    /// can merge do a better job of merging this information when it comes from multiple
    /// places.  THe end user can edit it, and we download updates from http://www.ofxhome.com 
    /// </summary>
    public class ChangeTrackedField
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public DateTime? LastChange { get; set; }
    }

    public class OfxInstitutionInfo
    {
        public string Id
        {
            get { return GetValue<string>("Id"); }
            set { SetValue("Id", value); }
        }

        public string Name
        {
            get { return GetValue<string>("Name"); }
            set { SetValue("Name", value); }

        }
        public string OfxVersion
        {
            get { return GetValue<string>("OfxVersion"); }
            set { SetValue("OfxVersion", value); }
        }
        public string Org
        {
            get { return GetValue<string>("Org"); }
            set { SetValue("Org", value); }
        }

        public string Fid
        {
            get { return GetValue<string>("Fid"); }
            set { SetValue("Fid", value); }
        }
        public string BankId
        {
            get { return GetValue<string>("BankId"); }
            set { SetValue("BankId", value); }
        }

        public string BrokerId
        {
            get { return GetValue<string>("BrokerId"); }
            set { SetValue("BrokerId", value); }
        }
        public string ProviderURL
        {
            get { return GetValue<string>("ProviderURL"); }
            set { SetValue("ProviderURL", value); }
        } 
        public string SmallLogoURL
        {
            get { return GetValue<string>("SmallLogoURL"); }
            set { SetValue("SmallLogoURL", value); }
        }
        public string Website
        {
            get { return GetValue<string>("Website"); }
            set { SetValue("Website", value); }
        }
        public string OfxHomeId
        {
            get { return GetValue<string>("OfxHomeId"); }
            set { SetValue("OfxHomeId", value); }
        }
        public string MoneyDanceId
        {
            get { return GetValue<string>("MoneyDanceId"); }
            set { SetValue("MoneyDanceId", value); }
        }
        public string AppId
        {
            get { return GetValue<string>("AppId"); }
            set { SetValue("AppId", value); }
        }
        public string AppVer
        {
            get { return GetValue<string>("AppVer"); }
            set { SetValue("AppVer", value); }
        }
        public string LastError
        {
            get { return GetValue<string>("LastError"); }
            set { SetValue("LastError", value); }
        }

        public DateTime? LastConnection
        {
            get { return GetValue<DateTime?>("LastConnection"); }
            set { SetValue("LastConnection", value); }
        }

        public bool Existing
        {
            get { return GetValue<bool>("Existing"); }
            set { SetValue("Existing", value); }
        }

        public bool HasError
        {
            get
            {
                return !string.IsNullOrEmpty(LastError);
            }
        }

        // this field is not persisted or merged, it is a transient value used in memory only.
        public OnlineAccount OnlineAccount { get; set; }


        // we keep track of each field change
        private Dictionary<string, ChangeTrackedField> fields = new Dictionary<string, ChangeTrackedField>();

        private T GetValue<T>(string name)
        {
            ChangeTrackedField field = null;
            fields.TryGetValue(name, out field);
            if (field == null || field.Value == null)
            {
                return default(T);
            }
            return (T)field.Value;
        }

        private void SetValue(string name, object value)
        {
            SetValue(name, value, DateTime.Now);
        }

        private void SetValue(string name, object value, DateTime? changed)
        {
            ChangeTrackedField field = null;
            fields.TryGetValue(name, out field);
            if (field == null)
            {
                field = new ChangeTrackedField() { Name = name };
                fields[name] = field;
            }
            if (field.Value != value)
            {
                field.Value = value;
                field.LastChange = changed;
            }
        }
        
        public override string ToString()
        {
            return this.Name ?? "";
        }

        internal static OfxInstitutionInfo Create(XElement e)
        {
            OfxInstitutionInfo result = new OfxInstitutionInfo();

            foreach (XElement child in e.Elements())
            {
                DateTime? lastChanged = null;
                string date = (string)child.Attribute("changed");
                if (!string.IsNullOrEmpty(date))
                {
                    DateTime dt;
                    if (DateTime.TryParse(date, out dt))
                    {
                        lastChanged = dt;
                    }
                }
                result.SetValue(child.Name.LocalName, child.Value, lastChanged);
            }
            return result;
        }

        private static DateTime? GetElementDateTime(XElement e, string name)
        {
            string s = GetElementString(e, name);
            if (!string.IsNullOrEmpty(s))
            {
                DateTime dt;
                if (DateTime.TryParse(s, out dt))
                {
                    return dt;
                }
            }
            return null;
        }

        public bool AddInfoFromOfxHome(XElement ofxHomeInfo)
        {
            /*
              <institution id="542">
                    <name>1st Advantage FCU</name>
                    <fid>251480563</fid>
                    <org>1st Advantage FCU</org>
                    <brokerid></brokerid>
                    <url>https://members.1stadvantage.org/scripts/isaofx.dll</url>
                    <ofxfail>0</ofxfail>
                    <sslfail>0</sslfail>
                    <lastofxvalidation>2011-10-21 22:00:07</lastofxvalidation>
                    <lastsslvalidation>2011-10-21 22:00:05</lastsslvalidation>
              </institution>
             */

            bool changed = false;

            DateTime? lastvalidation = null;

            string lastval = GetElementString(ofxHomeInfo, "lastofxvalidation");
            if (!string.IsNullOrEmpty(lastval))
            {
                DateTime dt;
                if (DateTime.TryParse(lastval, out dt))
                {
                    lastvalidation = dt;
                }                    
            }

            // the OfxHomeId field must already be set otherwise we wouldn't have been able
            // to get this update from OfxHome.com.

            changed |= SetIfNewer("Name", GetElementString(ofxHomeInfo, "name"), lastvalidation);
            changed |= SetIfNewer("Org", GetElementString(ofxHomeInfo, "org"), lastvalidation);

            if (string.IsNullOrEmpty(this.Org))
            {
                this.Org = GetElementString(ofxHomeInfo, "org");
                changed = true;
            }
            if (string.IsNullOrEmpty(this.Fid))
            {
                this.Fid = GetElementString(ofxHomeInfo, "fid");
                changed = true;
            }
            if (string.IsNullOrEmpty(this.BrokerId))
            {
                this.BrokerId = GetElementString(ofxHomeInfo, "brokerid");
                changed = true;
            }
            if (string.IsNullOrEmpty(this.ProviderURL))
            {
                this.ProviderURL = GetElementString(ofxHomeInfo, "url");
                changed = true;
            }

            return changed;
        }

        private bool SetIfNewer(string name, string value, DateTime? lastvalidation)
        {
            bool setit = false;
            ChangeTrackedField field;
            if (fields.TryGetValue(name, out field))
            {
                // see if ofxhome is more recent
                setit = (lastvalidation.HasValue && (!field.LastChange.HasValue || field.LastChange.Value < lastvalidation.Value));
            }
            else 
            {
                setit = true;
            }
            if (setit) 
            {
                SetValue(name, value, lastvalidation);
            }
            return setit;
        }

        private static string GetElementString(XElement parent, string name)
        {
            XElement e = parent.Element(name);
            if (e != null)
            {
                return e.Value;
            }
            return null;
        }


        internal XElement ToXml()
        {
            XElement e = new XElement("institution");

            foreach (var field in this.fields)
            {
                ChangeTrackedField ctf = field.Value;
                XElement fx = new XElement(ctf.Name, ctf.Value);
                if (ctf.LastChange.HasValue)
                {
                    // remember when it was last changed.
                    fx.SetAttributeValue("changed", ctf.LastChange.Value);
                }
                e.Add(fx);
            }
            return e;
        }

        internal bool Merge(OfxInstitutionInfo profile)
        {
            Debug.Assert((string.IsNullOrEmpty(this.MoneyDanceId) || string.IsNullOrEmpty(profile.MoneyDanceId) || string.Compare(this.MoneyDanceId, profile.MoneyDanceId) == 0)
                && (string.IsNullOrEmpty(this.OfxHomeId) || string.IsNullOrEmpty(profile.OfxHomeId) || string.Compare(this.OfxHomeId, profile.OfxHomeId) == 0));

            bool changed = false;

            foreach (ChangeTrackedField field in profile.fields.Values)
            {
                bool setit = false;
                ChangeTrackedField ourField;
                if (fields.TryGetValue(field.Name, out ourField))
                {
                    if (ourField.Value != field.Value)
                    {
                        // see which one is newer.
                        setit = field.LastChange.HasValue && (!ourField.LastChange.HasValue || ourField.LastChange.Value < field.LastChange.Value);
                    }
                    else
                    {
                        // values are equal, no need for a change then.
                    }
                }
                else 
                {
                    // new to us
                    setit = true;
                }
                if (setit)
                {
                    SetValue(field.Name, field.Value, field.LastChange);
                    changed = true;
                }
            }

            return changed;
        }


        /// <summary>
        /// This code can parse the Python script for MoneyDance in order to glean OfxInstitutionInfo.
        /// You can then merge that with the existing ofx-index data from ofxhome.com
        /// </summary>
        /// <param name="filename">The text file containing MoneyDance python code</param>
        /// <returns>The list of OfxInstitutionInfo found in the MoneyDance file</returns>
        public static List<OfxInstitutionInfo> ParseMoneyDancePythonScript(string filename)
        {
            List<OfxInstitutionInfo> result = new List<OfxInstitutionInfo>();
            
            string text = null;

            using (StreamReader reader = new StreamReader(filename))
            {
                text = reader.ReadToEnd();
            }

            for (int i = 0, n = text.Length; i < n; i++)
            {
                char ch = text[i];
                if (ch == '{')
                {
                    OfxInstitutionInfo info = new OfxInstitutionInfo();
                    result.Add(info);

                    string name = null;
                    string value = null;

                    for (i = i + 1; i < n; i++)
                    {
                        ch = text[i];
                        if (ch == '}')
                        {
                            // done
                            value = null;
                            break;
                        }
                        else if (ch == '"')
                        {
                            i++;
                            for (int j = i; i < n; j++)
                            {
                                ch = text[j];
                                if (ch == '"')
                                {
                                    value = text.Substring(i, j - i);
                                    i = j;
                                    break;
                                }
                            }

                            if (name != null)
                            {
                                // map to ofx-index format.
                                switch (name)
                                {
                                    case "fi_name":
                                        info.Name = value;
                                        break;
                                    case "fi_org":
                                        info.Org = value;
                                        break;
                                    case "id":
                                        info.MoneyDanceId = value;
                                        break;
                                    case "bootstrap_url":
                                        info.ProviderURL = value;
                                        break;
                                    case "fi_id":
                                        info.Fid = value;
                                        break;
                                    case "app_id":
                                        info.AppId = value;
                                        break;
                                    case "app_ver":
                                        info.AppVer = value;
                                        break;
                                    case "broker_id":
                                        info.BrokerId = value;
                                        break;
                                }

                                name = null;
                                value = null;
                            }
                        }
                        else if (ch == '=')
                        {
                            name = value;
                            value = null;
                        }
                    }
                }
            }

            return result;
        }

    }
}