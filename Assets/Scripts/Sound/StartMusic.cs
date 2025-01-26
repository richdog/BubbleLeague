using UnityEngine;

namespace Sound
{
    public class StartMusic : MonoBehaviour
    {
        public string eventName;


        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            SoundManager.Instance.SwitchMusic(eventName);
        }

        // Update is called once per frame
        private void Update()
        {
        }
    }
}