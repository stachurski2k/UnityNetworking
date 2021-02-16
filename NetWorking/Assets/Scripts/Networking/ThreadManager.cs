using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;
public class ThreadManager : MonoBehaviour
{
    private static readonly List<Action> executeOnMainThread = new List<Action>();
    private static readonly List<Action> executeCopiedOnMainThread = new List<Action>();
    private static bool actionToExecuteOnMainThread = false;
    public static ThreadManager instance;
    static List<Thread> threads=new List<Thread>();
    private void Awake()
    {
        if(instance==null){
            instance=this;
        }else{
            Destroy(this);
        }
    }
    private void FixedUpdate()
    {
        UpdateMain();
    }

    public static void ExecuteOnMainThread(Action _action)
    {
        if (_action == null)
        {
            return;
        }
        lock (executeOnMainThread)
        {
            executeOnMainThread.Add(_action);
            actionToExecuteOnMainThread = true;
        }
    }
    public static void ExecuteOnNewThread(Action executeOnNewThread){
        Thread thread=new Thread(new ThreadStart(executeOnNewThread));
        threads.Add(thread);
        thread.Start();
    }
    public static Thread GetExecuteOnNewThread(Action executeOnNewThread){
        Thread thread=new Thread(new ThreadStart(executeOnNewThread));
        threads.Add(thread);
        thread.Start();
        return thread;
    }
    public static void ExecuteOnNewThread(Action executeOnNewThread,Action doneCallback){
        Thread thread=new Thread(new ThreadStart(()=>{
            executeOnNewThread();
            ExecuteOnMainThread(doneCallback);
        }));
        threads.Add(thread);
        thread.Start();
    }
    public static void ExecuteOnNewThread<T>(Func<T> executeOnNewThread,Action<T> doneCallback){
        Thread thread=new Thread(new ThreadStart(()=>{
            T result=executeOnNewThread();
            ExecuteOnMainThread(()=>{
                doneCallback(result);
            });
        }));
        threads.Add(thread);
        thread.Start();
    }
    public static void UpdateMain()
    {
        if (actionToExecuteOnMainThread)
        {
            executeCopiedOnMainThread.Clear();
            lock (executeOnMainThread)
            {
                executeCopiedOnMainThread.AddRange(executeOnMainThread);
                executeOnMainThread.Clear();
                actionToExecuteOnMainThread = false;
            }

            for (int i = 0; i < executeCopiedOnMainThread.Count; i++)
            {
                executeCopiedOnMainThread[i]();
            }
        }
    }
    private void OnApplicationQuit()
    {
        foreach (var thread in threads)
        {
           // thread.Join();
        }
    }
}
