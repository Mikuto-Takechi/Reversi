using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UniRx;
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
        private const int BoardWidth = 8;
        private const int BoardHeight = 8;
        private Piece[,] _board = new Piece[BoardHeight, BoardWidth];
        private readonly (int row, int col)[] Directions = 
        {
            (0, 1), (0, -1), (1, 0), (-1, 0),
            (1, 1), (-1, -1), (1, -1), (-1, 1)
        };

        private CancellationTokenSource _cts;
        private List<IState> _turnList = new();
        private IDisposable _playerInput;
        public readonly Subject<Unit> OnPlaced = new();
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
            for (int row = 0; row < BoardHeight; row++)
            {
                for (int col = 0; col < BoardWidth; col++)
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

        public void AllowPlayerInput(bool allow)
        {
            _playerInput?.Dispose();
            if (allow)
            {
                _playerInput = CellClickRequester.OnClicked
                    .Subscribe(index => PlacePiece(index, true)).AddTo(this);
            }
        }

        public int GetCanPlacePositions(bool isBlack, out List<CellIndex> canPlaces)
        {
            int positionCount = 0;
            canPlaces = new();
            for (int row = 0; row < BoardHeight; row++)
            {
                for (int col = 0; col < BoardWidth; col++)
                {
                    var index = new CellIndex(row, col);
                    if (CanPlacePosition(index, isBlack, out _))
                    {
                        canPlaces.Add(index);
                        positionCount++;
                    }
                }
            }

            return positionCount;
        }
        public void PlacePiece(CellIndex cellIndex, bool isBlack)
        {
            if (!CanPlacePosition(cellIndex, isBlack, out var allFlipPieces)) return;
            CreatePiece(cellIndex, isBlack);
            foreach (var index in allFlipPieces)
            {
                _board[index.Row, index.Col].IsBlack = !_board[index.Row, index.Col].IsBlack;
            }
            OnPlaced.OnNext(Unit.Default);
        }

        void CreatePiece(CellIndex cellIndex, bool isBlack)
        {
            if (!CheckInRange(cellIndex) || _board[cellIndex.Row, cellIndex.Col]) return;
            _board[cellIndex.Row, cellIndex.Col] = Instantiate(_piecePrefab, _pieceRoot);
            _board[cellIndex.Row, cellIndex.Col].transform.localPosition =
                new Vector3(cellIndex.Row * _pieceDistance, 0,  cellIndex.Col * _pieceDistance);
            _board[cellIndex.Row, cellIndex.Col].IsBlack = isBlack;
        }
        /// <summary>
        /// 指定した場所から8方向を調べてひっくりかえせる場所があるならtrueを返す。
        /// </summary>
        bool CanPlacePosition(CellIndex cellIndex, bool isBlack, out List<CellIndex> allFlipPieces)
        {
            allFlipPieces = new();
            if (!CheckInRange(cellIndex) || _board[cellIndex.Row, cellIndex.Col]) return false;
            bool canPlace = false;
            
            foreach (var dir in Directions)
            {
                if (CanPlaceDirection(cellIndex, dir.row, dir.col, isBlack, out var flipPieces))
                {
                    canPlace = true;
                    allFlipPieces.AddRange(flipPieces);
                }
            }
            return canPlace;
        }

        bool CanPlaceDirection(CellIndex cellIndex, int rowDir, int colDir, bool isBlack, out List<CellIndex> flipPieces)
        {
            flipPieces = new();
            cellIndex.Row += rowDir;
            cellIndex.Col += colDir;
            //  1つ隣のマスが (配列の範囲外 || 何もない || 駒の色が同じ)なら置けない
            if (!CheckInRange(cellIndex) 
                || !_board[cellIndex.Row, cellIndex.Col] 
                || _board[cellIndex.Row, cellIndex.Col].IsBlack == isBlack) 
                return false;

            bool hasEndPoint = false;
            flipPieces.Add(cellIndex);
            while (true)
            {
                cellIndex.Row += rowDir;
                cellIndex.Col += colDir;
                if (!CheckInRange(cellIndex)
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

        bool CheckInRange(CellIndex cellIndex)
        {
            return cellIndex.Row is < BoardHeight and >= 0 && cellIndex.Col is < BoardWidth and >= 0;
        }
    }
}
