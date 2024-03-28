namespace miniecs;


public struct Comp1(int v)
{
    public int V = v;
};

public struct Comp2
{
    string v;
};

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void CreateWorld()
    {
        var world = new World();
    }

    [Test]
    public void CreateEntities()
    {
        var world = new World();
        var ent1 = world.NewEntity();
        var ent11 = ent1;
        var ent2 = world.NewEntity();
        Assert.That(ent11, Is.EqualTo(ent1));
        Assert.That(ent2, Is.Not.EqualTo(ent1));
    }

    [Test]
    public void AddRemoveComponents()
    {
        var world = new World();
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
        var world = new World();
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
        var world = new World();
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
        var world = new World();
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



}