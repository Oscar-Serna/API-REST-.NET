namespace Prueba1_SourceAFIS.Interfaces
{
    public class IObtenerPlantilla
    {

        public string Cedula { get; set; }
        public string Plantillas { get; set; }

        public IObtenerPlantilla(string Cedula, string Plantillas)
        {
            this.Cedula = Cedula;
            this.Plantillas = Plantillas;
        }

    }
}

/*
    AUTHOR: OSCAR GILBERTO SERNA HERNÁNDEZ
    GITHUB: https://github.com/Oscar-Serna
*/