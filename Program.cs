using Microsoft.Extensions.ObjectPool;

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
    where T : class, I, new()
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

internal static class Program
{
    private static readonly DefaultObjectPool<IReuseCase1> ObjectPoolCase1 = new(new DefaultReuseCasePolicy<MyObject, IReuseCase1>());

    private static readonly DefaultObjectPool<IReuseCase2> ObjectPoolCase2 = new(new DefaultReuseCasePolicy<MyObject, IReuseCase2>());

    /// <summary>
    /// This method's suggested to be added or just extended to DefaultObjectPool
    /// </summary>
    /// <typeparam name="T">The type which may has multiple Reusecase</typeparam>
    /// <typeparam name="I">The Component Interface for T</typeparam>
    /// <param name="pool"></param>
    /// <param name="eval">Delegate to evaluate underlying properties immediately, as can be codedly equivalent to a partial Reset(). If u don't want to do it immediately, u can simply use Get<I>() on the pool</param>
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
        int counter = 0;
        ConsoleKeyInfo key = default;

        void showProperties(MyObject obj)
        {
            Console.WriteLine("Property1=" + (obj.Property1 ?? "null") + "\t" + "Property2=" + (obj.Property2 ?? "null"));
            Console.WriteLine("Property3=" + (obj.Property3 ?? "null") + "\t" + "Property4=" + (obj.Property4 ?? "null"));
        }

        while (true)
        {
            Console.WriteLine("[Reuse Case I: case1Obj]");
            Console.WriteLine("<-Object Got from ObjectPoolCase1!");
            var objCase1 = ObjectPoolCase1.Get<MyObject, IReuseCase1>(t =>
            {
                t.Property1 = $"A{counter}";
                t.Property2 = $"B{counter}";

                return t;
            });
            showProperties(objCase1);
            ObjectPoolCase1.Return(objCase1);
            Console.WriteLine("->ObjCase1 Returned!\r\n");

            Console.WriteLine("[Reuse Case II: case2Obj]");
            Console.WriteLine("<-Object Got from ObjectPoolCase2!");
            var objCase2 = ObjectPoolCase2.Get<MyObject, IReuseCase2>(t =>
            {
                t.Property3 = $"C{counter}";
                t.Property4 = $"D{counter}";
                return t;
            });
            showProperties(objCase2);

            ObjectPoolCase2.Return(objCase2);
            Console.WriteLine("->ObjCase2 Returned!\r\n");

            Console.WriteLine("----------------- Press any other key to Reuse Once, -------------------\r\n");
            var k = Console.ReadKey().Key;

            counter++;
        }
    }
}