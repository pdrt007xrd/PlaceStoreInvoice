using System.ComponentModel.DataAnnotations;

namespace Ventas.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "El usuario es obligatorio.")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "La clave es obligatoria.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
