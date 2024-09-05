using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Reversi
{
    public class EnemyTurnState : IState
    {
        private GameManager _gameManager;
        private Text _turnText;
        
        public async UniTask Process(CancellationToken token)
        {
            _turnText.text = "敵のターン";
            _turnText.color = Color.red;
            int canPlaceCount = _gameManager.GetCanPlacePositions(false, out var canPlaces);
            if (canPlaceCount > 0)
            {
                var random = Random.Range(0, canPlaceCount);
                _gameManager.PlacePiece(canPlaces[random], false);
            }
            await UniTask.WaitForSeconds(1f, cancellationToken: token);
        }

        public EnemyTurnState(GameManager gm, Text turnText)
        {
            _gameManager = gm;
            _turnText = turnText;
        }
    }
}