using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class BkMusic : MonoBehaviour
{
    private static BkMusic instance;
    public static BkMusic Instance => instance;

    private AudioSource bkSource;

    // Start is called before the first frame update
    void Awake()
    {
        instance= this;
        DontDestroyOnLoad(this);

        bkSource = GetComponent<AudioSource>();
        bkSource.Play();
        MusicData data = GameDataMgr.Instance.musicData;
        SetIsOpen(data.musicIsOpen);
        ChangeValue(data.musicValue);
    }

    //开关方法
    public void SetIsOpen(bool isOpen)
    {
        bkSource.mute=!isOpen;
    }

    //大小方法
    public void ChangeValue(float v)
    {
        bkSource.volume = v;
    }
}
