using Backtrace.Unity;
using UnityEngine;
using System.IO;
using System.Net;

[RequireComponent(typeof(BacktraceClient))]
public class AsteraX : MonoBehaviour
{
    static private BacktraceClient backtraceClient;

    static private int incrementingNumber = 0;

    static private int _score;
    static public int score
    {
        get 
        {
            return _score;
        }
        set
        {
            _score = value;

            if (score == 100)
            {
                ConnectToSlowBackend();
            }
        }
    }

    void Awake()
    {
        AsteraX.backtraceClient = GetComponent<BacktraceClient>();
        AsteraX.backtraceClient["backtrace-unity-commit-sha"] = "ad3a94cb7cd18fb174684f904a76e2d3d9f0d115";

        backtraceClient.BeforeSend =
            (Backtrace.Unity.Model.BacktraceData model) =>
            {
                model.Attributes.Attributes.Add("customAttributeFromBeforeSend", "IncrementingNumber" + incrementingNumber++);
                return model;
            };
    }

    static private void ConnectToSlowBackend() 
    {
        Debug.Log("ConnectToSlowBackend - in");
        WebClient client = new WebClient();
        Stream stream = client.OpenRead("http://slowwly.robertomurray.co.uk/delay/5500/url/https://backtrace.io/wp-content/uploads/2018/02/backtrace-logo-default-retina.png");
        StreamReader reader = new StreamReader(stream);
        string content = reader.ReadToEnd(); 
        Debug.Log("Content length: " + content.Length);
        Debug.Log("ConnectToSlowBackend - out");
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
