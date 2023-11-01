using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlayMgr : MonoSingleton<GamePlayMgr>
{
    [SerializeField]
    private bool isInGame = false;
    private void OnEnable()
    {
        EventMgr.GetInstance().AddEventListener(EventDic.OnEnterMainGame, OnEnterGame);
        EventMgr.GetInstance().AddEventListener(EventDic.OnLeaveMainGame, OnLeaveGame);
    }
    private void OnDisable()
    {
        EventMgr.GetInstance().DeleteEventListener(EventDic.OnEnterMainGame, OnEnterGame);
        EventMgr.GetInstance().DeleteEventListener(EventDic.OnLeaveMainGame, OnLeaveGame);
    }
    private void OnEnterGame()
    {
        isInGame = true;
    }
    private void OnLeaveGame()
    {
        isInGame = false;
    }
}
