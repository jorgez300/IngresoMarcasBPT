// See https://aka.ms/new-console-template for more information
using IngresoMarcasBPT;

Console.WriteLine("Inicio");


var ARREGLO_FECHAS = "18-12-2023".Split(',');

MetodosEtruck metodos = new MetodosEtruck();

try
{
    foreach (string FECHA in ARREGLO_FECHAS)
    {
        Console.WriteLine(FECHA);
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine();

        metodos.LimpiaRegistrosManuales(FECHA);
        List<RegistrosEtruck> data = metodos.ObtieneRegistros(FECHA);

        foreach (RegistrosEtruck registro in data)
        {
            Console.WriteLine(registro.RECEP_ID);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();

            metodos.ActualizaPesos(registro);
            metodos.GeneraRegistroTara(registro);
            metodos.GeneraRegistroNeto(registro);
            metodos.EliminaRegistroFP(registro);
            metodos.GeneraRegistroSalida(registro);
        }

        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine();

    }


}
catch (Exception e)
{
    Console.WriteLine(e.Message);
}
finally
{
    metodos.connection.Close();
    metodos.connection.Dispose();
    Console.WriteLine("Listo");
    Console.ReadKey();
}



