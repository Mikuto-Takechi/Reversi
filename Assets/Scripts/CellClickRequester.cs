using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Reversi
{
    public class CellClickRequester : MonoBehaviour, IPointerClickHandler
    {
        public static Subject<CellIndex> OnClicked = new();
        
        [SerializeField] private CellIndex _cellIndex;
        
        public void OnPointerClick(PointerEventData eventData)
        {
            OnClicked.OnNext(_cellIndex);
        }
    }
}