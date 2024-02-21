namespace Prueba1_SourceAFIS.Models
{
    public class MEmpleado
    {

        public string id { get; set; }
        public string nombre { get; set; }
        public string cedula { get; set; }
        public string rfc { get; set; }
        public string puesto { get; set; }
        public string turno { get; set; }
        public string numHuellas { get; set; }
        public string dataHuellas { get; set; }

        public MEmpleado(string id, string nombre, string cedula, string rfc, string puesto, string turno, string numHuellas, string dataHuellas)
        {

            this.id = id;

            this.nombre = nombre;

            this.cedula = cedula;

            this.rfc = rfc;

            this.puesto = puesto;

            this.turno = turno;

            this.numHuellas = numHuellas;

            this.dataHuellas = dataHuellas;

        }

    }
}

/*
    AUTHOR: OSCAR GILBERTO SERNA HERNÁNDEZ
    GITHUB: https://github.com/Oscar-Serna
*/