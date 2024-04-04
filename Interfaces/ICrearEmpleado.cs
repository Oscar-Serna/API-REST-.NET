namespace Prueba1_SourceAFIS.Interfaces
{
    public interface ICrearEmpleado
    {
        public string? nombre { get; set; }
        public string? cedula { get; set; }
        public string? rfc { get; set; }
        public string? puesto { get; set; }
        public string? turno { get; set; }
        public int num_huellas { get; set; }
        public string[]? huellasBase64 { get; set; }

    }
}

/*
    AUTHOR: OSCAR GILBERTO SERNA HERNÁNDEZ
    GITHUB: https://github.com/Oscar-Serna
*/
