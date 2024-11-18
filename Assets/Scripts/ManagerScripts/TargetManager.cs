using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargetManager : MonoBehaviour
{
    // Start is called before the first frame update


    public RectTransform p1IndicatorArrow;
    public Transform p2Positioin;

    public void Update()
    {

        p1IndicatorArrow.GetComponent<RectTransform>().rotation = Quaternion.LookRotation(p2Positioin.position - p1IndicatorArrow.position);

    }

}
