using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityECSLink;

public class ECSWorldContainer : MonoBehaviour
{
    public static ECSWorldContainer Active;
    public ECS.World world = new();
    public ECS.Systems OnUpdate;
    public ECS.Systems OnFixedUpdate;

    void Awake()
    {
        Active = this;
        OnUpdate = new ECS.Systems(world);
        ////////////////// add here systems that is called on Update
        OnUpdate.Add(new ProcessGameObjects(world));
        // OnUpdate.Add(new MySystem(world));

        ///
        OnFixedUpdate = new ECS.Systems(world);
        ////////////////// add here systems that is called on FixedUpdate
        // OnFixedUpdate.Add(new MySystem(world));

        ///

    }

    void Start()
    {
        OnUpdate.Init();
        OnFixedUpdate.Init();
    }

    void Update()
    {
        OnUpdate.Execute();
    }

    void FixedUpdate()
    {
        OnFixedUpdate.Execute();
    }

}
