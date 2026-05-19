using UnityEngine;
using DG.Tweening;

public class SpinningUI : MonoBehaviour
{
    bool isSpinning = false;
    void Start(){
        if(!isSpinning){
            StartSpinning();
        }
    }

    public void StartSpinning(){
        isSpinning = true;
            transform.GetComponent<RectTransform>().DORotate(new Vector3(0f, 0f, 360f), 5f, RotateMode.FastBeyond360)
                .SetLoops(-1)
                .SetEase(Ease.Linear);
    }
}
