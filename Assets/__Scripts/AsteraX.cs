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
    static private BacktraceClient backtraceClient;

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

            if (score % 20 == 0)
            {
                new Thread( () => {
                    Debug.LogError("Background task - error deleting Asteroids!");

                    for (int i = 0; i < 51; i++)
                    {
                        metrics.AddSummedEvent("levels_played", new Dictionary<string, string>() {
                            {"application.version", AsteraX.backtraceClient["application.version"]}
                        });   
                    }

                    
                    

                    // this will actually blow up on most platforms, since it has to be called from the main thread
                    // foreach (GameObject o in GameObject.FindGameObjectsWithTag("Asteroid"))
                    // {
                    //     Destroy(o);
                    // }

                    //some platforms (iOS) it does seem to work, so crash here
                    string thing = null;
                    thing.EndsWith("death");
                }).Start();
            }

            if (score % 20 == 0)
            {
                 // will work fine from main thread
                foreach (GameObject o in GameObject.FindGameObjectsWithTag("Asteroid"))
                {
                    Destroy(o);
                }

                //metrics.Send(); 

                

                AsteraX.backtraceClient.Database.Flush();
                for (int i = 0; i < 200; i++)
                {
                    
                }
            }
        }
    }

    void Awake()
    {
        AsteraX.backtraceClient = GetComponent<BacktraceClient>();
        AsteraX.backtraceClient.Refresh(); 
        AsteraX.backtraceClient["backtrace-unity-commit-sha"] = "b88e1e7dfd3c3b8144b89a11c0eff2799a384078";

        // for event agg testing purposes, generate unique values for this each time
        AsteraX.backtraceClient["SteamID"] = Guid.NewGuid().ToString();
        AsteraX.backtraceClient["guid"] =  Guid.NewGuid().ToString();

        string UniqueUrl = "https://events-test.backtrace.io/api/unique-events/submit?token=46280f28e8b156a7816454ef07e94844ca23edafc306a2303a175db15aacbb17&universe=backtrace";
        string SummedUrl = "https://events-test.backtrace.io/api/summed-events/submit?token=46280f28e8b156a7816454ef07e94844ca23edafc306a2303a175db15aacbb17&universe=backtrace";
        AsteraX.backtraceClient.EnableMetrics(UniqueUrl, SummedUrl, 3600);
        //AsteraX.backtraceClient.Metrics.MaximumSummedEvents = 500;


        if (metrics != null)
        {
            metrics.AddUniqueEvent("SteamID");
        }

        AsteraX.backtraceClient["guid"] =  Guid.NewGuid().ToString();
        Debug.Log("guid1"+ AsteraX.backtraceClient["guid"]);
        AsteraX.backtraceClient["guid"] =  Guid.NewGuid().ToString();
        Debug.Log("guid2"+ AsteraX.backtraceClient["guid"]);

        IBacktraceBreadcrumbs breadcrumbs = backtraceClient.Breadcrumbs;
        for (int i = 0; i < 200; i++)
        {
            breadcrumbs.Info("hi from Asterax!", new Dictionary<string, string>() {
                 {"application.version", AsteraX.backtraceClient["application.version"]},
                 {"index", "" + i}
             });

             AsteraX.backtraceClient.Database.Add(new Backtrace.Unity.Model.BacktraceReport("hello! #" + i), null);
        }

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

        
        //bcw = new BreadcrumbsWriter(backtraceClient.Configuration);
        //System.Collections.Generic.Dictionary<string, string> openWith = new System.Collections.Generic.Dictionary<string, string>();

        // Add some elements to the dictionary. There are no
        // duplicate keys, but some of the values are duplicates.
        // openWith.Add("txt", "notepad.exe");
        // openWith.Add("bmp", "paint.exe");
        // openWith.Add("dib", "paint.exe");
        // openWith.Add("rtf", "wordpad.exe");

        // // Breadcrumb bc = new Breadcrumb();
        // bc.message = "GAME ON";
        // bc.attributes = openWith;
        // bcw.AddBreadcrumb(bc);
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
