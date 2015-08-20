using System;
using System.Configuration;
using System.Globalization;
using ReMi.BusinessLogic.BusinessRules;
using ReMi.Common.Constants.BusinessRules;
using ReMi.Common.Utils;

namespace ReMi.Api.Insfrastructure
{
    public class ApplicationSettings : IApplicationSettings
    {
        private const int SettingsSessionDuration = 15;
        private static DateTime _lastSettingsRefresh = DateTime.MinValue;
        private static object _sync = new object();

        private int _sessionDuration = -1;

        public IBusinessRuleEngine BusinessRuleEngine { get; set; }

        public int DefaultReleaseWindowDurationTime
        {
            get
            {
                var config = ConfigurationManager.AppSettings["DefaultReleaseWindowDurationTime"];

                return GetInt(config, 120);
            }
        }

        public bool LogJsonFormatted
        {
            get { return GetBool(ConfigurationManager.AppSettings["LogJsonFormatted"], true); }
        }
        public bool LogQueryResponses
        {
            get { return GetBool(ConfigurationManager.AppSettings["LogQueryResponses"]); }
        }

        public string FrontEndUrl
        {
            get { return ConfigurationManager.AppSettings["frontendUrl"]; }
        }

        public int SessionDuration
        {
            get
            {
                RefreshSettings();
                return _sessionDuration;
            }
        }

        #region Helpers

        private static int GetInt(string config, int defaultValue = 0)
        {
            if (!string.IsNullOrWhiteSpace(config))
            {
                int value;
                if (int.TryParse(config, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                    return value;
            }

            return defaultValue;
        }

        private static bool GetBool(string config, bool defaultValue = false)
        {
            if (!string.IsNullOrWhiteSpace(config))
            {
                bool value;
                if (bool.TryParse(config, out value))
                    return value;
            }

            return defaultValue;
        }

        private void RefreshSettings()
        {
            lock (_sync)
            {
                if (!((SystemTime.Now - _lastSettingsRefresh).TotalMinutes > SettingsSessionDuration))
                    return;

                _lastSettingsRefresh = SystemTime.Now;
                _sessionDuration = BusinessRuleEngine.Execute<int>(Guid.Empty,
                    BusinessRuleConstants.Config.SessionDurationRule.ExternalId,
                    null);
            }
        }

        #endregion
    }
}
