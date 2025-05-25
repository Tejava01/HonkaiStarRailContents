using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIWidgetScreen : MonoBehaviour
{
    [Header("PivotRightTop")]
    [SerializeField] private Button startBtn;
    [SerializeField] private Button resetBtn;
    [SerializeField] private Text curRoundTxt;
    [SerializeField] private Text stateTxt;

    [Header("PivotLeftTop")]
    [SerializeField] private Text goal1Txt;
    [SerializeField] private Text goal2Txt;
    [SerializeField] private Text recordTxt;

    //--------------------------------------------

    public Button StartBtn { get { return startBtn; } }
    public Button ResetBtn { get { return resetBtn; } }
    public Text StateTxt { get { return curRoundTxt; } }
    public Text CurRoundTxt { get { return stateTxt; } }

    public Text Goal1Txt { get { return goal1Txt; } }
    public Text Goal2Txt { get { return goal2Txt; } }  
    public Text RecordTxt { get { return recordTxt; } }    

}
