using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

namespace Sound
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance;

        private readonly List<EventInstance> _ambiences = new();

        private EventInstance? _currentMusic;

        private void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }


        public void SwitchMusic(string eventName)
        {
            var musicEvent = RuntimeManager.CreateInstance(eventName);

            if (_currentMusic.HasValue) _currentMusic.Value.stop(STOP_MODE.ALLOWFADEOUT);

            musicEvent.start();

            _currentMusic = musicEvent;
        }

        public void AddAmbience(string eventName)
        {
            var ambienceEvent = RuntimeManager.CreateInstance(eventName);

            ambienceEvent.start();

            _ambiences.Add(ambienceEvent);
        }

        public void StopAllAmbiences()
        {
            foreach (var ambience in _ambiences) ambience.stop(STOP_MODE.ALLOWFADEOUT);

            _ambiences.Clear();
        }
    }
}