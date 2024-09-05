using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Reversi
{
    public class PlayerTurnState : IState
    {
        private GameManager _gameManager;
        private Text _turnText;
        private float _timeLimit;
        
        public async UniTask Process(CancellationToken token)
        {
            _turnText.text = "プレイヤーのターン";
            _turnText.color = Color.yellow;
            _gameManager.AllowPlayerInput(true);
            var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            //  プレイヤーが配置するのを待つ
            await UniTask.WhenAny(_gameManager.OnPlaced.First().ToUniTask(cancellationToken:cts.Token), 
                TimeLimitCounter(cts.Token));
            cts.Cancel();
            _gameManager.AllowPlayerInput(false);
            await UniTask.WaitForSeconds(1f, cancellationToken: token);
        }

        async UniTask TimeLimitCounter(CancellationToken token)
        {
            float timer = _timeLimit;
            while (true)
            {
                await UniTask.Yield(token);
                timer -= Time.deltaTime;
                if (timer <= 0) break;
                _turnText.text = $"プレイヤーのターン:{timer:0.00}";
            }
            int canPlaceCount = _gameManager.GetCanPlacePositions(true, out var canPlaces);
            if (canPlaceCount > 0)
            {
                var random = Random.Range(0, canPlaceCount);
                _gameManager.PlacePiece(canPlaces[random], true);
            }
        }

        public PlayerTurnState(GameManager gm, Text turnText, float timeLimit)
        {
            _gameManager = gm;
            _turnText = turnText;
            _timeLimit = timeLimit;
        }
    }
}