using UnityEngine;
using ECS;


// ECS side of the link
namespace UnityECSLink
{

  public struct LinkedGameObject
  {
    public GameObject Obj;
  };

  public struct DestroyGameObject { };

  public struct InstantiateGameObject
  {
    public GameObject Template;
    public Vector3 pos;
    public Quaternion rot;
  };

  public class ProcessGameObjects : ECS.System
  {
    internal ProcessGameObjects(ECS.World aworld) : base(aworld) { }
    public override void Execute()
    {
      // remove game objects
      foreach (var ent in world.Each<DestroyGameObject>())
      {
        GameObject.Destroy(ent.Get<LinkedGameObject>().Obj);
        ent.Destroy();
      }
      // instantiate game objects
      foreach (var ent in world.Each<InstantiateGameObject>())
      {
        var info = ent.Get<InstantiateGameObject>();
        LinkedGameObject linked;
        linked.Obj = GameObject.Instantiate(info.Template, info.pos, info.rot);
        ent.Remove<InstantiateGameObject>();
        ent.Add(linked);
        var ReverseLink = linked.Obj.AddComponent<LinkedEntity>();
        ReverseLink.entity = ent;
      }
    }
  }
}
