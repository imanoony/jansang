using System;
using UnityEngine;

public class InSceneAudioPlayer : MonoBehaviour
{
    public int clipIndex;
    public bool tutorial;
    private void Start()
    {
        GameManager.Instance.Audio.PlayBgm(GameManager.Instance.Audio.bgmClips[clipIndex], loop:true);
        if (!tutorial) GameManager.Instance.UI.ShowGuideText("스테이지에 있는 모든 적을 쓰러뜨리세요.");
    }
}
