

using NUnit.Framework.Internal;

public struct Comp1(int v)
{
    public int V = v;
};

public struct Comp2(string v)
{
    public string V = v;
};

public class System1(ECS.World world) : ECS.System(world)
{
    public static List<string> Log = [];
    public override void Init()
    {
        Log.Add("Init");
    }
    public override void PreProcess()
    {
        Log.Add("PreProcess");
    }
    public override void Execute()
    {
        Log.Add("Execute");
    }
    public override void Teardown()
    {
        Log.Add("Teardown");
    }
    public override ECS.Filter? Filter(ECS.World world)
    {
        Log.Add("Filter");
        return world.Inc<Comp1>().Exc<Comp2>();
    }

    public override void Process(ECS.Entity e)
    {
        Log.Add(string.Format("Process {0}", e.Get<Comp1>().V.ToString()));
    }
}

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void CreateWorld()
    {
        var world = new ECS.World();
    }

    [Test]
    public void CreateEntities()
    {
        var world = new ECS.World();
        var ent1 = world.NewEntity();
        var ent11 = ent1;
        var ent2 = world.NewEntity();
        Assert.That(ent11, Is.EqualTo(ent1));
        Assert.That(ent2, Is.Not.EqualTo(ent1));
    }

    [Test]
    public void AddRemoveComponents()
    {
        var world = new ECS.World();
        var ent1 = world.NewEntity();
        var ent2 = world.NewEntity();
        Assert.That(ent1.Has<Comp1>(), Is.False);
        Assert.That(ent1.Has<Comp2>(), Is.False);
        ent1.Add(new Comp1(123));
        Assert.That(ent1.Has<Comp1>(), Is.True);
        Assert.That(ent1.Has<Comp2>(), Is.False);
        ent2.Add(new Comp1(124));
        Assert.That(ent1.Get<Comp1>().V, Is.EqualTo(123));
        Assert.That(ent2.Get<Comp1>().V, Is.EqualTo(124));
        ent1.Remove<Comp1>();
        Assert.That(ent1.Has<Comp1>(), Is.False);
        Assert.That(ent2.Has<Comp1>(), Is.True);
        Assert.That(ent2.Get<Comp1>().V, Is.EqualTo(124));
    }

    [Test]
    public void GetRefSetComponents()
    {
        var world = new ECS.World();
        var ent1 = world.NewEntity();
        ent1.Add(new Comp1(123));
        var c1 = ent1.Get<Comp1>();
        Assert.That(c1.V, Is.EqualTo(123));
        c1.V = 124;
        Assert.That(c1.V, Is.EqualTo(124));
        Assert.That(ent1.Get<Comp1>().V, Is.EqualTo(123));
        ent1.Set(c1);
        Assert.That(ent1.Get<Comp1>().V, Is.EqualTo(124));

        ref var c2 = ref ent1.GetRef<Comp1>();
        Assert.That(c2.V, Is.EqualTo(124));
        c2.V = 125;
        Assert.That(c2.V, Is.EqualTo(125));
        Assert.That(ent1.Get<Comp1>().V, Is.EqualTo(125));

    }

    [Test]
    public void SimpleIteration()
    {
        var world = new ECS.World();
        for (int i = 0; i < 10; i++)
        {
            var ent = world.NewEntity();
            ent.Add(new Comp1(i + 1));
        }
        int sum = 0;
        foreach (var ent in world.Each<Comp1>())
            sum += ent.Get<Comp1>().V;
        Assert.That(sum, Is.EqualTo(1 + 2 + 3 + 4 + 5 + 6 + 7 + 8 + 9 + 10));
    }

    [Test]
    public void IterationWithDeletion()
    {
        var world = new ECS.World();
        for (int i = 0; i < 10; i++)
        {
            var ent = world.NewEntity();
            ent.Add(new Comp1(i + 1));
        }
        int sum = 0;
        foreach (var ent in world.Each<Comp1>())
        {
            int i = ent.Get<Comp1>().V;
            if (i % 2 == 1)
                ent.Remove<Comp1>();
            else
                sum += i;
        }
        Assert.That(sum, Is.EqualTo(2 + 4 + 6 + 8 + 10));
    }

    [Test]
    public void SimpleFilters()
    {
        var world = new ECS.World();
        for (int i = 1; i < 11; i++)
        {
            var ent = world.NewEntity();
            ent.Add(new Comp1(i));
            if (i % 2 == 1)
                ent.Add(new Comp2(i.ToString()));
        }
        var filter = world.Inc<Comp1>().Exc<Comp2>();
        int sum = 0;
        foreach (var ent in filter)
            sum += ent.Get<Comp1>().V;
        Assert.That(sum, Is.EqualTo(2 + 4 + 6 + 8 + 10));
    }

    [Test]
    public void DestroyEntity()
    {
        var world = new ECS.World();
        var ent = world.NewEntity();
        ent.Add(new Comp1(123)).Add(new Comp2("Test"));
        ent.Destroy();
        Assert.Multiple(() =>
        {
            Assert.That(ent.Has<Comp1>(), Is.False);
            Assert.That(ent.Has<Comp2>(), Is.False);
        });
        ent.Destroy();
        Assert.That(ent.Has<Comp1>(), Is.False);
    }

    [Test]
    public void SimpleSystem()
    {
        var world = new ECS.World();
        var systems = new ECS.Systems(world);

        systems.Add(new System1(world));

        Assert.That(System1.Log, Is.Empty);

        systems.Init();
        Assert.That(System1.Log, Is.EqualTo(new List<string> { "Init", "Filter" }));

        System1.Log.Clear();
        systems.Execute();
        Assert.That(System1.Log, Is.EqualTo(new List<string> { "PreProcess", "Execute" }));

        System1.Log.Clear();
        world.NewEntity().Add(new Comp1(123)).Add(new Comp2("test"));
        world.NewEntity().Add(new Comp1(124));
        systems.Execute();
        Assert.That(System1.Log, Is.EqualTo(new List<string> { "PreProcess", "Process 124", "Execute" }));

        System1.Log.Clear();
        systems.Teardown();
        Assert.That(System1.Log, Is.EqualTo(new List<string> { "Teardown" }));
    }

}