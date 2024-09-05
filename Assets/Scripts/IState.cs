using System.Threading;
using Cysharp.Threading.Tasks;

namespace Reversi
{
    public interface IState
    {
        UniTask Process(CancellationToken token);
    }
}