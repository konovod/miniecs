using System.Data.Common;

namespace miniecs;


public struct Entity
{
  internal World World;
  internal int Id;

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
}
