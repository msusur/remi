using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Core;
using Moq;

namespace ReMi.TestUtils.UnitTests
{
    public static class ContainerMockExtensions
    {
        private static Mock<IComponentRegistry> _componentRegistyMock = new Mock<IComponentRegistry>();

        public static void SetupResolveNamed<TEntity>(this Mock<IContainer> containerMock, string name, TEntity obj)
        {
            var componentRegistration = new Mock<IComponentRegistration>().Object;

            containerMock.SetupGet(x => x.ComponentRegistry).Returns(_componentRegistyMock.Object);
            _componentRegistyMock.Setup(x => x.TryGetRegistration(
                    It.Is<KeyedService>(s => s.ServiceType == typeof(TEntity)
                            && s.ServiceKey.ToString() == name),
                        out componentRegistration))
                .Returns(true);
            containerMock.Setup(x => x.ResolveComponent(componentRegistration, It.IsAny<IEnumerable<Parameter>>()))
                .Returns(obj);
        }

        public static void SetupIsRegisteredWithName(this Mock<IContainer> containerMock, string name, Type type, bool isRegistered)
        {
            containerMock.SetupGet(x => x.ComponentRegistry).Returns(_componentRegistyMock.Object);
            _componentRegistyMock.Setup(x => x.IsRegistered(
                    It.Is<KeyedService>(s => s.ServiceType == type
                            && s.ServiceKey.ToString() == name)))
                .Returns(isRegistered);
        }

        public static void SetupResetContainer(this Mock<IContainer> containerMock)
        {
            _componentRegistyMock = new Mock<IComponentRegistry>();
        }
    }
}
