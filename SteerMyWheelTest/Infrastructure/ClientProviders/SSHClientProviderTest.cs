using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Neo4jClient.Cypher;
using Renci.SshNet;
using SteerMyWheel.Configuration;
using SteerMyWheel.Core.Model.Entities;
using SteerMyWheel.Infrastracture.Connectivity.ClientProviders;
using SteerMyWheel.Infrastracture.Connectivity.ClientProviders.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SteerMyWheelTest.Infrastructure.ClientProviders
{

    [TestClass]
    public class SSHClientProviderTest
    {
        
        private readonly RemoteHost _remoteHost = new RemoteHost("UATFRTAPP901","UATFRTAPP901",22,"kch-front","Supervision!");
        [TestMethod]
        [ExpectedException(typeof(SSHClientNotConnectedException))]
        public void GetConnection_Before_Connecting_Should_Raise_SSHClientNotConnectedException()
        {
            var logger = new LoggerFactory().CreateLogger<SSHClientProvider>();
            var _client = new SSHClientProvider(new Mock<GlobalConfig>().Object, logger);
            var result = _client.GetConnection();
            Assert.IsTrue(result.GetType() == typeof(SshClient));
        }

        [TestMethod]    
        public void GetConnection_After_Connecting_SSH_Should_Return_a_connected_SshClient()
        {

            var logger = new LoggerFactory().CreateLogger<SSHClientProvider>();
            var _client = new SSHClientProvider(new Mock<GlobalConfig>().Object, logger);
            _client.ConnectSSH(_remoteHost);
            var result = _client.GetConnection();
            Assert.IsTrue(result.IsConnected);
            Assert.IsTrue(result.GetType() == typeof(SshClient));

        }

        //TODO : ADD TESTS FOR DOWNLOAD, UPLOAD, COMMAND EXECUTION...
 
    }
}
