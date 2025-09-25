﻿namespace WebApplication1.Models
{
    public class Usuarios
    {
        public int Id { get; set; } 
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
    }
}
