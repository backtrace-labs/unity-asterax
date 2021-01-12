using System;
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
        // static property, set once
        // but you want to include something even if native crashes occur
        // still works! Let me demonstrate.
        AsteraX.backtraceClient["backtrace-unity-commit-sha"] = "517b40554fd2f09f27b90abaf493deeb92450c53";

        backtraceClient.BeforeSend =
            (Backtrace.Unity.Model.BacktraceData model) =>
            {
                // this call back is only called for C# exceptions
                // and ANRs as well (I'm fairly sure, let's find out)
                // but not native crashes obviously
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
