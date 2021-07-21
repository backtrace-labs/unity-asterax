using Backtrace.Unity;
using Backtrace.Unity.Interfaces;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System;
using Backtrace.Unity.Model.Breadcrumbs;
using Backtrace.Unity.Model.Metrics;

[RequireComponent(typeof(BacktraceClient))]
public class AsteraX : MonoBehaviour
{
    static public BacktraceClient backtraceClient;

    static private int incrementingNumber = 0;

    //static public BreadcrumbsWriter bcw;

    static public IBacktraceMetrics metrics
    {
        get
        {
            return backtraceClient.Metrics;
        }
    }

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

#if ((UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR)
            if (score == 100)
            {
                ConnectToSlowBackend();
            }
#endif

            if (score % 200 == 0)
            {
                new Thread( () => {
                    Debug.LogError("Background task - error deleting Asteroids!");

                    //some platforms (iOS) it does seem to work, so crash here
                    string thing = null;
                    thing.EndsWith("death");
                }).Start();
            }

            if (score % 100 == 0)
            {
                metrics.AddSummedEvent("levels_played", new Dictionary<string, string>() {
                    {"application.version", AsteraX.backtraceClient["application.version"]},
                    {"score", "" + score}
                });

                backtraceClient.Breadcrumbs.Info("Level Completed in Asterax!", new Dictionary<string, string>() {
                    {"application.version", AsteraX.backtraceClient["application.version"]},
                    {"score", "" + score}
                });
                
                 // will work fine from main thread
                foreach (GameObject o in GameObject.FindGameObjectsWithTag("Asteroid"))
                {
                    Destroy(o);
                }
            }
        }
    }

    void Awake()
    {
        AsteraX.backtraceClient = GetComponent<BacktraceClient>();
        AsteraX.backtraceClient.Refresh(); 
        AsteraX.backtraceClient["backtrace-unity-commit-sha"] = "7e10fc53f681c25f5e3774729fdfe6efa98859a8";

        // for event agg testing purposes, generate unique values for this each time
        // AsteraX.backtraceClient["SteamID"] = Guid.NewGuid().ToString();
        // AsteraX.backtraceClient["guid"] =  Guid.NewGuid().ToString();

        // string UniqueUrl = "https://events-test.backtrace.io/api/unique-events/submit?token=46280f28e8b156a7816454ef07e94844ca23edafc306a2303a175db15aacbb17&universe=backtrace&_mod_blackhole=false";
        // string SummedUrl = "https://events-test.backtrace.io/api/summed-events/submit?token=46280f28e8b156a7816454ef07e94844ca23edafc306a2303a175db15aacbb17&universe=backtrace&_mod_blackhole=false";
        // AsteraX.backtraceClient.EnableMetrics(UniqueUrl, SummedUrl, 3600);
        //AsteraX.backtraceClient.Metrics.MaximumSummedEvents = 500;

        backtraceClient.Breadcrumbs.Info("Startup in Asterax!", new Dictionary<string, string>() {
            {"application.version", AsteraX.backtraceClient["application.version"]},
        });

        //AsteraX.backtraceClient.Database.Add(new Backtrace.Unity.Model.BacktraceReport("hello! #" + i), null);

        backtraceClient.BeforeSend =
            (Backtrace.Unity.Model.BacktraceData model) =>
            {
                model.Attributes.Attributes.Add("customAttributeFromBeforeSend", "IncrementingNumber" + incrementingNumber++);

                using (StreamWriter outputFile = new StreamWriter(Path.Combine(Application.persistentDataPath, "data.txt"), true))
                {
                    outputFile.WriteLine(incrementingNumber);
                }

                return model;
            };
        Debug.Log(Application.persistentDataPath);
    }

#if ((UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR)
    static private void ConnectToSlowBackend() 
    {
        Debug.Log("ConnectToSlowBackend - in");
        WebClient client = new WebClient();
        Stream stream = client.OpenRead("https://deelay.me/10000/https://picsum.photos/200/300");
        StreamReader reader = new StreamReader(stream);
        string content = reader.ReadToEnd(); 
        Debug.Log("Content length: " + content.Length);

        Debug.Log("ConnectToSlowBackend - out");
    }
#endif


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
