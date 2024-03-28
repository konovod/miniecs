using System.ComponentModel;
using System.Data.Common;
using System.Runtime.InteropServices;

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

  internal Dictionary<Type, IPool> pools = new();

  internal Pool<T> GetStorage<T>() where T : struct
  {
    if (!pools.TryGetValue(typeof(T), out IPool pool))
    {
      pool = new Pool<T>();
      pools.Add(typeof(T), pool);
    }
    return pool as Pool<T>;
  }

}
internal interface IPool
{
  int Count();
  int Find(int id);
  bool Has(int id);
  void Remove(int id);
  int Add(int id);
}
internal class Pool<T> : IPool where T : struct
{
  public int Count() { return Items.Count; }
  public List<T> Items = new(128);
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

}