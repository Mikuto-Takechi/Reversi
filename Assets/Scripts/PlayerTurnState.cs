using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

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
            var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            //  プレイヤーが配置するのを待つ
            var result =  await UniTask.WhenAny(_gameManager.WaitPlaceAsync(cts.Token, token), TimeLimitCounter(cts.Token));
            cts.Cancel();
            if (result.hasResultLeft)
            {
                await result.result;
            }
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

            List<(CellIndex index, int canFlipCount)> canPlaces = new();
            int canPlaceCount = _gameManager.GetCanPlacePositions(true, canPlaces);
            if (canPlaceCount > 0)
            {
                var random = Random.Range(0, canPlaceCount);
                await _gameManager.PlacePieceAsync(canPlaces[random].index, true, token);
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