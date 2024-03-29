#nullable enable

using System.Collections.Generic;
using System;

namespace ECS
{


  public struct Entity
  {
    internal World World;
    internal int Id;

    public Entity Add<T>(in T item) where T : struct
    {
      Pool<T> pool = World.GetStorage<T>();
      int index = pool.Add(Id);
      pool.Items.Items[index] = item;
      return this;
    }

    public readonly bool Has<T>() where T : struct
    {
      return World.GetStorage<T>().Has(Id);
    }

    public readonly bool Has(Type type)
    {
      if (World.GetStorage(type) is IPool storage)
        return storage.Has(Id);
      else
        return false;
    }

    public Entity Remove<T>() where T : struct
    {
      World.GetStorage<T>().Remove(Id);
      return this;
    }

    public Entity RemoveIfPresent<T>() where T : struct
    {
      if (Has<T>())
        World.GetStorage<T>().Remove(Id);
      return this;
    }

    public Entity Remove(Type type)
    {
      World.GetStorage(type).Remove(Id);
      return this;
    }

    public Entity RemoveIfPresent(Type type)
    {
      if (Has(type))
        World.GetStorage(type).Remove(Id);
      return this;
    }

    public Entity Set<T>(in T item) where T : struct
    {
      if (Has<T>())
      {
        Pool<T> pool = World.GetStorage<T>();
        int index = pool.IndexByEntity[Id];
        pool.Items.Items[index] = item;
      }
      else
        Add(item);
      return this;
    }

    public readonly T Get<T>() where T : struct
    {
      Pool<T> pool = World.GetStorage<T>();
      int index = pool.Find(Id);
      return pool.Items.Items[index];
    }
    public ref T GetRef<T>() where T : struct
    {
      Pool<T> pool = World.GetStorage<T>();
      int index = pool.Find(Id);
      return ref pool.Items.Items[index];
    }

    public void Destroy()
    {
      foreach (var type in World.pools.Keys)
        RemoveIfPresent(type);
    }

  }

  public class World
  {
    protected int EntitiesCount;
    public Entity NewEntity()
    {
      Entity entity;
      entity.World = this;
      entity.Id = EntitiesCount++;
      return entity;
    }

    internal Dictionary<Type, IPool> pools = new();

    internal Pool<T> GetStorage<T>() where T : struct
    {
      if (!pools.TryGetValue(typeof(T), out IPool? pool))
      {
        pool = new Pool<T>();
        pools.Add(typeof(T), pool);
      }
      return pool as Pool<T>;
    }

    public ComponentEnumerable Each<T>() where T : struct
    {
      return new ComponentEnumerable(this, typeof(T));
    }

    internal IPool? GetStorage(Type type)
    {
      if (pools.TryGetValue(type, out IPool? result))
        return result;
      else
        return null;
    }

    public int CountComponents<T>() where T : struct
    {
      if (pools.TryGetValue(typeof(T), out IPool? result))
        return result.Count();
      else
        return 0;
    }
    public int CountComponents(Type type)
    {
      if (pools.TryGetValue(type, out IPool? result))
        return result.Count();
      else
        return 0;
    }

    public Filter Inc<T>()
    {
      return new Filter(this).Inc<T>();
    }

  }
  internal interface IPool
  {
    int Count();
    int Find(int id);
    bool Has(int id);
    void Remove(int id);
    int Add(int id);

    int IdByIndex(int id);
  }

  // Created because older c# do not support getting ref on a list element
  internal class MyList<T>
  {
    public T[] Items = new T[128];
    public int Count { get; private set; } = 0;

    public MyList() { }
    public void Add(T item)
    {
      if (Count >= Items.Length)
      {
        Array.Resize(ref Items, Items.Length * 2);
      }
      Items[Count] = item;
      Count++;
    }
    public void RemoveLast()
    {
      Count--;
    }
    public void Clear()
    {
      Count = 0;
    }

  }

  internal class Pool<T> : IPool where T : struct
  {
    public int Count() { return Items.Count; }
    public MyList<T> Items = new();
    public List<int> Entities = new(128);
    public Dictionary<int, int> IndexByEntity = new();

    public int Find(int id)
    {
      return IndexByEntity[id];
    }
    public bool Has(int id)
    {
      return IndexByEntity.ContainsKey(id);
    }
    public void Remove(int id)
    {
      int index = IndexByEntity[id];
      int last = Items.Count - 1;
      if (index != last)
      {
        IndexByEntity[Entities[last]] = index;
        Items.Items[index] = Items.Items[last];
        Entities[index] = Entities[last];
      }
      Items.RemoveLast();
      Entities.RemoveAt(last);
      IndexByEntity.Remove(id);
    }
    public int Add(int id)
    {
      Items.Add(default);
      Entities.Add(id);
      int index = Items.Count - 1;
      IndexByEntity.Add(id, index);
      return index;
    }

    public int IdByIndex(int id)
    {
      return Entities[id];
    }

    public void Clear()
    {
      Entities.Clear();
      Items.Clear();
      IndexByEntity.Clear();
    }

  }

  public readonly struct ComponentEnumerable
  {
    public ComponentEnumerable(World aworld, Type? atype)
    {
      world = aworld;
      type = atype;
    }

    readonly World world;
    readonly Type? type;

    public ComponentEnumerator GetEnumerator()
    {
      return new ComponentEnumerator(world, type);
    }
  }


  public struct ComponentEnumerator
  {

    public ComponentEnumerator(World aworld, Type? atype)
    {
      world = aworld;
      if (atype is Type type)
        pool = aworld.GetStorage(type);
      else
        pool = null;
      index = 0;
      cached = -1;
    }

    internal readonly World world;
    internal readonly IPool? pool;
    int index;
    int cached;

    public bool MoveNext()
    {
      if (pool is null)
        return false;
      if (index >= pool.Count())
        return false;
      int entity = pool.IdByIndex(index);
      if (entity == cached)
      {
        index++;
        if (index >= pool.Count())
          return false;
        cached = pool.IdByIndex(index);
      }
      else
        cached = entity;
      return true;
    }

    public Entity Current
    {
      get
      {
        Entity entity;
        entity.World = world;
        entity.Id = cached;
        return entity;
      }
    }
  }

  public struct FilterEnumerator
  {

    public FilterEnumerator(Filter afilter, ComponentEnumerator aunder)
    {
      filter = afilter;
      under = aunder;
    }
    internal readonly Filter filter;
    internal ComponentEnumerator under;

    public bool MoveNext()
    {
      while (true)
      {
        if (!under.MoveNext())
          return false;
        if (filter.Satisfy(under.Current))
          return true;
      }
    }

    public Entity Current
    {
      get => under.Current;
    }
  }

  public class Filter
  {

    public Filter(World aworld)
    {
      world = aworld;
    }
    World world;
    List<Type> Included = new();
    List<Type> Excluded = new();

    public Filter Inc<T>()
    {
      Included.Add(typeof(T));
      return this;
    }

    public Filter Exc<T>()
    {
      Excluded.Add(typeof(T));
      return this;
    }

    public bool Satisfy(Entity entity)
    {
      foreach (var typ in Included)
        if (!entity.Has(typ))
          return false;
      foreach (var typ in Excluded)
        if (entity.Has(typ))
          return false;
      return true;
    }

    public FilterEnumerator GetEnumerator()
    {
      var bestcount = int.MaxValue;
      Type? best = null;
      foreach (var v in Included)
      {
        var count = world.CountComponents(v);
        if (count < bestcount)
        {
          bestcount = count;
          best = v;
          if (bestcount == 0)
            return new FilterEnumerator(this, new ComponentEnumerator(world, null));
        }
      }
      return new FilterEnumerator(this, new ComponentEnumerator(world, best));
    }

  }

  public class System
  {
    public System(World aworld)
    {
      world = aworld;
    }

    public World world;
    public virtual void Init()
    {

    }
    public virtual void PreProcess()
    {

    }
    public virtual void Execute()
    {

    }
    public virtual void Teardown()
    {

    }
    public virtual Filter? Filter(World world)
    {
      return null;
    }

    public virtual void Process(Entity e)
    {
    }
  }


  public class Systems : System
  {
    public Systems(World aworld) : base(aworld) { }
    List<System> children = new();
    List<Filter?> filters = new();

    public override void Init()
    {
      foreach (var child in children)
        child.Init();
      foreach (var child in children)
        filters.Add(child.Filter(world));

    }
    public override void Execute()
    {
      foreach (var child in children)
        child.PreProcess();
      int i = 0;
      foreach (var child in children)
      {
        var fltx = filters[i];
        if (fltx is Filter flt)
          foreach (var entity in flt)
            child.Process(entity);
        i += 1;
      }
      foreach (var child in children)
        child.Execute();
    }
    public override void Teardown()
    {
      foreach (var child in children)
        child.Teardown();
    }

    public void Add(System sys)
    {
      children.Add(sys);
    }

    public void DelHere<T>() where T : struct
    {
      children.Add(new RemoveComponents<T>(world));
    }

  }

  internal class RemoveComponents<T> : System where T : struct
  {

    internal RemoveComponents(World aworld) : base(aworld)
    {
    }

    public override void Execute()
    {
      world.GetStorage<T>().Clear();
    }
  }
}