using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteerMyWheel.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SteerMyWheelTest.CronReading
{
    [TestClass]
    public class ParserConfigTest
    {

        [TestMethod]
        public void Get_Repository_Name_Should_Return_Name_Java()
        {
            var line = "/home/kch-front/scripts/quanthouse-security-enricher";
            var result = Regex.Match(line, ParserConfig.RepositoryName).ToString();
            Assert.AreEqual("quanthouse-security-enricher", result);
        }
        #region IsRole
        [TestMethod]
        public void IsRole_Should_Return_True_If_Its_a_role_line()
        {
            var line = "#Ansible: Rapprochement carnet retail quotidien";
            var result = ParserConfig.IsRole(line);
            Assert.IsTrue(result);
        }
        [TestMethod]
        public void IsRole_Should_Return_False_If_Its_not_a_role_line()
        {
            var line = "30 00 * * 1-5 /home/kch-front/tools/Log_Mgt/move_logs >> /home/kch-front/tools/Log_Mgt/log/move_log_$(date +\\%Y\\%m\\%d).log 2>&1";
            var result = ParserConfig.IsRole(line);
            Assert.IsFalse(result);
            line = "#30 00 * * 1-5 /home/kch-front/tools/Log_Mgt/move_logs >> /home/kch-front/tools/Log_Mgt/log/move_log_$(date +\\%Y\\%m\\%d).log 2>&1";
            result = ParserConfig.IsRole(line);
            Assert.IsFalse(result);
        }
        #endregion
        #region IsScript
        [TestMethod]
        public void IsScript_Should_Return_True_If_Its_A_Script_line()
        {
            var line = "30 23 * * 1-5 /home/kch-front/scripts/auditSelector/bin/auditSelector";
            Assert.IsTrue(ParserConfig.IsScript(line));
            line = "#30 23 * * 1-5 /home/kch-front/scripts/auditSelector/bin/auditSelector";
            Assert.IsTrue(ParserConfig.IsScript(line));
        }
        [TestMethod]
        public void Is_Script_Should_Return_False_If_Its_Not_A_Script_line()
        {
            var line = "#Ansible: Rapprochement carnet retail quotidien";
            Assert.IsFalse(ParserConfig.IsScript(line));
        }
        #endregion
        #region IsEnabled
        //Enabled => line doesn't start with #
        [TestMethod]
        public void Is_Enabled_Should_Return_True_If_Script_Line_Is_Enabled()
        {
            var line = "06 06 * * 1-5 /home/kch-front/scripts/ul_iris/bin/irisDailyConfig -recovery";
            Assert.IsTrue(ParserConfig.IsEnabled(line));
        }
        [TestMethod]
        public void Is_Enabled_Should_Return_False_If_Script_Line_Is_Disabled()
        {
            var line = "#06 06 * * 1-5 /home/kch-front/scripts/ul_iris/bin/irisDailyConfig -recovery";
            Assert.IsFalse(ParserConfig.IsEnabled(line));
        }
        #endregion
    }
}
