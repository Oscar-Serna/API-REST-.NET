using System;
using System.Drawing;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Prueba1_SourceAFIS.Models;
using System.Data.SqlClient;
using SixLabors.ImageSharp;
using Prueba1_SourceAFIS.Interfaces;
using SourceAFIS;


namespace Prueba1_SourceAFIS.Controllers
{
    public class EmpleadosController
    {

        private static string cadenaConexion = "Data Source=DESKTOP-8OFI3LC,1433;user id=code;password=123;Initial Catalog=HUELLASDB;Persist Security Info=true;Integrated Security=True";

        private string defaultKey = "un1d4d1nf0rm4t1c4c3cyt3";

        [HttpGet]
        [Route("api/empleados/obtenerEmpleados")]

        public async Task<List<MEmpleado>> ObtenerEmpleados([FromHeader] string AuthorizationKey)
        {

            var lista = new List<MEmpleado>();

            if(AuthorizationKey != defaultKey)
            {
                Console.WriteLine("ACCESO DENEGADO DESDE ObtenerEmpleados LLAVE INCORRECTA");
                lista.Add(new MEmpleado(null, "ACCESO DENEGADO", null, null, null, null, null, null));
                return lista;
            }

            try
            {
                using var conex = new SqlConnection(cadenaConexion);

                await conex.OpenAsync();

                using var command = new SqlCommand("SELECT * FROM HuellasTable;", conex);

                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    lista.Add(new MEmpleado(
                                reader[0].ToString(),
                                reader[1].ToString(),
                                reader[2].ToString(),
                                reader[3].ToString(),
                                reader[4].ToString(),
                                reader[5].ToString(),
                                reader[6].ToString(),
                                reader[7].ToString()
                        ));
                }


                return lista;
            }
            catch(SqlException e)
            {
                Console.WriteLine("ERROR AL OBTENER TODOS LOS EMPLEADOS");
                Console.WriteLine(e);
                return lista;
            }

        }

        [HttpGet]
        [Route("api/empleados/obtenerEmpleado")]
        public async Task<List<IObtenerEmpleado>> ObtenerEmpleado([FromHeader] string AuthorizationKey, [FromHeader] string cedula)
        {

            var lista = new List<IObtenerEmpleado>();

            if(AuthorizationKey != defaultKey)
            {
                Console.WriteLine("ACCESO DENEGADO DESDE ObtenerEmpleado LLAVE INCORRECTA");
                lista.Add(new IObtenerEmpleado("ACCESO DENEGADO", null, null, null, null));
                return lista;
            }

            using var conex = new SqlConnection(cadenaConexion);

            try
            {

                await conex.OpenAsync();

                using var command = new SqlCommand("SELECT nombre, cedula, rfc, turno FROM HuellasTable WHERE CAST(cedula AS NVARCHAR(MAX)) = @cedula", conex);

                command.Parameters.AddWithValue("@cedula", cedula);

                var reader = command.ExecuteReader();

                while (await reader.ReadAsync())
                {
                    lista.Add(new IObtenerEmpleado(
                            reader[0].ToString(), //nombre
                            reader[1].ToString(), //cedula
                            reader[2].ToString(), //rfc
                            reader[3].ToString(), //turno
                            ObtenerImagenEmpleado(cedula) //imagenBase64
                    ));
                }

                return lista;

            }catch(SqlException e)
            {
                Console.WriteLine("ERROR AL OBTENER LOS DATOS DEL USUARIO : ObtenerEmpleado");
                Console.WriteLine(e);
                return lista;
            }

        }

        [HttpPost]
        [Route("api/empleados/crearEmpleado")]
        public async Task<string> CrearEmpleado([FromHeader] string AuthorizationKey, [FromBody] ICrearEmpleado infoEmpleado)
        {

            if(AuthorizationKey != defaultKey)
            {
                Console.WriteLine("ACCESO DENEGADO DESDE CrearEmpleado LLAVE INCORRECTA");
                return "ACCESO DENEGADO";
            }

            using var conex = new SqlConnection(cadenaConexion);


            try
            {

                await conex.OpenAsync();

                string existencia = await ExisteUsuario(infoEmpleado.cedula, infoEmpleado.rfc);


                if (existencia == "EXISTE") return "YA EXISTE";
                if (existencia == "ERROR") return "ERROR";

                using var command = new SqlCommand("INSERT INTO HuellasTable ( nombre, cedula, rfc, puesto, turno, num_huellas ) VALUES ( @nombre, @cedula, @rfc, @puesto, @turno, @num_huellas ) ", conex);

                command.Parameters.AddWithValue("@nombre", infoEmpleado.nombre);
                command.Parameters.AddWithValue("@cedula", infoEmpleado.cedula);
                command.Parameters.AddWithValue("@rfc", infoEmpleado.rfc);
                command.Parameters.AddWithValue("@puesto", infoEmpleado.puesto);
                command.Parameters.AddWithValue("@turno", infoEmpleado.turno);
                command.Parameters.AddWithValue("@num_huellas", infoEmpleado.num_huellas);

                await command.ExecuteReaderAsync();

                if (!GuardarImagenesHuellas(infoEmpleado.cedula, infoEmpleado.huellasBase64)) return "ERROR";

                if (!(await GuardarPlantillasHuellas(infoEmpleado.cedula))) return "ERROR";

                return "CREADO";

            }
            catch (SqlException e)
            {
                Console.WriteLine("ERROR AL CREAR EL EMPLEADO : CrearEmpleado()");
                Console.WriteLine(e);
                return "ERROR";
            }



        }

        [HttpDelete]
        [Route("api/empleados/eliminarEmpleado")]
        public async Task<string> EliminarEmpleado([FromHeader] string AuthorizationKey, [FromHeader] string cedula)
        {
            
            if(AuthorizationKey != defaultKey)
            {
                Console.WriteLine("ACCESO DENEGADO DESDE EliminarEmpleado LLAVE INCORRECTA");
                return "ACCESO DENEGADO";
            }

            using var conex = new SqlConnection(cadenaConexion);

            try
            {

                await conex.OpenAsync();

                using var command = new SqlCommand("DELETE FROM HuellasTable WHERE CAST(cedula AS NVARCHAR(MAX)) = @cedula", conex);

                command.Parameters.AddWithValue("@cedula", cedula);

                command.ExecuteReader();

                string eliminarHuellas = EliminarImagenesHuellas(cedula);

                if ( eliminarHuellas == "NO HAY POR ELIMINAR" ) return "NO HAY POR ELIMINAR";
                if ( eliminarHuellas == "ERROR" ) return "ERROR";

                string eliminarArchivosCBOR = EliminarArchivosCBOR(cedula);

                if (eliminarArchivosCBOR == "ERROR") return "ERROR";

                return "ELIMINADO";

            }catch(SqlException e)
            {
                Console.WriteLine("ERROR AL ELIMINAR EL EMPLEADO : EliminarEmpleado()");
                Console.WriteLine(e);
                return "ERROR";
            }

        }

        public bool GuardarImagenesHuellas(string cedula, string[] huellasBase64)
        {
            var huellasDirectory = $"{Directory.GetCurrentDirectory()}\\Huellas\\";

            try
            {
                for (int i = 0; i < huellasBase64.Length; i++)
                {
                    string pathname = $"{huellasDirectory}{cedula}-{i}.png";

                    // Agrega caracteres de relleno '=' al final de la cadena base64 para que sea un multiplo de 4
                    string base64StringWithPadding = huellasBase64[i].PadRight((huellasBase64[i].Length + 3) & ~3, '=');

                    byte[] imageBytes = Convert.FromBase64String(base64StringWithPadding);

                    using var memoryStream = new MemoryStream(imageBytes);

                    using var image = System.Drawing.Image.FromStream(memoryStream);

                    image.Save(pathname, System.Drawing.Imaging.ImageFormat.Png);

                    Console.WriteLine("Imagen guardada en: " + huellasDirectory);
                }

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR AL GUARDAR LA IMAGEN DE HUELLA : GuardarImagenHuella()");
                Console.WriteLine(e);
                return false;
            }
        }

        public async Task<bool> GuardarPlantillasHuellas(string cedula)
        {

            string huellasPath = $"{Directory.GetCurrentDirectory()}\\Huellas\\";

            string plantillasCborPath = $"{Directory.GetCurrentDirectory()}\\PlantillasCBOR\\";

            string[] huellasDirectory = Directory.GetFiles(huellasPath);

            try
            {

                for(int i = 0; i < huellasDirectory.Length; i++)
                {

                    var huellaPath = huellasDirectory[i];

                    string huellaSinExtension = Path.GetFileNameWithoutExtension(huellaPath);

                    string textCedula = huellaSinExtension.Split('-')[0];

                    if (textCedula == cedula)
                    {

                        var template = new FingerprintTemplate(new FingerprintImage(File.ReadAllBytes(huellaPath)));

                        var serialized = template.ToByteArray();

                        string archivoCborPath = $"{plantillasCborPath}{cedula}-{i}.cbor";

                        File.WriteAllBytes(archivoCborPath, serialized);
                    }

                }

                return true;

            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR AL GUARDAR LAS PLANTILLAS EN LA DB: ");
                Console.WriteLine(e);
                return false;
            }


        }


        public async Task<string> ExisteUsuario(string cedula, string rfc)
        {

            using var conex = new SqlConnection(cadenaConexion);

            try
            {

                await conex.OpenAsync();

                using var command = new SqlCommand("SELECT * FROM HuellasTable WHERE CAST(cedula as NVARCHAR(MAX)) = @cedula OR CAST(rfc AS NVARCHAR(MAX)) = @rfc", conex);

                command.Parameters.AddWithValue("@cedula", cedula);
                command.Parameters.AddWithValue("@rfc", rfc);

                var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    return "EXISTE";
                }

                return "NO EXISTE";

            }catch(SqlException e)
            {
                Console.WriteLine("ERROR AL OBTENER EL USUARIO POSIBLEMENTE EXISTENTE");
                Console.WriteLine(e);
                return "ERROR";
            }

        }

        public string EliminarImagenesHuellas(string cedula)
        {
            //REMOVER IMAGENES CUANDO COINCIDA EL NOMBRE DE CEDULA ANTES DEL GUIÓN

            string huellasDirectory = $"{Directory.GetCurrentDirectory()}\\Huellas\\";

            try
            {

                string[] imagenesHuellas = Directory.GetFiles(huellasDirectory);

                int vecesEliminado = 0;

                foreach(var imagenHuella in imagenesHuellas)
                {

                    string imagen = Path.GetFileNameWithoutExtension(imagenHuella);

                    string textCedula = imagen.Split('-')[0];

                    if(textCedula == cedula)
                    {

                        File.Delete(imagenHuella);

                        vecesEliminado++;

                        Console.WriteLine($"SE ELIMINO LA IMÁGEN DE HUELLA {imagenHuella}");
                    }

                }

                if (vecesEliminado > 0) return "ELIMINADAS";
                else return "NO HAY POR ELIMINAR";

            }catch(Exception e)
            {
                Console.WriteLine("ERROR AL OBTENER EL DIRECTORIO DE HUELLAS");
                Console.WriteLine(e);
                return "ERROR";
            }
        }

        public string EliminarArchivosCBOR(string cedula)
        {

            string pathArchivosCBOR = $"{Directory.GetCurrentDirectory()}\\PlantillasCBOR";

            string[] directorioArchivosCBOR = Directory.GetFiles(pathArchivosCBOR);

            Console.WriteLine(directorioArchivosCBOR.Length);

            try
            {
                foreach (string archivoCBOR in directorioArchivosCBOR)
                {

                    string archivoSinExtension = Path.GetFileNameWithoutExtension(archivoCBOR);

                    string textCedula = archivoSinExtension.Split('-')[0];

                    Console.WriteLine($"Text cedula: {textCedula}");

                    if (textCedula == cedula)
                    {

                        File.Delete(archivoCBOR);

                    }
                }

                return "ELIMINADO";
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR AL ELIMINAR ARCHIVOS CBOR");
                Console.WriteLine(e);
                return "ERROR";
            }

        }

        public string ObtenerImagenEmpleado(string cedula)
        {

            string empleadosImgDirectory = $"{Directory.GetCurrentDirectory()}\\ImagenesEmpleados\\";

            string[] imagenesEmpleados = Directory.GetFiles(empleadosImgDirectory);

            foreach(string imagenEmpleado in imagenesEmpleados)
            {

                string textCedula = Path.GetFileNameWithoutExtension(imagenEmpleado);

                if(textCedula == cedula)
                {
                    try
                    {

                        byte[] imageBytes = File.ReadAllBytes(imagenEmpleado);

                        string imageBase64 = Convert.ToBase64String(imageBytes);

                        return imageBase64;

                    }catch(Exception e)
                    {

                        Console.WriteLine("ERROR AL OBTENER LA IMÁGEN DEL EMPLEADO");

                        Console.WriteLine(e);

                        return "ERROR";

                    }

                }
            }

            return "NO EXISTE";
        }
    
    }
}


/*
    AUTHOR: OSCAR GILBERTO SERNA HERNÁNDEZ
    GITHUB: https://github.com/Oscar-Serna
*/