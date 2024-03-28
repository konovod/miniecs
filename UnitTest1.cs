namespace miniecs;

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

}