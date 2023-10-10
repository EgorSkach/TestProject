using System.ComponentModel.DataAnnotations;

namespace TestProject.Models
{
    public class User
    {
        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Имя
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "Имя обязательно")]
        public string Name { get; set; }

        /// <summary>
        /// Возраст
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "Возраст обязателен")]
        public int Age { get; set; }

        /// <summary>
        /// Email
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "Email обязателен")]
        public string Email { get; set; }

        /// <summary>
        /// Роли пользователя
        /// </summary>
        public ICollection<Role> Roles { get; set; }
    }
}
