using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Reversi
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance;
        [SerializeField] private List<AudioData> _audioDataList;
        private AudioSource _seSource;

        private void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            _seSource = gameObject.AddComponent<AudioSource>();
            _seSource.loop = false;
            _seSource.playOnAwake = false;
        }

        public void PlaySE(string name)
        {
            var data = _audioDataList.FirstOrDefault(d => d.Name == name);
            if (data == null) return;
            _seSource.PlayOneShot(data.Clip);
        }
    }
}
