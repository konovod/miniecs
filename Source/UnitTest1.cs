using NUnit.Framework.Internal;
using NUnit.Framework;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;

namespace UnitTests
{
    public struct Comp1
    {
        public Comp1(int v) { V = v; }
        public int V;
    };

    public struct Comp2
    {
        public Comp2(string v) { V = v; }
        public string V;
    };

    public class System1 : ECS.System
    {
        internal System1(ECS.World aworld) : base(aworld) { }
        public static List<string> Log = new();
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

    public class SlowSystem : ECS.System
    {
        internal SlowSystem(ECS.World aworld) : base(aworld) { }
        public override void PreProcess()
        {
            System.Threading.Thread.Sleep(10);
        }
        public override void Execute()
        {
            System.Threading.Thread.Sleep(10);
        }
        public override ECS.Filter? Filter(ECS.World world)
        {
            return world.Inc<Comp1>().Exc<Comp2>();
        }

        public override void Process(ECS.Entity e)
        {
            System.Threading.Thread.Sleep(10);
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
            Assert.That(world.EntitiesCount, Is.EqualTo(2));
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
                Assert.That(world.EntitiesCount, Is.EqualTo(0));
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
            systems.Statistics.ToList().ForEach(x => Console.WriteLine(x.Key + " : " + x.Value));
        }

        [Test]
        public void DelHereSystem()
        {
            var world = new ECS.World();
            var systems = new ECS.Systems(world);
            var ent = world.NewEntity().Add(new Comp1(123)).Add(new Comp2("test"));
            systems.DelHere<Comp2>();
            systems.Init();
            Assert.Multiple(() =>
            {
                Assert.That(ent.Has<Comp1>(), Is.True);
                Assert.That(ent.Has<Comp2>(), Is.True);
            });
            systems.Execute();
            Assert.Multiple(() =>
            {
                Assert.That(ent.Has<Comp1>(), Is.True);
                Assert.That(ent.Has<Comp2>(), Is.False);
            });

        }

        [Test]
        public void EntityIteration()
        {
            var world = new ECS.World();
            for (int i = 0; i < 10; i++)
            {
                var ent = world.NewEntity();
                ent.Add(new Comp1(i + 1));
            }
            int sum = 0;
            foreach (var ent in world.EachEntity())
                sum += ent.Get<Comp1>().V;
            Assert.That(sum, Is.EqualTo(1 + 2 + 3 + 4 + 5 + 6 + 7 + 8 + 9 + 10));


            foreach (var ent in world.Each<Comp1>())
                ent.Remove<Comp1>();

            sum = 0;
            foreach (var ent in world.EachEntity())
                sum += 1;
            Assert.That(sum, Is.EqualTo(0));

            for (int i = 0; i < 10; i++)
            {
                var ent = world.NewEntity();
                ent.Add(new Comp1(i + 1));
            }

            var systems = new ECS.Systems(world);
            systems.DelHere<Comp1>();
            systems.Init();

            sum = 0;
            foreach (var ent in world.EachEntity())
                sum += 1;
            Assert.That(sum, Is.EqualTo(10));

            systems.Execute();

            sum = 0;
            foreach (var ent in world.EachEntity())
                sum += 1;
            Assert.That(sum, Is.EqualTo(0));
        }

        [Test]
        public void BenchmarkSystem()
        {
            var world = new ECS.World();
            var systems = new ECS.Systems(world);

            systems.Add(new SlowSystem(world));

            systems.Init();
            systems.Execute();
            systems.Statistics.ToList().ForEach(x => Console.WriteLine(x.Key + " : " + x.Value));
            Assert.Multiple(() =>
            {
                Assert.That(systems.Statistics["Total"], Is.GreaterThan(19).And.LessThan(25));
                Assert.That(systems.Statistics["UnitTests.SlowSystem"], Is.GreaterThan(19).And.LessThan(25));
                Assert.That(systems.Statistics["Total"], Is.GreaterThanOrEqualTo(systems.Statistics["UnitTests.SlowSystem"]));
            });
            world.NewEntity().Add(new Comp1(123)).Add(new Comp2("test"));
            world.NewEntity().Add(new Comp1(124));
            world.NewEntity().Add(new Comp1(125));
            systems.Execute();
            systems.Statistics.ToList().ForEach(x => Console.WriteLine(x.Key + " : " + x.Value));
            Assert.Multiple(() =>
            {
                Assert.That(systems.Statistics["Total"], Is.GreaterThan(39).And.LessThan(45));
                Assert.That(systems.Statistics["UnitTests.SlowSystem"], Is.GreaterThan(39).And.LessThan(45));
                Assert.That(systems.Statistics["Total"], Is.GreaterThanOrEqualTo(systems.Statistics["UnitTests.SlowSystem"]));
            });
            Console.WriteLine("Overhead: " + (systems.Statistics["Total"] * 2 - systems.Statistics.Values.Sum()));
            systems.Teardown();
        }

        [Test]
        public void ClearWorld()
        {
            var world = new ECS.World();
            for (int i = 0; i < 10; i++)
            {
                var ent = world.NewEntity();
                ent.Add(new Comp1(i + 1));
            }
            Assert.That(world.EntitiesCount, Is.EqualTo(10));
            world.DeleteAll();
            Assert.That(world.EntitiesCount, Is.EqualTo(0));
        }

        [Test]
        public void Firstcomponent()
        {
            var world = new ECS.World();
            var ent = world.NewEntity();
            ent.Add(new Comp1(123));
            Assert.That(world.FirstComponent<Comp1>().V, Is.EqualTo(123));
            world.RefFirstComponent<Comp1>().V += 1;
            Assert.That(world.FirstComponent<Comp1>().V, Is.EqualTo(124));
            ent.Destroy();
            Assert.Throws<Exception>(() => { world.FirstComponent<Comp1>(); });
        }

        [Test]
        public void AddDefaultComponent()
        {
            var world = new ECS.World();
            world.GetStorage<Comp1>();
            var ent = world.NewEntity();
            Type typ = ((new Random()).Next(1, 2) < 3.0) ? typeof(Comp1) : typeof(Comp2);
            ent.AddDefault(typ);
            Assert.That(ent.Get<Comp1>().V, Is.EqualTo(0));
            ent.Remove(typ);
            Assert.That(world.EntitiesCount, Is.EqualTo(0));
        }

    }
}