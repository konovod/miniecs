using UnityEngine;
using ECS;
using System;

// ECS side of the link
namespace UnityECSLink
{

  public struct LinkedGameObject
  {
    public GameObject Obj;
    public LinkedGameObject(GameObject obj)
    {
      Obj = obj;
    }
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
        var obj = GameObject.Instantiate(info.Template, info.pos, info.rot);
        ent.Remove<InstantiateGameObject>();
        ent.Add(new LinkedGameObject(obj));
        var ReverseLink = obj.AddComponent<LinkedEntity>();
        ReverseLink.entity = ent;
      }
    }
  }

  public interface IComponentProvider
  {
    public void ProvideComponent(ECS.Entity entity);

  }

  public abstract class ComponentProvider<T> : MonoBehaviour, IComponentProvider where T : struct
  {
    [SerializeField]
    protected T value;

    public void ProvideComponent(ECS.Entity entity)
    {
      entity.Add(value);
      Destroy(this);
    }

    void Awake()
    {
      gameObject.GetComponent<CreateECSEntity>().providers.Add(this);
    }

  }
}


