using DG.Tweening;
using UnityEngine;

namespace Reversi
{
    public class Piece : MonoBehaviour
    {
        [SerializeField] private bool _isBlack = true;
        private bool _instantiated;
        void Flip(bool isBlack)
        {
            if (isBlack)
                transform.eulerAngles = Vector3.zero;
            else
                transform.eulerAngles = new Vector3(180f, 0, 0);
        }
        void Flip(bool isBlack, float duration)
        {
            if (isBlack)
            {
                transform.DORotate(Vector3.zero, duration);
            }
            else
            {
                transform.DORotate(new Vector3(180f, 0, 0), duration);
            }
        }
        public bool IsBlack
        {
            get => _isBlack;
            set
            {
                if (_instantiated)
                    Flip(value, 0.5f);
                else
                    Flip(value);
                
                _isBlack = value;
            }
        }

        private void Start()
        {
            _instantiated = true;
        }
    }
}