using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class SpinningUI : MonoBehaviour
{
    void Start(){
        transform.GetComponent<RectTransform>().DORotate(new Vector3(0f, 0f, 360f), 5f, RotateMode.FastBeyond360)
            .SetLoops(-1)
            .SetEase(Ease.Linear);
    }
}
