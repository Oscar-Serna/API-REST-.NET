using System;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using SourceAFIS;
using System.Data.SqlClient;
using SixLabors.ImageSharp;
using Prueba1_SourceAFIS.Models;
using Prueba1_SourceAFIS.Interfaces;

namespace Prueba1_SourceAFIS.Controllers
{
    public class ComparacionController
    {

        private static string cadenaConexion = "*Se obtiene la cadena de conexion*";

        [HttpPost]
        [Route("api/comparacion/obtenerComparacion")]
        public async Task<IDevolverComparacion> ObtenerComparacion([FromBody] IObtenerComparacion interfazObtenerComparacion)
        {
            string pathHuellaTemporal = $"{Directory.GetCurrentDirectory()}\\huella.png";

            string pathPlantillasCBOR = $"{Directory.GetCurrentDirectory()}\\PlantillasCBOR\\";

            string[] directoryPlantillasCBOR = Directory.GetFiles(pathPlantillasCBOR);

            string imagenBase64 = interfazObtenerComparacion.imagenBase64;

            string guardarHuellaTemporal = GuardarHuellaTemporal(imagenBase64);

            if (guardarHuellaTemporal == "ERROR") return new IDevolverComparacion(false, "ERROR");

            try
            {

                var templateProbe = new FingerprintTemplate(new FingerprintImage(File.ReadAllBytes(pathHuellaTemporal)));

                var listaCandidatos = await ObtenerPlantillas();

                var matcher = new FingerprintMatcher(templateProbe);

                string matchCedula = null;

                string cedulaArchivoCBOR = null;

                double max = Double.NegativeInfinity;

                foreach(var candidato in listaCandidatos)
                {

                    string cedula = candidato.Cedula;

                    foreach(var archivoCBOR in directoryPlantillasCBOR)
                    {

                        string archivoSinExtension = Path.GetFileNameWithoutExtension(archivoCBOR);

                        string textCedula = archivoSinExtension.Split('-')[0];

                        if(textCedula == cedula)
                        {

                            var serialized = File.ReadAllBytes(archivoCBOR);

                            var template = new FingerprintTemplate(serialized);

                            double similarity = matcher.Match(template);

                            if(similarity > max)
                            {
                                max = similarity;
                                matchCedula = cedula;
                                cedulaArchivoCBOR = archivoCBOR;
                            }

                        }
                    }
                }

                double threshold = 40;

                bool coincide = max >= threshold;

                Console.WriteLine($"COINCIDE: {coincide} CEDULA:{cedulaArchivoCBOR} CON PUNTAJE: {max}");


                return new IDevolverComparacion(coincide, matchCedula);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR AL OBTENER LA COMPARACIÓN: ");
                Console.WriteLine(e);
                return new IDevolverComparacion(false, "ERROR");

            }

        }

        public string GuardarHuellaTemporal(string imagenBase64)
        {

            string huellaTemporalPath = $"{Directory.GetCurrentDirectory()}\\huella.png";

            try
            {

                string correctedImagenBase64 = imagenBase64.PadRight((imagenBase64.Length + 3) & ~3, '=');

                byte[] imageBytes = Convert.FromBase64String(correctedImagenBase64);

                using var memoryStream = new MemoryStream(imageBytes);

                using var image = System.Drawing.Image.FromStream(memoryStream);

                image.Save(huellaTemporalPath, System.Drawing.Imaging.ImageFormat.Png);

                Console.WriteLine($"IMAGEN GUARDADA EN: {huellaTemporalPath}");

                return "GUARDADA";

            } catch (Exception e)
            {
                Console.WriteLine("ERROR AL GUARDAR LA HUELLA TEMPORAL");
                Console.WriteLine(e);
                return "ERROR";
            }
        }

        public async Task<List<IObtenerPlantilla>> ObtenerPlantillas()
        {

            var listaPlantillas = new List<IObtenerPlantilla>();

            using var conex = new SqlConnection(cadenaConexion);

            try
            {
                
                await conex.OpenAsync();

                using var command = new SqlCommand("SELECT cedula FROM HuellasTable", conex);


                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    listaPlantillas.Add(new IObtenerPlantilla(reader[0].ToString(), null));
                }

                if (listaPlantillas.Count == 0) listaPlantillas.Add(new IObtenerPlantilla("SIN PLANTILLAS", null));

                return listaPlantillas;

            }
            catch(Exception e)
            {
                Console.WriteLine("ERROR AL OBTENER LAS PLANTILLAS DE LA DB");
                Console.WriteLine(e);
                listaPlantillas.Add(new IObtenerPlantilla("ERROR", null));
                return listaPlantillas;
            }

        }

    }
}

/*
    AUTHOR: OSCAR GILBERTO SERNA HERNÁNDEZ
    GITHUB: https://github.com/Oscar-Serna
*/
