using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DAO
{
    public class SqlOperation
    {
        public string ProcedureName { get; set; }

        public List<SqlParameter> Parameters { get; set; }

        public SqlOperation()
        {
            Parameters = new List<SqlParameter>();
        }

        public void AddStringParameter(string ParamName, string? ParamValue)
        {
            Parameters.Add(new SqlParameter(ParamName, ParamValue));
        }

        public void AddIntParam(string paramName, int paramValue)
        {
            Parameters.Add(new SqlParameter(paramName, paramValue));
        }

        public void AddDoubleParam(string paramName, double paramValue)
        {
            Parameters.Add(new SqlParameter(paramName, paramValue));
        }

        public void AddDateTimeParam(string paramName, DateTime paramValue)
        {
            Parameters.Add(new SqlParameter(paramName, paramValue));
        }

        public void AddDecimalParameter(string paramName, decimal paramValue)
        {
            Parameters.Add(new SqlParameter(paramName, paramValue));
        }

        public void AddBooleanParam(string paramName, bool paramValue)
        {
            Parameters.Add(new SqlParameter(paramName, paramValue));
        }
    }
}
