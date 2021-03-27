using Backtrace.Unity.Json;
using Backtrace.Unity.Model;
using System.IO;
using System.Collections.Generic;  
using UnityEngine;
using System;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public class Breadcrumb
{
    public long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(); 
    private long id = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    public string level = "debug", type = "log", message;    
    public Dictionary<string, string> attributes;

    /// <summary>
    /// Convert data to JSON
    /// </summary>
    /// <returns>Data JSON string</returns>
    public string ToJson()
    {
        var jObject = new BacktraceJObject(new Dictionary<string, string>()
        {
            ["level"] = level,
            ["type"] = type,
            ["message"] = message,
        });
        jObject.Add("timestamp", timestamp);
        jObject.Add("id", id);

        jObject.Add("attributes", new  BacktraceJObject(attributes));
        return jObject.ToJson();
    }

    
}

public class BreadcrumbsWriter
{
    private static readonly string BREADCRUMBS_DIRNAME = "breadcrumbs";
    private static readonly string BREADCRUMB_FILENAME = "bt-breadcrumbs-0";
    private readonly long maxFileSize = 64 * 1024; //64kB
    private readonly string breadcrumbsFilePath;
    private readonly LinkedList<string> breadcrumbs;
    private readonly object syncLock = new object();
    private readonly bool autoWrite;
    private readonly int waitBeforeWrite;
    private long breadcrumbsByteSize = 0L;
    private bool writeScheduled = false;

    /// <summary>
    /// Minimal constructor. Sets default of autowrite, 100ms wait before write and 32kB max breadcrumb file size
    /// </summary>
    /// <param name="backtraceConfiguration">BacktraceConfiguration</param>
    /// <returns>BreadcrumbsWriter</returns>
    public BreadcrumbsWriter(BacktraceConfiguration backtraceConfiguration) : this(backtraceConfiguration, true, 100, 32 * 1024) { }

    /// <summary>
    /// Full options constructor.
    /// </summary>
    /// <param name="backtraceConfiguration">BacktraceConfiguration</param>
    /// <param name="autowrite">Whether or not to spawn a background thread to write new breadcrumbs to disk</param>
    /// <param name="millisWaitBeforeWrite">How many millis to wait before spawning a write thread</param>
    /// <param name="backtraceConfiguration">BacktraceConfiguration</param>
    /// <returns>BreadcrumbsWriter</returns>
    public BreadcrumbsWriter(BacktraceConfiguration backtraceConfiguration, bool autowrite, int millisWaitBeforeWrite, long maxFileSize)
    {
        this.waitBeforeWrite = millisWaitBeforeWrite;
        this.autoWrite = autowrite;
        this.maxFileSize = maxFileSize;
        string breadcrumbsDir = backtraceConfiguration.GetFullDatabasePath() + BREADCRUMBS_DIRNAME;
        if (!Directory.Exists(breadcrumbsDir)) 
        {
            Directory.CreateDirectory(breadcrumbsDir);
        }
        
        breadcrumbsFilePath = breadcrumbsDir + Path.DirectorySeparatorChar + BREADCRUMB_FILENAME;
        if (File.Exists(breadcrumbsFilePath))
        {
            breadcrumbsByteSize = new FileInfo(breadcrumbsFilePath).Length;
            breadcrumbs = new LinkedList<string>(File.ReadAllLines(breadcrumbsFilePath));
        }
        else
        {
           breadcrumbs = new LinkedList<string>();
        }
    }

    /// <summary>
    /// Add breadcrumb by providing mandatory fields
    /// </summary>
    /// <param name="timestamp">millis since epoch</param>
    /// <param name="level">debug,info,warning,error,fatal</param>
    /// <param name="type">manual,log,navigation,http,system,user,configuration</param>
    /// <param name="message">the message</param>
    /// <param name="attributes">K,V pair of attributes</param>
    public void AddBreadcrumb(int timestamp, string level, string type, string message, Dictionary<string, string> attributes)
    {
        Breadcrumb bc = new Breadcrumb();
        bc.timestamp = timestamp;
        bc.level = level;
        bc.type = type;
        bc.message = message;
        bc.attributes = attributes;
        AddBreadcrumb(bc);
    }

    /// <summary>
    /// Writes in memory list to file
    /// </summary>
    // Note: this is not performance optimized at all. Pretty naive truncate/rewrite implementation that could be a lot smarter
    // Using the autowrite with waitForMillis boolean can prevent writing to disk for each breadcrumb at the cost of possibly not having the latest breadcrumbs
    // Possibly using async would help here too - but it's advised to call this from a background thread anyways (it should be threadsafe)
    public void Write()
    {
        writeScheduled = true;
        lock(syncLock)
        {
            FileStream breadcrumbsFileStream = new FileStream(breadcrumbsFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
            StreamWriter breadcrumbsFileStreamWriter = new StreamWriter(breadcrumbsFileStream);
            // truncate
            breadcrumbsFileStreamWriter.BaseStream.SetLength(0);
            // and write all
            foreach (string line in breadcrumbs)
            {
                breadcrumbsFileStreamWriter.WriteLine(line);
            }
            breadcrumbsFileStreamWriter.Flush();
            breadcrumbsFileStreamWriter.Close();
            writeScheduled = false;
        }
    }

    /// <summary>
    /// Add breadcrumb by providing Breadcrumb object
    /// </summary>
    /// <param name="breadcrumb">Breadcrumb</param>
    public void AddBreadcrumb(Breadcrumb breadcrumb)
    {
        string json = breadcrumb.ToJson();
        long breadcrumbSize = Encoding.UTF8.GetBytes(json).Length;

        lock(syncLock)
        {
            breadcrumbsByteSize += breadcrumbSize;

            // pop breadcrumbs until there's enough space
            while (breadcrumbsByteSize > maxFileSize)
            {
                string toBePurged = breadcrumbs.First.Value;
                breadcrumbs.RemoveFirst();
                breadcrumbsByteSize -= Encoding.UTF8.GetBytes(toBePurged).Length;
            }

            breadcrumbs.AddLast(json);
        }

        // if a write has been sheduled, the previous record will be written when it triggers, so don't schedule another
        if (autoWrite && !writeScheduled)
        {
            writeScheduled = true;
            Task.Run(() => 
            {
                this.Write();
            }).Wait(waitBeforeWrite);
        }
    }

}