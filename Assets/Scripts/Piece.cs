using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace Reversi
{
    public class Piece : MonoBehaviour
    {
        [SerializeField] private bool _isBlack = true;
        public bool IsBlack => _isBlack;
        public void SetFront(bool isBlack)
        {
            _isBlack = isBlack;
            if (isBlack)
            {
                transform.eulerAngles = Vector3.zero;
            }
            else
                transform.eulerAngles = new Vector3(180f, 0, 0);
        }
        public async UniTask Flip(bool isBlack, float duration, CancellationToken token)
        {
            _isBlack = isBlack;
            var eulerAngles = new Vector3(180f, 0, 0);
            if (isBlack) eulerAngles = Vector3.zero;
            AudioManager.Instance.PlaySE("FlipPiece");
            await transform.DORotate(eulerAngles, duration)
                .ToUniTask(cancellationToken: token);
        }
    }
}