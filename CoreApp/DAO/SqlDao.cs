using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DAO
{
    public class SqlDao
    {
        //Paso 1: Crear una instacia privada estática de la clase SqlDao.

        private static SqlDao _instance;

        private string _connectionString;

        //Paso 2: Redefinir el constructor default como privado para evitar que se pueda crear una instancia de la clase desde fuera.

        private SqlDao()
        {
            _connectionString = @"Data Source=srv-sqldatabase-billetico.database.windows.net;Initial Catalog=billetico-db;User ID=sysadmin;Password=BilleTico123!;Trust Server Certificate=True";
        }

        //Paso 3: Definir el metodo que expondrá la instancia de la clase SqlDao.

        public static SqlDao GetInstance()
        {
            //Verificar si la instancia es nula, si lo es, crear una nueva instancia de la clase SqlDao.
            if (_instance == null)
            {
                _instance = new SqlDao();
            }
            //Retornar la instancia de la clase SqlDao.
            return _instance;
        }

        //Metodo para la ejecución de stored procedures sin retorno de datos.

        public void ExecuteProcedure(SqlOperation sqlOperation)
        {
            // Conectarse a la base de datos y ejecutar el stored procedure sin retorno de datos.
            using (var conn = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand(sqlOperation.ProcedureName, conn)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                })
                {
                    //Set de los parametros
                    foreach (var param in sqlOperation.Parameters)
                    {
                        command.Parameters.Add(param);
                    }
                    //Ejectura el SP
                    conn.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        //Metodo para la ejecución de stored procedures con retorno de datos.

        public List<Dictionary<string, object>> ExecuteQueryProcedure(SqlOperation sqlOperation)
        {

            var lstResults = new List<Dictionary<string, object>>();

            using (var conn = new SqlConnection(_connectionString))

            {
                using (var command = new SqlCommand(sqlOperation.ProcedureName, conn)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                })
                {
                    //Set de los parametros
                    foreach (var param in sqlOperation.Parameters)
                    {
                        command.Parameters.Add(param);
                    }
                    //Ejectura el SP
                    conn.Open();

                    //de aca en adelante la implementacion es distinta con respecto al procedure anterior
                    // sentencia que ejectua el SP y captura el resultado
                    var reader = command.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {

                            var rowDict = new Dictionary<string, object>();

                            for (var index = 0; index < reader.FieldCount; index++)
                            {
                                var key = reader.GetName(index);
                                var value = reader.GetValue(index);
                                //aca agregamos los valores al diccionario de esta fila
                                rowDict[key] = value;
                            }
                            lstResults.Add(rowDict);
                        }
                    }

                }
            }

            return lstResults;
        }
    }
}
