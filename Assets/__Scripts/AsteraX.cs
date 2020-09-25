﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Backtrace.Unity;

[RequireComponent(typeof(BacktraceClient))]
public class AsteraX : MonoBehaviour
{
    static private BacktraceClient backtraceClient;
    
    void Awake()
    {
        AsteraX.backtraceClient = GetComponent<BacktraceClient>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static BacktraceClient GetBacktraceClient()
    {
        return backtraceClient;
    }

    static public void GetGyroscopeDevice()
    {
        // throws error!
        int x = 0;
        int y = 100 / x;
    }
}
