using UnityEngine;
using System;
using System.Collections;
using System.Diagnostics;
public class SSTimer : IDisposable 
{
    private string    m_tag         = string.Empty;
    private Stopwatch m_stopWatch   = null;
    bool m_bRed;
    string m_ReturnName = "";
    public SSTimer()
    {

    }

    public SSTimer(string tag, bool isRed = false) 
    {
        m_tag       = tag;
        m_stopWatch = Stopwatch.StartNew();
        m_bRed = isRed;
    }
    public void Stop()
    {
        m_stopWatch.Stop();
    }
    public void Dispose() 
    {
        m_stopWatch.Stop();
        if( m_bRed == false)
            UnityEngine.Debug.Log(m_tag + " 加载耗时:  毫秒 == " + m_stopWatch.ElapsedMilliseconds);
        else
            UnityEngine.Debug.LogError(m_tag + " 加载耗时:  毫秒 == " + m_stopWatch.ElapsedMilliseconds);
    }
}

