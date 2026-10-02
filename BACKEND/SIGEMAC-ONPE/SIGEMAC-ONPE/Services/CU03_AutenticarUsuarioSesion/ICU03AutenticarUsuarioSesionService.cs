namespace SIGEMAC_ONPE.Services.CU03_AutenticarUsuario
{
    public interface ICU03AutenticarUsuarioService
    {
        // Retorna un objeto con los datos del usuario si el login es exitoso, o null si falla.
        Task<object?> AutenticarAsync(string rol, string dni, string password);
    }
}