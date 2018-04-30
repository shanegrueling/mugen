namespace Sample.FoodHunter.Test.ComponentSystems
{
    using Logic.ComponentSystems;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Mugen.Abstraction;
    using Mugen.Abstraction.CommandBuffers;

    [TestClass]
    public class EatFoodTest
    {
        [TestMethod]
        public void Update()
        {
            var commandBufferMoq = new Mock<IEntityCommandBuffer<EatFood>>();

            var entityManagerMoq = new Mock<IEntityManager>();
            //entityManagerMoq.Setup(manager => manager.CreateCommandBuffer<EatFood>()).Returns(() => commandBufferMoq.Object);

            //var eatFoodSystem = new EatFood(entityManagerMoq.Object);
        }
    }
}