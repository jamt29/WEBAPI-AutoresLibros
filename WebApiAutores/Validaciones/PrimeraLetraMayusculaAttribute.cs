using System.ComponentModel.DataAnnotations;

namespace WebApiAutores.Validaciones
{
    public class PrimeraLetraMayusculaAttribute: ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
                return ValidationResult.Success;

            // Obtiene la primera letra del valor y la convierte en mayúscula
            var primeraLetra = value.ToString()[0].ToString();

            // Compara la primera letra con su versión en mayúscula
            if (primeraLetra == primeraLetra.ToUpper())
                return ValidationResult.Success;

            // Si la primera letra no es mayúscula, se devuelve un ValidationResult con un mensaje de error
            return new ValidationResult($"La primera letra de {value} debe ser mayúscula");
        }

    }
}
