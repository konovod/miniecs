<a href="https://github.com/konovod/miniecs/actions/workflows/ci.yml">
      <img src="https://github.com/konovod/miniecs/actions/workflows/ci.yml/badge.svg" alt="Build Status">
</a>

# MiniECS

There are plenty of ECS for C# already, this one is just to prove that ecs framework is pretty simple (until you start care for performance).
Not intended for general use, almost no safety checks - take LeoECS, Morpeh, DragonECS, DOTS or any other popular ECS framework.

## Check list:

- [x] 1. `world = new ECS::World()` # Создания "мира" с сущностями
- [x] 2. `entity = world.NewEntity()` # Создание новой сущности entity
- [x] 3. `entity.Add(new Comp1)` # Добавление компонента Comp1 к сущности entity
- [x] 4. `entity.Remove<Comp1>()` # Удаление компонента Comp1 с сущности entity
- [x] 5. `entity.Has<Comp1>()` # Возвращает True если на сущности есть компонент Comp1
- [x] 6. `entity.Get<Comp1>()` # Возвращает значение компонента Comp1 на сущности 
- [x] 7. `entity.GetRef<Comp1>()` # Возвращает ссылку на компонент Comp1 на сущности 
- [x] 8. `foreach(ent in world.Each<Comp1>())` # Итерация по всем сущностям имеющим компонент Comp1
- [x] 9. `foreach(ent in world.EachEntity())` # Итерация по всем сущностям в мире (больше для отладочных целей)
- [x] 10. `filter = world.Filter()` # создает запрос к миру
- [x] 11. `filter.Inc<Comp1>()` # добавляет к запросу условие "включает компонент Comp1"
- [x] 12. `filter.Exc<Comp1>()` # добавляет к запросу условие "не включает компонент Comp1"
- [x] 13. `foreach(ent in filter)` # итерация сущностей удовлетворяющих запросу query
- [x] 14. `entity.Destroy()` # Удаление сущности entity
- [x] 15. `systems = ECS::Systems.new(world)` # Создание группы систем
- [x] 16. `systems.Add(new System1(world))`  # Добавление системы System1 к группе систем
- [x] 17. `systems.Init()` # Вызов Init у всех систем группы, создание фильтров всех систем группы
- [x] 18. `systems.Execute()` # Вызов Execute у всех систем группы, вызов Process у всех систем группы для каждого entity из соотв.фильтра
- [x] 19. `systems.Teardown()` # Вызов Teardown у всех систем группы
- [x] 20. в 8, 13 можно удалять текущий компонент в процессе итерации. Также 8 и 13 могут быть вложены в любом порядке и удаление текущего компонента все равно не должно ломать итерацию.
- [x] 21. `systems.DelHere<Comp1>()` # Система удаляющая все компоненты Comp1
- [x] 22. Сущность удаляется после 4 или 21 если на ней не осталось компонентов

## Дополнительно:
 - В `systems.Statistics` хранится сколько мс заняло выполнение каждой из систем.
 - `world.DeleteAll()` удаляет все сущности
 - `world.EntitiesCount()` возвращает число сущностей в мире
 - `world.CountComponents<Comp1>()` возвращает число компонентов в мире
 - `entity.RemoveIfPresent<Comp1>()` # Удаление компонента Comp1 с сущности entity если он присутствует на ней
 - `entity.Set(new Comp1)` # Добавление или замена существующего компонента Comp1 на сущности entity
 - в `Remove, Has, RemoveIfPresent, CountComponents` можно передавать определяемый в рантайме Type.
 - `world.FirstComponent<Comp1>` и `world.RefFirstComponent<Comp1>` возвращают первый из компонентов Comp1 в мире. удобно для синглтон-компонентов
 - `entity.AddDefault(type)` Добавление компонента типа type известного в рантайме
 - `entity.Alive()` возвращает true если на сущности есть компоненты


## Интеграция с Unity
 - добавить содержимое папки UnityIntegration, добавить miniecs.cs в проект
