using Marketplace.Core.Bot.Abstractions;
using Marketplace.Core.Bot.Implementations;
using Marketplace.Core.Bot.Models;
using Marketplace.Core.Models;
using Marketplace.Core.Models.UserStates;
using Moq;

namespace Marketplace.Core.Bot.Tests;

[TestFixture]
public class ControllerCreatingTests
{
    [Test]
    public void TestBuildControllerRegistry()
    {
        // Arrange
        var creationContext = new ControllerContext(new User
            {
                Name = string.Empty
            },
            new DerivedUserState(),
            new Update());
        
        // Act
        var builder = new ControllerRegistryBuilder();
        builder.RegisterControllerFactoryMethod<DerivedUserState>(
            ctx => new Mock<AbstractController<DerivedUserState>>(ctx).Object);
        var factory = builder.Factory;
        
        var controller = factory.CreateController(creationContext);
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(controller, Is.Not.Null);
            Assert.That(controller, Is.AssignableTo<AbstractController<DerivedUserState>>());
        });
    }

    [Test]
    public void TryRegisterControllerForRegisteredState_ThrowsInvalidOperationException()
    {
        // Arrange
        var builder = new ControllerRegistryBuilder();
        builder.RegisterControllerFactoryMethod<DerivedUserState>(
            _ => throw new NotSupportedException());
        // Act
        TestDelegate throws = () => builder.RegisterControllerFactoryMethod<DerivedUserState>(
            _ => throw new NotSupportedException());
        // Assert
        Assert.That(throws, Throws.InvalidOperationException);
    }

    [Test]
    public void TryCreateControllerForNotRegisteredState_ReturnsNull()
    {
        // Arrange
        var builder = new ControllerRegistryBuilder();
        var factory = builder.Factory;
        // Act
        var controller = factory.CreateController(
            new ControllerContext(
                new User { Name = string.Empty },
                new DerivedUserState(),
                new Update()));
        // Assert
        Assert.That(controller, Is.Null);
    }
    
    public sealed class DerivedUserState : UserState
    {
        public override void Reset() => throw new NotSupportedException();
    }
}
