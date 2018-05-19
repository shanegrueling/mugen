namespace Mugen.Test
{
    using System.Collections.Generic;
    using Abstraction;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class EntityManagerTest
    {
        [TestMethod]
        public void CreateBlueprint()
        {
            var manager = new EntityManager();

            var blueprint = manager.CreateBlueprint(typeof(Test));

            Assert.IsNotNull(blueprint);
        }

        [TestMethod]
        public void CreateMultipleBlueprints()
        {
            var manager = new EntityManager();

            var blueprint = manager.CreateBlueprint(typeof(Test));
            var blueprint2 = manager.CreateBlueprint(typeof(Test2));

            Assert.IsNotNull(blueprint);
            Assert.IsNotNull(blueprint2);
            Assert.AreNotEqual(blueprint2, blueprint);
        }

        [TestMethod]
        public void CreateAndGetBlueprintWithDifferentOrder()
        {
            var manager = new EntityManager();

            var blueprint = manager.CreateBlueprint(typeof(Test), typeof(Test2));
            var blueprint2 = manager.CreateBlueprint(typeof(Test2), typeof(Test));

            Assert.IsNotNull(blueprint);
            Assert.IsNotNull(blueprint2);
            Assert.AreEqual(blueprint2, blueprint);
        }

        [TestMethod]
        public void CreateEntityWithBlueprint()
        {
            var manager = new EntityManager();

            var blueprint = manager.CreateBlueprint(typeof(Test), typeof(Test2));

            var entity = manager.CreateEntity(blueprint);

            Assert.IsNotNull(entity);
            Assert.IsTrue(manager.Exist(entity));
        }

        [DataTestMethod]
        [DataRow(16)]
        [DataRow(128)]
        [DataRow(512)]
        [DataRow(1024)]
        [DataRow(2098)]
        public void CreateMultipleEntitiesWithBlueprint(int count)
        {
            var manager = new EntityManager();

            var blueprint = manager.CreateBlueprint(typeof(Test), typeof(Test2));

            for (var i = 0; i < count; ++i)
            {
                var entity = manager.CreateEntity(blueprint);

                Assert.IsNotNull(entity);
                Assert.IsTrue(manager.Exist(entity));
            }
        }

        [TestMethod]
        public void CreateEntityAndAddComponent()
        {
            var manager = new EntityManager();
            var entity = manager.CreateEntity();

            manager.AddComponent(entity, new Test {Int = 1806});

            Assert.IsNotNull(entity);
            Assert.IsTrue(manager.Exist(entity));
            Assert.AreEqual(1806, manager.GetComponent<Test>(entity).Int);
        }

        [DataTestMethod]
        [DataRow(16)]
        [DataRow(128)]
        [DataRow(512)]
        [DataRow(1024)]
        [DataRow(2098)]
        public void CreateEntityWithBlueprintAndAddComponent(int count)
        {
            var manager = new EntityManager();
            var blueprint = manager.CreateBlueprint(typeof(Test));

            for (var i = 0; i < count; ++i)
            {
                var entity = manager.CreateEntity(blueprint);

                manager.AddComponent(entity, new Test2 {Float = 18.06f});

                Assert.IsNotNull(entity);
                Assert.IsTrue(manager.Exist(entity));
                Assert.AreEqual(18.06f, manager.GetComponent<Test2>(entity).Float, float.Epsilon);
            }
        }

        [TestMethod]
        public void GetComponentSetAndGetAgain()
        {
            var manager = new EntityManager();

            var blueprint = manager.CreateBlueprint(typeof(Test), typeof(Test2));

            var entity = manager.CreateEntity(blueprint);

            ref var comp = ref manager.GetComponent<Test>(entity);
            comp.Int = 1806;

            ref var comp2 = ref manager.GetComponent<Test>(entity);

            Assert.AreEqual(1806, comp2.Int);
        }

        [DataTestMethod]
        [DataRow(16)]
        [DataRow(128)]
        [DataRow(512)]
        [DataRow(1024)]
        [DataRow(2098)]
        public void CreateEntitWithBlueprintAndRemoveComponent(int count)
        {
            var manager = new EntityManager();
            var blueprint = manager.CreateBlueprint(typeof(Test), typeof(Test2));

            for (var i = 0; i < count; ++i)
            {
                var entity = manager.CreateEntity(blueprint);

                manager.RemoveComponent<Test2>(entity);

                Assert.IsNotNull(entity);
                Assert.IsTrue(manager.Exist(entity));
                Assert.IsFalse(manager.HasComponent<Test2>(entity));
            }
        }

        [TestMethod]
        public void CreateEntityWithDataAndDeleteAComponent()
        {
            var manager = new EntityManager();
            var blueprint = manager.CreateBlueprint(typeof(Test), typeof(Test2));
            var blueprint2 = manager.CreateBlueprint(typeof(Test));

            var entity = manager.CreateEntity(blueprint);
            manager.SetComponent(entity, new Test {Int = 1806});

            manager.RemoveComponent<Test2>(entity);

            Assert.AreEqual(1806, manager.GetComponent<Test>(entity).Int);
        }

        [TestMethod]
        public void CreateEntityWithDataAndAddAComponent()
        {
            var manager = new EntityManager();
            var blueprint = manager.CreateBlueprint(typeof(Test), typeof(Test2));
            var blueprint2 = manager.CreateBlueprint(typeof(Test));

            var entity = manager.CreateEntity(blueprint2);
            manager.SetComponent(entity, new Test {Int = 1806});

            manager.AddComponent<Test2>(entity);

            Assert.AreEqual(1806, manager.GetComponent<Test>(entity).Int);
        }

        [TestMethod]
        public void CreateEntityWithBlueprintAndFindInMatcher()
        {
            var manager = new EntityManager();
            var blueprint = manager.CreateBlueprint(typeof(Test), typeof(Test2), typeof(Test3));
            var blueprint2 = manager.CreateBlueprint(typeof(Test));
            var blueprint3 = manager.CreateBlueprint(typeof(Test), typeof(Test3));

            var m0 = manager.GetMatcher(typeof(Test), typeof(Test2));
            var m1 = manager.GetMatcher(typeof(Test));
            var m2 = manager.GetMatcher(typeof(Test2));
            var m3 = manager.GetMatcher(typeof(Test), typeof(Test3));

            var entity = manager.CreateEntity(blueprint);
            manager.SetComponent(entity, new Test {Int = 1806});

            Assert.AreEqual(1, m0.Length);
            Assert.AreEqual(1806, m0.GetComponentArray<Test>()[0].Int);
            Assert.AreEqual(1, m1.Length);
            Assert.AreEqual(1806, m1.GetComponentArray<Test>()[0].Int);
            Assert.AreEqual(1, m2.Length);
            Assert.AreEqual(1, m3.Length);
            Assert.AreEqual(1806, m3.GetComponentArray<Test>()[0].Int);
        }

        [TestMethod]
        public void CreateEntityWithBlueprintAndFindInMatcherAfterAddingComponent()
        {
            var manager = new EntityManager();
            var blueprint = manager.CreateBlueprint(typeof(Test), typeof(Test2), typeof(Test3));
            var blueprint2 = manager.CreateBlueprint(typeof(Test));
            var blueprint3 = manager.CreateBlueprint(typeof(Test), typeof(Test3));

            var m0 = manager.GetMatcher(typeof(Test), typeof(Test2));
            var m1 = manager.GetMatcher(typeof(Test));
            var m2 = manager.GetMatcher(typeof(Test2));
            var m3 = manager.GetMatcher(typeof(Test), typeof(Test3));

            var entity = manager.CreateEntity(blueprint2);
            manager.SetComponent(entity, new Test {Int = 1806});
            manager.AddComponent<Test3>(entity);

            Assert.AreEqual(0, m0.Length);
            Assert.AreEqual(1, m1.Length);
            Assert.AreEqual(1806, m1.GetComponentArray<Test>()[0].Int);
            Assert.AreEqual(0, m2.Length);
            Assert.AreEqual(1, m3.Length);
        }

        [TestMethod]
        public void CreateEntityWithBlueprintAndFindInMatcherAfterRemovingComponent()
        {
            var manager = new EntityManager();
            var blueprint = manager.CreateBlueprint(typeof(Test), typeof(Test2), typeof(Test3));
            var blueprint2 = manager.CreateBlueprint(typeof(Test));
            var blueprint3 = manager.CreateBlueprint(typeof(Test), typeof(Test3));

            var m0 = manager.GetMatcher(typeof(Test), typeof(Test2));
            var m1 = manager.GetMatcher(typeof(Test));
            var m2 = manager.GetMatcher(typeof(Test2));
            var m3 = manager.GetMatcher(typeof(Test), typeof(Test3));

            var entity = manager.CreateEntity(blueprint);
            manager.SetComponent(entity, new Test {Int = 1806});
            manager.RemoveComponent<Test2>(entity);

            Assert.AreEqual(0, m0.Length);
            //Assert.AreEqual(1806, m1.GetComponentArray<Test>()[0].Int);
            Assert.AreEqual(1, m1.Length);
            Assert.AreEqual(1806, m1.GetComponentArray<Test>()[0].Int);
            Assert.AreEqual(0, m2.Length);
            Assert.AreEqual(1, m3.Length);
            Assert.AreEqual(1806, m3.GetComponentArray<Test>()[0].Int);
        }

        [DataTestMethod]
        //[DataRow(16)]
        //[DataRow(128)]
        //[DataRow(512)]
        //[DataRow(1024)]
        [DataRow(2098)]
        public void CreateEntityAndAddComponentWithBuffer(int count)
        {
            var manager = new EntityManager();
            var blueprint = manager.CreateBlueprint(typeof(Test), typeof(Test2));

            var matcher = manager.GetMatcher(typeof(Test), typeof(Test2));
            var entities = new List<Entity>(count);
            for (var i = 0; i < count; ++i)
            {
                entities.Add(manager.CreateEntity(blueprint));
                manager.SetComponent(entities[i], new Test() { Int = 1806 });
            }

            var buffer = manager.CreateCommandBuffer<int>();

            for (var i = 0; i < count; ++i)
            {
                buffer.AddComponent<Test3>(matcher.GetEntityArray()[i]);
            }
            buffer.Playback();

            for (var i = 0; i < count; ++i)
            {
                Assert.AreEqual(1806, matcher.GetComponentArray<Test>()[i].Int, $"A {i}");
            }

            for (var i = 0; i < count; ++i)
            {
                buffer.RemoveComponent<Test3>(matcher.GetEntityArray()[i]);
            }
            buffer.Playback();

            for (var i = 0; i < count; ++i)
            {
                Assert.AreEqual(1806, matcher.GetComponentArray<Test>()[i].Int, $"R {i}");
            }
            for (var i = 0; i < count; ++i)
            {
                buffer.AddComponent<Test3>(matcher.GetEntityArray()[i]);
            }
            buffer.Playback();

            for (var i = 0; i < count; ++i)
            {
                Assert.AreEqual(1806, matcher.GetComponentArray<Test>()[i].Int, $"A2 {i}");
            }
        }

        public struct Test : IComponent
        {
            public int Int;
        }

        public struct Test2 : IComponent
        {
            public float Float;
        }

        public struct Test3 : IComponent
        {
            public int Int;
        }
    }
}