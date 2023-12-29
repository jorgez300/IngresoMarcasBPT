using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IngresoMarcasBPT
{
    public class RegistrosEtruck
    {
        public string RECEP_ID { get; set; } = string.Empty;
        public string FECHA_RECEP { get; set; } = string.Empty;
        public string PATENTE { get; set; } = string.Empty;
        public string PERIODO { get; set; } = string.Empty;
        public string FECHATARA { get; set; } = string.Empty;
        public string TARA { get; set; } = string.Empty;
        public string FECHABRUTO { get; set; } = string.Empty;
        public string BRUTO { get; set; } = string.Empty;
        public string NETO { get; set; } = string.Empty;
    }


    public class MetodosEtruck
    {
        public readonly OracleConnection connection = new OracleConnection(PRODUCTIVO);

        private OracleCommand cmd = new OracleCommand();

        private DataTable DT = new DataTable();




        private static string QUERY_OBTIENE_REGISTROS = @"SELECT 
            rcp.recep_id,
            fecha_recep,
            rcp.patente,
            PES.Patente,
            PES.Periodo,
            to_char(PES.FechaTara, 'YYYY-MM-DD HH24:MI:SS') FechaTara,
            PES.Tara,
            to_char(PES.FechaBruto, 'YYYY-MM-DD HH24:MI:SS') FechaBruto,
            PES.Bruto,
            PES.Neto
            FROM 
            CTAC.CTAC_RECEP RCP,
            (
                select 
                    trim(vhcpat) Patente,
                    TO_CHAR(pesfecing , 'DD-MM-YYYY') Periodo,
                    pesfecing FechaTara,
                    pesvhctar Tara,
                    pesejefec FechaBruto,
                    pestotcombru Bruto,
                    pesvhccomnet Neto
                from 
                UETRUCK30.PESAJE@DBL_ETRUCK_SF
                where 
                TO_CHAR(pesfecing , 'DD-MM-YYYY') = '@FECHA@' and
                pestotcombru <> 0 and pesvhctar <> 0 and pesvhccomnet <> 0
            ) PES
            WHERE 
            RCP.PATENTE = PES.Patente AND
            TO_CHAR(RCP.FECHA_RECEP , 'DD-MM-YYYY') = PES.Periodo AND
            RCP.ESTADO_RECEP = 'C' AND
            RCP.PLANTA_ID = 102723 AND
            TO_CHAR(RCP.FECHA_RECEP , 'DD-MM-YYYY') = '@FECHA@'
        ";

        private const string QUERY_ACTUALIZA_PESOS = @"DELETE from ctac.ctac_registros where 
            user_registro in ('REGISTRO_MANUAL_NETO', 'REGISTRO_MANUAL_TARA', 'REGISTRO_MANUAL_SALIDA') and
            to_char(hora, 'DD-MM-YYYY') = '@FECHA@'
        ";
        
        private const string QUERY_LIMPIA_REGISTROS = @"update ctac.ctac_recep a set
            a.et_peso_tara = @TARA@,
            a.et_peso_bruto = @BRUTO@,
            a.et_peso_neto = @NETO@
            where a.recep_id = @RECEP_ID@
        ";

        private const string QUERY_REGISTRO_TARA = @"INSERT INTO ctac.ctac_registros 
            (
                HORA,
                RECEP_ID,
                PUNTO_ID_ORIGEN,
                PUNTO_ID_DESTINO,
                PATENTE,
                USER_REGISTRO,
                FECHA_REGISTRO
            ) 
            VALUES
            (
                TO_DATE('@FECHA@', 'YYYY-MM-DD HH24:MI:SS'),
                @RECEP_ID@,
                'ROMENTCEL',
                'BODCELI',
                '@PATENTE@',
                'REGISTRO_MANUAL_TARA',
                TO_DATE('@FECHA@', 'YYYY-MM-DD HH24:MI:SS')
            )
        ";

        private const string QUERY_REGISTRO_NETO = @"INSERT INTO ctac.ctac_registros 
            (
                HORA,
                RECEP_ID,
                PUNTO_ID_ORIGEN,
                PUNTO_ID_DESTINO,
                PATENTE,
                USER_REGISTRO,
                FECHA_REGISTRO
            ) 
            VALUES
            (
                TO_DATE('@FECHA@', 'YYYY-MM-DD HH24:MI:SS'),
                @RECEP_ID@,
                'ROMSALCEL',
                'CAMSAL',
                '@PATENTE@',
                'REGISTRO_MANUAL_NETO',
                TO_DATE('@FECHA@', 'YYYY-MM-DD HH24:MI:SS')
            )
        ";

        private const string QUERY_ELIMINA_REGISTRO_SALIDA = @"DELETE FROM CTAC.CTAC_REGISTROS WHERE RECEP_ID = @RECEP_ID@ AND punto_id_destino = 'FP'";

        private const string QUERY_REGISTRO_SALIDA = @"INSERT INTO ctac.ctac_registros 
            (
                HORA,
                RECEP_ID,
                PUNTO_ID_ORIGEN,
                PUNTO_ID_DESTINO,
                PATENTE,
                USER_REGISTRO,
                FECHA_REGISTRO
            ) 
            VALUES
            (
                TO_DATE('@FECHA@', 'YYYY-MM-DD HH24:MI:SS')  + INTERVAL '@MINUTOS@' MINUTE,
                @RECEP_ID@,
                'CAMSAL',
                'FP',
                '@PATENTE@',
                'REGISTRO_MANUAL_SALIDA',
                TO_DATE('@FECHA@', 'YYYY-MM-DD HH24:MI:SS')
            )
        ";

        public List<RegistrosEtruck> ObtieneRegistros(string FECHA)
        {
            List<RegistrosEtruck> data = new();

            try
            {
                cmd = new OracleCommand();
                cmd.Connection = connection;
                cmd.CommandType = CommandType.Text;
                connection.Open();
                cmd.CommandText = QUERY_OBTIENE_REGISTROS.Replace("@FECHA@", FECHA);
                DT = new DataTable();
                OracleDataAdapter DA = new OracleDataAdapter(cmd);

                DA.Fill(DT);


                if (DT.Rows.Count > 0)
                {

                    foreach (DataRow dr in DT.Rows)
                    {
                        data.Add(new RegistrosEtruck
                        {

                            RECEP_ID = dr["RECEP_ID"].ToString(),
                            FECHA_RECEP = dr["FECHA_RECEP"].ToString(),
                            PATENTE = dr["PATENTE"].ToString(),
                            PERIODO = dr["PERIODO"].ToString(),
                            FECHATARA = dr["FECHATARA"].ToString(),
                            TARA = dr["TARA"].ToString(),
                            FECHABRUTO = dr["FECHABRUTO"].ToString(),
                            BRUTO = dr["BRUTO"].ToString(),
                            NETO = dr["NETO"].ToString()

                        });
                    }

                }
            }
            catch (Exception e)
            {

                throw e;
            }
            finally
            {
                connection.Close();
            }

            return data;
        }

        public void LimpiaRegistrosManuales(string FECHA)
        {

            string query = QUERY_LIMPIA_REGISTROS.Replace("@FECHA@", FECHA);

            try
            {

                Console.WriteLine(query);
                cmd = new OracleCommand();
                cmd.Connection = connection;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;

                connection.Open();

                cmd.ExecuteNonQuery();

                connection.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine("Error " + FECHA + ": " + e.Message);
                //throw e;
            }
            finally
            {
                connection.Close();
            }

        }

        public void ActualizaPesos(RegistrosEtruck ITEM)
        {

            string query = QUERY_ACTUALIZA_PESOS.Replace("@TARA@", ITEM.TARA).Replace("@BRUTO@", ITEM.BRUTO).Replace("@NETO@", ITEM.NETO).Replace("@RECEP_ID@", ITEM.RECEP_ID);

            try
            {

                Console.WriteLine(query);
                cmd = new OracleCommand();
                cmd.Connection = connection;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;

                connection.Open();

                cmd.ExecuteNonQuery();

                connection.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine("Error ActualizaPesos " + ITEM.RECEP_ID + ": " + e.Message);
                //throw e;
            }
            finally
            {
                connection.Close();
            }

        }

        public void GeneraRegistroTara(RegistrosEtruck ITEM)
        {

            string query = QUERY_REGISTRO_TARA.Replace("@FECHA@", ITEM.FECHATARA).Replace("@RECEP_ID@", ITEM.RECEP_ID).Replace("@PATENTE@", ITEM.PATENTE);

            try
            {

                Console.WriteLine(query);
                cmd = new OracleCommand();
                cmd.Connection = connection;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;

                connection.Open();

                cmd.ExecuteNonQuery();

                connection.Close();

            }
            catch (Exception e)
            {

                Console.WriteLine("Error GeneraRegistroTara " + ITEM.RECEP_ID + ": " + e.Message);
                //throw e;
            }
            finally
            {
                connection.Close();
            }

        }

        public void GeneraRegistroNeto(RegistrosEtruck ITEM)
        {

            string query = QUERY_REGISTRO_NETO.Replace("@FECHA@", ITEM.FECHABRUTO).Replace("@RECEP_ID@", ITEM.RECEP_ID).Replace("@PATENTE@", ITEM.PATENTE);

            try
            {

                Console.WriteLine(query);
                cmd = new OracleCommand();
                cmd.Connection = connection;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;

                connection.Open();

                cmd.ExecuteNonQuery();

                connection.Close();

            }
            catch (Exception e)
            {

                Console.WriteLine("Error GeneraRegistroNeto " + ITEM.RECEP_ID + ": " + e.Message);
                //throw e;
            }
            finally
            {
                connection.Close();
            }

        }

        public void EliminaRegistroFP(RegistrosEtruck ITEM)
        {

            string query = QUERY_ELIMINA_REGISTRO_SALIDA.Replace("@RECEP_ID@", ITEM.RECEP_ID);

            try
            {

                Console.WriteLine(query);
                cmd = new OracleCommand();
                cmd.Connection = connection;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;

                connection.Open();

                cmd.ExecuteNonQuery();

                connection.Close();

            }
            catch (Exception e)
            {

                Console.WriteLine("Error EliminaRegistroFP " + ITEM.RECEP_ID + ": " + e.Message);
                //throw e;
            }
            finally
            {
                connection.Close();
            }

        }

        public void GeneraRegistroSalida(RegistrosEtruck ITEM)
        {
            Random rnd = new Random();

            int minutos = rnd.Next(15, 50);

            string query = QUERY_REGISTRO_SALIDA.Replace("@FECHA@", ITEM.FECHABRUTO).Replace("@RECEP_ID@", ITEM.RECEP_ID).Replace("@PATENTE@", ITEM.PATENTE).Replace("@MINUTOS@", minutos.ToString());

            try
            {

                Console.WriteLine(query);
                cmd = new OracleCommand();
                cmd.Connection = connection;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;

                connection.Open();

                cmd.ExecuteNonQuery();

                connection.Close();

            }
            catch (Exception e)
            {

                Console.WriteLine("Error GeneraRegistroSalida " + ITEM.RECEP_ID + ": " + e.Message);
                //throw e;
            }
            finally
            {
                connection.Close();
            }

        }


    }


}
