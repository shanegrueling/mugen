namespace Mugen.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    [TestClass]
    public class EntityCommandBufferTest
    {
        private struct TestComponent : IComponent
        {

        }

        [DataTestMethod]
        [DataRow(1)]
        [DataRow(10)]
        [DataRow(100)]
        [DataRow(1000)]
        public void CreateNewEntityFromBlueprint(int amount)
        {
            var blueprint = new Blueprint(typeof(TestComponent));
            var entityManagerMoq = new Mock<IEntityManager>();

            entityManagerMoq.Setup(manager => manager.CreateEntity(It.Is<Blueprint>(blueprint1 => blueprint1.Equals(blueprint)))).Returns(Entity.Create).Verifiable("No Entity was created.");

            var entityCommandBuffer = new EntityCommandBuffer(entityManagerMoq.Object);

            for (var i = 0; i < amount; ++i)
            {
                entityCommandBuffer.CreateEntity(blueprint);
            }

            entityCommandBuffer.Playback();

            entityManagerMoq.Verify();
        }
    }
}