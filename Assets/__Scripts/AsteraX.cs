using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Backtrace.Unity;

[RequireComponent(typeof(BacktraceClient))]
public class AsteraX : MonoBehaviour
{
    static private BacktraceClient backtraceClient;

    static private int incrementingNumber = 0;
    
    void Awake()
    {
        AsteraX.backtraceClient = GetComponent<BacktraceClient>();

        backtraceClient.BeforeSend =
            (Backtrace.Unity.Model.BacktraceData model) =>
            {
                model.Attributes.Attributes.Add("customAttributeFromBeforeSend", "IncrementingNumber" + incrementingNumber++);
                return model;
            };

        if (String.IsNullOrWhiteSpace(PlayerPrefs.GetString("backtrace_url")))
        {
            PlayerPrefs.SetString("backtrace_url", AsteraX.backtraceClient.Configuration.ServerUrl);
            PlayerPrefs.Save();
        }
        
        AsteraX.backtraceClient.Configuration.ServerUrl = PlayerPrefs.GetString("backtrace_url");
        Debug.Log("backtrace_url: " + PlayerPrefs.GetString("backtrace_url"));
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
        // iOS allows you to divide by zero, cool huh? But crash anyways pls
        throw new System.DivideByZeroException("Attempted to divide by zero");
    }
}
