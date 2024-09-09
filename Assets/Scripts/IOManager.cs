using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Reversi
{
    /// <summary>
    /// 棋譜のデータを入出力するクラス
    /// </summary>
    public class IOManager : MonoBehaviour
    {
        [SerializeField] private Button _exportButton;
        [SerializeField] private Button _importButton;
        [SerializeField] private InputField _fileNameInputField;
        private const int UpperCaseMin = 'A', UpperCaseMax = 'H';
        private const int LowerCaseMin = 'a', LowerCaseMax = 'h';
        private const int NumberLetterMin = '1', NumberLetterMax = '8';
        private GameManager _gameManager;
        private void Awake()
        {
            _gameManager = FindObjectOfType<GameManager>();
            _exportButton?.onClick.AddListener(Export);
            _importButton?.onClick.AddListener(Import);
        }

        void Import()
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var removedString = string.Concat(_fileNameInputField.text.Where(c => !invalidChars.Contains(c)));
            _fileNameInputField.text = removedString;
            if (removedString == String.Empty) return;
            var query = ImportGameRecord(removedString);
            _gameManager.BoardInitialize(query);
        }
        void Export()
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var removedString = string.Concat(_fileNameInputField.text.Where(c => !invalidChars.Contains(c)));
            _fileNameInputField.text = removedString;
            if (removedString == String.Empty) return;
            ExportGameRecord(removedString);
        }
        /// <summary>
        /// 与えられた文字が定数の範囲内かをチェックする。
        /// </summary>
        bool CheckWithinRange(char letter1, char letter2)
        {
            var result1 = (int)letter1 is >= LowerCaseMin and <= LowerCaseMax or >= UpperCaseMin and <= UpperCaseMax;
            var result2 = (int)letter2 is >= NumberLetterMin and <= NumberLetterMax;
            return result1 && result2;
        }
        /// <summary>
        /// 棋譜のデータを読み取って命令に変換する
        /// </summary>
        /// <param name="fileName">ファイル名（識別子は要らない）</param>
        public Queue<CellIndex> ImportGameRecord(string fileName)
        {
            var filePath = Application.streamingAssetsPath + "/" + fileName + ".txt";
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"{filePath}というファイルが見つかりませんでした。");
                return null;
            }

            Queue<CellIndex> queue;
            using (var dataReader = new StreamReader(filePath))
            {
                var boardData = dataReader.ReadToEnd();
                boardData = boardData.Replace("\r", "").Replace("\n", "");
                boardData = Regex.Replace(boardData, @"\s", "");
                //  文字数が偶数でなければ間違ったデータなのでnullを返す。
                if (boardData.Length % 2 != 0)
                {
                    Debug.LogWarning($"{filePath}内の値が正しくないため読み込みを中止します。");
                    return null;
                }
                queue = new();
                for (int i = 0; i < boardData.Length; i += 2)
                {
                    if (!CheckWithinRange(boardData[i], boardData[i + 1]))
                    {
                        Debug.LogWarning($"{filePath}内の値が正しくないため読み込みを中止します。");
                        return null;
                    }

                    if (Char.IsUpper(boardData[i]))
                        queue.Enqueue(new CellIndex(boardData[i] - UpperCaseMin, NumberLetterMax - boardData[i + 1]));
                    else
                        queue.Enqueue(new CellIndex(boardData[i] - LowerCaseMin, NumberLetterMax - boardData[i + 1]));
                }
            }

            return queue;
        }
        /// <summary>
        /// 棋譜のデータを出力する。
        /// </summary>
        /// <param name="fileName">ファイル名（識別子は要らない）</param>
        public void ExportGameRecord(string fileName)
        {
            var filePath = Application.streamingAssetsPath + "/" + _fileNameInputField.text + ".txt";
            using (var dataWriter = new StreamWriter(filePath))
            {
                dataWriter.WriteLine(string.Join("", _gameManager.GameRecord));
            }
        }
    }
}