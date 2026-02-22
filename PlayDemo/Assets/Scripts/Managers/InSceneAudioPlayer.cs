using System;
using UnityEngine;

public class InSceneAudioPlayer : MonoBehaviour
{
    public int clipIndex;

    private void Start()
    {
        GameManager.Instance.Audio.PlayBgm(GameManager.Instance.Audio.bgmClips[clipIndex], loop:true);
    }
}
