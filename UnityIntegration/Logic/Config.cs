using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityECSLink;

namespace ECSGame
{
  public static class Config
  {
    public static void InitSystems(ECS.World world, ECS.Systems OnUpdate, ECS.Systems OnFixedUpdate)
    {
      ////////////////// add here systems that is called on Update
      OnUpdate.Add(new ExampleSystem(world));

      ////////////////// add here systems that is called on FixedUpdate
      // OnFixedUpdate.Add(new ExampleFixedSystem(world));

    }
    private static void Link<T>(GameObject gameObject, ECS.Entity entity) where T : Component
    {
      if (gameObject.TryGetComponent<T>(out T comp))
        entity.Add(new LinkedComponent<T>(comp));
    }

    public static void LinkComponents(GameObject gameObject, ECS.Entity entity)
    {
      //Add all components that needs to be accessed from ecs world
      Link<NavMeshAgent>(gameObject, entity);
      Link<Renderer>(gameObject, entity);
    }
  }
}