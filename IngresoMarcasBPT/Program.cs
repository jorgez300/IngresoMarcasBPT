// See https://aka.ms/new-console-template for more information
using IngresoMarcasBPT;

Console.WriteLine("Inicio");


var ARREGLO_FECHAS = "18-12-2023".Split(',');

MetodosEtruck metodos = new MetodosEtruck();

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
        metodos.ActualizaPesos(registro);
        metodos.GeneraRegistroEntrada(registro);
        metodos.GeneraRegistroSalida(registro);
    }

    Console.WriteLine();
    Console.WriteLine();
    Console.WriteLine();
    Console.ReadKey();

}


metodos.connection.Close();
metodos.connection.Dispose();
Console.WriteLine("Listo");
Console.ReadKey();