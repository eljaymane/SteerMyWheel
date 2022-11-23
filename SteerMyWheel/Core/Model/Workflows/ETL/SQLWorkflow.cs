using SteerMyWheel.Domain.Model.Workflow;
using System;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.Workflows.ETL
{
    public class SQLWorkflow : BaseWorkflow
    {
        private SqlClientFactory _client;
        private object[] data;

        public SQLWorkflow(string name, string description, DateTime executionDate, BaseWorkflow next, BaseWorkflow previous) : base(name, description, executionDate, next, previous)
        {
        }

        public SqlConnectionStringBuilder Builder { get; set; }
        public string SQLQuery { get; set; }
        public object[] GetData()
        {
            return data;
        }
        public object[] QueryData()
        {
            using (SqlConnection sqlConnection = new SqlConnection(Builder.ConnectionString))
            {
                sqlConnection.OpenAsync().Wait();   
                using (SqlCommand command = new SqlCommand(SQLQuery,sqlConnection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            object[] result = new object[10000];
                            reader.GetSqlValues(result);
                            return result;
                        }
                    }
                };
            }
            return null;
        }
        public override bool CanExecute()
        {
            return true;
        }

        public override Task Execute()
        {
            QueryData();
            return Task.CompletedTask;
        }

        public override Task ExecuteAsync()
        {
            throw new NotImplementedException();
        }
    }
}
