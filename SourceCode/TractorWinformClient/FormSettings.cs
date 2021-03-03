﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;

namespace Duan.Xiugang.Tractor
{
    public delegate void SettingsUpdatedEventHandler();

    public partial class FormSettings : Form
    {
        public event SettingsUpdatedEventHandler SettingsUpdatedEvent;
        public static string regexHostAndPort = @"net.tcp://(?<host>.*):(?<port>\d*)/";
        AppSettingsReader myreader;

        private string NodePathEndpoint = "//configuration//system.serviceModel//client//endpoint";
        public string KeyNickName = "nickName";
        public string KeyUpdateOnLoad = "updateOnLoad";

        public FormSettings()
        {
            InitializeComponent();
            myreader = new AppSettingsReader();
            string hostName = GetHostAndPortFromConfig();

            this.tbxHostName.Text = hostName;
            this.tbxNickName.Text = GetSettingString(KeyNickName);
            this.cbxUpdateOnLoad.Checked = GetSettingBool(KeyUpdateOnLoad);
        }

        private string GetHostAndPortFromConfig()
        {
            string curHostNameRaw = GetHostName();

            Regex r = new Regex(regexHostAndPort);
            Match m = r.Match(curHostNameRaw);
            if (!m.Success) throw new FormatException();
            return m.Result("${host}:${port}");
        }

        public string GetHostName()
        {
            return loadConfigDocument().SelectSingleNode(NodePathEndpoint).Attributes["address"].Value;
        }

        public void SaveHostName(string value)
        {
            // load config document for current assembly
            XmlDocument doc = loadConfigDocument();

            // retrieve appSettings node
            XmlNode node = doc.SelectSingleNode(NodePathEndpoint);

            if (node == null)
                throw new InvalidOperationException("Error. Could not find endpoint node in config file.");

            try
            {
                // select the 'add' element that contains the key
                //XmlElement elem = (XmlElement)node.SelectSingleNode(string.Format("//add[@key='{0}']", key));
                node.Attributes["address"].Value = value;

                doc.Save(getConfigFilePath());
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public XmlDocument loadConfigDocument()
        {
            XmlDocument doc = null;
            try
            {
                doc = new XmlDocument();
                doc.Load(getConfigFilePath());
                return doc;
            }
            catch (System.IO.FileNotFoundException e)
            {
                throw new Exception("No configuration file found.", e);
            }
        }

        private string getConfigFilePath()
        {
            return Assembly.GetExecutingAssembly().Location + ".config";
        }

        public string GetSettingString(string key)
        {
            try
            {
                return (String)myreader.GetValue(key, typeof(String));
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public bool GetSettingBool(string key)
        {
            try
            {
                return (bool)myreader.GetValue(key, typeof(Boolean));
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private void SetSetting(string key, string value)
        {
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            configuration.AppSettings.Settings[key].Value = value;
            configuration.Save(ConfigurationSaveMode.Minimal, true);
            ConfigurationManager.RefreshSection("appSettings");
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            string oldHostName = GetHostAndPortFromConfig();
            SaveHostName(string.Format("net.tcp://{0}/TractorHost", this.tbxHostName.Text));
            SetSetting(KeyNickName, tbxNickName.Text);
            SetSetting(KeyUpdateOnLoad, cbxUpdateOnLoad.Checked.ToString().ToLower());
            SettingsUpdatedEvent();
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
