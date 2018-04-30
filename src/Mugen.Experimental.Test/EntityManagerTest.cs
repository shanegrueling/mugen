﻿namespace Mugen.Experimental.Test
{
    using System;
    using System.Runtime.InteropServices;
    using Abstraction;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class EntityManagerTest
    {
        private struct Test : IComponent
        {
            public int Int;
        }

        private struct Test2 : IComponent
        {
            public float Float;
        }

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

            manager.AddComponent(entity, new Test { Int = 1806});

            Assert.IsNotNull(entity);
            Assert.IsTrue(manager.Exist(entity));
            Assert.AreEqual(1806, manager.GetComponent<Test>(entity).Int);
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

        private unsafe struct Chunk
        {
            public int Value;
            public fixed byte Buffer[1];
        }

        [TestMethod]
        public unsafe void TestMethod()
        {
            var chunk = (Chunk*) Marshal.AllocHGlobal(sizeof(Chunk) + (sizeof(Test) + sizeof(Test2)) * 1024);

            var testSpan = new Span<Test>(chunk->Buffer, 1024);

            var test2Span = new Span<Test2>(chunk->Buffer + sizeof(Test)*1024, 1024);

            for (var i = 0; i < 1024; ++i)
            {
                testSpan[i] = new Test { Int = i};
                test2Span[i] = new Test2 { Float = i };
            }

            for (var i = 0; i < 1024; ++i)
            {
                Assert.AreEqual(i, testSpan[i].Int);
                Assert.AreEqual(i, test2Span[i].Float, float.Epsilon);
            }
        }
    }
}