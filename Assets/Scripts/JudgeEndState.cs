using System.Threading;
using Cysharp.Threading.Tasks;

namespace Reversi
{
    public class JudgeEndState : IState
    {
        private GameManager _gameManager;
        public async UniTask Process(CancellationToken token)
        {
            int playerCanPlaceCount = _gameManager.GetCanPlacePositions(true, null);
            int enemyCanPlaceCount = _gameManager.GetCanPlacePositions(false, null);
            if (playerCanPlaceCount <= 0 && enemyCanPlaceCount <= 0)
            {
                _gameManager.GameEnd();
            }
            await UniTask.Yield(cancellationToken: token);
        }
        public JudgeEndState(GameManager gm)
        {
            _gameManager = gm;
        }
    }
}