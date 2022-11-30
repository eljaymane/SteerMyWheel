using Microsoft.Data.SqlClient;
using System;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Model.Workflows.Abstractions
{
    public abstract class BaseSQLWorkflow : BaseWorkflow
    {
        private SqlClientFactory _client;
        public object[] data;

        public BaseSQLWorkflow(string name, string description, DateTime executionDate, BaseWorkflow next, BaseWorkflow previous) : base(name, description, executionDate, next, previous)
        {
        }
        public SqlConnectionStringBuilder Builder { get; set; }
        public string SQLQuery { get; set; }
        public object[] GetData()
        {
            return data;
        }
        public object[] ExecuteQueryWithResult()
        {
            using (SqlConnection sqlConnection = new SqlConnection(Builder.ConnectionString))
            {
                sqlConnection.OpenAsync().Wait();
                using (SqlCommand command = new SqlCommand(SQLQuery, sqlConnection))
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

        public Task<bool> ExecuteQueryWithoutResult()
        {
            using (SqlConnection sqlConnection = new SqlConnection(Builder.ConnectionString))
            {
                sqlConnection.OpenAsync().Wait();
                using (SqlCommand command = new SqlCommand(SQLQuery, sqlConnection))
                {
                    try
                    {
                        command.ExecuteNonQuery();
                        return Task.FromResult(true);
                    }
                    catch (Exception ex)
                    {
                        return Task.FromResult(false);
                    }
                   ;

                };
            }
        }

        public abstract override bool CanExecute();



        public abstract override Task ExecuteAsync(BaseWorkflowContext context);

    }
}
