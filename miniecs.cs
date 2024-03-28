using System.ComponentModel;
using System.Data.Common;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace miniecs;


public struct Entity
{
  internal World World;
  internal int Id;

  public Entity Add<T>(in T item) where T : struct
  {
    Pool<T> pool = World.GetStorage<T>();
    int index = pool.Add(Id);
    pool.Items[index] = item;
    return this;
  }

  public readonly bool Has<T>() where T : struct
  {
    return World.GetStorage<T>().Has(Id);
  }

  public readonly bool Has(Type type)
  {
    return World.GetStorage(type).Has(Id);
  }

  public Entity Remove<T>() where T : struct
  {
    World.GetStorage<T>().Remove(Id);
    return this;
  }

  public Entity Set<T>(in T item) where T : struct
  {
    if (Has<T>())
    {
      Pool<T> pool = World.GetStorage<T>();
      int index = pool.IndexByEntity[Id];
      pool.Items[index] = item;
    }
    else
      Add(item);
    return this;
  }

  public readonly T Get<T>() where T : struct
  {
    Pool<T> pool = World.GetStorage<T>();
    int index = pool.Find(Id);
    return pool.Items[index];
  }
  public ref T GetRef<T>() where T : struct
  {
    Pool<T> pool = World.GetStorage<T>();
    int index = pool.Find(Id);
    var span = CollectionsMarshal.AsSpan(pool.Items);
    return ref span[index];
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

  internal Dictionary<Type, IPool> pools = [];

  internal Pool<T> GetStorage<T>() where T : struct
  {
    if (!pools.TryGetValue(typeof(T), out IPool pool))
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

  internal IPool GetStorage(Type type)
  {
    return pools[type];
  }

  public Filter Filter()
  {
    return new Filter(this);
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
internal class Pool<T> : IPool where T : struct
{
  public int Count() { return Items.Count; }
  public List<T> Items = new(128);
  public List<int> Entities = new(128);
  public Dictionary<int, int> IndexByEntity = [];

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
      Items[index] = Items[last];
      Entities[index] = Entities[last];
    }
    Items.RemoveAt(last);
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


}

public readonly struct ComponentEnumerable(World aworld, Type atype)
{
  readonly World world = aworld;
  readonly Type type = atype;

  public ComponentEnumerator GetEnumerator()
  {
    return new ComponentEnumerator(world, type);
  }
}


public struct ComponentEnumerator(World aworld, Type atype)
{
  internal readonly World world = aworld;
  internal readonly IPool pool = aworld.GetStorage(atype);
  int index = 0;
  int cached = -1;

  public bool MoveNext()
  {
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

public struct FilterEnumerator(Filter afilter, ComponentEnumerator aunder)
{
  internal readonly Filter filter = afilter;
  internal ComponentEnumerator under = aunder;

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


public class Filter(World aworld)
{
  World world = aworld;
  List<Type> Included = [];
  List<Type> Excluded = [];

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
    var best = Included.MinBy(v => world.GetStorage(v).Count());
    return new FilterEnumerator(this, new ComponentEnumerator(world, best));
  }

}