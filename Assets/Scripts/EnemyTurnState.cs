using System.Collections.Generic;
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
            
            List<(CellIndex index, int canFlipCount)> canPlaces = new();
            int canPlaceCount = _gameManager.GetCanPlacePositions(false, canPlaces);
            if (canPlaceCount > 0)
            {
                (CellIndex Index, int canFlipCount) maxFlipData = (default, 0);
                foreach (var flipData in canPlaces)
                {
                    if (maxFlipData.canFlipCount < flipData.canFlipCount)
                        maxFlipData = flipData;
                }
                await _gameManager.PlacePiece(maxFlipData.Index, false, token);
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