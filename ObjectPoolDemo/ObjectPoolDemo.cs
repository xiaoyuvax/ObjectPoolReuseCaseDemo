using Microsoft.Extensions.ObjectPool;
using System;
using System.IO;
using Wima.Log;

namespace ObjectPoolResuseCases;

public interface IReuseCase1
{
    public string Property1 { get; set; }
    public string Property2 { get; set; }
};

public interface IReuseCase2
{
    public string Property3 { get; set; }
    public string Property4 { get; set; }
};

/// <summary>
/// Default PooledObjectPolicy for ReuseCase paradigm
/// </summary>
/// <typeparam name="T">The implementing type to be reused</typeparam>
/// <typeparam name="I">The Component Interface to associate with corresponding ObjectPool</typeparam>
public class DefaultReuseCasePolicy<T, I> : IPooledObjectPolicy<I>
    where T : I, new()
    where I : class
{
    public I Create() => new T();

    public bool Return(I obj) => true;
}

public class MyObject : IReuseCase1, IReuseCase2
{
    public string Property1 { get; set; }
    public string Property2 { get; set; }
    public string Property3 { get; set; }
    public string Property4 { get; set; }
}

internal static class ObjectPoolDemo
{
    private static WimaLogger log = new("ObjectPoolReuseCaseDemo");

    private static readonly DefaultObjectPool<IReuseCase1> ObjectPoolCase1 = new(new DefaultReuseCasePolicy<MyObject, IReuseCase1>());

    private static readonly DefaultObjectPool<IReuseCase2> ObjectPoolCase2 = new(new DefaultReuseCasePolicy<MyObject, IReuseCase2>());

    /// <summary>
    /// This method's suggested to be added or just extended to DefaultObjectPool<T>,
    /// as to retrieve an instance from the pool with contracted subsets of properties according to multiple lifecycle-lines of the type.
    /// This method is only a helper and demonstration of the 3 steps to use Component Interfaces, but not a must.
    /// </summary>
    /// <typeparam name="T">The type which may has multiple Reuse Cases</typeparam>
    /// <typeparam name="I">One of the component interfaces for T</typeparam>
    /// <param name="case1"></param>
    /// <param name="eval">Delegate to evaluate underlying properties immediately, if u don't want to do it immediately, u can simply use Get<I>() on the ObjectPool</param>
    /// <returns></returns>
    public static T Get<T, I>(this DefaultObjectPool<I> pool, Func<I, I> eval = null)
        where T : I
        where I : class
    {
        I objI = pool.Get();
        objI = eval?.Invoke(objI) ?? objI;   //note: if the delegate returns null, objI would not be changed.
        return objI is T objT ? objT : default;
    }

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        log.LogModes = LogMode.Console;
        int counter = 0;

        static void showProperties(MyObject obj)
        {
            log.Info("Property1=" + (obj.Property1 ?? "null") + "\t" + "Property2=" + (obj.Property2 ?? "null"));
            log.Info("Property3=" + (obj.Property3 ?? "null") + "\t" + "Property4=" + (obj.Property4 ?? "null"));
        }

        while (true)
        {
            log.Info("[Reuse Case I: case1Obj]");
            log.Info("<-Object Got from ObjectPoolCase1!");
            var objCase1 = ObjectPoolCase1.Get<MyObject, IReuseCase1>(t =>
            {
                t.Property1 = $"A{counter}";
                t.Property2 = $"B{counter}";

                return t;
            });
            showProperties(objCase1);
            ObjectPoolCase1.Return(objCase1);
            log.Info("->ObjCase1 Returned!\r\n");

            log.Info("[Reuse Case II: case2Obj]");
            log.Info("<-Object Got from ObjectPoolCase2!");
            var objCase2 = ObjectPoolCase2.Get<MyObject, IReuseCase2>(t =>
            {                
                t.Property3 = $"C{counter}";
                t.Property4 = $"D{counter}";
                return t;
            });
            showProperties(objCase2);

            ObjectPoolCase2.Return(objCase2);
            log.Info("->ObjCase2 Returned!\r\n");

            log.Info("----------------- Press any other key to Reuse Once, -------------------\r\n");
            var k = Console.ReadKey().Key;

            counter++;
        }
    }
}