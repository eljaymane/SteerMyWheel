using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SteerMyWheel.Core.Model.CronReading;
using SteerMyWheel.Core.Model.Entities;
using SteerMyWheel.Infrastracture.Connectivity.Repositories;
using SteerMyWheel.Core.Model.CronReading.Exceptions;

namespace SteerMyWheelTest.CronReading
{
    [TestClass]
    public class ReaderStateContextTest
    {
        [TestMethod]
        public void Initialize_context_correctly_sets_current_host_name()
        {
            var host = new RemoteHost("test", "test", 22, "test","test");
            var logger = new LoggerFactory().CreateLogger<ReaderStateContext>();
            var remoteHostRepo = new Mock<BaseGraphRepository<RemoteHost,string>>();
            remoteHostRepo.Setup(e => e.Create(host)).Returns(host);
            var dao = new GlobalEntityRepository(remoteHostRepo.Object,null,null);
            var context = new ReaderStateContext(logger,dao);
            context.Initialize(host);
            Assert.AreEqual(host.Name, context.currentHostName);
        }

        [TestMethod]
        public void setting_context_state_correctly_changes_state()
        {
            var host = new RemoteHost("test", "test", 22, "test", "test");
            var logger = new LoggerFactory().CreateLogger<ReaderStateContext>();
            var remoteHostRepo = new Mock<BaseGraphRepository<RemoteHost, string>>();
            remoteHostRepo.Setup(e => e.Create(host)).Returns(host);
            var dao = new GlobalEntityRepository(remoteHostRepo.Object, null, null);
            var context = new ReaderStateContext(logger, dao);
            var state = new NewRoleReaderState("test");
            context.Initialize(host);
            context.setState(state);
            Assert.AreEqual(state, context.currentState);
        }

        [TestMethod]
        [ExpectedException(typeof(ReaderStateContextNotInitializedException))]
        public void should_throw_exception_if_no_host_is_set()
        {
            var host = new RemoteHost("test", "test", 22, "test", "test");
            var logger = new LoggerFactory().CreateLogger<ReaderStateContext>();
            var remoteHostRepo = new Mock<BaseGraphRepository<RemoteHost, string>>();
            remoteHostRepo.Setup(e => e.Create(host)).Returns(host);
            var dao = new GlobalEntityRepository(remoteHostRepo.Object, null, null);
            var context = new ReaderStateContext(logger, dao);
            context.Initialize(host);
            context.currentHostName = "";
            context.setState(new NewRoleReaderState("test"));
        }

    }
}
