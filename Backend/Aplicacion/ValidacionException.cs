namespace Api.Aplicacion;

public class ValidacionException : Exception
{
    public Dictionary<string, string[]> Errores { get; }

    public ValidacionException(Dictionary<string, string[]> errores)
        : base("Error de validación")
    {
        Errores = errores;
    }
}