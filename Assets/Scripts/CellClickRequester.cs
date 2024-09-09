using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Reversi
{
    public class CellClickRequester : MonoBehaviour, IPointerClickHandler
    {
        public static UniTaskCompletionSource<CellIndex> CompletionSource;
        [SerializeField] private CellIndex _cellIndex;
        
        public void OnPointerClick(PointerEventData eventData)
        {
            CompletionSource?.TrySetResult(_cellIndex);
        }
    }
}