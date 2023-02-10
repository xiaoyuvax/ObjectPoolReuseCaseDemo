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

public class DefaultObjectPoolPolicy4ReuseCases<T, I> : IPooledObjectPolicy<I>
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
    private static readonly DefaultObjectPool<IReuseCase1> ObjectPoolCase1 = new(new DefaultObjectPoolPolicy4ReuseCases<MyObject, IReuseCase1>());

    private static readonly DefaultObjectPool<IReuseCase2> ObjectPoolCase2 = new(new DefaultObjectPoolPolicy4ReuseCases<MyObject, IReuseCase2>());

    /// <summary>
    /// This is the method suggest to be added or just extended to DefaultObjectPool
    /// </summary>
    /// <typeparam name="T">The type which may has multiple Reusecase</typeparam>
    /// <typeparam name="I">The component interface for T</typeparam>
    /// <param name="case1"></param>
    /// <param name="eval">Delegate to evaluate underlying properties immediately, if u don't want to do it immediately, u can simply use Get<I>() on the ObjectPool</param>
    /// <returns></returns>
    public static T Get<T, I>(this DefaultObjectPool<I> case1, Func<I, I> eval = null)
        where T : I
        where I : class
    {
        I objI = case1.Get();
        objI = eval?.Invoke(objI) ?? objI;   //note: if the delegate returns null, objI would not be changed.
        return objI is T objT ? objT : default;
    }

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        /*After experimenting similar resetting mechanism in my code, i found it a big performance disaster to reset every instance,
        especially if a Type owns too many properties, and  if most of its properties r to be overwritten later.
        now i removed all resetting implementations and adopted a performance friendly solution:
        use different dedicated DefaultObjectPools for different object-reuse case(per object life-cycle) of the same type, so that the same group of properties r always overwritten exactly in reusing code(i.e.avoid messing up reuse-cases), thus to avoid resetting.and wherein u don't have to use Reset() nor custom ObjectPoolPolicy while assuring best performance.
        The only problem here is how to manage different reuse-cases, better with code, as to make it more readable and managable.
        i have contemplated some solutions :
        1) defined compulsory evaluation to predefined upon callling a new Return<T>(Func<T, T>) by using Component Interfaces.
        Class MyObject : IPropertyGroup4ReuseCase1, IPropertyGroup4ReuseCase2 {}

        var a = Case1ObjectPool.Return<IPropertyGroup4ReuseCase1>(obj => { obj.Property1 = newValue1; obj.Property2 = newValue2; })

        2) i would also expect some analyzer/compiler help, a simple warning, if reuse-case consistency is violated,
        */
        int counter = 0;
        ConsoleKeyInfo key = default;

        void showProperties(MyObject obj)
        {
            Console.WriteLine("Property1=" + (obj.Property1 ?? "null") + "\t" + "Property2=" + (obj.Property2 ?? "null"));
            Console.WriteLine("Property3=" + (obj.Property3 ?? "null") + "\t" + "Property4=" + (obj.Property4 ?? "null"));
        }

        while (!"qQ".Contains(key.KeyChar))
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

            Console.WriteLine("-----------------Press Q or q to quit, Press any other key to Reuse Once, -------------------\r\n");
            var k = Console.ReadKey().Key;

            counter++;
        }
    }
}