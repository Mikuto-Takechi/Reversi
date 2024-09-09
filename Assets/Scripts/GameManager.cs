using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Reversi
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private Piece _piecePrefab;
        [SerializeField] private Transform _pieceRoot;
        [SerializeField] private float _pieceDistance = 1.25f;
        [SerializeField] private Text _turnView;
        [SerializeField, Tooltip("プレイヤーの待ち時間")] private float _timeLimit = 10f;
        
        private const int BoardRow = 8;
        private const int BoardCol = 8;
        private Piece[,] _board = new Piece[BoardRow, BoardCol];
        private CancellationTokenSource _cts;
        private List<IState> _turnList = new();
        private IDisposable _playerInput;
        public readonly List<string> GameRecord = new();
        void Awake()
        {
            CreatePiece(new CellIndex(3, 3), true);
            CreatePiece(new CellIndex(4, 4), true);
            CreatePiece(new CellIndex(3, 4), false);
            CreatePiece(new CellIndex(4, 3), false);
            
            _turnList.Add(new PlayerTurnState(this, _turnView, _timeLimit));
            _turnList.Add(new EnemyTurnState(this, _turnView));
            _turnList.Add(new JudgeEndState(this));
            TurnLoop().Forget();
        }

        async UniTaskVoid TurnLoop()
        {
            _cts = CancellationTokenSource.
                CreateLinkedTokenSource(gameObject.GetCancellationTokenOnDestroy());
            while (true)
            {
                foreach (var turn in _turnList)
                {
                    await turn.Process(_cts.Token);
                }

                if (_turnList.Count <= 0) break;
                await UniTask.Yield(_cts.Token);
            }
        }

        public void GameEnd()
        {
            _cts.Cancel();
            int blackPiece = 0, whitePiece = 0;
            for (int row = 0; row < BoardRow; row++)
            {
                for (int col = 0; col < BoardCol; col++)
                {
                    if (!_board[row, col]) continue;

                    if (_board[row, col].IsBlack)
                        blackPiece++;
                    else
                        whitePiece++;
                }
            }
            
            _turnView.color = Color.white;
            if (blackPiece > whitePiece)
                _turnView.text = "ゲーム終了: プレイヤーの勝利";
            else if(blackPiece == whitePiece)
                _turnView.text = "ゲーム終了: 引き分け";
            else
                _turnView.text = "ゲーム終了: 敵の勝利";
        }
        public async UniTask<UniTask> WaitPlaceAsync(CancellationToken token, CancellationToken defaultToken)
        {
            while (true)
            {
                CellClickRequester.CompletionSource = new();
                var index = await CellClickRequester.CompletionSource.Task.AttachExternalCancellation(token);
                if (CanPlacePosition(index, true, null))
                {
                    return PlacePiece(index, true, defaultToken);
                }
                await UniTask.Yield(token);
            }
        }
        
        public int GetCanPlacePositions(bool isBlack, List<(CellIndex index, int canFlipCount)> canPlaces)
        {
            int positionCount = 0;
            for (int row = 0; row < BoardRow; row++)
            {
                for (int col = 0; col < BoardCol; col++)
                {
                    var index = new CellIndex(row, col);
                    List<CellIndex> allFlipPieces = new();
                    if (CanPlacePosition(index, isBlack, allFlipPieces))
                    {
                        canPlaces?.Add((index, allFlipPieces.Count));
                        positionCount++;
                    }
                }
            }

            return positionCount;
        }
        /// <summary>
        /// 駒を配置する。
        /// </summary>
        public async UniTask PlacePiece(CellIndex cellIndex, bool isBlack, CancellationToken ct)
        {
            List<CellIndex> allFlipPieces = new();
            if (!CanPlacePosition(cellIndex, isBlack, allFlipPieces)) return;
            CreatePiece(cellIndex, isBlack);
            AudioManager.Instance.PlaySE("PlacePiece");
            
            foreach (var index in allFlipPieces)
            {
                await _board[index.Row, index.Col].Flip(isBlack, 0.25f, ct);
            }
            GameRecord.Add(cellIndex.ToString());
        }
        /// <summary>
        /// 駒を生成する。
        /// </summary>
        void CreatePiece(CellIndex cellIndex, bool isBlack)
        {
            if (!CheckWithinRange(cellIndex) || _board[cellIndex.Row, cellIndex.Col]) return;
            _board[cellIndex.Row, cellIndex.Col] = Instantiate(_piecePrefab, _pieceRoot);
            _board[cellIndex.Row, cellIndex.Col].transform.localPosition =
                new Vector3(cellIndex.Row * _pieceDistance, 0,  cellIndex.Col * _pieceDistance);
            _board[cellIndex.Row, cellIndex.Col].SetFront(isBlack);
        }
        /// <summary>
        /// 指定した場所から8方向を調べてひっくりかえせる場所があるならtrueを返す。
        /// </summary>
        public bool CanPlacePosition(CellIndex cellIndex, bool isBlack, List<CellIndex> allFlipPieces)
        {
            if (!CheckWithinRange(cellIndex) || _board[cellIndex.Row, cellIndex.Col]) return false;
            bool canPlace = false;
            
            foreach (var dir in CellIndex.Directions)
            {
                List<CellIndex> flipPieces = new();
                if (CanPlaceDirection(cellIndex, dir.row, dir.col, isBlack, flipPieces))
                {
                    canPlace = true;
                    allFlipPieces?.AddRange(flipPieces);
                }
            }
            return canPlace;
        }

        bool CanPlaceDirection(CellIndex cellIndex, int rowDir, int colDir, bool isBlack, List<CellIndex> flipPieces)
        {
            cellIndex.Row += rowDir;
            cellIndex.Col += colDir;
            //  1つ隣のマスが (配列の範囲外 || 何もない || 駒の色が同じ)なら置けない
            if (!CheckWithinRange(cellIndex) 
                || !_board[cellIndex.Row, cellIndex.Col] 
                || _board[cellIndex.Row, cellIndex.Col].IsBlack == isBlack) 
                return false;

            bool hasEndPoint = false;
            flipPieces.Add(cellIndex);
            while (true)
            {
                cellIndex.Row += rowDir;
                cellIndex.Col += colDir;
                if (!CheckWithinRange(cellIndex)
                    || !_board[cellIndex.Row, cellIndex.Col])
                {
                    break;
                }
                if (_board[cellIndex.Row, cellIndex.Col].IsBlack == isBlack)
                {
                    hasEndPoint = true;
                    break;
                }
                flipPieces.Add(cellIndex);
            }

            return hasEndPoint;
        }

        /// <summary>
        /// 指定したインデックスが配列の範囲内かどうかをチェックする。
        /// </summary>
        bool CheckWithinRange(CellIndex cellIndex)
        {
            return cellIndex.Row is < BoardRow and >= 0 && cellIndex.Col is < BoardCol and >= 0;
        }
    }
}
