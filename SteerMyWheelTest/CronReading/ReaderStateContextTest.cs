using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SteerMyWheel.Core.Model.CronReading;
using SteerMyWheel.Core.Model.Entities;
using SteerMyWheel.Infrastracture.Connectivity.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    }
}
