namespace Prueba1_SourceAFIS.Interfaces
{
    public class IDevolverComparacion
    {

        public bool result { get; set; }
        public string cedula { get; set; }

        public IDevolverComparacion(bool result, string cedula)
        {
            this.result = result;
            this.cedula = cedula;
        }

    }
}

/*
    AUTHOR: OSCAR GILBERTO SERNA HERNÁNDEZ
    GITHUB: https://github.com/Oscar-Serna
*/