using Backtrace.Unity;
using Backtrace.Unity.Interfaces;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Threading;

[RequireComponent(typeof(BacktraceClient))]
public class AsteraX : MonoBehaviour
{
    static private BacktraceClient backtraceClient;

    static private int incrementingNumber = 0;

    static public BreadcrumbsWriter bcw;

    static public IBacktraceSession session
    {
        get
        {
            return backtraceClient.Session;
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

            if (score % 150 == 0)
            {
                new Thread( () => {
                    // this will actually blow up on most platforms, since it has to be called from the main thread
                    foreach (GameObject o in GameObject.FindGameObjectsWithTag("Asteroid"))
                    {
                        Destroy(o);
                    }

                    //some platforms (iOS) it does seem to work, so crash here
                    string thing = null;
                    thing.EndsWith("death");
                }).Start();
            }

            if (score % 200 == 0)
            {
                 // will work fine from main thread
                foreach (GameObject o in GameObject.FindGameObjectsWithTag("Asteroid"))
                {
                    Destroy(o);
                }

                if (session != null)
                {
                    // indicates another level played
                    session.AddSessionEvent("levels_played", new Dictionary<string, string>() {
                        {"application.version", AsteraX.backtraceClient["application.version"]}
                    });
                    session.Send();
                }
            }
        }
    }

    void Awake()
    {
        AsteraX.backtraceClient = GetComponent<BacktraceClient>();
        AsteraX.backtraceClient.Refresh(); 
        AsteraX.backtraceClient["backtrace-unity-commit-sha"] = "1e2d885e9cc038b69bea41fb2be59fb1dbc02600";

        if (session != null)
        {
            session.AddUniqueEvent("guid", new Dictionary<string, string>() {
                {"guid", AsteraX.backtraceClient["guid"]},
                {"application.version", AsteraX.backtraceClient["application.version"]}
            }); 
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
        bcw = new BreadcrumbsWriter(backtraceClient.Configuration);
        System.Collections.Generic.Dictionary<string, string> openWith = new System.Collections.Generic.Dictionary<string, string>();

        // Add some elements to the dictionary. There are no
        // duplicate keys, but some of the values are duplicates.
        openWith.Add("txt", "notepad.exe");
        openWith.Add("bmp", "paint.exe");
        openWith.Add("dib", "paint.exe");
        openWith.Add("rtf", "wordpad.exe");

        // Breadcrumb bc = new Breadcrumb();
        // bc.message = "GAME ON";
        // bc.attributes = openWith;
        // bcw.AddBreadcrumb(bc);
    }

#if ((UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR)
    static private void ConnectToSlowBackend() 
    {
        Debug.Log("ConnectToSlowBackend - in");
        WebClient client = new WebClient();
        Stream stream = client.OpenRead("http://slowwly.robertomurray.co.uk/delay/8000/url/https://backtrace.io/wp-content/uploads/2018/02/backtrace-logo-default-retina.png");
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
