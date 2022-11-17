using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteerMyWheel.Domain.Discovery.CronParsing;
using System.Xml.Linq;

namespace SteerMyWheelTest
{
    [TestClass]
    public class CronParserTest
    {
        #region GetName
        [TestMethod]
        public void GetName_Should_Return_Name_Of_Script_Executable_CRON_EXECUTABLE()
        {
            //CASE : CRON + EXECUTABLE PATH
            var name = CronParser.GetName("45 09 29 * * /home/kch-front/scripts/misc1/bin/CheckUllinkUsers/getUllinkUserReport.sh > /home/kch-front/scripts/misc1/bin/CheckUllinkUsers/log/GetUllinkReport_$(date +\\%Y\\%m\\%d).log");
            Assert.AreEqual(name, "getUllinkUserReport.sh");
           
            
        }
        [TestMethod]
        public void GetName_Should_Return_Name_Of_Script_Executable_CRON_EXECUTABLE_STDO_REDIRECT()
        {
            //CASE CRON + EXECUTABLE PATH + STDO REDIRECT
            var name = CronParser.GetName("40 06 * * 2-6 /home/kch-front/scripts/ul_iris/bin/irisChangeAudit > /home/kch-front/scripts/ul_iris/log/irisChangeAudit_script.log 2>&1");
            Assert.AreEqual(name, "irisChangeAudit");
        }
        [TestMethod]
        public void GetName_Should_Return_Name_Of_Script_Executable_CRON_SH_LAUNCHING_SCRIPT_EXECUTABLE_PATH()
        {
            //CASE CRON + SH_LAUNCHING_SCRIPT + EXECUTABLE PATH
            var name = CronParser.GetName("00 22 * * 1-5 /home/kch-front/scripts/common/ext/launch_script_with_env.sh /home/kch-front/scripts/ZipBackupedLogs/ZipBackupedLogsLauncher.py");
            Assert.AreEqual(name, "ZipBackupedLogsLauncher.py");
        }
        [TestMethod]
        public void GetName_Should_Return_Name_Of_Script_Executable_CRON_SH_LAUNCHING_SCRIPT_EXECUTABLE_PATH_STDO_REDIRECT()
        {
            //CASE CRON + SH_LAUNCHING_SCRIPT + EXECUTABLE PATH + STDO REDIRECT
            var name = CronParser.GetName("00 02 * * 2-6 /home/kch-front/scripts/common/ext/launch_script_with_env.sh /home/kch-front/scripts/Backupfront/bin/backupfront.py -b > /home/kch-front/scripts/MorningCheckScripts/crontab/backupfront_$(date +\\%Y\\%m\\%d).log 2>&1");
            Assert.AreEqual(name, "backupfront.py -b");
        }
        [TestMethod]
        public void GetName_Should_Return_Name_Of_Script_Executable_CD_AND_EXECUTABLE_PATH()
        {
            //CASE CD && EXECUTABLE_PATH
            var name = CronParser.GetName("40 11 * * 1-5 cd /home/kch-front/scripts/market-trade-export && ./bin/market-trade-export-morning.sh");
            Assert.AreEqual(name, "market-trade-export-morning.sh");
        }

        [TestMethod]
        public void GetName_Should_Return_Name_Of_Script_Executable_Java()
        {
            //CASE JAVA
            var name = CronParser.GetName("15 08 * * 1-5  cd /home/kch-front/scripts/quanthouse-security-enricher && java -jar -DenvTarget=prod -Xms1g -Xmx6g quanthouse-security-enricher-1.1.5.jar");
            Assert.AreEqual(name, "quanthouse-security-enricher-1.1.5.jar");
        }
        #endregion
        #region GetPath
        [TestMethod]
        public void Get_Path_Should_Return_Path_Java()
        {
            var line = "50 06 * * 1-5  cd /home/kch-front/scripts/quanthouse-security-enricher && java -jar -DenvTarget=prod -Xms1g -Xmx6g quanthouse-security-enricher-1.1.5.jar";
            Assert.AreEqual(CronParser.GetPath(line), "/home/kch-front/scripts/quanthouse-security-enricher");
        }
        [TestMethod]
        public void Get_Path_Should_Return_Path_Simple()
        {
            var line = "*/4 08-17 * * 1-5 /home/kch-front/scripts/IDBPostTradeReport/bin/postTradeReport.pl";
            Assert.AreEqual(CronParser.GetPath(line), "/home/kch-front/scripts/IDBPostTradeReport/bin/");
        }
        [TestMethod]
        public void Get_Path_Should_Return_Path_CD_AND_Statement()
        {
            var line = "40 11 * * 1-5  cd /home/kch-front/scripts/market-trade-export && ./bin/market-trade-export-morning.sh";
            Assert.AreEqual(CronParser.GetPath(line), "/home/kch-front/scripts/market-trade-export");
        }
        [TestMethod]
        public void Get_Path_Should_Return_Path_STDO_Redirect()
        {
            var line = "15 07 * * 1-5 /home/kch-front/scripts/MorningCheckScripts/IRIS/bin/irisMorningCheck.pl > /home/kch-front/scripts/MorningCheckScripts/IRIS/log/run_irisMorningCheck.log 2>&1";
            Assert.AreEqual(CronParser.GetPath(line), "/home/kch-front/scripts/MorningCheckScripts/IRIS/bin/");
        }
        [TestMethod]
        public void Get_Path_Should_Return_Path_Double_STDO_Redirect()
        {
            var line = "30 00 * * 1-5 /home/kch-front/tools/Log_Mgt/move_logs >> /home/kch-front/tools/Log_Mgt/log/move_log_$(date +\\%Y\\%m\\%d).log 2>&1";
            Assert.AreEqual(CronParser.GetPath(line), "/home/kch-front/tools/Log_Mgt/move_logs");
        }
        [TestMethod]
        public void Get_Path_Should_Return_Path_Launching_Sh_STDO_Redirect()
        {
            var line = "00 05 * * 2-6 /home/kch-front/scripts/common/ext/launch_script_with_env.sh /home/kch-front/scripts/BackupDiamond/BackupDiamondLauncher.py > /home/kch-front/scripts/BackupDiamond/log/BackupDiamond_$(date +\\%Y\\%m\\%d).log";
            Assert.AreEqual(CronParser.GetPath(line), "/home/kch-front/scripts/BackupDiamond/");
        }
        #endregion
        #region GetCron
        [TestMethod]
        public void Get_Cron_Should_Return_Cron_Expression()
        {
            var line = "#30 09 * * 1-5  cd /home/kch-front/scripts/quanthouse-security-enricher && java -jar -DenvTarget=prod -Xms1g -Xmx6g quanthouse-security-enricher-2.0.0_FOR_TEST.jar";
            Assert.AreEqual(CronParser.GetCron(line), "30 09 * * 1-5");

        }
        #endregion
        [TestMethod]
        public void Get_Role_Should_Return_Role()
        {
            var line = "#Ansible: Cash equity trade report recap";
            Assert.AreEqual(CronParser.GetRole(line), "Ansible: Cash equity trade report recap");
        }
        [TestMethod]
        public void Get_ExecCommand_Should_Return_Script_Line_Without_Cron_Expression()
        {
            var line = "05 08 * * 1-5 /home/kch-front/scripts/common/ext/launch_script_with_env.sh /home/kch-front/scripts/RetailDuplicatedStreamOrders/RetailDuplicatedStreamOrdersLauncher.py";
            Assert.AreEqual(CronParser.GetExecCommand(line), "/home/kch-front/scripts/common/ext/launch_script_with_env.sh /home/kch-front/scripts/RetailDuplicatedStreamOrders/RetailDuplicatedStreamOrdersLauncher.py");
        }
    }
}
