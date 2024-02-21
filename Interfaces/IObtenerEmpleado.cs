namespace Prueba1_SourceAFIS.Interfaces
{
    public class IObtenerEmpleado
    {
        public string? Nombre { get; set; }
        public string? Cedula { get; set; }
        public string? Rfc { get; set; }
        public string? Turno { get; set; }
        public string? ImagenBase64 { get; set; }

        public IObtenerEmpleado(string Nombre, string Cedula, string Rfc, string Turno, string ImagenBase64)
        {

            this.Nombre = Nombre;
            this.Cedula = Cedula;
            this.Rfc = Rfc;
            this.Turno = Turno;
            this.ImagenBase64 = ImagenBase64;

        }

    }
}

/*
    AUTHOR: OSCAR GILBERTO SERNA HERNÁNDEZ
    GITHUB: https://github.com/Oscar-Serna
*/