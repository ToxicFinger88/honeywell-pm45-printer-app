
using System;
using Honeywell.Printer;

namespace PM45DemoV03
{
    internal class Program
    {
        // Componentes principales
        static UI.Canvas canvas;
        static UI.Canvas.Text estado;
        static PrintControl printer;

        // Control para evitar impresiones simultaneas
        static bool imprimiendo = false;

        static int Main(string[] args)
        {
            try
            {
                Console.WriteLine("================================");
                Console.WriteLine(" HONEYWELL PM45 - DEMO V03");
                Console.WriteLine("================================");

                // Inicializar pantalla
                canvas = new UI.Canvas();

                if (!canvas.HasDisplay)
                {
                    Console.WriteLine("ERROR: Pantalla no disponible");
                    canvas.Dispose();
                    return 1;
                }

                Console.WriteLine("Ancho: " + canvas.Width);
                Console.WriteLine("Alto: " + canvas.Height);
                Console.WriteLine("Pantalla tactil: " + canvas.HasTouch);

                // Inicializar controlador de impresion
                // Se crea UNA SOLA VEZ
                printer = new PrintControl();

                Console.WriteLine(
                    "Controlador de impresion inicializado");

                int ancho = canvas.Width;

                // Colores
                Color fondo = new Color(20, 32, 48, 255);
                Color blanco = new Color(255, 255, 255, 255);
                Color azul = new Color(20, 120, 200, 255);
                Color gris = new Color(85, 100, 115, 255);

                // Fondo
                canvas += new UI.Canvas.Rectangle(
                    0, 0,
                    canvas.Width,
                    canvas.Height,
                    fondo);

                // Titulo
                canvas += new UI.Canvas.Text(
                    20, 30,
                    "PM45 - DEMO",
                    "Univers",
                    22,
                    blanco);

                // Estado
                estado = new UI.Canvas.Text(
                    20, 85,
                    "Programa iniciado",
                    "Univers",
                    16,
                    blanco);

                canvas += estado;

                // ===================================
                // BOTON IMPRIMIR
                // ===================================

                UI.Canvas.Rectangle botonImprimir =
                    new UI.Canvas.Rectangle(
                        20, 145,
                        ancho - 40,
                        55,
                        azul);

                botonImprimir.MouseDown += PulsarImprimir;

                canvas += botonImprimir;

                // Texto visual, sin evento tactil
                UI.Canvas.Text textoImprimir =
                    new UI.Canvas.Text(
                        65, 160,
                        "IMPRIMIR",
                        "Univers",
                        20,
                        blanco);

                canvas += textoImprimir;

                // ===================================
                // BOTON SALIR
                // ===================================

                UI.Canvas.Rectangle botonSalir =
                    new UI.Canvas.Rectangle(
                        20, 225,
                        ancho - 40,
                        55,
                        gris);

                botonSalir.MouseDown += PulsarSalir;

                canvas += botonSalir;

                // Texto visual, sin evento tactil
                UI.Canvas.Text textoSalir =
                    new UI.Canvas.Text(
                        90, 240,
                        "SALIR",
                        "Univers",
                        20,
                        blanco);

                canvas += textoSalir;

                Console.WriteLine(
                    "Interfaz tactil preparada");

                Console.WriteLine(
                    "Esperando pulsaciones...");

                // Iniciar interfaz grafica
                canvas.Run();

                Console.WriteLine(
                    "Aplicacion finalizada");

                canvas.Dispose();

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR GENERAL:");
                Console.WriteLine(ex.ToString());

                if (canvas != null)
                {
                    canvas.Dispose();
                }

                return 1;
            }
        }

        // ===========================================
        // EVENTO: IMPRIMIR
        // ===========================================

        static void PulsarImprimir(
            object sender,
            UI.Canvas.MouseEventArgs e)
        {
            if (imprimiendo)
            {
                Console.WriteLine(
                    "Impresion en proceso. Espere...");
                return;
            }

            imprimiendo = true;

            Console.WriteLine(
                "IMPRIMIR pulsado: X=" +
                e.X + " Y=" + e.Y);

            try
            {
                estado.Data = "Imprimiendo...";
                canvas.Refresh();

                // Crear etiqueta nueva
                Drawing etiqueta = new Drawing();

                // Primera linea
                Drawing.Text titulo = new Drawing.Text();

                titulo.Point = new Point(40, 40);
                titulo.Data = "HONEYWELL PM45";
                titulo.Height = 28;

                etiqueta += titulo;

                // Segunda linea
                Drawing.Text subtitulo = new Drawing.Text();

                subtitulo.Point = new Point(40, 100);
                subtitulo.Data = "DEMO C# - V03";
                subtitulo.Height = 24;

                etiqueta += subtitulo;

                // Tercera linea
                Drawing.Text mensaje = new Drawing.Text();

                mensaje.Point = new Point(40, 160);
                mensaje.Data = "IMPRESION DE PRUEBA";
                mensaje.Height = 20;

                etiqueta += mensaje;

                etiqueta.PartialRendering = true;

                // Reutilizar el controlador existente
                State resultado = printer.PrintFeed(
                    etiqueta, 1);

                if (resultado == State.NoError)
                {
                    estado.Data = "Impresion enviada";

                    Console.WriteLine(
                        "Impresion enviada correctamente");
                }
                else
                {
                    estado.Data = "Error de impresion";

                    Console.WriteLine(
                        "Error PrintFeed: " + resultado);
                }
            }
            catch (Exception ex)
            {
                estado.Data = "Error de impresion";

                Console.WriteLine(
                    "ERROR AL IMPRIMIR:");

                Console.WriteLine(ex.ToString());
            }
            finally
            {
                canvas.Refresh();
                imprimiendo = false;
            }
        }

        // ===========================================
        // EVENTO: SALIR
        // ===========================================

        static void PulsarSalir(
            object sender,
            UI.Canvas.MouseEventArgs e)
        {
            Console.WriteLine(
                "SALIR pulsado: X=" +
                e.X + " Y=" + e.Y);

            canvas.Exit();
        }
    }
}
