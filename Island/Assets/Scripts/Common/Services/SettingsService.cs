using System;
using Island.Gameplay.Settings;
using JetBrains.Annotations;
using TendedTarsier.Core.Services;
using UniRx;
using UnityEngine;
using Zenject;

namespace Island.Common.Services
{
    [UsedImplicitly]
    public class SettingsService : ServiceBase
    {
        private CameraConfig _cameraConfig;

        [Inject]
        private void Construct(CameraConfig cameraConfig)
        {
            _cameraConfig = cameraConfig;
            
            Fov = new ReactivePrefs<int>("Fov", _cameraConfig.DefaultFov);
            CameraSensitivity = new ReactivePrefs<int>("CameraSensitivity", _cameraConfig.DefaultCameraSensitivity);
        }

        public ReactivePrefs<int> Fov;
        public ReactivePrefs<int> CameraSensitivity;
    }

    public class ReactivePrefs<T> : ReactiveProperty<T>
    {
        private readonly string _key;

        public ReactivePrefs(string key, T defaultValue)
        {
            _key = key;
            if (typeof(T) == typeof(int) || typeof(T) == typeof(bool))
            {
                var value = PlayerPrefs.GetInt(_key, Convert.ToInt32(defaultValue));
                Value = (T)Convert.ChangeType(value, typeof(T));
            }
            else if (typeof(T) == typeof(float))
            {
                var value = PlayerPrefs.GetFloat(_key, Convert.ToSingle(defaultValue));
                Value = (T)Convert.ChangeType(value, typeof(T));
            }
            else if (typeof(T) == typeof(string))
            {
                var value = PlayerPrefs.GetString(_key, Convert.ToString(defaultValue));
                Value = (T)Convert.ChangeType(value, typeof(T));
            }
            
        }
        
        protected override void SetValue(T value)
        {
            base.SetValue(value);
            if (typeof(T) == typeof(int) || typeof(T) == typeof(bool))
            {
                PlayerPrefs.SetInt(_key, Convert.ToInt32(value));
            }
            else if (typeof(T) == typeof(float))
            {
                PlayerPrefs.SetFloat(_key, Convert.ToSingle(value));
            }
            else if (typeof(T) == typeof(string))
            {
                PlayerPrefs.SetString(_key, Convert.ToString(value));
            }
        }
    }
}