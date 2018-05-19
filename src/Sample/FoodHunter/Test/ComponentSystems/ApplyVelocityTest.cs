namespace Sample.FoodHunter.Test.ComponentSystems
{
    using Logic.Components;
    using Logic.ComponentSystems;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Mugen.Math;

    [TestClass]
    public class ApplyVelocityTest
    {
        [TestMethod]
        public void Update()
        {
            var position = new Position {Value = new float2(0, 0)};
            var velocity = new Velocity {Value = new float2(1, 1)};

            ApplyVelocity.Update(0.33f, ref position, ref velocity);

            Assert.AreEqual(0.33f, position.Value.X, float.Epsilon);
            Assert.AreEqual(0.33f, position.Value.Y, float.Epsilon);
        }
    }
}