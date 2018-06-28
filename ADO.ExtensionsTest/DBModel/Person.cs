using ADO.Extensions.Reflection;

namespace ADO.ExtensionsTest.DBModel
{
    [TableName("ENG_USUARIO")]
    public class Person
    {
        [ColumnName("COD_USUARIO"), IsPK(true)]
        public string CodUsuario { get; set; }

        [ColumnName("SENHA"), IsPK(true)]
        public string Senha { get; set; }
    }
}
