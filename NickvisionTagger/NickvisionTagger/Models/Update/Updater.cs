using System;
using System.Threading.Tasks;

namespace NickvisionTagger.Models.Update
{
    public class Updater
    {
        private string _linkToConfig;
        private Version _currentApplicationVersion;
        private UpdateConfig _updateConfig;
        private bool _updateAvaliable;

        public bool UpdateAvaliable => _updateAvaliable && _updateConfig != null;
        public Version LatestVersion => _updateConfig == null ? null : new Version(_updateConfig.LatestVersion);
        public string Changelog => _updateConfig.Changelog ?? null;

        public Updater(string linkToConfig, Version currentApplicationVersion)
        {
            _linkToConfig = linkToConfig;
            _currentApplicationVersion = currentApplicationVersion;
            _updateConfig = null;
            _updateAvaliable = false;
        }

        public async Task<bool> CheckForUpdatesAsync()
        {
            _updateConfig = await UpdateConfig.LoadFromWebAsync(_linkToConfig);
            if (_updateConfig != null && LatestVersion > _currentApplicationVersion)
            {
                _updateAvaliable = true;
            }
            return UpdateAvaliable;
        }
    }
}
