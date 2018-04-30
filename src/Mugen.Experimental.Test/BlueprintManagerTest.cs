namespace Mugen.Experimental.Test
{
    using Abstraction;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class BlueprintManagerTest
    {
        private struct Test1 : IComponent
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
            var manager = new BlueprintManager();

            var blueprint = manager.GetOrCreateBlueprint(new[] {new ComponentType(typeof(Test1)),});

            Assert.IsNotNull(blueprint);
        }

        [TestMethod]
        public void CreateMultipleBlueprints()
        {
            var manager = new BlueprintManager();

            var blueprint = manager.GetOrCreateBlueprint(new[] {new ComponentType(typeof(Test1)),});
            var blueprint2 = manager.GetOrCreateBlueprint(new[] {new ComponentType(typeof(Test2)),});

            Assert.IsNotNull(blueprint);
            Assert.IsNotNull(blueprint2);
            Assert.AreNotEqual(blueprint2, blueprint);
        }

        [TestMethod]
        public void CreateAndGetBlueprint()
        {
            var manager = new BlueprintManager();

            var blueprint = manager.GetOrCreateBlueprint(new[] {new ComponentType(typeof(Test1)),});
            var blueprint2 = manager.GetOrCreateBlueprint(new[] {new ComponentType(typeof(Test1)),});

            Assert.IsNotNull(blueprint);
            Assert.IsNotNull(blueprint2);
            Assert.AreEqual(blueprint2, blueprint);
        }
    }
}