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
        Assert.AreEqual(ent1, ent11);
        Assert.AreNotEqual(ent11, ent2);
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
    public void GetRefComponents()
    {
        var world = new World();
        var ent1 = world.NewEntity();
        ent1.Add(new Comp1(123));
        var c1 = ent1.Get<Comp1>();
        Assert.That(c1.V, Is.EqualTo(123));
        c1.V = 124;
        Assert.That(c1.V, Is.EqualTo(124));
        Assert.That(ent1.Get<Comp1>().V, Is.EqualTo(123));

        ref var c2 = ref ent1.GetRef<Comp1>();
        Assert.That(c2.V, Is.EqualTo(123));
        c2.V = 125;
        Assert.That(c2.V, Is.EqualTo(125));
        Assert.That(ent1.Get<Comp1>().V, Is.EqualTo(125));

    }

}